
Option Strict On
Option Explicit On

Imports System.Xml
Imports System.Reflection
Imports HttpMultipartParser
Imports Contensive.BaseClasses
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core
    Public Class coreClass
        Implements IDisposable
        '
        '======================================================================
        '
        ' core 
        '   -- carries objects For services that need initialization (non Static)
        '   -- (no output buffer) html 'buffer' should be an object created, head added, body added then released
        '       -- methods like cp.html.addhead add things to this html object
        '
        ' -- objects passed by constructor - do not dispose
        ' -- yes, cp is needed to pass int addon execution and script execution - but DO NOT call if from anything else
        ' -- no, cpCore should never call up to cp. cp is the api that calls core.
        Friend cp_forAddonExecutionOnly As CPClass                                   ' constructor -- top-level cp
        '
        ' -- shared globals
        '
        Public serverConfig As Models.Entity.serverConfigModel
        '
        ' -- application storage
        '
        Friend deleteOnDisposeFileList As New List(Of String)               ' tmp file list of files that need to be deleted during dispose
        Friend exceptionList As List(Of String)                             ' exceptions collected during document construction
        '
        ' -- state, authentication, authorization
        ' -- these are set id=0 at construction, then initialize if authentication used
        '
        Public authContext As authContextModel
        '
        ' -- Debugging
        '
        Private appStopWatch As Stopwatch = Stopwatch.StartNew()
        Public app_startTime As Date                                   ' set in constructor
        Public app_startTickCount As Integer = 0
        Public debug_allowDebugLog As Boolean = False                             ' turn on in script -- use to write /debug.log in content files for whatever is needed
        Public blockExceptionReporting As Boolean = False                   ' used so error reporting can not call itself
        Public app_errorCount As Integer = 0
        Public debug_iUserError As String = ""                              ' User Error String
        Public html_PageErrorWithoutCsv As Boolean = False  ' if true, the error occurred before Csv was available and main_TrapLogMessage needs to be saved and popedup
        Public main_TrapLogMessage As String = ""        ' The content of the current traplog (keep for popups if no Csv)
        Public main_ClosePageCounter As Integer = 0
        Public html_BlockClosePageLink As Boolean = False      ' if true,block the href to contensive
        Public main_testPointMessage As String = ""          '
        Public testPointPrinting As Boolean = False    ' if true, send main_TestPoint messages to the stream
        '
        Public docOpen As Boolean = False                                   ' when false, routines should not add to the output and immediately exit
        '
        Public Const cache_linkAlias_cacheName = "cache_linkAlias"
        Public cache_linkAlias As String(,)
        Public cache_linkAliasCnt As Integer = 0
        Public cache_linkAlias_NameIndex As keyPtrController
        Public cache_linkAlias_PageIdQSSIndex As keyPtrController
        '-
        '-----------------------------------------------------------------------
        '-----------------------------------------------------------------------
        '-----------------------------------------------------------------------
        '-----------------------------------------------------------------------
        '
        '========================================================================================================================
        '   Internal cache (for content used to run the system)
        '========================================================================================================================
        '
        ' ----- cache styleAddonRules
        '
        'Public cache_addonStyleRules As coreCacheKeyPtrClass
        'Public Class styleAddonRuleClass
        '    Public addonId As Integer
        '    Public styleId As Integer
        'End Class
        'Public Class cache_sharedStylesAddonRulesClass
        '    Public rule As List(Of styleAddonRuleClass)
        '    Public addonIdIndex As keyPtrIndexClass
        'End Class
        'Private cache_styleAddonRules As New cache_sharedStylesAddonRulesClass
        '
        ' ----- addonIncludeRules
        '
        <Serializable()>
        Public Class addonIncludeRulesClass
            Public item As String(,)
            Public itemCnt As Integer = 0
            Public addonIdIndex As keyPtrController
        End Class
        Public cache_addonIncludeRules As addonIncludeRulesClass
        '
        ' ----- libraryFiles
        '
        Public cache_libraryFiles As String(,)
        Public cache_libraryFilesCnt As Integer = 0
        Public cache_libraryFilesIdIndex As keyPtrController
        '
        ' ----- linkForwward
        '
        Public cache_linkForward As String = ""
        '
        '
        ' should have been userTypeEnum
        ' only for main_GetFormInputWysiwig - to be deprecated anyway
        Private Enum main_EditorUserScopeEnum
            ' should have been userTypeAdministrator, etc
            Administrator = 1
            ContentManager = 2
            PublicUser = 3
        End Enum
        '
        ' should have been contentTypeEnum
        Private Enum main_EditorContentScopeEnum
            ' should have been contentTypePage
            Page = 1
            pagetemplate = 2
            Email = 3
            EmailTemplate = 4
        End Enum
        '
        Private main_PleaseWaitStarted As Boolean = False
        ''
        '' file systems
        ''
        'Public Property serverFiles As coreFileSystemClass           ' files written directly to the local server
        ''Public Property appRootFiles As coreFileSystemClass         ' wwwRoot path for the app server, both local and scale modes
        'Public Property privateFiles As coreFileSystemClass         ' path not available to web interface, local: physcial storage location, scale mode mirror location
        'Public Property cdnFiles As coreFileSystemClass             ' file uploads etc. Local mode this should point to appRoot folder (or a virtual folder in appRoot). Scale mode it goes to an s3 mirror
        '
        ' SF Resize Algorithms
        '
        Public Enum csv_SfImageResizeAlgorithms
            Box = 0
            Triangle = 1
            Hermite = 2
            Bell = 3
            BSpline = 4
            Lanczos3 = 5
            Mitchell = 6
            Stretch = 7
        End Enum
        '
        '
        Private csv_DynamicMenuACSelect As String = ""
        Private csv_DynamicFormACSelect As String = ""
        ''
        ''------------------------------------------------------------------------
        ''   SQL Timeouts
        ''------------------------------------------------------------------------
        ''
        'Private csv_SQLTimeout As Integer
        'Private csv_SlowSQLThreshholdMSec As Integer
        ''
        ''------------------------------------------------------------------------
        '' ----- Table Schema caching to speed up update
        ''------------------------------------------------------------------------
        ''
        'Private Structure app.tableSchemaClass
        '    Dim TableName As String
        '    Dim Dirty As Boolean
        '    Dim ColumnCount As Integer
        '    Dim ColumnName() As String
        '    Dim IndexCount As Integer
        '    Dim IndexName() As String
        '    Dim IndexFieldName() As String
        'End Structure
        'Private app.tableSchemaCount As Integer
        'Private app.tableSchema() As app.tableSchemaClass
        '
        '------------------------------------------------------------------------
        ' ----- Debugging
        '------------------------------------------------------------------------
        '
        ' ##### this was removed, but I put it back because we -forgot- that csv_ClearBake was required, and we spent too
        '       much time figuring out why a change to a record did not clear the bake. An argument was added to
        '       app.csv_SaveCSRecord to block the csv_ClearBake. If someone needs to prevent the clearing, this needs to be set
        'Private Const app.csv_AllowAutocsv_ClearContentTimeStamp = True    ' Trial removal - put back

        '
        Public csv_ConnectionHandleLocal As Integer = 0              ' Local storage for connection handle established when appServices opened
        Public csv_ConnectionID As Integer = 0                     ' Random number (semi) unique to this hit
        '
        ' app.config.urlencoder is a string from the licence system used to encode cookies
        '
        'Private app.config.urlencoderLocal As String
        'Private app.siteProperty_ServerPageDefault As String
        '
        '================================================================================
        '   (NOT thread safe)
        '   a list of addons that have already been executed during this csv-lifetime.
        '   - used to prevent javascript and styles from being added to cpCoreClass twice
        '================================================================================
        '
        Friend addonsRunOnThisPageIdList As New List(Of Integer)
        'Private csv_addon.addon_execute_AddonsRunOnThisPageIdList As String = ""
        '
        '================================================================================
        '   (NOT thread safe)
        '   a list of addons currently running.
        '       - appended each time an addon is run
        '       - append removed when the addon exits
        '================================================================================
        '
        Friend addonsCurrentlyRunningIdList As New List(Of Integer)
        'Private csv_addon.addon_execute_AddonsCurrentlyRunningIdList As String = ""
        ''
        '' deprecated - here for compatibiity
        ''
        'Structure csv_DataSourceConnType
        '    Dim Conn As Connection
        '    Dim IsOpen As Boolean
        '    Dim Type As Integer
        'End Structure
        ''
        ''------------------------------------------------------------------------
        '' ----- app.csv_ContentSet Storage
        ''------------------------------------------------------------------------
        ''
        ''Const csv_DefaultPageSize = 9999
        '''
        'Private app.csv_ContentSet() As ContentSetType2
        'Public app.csv_ContentSetCount As Integer       ' The number of elements being used
        'Public app.csv_ContentSetSize As Integer        ' The number of current elements in the array
        'Const app.csv_ContentSetChunk = 50              ' How many are added at a time
        ''
        '' when true, all app.csOpen, etc, will be setup, but not return any data (app.csv_IsCSOK false)
        '' this is used to generate the app.csv_ContentSet.Source so we can run a app.csv_GetContentRows without first opening a recordset
        ''
        'Private csv_OpenCSWithoutRecords As Boolean
        '
        '------------------------------------------------------------------------
        ' ----- Build Version
        '------------------------------------------------------------------------
        '
        '  Private csv_Private_DataBuildVersion As String = ""          ' From ccSetup, set to ccLib version strin. If not matched, randomize BuildKey
        '
        '------------------------------------------------------------------------
        ' ----- Style Sheets
        '------------------------------------------------------------------------
        '
        Public Structure csv_stylesheetCacheType
            Dim templateId As Integer
            Dim EmailID As Integer
            Dim StyleSheet As String
        End Structure
        Public csv_stylesheetCache() As csv_stylesheetCacheType
        Public csv_stylesheetCacheCnt As Integer
        'Private Private_StyleSheet_Loaded As Boolean
        'Private Private_StyleSheet As String
        '
        'Private Private_StyleSheetProcessed_Loaded As Boolean
        'Private Private_StyleSheetProcessed As String
        '
        '------------------------------------------------------------------------
        ' ----- Database Upgrade Flag
        '       (mimicked as Public Property)
        '------------------------------------------------------------------------
        '
        'Private csv_UpgradeInProgressLocal As Boolean    ' When true, currently upgrading (concurancy issue)
        'Private new_loadCdefCache_loadContentEngineContentEngineInProcess As Boolean
        '
        '------------------------------------------------------------------------
        '   Sort Method temp storage
        '------------------------------------------------------------------------
        '
        'Private structure csv_SortMethodsType
        '    Id as integer
        '    Method As String
        'end  structure
        ''

        ''
        ''------------------------------------------------------------------------
        '' ----- Stream
        ''       This storage controls the csv_WriteStream method
        ''------------------------------------------------------------------------
        ''
        'Private csv_StreamListSize As Integer          ' size of csv_StreamList()
        'Private web_StreamListCount As Integer         ' valid entries in csv_StreamList
        'Private web_StreamList() As String        ' Set with csv_OpenStream/csv_CloseStream - if non-empty, writes stream appends here
        '
        ' ----- ContentField Type
        '       Stores information about fields in a content set
        '
        Private Structure ContentSetWriteCacheType
            Dim Name As String
            Dim Caption As String
            Dim ValueVariant As Object
            Dim fieldType As Integer
            Dim Changed As Boolean                  ' If true, the next app.csv_SaveCSRecord will save this field
        End Structure

        '
        ' ----- app.csv_ContentSet Type
        '       Stores pointers to open recordsets of content being used by the page
        '
        Private Structure ContentSetType2
            Dim IsOpen As Boolean                   ' If true, it is in use
            Dim LastUsed As Date                    ' The date/time this app.csv_ContentSet was last used
            Dim Updateable As Boolean               ' Can not update an app.csv_OpenCSSQL because Fields are not accessable
            Dim NewRecord As Boolean                ' true if it was created here
            'ContentPointer as integer              ' Pointer to the content for this Set
            Dim ContentName As String
            Dim CDef As cdefModel
            Dim OwnerMemberID As Integer               ' ID of the member who opened the app.csv_ContentSet
            '
            ' Workflow editing modes
            '
            Dim WorkflowAuthoringMode As Boolean    ' if true, these records came from the AuthoringTable, else ContentTable
            Dim WorkflowEditingRequested As Boolean ' if true, the CS was opened requesting WorkflowEditingMode
            Dim WorkflowEditingMode As Boolean      ' if true, the current record can be edited, else just rendered (effects EditBlank and app.csv_SaveCSRecord)
            '
            ' ----- Write Cache
            '
            Dim writeCacheChanged As Boolean          ' if true, writeCache contains changes
            Dim writeCache() As ContentSetWriteCacheType ' array of fields buffered for this set
            Dim writeCacheSize As Integer                ' the total number of fields in the row
            Dim writeCacheCount As Integer               ' the number of field() values to write
            Dim IsModified As Boolean               ' Set when CS is opened and if a save happens
            '
            ' ----- Recordset used to retrieve the results
            '
            'dim dt as datatable                        ' The Recordset
            'RSOpen As Boolean                   ' true if the recordset is open
            'EOF As Boolean                      ' if true, Row is empty and at end of records
            ' ##### new way 4/19/2004
            '   readCache stores only the current row
            '   RS holds all other rows
            '   app.csv_cs_getRow returns the readCache
            '   app.csv_NextCSRecord saves the difference between the readCache and the writeCache, and movesnext, inc ResultachePointer
            '   csv_LoadreadCache stores the current RS row to the readCache
            '
            '
            ' ##### old way
            ' Storage for the RecordSet results (future)
            '       Result - refers to the entire set of rows the the SQL (Source) returns
            '       readCache - the block of records currently stored in member (readCacheTop to readCacheTop+PageSize-1)
            '       readCache is initially loaded with PageSize records, starting on page PageNumber
            '       app.csv_NextCSRecord increments readCacheRowPtr
            '           If readCacheRowPtr > readCacheRowCnt-1 then csv_LoadreadCache
            '       EOF true if ( readCacheRowPtr > readCacheRowCnt-1 ) and ( readCacheRowCnt < PageSize )
            '
            Dim Source As String                    ' Holds the SQL that created the result set
            Dim DataSource As String                ' The Datasource of the SQL that created the result set
            Dim PageSize As Integer                    ' Number of records in a cache page
            Dim PageNumber As Integer                  ' The Page that this result starts with
            '
            ' ----- Read Cache
            '
            Dim fieldNames() As String       ' 1-D array of the result field names
            Dim ResultColumnCount As Integer           ' number of columns in the fieldNames and readCache
            'deprecated, but leave here for the test - useMultiRowCache
            Dim ResultEOF As Boolean                ' readCache is at the last record
            '
            ' ----- Read Cache
            '
            Dim readCache As String(,)            ' 2-D array of the result rows/columns
            Dim readCacheRowCnt As Integer         ' number of rows in the readCache
            Dim readCacheRowPtr As Integer         ' Pointer to the current result row, first row is 0, BOF is -1
            '
            ' converted array to dictionary - Dim FieldPointer As Integer                ' Used for GetFirstField, GetNextField, etc
            '
            Dim SelectTableFieldList As String      ' comma delimited list of all fields selected, in the form table.field
            'Rows as object                     ' getRows read during csv_InitContentSetResult
        End Structure
        ''
        '' $$$$$ remove this after testing - just for binary compatibility
        ''
        'public structure app.csv_ContentSetType
        '    IsOpen As Boolean                   ' If true, it is in use
        '    LastUsed As Date = New Date().MinValue                    ' The date/time this app.csv_ContentSet was last used
        '    Updateable As Boolean               ' Can not update an app.csv_OpenCSSQL because Fields are not accessable
        '    NewRecord As Boolean                ' true if it was created here
        '    'ContentPointer as integer              ' Pointer to the content for this Set
        '    ContentName As String
        '    CDef As CDefType
        '    OwnerMemberID as integer               ' ID of the member who opened the app.csv_ContentSet
        '    '
        '    ' Workflow editing modes
        '    '
        '    WorkflowAuthoringMode As Boolean    ' if true, these records came from the AuthoringTable, else ContentTable
        '    WorkflowEditingRequested As Boolean ' if true, the CS was opened requesting WorkflowEditingMode
        '    WorkflowEditingMode As Boolean      ' if true, the current record can be edited, else just rendered (effects EditBlank and app.csv_SaveCSRecord)
        '    '
        '    ' ----- Write Cache
        '    '
        '    RowCacheChanged As Boolean          ' if true, RowCache contains changes
        '    RowCache() As app.csv_ContentSetRowCacheType ' array of fields buffered for this set
        '    RowCacheSize as integer                ' the total number of fields in the row
        '    RowCacheCount as integer               ' the number of field() values to write
        '    IsModified As Boolean               ' Set when CS is opened and if a save happens
        '    '
        '    ' ----- Recordset used to retrieve the results
        '    '
        '    RS as datatable                        ' The Recordset
        '    'RSOpen As Boolean                   ' true if the recordset is open
        '    'EOF As Boolean                      ' if true, Row is empty and at end of records
        '    ' ##### new way 4/19/2004
        '    '   ResultCache stores only the current row
        '    '   RS holds all other rows
        '    '   app.csv_cs_getRow returns the ResultCache
        '    '   app.csv_NextCSRecord saves the difference between the ResultCache and the RowCache, and movesnext, inc ResultachePointer
        '    '   csv_LoadResultCache stores the current RS row to the ResultCache
        '    '
        '    '
        '    ' ##### old way
        '    ' Storage for the RecordSet results (future)
        '    '       Result - refers to the entire set of rows the the SQL (Source) returns
        '    '       ResultCache - the block of records currently stored in member (ResultCacheTop to ResultCacheTop+PageSize-1)
        '    '       ResultCache is initially loaded with PageSize records, starting on page PageNumber
        '    '       app.csv_NextCSRecord increments ResultCachePointer
        '    '           If ResultCachePointer > ResultCacheRowCount-1 then csv_LoadResultCache
        '    '       EOF true if ( ResultCachePointer > ResultCacheRowCount-1 ) and ( ResultCacheRowCount < PageSize )
        '    '
        '    Source As String                    ' Holds the SQL that created the result set
        '    DataSource As String                ' The Datasource of the SQL that created the result set
        '    PageSize as integer                    ' Number of records in a cache page
        '    PageNumber as integer                  ' The Page that this result starts with
        '    '
        '    ' ----- Read Cache
        '    '
        '    fieldNames() As String       ' 1-D array of the result field names
        '    ResultColumnCount as integer           ' number of columns in the fieldNames and ResultCacheValues
        '    ResultEOF As Boolean                ' Resultcache is at the last record
        '    ResultCacheValues() as object      ' 2-D array of the result rows/columns
        '    ResultCacheRowCount as integer         ' number of rows in the ResultCacheValues
        '    ResultCachePointer as integer          ' Pointer to the current result row, if 0, this is BOF
        '    '
        '    FieldPointer as integer                ' Used for GetFirstField, GetNextField, etc
        '    '
        '    SelectTableFieldList As String      ' comma delimited list of all fields selected, in the form table.field
        'end  structure
        '
        ' Cache for csv_CreateSQLTable, to keep one instance of CS from creating the same table many times
        '
        Private csv_CreateSQLTable_CreatedList As String = ""
        '
        ' Storage for csv_GetDefaultValue() and \cclib\Config\DefaultValues.txt file
        '
        Private DefaultValues As String = ""
        Private DefaultValueArray() As String
        Private DefaultValueArrayCnt As Integer
        '
        ' Attributes collected while composing content -
        '   These need to be added to the page or email after the content is complete.
        '   pages - after each encode content call, get these and add them into the page
        '   email - after the email is encoded, add these in
        '

        Private web_EncodeContent_JavascriptOnLoad_Cnt As Integer
        Private web_EncodeContent_JavascriptOnLoad() As String
        '
        Private web_EncodeContent_JSFilename_Cnt As Integer
        Private web_EncodeContent_JSFilename() As String
        '
        Friend web_EncodeContent_JavascriptBodyEnd_cnt As Integer
        Friend web_EncodeContent_JavascriptBodyEnd() As String
        '
        Friend web_EncodeContent_StyleFilenames_Cnt As Integer
        Friend web_EncodeContent_StyleFilenames() As String
        '
        Friend web_EncodeContent_HeadTags As String = ""
        '
        ' storage moved here from main - if addon move to csv is successful, this will stay
        '
        Public pageManager_PageAddonCnt As Integer = 0
        '
        '===================================================================================================
        ''' <summary>
        ''' addonCache object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property cache_addonStyleRules() As cacheKeyPtrController
            Get
                If (_cache_addonStyleRules Is Nothing) Then
                    _cache_addonStyleRules = New cacheKeyPtrController(Me, cacheNameAddonStyleRules, sqlAddonStyles, "shared style add-on rules,add-ons,shared styles")
                End If
                Return _cache_addonStyleRules
            End Get
        End Property
        Private _cache_addonStyleRules As cacheKeyPtrController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' addonCache object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property dataSources() As Dictionary(Of String, Models.Entity.dataSourceModel)
            Get
                If (_dataSources Is Nothing) Then
                    _dataSources = Models.Entity.dataSourceModel.getNameDict(Me)
                End If
                Return _dataSources
            End Get
        End Property
        Private _dataSources As Dictionary(Of String, Models.Entity.dataSourceModel) = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' email controller
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property email As emailController
            Get
                If (_email Is Nothing) Then
                    _email = New emailController(Me)
                End If
                Return _email
            End Get
        End Property
        Private _email As emailController
        '
        '===================================================================================================
        ''' <summary>
        ''' pageManager - too many page link tie-ins to make it a real addon. Code internal to handle models, then call it with an addon
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property pages As pagesController
            Get
                If (_pages Is Nothing) Then
                    _pages = New pagesController(Me)
                End If
                Return _pages
            End Get
        End Property
        Private _pages As pagesController
        '
        '===================================================================================================
        ''' <summary>
        ''' menuFlyout
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property menuTab As menuTabController
            Get
                If (_menuTab Is Nothing) Then
                    _menuTab = New menuTabController(Me)
                End If
                Return _menuTab
            End Get
        End Property
        Private _menuTab As menuTabController
        '
        '===================================================================================================
        ''' <summary>
        ''' menuFlyout
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property htmlDoc As Controllers.htmlDocController
            Get
                If (_htmlDoc Is Nothing) Then
                    _htmlDoc = New Controllers.htmlDocController(Me)
                End If
                Return _htmlDoc
            End Get
        End Property
        Private _htmlDoc As Controllers.htmlDocController
        '
        '===================================================================================================
        ''' <summary>
        ''' menuFlyout
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property addon As Controllers.addonController
            Get
                If (_addon Is Nothing) Then
                    _addon = New Controllers.addonController(Me)
                End If
                Return _addon
            End Get
        End Property
        Private _addon As Controllers.addonController
        '
        '===================================================================================================
        ''' <summary>
        ''' menuFlyout
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property menuFlyout As menuFlyoutController
            Get
                If (_menuFlyout Is Nothing) Then
                    _menuFlyout = New menuFlyoutController(Me)
                End If
                Return _menuFlyout
            End Get
        End Property
        Private _menuFlyout As menuFlyoutController
        '
        '===================================================================================================
        ''' <summary>
        ''' userProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property userProperty As propertyModelClass
            Get
                If (_userProperty Is Nothing) Then
                    _userProperty = New propertyModelClass(Me, PropertyTypeMember)
                End If
                Return _userProperty
            End Get
        End Property
        Private _userProperty As propertyModelClass
        '
        '===================================================================================================
        ''' <summary>
        ''' visitorProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property visitorProperty As propertyModelClass
            Get
                If (_visitorProperty Is Nothing) Then
                    _visitorProperty = New propertyModelClass(Me, PropertyTypeVisitor)
                End If
                Return _visitorProperty
            End Get
        End Property
        Private _visitorProperty As propertyModelClass
        '
        '===================================================================================================
        ''' <summary>
        ''' visitProperty
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property visitProperty As propertyModelClass
            Get
                If (_visitProperty Is Nothing) Then
                    _visitProperty = New propertyModelClass(Me, PropertyTypeVisit)
                End If
                Return _visitProperty
            End Get
        End Property
        Private _visitProperty As propertyModelClass
        '
        '===================================================================================================
        ''' <summary>
        ''' webServer
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property webServer As webServerController
            Get
                If (_webServer Is Nothing) Then
                    _webServer = New webServerController(Me)
                End If
                Return _webServer
            End Get
        End Property
        Private _webServer As webServerController
        '
        '===================================================================================================
        ''' <summary>
        ''' security object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property security() As securityController
            Get
                If (_security Is Nothing) Then
                    _security = New securityController(Me, serverConfig.appConfig.privateKey)
                End If
                Return _security
            End Get
        End Property
        Private _security As securityController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' docProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property docProperties() As docPropertyController
            Get
                If (_doc Is Nothing) Then
                    _doc = New docPropertyController(Me)
                End If
                Return _doc
            End Get
        End Property
        Private _doc As docPropertyController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' appRootFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property appRootFiles() As fileController
            Get
                If (_appRootFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _appRootFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _appRootFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.activeSync, fileController.normalizePath(serverConfig.appConfig.appRootFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _appRootFiles
            End Get
        End Property
        Private _appRootFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' serverFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property serverFiles() As fileController
            Get
                If (_serverFiles Is Nothing) Then
                    If serverConfig.isLocalFileSystem Then
                        '
                        ' local server -- everything is ephemeral
                        _serverFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, "")
                    Else
                        '
                        ' cluster mode - each filesystem is configured accordingly
                        _serverFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, "")
                    End If
                End If
                Return _serverFiles
            End Get
        End Property
        Private _serverFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' privateFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property privateFiles() As fileController
            Get
                If (_privateFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _privateFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _privateFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.privateFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _privateFiles
            End Get
        End Property
        Private _privateFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' privateFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property programDataFiles() As fileController
            Get
                If (_programDataFiles Is Nothing) Then
                    '
                    ' -- always local -- must be because this object is used to read serverConfig, before the object is valid
                    Dim programDataPath As String = fileController.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) & "Contensive\"
                    _programDataFiles = New fileController(Me, True, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(programDataPath))
                End If
                Return _programDataFiles
            End Get
        End Property
        Private _programDataFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' privateFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property programFiles() As fileController
            Get
                If (_programFiles Is Nothing) Then
                    '
                    ' -- always local
                    _programFiles = New fileController(Me, True, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.programFilesPath))
                End If
                Return _programFiles
            End Get
        End Property
        Private _programFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' cdnFiles object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property cdnFiles() As fileController
            Get
                If (_cdnFiles Is Nothing) Then
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If (serverConfig.appConfig.enabled) Then
                            If serverConfig.isLocalFileSystem Then
                                '
                                ' local server -- everything is ephemeral
                                _cdnFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.noSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath))
                            Else
                                '
                                ' cluster mode - each filesystem is configured accordingly
                                _cdnFiles = New fileController(Me, serverConfig.isLocalFileSystem, fileController.fileSyncModeEnum.passiveSync, fileController.normalizePath(serverConfig.appConfig.cdnFilesPath))
                            End If
                        End If
                    End If
                End If
                Return _cdnFiles
            End Get
        End Property
        Private _cdnFiles As fileController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' addonCache object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property addonCache() As Models.Entity.addonLegacyModel
            Get
                If (_addonCache Is Nothing) Then
                    _addonCache = New Models.Entity.addonLegacyModel(Me)
                End If
                Return _addonCache
            End Get
        End Property
        Private _addonCache As Models.Entity.addonLegacyModel = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property domains() As Models.Entity.domainLegacyModel
            Get
                If (_domains Is Nothing) Then
                    _domains = New Models.Entity.domainLegacyModel(Me)
                End If
                Return _domains
            End Get
        End Property
        Private _domains As Models.Entity.domainLegacyModel = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' JSON serialize/deserialize client
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property json() As System.Web.Script.Serialization.JavaScriptSerializer
            Get
                If (_json Is Nothing) Then
                    _json = New System.Web.Script.Serialization.JavaScriptSerializer
                End If
                Return _json
            End Get
        End Property
        Private _json As System.Web.Script.Serialization.JavaScriptSerializer
        '
        '===================================================================================================
        ''' <summary>
        ''' siteProperties object
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property siteProperties() As Models.Context.siteContextModel
            Get
                If (_siteProperties Is Nothing) Then
                    _siteProperties = New Models.Context.siteContextModel(Me)
                End If
                Return _siteProperties
            End Get
        End Property
        Private _siteProperties As Models.Context.siteContextModel = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' returns the cache object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property workflow() As workflowController
            Get
                If (_workflow Is Nothing) Then
                    _workflow = New workflowController(Me)
                End If
                Return _workflow
            End Get
        End Property
        Private _workflow As workflowController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' returns the cache object.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property cache() As Controllers.cacheController
            Get
                If (_cache Is Nothing) Then
                    _cache = New Controllers.cacheController(Me)
                End If
                Return _cache
            End Get
        End Property
        Private _cache As Controllers.cacheController = Nothing
        '
        Public ReadOnly Property metaData As cdefController
            Get
                If _metaData Is Nothing Then
                    _metaData = New cdefController(Me)
                End If
                Return _metaData
            End Get
        End Property
        Private _metaData As cdefController = Nothing
        '
        '===================================================================================================
        ''' <summary>
        ''' a lazy constructed instance of the application db controller -- use to access the application's database
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>_app created duirng init(), after cp.context() is loaded</remarks>
        Public ReadOnly Property db As dbController
            Get
                If (_db Is Nothing) Then
                    _db = New dbController(Me)
                End If
                Return _db
            End Get
        End Property
        Private _db As dbController
        '
        '===================================================================================================
        ''' <summary>
        ''' a lazy constructed instance of the db server controller -- used by the application dbs, and to create new catalogs
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks>_app created duirng init(), after cp.context() is loaded</remarks>
        Public ReadOnly Property dbServer As dbEngineController
            Get
                If (_dbEngine Is Nothing) Then
                    _dbEngine = New dbEngineController(Me)
                End If
                Return _dbEngine
            End Get
        End Property
        Private _dbEngine As dbEngineController
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for cluster use.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass)
            MyBase.New()
            cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me)
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            webServer.iisContext = Nothing
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, serverConfig As Models.Entity.serverConfigModel)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady
            webServer.iisContext = Nothing
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, serverConfig As Models.Entity.serverConfigModel, httpContext As System.Web.HttpContext)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            Me.serverConfig = serverConfig
            Me.serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            Me.serverConfig.appConfig.appStatus = Models.Entity.serverConfigModel.applicationStatusEnum.ApplicationStatusReady
            webServer.initWebContext(httpContext)
            constructorInitialize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for app, non-Internet use. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cp As CPClass, applicationName As String)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            If (serverConfig.appConfig IsNot Nothing) Then
                webServer.iisContext = Nothing
                constructorInitialize()
            End If
        End Sub
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor for a web request/response environment. cpCoreClass is the primary object internally, created by cp.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks>
        ''' All iis httpContext is loaded here and the context should not be used after this method.
        ''' </remarks>
        Public Sub New(cp As CPClass, applicationName As String, httpContext As System.Web.HttpContext)
            MyBase.New()
            Me.cp_forAddonExecutionOnly = cp
            '
            ' -- create default auth objects for non-user methods, or until auth is available
            authContext = New authContextModel
            '
            serverConfig = Models.Entity.serverConfigModel.getObject(Me, applicationName)
            serverConfig.defaultDataSourceType = Models.Entity.dataSourceModel.dataSourceTypeEnum.sqlServerNative
            If (serverConfig.appConfig IsNot Nothing) Then
                Call webServer.initWebContext(httpContext)
                constructorInitialize()
            End If
        End Sub
        ''
        '' ----- Get a DataSource ID from its Name
        ''       If it is not found, -1 is returned (for system datasource)
        ''
        'Public Function csv_GetDataSourceID(ByVal DataSourceName As String) As Integer
        '    Return app.csv_GetDataSourceID(DataSourceName)
        'End Function
        ''
        '' ----- Get a DataSource type (SQL Server, etc) from its Name
        ''
        'Public Function app.csv_GetDataSourceType(ByVal DataSourceName As String) As Integer
        '    Return app.csv_GetDataSourceType(DataSourceName)
        'End Function
        '    On Error GoTo ErrorTrap: 'Const Tn = "MethodName-026": 'Dim th as integer: th = profileLogMethodEnter(Tn)
        '    '
        '    Dim DataSourcePointer as integer
        '    '
        '            DataSourcePointer = app.csv_GetDataSourcePointer(DataSourceName)
        '    If app.dataSources.length > 0 Then
        '                If Not app.DataSourceConnectionObjs(DataSourcePointer).IsOpen Then
        '                    Call app.csv_OpenDataSource(DataSourceName, 30)
        '                End If
        '                csv_GetDataSourceType = app.DataSourceConnectionObjs(DataSourcePointer).Type
        '    End If
        '    '
        '    Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_GetDataSourceType", True)
        'End Function
        ''
        '' ----- Get a DataSource default cursor location
        ''
        'Private Function csv_GetDataSourceDefaultCursorLocation(DataSourceName As String) as integer
        '    On Error GoTo ErrorTrap: 'Const Tn = "GetDataSourceDefaultCursorLocation": 'Dim th as integer: th = profileLogMethodEnter(Tn)
        '    '
        '    Dim DataSourcePointer as integer
        '    '
        '            DataSourcePointer = app.csv_GetDataSourcePointer(DataSourceName)
        '    If app.dataSources.length > 0 Then
        '                If Not app.DataSourceConnectionObjs(DataSourcePointer).IsOpen Then
        '                    Call app.csv_OpenDataSource(DataSourceName, 30)
        '                End If
        '                csv_GetDataSourceDefaultCursorLocation = app.DataSourceConnectionObjs(DataSourcePointer).DefaultCursorLocation
        '    End If
        '    '
        '    Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_GetDataSourceDefaultCursorLocation", True)
        'End Function

        '
        '========================================================================
        ' Get a tables first ContentID from Tablename
        '========================================================================
        '
        Public Function GetContentIDByTablename(TableName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-028" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim SQL As String
            Dim CS As Integer
            '
            MethodName = "csv_GetContentIDByTablename"
            '
            GetContentIDByTablename = -1
            If TableName <> "" Then
                SQL = "select ContentControlID from " & TableName & " where contentcontrolid is not null order by contentcontrolid;"
                CS = db.cs_openCsSql_rev("Default", SQL, 1, 1)
                If db.cs_ok(CS) Then
                    GetContentIDByTablename = db.cs_getInteger(CS, "ContentControlID")
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            handleExceptionAndContinue(New ApplicationException("Unexpected exception")) : Throw New ApplicationException("Unexpected exception")
        End Function
        '
        '========================================================================
        ' app.csv_DeleteTableRecord
        '========================================================================
        '
        Public Sub DeleteTableRecordChunks(ByVal DataSourceName As String, ByVal TableName As String, ByVal Criteria As String, Optional ByVal ChunkSize As Integer = 1000, Optional ByVal MaxChunkCount As Integer = 1000)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-029" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim PreviousCount As Integer
            Dim CurrentCount As Integer
            Dim LoopCount As Integer
            Dim SQL As String
            Dim iChunkSize As Integer
            Dim iChunkCount As Integer
            'dim dt as datatable
            Dim DataSourceType As Integer
            '
            DataSourceType = db.getDataSourceType(DataSourceName)
            If (DataSourceType <> DataSourceTypeODBCSQLServer) And (DataSourceType <> DataSourceTypeODBCAccess) Then
                '
                ' If not SQL server, just delete them
                '
                Call db.DeleteTableRecords(TableName, Criteria, DataSourceName)
            Else
                '
                ' ----- Clear up to date for the properties
                '
                iChunkSize = ChunkSize
                If iChunkSize = 0 Then
                    iChunkSize = 1000
                End If
                iChunkCount = MaxChunkCount
                If iChunkCount = 0 Then
                    iChunkCount = 1000
                End If
                '
                ' Get an initial count and allow for timeout
                '
                PreviousCount = -1
                LoopCount = 0
                CurrentCount = 0
                SQL = "select count(*) as RecordCount from " & TableName & " where " & Criteria
                Dim dt As DataTable
                dt = db.executeSql(SQL)
                If dt.Rows.Count > 0 Then
                    CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
                Do While (CurrentCount <> 0) And (PreviousCount <> CurrentCount) And (LoopCount < iChunkCount)
                    If db.getDataSourceType(DataSourceName) = DataSourceTypeODBCMySQL Then
                        SQL = "delete from " & TableName & " where id in (select ID from " & TableName & " where " & Criteria & " limit " & iChunkSize & ")"
                    Else
                        SQL = "delete from " & TableName & " where id in (select top " & iChunkSize & " ID from " & TableName & " where " & Criteria & ")"
                    End If
                    Call db.executeSql(SQL, DataSourceName)
                    PreviousCount = CurrentCount
                    SQL = "select count(*) as RecordCount from " & TableName & " where " & Criteria
                    dt = db.executeSql(SQL)
                    If dt.Rows.Count > 0 Then
                        CurrentCount = genericController.EncodeInteger(dt.Rows(0).Item(0))
                    End If
                    LoopCount = LoopCount + 1
                Loop
                If (CurrentCount <> 0) And (PreviousCount = CurrentCount) Then
                    '
                    ' records did not delete
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Error deleting record chunks. No records were deleted and the process was not complete.")
                ElseIf (LoopCount >= iChunkCount) Then
                    '
                    ' records did not delete
                    '
                    Call Err.Raise(ignoreInteger, "dll", "Error deleting record chunks. The maximum chunk count was exceeded while deleting records.")
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "app.csv_DeleteTableRecordChunks", True)
        End Sub
        '
        '========================================================================
        ' Get a string that can be used in the where criteria of a SQL statement
        ' opening the content pointed to by the content pointer. This criteria
        ' will include both the content, and its child contents.
        '========================================================================
        '
        Public Function content_getContentControlCriteria(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-032" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim CDef As cdefModel
            '
            MethodName = "csv_GetContentControlCriteria"
            '
            CDef = metaData.getCdef(ContentName)
            content_getContentControlCriteria = CDef.ContentControlCriteria
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            handleExceptionAndContinue(New ApplicationException("Unexpected exception")) : Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function
        '
        '=============================================================================
        '   Update the content fields for all definitions that match this table
        '=============================================================================
        '
        Public Sub content_CreateContentFieldsFromSQLTable(ByVal DataSourceName As String, ByVal TableName As String)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-036" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            'Dim RSTargetTable as datatable
            ''Dim RSContent as datatable
            'Dim RSContentField as datatable
            '
            Dim SQL As String
            Dim DateAdded As Date = Date.MinValue
            Dim CreateKey As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim ContentID As Integer
            Dim BlankRecordID As Integer
            Dim MethodName As String
            Dim ContentName As String
            Dim TableFieldName As String
            ' Dim StateOfAllowContentAutoLoad As Boolean
            '
            MethodName = "csv_CreateContentFieldsFromSQLTable"
            '
            'StateOfAllowContentAutoLoad = app.AllowContentAutoLoad
            'app.AllowContentAutoLoad = False
            '
            ' ----- Get the content definition (must already be created)
            '
            SQL = "SELECT ccContent.Name AS ContentName, ccTables.Name AS TableName, ccContent.ID AS ContentID" _
                & " FROM ccContent LEFT JOIN ccTables ON ccContent.ContentTableID = ccTables.ID" _
                & " WHERE ccTables.Name=" & db.encodeSQLText(TableName) & ";"
            Const dtColumnContentName As Integer = 0
            Const dtColumnTableName As Integer = 1
            Const dtColumnContentId As Integer = 2
            '
            Dim dt As DataTable
            dt = db.executeSql(SQL)
            If dt.Rows.Count = 0 Then
                Throw (New ApplicationException("No Content Definition could be found for records in table [" & TableName & "]")) ' handleLegacyError25(MethodName, (""))
            Else
                '
                '----------------------------------------------------------------
                ' Read in a record from the table to get fields
                '----------------------------------------------------------------
                '
                SQL = db.GetSQLSelect("default", TableName, , , , , 1)
                'If csv_GetDataSourceType(DataSourceName) = DataSourceTypeODBCMySQL Then
                '    SQL = "select * from " & TableName & " limit 1"
                'Else
                '    SQL = "select top 1 * from " & TableName & ";"
                'End If
                Dim dtTargetTable As DataTable = db.executeSql(SQL, DataSourceName)
                If dtTargetTable.Rows.Count = 0 Then
                    '
                    ' --- no records were found, add a blank if we can
                    '
                    DateAdded = DateTime.Now()
                    CreateKey = genericController.getRandomLong()
                    SQL = "INSERT INTO " & TableName & " (CreateKey,DateAdded" _
                        & ")VALUES(" _
                        & " " & db.encodeSQLNumber(CreateKey) _
                        & "," & db.encodeSQLDate(DateAdded) _
                        & ");"
                    Call db.executeSql(SQL, DataSourceName)
                    SQL = db.GetSQLSelect("default", TableName, "ID", "DateAdded=" & db.encodeSQLDate(DateAdded) & " AND CreateKey=" & db.encodeSQLNumber(CreateKey))
                    dtTargetTable = db.executeSql(SQL, DataSourceName)
                    If dtTargetTable.Rows.Count = 0 Then
                        Throw (New ApplicationException("Could not locate a new re   cord added to table [" & TableName & "]")) ' handleLegacyError25(MethodName, (""))
                    Else
                        BlankRecordID = genericController.EncodeInteger(dtTargetTable.Rows(0).Item("id"))
                    End If
                    SQL = db.GetSQLSelect("default", TableName, , , , , 1)
                    dtTargetTable = db.executeSql(SQL, DataSourceName)
                    If dtTargetTable.Rows.Count = 0 Then
                        Throw (New ApplicationException("Could not open a record to table [" & TableName & "].")) ' handleLegacyError25(MethodName, (""))
                    End If
                Else
                    '
                    '-----------------------------------------------------------
                    ' --- Create the ccFields record for each content
                    '-----------------------------------------------------------
                    '
                    For Each dr As DataRow In dtTargetTable.Rows
                        'dr.Columns("")
                        ContentName = genericController.encodeText(dt(dtColumnContentName))
                        ContentID = genericController.EncodeInteger(dt(dtColumnContentId))
                        For Each dc As DataColumn In dtTargetTable.Rows
                            TableFieldName = dc.ColumnName
                            SQL = "SELECT * FROM ccFields where (ContentID=" & ContentID & ")and(name=" & db.encodeSQLText(TableFieldName) & ")"
                            Dim dtField As DataTable = db.executeSql(SQL, "Default")
                            If dtField.Rows.Count = 0 Then
                                Call db.createContentFieldFromTableField(ContentName, dc.ColumnName, genericController.EncodeInteger(dc.DataType))
                                'Call CreateContentFieldFromTableField(ContentName, dc.ColumnName, genericController.EncodeInteger(dc.DataType))
                            End If
                        Next
                    Next
                End If
            End If
            '
            ' ----- Load CDef
            '
            If (Not upgradeInProgress) Then
                cache.invalidateAll()
                metaData.clear()
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Sub

        '
        '========================================================================
        '   Get Content Definitions in XML format
        '========================================================================
        '
        Public Function GetXMLContentDefinition3(Optional ByVal ContentName As String = "", Optional ByVal IncludeBaseFields As Boolean = False) As String
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-046" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim MethodName As String
            Dim XML As xmlController
            '
            MethodName = "csv_GetXMLContentDefinition3"
            '
            XML = New xmlController(Me)
            GetXMLContentDefinition3 = XML.GetXMLContentDefinition3(ContentName, IncludeBaseFields)
            XML = Nothing
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            XML = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        End Function


        '
        '========================================================================
        ' ----- Get FieldDescritor from FieldType
        '========================================================================
        '
        Public Function GetSQLAlterColumnType(ByVal DataSourceName As String, ByVal fieldType As Integer) As String
            Return db.getSQLAlterColumnType(DataSourceName, fieldType)
        End Function
        '        '
        '========================================================================
        ' ----- Get FieldType from ADO Field Type
        '========================================================================
        '
        Private Function GetFieldTypeByADOType(ByVal ADOFieldType As Integer) As Integer
            Return db.getFieldTypeIdByADOType(ADOFieldType)
        End Function
        '
        '========================================================================
        ' Check for a table in a datasource
        '   if the table is missing, create the table and the core fields
        '       if NoAutoIncrement is false or missing, the ID field is created as an auto incremenet
        '       if NoAutoIncrement is true, ID is created an an long
        '   if the table is present, check all core fields
        '========================================================================
        '
        Public Sub CreateSQLTable(ByVal DataSourceName As String, ByVal TableName As String, Optional ByVal AllowAutoIncrement As Boolean = True)
            Call db.createSQLTable(DataSourceName, TableName, AllowAutoIncrement)
        End Sub

        '
        '========================================================================
        '   Delete a table field from a table
        '======================================================================
        Public Sub DeleteTable(ByVal DataSourceName As String, ByVal TableName As String)
            db.deleteTable(DataSourceName, TableName)
        End Sub
        '
        '========================================================================
        '   Delete a table field from a table
        '========================================================================
        '
        Public Sub DeleteTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String)
            Call db.deleteTableField(DataSourceName, TableName, FieldName)
        End Sub
        '
        '========================================================================
        ' Create an index on a table
        '
        '   Fieldnames is  a comma delimited list of fields
        '========================================================================
        '
        Public Sub CreateSQLIndex(ByVal DataSourceName As String, ByVal TableName As String, ByVal IndexName As String, ByVal FieldNames As String)
            Call db.createSQLIndex(DataSourceName, TableName, IndexName, FieldNames)
        End Sub
        '
        '======================================================================================
        '   Mimicks a local
        '======================================================================================
        '
        Public Property upgradeInProgress() As Boolean
            Get
                Return _upgradeInProgress
            End Get
            Set(ByVal value As Boolean)
                _upgradeInProgress = value
            End Set
        End Property
        Private _upgradeInProgress As Boolean             ' Block content autoload when upgrading
        '
        '==================================================================================================
        ' ----- Remove this record from all watch lists
        '       Mark permanent if the content is being deleted. non-permanent otherwise
        '==================================================================================================
        '
        Public Sub csv_DeleteContentTracking(ByVal ContentName As String, ByVal RecordID As Integer, ByVal Permanent As Boolean)
            On Error GoTo ErrorTrap 'Const Tn = "MethodName-098" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim ContentID As Integer
            '
            ' ----- remove all ContentWatchListRules (uncheck the watch lists in admin)
            '
            ContentID = metaData.getContentId(ContentName)
            Call db.deleteContentRules(ContentID, RecordID)
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_DeleteContentTracking", True)
        End Sub
        '        '
        '        '========================================================================
        '        '   Returns an application link correctly filtered for the Siteproperty Domain
        '        '========================================================================
        '        '
        '        Public Function filterDomainName(ByVal Link As String) As String
        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-102" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim EndPosition As Integer
        '            Dim linkprotocol As String = ""
        '            Dim LinkHost As String = ""
        '            Dim LinkPath As String = ""
        '            Dim LinkPage As String = ""
        '            Dim LinkQueryString As String = ""
        '            Dim MethodName As String = "csv_FilterDomainName"
        '            Dim csv_DomainName As String
        '            '
        '            'MethodName = "csv_FilterDomainName"
        '            '
        '            filterDomainName = Link
        '            csv_DomainName = serverconfig.appConfig.domainList(0)
        '            If genericController.vbInstr(1, csv_DomainName, ",") <> 0 Then
        '                csv_DomainName = Mid(csv_DomainName, 1, genericController.vbInstr(1, csv_DomainName, ",") - 1)
        '            End If
        '            '
        '            ' ----- set the Links Host to the Site Property Domain for consistancy with Spider
        '            '
        '            Call SeparateURL(Link, linkprotocol, LinkHost, LinkPath, LinkPage, LinkQueryString)
        '            '
        '            filterDomainName = linkprotocol & csv_DomainName & LinkPath & LinkPage & LinkQueryString
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Function
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub web_OpenStream(ByVal Filename As String)
        '            On Error GoTo ErrorTrap 'Const Tn = "csv_OpenStream" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "csv_OpenStream"
        '            '
        '            If web_StreamListCount >= csv_StreamListSize Then
        '                csv_StreamListSize = csv_StreamListSize + 1
        '                ReDim Preserve web_StreamList(csv_StreamListSize)
        '            End If
        '            web_StreamList(web_StreamListCount) = Filename
        '            web_StreamListCount = web_StreamListCount + 1
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub web_CloseStream()
        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-130" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "csv_CloseStream"
        '            '
        '            If web_StreamListCount > 0 Then
        '                web_StreamListCount = web_StreamListCount - 1
        '            End If
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '        '
        '        '========================================================================
        '        '
        '        '========================================================================
        '        '
        '        Public Sub log_TestPoint2(ByVal Message As String)
        '            On Error GoTo ErrorTrap 'Const Tn = "MethodName-132" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim ElapsedTime As Double
        '            Dim MethodName As String
        '            'Dim iMessage As String
        '            '
        '            MethodName = "csv_TestPoint"
        '            '
        '            '
        '            ' ----- If not Pagecsv_TestPointLogging, exit right away
        '            '
        '            If web_StreamListCount > 0 Then
        '                ElapsedTime = (GetTickCount) / 1000
        '                Message = Format((ElapsedTime), "0000000.000") & " - " & Message
        '            End If
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, MethodName, True)
        '        End Sub
        '
        ' Get the applications root path (ServerAppcsv_RootPath to WebClient)
        '
        Public ReadOnly Property app_rootWebPath() As String
            Get
                app_rootWebPath = requestAppRootPath
            End Get
        End Property
        '
        '   Get the Initialized Domain Name
        '
        Public ReadOnly Property app_domainList() As String
            Get
                app_domainList = serverConfig.appConfig.domainList(0)
            End Get


        End Property
        '
        '========================================================================
        '   Returns true if the field exists in the table
        '========================================================================
        '
        Public Function IsSQLTableField(ByVal DataSourceName As String, ByVal TableName As String, ByVal FieldName As String) As Boolean
            Return db.isSQLTableField(DataSourceName, TableName, FieldName)
        End Function
        '
        '========================================================================
        '   Returns true if the table exists
        '========================================================================
        '
        Public Function IsSQLTable(ByVal DataSourceName As String, ByVal TableName As String) As Boolean
            Return db.isSQLTable(DataSourceName, TableName)
        End Function


        ''=================================================================================
        '' Returns a pointer into the app.tableSchema() array for the table that matches
        ''=================================================================================
        ''
        'Public Function GetConnectionString(ByVal DataSourceName As String) As String
        '    GetConnectionString = ""
        '    Try
        '        Dim Pointer As Integer
        '        '
        '        If Not (_db Is Nothing) Then
        '            If genericController.vbUCase(DataSourceName) = "DEFAULT" Then
        '                GetConnectionString = db.DefaultConnectionString
        '            Else
        '                Pointer = db.GetDataSourcePointer(DataSourceName)
        '                If Pointer >= 0 Then
        '                    GetConnectionString = db.dataSources(Pointer).odbcConnectionString
        '                End If
        '            End If
        '        End If
        '    Catch ex As Exception
        '        Call throw (ex)
        '    End Try
        'End Function

        '
        '=============================================================================
        '   Verify an Admin Menu Entry
        '       Entries are unique by their name
        '=============================================================================
        '
        Public Sub admin_VerifyAdminMenu(ByVal ParentName As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, Optional ByVal AdminOnly As Boolean = False, Optional ByVal DeveloperOnly As Boolean = False, Optional ByVal NewWindow As Boolean = False, Optional ByVal Active As Boolean = True)
            Call Controllers.appBuilderController.admin_VerifyMenuEntry(Me, ParentName, EntryName, ContentName, LinkPage, SortOrder, AdminOnly, DeveloperOnly, NewWindow, Active, "Menu Entries", "")
        End Sub
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function GetRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            Return db.getRecordID(ContentName, RecordName)
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            Return db.getRecordName(ContentName, RecordID)
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function metaData_IsContentFieldSupported(ByVal ContentName As String, ByVal FieldName As String) As Boolean
            Return metaData.isContentFieldSupported(ContentName, FieldName)
        End Function
        '
        '
        '
        Public Sub tasks_RequestTask(ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String, ByVal RequestedByMemberID As Integer)
            On Error GoTo ErrorTrap 'Const Tn = "RequestTask" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim TaskName As String

            '
            If ExportName = "" Then
                TaskName = CStr(Now()) & " snapshot of unnamed data"
            Else
                TaskName = CStr(Now()) & " snapshot of " & genericController.vbLCase(ExportName)
            End If
            CS = db.cs_insertRecord("Tasks", RequestedByMemberID)
            If db.cs_ok(CS) Then
                Call db.cs_getFilename(CS, "Filename", Filename)
                Call db.cs_set(CS, "Name", TaskName)
                Call db.cs_set(CS, "Command", Command)
                Call db.cs_set(CS, "SQLQuery", SQL)
            End If
            Call db.cs_Close(CS)
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_RequestTask", True)
        End Sub
        '
        '========================================================================
        '   Open a content set with the current whats new list
        '========================================================================
        '
        Public Function csOpenWatchList(ByVal ListName As String, ByVal SortFieldList As String, ByVal ActiveOnly As Boolean, ByVal PageSize As Integer, ByVal PageNumber As Integer) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "OpenCSContentWatchList" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim SQL As String
            'Dim SortFieldList As String
            'Dim iActiveOnly As Boolean
            Dim MethodName As String
            'Dim ListName As String
            Dim CS As Integer
            '
            'SortFieldList = Trim(encodeMissingText(SortFieldList, ""))
            'SortFieldList = encodeMissingText(SortFieldList, "DateAdded")
            If SortFieldList = "" Then
                SortFieldList = "DateAdded"
            End If
            'iActiveOnly = encodeMissingText(ActiveOnly, True)
            'ListName = Trim(genericController.encodeText(ListName))
            '
            MethodName = "csOpenWatchList( " & ListName & ", " & SortFieldList & ", " & ActiveOnly & " )"
            '
            ' ----- Add tablename to the front of SortFieldList fieldnames
            '
            SortFieldList = " " & genericController.vbReplace(SortFieldList, ",", " , ") & " "
            SortFieldList = genericController.vbReplace(SortFieldList, " ID ", " ccContentWatch.ID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " Link ", " ccContentWatch.Link ")
            SortFieldList = genericController.vbReplace(SortFieldList, " LinkLabel ", " ccContentWatch.LinkLabel ")
            SortFieldList = genericController.vbReplace(SortFieldList, " SortOrder ", " ccContentWatch.SortOrder ")
            SortFieldList = genericController.vbReplace(SortFieldList, " DateAdded ", " ccContentWatch.DateAdded ")
            SortFieldList = genericController.vbReplace(SortFieldList, " ContentID ", " ccContentWatch.ContentID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " RecordID ", " ccContentWatch.RecordID ")
            SortFieldList = genericController.vbReplace(SortFieldList, " ModifiedDate ", " ccContentWatch.ModifiedDate ")
            '
            SQL = "SELECT ccContentWatch.ID AS ID, ccContentWatch.Link as Link, ccContentWatch.LinkLabel as LinkLabel, ccContentWatch.SortOrder as SortOrder, ccContentWatch.DateAdded as DateAdded, ccContentWatch.ContentID as ContentID, ccContentWatch.RecordID as RecordID, ccContentWatch.ModifiedDate as ModifiedDate" _
                & " FROM (ccContentWatchLists LEFT JOIN ccContentWatchListRules ON ccContentWatchLists.ID = ccContentWatchListRules.ContentWatchListID) LEFT JOIN ccContentWatch ON ccContentWatchListRules.ContentWatchID = ccContentWatch.ID" _
                & " WHERE (((ccContentWatchLists.Name)=" & db.encodeSQLText(ListName) & ")" _
                    & "AND ((ccContentWatchLists.Active)<>0)" _
                    & "AND ((ccContentWatchListRules.Active)<>0)" _
                    & "AND ((ccContentWatch.Active)<>0)" _
                    & "AND (ccContentWatch.Link is not null)" _
                    & "AND (ccContentWatch.LinkLabel is not null)" _
                    & "AND ((ccContentWatch.WhatsNewDateExpires is null)or(ccContentWatch.WhatsNewDateExpires>" & db.encodeSQLDate(Now) & "))" _
                    & ")" _
                & " ORDER BY " & SortFieldList & ";"
            csOpenWatchList = db.cs_openCsSql_rev("Default", SQL)
            If Not db.cs_ok(csOpenWatchList) Then
                '
                ' Check if listname exists
                '
                CS = db.cs_open("Content Watch Lists", "name=" & db.encodeSQLText(ListName), "ID", , , , , "ID")
                If Not db.cs_ok(CS) Then
                    Call db.cs_Close(CS)
                    CS = db.cs_insertRecord("Content Watch Lists", 0)
                    If db.cs_ok(CS) Then
                        Call db.cs_set(CS, "name", ListName)
                    End If
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csOpenWatchList", True)
        End Function
        '        '
        '        '=============================================================================
        '        '   Return just the copy from a content page
        '        '=============================================================================
        '        '
        '        Public Function csv_TextDeScramble(ByVal Copy As String) As String
        '            On Error GoTo ErrorTrap : 'Const Tn = "TextDeScramble" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim CS As Integer
        '            Dim CPtr As Integer
        '            Dim C As String
        '            Dim CValue As Integer
        '            Dim crc As Integer
        '            Dim ModAnswer As String
        '            Dim Source As String
        '            Dim Base As Integer
        '            Const CMin = 32
        '            Const CMax = 126
        '            '
        '            ' assume this one is not converted
        '            '
        '            Source = Copy
        '            Base = 50
        '            '
        '            ' First characger must be _
        '            ' Second character is the scramble version 'a' is the starting system
        '            '
        '            If Mid(Source, 1, 2) <> "_a" Then
        '                csv_TextDeScramble = Copy
        '            Else
        '                Source = Mid(Source, 3)
        '                '
        '                ' cycle through all characters
        '                '
        '                For CPtr = Len(Source) - 1 To 1 Step -1
        '                    C = Mid(Source, CPtr, 1)
        '                    CValue = Asc(C)
        '                    crc = crc + CValue
        '                    If (CValue < CMin) Or (CValue > CMax) Then
        '                        '
        '                        ' if out of ascii bounds, just leave it in place
        '                        '
        '                    Else
        '                        CValue = CValue - Base
        '                        If CValue < CMin Then
        '                            CValue = CValue + CMax - CMin + 1
        '                        End If
        '                    End If
        '                    csv_TextDeScramble = csv_TextDeScramble & chr(CValue)
        '                Next
        '                '
        '                ' Test mod
        '                '
        '                If CStr(crc Mod 9) <> Mid(Source, Len(Source), 1) Then
        '                    '
        '                    ' Nope - set it back to the input
        '                    '
        '                    csv_TextDeScramble = Copy
        '                End If
        '            End If
        '            '
        '            'csv_TextDeScramble = Mid(Source, 2)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_TextDeScramble", True)
        '        End Function

        '        '
        '        '=============================================================================
        '        '   Return just the copy from a content page
        '        '=============================================================================
        '        '
        '        Public Function csv_TextScramble(ByVal Copy As String) As String
        '            On Error GoTo ErrorTrap : 'Const Tn = "TextScramble" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        '            '
        '            Dim CS As Integer
        '            Dim CPtr As Integer
        '            Dim C As String
        '            Dim CValue As Integer
        '            Dim crc As Integer
        '            Dim Base As Integer
        '            Const CMin = 32
        '            Const CMax = 126
        '            '
        '            ' scrambled starts with _
        '            '
        '            Base = 50
        '            For CPtr = 1 To Len(Copy)
        '                C = Mid(Copy, CPtr, 1)
        '                CValue = Asc(C)
        '                If (CValue < CMin) Or (CValue > CMax) Then
        '                    '
        '                    ' if out of ascii bounds, just leave it in place
        '                    '
        '                Else
        '                    CValue = CValue + Base
        '                    If CValue > CMax Then
        '                        CValue = CValue - CMax + CMin - 1
        '                    End If
        '                End If
        '                '
        '                ' CRC is addition of all scrambled characters
        '                '
        '                crc = crc + CValue
        '                '
        '                ' put together backwards
        '                '
        '                csv_TextScramble = chr(CValue) & csv_TextScramble
        '            Next
        '            '
        '            ' Ends with the mod of the CRC and 13
        '            '
        '            csv_TextScramble = "_a" & csv_TextScramble & CStr(crc Mod 9)
        '            '
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Call csv_HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_TextScramble", True)
        '        End Function
        '
        '===========================================================================================
        '   Verify the Menu record is there, add it if not
        '   If it is default, add all existing sections to it
        '   If this version is too old, it returns 0
        '===========================================================================================
        '
        Public Function csv_VerifyDynamicMenu(ByVal MenuName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "VerifyDynamicMenu" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim CSRule As Integer
            Dim DefaultFound As Boolean
            Dim iMenuName As String
            '
            If True Then
                '
                iMenuName = MenuName
                If iMenuName = "" Then
                    iMenuName = "Default"
                End If
                '
                CS = db.cs_openCsSql_rev("default", "select ID from ccDynamicMenus where name=" & db.encodeSQLText(iMenuName))
                If db.cs_ok(CS) Then
                    csv_VerifyDynamicMenu = db.cs_getInteger(CS, "ID")
                End If
                Call db.cs_Close(CS)
                '
                If csv_VerifyDynamicMenu = 0 Then
                    '
                    ' Add the Menu
                    '
                    CS = db.cs_insertRecord("Dynamic Menus", SystemMemberID)
                    If db.cs_ok(CS) Then
                        csv_VerifyDynamicMenu = db.cs_getInteger(CS, "ID")
                        Call db.cs_set(CS, "name", iMenuName)
                        If True Then
                            Call db.cs_set(CS, "ccGuid", DefaultDynamicMenuGuid)
                        End If
                    End If
                    Call db.cs_Close(CS)
                    '
                    If genericController.vbUCase(iMenuName) = "DEFAULT" Then
                        '
                        ' Adding the Default menu - put all sections into this when it is created
                        '
                        CS = db.cs_open("Site Sections")
                        Do While db.cs_ok(CS)
                            CSRule = db.cs_insertRecord("Dynamic Menu Section Rules", SystemMemberID)
                            If db.cs_ok(CSRule) Then
                                Call db.cs_set(CSRule, "DynamicMenuID", csv_VerifyDynamicMenu)
                                Call db.cs_set(CSRule, "SectionID", db.cs_getInteger(CS, "ID"))
                            End If
                            Call db.cs_Close(CSRule)
                            db.cs_goNext(CS)
                        Loop
                        Call db.cs_Close(CS)
                    End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_VerifyDynamicMenu", True, True)
        End Function
        '
        '===========================================================================================
        '   Verify the Menu record is there, add it if not
        '   If it is default, add all existing sections to it
        '===========================================================================================
        '
        Public Function csv_GetDynamicMenuACSelect() As String
            On Error GoTo ErrorTrap 'Const Tn = "cs_getv_DynamicMenuACSelect" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            '
            If True Then
                '
                If csv_DynamicMenuACSelect = "" Then
                    CS = db.cs_open("Dynamic Menus", , "Name", , , , , "Name")
                    If Not db.cs_ok(CS) Then
                        Call db.cs_Close(CS)
                        Call csv_VerifyDynamicMenu("Default")
                        CS = db.cs_open("Dynamic Menus", , "Name", , , , , "Name")
                    End If
                    Do While db.cs_ok(CS)
                        If csv_DynamicMenuACSelect <> "" Then
                            csv_DynamicMenuACSelect = csv_DynamicMenuACSelect & "|"
                        End If
                        csv_DynamicMenuACSelect = csv_DynamicMenuACSelect & db.cs_getText(CS, "name")
                        db.cs_goNext(CS)
                    Loop
                    Call db.cs_Close(CS)
                End If
                csv_GetDynamicMenuACSelect = csv_DynamicMenuACSelect
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "VerifyDefaultDynamicMenu", True, True)
        End Function
        ''
        ''
        ''
        'Public Property app.config.urlencoder() As String
        '    Get
        '        Return app.config.urlencoderLocal

        '    End Get
        '    Set(ByVal value As String)
        '        app.config.urlencoderLocal = value

        '    End Set
        'End Property
        '
        '
        '
        '
        '=====================================================================================================
        '   Insert into the ActivityLog
        '=====================================================================================================
        '
        Public Sub log_logActivity(ByVal Message As String, ByVal ByMemberID As Integer, ByVal SubjectMemberID As Integer, ByVal SubjectOrganizationID As Integer, Optional ByVal Link As String = "", Optional ByVal VisitorID As Integer = 0, Optional ByVal VisitID As Integer = 0)
            On Error GoTo ErrorTrap 'Const Tn = "LogActivity2" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            '
            CS = db.cs_insertRecord("Activity Log", ByMemberID)
            If db.cs_ok(CS) Then
                Call db.cs_set(CS, "MemberID", SubjectMemberID)
                Call db.cs_set(CS, "OrganizationID", SubjectOrganizationID)
                Call db.cs_set(CS, "Message", Message)
                Call db.cs_set(CS, "Link", Link)
                Call db.cs_set(CS, "VisitorID", VisitorID)
                Call db.cs_set(CS, "VisitID", VisitID)
            End If
            Call db.cs_Close(CS)
            '
            Exit Sub
            '
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Sub
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
        Public Function csv_GetAddonOption(OptionName As String, OptionString As String) As String
            On Error GoTo ErrorTrap 'Const Tn = "GetAddonOption": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            Dim Delimiter As String
            Dim Options() As String
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim TestName As String
            Dim TargetName As String
            '
            WorkingString = OptionString
            csv_GetAddonOption = ""
            If WorkingString <> "" Then
                TargetName = genericController.vbLCase(OptionName)
                'targetName = genericController.vbLCase(encodeNvaArgument(OptionName))
                Options = Split(OptionString, "&")
                'Options = Split(OptionString, vbCrLf)
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
                            csv_GetAddonOption = genericController.decodeNvaArgument(Trim(Mid(Options(Ptr), Pos + 1)))
                            'csv_GetAddonOption = Trim(Mid(Options(Ptr), Pos + 1))
                            Exit For
                        End If
                    End If

                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JavascriptOnLoad() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JavascriptOnLoad": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JavascriptOnLoad_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JavascriptOnLoad_Cnt - 1
                    If web_EncodeContent_JavascriptOnLoad(Ptr) <> "" Then
                        csv_GetEncodeContent_JavascriptOnLoad = web_EncodeContent_JavascriptOnLoad(Ptr)
                        web_EncodeContent_JavascriptOnLoad(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JavascriptBodyEnd() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JavascriptBodyEnd": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JavascriptBodyEnd_cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JavascriptBodyEnd_cnt - 1
                    If web_EncodeContent_JavascriptBodyEnd(Ptr) <> "" Then
                        csv_GetEncodeContent_JavascriptBodyEnd = web_EncodeContent_JavascriptBodyEnd(Ptr)
                        web_EncodeContent_JavascriptBodyEnd(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_JSFilename() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_JSFilename": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_JSFilename_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_JSFilename_Cnt - 1
                    If web_EncodeContent_JSFilename(Ptr) <> "" Then
                        csv_GetEncodeContent_JSFilename = web_EncodeContent_JSFilename(Ptr)
                        web_EncodeContent_JSFilename(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '   Returns the next entry in the array, empty when there are no more
        '
        Public Function csv_GetEncodeContent_StyleFilenames() As String
            On Error GoTo ErrorTrap 'Const Tn = "csv_GetEncodeContent_StyleFilenames": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Ptr As Integer
            '
            If web_EncodeContent_StyleFilenames_Cnt >= 0 Then
                For Ptr = 0 To web_EncodeContent_StyleFilenames_Cnt - 1
                    If web_EncodeContent_StyleFilenames(Ptr) <> "" Then
                        csv_GetEncodeContent_StyleFilenames = web_EncodeContent_StyleFilenames(Ptr)
                        web_EncodeContent_StyleFilenames(Ptr) = ""
                        Exit For
                    End If
                Next
            End If
            '
            Exit Function
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Function
        '
        '========================================================================================================
        ' Returns any head tags picked up during csv_EncodeContent that must be delivered in teh page
        '========================================================================================================
        '
        Public Function web_GetEncodeContent_HeadTags() As String
            '
            web_GetEncodeContent_HeadTags = web_EncodeContent_HeadTags
            web_EncodeContent_HeadTags = ""
            '
        End Function
        ''
        '' ----- temp solution to convert error reporting without spending the time right now
        ''
        'Friend Sub handleLegacyError25(MethodName As String, ErrDescription As String)
        '    Throw New ApplicationException(MethodName & ", " & ErrDescription)
        'End Sub
        '
        '
        '
        Public Sub image_ResizeImage2(SrcFilename As String, DstFilename As String, Width As Integer, Height As Integer, Algorithm As csv_SfImageResizeAlgorithms)
            On Error GoTo ErrorTrap 'Const Tn = "ResizeImage2": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim sf As New imageEditController
            '
            If Width = 0 And Height = 0 Then
                '
                ' error, do nothing but log
                '
                handleExceptionAndContinue(New ApplicationException("Attempt to resize an image to 0,0. This is not allowed.")) ' handleLegacyError3(serverConfig.appConfig.name, "", "dll", "cpCoreClass", "csv_ResizeImage2", ignoreInteger, "", "", False, True, "")
            Else
                If sf.load(SrcFilename) Then
                    If Width = 0 Then
                        sf.width = CInt(Int((sf.width * sf.height) / Height))
                        sf.height = Height
                    ElseIf Height = 0 Then
                        sf.height = CInt(Int((sf.height * sf.width) / Width))
                        sf.width = Width
                    Else
                        sf.height = Height
                        sf.width = Width
                    End If
                    Call sf.save(DstFilename)
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            Throw (New Exception("Unexpected exception"))
        End Sub
        '
        '
        '
        Public Sub image_ResizeImage(SrcFilename As String, DstFilename As String, Width As Integer, Height As Integer)
            Try
                Dim Algorithm As Integer
                '
                Algorithm = genericController.EncodeInteger(siteProperties.getText("ImageResizeSFAlgorithm", "5"))
                Call image_ResizeImage2(SrcFilename, DstFilename, Width, Height, DirectCast(Algorithm, csv_SfImageResizeAlgorithms))
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Serialize an object into a JSON string
        '''' </summary>
        '''' <param name="source"></param>
        '''' <returns></returns>
        'Public Function common_jsonSerialize(source As Object) As String
        '    Try
        '        Dim json As New System.Web.Script.Serialization.JavaScriptSerializer
        '        Return json.Serialize(source)
        '    Catch ex As Exception
        '        throw (ex)
        '        Return ""
        '    End Try
        'End Function
        ''
        ''====================================================================================================
        '''' <summary>
        '''' Deserialize as JSON string into a generic object
        '''' </summary>
        '''' <param name="Source"></param>
        '''' <returns></returns>
        'Public Function common_jsonDeserialize(Source As String) As Object
        '    Dim returnObj As Object = Nothing
        '    Try
        '        Dim json As New System.Web.Script.Serialization.JavaScriptSerializer
        '        returnObj = json.Deserialize(Of Object)(Source)
        '    Catch ex As Exception
        '        throw (ex)
        '    End Try
        '    Return returnObj
        'End Function
        '
        '================================================================================================
        '   deprecated, use csv_getStyleSheet2
        '================================================================================================
        '
        Public Function csv_getStyleSheetProcessed() As String
            csv_getStyleSheetProcessed = htmlDoc.html_getStyleSheet2(0, 0)
        End Function
        '
        '================================================================================================
        '   deprecated, feature not supported
        '================================================================================================
        '
        Public Function csv_ProcessStyleSheet(Source As String) As String
            csv_ProcessStyleSheet = Source
        End Function
        ''
        ''------------------------------------------------------------------------------------------------------------
        ''   encode an argument to be used in an addonOptionString
        ''       optionstring is "name = value &"
        ''       can be Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
        ''------------------------------------------------------------------------------------------------------------
        ''
        'Public Function encodeNvaArgument(Arg As String) As String
        '    encodeNvaArgument = encodeNvaArgument(Arg)
        'End Function
        ''
        ''------------------------------------------------------------------------------------------------------------
        ''   Decodes an argument parsed from an AddonOptionString for all non-allowed characters
        ''       AddonOptionString is a & delimited string of name=value[selector]descriptor
        ''------------------------------------------------------------------------------------------------------------
        ''
        'Public Function genericController.decodeNvaArgument(EncodedArg As String) As String
        '    decodeNvaArgument = genericController.decodeNvaArgument(EncodedArg)
        'End Function
        '
        '=================================================================================================================
        '   csv_GetAddonOptionStringValue
        '
        '   gets the value from a list matching the name
        '
        '   InstanceOptionstring is an "AddonEncoded" name=AddonEncodedValue[selector]descriptor&name=value string
        '=================================================================================================================
        '
        Public Function csv_GetAddonOptionStringValue(OptionName As String, addonOptionString As String) As String
            On Error GoTo ErrorTrap
            '
            Dim Pos As Integer
            Dim s As String
            '
            s = genericController.getSimpleNameValue(OptionName, addonOptionString, "", "&")
            Pos = genericController.vbInstr(1, s, "[")
            If Pos > 0 Then
                s = Left(s, Pos - 1)
            End If
            s = genericController.decodeNvaArgument(s)
            '
            csv_GetAddonOptionStringValue = Trim(s)
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError3("", "", "dll", "ccCommonModule", "csv_GetAddonOptionStringValue", Err.Number, Err.Source, Err.Description, True, False, "")
        End Function
        '
        '================================================================================================
        '   see csv_reportWarning2
        '================================================================================================
        '
        Public Function csv_reportWarning(Name As String, Description As String, generalKey As String, specificKey As String) As String
            Call csv_reportWarning2(Name, Left(Description, 250), "", 0, Description, generalKey, specificKey)
        End Function
        '
        '================================================================================================
        '   Report Warning
        '       A warning is logged in the site warnings log
        '           name - a generic description of the warning
        '               "bad link found on page"
        '           short description - a <255 character cause
        '               "bad link http://thisisabadlink.com"
        '           location - the URL, service or process that caused the problem
        '               "http://goodpageThankHasBadLink.com"
        '           pageid - the record id of the bad page.
        '               "http://goodpageThankHasBadLink.com"
        '           description - a specific description
        '               "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           generalKey - a generic string that describes the warning. the warning report
        '               will display one line for each generalKey (name matches guid)
        '               like "bad link"
        '           specificKey - a string created by the addon logging so it does not continue to log exactly the
        '               same warning over and over. If there are 100 different link not found warnings,
        '               there should be 100 entires with the same guid and name, but 100 different keys. If the
        '               an identical key is found the count increments.
        '               specifickey is like "link to http://www.this.com/pagename was found on http://www.this.com/About-us"
        '           count - the number of times the key was attempted to add. "This error was reported 100 times"
        '================================================================================================
        '
        Public Function csv_reportWarning2(Name As String, shortDescription As String, location As String, PageID As Integer, Description As String, generalKey As String, specificKey As String) As String
            '
            Dim SQL As String
            'dim dt as datatable
            Dim warningId As Integer
            Dim CS As Integer
            '
            warningId = 0
            SQL = "select top 1 ID from ccSiteWarnings" _
                & " where (generalKey=" & db.encodeSQLText(generalKey) & ")" _
                & " and(specificKey=" & db.encodeSQLText(specificKey) & ")" _
                & ""
            Dim dt As DataTable
            dt = db.executeSql(SQL)
            If dt.Rows.Count > 0 Then
                warningId = genericController.EncodeInteger(dt.Rows(0).Item("id"))
            End If
            '
            If warningId <> 0 Then
                '
                ' increment count for matching warning
                '
                SQL = "update ccsitewarnings set count=count+1,DateLastReported=" & db.encodeSQLDate(Now()) & " where id=" & warningId
                Call db.executeSql(SQL)
            Else
                '
                ' insert new record
                '
                CS = db.cs_insertRecord("Site Warnings", 0)
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "name", Name)
                    Call db.cs_set(CS, "description", Description)
                    Call db.cs_set(CS, "generalKey", generalKey)
                    Call db.cs_set(CS, "specificKey", specificKey)
                    Call db.cs_set(CS, "count", 1)
                    Call db.cs_set(CS, "DateLastReported", Now())
                    If True Then
                        Call db.cs_set(CS, "shortDescription", shortDescription)
                        Call db.cs_set(CS, "location", location)
                        Call db.cs_set(CS, "pageId", PageID)
                    End If
                End If
                Call db.cs_Close(CS)
            End If
            '
        End Function
        '
        '================================================================================================
        '   csv_reportAlarm
        '       Saves the error message in a file in the Alarms folder, which will set off the server alarm
        '================================================================================================
        '
        Public Sub csv_reportAlarm(Cause As String)
            Call logController.appendLog(Me, Cause, "Alarms", "alarm")
        End Sub
        ''
        ''------------------------------------------------------------------------------------------------------------
        ''   encode an argument to be used in a 'name=value&' string
        ''       - ohter characters are reserved to do further parsing, see encodeNvaArgument
        ''------------------------------------------------------------------------------------------------------------
        ''
        'Public Function encodeNvaArgument(Arg As String) As String
        '    encodeNvaArgument = encodeNvaArgument(Arg)
        'End Function
        ''
        ''------------------------------------------------------------------------------------------------------------
        ''   decode an argument that came from parsing a name or value from a 'name=value&' string
        ''       split on '&', then on '=', then decode each of the two arguments from either side
        ''       - other characters are reserved to do further parsing, see encodeNvaArgument
        ''------------------------------------------------------------------------------------------------------------
        ''
        'Public Function genericController.decodeNvaArgument(EncodedArg As String) As String
        '    decodeNvaArgument = genericController.decodeNvaArgument(EncodedArg)
        'End Function

        '
        '=================================================================================================================================================
        '   csv_addLinkAlias
        '
        '   Link Alias
        '       A LinkAlias name is a unique string that identifies a page on the site.
        '       A page on the site is generated from the PageID, and the QueryStringSuffix
        '       PageID - obviously, this is the ID of the page
        '       QueryStringSuffix - other things needed on the Query to display the correct content.
        '           The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
        '           Link, plus the suffix needed to find the content.
        '
        '       When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
        '
        '   The Link Alias table no longer needs the Link field.
        '
        '=================================================================================================================================================
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Sub app_addLinkAlias(linkAlias As String, PageID As Integer, QueryStringSuffix As String)
            Dim return_ignoreError As String = ""
            Call app_addLinkAlias2(linkAlias, PageID, QueryStringSuffix, True, False, return_ignoreError)
        End Sub
        '
        ' +++++ 9/8/2011 4.1.482, added csv_addLinkAlias to csv and changed main to call
        '
        Public Sub app_addLinkAlias2(linkAlias As String, PageID As Integer, QueryStringSuffix As String, OverRideDuplicate As Boolean, DupCausesWarning As Boolean, ByRef return_WarningMessage As String)
            On Error GoTo ErrorTrap
            '
            Const SafeString = "0123456789abcdefghijklmnopqrstuvwxyz-_/."
            '
            Dim Ptr As Integer
            Dim TestChr As String
            Dim Src As String
            Dim FieldList As String
            Dim LinkAliasPageID As Integer
            Dim PageContentCID As Integer
            Dim WorkingLinkAlias As String
            Dim CS As Integer
            Dim LoopCnt As Integer
            'Dim fs As New fileSystemClass
            Dim FolderCheck As String
            Dim SQL As String
            Dim AllowLinkAlias As Boolean
            'dim buildversion As String
            '
            If (True) Then
                AllowLinkAlias = siteProperties.getBoolean("allowLinkAlias", False)
                WorkingLinkAlias = linkAlias
                If (WorkingLinkAlias <> "") Then
                    '
                    ' remove nonsafe URL characters
                    '
                    Src = WorkingLinkAlias
                    Src = genericController.vbReplace(Src, "’", "'")
                    Src = genericController.vbReplace(Src, vbTab, " ")
                    WorkingLinkAlias = ""
                    For Ptr = 1 To Len(Src) + 1
                        TestChr = Mid(Src, Ptr, 1)
                        If genericController.vbInstr(1, SafeString, TestChr, vbTextCompare) <> 0 Then
                        Else
                            TestChr = vbTab
                        End If
                        WorkingLinkAlias = WorkingLinkAlias & TestChr
                    Next
                    Ptr = 0
                    Do While genericController.vbInstr(1, WorkingLinkAlias, vbTab & vbTab) <> 0 And (Ptr < 100)
                        WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab & vbTab, vbTab)
                        Ptr = Ptr + 1
                    Loop
                    If Right(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 1, Len(WorkingLinkAlias) - 1)
                    End If
                    If Left(WorkingLinkAlias, 1) = vbTab Then
                        WorkingLinkAlias = Mid(WorkingLinkAlias, 2)
                    End If
                    WorkingLinkAlias = genericController.vbReplace(WorkingLinkAlias, vbTab, "-")
                    If (WorkingLinkAlias <> "") Then
                        '
                        ' Make sure there is not a folder or page in the wwwroot that matches this Alias
                        '
                        If Left(WorkingLinkAlias, 1) <> "/" Then
                            WorkingLinkAlias = "/" & WorkingLinkAlias
                        End If
                        '
                        If genericController.vbLCase(WorkingLinkAlias) = genericController.vbLCase("/" & serverConfig.appConfig.name) Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf genericController.vbLCase(WorkingLinkAlias) = "/cclib" Then
                            '
                            ' This alias points to the cclib folder
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a virtual directory in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        ElseIf appRootFiles.pathExists(serverConfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            'ElseIf appRootFiles.pathExists(serverConfig.clusterPath & serverconfig.appConfig.appRootFilesPath & "\" & Mid(WorkingLinkAlias, 2)) Then
                            '
                            ' This alias points to a different link, call it an error
                            '
                            If AllowLinkAlias Then
                                return_WarningMessage = "" _
                                    & "The Link Alias being created (" & WorkingLinkAlias & ") can not be used because there is a folder in your website directory that already uses this name." _
                                    & " Please change it to ensure the Link Alias is unique. To set or change the Link Alias, use the Link Alias tab and select a name not used by another page."
                            End If
                        Else
                            '
                            ' Make sure there is one here for this
                            '
                            If True Then
                                FieldList = "Name,PageID,QueryStringSuffix"
                            Else
                                '
                                ' must be > 33914 to run this routine
                                '
                                FieldList = "Name,PageID,'' as QueryStringSuffix"
                            End If
                            CS = db.cs_open("Link Aliases", "name=" & db.encodeSQLText(WorkingLinkAlias), , , , , , FieldList)
                            If Not db.cs_ok(CS) Then
                                '
                                ' Alias not found, create a Link Aliases
                                '
                                Call db.cs_Close(CS)
                                CS = db.cs_insertRecord("Link Aliases", 0)
                                If db.cs_ok(CS) Then
                                    Call db.cs_set(CS, "Name", WorkingLinkAlias)
                                    'Call app.csv_SetCS(CS, "Link", Link)
                                    Call db.cs_set(CS, "Pageid", PageID)
                                    If True Then
                                        Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                    End If
                                End If
                            Else
                                '
                                ' Alias found, verify the pageid & QueryStringSuffix
                                '
                                Dim CurrentLinkAliasID As Integer
                                Dim resaveLinkAlias As Boolean
                                Dim CS2 As Integer
                                LinkAliasPageID = db.cs_getInteger(CS, "pageID")
                                If (db.cs_getText(CS, "QueryStringSuffix").ToLower = QueryStringSuffix.ToLower) And (PageID = LinkAliasPageID) Then
                                    '
                                    ' it maches a current entry for this link alias, if the current entry is not the highest number id,
                                    '   remove it and add this one
                                    '
                                    CurrentLinkAliasID = db.cs_getInteger(CS, "id")
                                    CS2 = db.cs_openCsSql_rev("default", "select top 1 id from ccLinkAliases where pageid=" & LinkAliasPageID & " order by id desc")
                                    If db.cs_ok(CS2) Then
                                        resaveLinkAlias = (CurrentLinkAliasID <> db.cs_getInteger(CS2, "id"))
                                    End If
                                    Call db.cs_Close(CS2)
                                    If resaveLinkAlias Then
                                        Call db.executeSql("delete from ccLinkAliases where id=" & CurrentLinkAliasID)
                                        Call db.cs_Close(CS)
                                        CS = db.cs_insertRecord("Link Aliases", 0)
                                        If db.cs_ok(CS) Then
                                            Call db.cs_set(CS, "Name", WorkingLinkAlias)
                                            Call db.cs_set(CS, "Pageid", PageID)
                                            If True Then
                                                Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                            End If
                                        End If
                                    End If
                                Else
                                    '
                                    ' Does not match, this is either a change, or a duplicate that needs to be blocked
                                    '
                                    If OverRideDuplicate Then
                                        '
                                        ' change the Link Alias to the new link
                                        '
                                        'Call app.csv_SetCS(CS, "Link", Link)
                                        Call db.cs_set(CS, "Pageid", PageID)
                                        If True Then
                                            Call db.cs_set(CS, "QueryStringSuffix", QueryStringSuffix)
                                        End If
                                    ElseIf AllowLinkAlias Then
                                        '
                                        ' This alias points to a different link, and link aliasing is in use, call it an error (but save record anyway)
                                        '
                                        If DupCausesWarning Then
                                            If LinkAliasPageID = 0 Then '
                                                PageContentCID = metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique on this site. To set or change the Link Alias, clicke the Link Alias tab and select a name not used by another page or a folder in your website."
                                            Else
                                                PageContentCID = metaData.getContentId("Page Content")
                                                return_WarningMessage = "" _
                                                    & "This page has been saved, but the Link Alias could not be created (" & WorkingLinkAlias & ") because it is already in use for another page (<a href=""?af=4&cid=" & PageContentCID & "&id=" & LinkAliasPageID & """>edit</a>)." _
                                                    & " To use Link Aliasing (friendly page names) for this page, the Link Alias value must be unique. To set or change the Link Alias, click the Link Alias tab and select a name not used by another page or a folder in your website."
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            Call db.cs_Close(CS)
                            Call cache_linkAlias_clear()
                        End If
                    End If
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError4(Err.Number, Err.Source, Err.Description, "csv_addLinkAlias", True)
        End Sub
        '
        '
        '
        Public Function csv_GetLinkedText(ByVal AnchorTag As String, ByVal AnchorText As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLinkedText")
            '
            Dim UcaseAnchorText As String
            Dim LinkPosition As Integer
            Dim MethodName As String
            Dim iAnchorTag As String
            Dim iAnchorText As String
            '
            MethodName = "csv_GetLinkedText"
            '
            csv_GetLinkedText = ""
            iAnchorTag = genericController.encodeText(AnchorTag)
            iAnchorText = genericController.encodeText(AnchorText)
            UcaseAnchorText = genericController.vbUCase(iAnchorText)
            If (iAnchorTag <> "") And (iAnchorText <> "") Then
                LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", -1)
                If LinkPosition = 0 Then
                    csv_GetLinkedText = iAnchorTag & iAnchorText & "</a>"
                Else
                    csv_GetLinkedText = iAnchorText
                    LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", -1)
                    Do While LinkPosition > 1
                        csv_GetLinkedText = Mid(csv_GetLinkedText, 1, LinkPosition - 1) & "</a>" & Mid(csv_GetLinkedText, LinkPosition + 7)
                        LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1)
                        If LinkPosition <> 0 Then
                            csv_GetLinkedText = Mid(csv_GetLinkedText, 1, LinkPosition - 1) & iAnchorTag & Mid(csv_GetLinkedText, LinkPosition + 6)
                        End If
                        LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", LinkPosition)
                    Loop
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError7(MethodName, "Unexpected Trap")
            '
        End Function
        '
        '========================================================================
        '   convert a virtual file into a Link usable on the website:
        '       convert all \ to /
        '       if it includes "://", leave it along
        '       if it starts with "/", it is already root relative, leave it alone
        '       else (if it start with a file or a path), add the serverFilePath
        '========================================================================
        '
        Public Function csv_getVirtualFileLink(ByVal serverFilePath As String, ByVal virtualFile As String) As String
            Dim returnLink As String
            '
            returnLink = virtualFile
            returnLink = genericController.vbReplace(returnLink, "\", "/")
            If genericController.vbInstr(1, returnLink, "://") <> 0 Then
                '
                ' icon is an Absolute URL - leave it
                '
            ElseIf Left(returnLink, 1) = "/" Then
                '
                ' icon is Root Relative, leave it
                '
            Else
                '
                ' icon is a virtual file, add the serverfilepath
                '
                returnLink = serverFilePath & returnLink
            End If
            csv_getVirtualFileLink = returnLink
        End Function
        '
        '========================================================================
        '   convert a resource file into a filename that can be read with app.csv_ReadFile()
        '       convert all / to \
        '       if it includes "://", it is a root file
        '       if it starts with "/", it is already root relative
        '       else (if it start with a file or a path), add the serverFilePath
        '========================================================================
        '
        Public Function csv_getPhysicalFilename(ByVal VirtualFilename As String) As String
            Return genericController.convertCdnUrlToCdnPathFilename(VirtualFilename)
        End Function
        '
        '========================================================================
        '   42private
        '
        ' ----- Process Member Actions (called only from Init)
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessFormMyProfile")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CSMember As Integer
            Dim CS As Integer
            Dim TopicCount As Integer
            Dim TopicPointer As Integer
            Dim TopicID As Integer
            Dim TopicRulesCID As Integer
            Dim Panel As String
            Dim MethodName As String
            Dim CreatePathBlock As Boolean
            Dim AllowChange As Boolean
            Dim PathID As Integer
            Dim Filename As String
            Dim Button As String
            Dim GroupCount As Integer
            Dim GroupPointer As Integer
            Dim CSPointer As Integer
            Dim CDef As cdefModel
            Dim ContentName As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim FieldName As String
            Dim FieldValue As String
            Dim CSTest As Integer
            Dim PeopleCID As Integer
            Dim ErrorMessage As String = ""
            Dim ErrorCode As Integer
            Dim FirstName As String
            Dim LastName As String
            Dim Newusername As String
            Dim NewPassword As String
            '
            MethodName = "main_ProcessFormMyProfile"
            '
            ' ----- Check if new username is allowed
            '
            Button = docProperties.getText("Button")
            If (Button = ButtonSave) Then
                AllowChange = True
                PeopleCID = main_GetContentID("People")
                Newusername = docProperties.getText("username")
                NewPassword = docProperties.getText("password")
                If Newusername = "" Then
                    '
                    ' Sest to blank
                    '
                    AllowChange = True
                ElseIf genericController.vbUCase(Newusername) <> genericController.vbUCase(authContext.user.Username) Then
                    '
                    ' ----- username changed, check if change is allowed
                    '
                    If Not authContext.isNewLoginOK(Me, Newusername, NewPassword, ErrorMessage, ErrorCode) Then
                        error_AddUserError(ErrorMessage)
                        AllowChange = False
                    End If
                End If
                If AllowChange Then
                    CSMember = db.cs_open("people", "id=" & db.encodeSQLNumber(authContext.user.ID))
                    If Not db.cs_ok(CSMember) Then
                        Call error_AddUserError("There was a problem locating your account record. No changes were saved.")
                        ' if user error, it goes back to the hardcodedpage
                        'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                    Else
                        '
                        ' Check for unique violations first
                        '
                        ContentName = metaData.getContentNameByID(db.cs_getInteger(CSMember, "ContentControlID"))
                        If ContentName = "" Then
                            Call error_AddUserError("There was a problem locating the information you requested.")
                        Else
                            CDef = metaData.getCdef(ContentName)
                            For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In CDef.fields
                                Dim field As CDefFieldModel = keyValuePair.Value
                                If field.UniqueName Then
                                    FieldName = field.nameLc
                                    FieldValue = docProperties.getText(FieldName)
                                    If FieldValue <> "" Then
                                        CSTest = db.cs_open(ContentName, "(" & FieldName & "=" & db.encodeSQLText(FieldValue) & ")and(ID<>" & authContext.user.ID & ")")
                                        If db.cs_ok(CSTest) Then
                                            Call error_AddUserError("The field '" & FieldName & "' must be unique, and another account has already used '" & FieldValue & "'")
                                        End If
                                        Call db.cs_Close(CSTest)
                                    End If
                                End If
                            Next
                        End If
                        If error_IsUserError() Then
                            ' goes to hardcodedpage on user error
                            'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                        Else
                            '
                            ' Personal Info
                            '
                            FirstName = docProperties.getText("FirstName")
                            LastName = docProperties.getText("LastName")
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "firstname")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "LastName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Name")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "email")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "company")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "title")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "address")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "city")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "state")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "zip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "country")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "phone")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "fax")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "ResumeFilename")
                            '
                            ' Billing Info
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "BillName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billemail")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcompany")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billaddress")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcity")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billstate")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billzip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billcountry")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billphone")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Billfax")
                            '
                            ' Shiping Info
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "ShipName")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcompany")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipaddress")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcity")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipstate")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipzip")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipcountry")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "Shipphone")
                            '
                            ' Site preferences
                            '
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "username")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "password")
                            Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AllowBulkEmail")
                            Call main_ProcessFormMyProfile_UpdateField(CSMember, "LanguageID")

                            If siteProperties.getBoolean("AllowAutoLogin", False) Then
                                Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AutoLogin")
                            End If
                            If authContext.isAuthenticatedContentManager(Me) Then
                                Call main_ProcessFormMyProfile_UpdateFieldBoolean(CSMember, "AllowToolsPanel")
                            End If
                            '
                            ' --- update Topic records
                            '
                            Call main_ProcessCheckList(rnMyProfileTopics, "people", "memberid", "topics", "member topic rules", "memberid", "topicid")
                            '
                            ' --- Update Group Records
                            '
                            Call main_ProcessCheckList("MemberRules", "Members", genericController.encodeText(authContext.user.ID), "Groups", "Member Rules", "MemberID", "GroupID")
                            '
                            '
                            '
                            If app_errorCount > 0 Then
                                Call error_AddUserError("An error occurred which prevented your information from being saved.")
                                'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                            Else
                                If app_errorCount > 0 Then
                                    Call error_AddUserError("An error occurred while saving your information.")
                                    'LegacyInterceptPageSN = LegacyInterceptPageSNMyProfile
                                End If
                            End If
                        End If
                        Call cache.invalidateContent("People")
                    End If
                    Call db.cs_Close(CSMember)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        End Sub
        '
        '========================================================================
        '   42private
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile_UpdateField(ByVal CSMember As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap
            '
            Dim FieldValue As String
            '
            FieldValue = docProperties.getText(FieldName)
            If db.cs_getText(CSMember, FieldName) <> FieldValue Then
                Call log_LogActivity2("profile changed " & FieldName, authContext.user.ID, authContext.user.OrganizationID)
                Call db.cs_set(CSMember, FieldName, FieldValue)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_ProcessFormMyProfile_UpdateField")
        End Sub
        '
        '========================================================================
        '   42private
        '========================================================================
        '
        Private Sub main_ProcessFormMyProfile_UpdateFieldBoolean(ByVal CSMember As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap
            '
            Dim FieldValue As Boolean
            '
            FieldValue = docProperties.getBoolean(FieldName)
            If db.cs_getBoolean(CSMember, FieldName) <> FieldValue Then
                Call log_LogActivity2("profile changed " & FieldName, authContext.user.ID, authContext.user.OrganizationID)
                Call db.cs_set(CSMember, FieldName, FieldValue)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_ProcessFormMyProfile_UpdateFieldBoolean")
        End Sub
        '
        '===========================================================================================
        '   42legacy
        ' ----- main_RedirectHTTP
        '   This is a compatibility call that requires the HTTP be included, or it will be added.
        '   That means "www.contensive.com" will work, but "index.asp" will not
        '===========================================================================================
        '
        Public Sub main_RedirectHTTP(ByVal Link As String)
            If Not genericController.isInStr(1, Link, "://") Then
                Link = webServer.webServerIO_requestProtocol & Link
            End If
            Call webServer.webServerIO_Redirect2(Link, "call to main_RedirectHTTP(" & genericController.encodeText(Link) & "), no reason given.", False)
        End Sub
        '
        '===========================================================================================
        '   ----- Redirect without reason - compatibility only
        '===========================================================================================
        '
        Public Sub main_Redirect(ByVal Link As Object)
            Call webServer.webServerIO_Redirect2(genericController.encodeText(Link), "No explaination provided", False)
        End Sub
        '
        '========================================================================
        ' Stop sending to the HTMLStream
        '========================================================================
        '
        Public Sub doc_close()
            '
            ' 2011/3/11 - just stop future Contensive output, do not end the parent's response object, developer may want to add more
            '
            docOpen = False
        End Sub
        '
        '=============================================================================
        ' Cleans a text file of control characters, allowing only vblf
        '=============================================================================
        '
        Public Function main_RemoveControlCharacters(ByVal DirtyText As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("RemoveControlCharacters")
            '
            Dim Pointer As Integer
            Dim ChrTest As Integer
            Dim MethodName As String
            Dim iDirtyText As String
            '
            MethodName = "main_RemoveControlCharacters"
            '
            iDirtyText = genericController.encodeText(DirtyText)
            main_RemoveControlCharacters = ""
            If (iDirtyText <> "") Then
                main_RemoveControlCharacters = ""
                For Pointer = 1 To Len(iDirtyText)
                    ChrTest = Asc(Mid(iDirtyText, Pointer, 1))
                    If ChrTest >= 32 And ChrTest < 128 Then
                        main_RemoveControlCharacters = main_RemoveControlCharacters & Chr(ChrTest)
                    Else
                        Select Case ChrTest
                            Case 9
                                main_RemoveControlCharacters = main_RemoveControlCharacters & " "
                            Case 10
                                main_RemoveControlCharacters = main_RemoveControlCharacters & vbLf
                        End Select
                    End If
                Next
                '
                ' limit CRLF to 2
                '
                Do While genericController.vbInstr(main_RemoveControlCharacters, vbLf & vbLf & vbLf) <> 0
                    main_RemoveControlCharacters = genericController.vbReplace(main_RemoveControlCharacters, vbLf & vbLf & vbLf, vbLf & vbLf)
                Loop
                '
                ' limit spaces to 1
                '
                Do While genericController.vbInstr(main_RemoveControlCharacters, "  ") <> 0
                    main_RemoveControlCharacters = genericController.vbReplace(main_RemoveControlCharacters, "  ", " ")
                Loop
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '========================================================================
        '   Test Point
        '       If main_PageTestPointPrinting print a string, value paior
        '========================================================================
        '
        Public Sub debug_testPoint(Message As String)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("TestPoint")
            '
            Dim ElapsedTime As Single
            Dim iMessage As String
            '
            '
            ' ----- If not main_PageTestPointPrinting, exit right away
            '
            If testPointPrinting Then
                '
                ' write to stream
                '
                ElapsedTime = CSng(GetTickCount - app_startTickCount) / 1000
                iMessage = genericController.encodeText(Message)
                iMessage = Format((ElapsedTime), "00.000") & " - " & iMessage
                main_testPointMessage = main_testPointMessage & "<nobr>" & iMessage & "</nobr><br >"
                'writeAltBuffer ("<nobr>" & iMessage & "</nobr><br >")
            End If
            If siteProperties.allowTestPointLogging Then
                '
                ' write to debug log in virtual files - to read from a test verbose viewer
                '
                iMessage = genericController.encodeText(Message)
                iMessage = genericController.vbReplace(iMessage, vbCrLf, " ")
                iMessage = genericController.vbReplace(iMessage, vbCr, " ")
                iMessage = genericController.vbReplace(iMessage, vbLf, " ")
                iMessage = FormatDateTime(Now, vbShortTime) & vbTab & Format((ElapsedTime), "00.000") & vbTab & authContext.visit.ID & vbTab & iMessage
                '
                logController.appendLog(Me, iMessage, "", "testPoints_" & serverConfig.appConfig.name)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_TestPoint")
        End Sub
        ''
        ''========================================================================
        '' main_RedirectByRecord( iContentName, iRecordID )
        ''   looks up the record
        ''   increments the 'clicks' field and redirects to the 'link' field
        ''   if the record is not found or there is no link, it just returns
        ''   Note: also supports iContentName for pre-2.1 sites
        ''========================================================================
        ''
        'Public Sub main_RedirectByRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "")
        '    Call main_RedirectByRecord_ReturnStatus(ContentName, RecordID, FieldName)
        'End Sub
        '
        '========================================================================
        ' main_RedirectByRecord( iContentName, iRecordID )
        '   looks up the record
        '   increments the 'clicks' field and redirects to the 'link' field
        '   returns true if the redirect happened OK
        '========================================================================
        '
        Public Function main_RedirectByRecord_ReturnStatus(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "") As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("RedirectByRecord_ReturnStatus")
            '
            'If Not (true) Then Exit Function
            '
            Dim Link As String
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim ContentID As Integer
            Dim CSHost As Integer
            Dim HostContentName As String
            Dim HostRecordID As Integer
            Dim BlockRedirect As Boolean
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim iFieldName As String
            Dim LinkPrefix As String
            Dim EncodedLink As String
            Dim NonEncodedLink As String = ""
            Dim RecordActive As Boolean
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iFieldName = genericController.encodeEmptyText(FieldName, "link")
            '
            MethodName = "main_RedirectByRecord_ReturnStatus( " & iContentName & ", " & iRecordID & ", " & genericController.encodeEmptyText(FieldName, "(fieldname empty)") & ")"
            '
            main_RedirectByRecord_ReturnStatus = False
            BlockRedirect = False
            CSPointer = db.cs_open(iContentName, "ID=" & iRecordID)
            If db.cs_ok(CSPointer) Then
                ' 2/18/2008 - EncodeLink change
                '
                ' Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                '
                EncodedLink = Trim(db.cs_getText(CSPointer, iFieldName))
                If EncodedLink = "" Then
                    BlockRedirect = True
                Else
                    '
                    ' ----- handle content special cases (prevent redirect to deleted records)
                    '
                    NonEncodedLink = htmlDoc.main_DecodeUrl(EncodedLink)
                    Select Case genericController.vbUCase(iContentName)
                        Case "CONTENT WATCH"
                            '
                            ' ----- special case
                            '       if this is a content watch record, check the underlying content for
                            '       inactive or expired before redirecting
                            '
                            LinkPrefix = webServer.webServerIO_requestContentWatchPrefix
                            ContentID = (db.cs_getInteger(CSPointer, "ContentID"))
                            HostContentName = metaData.getContentNameByID(ContentID)
                            If (HostContentName = "") Then
                                '
                                ' ----- Content Watch with a bad ContentID, mark inactive
                                '
                                BlockRedirect = True
                                Call db.cs_set(CSPointer, "active", 0)
                            Else
                                HostRecordID = (db.cs_getInteger(CSPointer, "RecordID"))
                                If HostRecordID = 0 Then
                                    '
                                    ' ----- Content Watch with a bad iRecordID, mark inactive
                                    '
                                    BlockRedirect = True
                                    Call db.cs_set(CSPointer, "active", 0)
                                Else
                                    CSHost = db.cs_open(HostContentName, "ID=" & HostRecordID)
                                    If Not db.cs_ok(CSHost) Then
                                        '
                                        ' ----- Content Watch host record not found, mark inactive
                                        '
                                        BlockRedirect = True
                                        Call db.cs_set(CSPointer, "active", 0)
                                    End If
                                End If
                                Call db.cs_Close(CSHost)
                            End If
                            If BlockRedirect Then
                                '
                                ' ----- if a content watch record is blocked, delete the content tracking
                                '
                                Call metaData_DeleteContentTracking(HostContentName, HostRecordID, False)
                            End If
                    End Select
                End If
                If Not BlockRedirect Then
                    '
                    ' If link incorrectly includes the LinkPrefix, take it off first, then add it back
                    '
                    NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix)
                    If db.cs_isFieldSupported(CSPointer, "Clicks") Then
                        Call db.cs_set(CSPointer, "Clicks", (db.cs_getNumber(CSPointer, "Clicks")) + 1)
                    End If
                    Call webServer.webServerIO_Redirect2(LinkPrefix & NonEncodedLink, "Call to " & MethodName & ", no reason given.", False)
                    main_RedirectByRecord_ReturnStatus = True
                End If
            End If
            Call db.cs_Close(CSPointer)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '========================================================================
        ' main_IncrementTableField( TableName, RecordID, Fieldname )
        '========================================================================
        '
        Public Sub main_IncrementTableField(ByVal TableName As String, ByVal RecordID As Integer, ByVal FieldName As String, Optional ByVal DataSourceName As String = "")
            Call db.executeSql("update " & TableName & " set " & FieldName & "=" & FieldName & "+1 where id=" & RecordID, DataSourceName)

            '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("IncrementTableField")
            '            '
            '            'If Not (true) Then Exit Sub
            '            '
            '            Dim SQL As String
            '            'dim dt as datatable
            '            Dim iDataSourceName As String
            '            Dim RecordValue As Integer
            '            Dim iFieldName As String
            '            Dim iTableName As String
            '            Dim iRecordID As Integer
            '            '
            '            iDataSourceName = genericController.encodeText(main_encodeMissingText(DataSourceName, "Default"))
            '            iFieldName = genericController.encodeText(FieldName)
            '            iTableName = genericController.encodeText(TableName)
            '            iRecordID = genericController.EncodeInteger(RecordID)
            '            '
            '            SQL = "Select " & iFieldName & " FROM " & iTableName & " where ID=" & iRecordID & ";"
            '            RS = main_OpenRSSQL(iDataSourceName, SQL)
            '            If (isDataTableOk(rs)) Then
            '                If Not rs.rows.count=0 Then
            '                    RecordValue = genericController.EncodeInteger(RS(iFieldName)) + 1
            '                    SQL = "Update " & iTableName & " set " & iFieldName & "=" & RecordValue & " where ID=" & iRecordID & ";"
            '                    Call main_ExecuteSQL(iDataSourceName, SQL)
            '                End If
            '                If false Then
            '                    'RS.Close()
            '                End If
            '                'RS = Nothing
            '            End If

            '            Exit Sub
            '            '
            'ErrorTrap:
            '            Call main_HandleClassErrorAndResume_TrapPatch1("main_IncrementTableField")
        End Sub
        '
        '=============================================================================
        '   See main_GetNameValue_Internal
        '=============================================================================
        '
        Public Function main_GetNameValue(Tag As String, Name As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetNameValue")
            '
            'If Not (true) Then Exit Function
            main_GetNameValue = main_GetNameValue_Internal(genericController.encodeText(Tag), genericController.encodeText(Name))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetNameValue")
        End Function
        '
        '=============================================================================
        ' ----- Return the value associated with the name given
        '   NameValueString is a string of Name=Value pairs, separated by spaces or "&"
        '   If Name is not given, returns ""
        '   If Name present but no value, returns true (as if Name=true)
        '   If Name = Value, it returns value
        '=============================================================================
        '
        Public Function main_GetNameValue_Internal(NameValueString As String, Name As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetNameValue_Internal")
            '
            'If Not (true) Then Exit Function
            '
            Dim NameValueStringWorking As String
            Dim UcaseNameValueStringWorking As String
            Dim Position As Integer
            Dim PositionEqual As Integer
            Dim PositionEnd As Integer
            Dim MethodName As String
            Dim pairs() As String
            Dim PairCount As Integer
            Dim PairPointer As Integer
            Dim PairSplit() As String
            '
            MethodName = "main_GetNameValue_Internal"
            '
            If ((NameValueString <> "") And (Name <> "")) Then
                Do While genericController.vbInstr(1, NameValueStringWorking, " =") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " =", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "= ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "= ", "=")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, "& ") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "& ", "&")
                Loop
                Do While genericController.vbInstr(1, NameValueStringWorking, " &") <> 0
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " &", "&")
                Loop
                NameValueStringWorking = NameValueString & "&"
                UcaseNameValueStringWorking = genericController.vbUCase(NameValueStringWorking)
                '
                main_GetNameValue_Internal = ""
                If NameValueStringWorking <> "" Then
                    pairs = Split(NameValueStringWorking, "&")
                    PairCount = UBound(pairs) + 1
                    For PairPointer = 0 To PairCount - 1
                        PairSplit = Split(pairs(PairPointer), "=")
                        If genericController.vbUCase(PairSplit(0)) = genericController.vbUCase(Name) Then
                            If UBound(PairSplit) > 0 Then
                                main_GetNameValue_Internal = PairSplit(1)
                            End If
                            Exit For
                        End If
                    Next
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelHilite", Optional ByVal StyleShadow As String = "ccPanelShadow", Optional ByVal Width As String = "100%", Optional ByVal Padding As Integer = 5, Optional ByVal HeightMin As Integer = 1) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanel")
            '
            'If Not (true) Then Exit Function
            '
            '
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            Dim s As String
            Dim s0 As String
            Dim s1 As String
            Dim s2 As String
            Dim s3 As String
            Dim s4 As String
            Dim contentPanelWidthStyle As String
            '
            MethodName = "main_GetPanelTop"
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = Padding.ToString
            MyHeightMin = HeightMin.ToString
            '
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
                contentPanelWidthStyle = ContentPanelWidth & "px"
            Else
                ContentPanelWidth = "100%"
                contentPanelWidthStyle = ContentPanelWidth
            End If
            '
            '
            '
            s0 = "" _
                & cr & "<td style=""padding:" & MyPadding & "px;vertical-align:top"" class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(genericController.encodeText(Panel)) _
                & cr & "</td>" _
                & ""
            '
            s1 = "" _
                & cr & "<tr>" _
                & genericController.kmaIndent(s0) _
                & cr & "</tr>" _
                & ""
            s2 = "" _
                & cr & "<table style=""width:" & contentPanelWidthStyle & ";border:0px;"" class=""" & MyStylePanel & """ cellspacing=""0"">" _
                & genericController.kmaIndent(s1) _
                & cr & "</table>" _
                & ""
            s3 = "" _
                & cr & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(s2) _
                & cr & "</td>" _
                & cr & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & ""
            s4 = "" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & genericController.kmaIndent(s3) _
                & cr & "</tr>" _
                & cr & "<tr>" _
                & cr2 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr & "</tr>" _
                & ""
            main_GetPanel = "" _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>" _
                & genericController.kmaIndent(s4) _
                & cr & "</table>" _
                & ""

            '-------------------------------------------------------------------------
            '
            '    main_GetPanel = "" _
            '        & cr & main_GetPanelTop(StylePanel, StyleHilite, StyleShadow, Width, Padding, HeightMin) _
            '        & genericController.kmaIndent(genericController.encodeText(Panel)) _
            '        & cr & main_GetPanelBottom(StylePanel, StyleHilite, StyleShadow, Width, Padding)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanel")
        End Function
        '
        '========================================================================
        '   main_GetPanel( Panel, Optional StylePanel, Optional StyleHilite, Optional StyleShadow, Optional Width, Optional Padding, Optional HeightMin) As String
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetReversePanel(ByVal Panel As String, Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "ccPanelShadow", Optional ByVal StyleShadow As String = "ccPanelHilite", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetReversePanel")
            '
            'If Not (true) Then Exit Function
            '
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            '
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelShadow")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelHilite")

            main_GetReversePanel = main_GetPanelTop(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding, HeightMin) _
                & genericController.encodeText(Panel) _
                & main_GetPanelBottom(StylePanel, MyStyleHilite, MyStyleShadow, Width, Padding)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetReversePanel")
        End Function
        '
        '========================================================================
        ' Return a panel header with the header message reversed out of the left
        '========================================================================
        '
        Public Function main_GetPanelHeader(ByVal HeaderMessage As String, Optional ByVal RightSideMessage As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelHeader")
            '
            Dim iHeaderMessage As String
            Dim iRightSideMessage As String
            Dim Adminui As New adminUIController(Me)
            '
            'If Not (true) Then Exit Function
            '
            iHeaderMessage = genericController.encodeText(HeaderMessage)
            iRightSideMessage = genericController.encodeEmptyText(RightSideMessage, FormatDateTime(app_startTime))
            main_GetPanelHeader = Adminui.GetHeader(iHeaderMessage, iRightSideMessage)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelHeader")
        End Function

        '
        '========================================================================
        ' Prints the top of display panel
        '   Must be closed with PrintPanelBottom
        '========================================================================
        '
        Public Function main_GetPanelTop(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "", Optional ByVal HeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelTop")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentPanelWidth As String
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            Dim MyHeightMin As String
            '
            main_GetPanelTop = ""
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MyHeightMin = genericController.encodeEmptyText(HeightMin, "1")
            MethodName = "main_GetPanelTop"
            If genericController.vbIsNumeric(MyWidth) Then
                ContentPanelWidth = (CInt(MyWidth) - 2).ToString
            Else
                ContentPanelWidth = "100%"
            End If
            main_GetPanelTop = main_GetPanelTop _
                & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""" & MyWidth & """ class=""" & MyStylePanel & """>"
            '
            ' --- top hilite row
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>"
            '
            ' --- center row with Panel
            '
            main_GetPanelTop = main_GetPanelTop _
                & cr2 & "<tr>" _
                & cr3 & "<td width=""1"" height=""" & MyHeightMin & """ class=""" & MyStyleHilite & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""" & MyHeightMin & """ width=""1"" ></td>" _
                & cr3 & "<td width=""" & ContentPanelWidth & """ valign=""top"" align=""left"" class=""" & MyStylePanel & """>" _
                & cr4 & "<table border=""0"" cellpadding=""" & MyPadding & """ cellspacing=""0"" width=""" & ContentPanelWidth & """ class=""" & MyStylePanel & """>" _
                & cr5 & "<tr>" _
                & cr6 & "<td valign=""top"" class=""" & MyStylePanel & """><Span class=""" & MyStylePanel & """>"
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Return a panel with the input as center
        '========================================================================
        '
        Public Function main_GetPanelBottom(Optional ByVal StylePanel As String = "", Optional ByVal StyleHilite As String = "", Optional ByVal StyleShadow As String = "", Optional ByVal Width As String = "", Optional ByVal Padding As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelBottom")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim MyStylePanel As String
            Dim MyStyleHilite As String
            Dim MyStyleShadow As String
            Dim MyWidth As String
            Dim MyPadding As String
            '
            MyStylePanel = genericController.encodeEmptyText(StylePanel, "ccPanel")
            MyStyleHilite = genericController.encodeEmptyText(StyleHilite, "ccPanelHilite")
            MyStyleShadow = genericController.encodeEmptyText(StyleShadow, "ccPanelShadow")
            MyWidth = genericController.encodeEmptyText(Width, "100%")
            MyPadding = genericController.encodeEmptyText(Padding, "5")
            MethodName = "main_GetPanelBottom"
            '
            main_GetPanelBottom = main_GetPanelBottom _
                & cr6 & "</span></td>" _
                & cr5 & "</tr>" _
                & cr4 & "</table>" _
                & cr3 & "</td>" _
                & cr3 & "<td width=""1"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""1"" ></td>" _
                & cr2 & "</tr>" _
                & cr2 & "<tr>" _
                & cr3 & "<td colspan=""3"" class=""" & MyStyleShadow & """><img alt=""space"" src=""/ccLib/images/spacer.gif"" height=""1"" width=""" & MyWidth & """ ></td>" _
                & cr2 & "</tr>" _
                & cr & "</table>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function main_GetPanelButtons(ByVal ButtonValueList As String, ByVal ButtonName As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelButtons")
            '
            'If Not (true) Then Exit Function
            '
            Dim iButtonValueList As String
            Dim iButtonName As String
            Dim MethodName As String
            Dim Adminui As New adminUIController(Me)
            '
            iButtonValueList = genericController.encodeText(ButtonValueList)
            iButtonName = genericController.encodeText(ButtonName)
            '
            MethodName = "main_GetPanelButtons()"
            '
            main_GetPanelButtons = Adminui.GetButtonBar(Adminui.GetButtonsFromList(iButtonValueList, True, True, iButtonName), "")
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '
        '
        Public Function main_GetPanelRev(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelRev")
            '
            'If Not (true) Then Exit Function
            '
            main_GetPanelRev = main_GetPanel(PanelContent, "ccPanel", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelRev")
        End Function
        '
        '
        '
        Public Function main_GetPanelInput(ByVal PanelContent As String, Optional ByVal PanelWidth As String = "", Optional ByVal PanelHeightMin As String = "1") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPanelInput")
            '
            'If Not (true) Then Exit Function
            '
            main_GetPanelInput = main_GetPanel(PanelContent, "ccPanelInput", "ccPanelShadow", "ccPanelHilite", PanelWidth, 2, genericController.EncodeInteger(PanelHeightMin))
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetPanelInput")
        End Function
        '
        '========================================================================
        ' Print the tools panel at the bottom of the page
        '========================================================================
        '
        Public Function main_GetToolsPanel() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetToolsPanel")
            '
            'If Not (true) Then Exit Function
            '
            Dim copyNameValue As String
            Dim CopyName As String
            Dim copyValue As String
            Dim copyNameValueSplit() As String
            Dim VisitMin As Integer
            Dim VisitHrs As Integer
            Dim VisitSec As Integer
            Dim DebugPanel As String
            Dim Copy As String
            Dim CopySplit() As String
            Dim Ptr As Integer
            Dim EditTagID As String
            Dim QuickEditTagID As String
            Dim AdvancedEditTagID As String
            Dim WorkflowTagID As String
            Dim Tag As String
            Dim PathID As Integer
            Dim CS As Integer
            Dim PathsContentID As Integer
            Dim MethodName As String
            Dim TagID As String
            Dim ButtonPanel As String
            Dim ToolsPanel As stringBuilderLegacyController
            Dim OptionsPanel As String
            Dim LinkPanel As stringBuilderLegacyController
            Dim LoginPanel As String
            Dim iValueBoolean As Boolean
            Dim WorkingQueryString As String
            Dim ActionURL As String
            Dim BubbleCopy As String
            Dim AnotherPanel As stringBuilderLegacyController
            Dim ClipBoard As String
            Dim RenderTimeString As String
            Dim Adminui As New adminUIController(Me)
            Dim ToolsPanelAddonID As Integer
            Dim ShowLegacyToolsPanel As Boolean
            Dim QS As String
            '
            MethodName = "main_GetToolsPanel"
            '
            If authContext.user.AllowToolsPanel Then
                ShowLegacyToolsPanel = siteProperties.getBoolean("AllowLegacyToolsPanel", True)
                '
                ' --- Link Panel - used for both Legacy Tools Panel, and without it
                '
                LinkPanel = New stringBuilderLegacyController
                LinkPanel.Add(SpanClassAdminSmall)
                LinkPanel.Add("Contensive " & codeVersion() & " | ")
                LinkPanel.Add(FormatDateTime(app_startTime) & " | ")
                LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http://support.Contensive.com/"">Support</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL) & """>Admin Home</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML("http://" & webServer.webServerIO_requestDomain) & """>Public Home</A> | ")
                LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                If siteProperties.getBoolean("AllowMobileTemplates", False) Then
                    If authContext.visit.Mobile Then
                        QS = web_RefreshQueryString
                        QS = genericController.ModifyQueryString(QS, "method", "forcenonmobile")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Non-Mobile Version</A> | ")
                    Else
                        QS = web_RefreshQueryString
                        QS = genericController.ModifyQueryString(QS, "method", "forcemobile")
                        LinkPanel.Add("<a class=""ccAdminLink"" href=""?" & QS & """>Mobile Version</A> | ")
                    End If
                End If
                LinkPanel.Add("</span>")
                '
                If ShowLegacyToolsPanel Then
                    ToolsPanel = New stringBuilderLegacyController
                    WorkingQueryString = genericController.ModifyQueryString(web_RefreshQueryString, "ma", "", False)
                    '
                    ' ----- Tools Panel Caption
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink("2", "Contensive Tools Panel", BubbleCopy)
                    BubbleCopy = "Use the Tools Panel to enable features such as editing and debugging tools. It also includes links to the admin site, the support site and the My Profile page."
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanelHeader("Contensive Tools Panel" & helpLink)
                    '
                    ToolsPanel.Add(htmlDoc.html_GetFormStart(WorkingQueryString))
                    ToolsPanel.Add(htmlDoc.html_GetFormInputHidden("Type", FormTypeToolsPanel))
                    '
                    If True Then
                        '
                        ' ----- Create the Options Panel
                        '
                        'PathsContentID = main_GetContentID("Paths")
                        '                '
                        '                ' Allow Help Links
                        '                '
                        '                iValueBoolean = visitProperty.getboolean("AllowHelpIcon")
                        '                TagID =  "AllowHelpIcon"
                        '                OptionsPanel = OptionsPanel & "" _
                        '                    & CR & "<div class=""ccAdminSmall"">" _
                        '                    & cr2 & "<LABEL for=""" & TagID & """>" & main_GetFormInputCheckBox2(TagID, iValueBoolean, TagID) & "&nbsp;Help</LABEL>" _
                        '                    & CR & "</div>"
                        '
                        EditTagID = "AllowEditing"
                        QuickEditTagID = "AllowQuickEditor"
                        AdvancedEditTagID = "AllowAdvancedEditor"
                        WorkflowTagID = "AllowWorkflowRendering"
                        '
                        ' Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(7, "Enable Editing", "Display the edit tools for basic content, such as pages, copy and sections. ")
                        iValueBoolean = visitProperty.getBoolean("AllowEditing")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(EditTagID, iValueBoolean, EditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & EditTagID & """>" & Tag & "&nbsp;Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Quick Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(8, "Enable Quick Edit", "Display the quick editor to edit the main page content.")
                        iValueBoolean = visitProperty.getBoolean("AllowQuickEditor")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(QuickEditTagID, iValueBoolean, QuickEditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & EditTagID & "').checked=false;document.getElementById('" & AdvancedEditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & QuickEditTagID & """>" & Tag & "&nbsp;Quick Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Advanced Edit
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(0, "Enable Advanced Edit", "Display the edit tools for advanced content, such as templates and add-ons. Basic content edit tools are also displayed.")
                        iValueBoolean = visitProperty.getBoolean("AllowAdvancedEditor")
                        Tag = htmlDoc.html_GetFormInputCheckBox2(AdvancedEditTagID, iValueBoolean, AdvancedEditTagID)
                        Tag = genericController.vbReplace(Tag, ">", " onClick=""document.getElementById('" & QuickEditTagID & "').checked=false;document.getElementById('" & EditTagID & "').checked=false;"">")
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & AdvancedEditTagID & """>" & Tag & "&nbsp;Advanced Edit</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Workflow Authoring Render Mode
                        '
                        helpLink = ""
                        'helpLink = main_GetHelpLink(9, "Enable Workflow Rendering", "Control the display of workflow rendering. With workflow rendering enabled, any changes saved to content records that have not been published will be visible for your review.")
                        If siteProperties.allowWorkflowAuthoring Then
                            iValueBoolean = visitProperty.getBoolean("AllowWorkflowRendering")
                            Tag = htmlDoc.html_GetFormInputCheckBox2(WorkflowTagID, iValueBoolean, WorkflowTagID)
                            OptionsPanel = OptionsPanel _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & WorkflowTagID & """>" & Tag & "&nbsp;Render Workflow Authoring Changes</LABEL>" & helpLink _
                                & cr & "</div>"
                        End If
                        helpLink = ""
                        iValueBoolean = visitProperty.getBoolean("AllowDebugging")
                        TagID = "AllowDebugging"
                        Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, iValueBoolean, TagID)
                        OptionsPanel = OptionsPanel _
                            & cr & "<div class=""ccAdminSmall"">" _
                            & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "&nbsp;Debug</LABEL>" & helpLink _
                            & cr & "</div>"
                        '
                        ' Create Path Block Row
                        '
                        If authContext.isAuthenticatedDeveloper(Me) Then
                            TagID = "CreatePathBlock"
                            If siteProperties.allowPathBlocking Then
                                '
                                ' Path blocking allowed
                                '
                                'OptionsPanel = OptionsPanel & SpanClassAdminSmall & "<LABEL for=""" & TagID & """>"
                                CS = db.cs_open("Paths", "name=" & db.encodeSQLText(webServer.webServerIO_requestPath), , , , , , "ID")
                                If db.cs_ok(CS) Then
                                    PathID = (db.cs_getInteger(CS, "ID"))
                                End If
                                Call db.cs_Close(CS)
                                If PathID <> 0 Then
                                    '
                                    ' Path is blocked
                                    '
                                    Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Path is blocked [" & webServer.webServerIO_requestPath & "] [<a href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?af=" & AdminFormEdit & "&id=" & PathID & "&cid=" & main_GetContentID("paths") & "&ad=1") & """ target=""_blank"">edit</a>]</LABEL>"
                                Else
                                    '
                                    ' Path is not blocked
                                    '
                                    Tag = htmlDoc.html_GetFormInputCheckBox2(TagID, False, TagID) & "&nbsp;Block this path [" & webServer.webServerIO_requestPath & "]</LABEL>"
                                End If
                                helpLink = ""
                                'helpLink = main_GetHelpLink(10, "Enable Debugging", "Debugging is a developer only debugging tool. With Debugging enabled, ccLib.TestPoints(...) will print, ErrorTrapping will be displayed, redirections are blocked, and more.")
                                OptionsPanel = OptionsPanel _
                                    & cr & "<div class=""ccAdminSmall"">" _
                                    & cr2 & "<LABEL for=""" & TagID & """>" & Tag & "</LABEL>" & helpLink _
                                    & cr & "</div>"
                            End If
                        End If
                        '
                        ' Buttons
                        '
                        OptionsPanel = OptionsPanel & "" _
                            & cr & "<div class=""ccButtonCon"">" _
                            & cr2 & "<input type=submit name=" & "mb value=""" & ButtonApply & """>" _
                            & cr & "</div>" _
                            & ""
                    End If
                    '
                    ' ----- Create the Login Panel
                    '
                    If Trim(authContext.user.Name) = "" Then
                        Copy = "You are logged in as member #" & authContext.user.ID & "."
                    Else
                        Copy = "You are logged in as " & authContext.user.Name & "."
                    End If
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & Copy & "" _
                        & cr & "</div>"
                    '
                    ' Username
                    '
                    Dim Caption As String
                    If siteProperties.getBoolean("allowEmailLogin", False) Then
                        Caption = "Username&nbsp;or&nbsp;Email"
                    Else
                        Caption = "Username"
                    End If
                    TagID = "Username"
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputText2(TagID, "", 1, 30, TagID, False) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                    '
                    ' Username
                    '
                    If siteProperties.getBoolean("allownopasswordLogin", False) Then
                        Caption = "Password&nbsp;(optional)"
                    Else
                        Caption = "Password"
                    End If
                    TagID = "Password"
                    LoginPanel = LoginPanel & "" _
                        & cr & "<div class=""ccAdminSmall"">" _
                        & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputText2(TagID, "", 1, 30, TagID, True) & "&nbsp;" & Caption & "</LABEL>" _
                        & cr & "</div>"
                    '
                    ' Autologin checkbox
                    '
                    If siteProperties.getBoolean("AllowAutoLogin", False) Then
                        If authContext.visit.CookieSupport Then
                            TagID = "autologin"
                            LoginPanel = LoginPanel & "" _
                                & cr & "<div class=""ccAdminSmall"">" _
                                & cr2 & "<LABEL for=""" & TagID & """>" & htmlDoc.html_GetFormInputCheckBox2(TagID, True, TagID) & "&nbsp;Login automatically from this computer</LABEL>" _
                                & cr & "</div>"
                        End If
                    End If
                    '
                    ' Buttons
                    '
                    LoginPanel = LoginPanel & Adminui.GetButtonBar(Adminui.GetButtonsFromList(ButtonLogin & "," & ButtonLogout, True, True, "mb"), "")
                    '
                    ' ----- assemble tools panel
                    '
                    Copy = "" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.kmaIndent(LoginPanel) _
                        & cr & "</td>" _
                        & cr & "<td width=""50%"" class=""ccPanelInput"" style=""vertical-align:bottom;"">" _
                        & genericController.kmaIndent(OptionsPanel) _
                        & cr & "</td>"
                    Copy = "" _
                        & cr & "<tr>" _
                        & genericController.kmaIndent(Copy) _
                        & cr & "</tr>" _
                        & ""
                    Copy = "" _
                        & cr & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">" _
                        & genericController.kmaIndent(Copy) _
                        & cr & "</table>"
                    ToolsPanel.Add(main_GetPanelInput(Copy))
                    ToolsPanel.Add(htmlDoc.html_GetFormEnd)
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanel(ToolsPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    '
                    main_GetToolsPanel = main_GetToolsPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    '
                    LinkPanel = Nothing
                    ToolsPanel = Nothing
                    AnotherPanel = Nothing
                End If
                '
                ' --- Developer Debug Panel
                '
                If visitProperty.getBoolean("AllowDebugging") Then
                    '
                    ' --- Debug Panel Header
                    '
                    LinkPanel = New stringBuilderLegacyController
                    LinkPanel.Add(SpanClassAdminSmall)
                    'LinkPanel.Add( "WebClient " & main_WebClientVersion & " | "
                    LinkPanel.Add("Contensive " & codeVersion() & " | ")
                    LinkPanel.Add(FormatDateTime(app_startTime) & " | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""http: //support.Contensive.com/"">Support</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL) & """>Admin Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" href=""" & htmlDoc.html_EncodeHTML("http://" & webServer.webServerIO_requestDomain) & """>Public Home</A> | ")
                    LinkPanel.Add("<a class=""ccAdminLink"" target=""_blank"" href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?" & RequestNameHardCodedPage & "=" & HardCodedPageMyProfile) & """>My Profile</A> | ")
                    LinkPanel.Add("</span>")
                    '
                    '
                    '
                    'DebugPanel = DebugPanel & main_GetPanel(LinkPanel.Text, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", "5")
                    '
                    DebugPanel = DebugPanel _
                        & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                        & cr2 & "<tr>" _
                        & cr3 & "<td width=""100"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100"" height=""1"" ></td>" _
                        & cr3 & "<td width=""100%"" class=""ccPanel""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td>" _
                        & cr2 & "</tr>"
                    '
                    DebugPanel = DebugPanel & main_DebugPanelRow("DOM", "<a class=""ccAdminLink"" href=""/ccLib/clientside/DOMViewer.htm"" target=""_blank"">Click</A>")
                    DebugPanel = DebugPanel & main_DebugPanelRow("Trap Errors", htmlDoc.html_EncodeHTML(siteProperties.trapErrors.ToString))
                    DebugPanel = DebugPanel & main_DebugPanelRow("Trap Email", htmlDoc.html_EncodeHTML(siteProperties.getText("TrapEmail")))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerLink", htmlDoc.html_EncodeHTML(webServer.webServerIO_ServerLink))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerDomain", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestDomain))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerProtocol", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestProtocol))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerHost", htmlDoc.html_EncodeHTML(webServer.requestDomain))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPath", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestPath))
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerPage", htmlDoc.html_EncodeHTML(webServer.webServerIO_requestPage))
                    Copy = ""
                    If webServer.requestQueryString <> "" Then
                        CopySplit = Split(webServer.requestQueryString, "&")
                        For Ptr = 0 To UBound(CopySplit)
                            copyNameValue = CopySplit(Ptr)
                            If copyNameValue <> "" Then
                                copyNameValueSplit = Split(copyNameValue, "=")
                                CopyName = genericController.DecodeResponseVariable(copyNameValueSplit(0))
                                copyValue = ""
                                If UBound(copyNameValueSplit) > 0 Then
                                    copyValue = genericController.DecodeResponseVariable(copyNameValueSplit(1))
                                End If
                                Copy = Copy & cr & "<br>" & htmlDoc.html_EncodeHTML(CopyName & "=" & copyValue)
                            End If
                        Next
                        Copy = Mid(Copy, 8)
                    End If
                    DebugPanel = DebugPanel & main_DebugPanelRow("main_ServerQueryString", Copy)
                    Copy = ""
                    For Each key As String In docProperties.getKeyList()
                        Dim docProperty As docPropertiesClass = docProperties.getProperty(key)
                        If docProperty.IsForm Then
                            Copy = Copy & cr & "<br>" & htmlDoc.html_EncodeHTML(docProperty.NameValue)
                        End If
                    Next
                    'DebugPanel = DebugPanel & main_DebugPanelRow("ServerForm", Copy)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Request Path", html.html_EncodeHTML(web_requestPath))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("CDN Files Path", html.html_EncodeHTML(serverconfig.appConfig.cdnFilesNetprefix))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Referrer", html.html_EncodeHTML(web.requestReferrer))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Cookies", html.html_EncodeHTML(web.requestCookieString))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Id", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("visits") & "&af=4&id=" & main_VisitId & """>" & main_VisitId & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Start Date", genericController.encodeText(main_VisitStartDateValue))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Start Time", genericController.encodeText(main_VisitStartTime))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Last Time", genericController.encodeText(main_VisitLastTime))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Cookies Supported", genericController.encodeText(main_VisitCookieSupport))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Pages", genericController.encodeText(main_VisitPages))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visitor ID", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("visitors") & "&af=4&id=" & main_VisitorID & """>" & main_VisitorID & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Visitor New", genericController.encodeText(main_VisitorNew))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member ID", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("people") & "&af=4&id=" & authcontext.user.userId & """>" & authcontext.user.userId & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member Name", html.html_EncodeHTML(authContext.user.userName))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member New", genericController.encodeText(authContext.user.userIsNew))
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Member Language", authcontext.user.userLanguage)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Page", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("page content") & "&af=4&id=" & currentPageID & """>" & currentPageID & ", " & currentPageName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Section", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("site sections") & "&af=4&id=" & currentSectionID & """>" & currentSectionID & ", " & currentSectionName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Template", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("page templates") & "&af=4&id=" & currentTemplateID & """>" & currentTemplateID & ", " & currentTemplateName & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Domain", "<a href=""" & siteProperties.adminURL & "?cid=" & main_GetContentID("domains") & "&af=4&id=" & domains.domainDetails.id & """>" & domains.domainDetails.id & ", " & main_ServerDomain & "</a>")
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Template Reason", pageManager_TemplateReason)
                    'DebugPanel = DebugPanel & main_DebugPanelRow("ProcessID", GetProcessID.ToString())
                    'DebugPanel = DebugPanel & main_DebugPanelRow("Krnl ProcessID", genericController.encodeText(csv_HostServiceProcessID))
                    DebugPanel = DebugPanel & main_DebugPanelRow("Render Time &gt;= ", Format((GetTickCount - app_startTickCount) / 1000, "0.000") & " sec")
                    If True Then
                        VisitHrs = CInt(authContext.visit.TimeToLastHit / 3600)
                        VisitMin = CInt(authContext.visit.TimeToLastHit / 60) - (60 * VisitHrs)
                        VisitSec = authContext.visit.TimeToLastHit Mod 60
                        DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(authContext.visit.TimeToLastHit) & " sec, (" & VisitHrs & " hrs " & VisitMin & " mins " & VisitSec & " secs)")
                        'DebugPanel = DebugPanel & main_DebugPanelRow("Visit Length", CStr(main_VisitTimeToLastHit) & " sec, (" & Int(main_VisitTimeToLastHit / 60) & " min " & (main_VisitTimeToLastHit Mod 60) & " sec)")
                    End If
                    DebugPanel = DebugPanel & main_DebugPanelRow("Addon Profile", "<hr><ul class=""ccPanel"">" & "<li>tbd</li>" & cr & "</ul>")
                    '
                    DebugPanel = DebugPanel & "</table>"
                    '
                    If ShowLegacyToolsPanel Then
                        '
                        ' Debug Panel as part of legacy tools panel
                        '
                        main_GetToolsPanel = main_GetToolsPanel _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    Else
                        '
                        ' Debug Panel without Legacy Tools panel
                        '
                        main_GetToolsPanel = main_GetToolsPanel _
                            & main_GetPanelHeader("Debug Panel") _
                            & main_GetPanel(LinkPanel.Text) _
                            & main_GetPanel(DebugPanel, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 5)
                    End If
                End If
                main_GetToolsPanel = cr & "<div class=""ccCon"">" & genericController.kmaIndent(main_GetToolsPanel) & cr & "</div>"
            End If
            '
            Exit Function
            '
ErrorTrap:
            LinkPanel = Nothing
            ToolsPanel = Nothing
            AnotherPanel = Nothing
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        End Function
        '
        '
        '
        Private Function main_DebugPanelRow(Label As String, Value As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DebugPanelRow")
            '
            'If Not (true) Then Exit Function
            '
            main_DebugPanelRow = cr2 & "<tr><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Label & "</td><td valign=""top"" class=""ccPanel ccAdminSmall"">" & Value & "</td></tr>"
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_DebugPanelRow")
        End Function
        '        '
        '        '========================================================================
        '        '   Content Watch
        '        '
        '        '   Creates or updates a record in the content watch content. Content Watch
        '        '   contains a record that links
        '        '   Update link entry for content watch record for this content record
        '        '========================================================================
        '        '
        '        Public Sub main_TrackContent(ByVal ContentName As String, ByVal RecordID As Integer)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("TrackContent")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim CSPointer As Integer
        '            Dim MethodName As String
        '            Dim iContentName As String
        '            Dim iRecordID As Integer
        '            '
        '            If Not main_GetStreamBoolean2(RequestNameBlockContentTracking) Then
        '                iContentName = genericController.encodeText(ContentName)
        '                iRecordID = genericController.EncodeInteger(RecordID)
        '                '
        '                MethodName = "main_TrackContent"
        '                '
        '                CSPointer = main_OpenCSContentRecord2(iContentName, iRecordID)
        '                If Not app.IsCSOK(CSPointer) Then
        '                    throw New ApplicationException("Unexpected exception") ' handleLegacyError14(MethodName, "main_TrackContent, Error opening ContentSet from Content/Record [" & iContentName & "/" & genericController.encodeText(iRecordID) & "].")
        '                Else
        '                    Call main_TrackContentSet(CSPointer)
        '                End If
        '            End If
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Print a content blocks headline
        '        '   note - this call includes encoding
        '        '========================================================================
        '        '
        '        Public Function main_GetTitle(ByVal Title As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetTitle")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim iTitle As String
        '            '
        '            iTitle = genericController.encodeText(Title)
        '            If iTitle <> "" Then
        '                main_GetTitle = "<p>" & AddSpan(iTitle, "ccHeadline") & "</p>"
        '            End If
        '            Exit Function
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18("main_GetTitle")
        '        End Function
        ''
        ''========================================================================
        ''   Print the login form in an intercept page
        ''========================================================================
        ''
        'Public Function main_GetLoginPage() As String
        '    main_GetLoginPage = user_GetLoginPage2(False)
        'End Function
        '        '
        '        '========================================================================
        '        ' ----- main_GetJoinForm()
        '        '   Prints the Registration Form
        '        '   If you are already a member, it takes you to the member profile form
        '        '   If the site does not allow open joining, this is blocked.
        '        '========================================================================
        '        '
        '        Public Function main_GetJoinForm() As String
        '            Dim returnHtml As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetJoinForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim CS As Integer
        '            Dim CSPeople As Integer
        '            Dim CSUsernameCheck As Integer
        '            Dim JoinButton As String
        '            Dim MethodName As String
        '            Dim Copy As String
        '            Dim GroupIDList As String
        '            Dim FirstName As String
        '            '
        '            MethodName = "main_GetJoinForm"
        '            '
        '            CSPeople = app.csOpen("people", "ID=" & authcontext.user.userid)
        '            If Not app.csOk(CSPeople) Then
        '                '
        '                ' ----- could not open people, can not continue
        '                '
        '                throw New ApplicationException("Unexpected exception") ' handleLegacyError14(MethodName, "main_GetJoinForm, could not open the guest identity")
        '            Else
        '                If True Then
        '                    If authcontext.user.user_isRecognized() And Not authcontext.user.user_isAuthenticated() Then
        '                        '
        '                        ' ----- Recognized but Not authenticated
        '                        '
        '                        returnHtml = returnHtml & main_GetLoginForm()
        '                    Else
        '                        '
        '                        ' ----- Not authenticated, Guest identity, ask for information.
        '                        '
        '                        Dim QS As String
        '                        FirstName = db.cs_getText(CSPeople, "firstName")
        '                        If genericController.vbLCase(FirstName) = "guest" Then
        '                            FirstName = ""
        '                        End If
        '                        QS = web_RefreshQueryString
        '                        QS = genericController.ModifyQueryString(QS, "S", "")
        '                        QS = genericController.ModifyQueryString(QS, "ccIPage", "")
        '                        returnHtml = returnHtml & main_GetFormStart(QS)
        '                        returnHtml = returnHtml & htmldoc.html_GetFormInputHidden("Type", FormTypeJoin)
        '                        returnHtml = returnHtml & "<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""100%"">"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "First Name</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "firstname"" VALUE=""" & html.html_EncodeHTML(FirstName) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Last Name</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "lastname"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "lastname")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Email</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "email"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "email")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Username</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input NAME=""" & "username"" VALUE=""" & html.html_EncodeHTML(db.cs_getText(CSPeople, "username")) & """ SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr>"
        '                        returnHtml = returnHtml & "<td align=""right"" width=""30%"">" & SpanClassAdminNormal & "Password</span></td>"
        '                        returnHtml = returnHtml & "<td align=""left""  width=""70%""><input type=password NAME=""" & "password"" SIZE=""20"" MAXLENGTH=""50""></td>"
        '                        returnHtml = returnHtml & "</tr>"
        '                        returnHtml = returnHtml & "<tr><td colspan=""2"">" & main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>"
        '                        returnHtml = returnHtml & "</table>"
        '                        returnHtml = returnHtml & "</form>"
        '                    End If
        '                End If
        '            End If
        '            '
        '            main_GetJoinForm = returnHtml
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_GetMyProfileForm()
        '        '
        '        '   Anyone can have access to this form, if they are authenticated.
        '        '   To give a guest access, assign then a username and password and authenticated them.
        '        '========================================================================
        '        '
        '        Public Function main_GetMyProfileForm(PeopleID As Integer) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Const TableOpen = vbCrLf & "<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%""><tr><td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1""></td><td><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1""></td></tr>"
        '            Const TableClose = vbCrLf & "</table>"
        '            '
        '            Dim CSMember As Integer
        '            Dim MethodName As String
        '            Dim iPeopleID As Integer
        '            Dim CSLastVisit As Integer
        '            Dim RowDivider As String
        '            Dim RowCount As Integer
        '            Dim Stream As New coreFastStringClass
        '            Dim ButtonPanel As String
        '            Dim ButtonList As String
        '            Dim ContentName As String
        '            Dim ContentID As Integer
        '            Dim s As coreFastStringClass
        '            '
        '            iPeopleID = genericController.EncodeInteger(PeopleID)
        '            '
        '            MethodName = "main_GetMyProfileForm"
        '            '
        '            If Not authcontext.user.user_isAuthenticated() Then
        '                Call error_AddUserError("You can not edit your MyAccount page until you have logged in.")
        '            Else
        '                CSMember = main_OpenContent("People", "ID=" & app.EncodeSQLNumber(iPeopleID))
        '                If app.csOk(CSMember) Then
        '                    '
        '                    ContentID = app.cs_getInteger(CSMember, "ContentControlID")
        '                    ContentName = metaData.getContentNameByID(ContentID)
        '                    '
        '                    ' ----- member personal information
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Name", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "FirstName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LastName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Email", "The internet email address where you can be contacted. This address is used to confirm your username and paStreamword.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Company", "Your employer", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "title", "Your job title", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address2", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "City", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "State", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Zip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Country", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Phone", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Fax", "", RowCount) & vbCrLf)
        '                    's.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "imagefilename", "", RowCount) & vbcrlf )
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ResumeFilename", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Contact", s.Text)
        '                    '
        '                    ' ----- billing
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillEmail", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCompany", "Your employer", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillAddress", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCity", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillState", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCountry", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillPhone", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillFax", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Billing", s.Text)
        '                    '
        '                    ' ----- Shipping Information
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipName", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCompany", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipAddress", "Your street address", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCity", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipState", "Your state or provence", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCountry", "", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipPhone", "", RowCount) & vbCrLf)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Shipping", s.Text)
        '                    '
        '                    ' ----- Site Preferences
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "username", "Used with your password to gain access to the site.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Password", "Use with your username to gain access to the site.", RowCount) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowbulkemail", "If checked, we may send you updates about our site from time to time.", RowCount) & vbCrLf)
        '                    ' 6/18/2009 - removed notes from base
        '                    '            s.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "sendnotes", "If checked, notes sent to you as a site member will be emailed. Otherwise, they are available only when you have logged on.", RowCount) & vbCrLf
        '                    s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LanguageID", "select your prefered language. If content is aviable in your language, is will be displayed. Otherwise, the default language will be used.", RowCount) & vbCrLf)
        '                    If genericController.EncodeBoolean(app.siteProperty_getBoolean("AllowAutoLogin", False)) Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "autologin", "This site allows automatic login. If this box is check, you will enable this function for your member account.", RowCount) & vbCrLf)
        '                    End If
        '                    If authcontext.user.main_IsContentManager() Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowtoolspanel", "If checked, a tools panel appears at the bottom of every active page with acceStream to key administrative functions.", RowCount) & vbCrLf)
        '                    End If
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Preferences", s.Text)
        '                    '
        '                    ' ----- Interest Topics
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Selected Topics</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Topics(iPeopleID, ContentName) & vbCrLf)
        '                    s.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topics Habits</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_TopicHabits)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Topics", s.Text)
        '                    '
        '                    ' ----- Group main_MemberShip
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_Groups)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Groups", s.Text)
        '                    '
        '                    ' ----- Records
        '                    '
        '                    s = New coreFastStringClass
        '                    s.Add(TableOpen)
        '                    s.Add(main_GetMyProfileForm_Row("Member Number", genericController.encodeText(authContext.user.userid)) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Row("Visitor Number", genericController.encodeText(main_VisitorID)) & vbCrLf)
        '                    s.Add(main_GetMyProfileForm_Row("Visit Number", genericController.encodeText(main_VisitId)) & vbCrLf)
        '                    CSLastVisit = app.csOpen("Visits", "MemberID=" & iPeopleID, "ID DESC")
        '                    If app.csOk(CSLastVisit) Then
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "StartTime", "", RowCount, "Start Time", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "REMOTE_ADDR", "", RowCount, "IP Address", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "Browser", "", RowCount, "", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "HTTP_REFERER", "", RowCount, "Referer", True) & vbCrLf)
        '                        s.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "CookieSupport", "", RowCount, "Cookie Support", True) & vbCrLf)
        '                    End If
        '                    Call app.csClose(CSLastVisit)
        '                    s.Add(TableClose)
        '                    Call main_AddLiveTabEntry("Statistics", s.Text)
        '                    '
        '                    ' ----- save button
        '                    '
        '                    RowCount = 0
        '                    Stream.Add(main_GetFormStart)
        '                    ButtonList = ButtonSave & "," & ButtonCancel
        '                    ButtonPanel = main_GetPanelButtons(ButtonList, "Button")
        '                    Stream.Add(ButtonPanel)
        '                    Stream.Add(html_GetFormInputHidden("Type", FormTypeMyProfile))
        '                    Stream.Add(error_GetUserError())
        '                    Stream.Add("<div>&nbsp;</div>")
        '                    Stream.Add(main_GetLiveTabs())
        '                    Stream.Add(ButtonPanel)
        '                    '
        '                    main_GetMyProfileForm = Stream.Text & "</form>"
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get_old_MyProfileForm()
        '        '
        '        '   Anyone can have access to this form, if they are authenticated.
        '        '   To give a guest access, assign then a username and password and authenticated them.
        '        '========================================================================
        '        '
        '        Public Function main_Get_old_MyProfileForm(PeopleID As Integer) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Get_old_MyProfileForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim CSMember As Integer
        '            Dim MethodName As String
        '            Dim iPeopleID As Integer
        '            Dim CSLastVisit As Integer
        '            Dim RowDivider As String
        '            Dim RowCount As Integer
        '            Dim Stream As New coreFastStringClass
        '            Dim ButtonPanel As String
        '            Dim ButtonList As String
        '            Dim ContentName As String
        '            Dim ContentID As Integer
        '            Dim tabContent(5) As String
        '            '
        '            iPeopleID = genericController.EncodeInteger(PeopleID)
        '            '
        '            MethodName = "main_Get_old_MyProfileForm"
        '            '
        '            If Not authcontext.user.user_isAuthenticated() Then
        '                Call error_AddUserError("You can not edit your MyAccount page until you have logged in.")
        '            Else
        '                CSMember = main_OpenContent("People", "ID=" & db.EncodeSQLNumber(iPeopleID))
        '                If db.csOk(CSMember) Then
        '                    ContentID = db.cs_getInteger(CSMember, "ContentControlID")
        '                    ContentName = metaData.getContentNameByID(ContentID)
        '                    '
        '                    RowDivider = "<tr><td width=""100%"" align=""left""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>" & vbCrLf
        '                    RowCount = 0
        '                    Stream.Add(main_GetFormStart)
        '                    ButtonList = ButtonSave & "," & ButtonCancel
        '                    ButtonPanel = main_GetPanelButtons(ButtonList, "Button")
        '                    Stream.Add(ButtonPanel)
        '                    Stream.Add(html_GetFormInputHidden("Type", FormTypeMyProfile))
        '                    Stream.Add(error_GetUserError())
        '                    Stream.Add("<table border=""0"" cellpadding=""3"" cellspacing=""0"" width=""100%"">")
        '                    Stream.Add("<tr><td width=""150""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""140"" height=""1"" ></td><td><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    '
        '                    ' ----- member personal information
        '                    '
        '                    Stream.Add("<tr>" & vbCrLf)
        '                    Stream.Add("<td colspan=""2"">" & SpanClassAdminNormal & "<b>Your Information</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td>" & vbCrLf)
        '                    Stream.Add("</tr>" & vbCrLf)
        '                    '
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Name", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "FirstName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LastName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Email", "The internet email address where you can be contacted. This address is used to confirm your username and paStreamword.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Company", "Your employer", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "title", "Your job title", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Address", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "City", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "State", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Zip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Country", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Phone", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Fax", "", RowCount) & vbCrLf)
        '                    'Stream.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "imagefilename", "", RowCount) & vbcrlf )
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ResumeFilename", "", RowCount) & vbCrLf)
        '                    '
        '                    Stream.Add("<tr>" & vbCrLf)
        '                    Stream.Add("<td colspan=""2"">" & SpanClassAdminNormal & "<b>Billing Information (for online commerce only)</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td>" & vbCrLf)
        '                    Stream.Add("</tr>" & vbCrLf)
        '                    '
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillEmail", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCompany", "Your employer", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillAddress", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCity", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillState", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillCountry", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillPhone", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "BillFax", "", RowCount) & vbCrLf)
        '                    '
        '                    ' ----- Shipping Information
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Shipping Information (for online commerce only)</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipName", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCompany", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipAddress", "Your street address", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCity", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipState", "Your state or provence", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipZip", "Your zipcode or postal code", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipCountry", "", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "ShipPhone", "", RowCount) & vbCrLf)
        '                    '
        '                    ' ----- Site Preferences
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Site Preferences</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "username", "Used with your password to gain access to the site.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "Password", "Use with your username to gain access to the site.", RowCount) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowbulkemail", "If checked, we may send you updates about our site from time to time.", RowCount) & vbCrLf)
        '                    ' 6/18/2009 - removed notes from base
        '                    '            Stream.Add( main_GetMyProfileForm_RowCS(CSMember, "People", "sendnotes", "If checked, notes sent to you as a site member will be emailed. Otherwise, they are available only when you have logged on.", RowCount) & vbcrlf )
        '                    Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "LanguageID", "", RowCount) & vbCrLf)
        '                    If siteProperties.getBoolean("AllowAutoLogin", False) Then
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "autologin", "This site allows automatic login. If this box is check, you will enable this function for your member account.", RowCount) & vbCrLf)
        '                    End If
        '                    If authcontext.user.main_IsContentManager() Then
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSMember, "People", "allowtoolspanel", "If checked, a tools panel appears at the bottom of every active page with acceStream to key administrative functions.", RowCount) & vbCrLf)
        '                    End If
        '                    '
        '                    ' ----- Interest Topics
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topics of Interest</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Topics(iPeopleID, ContentName) & vbCrLf)
        '                    '
        '                    ' ----- Topics Habits
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Topic Habits</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_TopicHabits)
        '                    '
        '                    ' ----- Group main_MemberShip
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Group main_MemberShip</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Groups)
        '                    '
        '                    ' ----- Records
        '                    '
        '                    Stream.Add("<tr><td colspan=""2"">" & SpanClassAdminNormal & "<b>Statistics</b></span><br ><img src=""/ccLib/images/black.gif"" width=""100%"" height=""1"" ></td></tr>" & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Member Number", genericController.encodeText(authContext.user.userId)) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Visitor Number", genericController.encodeText(main_VisitorID)) & vbCrLf)
        '                    Stream.Add(main_GetMyProfileForm_Row("Visit Number", genericController.encodeText(main_VisitId)) & vbCrLf)
        '                    CSLastVisit = db.csOpen("Visits", "MemberID=" & iPeopleID, "ID DESC")
        '                    If db.csOk(CSLastVisit) Then
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "main_VisitorID", "", RowCount) & vbcrlf )
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "ID", "", RowCount) & vbcrlf )
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "StartTime", "", RowCount, "Start Time", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "REMOTE_ADDR", "", RowCount, "IP Address", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "Browser", "", RowCount, "", True) & vbCrLf)
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "HTTP_REFERER", "", RowCount, "Referer", True) & vbCrLf)
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "RefererPathPage", "", RowCount) & vbcrlf )
        '                        Stream.Add(main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "CookieSupport", "", RowCount, "Cookie Support", True) & vbCrLf)
        '                        'Stream.Add( main_GetMyProfileForm_RowCS(CSLastVisit, "Visits", "main_VisitAuthenticated", "", RowCount) & vbcrlf )
        '                    End If
        '                    Call db.csClose(CSLastVisit)
        '                    '
        '                    Stream.Add("</table>")
        '                    '
        '                    ' ----- save button
        '                    '
        '                    Stream.Add(ButtonPanel)
        '                    '
        '                    main_Get_old_MyProfileForm = Stream.Text & "</form>"
        '                End If
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError18(MethodName)
        '            '
        '        End Function
        '        '
        '        ' -----
        '        '
        '        Private Function main_GetMyProfileForm_RowCS(ByVal CSPointer As Integer, ByVal ContentName As String, ByVal FieldName As String, ByVal Explaination As String, ByVal RowCount As Integer, Optional ByVal Caption As String = "", Optional ByVal readOnlyField As Boolean = False) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_RowCS")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim Stream As String
        '            Dim MethodName As String
        '            Dim iCaption As String
        '            '
        '            MethodName = "main_GetMyProfileForm_RowCS"
        '            '
        '            iCaption = Caption
        '            If Caption = "" Then
        '                iCaption = db.cs_getFieldCaption(CSPointer, FieldName)
        '            End If
        '            '
        '            If Not db.csOk(CSPointer) Then
        '                throw (New Exception("ContentSet argument is not valid"))
        '            Else
        '                If readOnlyField Then
        '                    Stream = db.cs_getText(CSPointer, FieldName)
        '                Else
        '                    Stream = html_GetFormInputCS(CSPointer, ContentName, FieldName, , 60)
        '                End If
        '                main_GetMyProfileForm_RowCS = main_GetMyProfileForm_Row(iCaption, Stream)
        '            End If
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13(MethodName)
        '        End Function
        '        '
        '        ' ----- main_GetMyProfileForm_RowCS()
        '        '
        '        Private Function main_GetMyProfileForm_Row(LeftSide As String, RightSide As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Row")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetMyProfileForm_Row = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""middle""><span class=""ccAdminSmall"">" & LeftSide & "</span></td>" _
        '                & "<td align=""left""  valign=""middle""><span class=""ccAdminSmall"">" & RightSide & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_GetMyProfileForm_Row")
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_Topics(PeopleID As Integer, ContentName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Topics")
        '            '
        '            Dim Stream As String
        '            '
        '            Stream = main_GetFormInputCheckList(rnMyProfileTopics, "people", PeopleID, "topics", "member topic rules", "memberid", "topicid")
        '            'Stream = main_GetFormInputTopics("Topic", "Topics", ContentName, PeopleID)
        '            '
        '            ' Empty case
        '            '
        '            If Stream = "" Then
        '                Stream = "There are currently no topics defined"
        '            End If
        '            '
        '            ' Set it in the output
        '            '
        '            main_GetMyProfileForm_Topics = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & Stream & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            throw New ApplicationException("Unexpected exception") ' handleLegacyError13("main_GetMyProfileForm_Topics")
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_TopicHabits() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_TopicHabits")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim SQL As String
        '            Dim CS As Integer
        '            Dim Stream As String
        '            Dim MethodName As String
        '            Dim TopicCount As Integer
        '            '
        '            MethodName = "main_GetMyProfileForm_TopicHabits"
        '            '
        '            ' ----- Gather all the topics to which this member belongs
        '            '
        '            SQL = "SELECT ccTopics.Name as Name, Sum( ccTopicHabits.Score ) as Score" _
        '                & " FROM ccTopics LEFT JOIN ccTopicHabits ON ccTopics.ID = ccTopicHabits.TopicID" _
        '                & " WHERE (((ccTopics.Active)<>0) AND ((ccTopicHabits.MemberID)=" & authcontext.user.userId & ")) OR (((ccTopics.Active)<>0) AND ((ccTopicHabits.MemberID) Is Null))" _
        '                & " Group By ccTopics.name" _
        '                & " Order by ccTopics.Name"
        '            CS = db.csOpenSql(SQL)
        '            Do While db.csOk(CS)
        '                Stream = Stream & SpanClassAdminNormal & db.cs_getText(CS, "name") & " = " & genericController.encodeText(db.cs_getInteger(CS, "score")) & "</span><BR >"
        '                Call db.csGoNext(CS)
        '                TopicCount = TopicCount + 1
        '            Loop
        '            Call db.csClose(CS)
        '            '
        '            '
        '            '
        '            If TopicCount = 0 Then
        '                Stream = "There are currently no topics defined"
        '            End If
        '            '
        '            ' ----- Set it in the output
        '            '
        '            main_GetMyProfileForm_TopicHabits = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & Stream & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13(MethodName)
        '        End Function
        '        '
        '        '========================================================================
        '        ' ----- main_Get a string with the topic groups and check the ones this member has selected
        '        '========================================================================
        '        '
        '        Private Function main_GetMyProfileForm_Groups() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetMyProfileForm_Groups")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim MethodName As String
        '            Dim PublicJoinCriteria As String
        '            Dim Stream As String
        '            Dim GroupList As String

        '            '
        '            MethodName = "main_GetMyProfileForm_Groups"
        '            '
        '            If Not authcontext.user.user_isAdmin() Then
        '                PublicJoinCriteria = "ccgroups.PublicJoin<>0"
        '            End If
        '            '
        '            GroupList = main_GetFormInputCheckList("MemberRules", "People", authcontext.user.userId, "Groups", "Member Rules", "MemberID", "GroupID", PublicJoinCriteria, "Caption")
        '            If GroupList = "" Then
        '                GroupList = "<div>There are no public groups</div>"
        '            End If
        '            main_GetMyProfileForm_Groups = "" _
        '                & "<tr>" _
        '                & "<td align=""right"" valign=""top"">" & SpanClassAdminSmall & "</span></td>" _
        '                & "<td valign=""top"">" & SpanClassAdminNormal & GroupList & "</span></td>" _
        '                & "</tr>"
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13(MethodName)
        '        End Function
        '
        '=============================================================================
        ' main_Get the GroupID from iGroupName
        '=============================================================================
        '
        Public Function group_GetGroupID(ByVal GroupName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetGroupID")
            '
            'If Not (true) Then Exit Function
            '
            Dim dt As DataTable
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_GetGroupID"
            '
            group_GetGroupID = 0
            If (iGroupName <> "") Then
                '
                ' ----- main_Get the Group ID
                '
                dt = db.executeSql("select top 1 id from ccGroups where name=" & db.encodeSQLText(iGroupName))
                If dt.Rows.Count > 0 Then
                    group_GetGroupID = genericController.EncodeInteger(dt.Rows(0).Item(0))
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        End Function
        '
        '=============================================================================
        ' main_Get the GroupName from iGroupID
        '=============================================================================
        '
        Public Function group_GetGroupName(GroupID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetGroupByID")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupID As Integer
            '
            iGroupID = genericController.EncodeInteger(GroupID)
            '
            MethodName = "main_GetGroupByID"
            '
            group_GetGroupName = ""
            If (iGroupID > 0) Then
                '
                ' ----- main_Get the Group name
                '
                CS = csOpenRecord("Groups", iGroupID)
                If db.cs_ok(CS) Then
                    group_GetGroupName = genericController.encodeText(cs_GetField(CS, "Name"))
                End If
                Call db.cs_Close(CS)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Function group_Add(ByVal GroupName As String, Optional ByVal GroupCaption As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddGroup")
            '
            'If Not (true) Then Exit Function
            '
            'dim dt as datatable
            Dim CS As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iGroupCaption As String
            '
            MethodName = "main_AddGroup"
            '
            iGroupName = genericController.encodeText(GroupName)
            iGroupCaption = genericController.encodeEmptyText(GroupCaption, iGroupName)
            '
            group_Add = -1
            Dim dt As DataTable
            dt = db.executeSql("SELECT ID FROM ccgroups WHERE NAME=" & db.encodeSQLText(iGroupName))
            If dt.Rows.Count > 0 Then
                group_Add = genericController.EncodeInteger(dt.Rows(0).Item(0))
            Else
                CS = db.cs_insertRecord("Groups", SystemMemberID)
                If db.cs_ok(CS) Then
                    group_Add = genericController.EncodeInteger(db.cs_getField(CS, "ID"))
                    Call db.cs_set(CS, "name", iGroupName)
                    Call db.cs_set(CS, "caption", iGroupCaption)
                    Call db.cs_set(CS, "active", True)
                End If
                Call db.cs_Close(CS)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        End Function

        '
        '=============================================================================
        ' Add a new group, return its GroupID
        '=============================================================================
        '
        Public Sub group_DeleteGroup(ByVal GroupName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteGroup")
            '
            'If Not (true) Then Exit Sub
            '
            Call db.deleteContentRecords("Groups", "name=" & db.encodeSQLText(GroupName))
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_DeleteGroup")
        End Sub
        '
        '=============================================================================
        ' Add a member to a group
        '=============================================================================
        '
        Public Sub group_AddGroupMember(ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID, Optional ByVal DateExpires As Date = Nothing)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AddGroupMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            Dim iDateExpires As Date
            '
            MethodName = "main_AddGroupMember"
            '
            iGroupName = genericController.encodeText(GroupName)
            iDateExpires = DateExpires 'encodeMissingDate(DateExpires, Date.MinValue)
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(iGroupName)
                If (GroupID < 1) Then
                    GroupID = group_Add(GroupName, GroupName)
                End If
                If (GroupID < 1) Then
                    Throw (New ApplicationException("main_AddGroupMember could not find or add Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                Else
                    CS = db.cs_open("Member Rules", "(MemberID=" & db.encodeSQLNumber(NewMemberID) & ")and(GroupID=" & db.encodeSQLNumber(GroupID) & ")", , False)
                    If Not db.cs_ok(CS) Then
                        Call db.cs_Close(CS)
                        CS = db.cs_insertRecord("Member Rules")
                    End If
                    If Not db.cs_ok(CS) Then
                        Throw (New ApplicationException("main_AddGroupMember could not add this member to the Group [" & GroupName & "]")) ' handleLegacyError14(MethodName, "")
                    Else
                        Call db.cs_set(CS, "active", True)
                        Call db.cs_set(CS, "memberid", NewMemberID)
                        Call db.cs_set(CS, "groupid", GroupID)
                        If iDateExpires <> Date.MinValue Then
                            Call db.cs_set(CS, "DateExpires", iDateExpires)
                        Else
                            Call db.cs_set(CS, "DateExpires", "")
                        End If
                    End If
                    Call db.cs_Close(CS)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '=============================================================================
        ' Delete a member from a group
        '=============================================================================
        '
        Public Sub group_DeleteGroupMember(ByVal GroupName As String, Optional ByVal NewMemberID As Integer = SystemMemberID)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteGroupMember")
            '
            'If Not (true) Then Exit Sub
            '
            Dim GroupID As Integer
            Dim MethodName As String
            Dim iGroupName As String
            '
            iGroupName = genericController.encodeText(GroupName)
            '
            MethodName = "main_DeleteGroupMember"
            '
            If iGroupName <> "" Then
                GroupID = group_GetGroupID(iGroupName)
                If (GroupID < 1) Then
                ElseIf (NewMemberID < 1) Then
                    Throw (New ApplicationException("Member ID is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    Call db.deleteContentRecords("Member Rules", "(MemberID=" & db.encodeSQLNumber(NewMemberID) & ")AND(groupid=" & db.encodeSQLNumber(GroupID) & ")")
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '        '
        '        '=============================================================================
        '        '   main_Get the Admin Form
        '        '=============================================================================
        '        '
        '        Public Function main_GetAdminForm(Optional ByVal Content As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetAdminForm")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim admin As New Contensive.Addons.adminClass()
        '            '
        '            ' main_GetClose page was removed from AdminClass so main_GetAdminPage can call it after main_GetHTMLHead (in main_GetAdminStart)
        '            '
        '            'Call AppendLog("call main_getEndOfBody, from main_getAdminForm,")
        '            main_GetAdminForm = "" _
        '                & Admin.execute_getContent(Content) _
        '                & main_GetEndOfBody(True, True, False, True)
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Admin = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetAdminForm")
        '        End Function
        ''
        ''=============================================================================
        ''   Legacy
        ''=============================================================================
        ''
        'Public Function main_GetAdminPage(Optional ByVal Content As String = "") As String
        '    main_GetAdminPage = addonToBe_admin(Content)
        'End Function
        '
        '=============================================================================
        ' Print the admin developer tools page
        '=============================================================================
        '
        Public Function tools_GetToolsForm() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetToolsForm")
            '
            '
            Dim Tools As New coreToolsClass(Me)
            tools_GetToolsForm = Tools.GetForm()
            Tools = Nothing
            Exit Function
            '
ErrorTrap:
            Tools = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("PrintToolsForm")
        End Function
        '
        '=============================================================================
        '   Return just the copy from a content page
        '=============================================================================
        '
        Public Sub content_SetContentCopy(ByVal CopyName As String, ByVal Content As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetContentCopy")
            '
            'If Not (true) Then Exit Sub
            '
            Dim CS As Integer
            Dim iCopyName As String
            Dim iContent As String
            'dim buildversion As String
            Const ContentName = "Copy Content"
            '
            '  BuildVersion = app.dataBuildVersion
            If False Then '.3.210" Then
                Throw (New Exception("Contensive database was created with version " & siteProperties.dataBuildVersion & ". main_SetContentCopy requires an builder."))
            Else
                iCopyName = genericController.encodeText(CopyName)
                iContent = genericController.encodeText(Content)
                CS = db.cs_open(ContentName, "name=" & EncodeSQLText(iCopyName))
                If Not db.cs_ok(CS) Then
                    Call db.cs_Close(CS)
                    CS = db.cs_insertRecord(ContentName)
                End If
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "name", iCopyName)
                    Call db.cs_set(CS, "Copy", iContent)
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Sub
            '
ErrorTrap:
            '    PageList = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentCopy")
        End Sub

        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   iRootPageName is the name of the position on the site, not the name of the
        '        '   content.
        '        '
        '        '   PageName is optional only if main_PreloadContentPage has been called, set to ""
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPage(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal OrderByClause As String = "", Optional ByVal AllowChildPageList As Boolean = True, Optional ByVal AllowReturnLink As Boolean = True, Optional ByVal Bid As Integer = 0) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPage")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim FieldRows As Integer
        '            Dim quickEditor As String
        '            Dim iRootPageName As String
        '            Dim rootPageId As Integer
        '            Dim pageId As Integer
        '            Dim iContentName As String
        '            Dim iOrderByClause As String
        '            Dim contentPage As String
        '            '
        '            ' ----- Type the input
        '            '
        '            '$$$$$ cache this - somewhere it opens cs with icontentname
        '            iContentName = genericController.encodeEmptyText(ContentName, "Page Content")
        '            iRootPageName = Trim(genericController.encodeText(RootPageName))
        '            If iRootPageName <> "" Then
        '                rootPageId = main_GetRecordID(iContentName, iRootPageName)
        '            End If
        '            iOrderByClause = genericController.encodeText(OrderByClause)
        '            If (Bid = 0) Then
        '                pageId = docProperties.getInteger("bid")
        '            Else
        '                pageId = Bid
        '            End If
        '            '
        '            ' ----- Test if the page has been preloaded
        '            '
        '            contentPage = pageManager_GetHtmlBody_GetSection_GetContent(pageId, rootPageId, iContentName, iOrderByClause, AllowChildPageList, AllowReturnLink, False, 0, siteProperties.useContentWatchLink, False)
        '            main_GetContentPage = main_GetEditWrapper(iContentName, contentPage)
        '            '
        '            ' ----- Redirect if required
        '            '       ##### to be moved directly into page list routines
        '            '
        '            If pageManager_RedirectLink <> "" Then
        '                '
        '                ' redirect
        '                '
        '                Call web_Redirect2(pageManager_RedirectLink, pageManager_RedirectReason, pageManager_RedirectBecausePageNotFound)
        '            ElseIf (InStr(1, main_GetContentPage, main_fpo_QuickEditing) <> 0) Then
        '                '
        '                ' quick editor
        '                '
        '                FieldRows = genericController.EncodeInteger(properties_user_getText(ContentName & ".copyFilename.PixelHeight", "500"))
        '                If FieldRows < 50 Then
        '                    FieldRows = 50
        '                    Call properties_SetMemberProperty(ContentName & ".copyFilename.PixelHeight", 50)
        '                End If
        '                quickEditor = html_GetFormInputHTML("copyFilename", main_QuickEditCopy)
        '                main_GetContentPage = genericController.vbReplace(main_GetContentPage, main_fpo_QuickEditing, quickEditor)
        '            End If
        '            Exit Function
        '            '
        'ErrorTrap:
        '            '    PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPage")
        '        End Function
        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   iRootPageName is the name of the position on the site, not the name of the
        '        '   content.
        '        '
        '        '   PageName is optional only if main_PreloadContentPage has been called, set to ""
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPageArchive(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal OrderByClause As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPageArchive")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            Dim iRootPageName As String
        '            Dim rootPageId As Integer
        '            Dim PageRecordID As Integer
        '            Dim iContentName As String
        '            '
        '            iRootPageName = Trim(genericController.encodeText(RootPageName))
        '            iContentName = genericController.encodeEmptyText(ContentName, "Page Content")
        '            PageRecordID = docProperties.getInteger("bid")
        '            If iRootPageName <> "" Then
        '                rootPageId = main_GetRecordID(iContentName, iRootPageName)
        '            End If
        '            '
        '            main_GetContentPageArchive = pageManager_GetHtmlBody_GetSection_GetContent(PageRecordID, rootPageId, iContentName, genericController.encodeText(OrderByClause), True, True, True, 0, siteProperties.useContentWatchLink, False)
        '            '
        '            If pageManager_RedirectLink <> "" Then
        '                Call web_Redirect2(pageManager_RedirectLink, "Redirecting due to a main_GetContentPageArchive condition. (" & pageManager_RedirectReason & ")", pageManager_RedirectBecausePageNotFound)
        '            End If
        '            '
        '            main_GetContentPageArchive = main_GetEditWrapper(iContentName & " Archive", main_GetContentPageArchive)
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            '   PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPageArchive")
        '        End Function
        '        '
        '        '=============================================================================
        '        '   Print content in a page format
        '        '
        '        '   RootPageNameLocal is the name of the position on the site, not the name of the
        '        '   content.
        '        '=============================================================================
        '        '
        '        Public Function main_GetContentPageMenu(ByVal RootPageName As String, Optional ByVal ContentName As String = "", Optional ByVal Link As String = "", Optional ByVal RootPageRecordID As Integer = 0, Optional ByVal DepthLimit As Integer = 0, Optional ByVal MenuStyle As String = "", Optional ByVal StyleSheetPrefix As String = "", Optional ByVal MenuImage As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPageMenu")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            'Dim PageList As PageListClass
        '            Dim RootPageNameLocal As String
        '            Dim ContentNameLocal As String
        '            Dim PageLink As String
        '            Dim ChildPageRecordID As Integer
        '            Dim MenuStyleLocal As Integer
        '            Dim StyleSheetPrefixLocal As String
        '            Dim IMenuImage As String
        '            Dim UseContentWatchLink As Boolean
        '            '
        '            RootPageNameLocal = Trim(genericController.encodeText(RootPageName))
        '            ContentNameLocal = genericController.encodeEmptyText(ContentName, "Page Content")
        '            PageLink = genericController.encodeEmptyText(Link, "")
        '            MenuStyleLocal = encodeEmptyInteger(MenuStyle, 1)
        '            StyleSheetPrefixLocal = genericController.encodeEmptyText(StyleSheetPrefix, "ccFlyout")
        '            IMenuImage = genericController.encodeEmptyText(MenuImage, "")
        '            UseContentWatchLink = siteProperties.useContentWatchLink
        '            '
        '            main_GetContentPageMenu = main_GetSectionMenu_NameMenu(RootPageNameLocal, ContentNameLocal, PageLink, RootPageRecordID, DepthLimit, MenuStyleLocal, StyleSheetPrefixLocal, IMenuImage, IMenuImage, "", 0, UseContentWatchLink)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            'Set PageList = Nothing
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPageMenu")
        '        End Function
        '        '
        '        '   2.0 compatibility
        '        '
        '        Public Function main_OpenContent(ByVal ContentName As String, Optional ByVal Criteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("OpenContent")
        '            '
        '            'If Not (true) Then Exit Function
        '            main_OpenContent = db.csOpen(genericController.encodeText(ContentName), Criteria, SortFieldList, ActiveOnly)
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_OpenContent")
        '        End Function
        '
        '========================================================================
        ' main_Gets the field in the current CSRow according to its definition
        '========================================================================
        '
        Public Function cs_cs_getRecordEditLink(ByVal CSPointer As Integer, Optional ByVal AllowCut As Object = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordEditLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim RecordName As String
            Dim ContentName As String
            Dim RecordID As Integer
            Dim ContentControlID As Integer
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            '
            MethodName = "main_cs_getRecordEditLink"
            '
            If iCSPointer = -1 Then
                Throw (New ApplicationException("main_cs_getRecordEditLink called with invalid iCSPointer")) ' handleLegacyError14(MethodName, "")
            Else
                If Not db.cs_ok(iCSPointer) Then
                    Throw (New ApplicationException("main_cs_getRecordEditLink called with Not main_CSOK")) ' handleLegacyError14(MethodName, "")
                Else
                    '
                    ' Print an edit link for the records Content (may not be iCSPointer content)
                    '
                    RecordID = (db.cs_getInteger(iCSPointer, "ID"))
                    RecordName = db.cs_getText(iCSPointer, "Name")
                    ContentControlID = (db.cs_getInteger(iCSPointer, "contentcontrolid"))
                    ContentName = metaData.getContentNameByID(ContentControlID)
                    If ContentName <> "" Then
                        cs_cs_getRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), RecordName, authContext.isEditing(Me, ContentName))
                    End If
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        ''========================================================================
        ''   upgraded
        ''========================================================================
        ''
        'Public Function main_GetFormStart(Optional ByVal ActionQueryString As String = "") As String
        '    main_GetFormStart = main_GetFormStart(ActionQueryString)
        'End Function
        ''
        ''========================================================================
        '' ----- Starts an HTML form
        ''       Should be closed with PrintFormEnd
        ''========================================================================
        ''
        'Public Function main_GetFormStart(Optional ByVal ActionQueryString As String = "", Optional ByVal htmlName As String = "", Optional ByVal HtmlId As String = "") As String
        '    main_GetFormStart2 = main_GetFormStart3(ActionQueryString, htmlName, HtmlId)

        'End Function
        '
        '
        '
        Public Function exportAscii_GetAsciiExport(ByVal ContentName As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetAsciiExport")
            '
            'If Not (true) Then Exit Function
            '
            'dim dt as datatable
            Dim SQL As String
            Dim Criteria As String
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim Delimiter As String
            Dim Copy As String
            Dim DataSourceName As String
            Dim TableName As String
            Dim CSPointer As Integer
            Dim FieldNameVariant As String
            Dim FieldName As String
            Dim UcaseFieldName As String
            Dim fieldType As Integer
            Dim MethodName As String
            Dim iContentName As String
            Dim sb As New System.Text.StringBuilder
            'Dim PageSize As Integer
            'Dim PageNumber As Integer
            Dim TestFilename As String
            '
            TestFilename = "AsciiExport" & common_GetRandomLong_Internal() & ".txt"
            '
            iContentName = genericController.encodeText(ContentName)
            'PageSize = encodeEmptyInteger(PageSize, 1000)
            If PageSize = 0 Then
                PageSize = 1000
            End If
            'PageNumber = encodeEmptyInteger(PageNumber, 1)
            If PageNumber = 0 Then
                PageNumber = 1
            End If
            '
            MethodName = "main_GetAsciiExport"
            '
            ' ----- Check for special case iContentNames
            '
            Call webServer.webServerIO_setResponseContentType("text/plain")
            Call htmlDoc.enableOutputBuffer(False)
            TableName = genericController.GetDbObjectTableName(GetContentTablename(iContentName))
            Select Case genericController.vbUCase(TableName)
                Case "CCMEMBERS"
                    '
                    ' ----- People and member content export
                    '
                    If Not authContext.isAuthenticatedAdmin(Me) Then
                        Call sb.Append("Warning: You must be a site administrator to export this information.")
                    Else
                        CSPointer = db.cs_open(iContentName, , "ID", False, , , ,, PageSize, PageNumber)
                        '
                        ' ----- print out the field names
                        '
                        If db.cs_ok(CSPointer) Then
                            Call sb.Append("""EID""")
                            Delimiter = ","
                            FieldNameVariant = db.cs_getFirstFieldName(CSPointer)
                            Do While (FieldNameVariant <> "")
                                FieldName = genericController.encodeText(FieldNameVariant)
                                UcaseFieldName = genericController.vbUCase(FieldName)
                                If (UcaseFieldName <> "USERNAME") And (UcaseFieldName <> "PASSWORD") Then
                                    Call sb.Append(Delimiter & """" & FieldName & """")
                                End If
                                FieldNameVariant = db.cs_getNextFieldName(CSPointer)
                                '''DoEvents
                            Loop
                            Call sb.Append(vbCrLf)
                        End If
                        '
                        ' ----- print out the values
                        '
                        Do While db.cs_ok(CSPointer)
                            If Not (db.cs_getBoolean(CSPointer, "Developer")) Then
                                Copy = security.encodeToken((db.cs_getInteger(CSPointer, "ID")), app_startTime)
                                Call sb.Append("""" & Copy & """")
                                Delimiter = ","
                                FieldNameVariant = db.cs_getFirstFieldName(CSPointer)
                                Do While (FieldNameVariant <> "")
                                    FieldName = genericController.encodeText(FieldNameVariant)
                                    UcaseFieldName = genericController.vbUCase(FieldName)
                                    If (UcaseFieldName <> "USERNAME") And (UcaseFieldName <> "PASSWORD") Then
                                        Copy = db.cs_get(CSPointer, FieldName)
                                        If Copy <> "" Then
                                            Copy = genericController.vbReplace(Copy, """", "'")
                                            Copy = genericController.vbReplace(Copy, vbCrLf, " ")
                                            Copy = genericController.vbReplace(Copy, vbCr, " ")
                                            Copy = genericController.vbReplace(Copy, vbLf, " ")
                                        End If
                                        Call sb.Append(Delimiter & """" & Copy & """")
                                    End If
                                    FieldNameVariant = db.cs_getNextFieldName(CSPointer)
                                    '''DoEvents
                                Loop
                                Call sb.Append(vbCrLf)
                            End If
                            Call db.cs_goNext(CSPointer)
                            '''DoEvents
                        Loop
                    End If
                    ' End Case
                Case Else
                    '
                    ' ----- All other content
                    '
                    If Not authContext.isAuthenticatedContentManager(Me, iContentName) Then
                        Call sb.Append("Error: You must be a content manager to export this data.")
                    Else
                        CSPointer = db.cs_open(iContentName, , "ID", False, , , ,, PageSize, PageNumber)
                        '
                        ' ----- print out the field names
                        '
                        If db.cs_ok(CSPointer) Then
                            Delimiter = ""
                            FieldNameVariant = db.cs_getFirstFieldName(CSPointer)
                            Do While (FieldNameVariant <> "")
                                Call appRootFiles.appendFile(TestFilename, Delimiter & """" & FieldNameVariant & """")
                                Delimiter = ","
                                FieldNameVariant = db.cs_getNextFieldName(CSPointer)
                                '''DoEvents
                            Loop
                            Call appRootFiles.appendFile(TestFilename, vbCrLf)
                        End If
                        '
                        ' ----- print out the values
                        '
                        Do While db.cs_ok(CSPointer)
                            Delimiter = ""
                            FieldNameVariant = db.cs_getFirstFieldName(CSPointer)
                            Do While (FieldNameVariant <> "")
                                Select Case db.cs_getFieldTypeId(CSPointer, genericController.encodeText(FieldNameVariant))
                                    Case FieldTypeIdFileTextPrivate, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTMLPrivate
                                        Copy = main_cs_getEncodedField(CSPointer, genericController.encodeText(FieldNameVariant))
                                    Case FieldTypeIdLookup
                                        Copy = db.cs_getLookup(CSPointer, genericController.encodeText(FieldNameVariant))
                                    Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                                    Case Else
                                        Copy = db.cs_getText(CSPointer, genericController.encodeText(FieldNameVariant))
                                End Select
                                If Copy <> "" Then
                                    Copy = genericController.vbReplace(Copy, """", "'")
                                    Copy = genericController.vbReplace(Copy, vbCrLf, " ")
                                    Copy = genericController.vbReplace(Copy, vbCr, " ")
                                    Copy = genericController.vbReplace(Copy, vbLf, " ")
                                End If
                                Call appRootFiles.appendFile(TestFilename, Delimiter & """" & Copy & """")
                                Delimiter = ","
                                FieldNameVariant = db.cs_getNextFieldName(CSPointer)
                                '''DoEvents
                            Loop
                            Call appRootFiles.appendFile(TestFilename, vbCrLf)
                            Call db.cs_goNext(CSPointer)
                            '''DoEvents
                        Loop
                    End If
            End Select
            exportAscii_GetAsciiExport = appRootFiles.readFile(TestFilename)
            Call appRootFiles.deleteFile(TestFilename)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '        '
        '        '
        '        '
        '        Public Sub user_SetMember(PeopleID As Integer)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetMember")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim CSPointer As Integer
        '            Dim MethodName As String
        '            Dim iPeopleID As Integer
        '            '
        '            iPeopleID = genericController.EncodeInteger(PeopleID)
        '            '
        '            MethodName = "main_SetMember"
        '            '
        '            CSPointer = db.csOpen("people", "id=" & db.EncodeSQLNumber(iPeopleID))
        '            If Not db.csOk(CSPointer) Then
        '                Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError14(MethodName, "main_SetMember ErrorTrap, could not find RecordID [" & iPeopleID & "] in people content.")
        '            Else
        '                Call db.setCS(CSPointer, "ContentControlID", main_GetContentID("Members"))
        '            End If
        '            Call db.csClose(CSPointer)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_SetMember")
        '        End Sub
        '
        '
        '
        Public Sub main_ClearStream()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ClearStream")
            '
            htmlDoc.docBuffer = ""
            webServer.webServerIO_bufferRedirect = ""
            webServer.webServerIO_bufferResponseHeader = ""
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ClearStream")
            '
        End Sub
        '        '
        '        '========================================================================
        '        '   Read in a file from the sites virtual file directory given filename
        '        '========================================================================
        '        '
        '        Public Function app.contentFiles.ReadFile(ByVal Filename As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ReadVirtualFile")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_ReadVirtualFile = app.contentFiles.ReadFile(Filename)
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ReadVirtualFile")
        '        End Function
        '            '
        '            '========================================================================
        '            '   Save data to a file in the sites virtual file directory
        '            '========================================================================
        '            '
        '            Public Sub app.publicFiles.SaveFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("SaveVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_SaveVirtualFile"
        '            '
        '            Call app.publicFiles.SaveFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Delete a file from the virtual director
        '        '========================================================================
        '        '
        '        Public Sub app.publicFiles.DeleteFile(ByVal Filename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("DeleteVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_DeleteVirtualFile"
        '            '
        '        Call app.publicFiles.DeleteFile(genericController.encodeText(Filename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' Delete a file from the virtual director
        '        '========================================================================
        '        '
        '        Public Sub main_CopyVirtualFile(ByVal SourceFilename As String, ByVal DestinationFilename As String)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("CopyVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_CopyVirtualFilename"
        '            '
        '            Call app.contentFiles.copyFile(genericController.encodeText(SourceFilename), genericController.encodeText(DestinationFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        '   append data to the end of a file in the sites virtual file directory
        '        '========================================================================
        '        '
        '        Public Sub main_AppendVirtualFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AppendVirtualFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_AppendVirtualFile"
        '            '
        '            Call app.publicFiles.appendFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
        '        End Sub
        '        '
        '        '========================================================================
        '        '   Save data to a file
        '        '========================================================================
        '        '
        '        Public Sub main_SaveFile(ByVal Filename As Object, ByVal FileContent As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("SaveFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_SaveFile"
        '            '
        '        Call app.publicFiles.SaveFile(genericController.encodeText(Filename), genericController.encodeText(FileContent))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '        '
        '        '========================================================================
        '        ' ----- Creates a file folder if it does not exist
        '        '========================================================================
        '        '
        '        Public Sub main_CreateFileFolder(ByVal FolderPath As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("CreateFileFolder")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_CreateFileFolder"
        '            '
        '        Call app.publicFiles.createPath(genericController.encodeText(FolderPath))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   Deletes a file if it exists
        '            '========================================================================
        '            '
        '        Public Sub mainx_DeleteFile(ByVal Filename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("DeleteFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_DeleteFile"
        '            '
        '            Call app.csv_DeleteFile(genericController.encodeText(Filename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   Copy a file
        '            '========================================================================
        '            '
        '        Public Sub main_xcopyFile(ByVal SourcePathFilename As Object, ByVal DestinationPathFilename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("copyFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_copyFile"
        '            '
        '            Call app.csv_CopyFile(genericController.encodeText(SourcePathFilename), genericController.encodeText(DestinationPathFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   rename a file
        '            '========================================================================
        '            '
        '        Public Sub main_renamxeFile(ByVal SourcePathFilename As Object, ByVal DestinationFilename As Object)
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("renameFile")
        '            '
        '            'If Not (true) Then Exit Sub
        '            Dim MethodName As String
        '            '
        '            MethodName = "main_renameFile"
        '            '
        '            Call app.csv_renameFile(genericController.encodeText(SourcePathFilename), genericController.encodeText(DestinationFilename))
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1(MethodName)
        '            '
        '        End Sub
        '            '
        '            '========================================================================
        '            '   main_Get a list of files in a folder
        '            '========================================================================
        '            '
        '        Public Function main_GetFxileList(ByVal FolderPath As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As IO.FileInfo()
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("GetFileList")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetFileList = app.csv_GetFileList(genericController.encodeText(FolderPath), genericController.EncodeInteger(PageSize), genericController.EncodeInteger(PageNumber))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1("main_GetFileList")
        '        End Function
        '        '
        '        '========================================================================
        '        '   main_Get a list of files in a folder
        '        '========================================================================
        '        '
        '        Public Function main_GetFileCount(ByVal FolderPath As Object) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFileCount")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetFileCount = app.getPublicFileCount(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFileCount")
        '        End Function
        '
        '========================================================================
        '   main_Get a list of files in a folder
        '========================================================================
        '
        Public Function getFolderNameList(ByVal FolderPath As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFolderList")
            '
            'If Not (true) Then Exit Function
            '
            getFolderNameList = appRootFiles.getFolderNameList(genericController.encodeText(FolderPath))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFolderList")
        End Function
        '            '
        '            '========================================================================
        '            '   main_Get a list of files in a folder in the Virtual Content path
        '            '========================================================================
        '            '
        '        Public Function main_GetVirtxualFileList(ByVal FolderPath As String, Optional ByVal PageSize As Integer = 1000, Optional ByVal PageNumber As Integer = 1) As IO.FileInfo()
        '            On Error GoTo ErrorTrap : ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFileList")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetVirtualFileList = app.publicFiles.GetFolderFiles(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call main_HandleClassErrorAndResume_TrapPatch1("main_GetVirtualFileList")
        '        End Function
        '        '
        '        '========================================================================
        '        '   main_Get a list of files in a folder in the Virtual Content path
        '        '========================================================================
        '        '
        '        Public Function main_GetVirtualFileCount(ByVal FolderPath As Object) As Integer
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFileCount")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetVirtualFileCount = app.csv_GetVirtualFileCount(genericController.encodeText(FolderPath))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetVirtualFileCount")
        '        End Function
        '
        '========================================================================
        '   main_Get a list of files in a folder in the Virtual Content path
        '========================================================================
        '
        Public Function main_GetVirtualFolderList(ByVal FolderPath As Object) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetVirtualFolderList")
            '
            'If Not (true) Then Exit Function
            '
            main_GetVirtualFolderList = cdnFiles.getFolderNameList(genericController.encodeText(FolderPath))
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetVirtualFolderList")
        End Function
        '
        '========================================================================
        ' main_Get a Contents ID from the ContentName
        '========================================================================
        '
        Public Function main_GetContentID(ByVal ContentName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentID")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "main_GetContentID"
            '
            main_GetContentID = metaData.getContentId(genericController.encodeText(ContentName))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        ''
        ''========================================================================
        '' main_Get a Contents Name from the ContentID
        ''========================================================================
        ''
        'Public Function metaData.getContentNameByID(ByVal ContentID As Integer) As String
        '    Return metaData.getContentNameByID(ContentID)
        'End Function
        '
        ' ----- main_Get a DataSource Name from its ContentName
        '
        Public Function main_GetContentDataSource(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentDataSource")
            '
            'If Not (true) Then Exit Function
            Dim MethodName As String
            '
            MethodName = "main_GetContentDataSource"
            '
            main_GetContentDataSource = metaData.getContentDataSource(genericController.encodeText(ContentName))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function

        '
        '========================================================================
        ' main_DeleteContentRecord by content name
        '
        '   To be compatible with a previous release, if the RecordID is not an integer,
        '   call main_DeleteContentRecords.
        '========================================================================
        '
        Public Sub DeleteContentRecord(ByVal ContentName As String, ByVal RecordID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteContentRecord")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim iRecordID As Integer
            Dim iContentName As String
            '
            iRecordID = genericController.EncodeInteger(RecordID)
            iContentName = genericController.encodeText(ContentName)
            '
            MethodName = "main_DeleteContentRecord"
            '
            If (iContentName = "") Or (iRecordID = 0) Then
                If (genericController.encodeText(RecordID) <> "") And (genericController.encodeText(RecordID) <> "0") Then
                    Call db.deleteContentRecord(ContentName, RecordID)
                Else
                    Throw (New Exception("Invalid ContentName [" & iContentName & "] or RecordID [" & genericController.encodeText(RecordID) & "]"))
                End If
            Else
                Call db.deleteContentRecord(iContentName, iRecordID, authContext.user.ID)
            End If
            Call main_ProcessSpecialCaseAfterSave(True, iContentName, iRecordID, "", 0, False)
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' main_DeleteCSRecord
        '========================================================================
        '
        Public Sub DeleteCSRecord(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteCSRecord")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_DeleteCSRecord"
            '
            Call db.cs_deleteRecord(genericController.EncodeInteger(CSPointer))
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' main_InsertContentRecordGetID
        '   Inserts a record into a content table.
        '   Returns the ID of the record, 0 if error
        '========================================================================
        '
        Public Function metaData_InsertContentRecordGetID(ByVal ContentName As String) As Integer
            metaData_InsertContentRecordGetID = db.metaData_InsertContentRecordGetID(genericController.encodeText(ContentName), authContext.user.ID)
        End Function
        '
        '=============================================================================
        ' Create a child content from a parent content
        '
        '   If child does not exist, copy everything from the parent
        '   If child already exists, add any missing fields from parent
        '=============================================================================
        '
        Public Sub metaData_CreateContentChild(ByVal ChildContentName As String, ByVal ParentContentName As String)
            Call metaData.createContentChild(genericController.encodeText(ChildContentName), genericController.encodeText(ParentContentName), authContext.user.ID)
        End Sub
        '
        ' ----- alternate name
        '
        Public Function InsertCSContent(ByVal ContentName As String) As Integer
            InsertCSContent = db.cs_insertRecord(genericController.encodeText(ContentName))
        End Function
        '
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Function web_GetBrowserLanguageID() As Integer
            Dim LanguageID As Integer = 0
            Dim LanguageName As String = ""
            Call web_GetBrowserLanguage(LanguageID, LanguageName)
            web_GetBrowserLanguageID = LanguageID
        End Function
        '
        '========================================================================
        '   Determine the current persons Language
        '
        '   Return the ID in the Languages content
        '========================================================================
        '
        Public Sub web_GetBrowserLanguage(ByRef LanguageID As Integer, ByRef LanguageName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetBrowserLanguage")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim CommaPosition As Integer
            Dim DashPosition As Integer
            Dim AcceptLanguageString As String
            Dim AcceptLanguage As String
            '
            MethodName = "main_GetBrowserLanguage"
            LanguageID = 0
            LanguageName = ""
            '
            ' ----- Determine Language by browser
            '
            AcceptLanguageString = genericController.encodeText(webServer.RequestLanguage) & ","
            CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Do While CommaPosition <> 0 And LanguageID = 0
                AcceptLanguage = Trim(Mid(AcceptLanguageString, 1, CommaPosition - 1))
                AcceptLanguageString = Mid(AcceptLanguageString, CommaPosition + 1)
                If Len(AcceptLanguage) > 0 Then
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, "-")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    DashPosition = genericController.vbInstr(1, AcceptLanguage, ";")
                    If DashPosition > 1 Then
                        AcceptLanguage = Mid(AcceptLanguage, 1, DashPosition - 1)
                    End If
                    If Len(AcceptLanguage) > 0 Then
                        CS = db.cs_open("languages", "HTTP_Accept_LANGUAGE=" & db.encodeSQLText(AcceptLanguage), , , , , , "ID", 1)
                        If db.cs_ok(CS) Then
                            LanguageID = db.cs_getInteger(CS, "ID")
                            LanguageName = db.cs_getText(CS, "Name")
                        End If
                        Call db.cs_Close(CS)
                    End If
                End If
                CommaPosition = genericController.vbInstr(1, AcceptLanguageString, ",")
            Loop
            '
            If LanguageID = 0 Then
                '
                ' ----- no matching browser language, use site default
                '
                CS = db.cs_open("languages", "name=" & db.encodeSQLText(siteProperties.language), , , , , , "ID", 1)
                If db.cs_ok(CS) Then
                    LanguageID = db.cs_getInteger(CS, "ID")
                    LanguageName = db.cs_getText(CS, "Name")
                End If
                Call db.cs_Close(CS)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        ' main_GetRecordEditLink( iContentName, iRecordID )
        '
        '   iContentName The content for this link
        '   iRecordID    The ID of the record in the Table
        '========================================================================
        '
        Public Function main_GetRecordEditLink(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal AllowCut As Boolean = False) As String
            main_GetRecordEditLink = main_GetRecordEditLink2(ContentName, RecordID, genericController.EncodeBoolean(AllowCut), "", authContext.isEditing(Me, ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordEditLink2( iContentName, iRecordID, AllowCut, RecordName )
        '
        '   ContentName The content for this link
        '   RecordID    The ID of the record in the Table
        '   AllowCut
        '   RecordName
        '   IsEditing
        '========================================================================
        '
        Public Function main_GetRecordEditLink2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal AllowCut As Boolean, ByVal RecordName As String, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordEditLink2")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim SQL As String
            Dim ContentID As Integer
            Dim Link As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim RootEntryName As String
            Dim ClipBoard As String
            Dim WorkingLink As String
            Dim iAllowCut As Boolean
            Dim Icon As String
            Dim ContentCaption As String
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iAllowCut = genericController.EncodeBoolean(AllowCut)
            ContentCaption = htmlDoc.html_EncodeHTML(iContentName)
            If genericController.vbLCase(ContentCaption) = "aggregate functions" Then
                ContentCaption = "Add-on"
            End If
            If genericController.vbLCase(ContentCaption) = "aggregate function objects" Then
                ContentCaption = "Add-on"
            End If
            ContentCaption = ContentCaption & " record"
            If RecordName <> "" Then
                ContentCaption = ContentCaption & ", named '" & RecordName & "'"
            End If
            '
            MethodName = "main_GetRecordEditLink2"
            '
            main_GetRecordEditLink2 = ""
            If (iContentName = "") Then
                Throw (New ApplicationException("ContentName [" & ContentName & "] is invalid")) ' handleLegacyError14(MethodName, "")
            Else
                If (iRecordID < 1) Then
                    Throw (New ApplicationException("RecordID [" & RecordID & "] is invalid")) ' handleLegacyError14(MethodName, "")
                Else
                    If IsEditing Then
                        '
                        ' Edit link, main_Get the CID
                        '
                        ContentID = main_GetContentID(iContentName)
                        '
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "<a" _
                            & " class=""ccRecordEditLink"" " _
                            & " TabIndex=-1" _
                            & " href=""" & htmlDoc.html_EncodeHTML(siteProperties.adminURL & "?cid=" & ContentID & "&id=" & iRecordID & "&af=4&aa=2&ad=1") & """"
                        If Not htmlDoc.main_ReturnAfterEdit Then
                            main_GetRecordEditLink2 = main_GetRecordEditLink2 & " target=""_blank"""
                        End If
                        main_GetRecordEditLink2 = main_GetRecordEditLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentEdit.gif""" _
                            & " border=""0""" _
                            & " alt=""Edit this " & htmlDoc.html_EncodeHTML(ContentCaption) & """" _
                            & " title=""Edit this " & htmlDoc.html_EncodeHTML(ContentCaption) & """" _
                            & " align=""absmiddle""" _
                            & "></a>"
                        '
                        ' Cut Link if enabled
                        '
                        If iAllowCut Then
                            WorkingLink = genericController.modifyLinkQuery(webServer.webServerIO_requestPage & "?" & web_RefreshQueryString, RequestNameCut, genericController.encodeText(ContentID) & "." & genericController.encodeText(RecordID), True)
                            main_GetRecordEditLink2 = "" _
                                & main_GetRecordEditLink2 _
                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & htmlDoc.html_EncodeHTML(WorkingLink) & """><img src=""/ccLib/images/Contentcut.gif"" border=""0"" alt=""Cut this " & ContentCaption & " to clipboard"" title=""Cut this " & ContentCaption & " to clipboard"" align=""absmiddle""></a>"
                        End If
                        '
                        ' Help link if enabled
                        '
                        Dim helpLink As String
                        helpLink = ""
                        'helpLink = main_GetHelpLink(5, "Editing " & ContentCaption, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentEdit.gif"" style=""vertical-align:middle""> Edit-Content icon<br><br>Edit-Content icons appear in your content. Click them to edit your content.")
                        main_GetRecordEditLink2 = "" _
                            & main_GetRecordEditLink2 _
                            & helpLink
                        '
                        main_GetRecordEditLink2 = "<span class=""ccRecordLinkCon"" style=""white-space:nowrap;"">" & main_GetRecordEditLink2 & "</span>"
                        ''
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:absolute;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                        '
                        'main_GetRecordEditLink2 = "" _
                        '    & cr & "<div style=""position:relative;display:inline;"">" _
                        '    & genericController.kmaIndent(main_GetRecordEditLink2) _
                        '    & cr & "</div>"
                    End If

                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' Print an add link for the current ContentSet
        '   iCSPointer is the content set to be added to
        '   PresetNameValueList is a name=value pair to force in the added record
        '========================================================================
        '
        Public Function main_cs_getRecordAddLink(ByVal CSPointer As Integer, Optional ByVal PresetNameValueList As String = "", Optional ByVal AllowPaste As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentName As String
            Dim iPresetNameValueList As String
            Dim MethodName As String
            Dim iCSPointer As Integer
            '
            iCSPointer = genericController.EncodeInteger(CSPointer)
            iPresetNameValueList = genericController.encodeEmptyText(PresetNameValueList, "")
            '
            MethodName = "main_cs_getRecordAddLink"
            '
            If iCSPointer < 0 Then
                Throw (New ApplicationException("invalid ContentSet pointer [" & iCSPointer & "]")) ' handleLegacyError14(MethodName, "main_cs_getRecordAddLink was called with ")
            Else
                '
                ' Print an add tag to the iCSPointers Content
                '
                ContentName = db.cs_getContentName(iCSPointer)
                If ContentName = "" Then
                    Throw (New ApplicationException("main_cs_getRecordAddLink was called with a ContentSet that was created with an SQL statement. The function requires a ContentSet opened with an OpenCSContent.")) ' handleLegacyError14(MethodName, "")
                Else
                    main_cs_getRecordAddLink = main_GetRecordAddLink(ContentName, iPresetNameValueList, AllowPaste)
                End If
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink( iContentName, iPresetNameValueList )
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink(ByVal ContentName As String, ByVal PresetNameValueList As String, Optional ByVal AllowPaste As Boolean = False) As String
            main_GetRecordAddLink = main_GetRecordAddLink2(genericController.encodeText(ContentName), genericController.encodeText(PresetNameValueList), AllowPaste, authContext.isEditing(Me, ContentName))
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink2
        '
        '   Returns a string of add tags for the Content Definition included, and all
        '   child contents of that area.
        '
        '   iContentName The content for this link
        '   iPresetNameValueList The sql equivalent used to select the record.
        '           translates to name0=value0,name1=value1.. pairs separated by ,
        '
        '   LowestRootMenu - The Menu in the flyout structure that is the furthest down
        '   in the chain that the user has content access to. This is so a content manager
        '   does not have to navigate deep into a structure to main_Get to content he can
        '   edit.
        '   Basically, the entire menu is created down from the MenuName, and populated
        '   with all the entiries this user has access to. The LowestRequiredMenuName is
        '   is returned from the _branch routine, and that is to root on-which the
        '   main_GetMenu uses
        '========================================================================
        '
        Public Function main_GetRecordAddLink2(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal AllowPaste As Boolean, ByVal IsEditing As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink")
            '
            'If Not (true) Then Exit Function
            '
            Dim ParentID As Integer
            Dim BufferString As String
            Dim MethodName As String
            Dim iContentName As String
            Dim iContentID As Integer
            Dim iPresetNameValueList As String
            Dim MenuName As String
            Dim MenuHasBranches As Boolean
            Dim LowestRequiredMenuName As String
            Dim ClipBoard As String
            Dim PasteLink As String
            Dim Position As Integer
            Dim ClipBoardArray As String()
            Dim ClipboardContentID As Integer
            Dim ClipChildRecordID As Integer
            Dim iAllowPaste As Boolean
            Dim useFlyout As Boolean
            Dim csChildContent As Integer
            Dim Link As String
            '
            MethodName = "main_GetRecordAddLink"
            '
            main_GetRecordAddLink2 = ""
            If IsEditing Then
                iContentName = genericController.encodeText(ContentName)
                iPresetNameValueList = genericController.encodeText(PresetNameValueList)
                iPresetNameValueList = genericController.vbReplace(iPresetNameValueList, "&", ",")
                iAllowPaste = genericController.EncodeBoolean(AllowPaste)

                If iContentName = "" Then
                    Throw (New ApplicationException("Method called with blank ContentName")) ' handleLegacyError14(MethodName, "")
                Else
                    iContentID = main_GetContentID(iContentName)
                    csChildContent = db.cs_open("Content", "ParentID=" & iContentID, , , , , , "id")
                    useFlyout = db.cs_ok(csChildContent)
                    Call db.cs_Close(csChildContent)
                    '
                    If Not useFlyout Then
                        Link = siteProperties.adminURL & "?cid=" & iContentID & "&af=4&aa=2&ad=1"
                        If PresetNameValueList <> "" Then
                            Link = Link & "&wc=" & htmlDoc.main_EncodeRequestVariable(PresetNameValueList)
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "<a" _
                            & " TabIndex=-1" _
                            & " href=""" & htmlDoc.html_EncodeHTML(Link) & """"
                        If Not htmlDoc.main_ReturnAfterEdit Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & " target=""_blank"""
                        End If
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                            & "><img" _
                            & " src=""/ccLib/images/IconContentAdd.gif""" _
                            & " border=""0""" _
                            & " alt=""Add record""" _
                            & " title=""Add record""" _
                            & " align=""absmiddle""" _
                            & "></a>"
                    Else
                        '
                        MenuName = common_GetRandomLong_Internal().ToString
                        Call htmlDoc.menu_AddEntry(MenuName, , "/ccLib/images/IconContentAdd.gif", , , , "stylesheet", "stylesheethover")
                        LowestRequiredMenuName = main_GetRecordAddLink_AddMenuEntry(iContentName, iPresetNameValueList, "", MenuName, MenuName)
                    End If
                    '
                    ' Add in the paste entry, if needed
                    '
                    If iAllowPaste Then
                        ClipBoard = visitProperty.getText("Clipboard", "")
                        If ClipBoard <> "" Then
                            Position = genericController.vbInstr(1, ClipBoard, ".")
                            If Position <> 0 Then
                                ClipBoardArray = Split(ClipBoard, ".")
                                If UBound(ClipBoardArray) > 0 Then
                                    ClipboardContentID = genericController.EncodeInteger(ClipBoardArray(0))
                                    ClipChildRecordID = genericController.EncodeInteger(ClipBoardArray(1))
                                    'iContentID = main_GetContentID(iContentName)
                                    If IsWithinContent(ClipboardContentID, iContentID) Then
                                        If genericController.vbInstr(1, iPresetNameValueList, "PARENTID=", vbTextCompare) <> 0 Then
                                            '
                                            ' must test for main_IsChildRecord
                                            '
                                            BufferString = iPresetNameValueList
                                            BufferString = genericController.vbReplace(BufferString, "(", "")
                                            BufferString = genericController.vbReplace(BufferString, ")", "")
                                            BufferString = genericController.vbReplace(BufferString, ",", "&")
                                            ParentID = genericController.EncodeInteger(main_GetNameValue_Internal(BufferString, "Parentid"))
                                        End If


                                        If (ParentID <> 0) And (Not pages.main_IsChildRecord(iContentName, ParentID, ClipChildRecordID)) Then
                                            '
                                            ' Can not paste as child of itself
                                            '
                                            PasteLink = webServer.webServerIO_requestPage & "?" & web_RefreshQueryString
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePaste, "1", True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentContentID, CStr(iContentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteParentRecordID, CStr(ParentID), True)
                                            PasteLink = genericController.modifyLinkQuery(PasteLink, RequestNamePasteFieldList, iPresetNameValueList, True)
                                            main_GetRecordAddLink2 = main_GetRecordAddLink2 _
                                                & "<a class=""ccRecordCutLink"" TabIndex=""-1"" href=""" & htmlDoc.html_EncodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste record in clipboard here"" title=""Paste record in clipboard here"" align=""absmiddle""></a>"
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    '
                    ' Add in the available flyout menu entries
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & menuFlyout.getMenu(LowestRequiredMenuName, 0)
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "class=""ccFlyoutButton"" ", "", 1, 99, vbTextCompare)
                        If PasteLink <> "" Then
                            main_GetRecordAddLink2 = main_GetRecordAddLink2 & "<a TabIndex=-1 href=""" & htmlDoc.html_EncodeHTML(PasteLink) & """><img src=""/ccLib/images/ContentPaste.gif"" border=""0"" alt=""Paste content from clipboard"" align=""absmiddle""></a>"
                        End If
                    End If
                    '
                    ' Help link if enabled
                    '
                    Dim helpLink As String
                    helpLink = ""
                    'helpLink = main_GetHelpLink(6, "Adding " & iContentName, "Turn on Edit icons by checking 'Edit' in the tools panel, and click apply.<br><br><img src=""/ccLib/images/IconContentAdd.gif"" " & IconWidthHeight & " style=""vertical-align:middle""> Add-Content icon<br><br>Add-Content icons appear in your content. Click them to add content.")
                    main_GetRecordAddLink2 = main_GetRecordAddLink2 & helpLink                '
                    If main_GetRecordAddLink2 <> "" Then
                        main_GetRecordAddLink2 = "" _
                            & vbCrLf & vbTab & "<div style=""display:inline;"">" _
                            & genericController.kmaIndent(main_GetRecordAddLink2) _
                            & vbCrLf & vbTab & "</div>"
                    End If
                    '
                    ' ----- Add the flyout panels to the content to return
                    '       This must be here so if the call is made after main_ClosePage, the panels will still deliver
                    '
                    If LowestRequiredMenuName <> "" Then
                        main_GetRecordAddLink2 = main_GetRecordAddLink2 & htmlDoc.menu_GetClose()
                        If genericController.vbInstr(1, main_GetRecordAddLink2, "IconContentAdd.gif", vbTextCompare) <> 0 Then
                            main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "IconContentAdd.gif"" ", "IconContentAdd.gif"" align=""absmiddle"" ")
                        End If
                    End If
                    If htmlDoc.main_ReturnAfterEdit Then
                        main_GetRecordAddLink2 = genericController.vbReplace(main_GetRecordAddLink2, "target=", "xtarget=", 1, 99, vbTextCompare)
                    End If
                    'End If
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_GetRecordAddLink_AddMenuEntry( ContentName, PresetNameValueList, ContentNameList, MenuName )
        '
        '   adds an add entry for the content name, and all the child content
        '   returns the MenuName of the lowest branch that has valid
        '   menu entries.
        '
        '   ContentName The content for this link
        '   PresetNameValueList The sql equivalent used to select the record.
        '           translates to (name0=value0)&(name1=value1).. pairs separated by &
        '   ContentNameList is a comma separated list of names of the content included so far
        '   MenuName is the name of the root branch, for flyout menu
        '
        '   IsMember(), main_IsAuthenticated() And Member_AllowLinkAuthoring must already be checked
        '========================================================================
        '
        Private Function main_GetRecordAddLink_AddMenuEntry(ByVal ContentName As String, ByVal PresetNameValueList As String, ByVal ContentNameList As String, ByVal MenuName As String, ByVal ParentMenuName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordAddLink_AddMenuEntry")
            '
            'If Not (true) Then Exit Function
            '
            Dim Copy As String
            Dim CID As Integer
            Dim CS As Integer
            Dim SQL As String
            Dim LinkCount As Integer
            Dim csChildContent As Integer
            Dim ContentID As Integer
            Dim Link As String
            Dim MyContentNameList As String
            Dim MethodName As String
            Dim Label As String
            Dim ButtonCaption As String
            Dim ContentRecordFound As Boolean
            Dim ContentAllowAdd As Boolean
            Dim GroupRulesAllowAdd As Boolean
            Dim MemberRulesDateExpires As Date
            Dim MemberRulesAllow As Boolean
            Dim ChildMenuButtonCount As Integer
            Dim ChildMenuName As String
            Dim ChildContentName As String
            '
            MethodName = "main_GetRecordAddLink_AddMenuEntry"
            '
            Link = ""
            MyContentNameList = ContentNameList
            If (ContentName = "") Then
                Throw (New ApplicationException("main_GetRecordAddLink, ContentName is empty")) ' handleLegacyError14(MethodName, "")
            Else
                If (InStr(1, MyContentNameList, "," & genericController.vbUCase(ContentName) & ",") >= 0) Then
                    Throw (New ApplicationException("main_GetRecordAddLink_AddMenuEntry, Content Child [" & ContentName & "] is one of its own parents")) ' handleLegacyError14(MethodName, "")
                Else
                    MyContentNameList = MyContentNameList & "," & genericController.vbUCase(ContentName) & ","
                    '
                    ' ----- Select the Content Record for the Menu Entry selected
                    '
                    ContentRecordFound = False
                    If authContext.isAuthenticatedAdmin(Me) Then
                        '
                        ' ----- admin member, they have access, main_Get ContentID and set markers true
                        '
                        SQL = "SELECT ID as ContentID, AllowAdd as ContentAllowAdd, 1 as GroupRulesAllowAdd, null as MemberRulesDateExpires" _
                            & " FROM ccContent" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " );"
                        CS = db.cs_openSql(SQL)
                        If db.cs_ok(CS) Then
                            '
                            ' Entry was found
                            '
                            ContentRecordFound = True
                            ContentID = db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = True
                            MemberRulesDateExpires = Date.MinValue
                            MemberRulesAllow = True
                        End If
                        Call db.cs_Close(CS)
                    Else
                        '
                        ' non-admin member, first check if they have access and main_Get true markers
                        '
                        SQL = "SELECT ccContent.ID as ContentID, ccContent.AllowAdd as ContentAllowAdd, ccGroupRules.AllowAdd as GroupRulesAllowAdd, ccMemberRules.DateExpires as MemberRulesDateExpires" _
                            & " FROM (((ccContent" _
                                & " LEFT JOIN ccGroupRules ON ccGroupRules.ContentID=ccContent.ID)" _
                                & " LEFT JOIN ccgroups ON ccGroupRules.GroupID=ccgroups.ID)" _
                                & " LEFT JOIN ccMemberRules ON ccgroups.ID=ccMemberRules.GroupID)" _
                                & " LEFT JOIN ccMembers ON ccMemberRules.MemberID=ccMembers.ID" _
                            & " WHERE (" _
                            & " (ccContent.Name=" & db.encodeSQLText(ContentName) & ")" _
                            & " AND(ccContent.active<>0)" _
                            & " AND(ccGroupRules.active<>0)" _
                            & " AND(ccMemberRules.active<>0)" _
                            & " AND((ccMemberRules.DateExpires is Null)or(ccMemberRules.DateExpires>" & db.encodeSQLDate(app_startTime) & "))" _
                            & " AND(ccgroups.active<>0)" _
                            & " AND(ccMembers.active<>0)" _
                            & " AND(ccMembers.ID=" & authContext.user.ID & ")" _
                            & " );"
                        CS = db.cs_openSql(SQL)
                        If db.cs_ok(CS) Then
                            '
                            ' ----- Entry was found, member has some kind of access
                            '
                            ContentRecordFound = True
                            ContentID = db.cs_getInteger(CS, "ContentID")
                            ContentAllowAdd = db.cs_getBoolean(CS, "ContentAllowAdd")
                            GroupRulesAllowAdd = db.cs_getBoolean(CS, "GroupRulesAllowAdd")
                            MemberRulesDateExpires = db.cs_getDate(CS, "MemberRulesDateExpires")
                            MemberRulesAllow = False
                            If MemberRulesDateExpires = Date.MinValue Then
                                MemberRulesAllow = True
                            ElseIf (MemberRulesDateExpires > app_startTime) Then
                                MemberRulesAllow = True
                            End If
                        Else
                            '
                            ' ----- No entry found, this member does not have access, just main_Get ContentID
                            '
                            ContentRecordFound = True
                            ContentID = main_GetContentID(ContentName)
                            ContentAllowAdd = False
                            GroupRulesAllowAdd = False
                            MemberRulesAllow = False
                        End If
                        Call db.cs_Close(CS)
                    End If
                    If ContentRecordFound Then
                        '
                        ' Add the Menu Entry* to the current menu (MenuName)
                        '
                        Link = ""
                        ButtonCaption = ContentName
                        main_GetRecordAddLink_AddMenuEntry = MenuName
                        If ContentAllowAdd And GroupRulesAllowAdd And MemberRulesAllow Then
                            Link = siteProperties.adminURL & "?cid=" & ContentID & "&af=4&aa=2&ad=1"
                            If PresetNameValueList <> "" Then
                                Dim NameValueList As String
                                NameValueList = PresetNameValueList
                                Link = Link & "&wc=" & htmlDoc.main_EncodeRequestVariable(PresetNameValueList)
                            End If
                        End If
                        Call htmlDoc.menu_AddEntry(MenuName & ":" & ContentName, ParentMenuName, , , Link, ButtonCaption, "", "", True)
                        '
                        ' Create child submenu if Child Entries found
                        '
                        csChildContent = db.cs_open("Content", "ParentID=" & ContentID, , , , , , "name")
                        If Not db.cs_ok(csChildContent) Then
                            '
                            ' No child menu
                            '
                        Else
                            '
                            ' Add the child menu
                            '
                            ChildMenuName = MenuName & ":" & ContentName
                            ChildMenuButtonCount = 0
                            '
                            ' ----- Create the ChildPanel with all Children found
                            '
                            Do While db.cs_ok(csChildContent)
                                ChildContentName = db.cs_getText(csChildContent, "name")
                                Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ParentMenuName)
                                'Copy = main_GetRecordAddLink_AddMenuEntry(ChildContentName, PresetNameValueList, MyContentNameList, MenuName, ChildMenuName)
                                If Copy <> "" Then
                                    ChildMenuButtonCount = ChildMenuButtonCount + 1
                                End If
                                If (main_GetRecordAddLink_AddMenuEntry = "") And (Copy <> "") Then
                                    main_GetRecordAddLink_AddMenuEntry = Copy
                                End If
                                db.cs_goNext(csChildContent)
                            Loop
                        End If
                    End If
                End If
                Call db.cs_Close(csChildContent)
                'main_GetRecordAddLink_AddMenuEntry = Link
            End If
            '
            'main_TestPointExit
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetRecordAddLink_AddMenuEntry")
        End Function
        '
        '========================================================================
        ' Depricated - Use main_GetRecordEditLink and main_GetRecordAddLink
        '========================================================================
        '
        Public Function main_GetRecordEditLinkByContent(ByVal ContentID As Integer, ByVal RecordIDVariant As Object, ByVal Criteria As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordEditLinkByContent")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim ContentName As String
            '
            MethodName = "main_GetRecordEditLinkByContent"
            '
            ContentName = metaData.getContentNameByID(ContentID)
            If ContentName <> "" Then
                If Not genericController.IsNull(RecordIDVariant) Then
                    main_GetRecordEditLinkByContent = main_GetRecordEditLink2(ContentName, genericController.EncodeInteger(RecordIDVariant), False, "", authContext.isEditing(Me, ContentName))
                Else
                    main_GetRecordEditLinkByContent = main_GetRecordAddLink(ContentName, Criteria)
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function

        '
        '========================================================================
        ' ----- Compatibility Only
        '========================================================================
        '
        Public Function main_GetFormCSInput(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormCSInput")
            '
            'If Not (true) Then Exit Function
            '
            If db.cs_ok(genericController.EncodeInteger(CSPointer)) Then
                '
                ' Just do a text box with the value
                '
                main_GetFormCSInput = htmlDoc.html_GetFormInputText2(FieldName, genericController.encodeText(db.cs_getField(CSPointer, FieldName)))
            Else
                '
                ' Just do a text box with a blank
                '
                main_GetFormCSInput = htmlDoc.html_GetFormInputText2(genericController.encodeText(FieldName), "")
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFormCSInput")
            '
        End Function
        '
        '==================================================================================================
        ' ----- Remove this record from all watch lists
        '       Mark permanent if the content is being deleted. non-permanent otherwise
        '==================================================================================================
        '
        Public Sub metaData_DeleteContentTracking(ContentName As String, RecordID As Integer, Permanent As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("DeleteContentTracking")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_DeleteContentTracking( " & ContentName & ", " & RecordID & " )"
            '
            Call csv_DeleteContentTracking(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID), genericController.EncodeBoolean(Permanent))
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '=================================================================================
        '   Public for main_GetRandomLong_Internal
        '=================================================================================
        '
        Public Function common_GetRandomLong() As Integer
            On Error GoTo ErrorTrap
            '
            common_GetRandomLong = common_GetRandomLong_Internal()
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetRandomLong")
        End Function
        '
        '=================================================================================
        '   main_Get a Random long value
        '=================================================================================
        '
        Public Function common_GetRandomLong_Internal() As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRandomLong_Internal")
            '
            Dim RandomLimit As Single
            '
            RandomLimit = ((2 ^ 30) - 1)
            common_GetRandomLong_Internal = CInt(Rnd() * RandomLimit)
            '
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetRandomLong_Internal")
        End Function
        '
        '========================================================================
        ' ----- Starts an HTML page (for an admin page -- not a public page)
        '========================================================================
        '
        Public Function admin_GetPageStart(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
            admin_GetPageStart = admin_GetPageStart2(Title, PageMargin)
        End Function
        '
        '========================================================================
        ' ----- Starts an HTML page (for an admin page -- not a public page)
        '========================================================================
        '
        Public Function admin_GetPageStart2(Optional ByVal Title As String = "", Optional ByVal PageMargin As Integer = 0) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetPageStartAdmin")
            '
            'If Not (true) Then Exit Function
            '
            If Title <> "" Then
                Call htmlDoc.main_AddPagetitle(Title)
            End If
            If htmlDoc.main_MetaContent_Title = "" Then
                Call htmlDoc.main_AddPagetitle("Admin-" & webServer.webServerIO_requestDomain)
            End If
            webServer.webServerIO_response_NoFollow = True
            '
            ' main_SetMetaContent - this is done by the 'content' contributer for the page
            '
            Call main_SetMetaContent(0, 0)
            '
            admin_GetPageStart2 = "" _
                & main_DocTypeAdmin _
                & vbCrLf & "<html>" _
                & vbCrLf & "<head>" _
                & webServer.webServerIO_GetHTMLInternalHead(True) _
                & vbCrLf & "</head>" _
                & vbCrLf & "<body class=""ccBodyAdmin ccCon"">"
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetPageStartAdmin")
        End Function
        '
        '
        '
        Public Property web_RefreshQueryString() As String
            Get
                web_RefreshQueryString = htmlDoc.htmlDoc_RefreshQueryString
            End Get
            Set(ByVal value As String)
                htmlDoc.htmlDoc_RefreshQueryString = value
            End Set
        End Property
        '
        '========================================================================
        ' -----
        '========================================================================
        '
        Public Function properties_user_getText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetMemberID As Integer = SystemMemberID) As String
            Dim returnProperty As String = DefaultValue
            Try
                If TargetMemberID = SystemMemberID Then
                    returnProperty = userProperty.getText(PropertyName, DefaultValue, authContext.user.ID)
                Else
                    returnProperty = userProperty.getText(PropertyName, DefaultValue, TargetMemberID)
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnProperty
        End Function
        '
        Public Function properties_user_getInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As Integer = 0, Optional ByVal TargetMemberID As Integer = SystemMemberID) As Integer
            Return genericController.EncodeInteger(properties_user_getText(PropertyName, DefaultValue.ToString, TargetMemberID))
        End Function
        ''
        ''========================================================================
        '' -----
        ''========================================================================
        ''
        'Public Sub userProperty_SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetMemberID As Integer = SystemMemberID)
        '    Try
        '        If TargetMemberID = SystemMemberID Then
        '            Call userProperty.setProperty(PropertyName, Value, authcontext.user.userId)
        '        Else
        '            Call userProperty.setProperty(PropertyName, Value, TargetMemberID)
        '        End If
        '    Catch ex As Exception
        '        throw (ex)
        '    End Try
        'End Sub
        ''
        'public Sub userProperty_SetProperty(ByVal PropertyName As String, ByVal Value As Integer, Optional ByVal TargetMemberID As Integer = SystemMemberID)
        '    userProperty_SetProperty(PropertyName, Value.ToString, TargetMemberID)
        'End Sub
        ''
        ''========================================================================
        '' -----
        ''========================================================================
        ''
        'Public Sub properties_SetVisitProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitId As Integer = 0)
        '    Try
        '        If TargetVisitId = 0 Then
        '            Call visitProperty.setProperty(PropertyName, Value, main_VisitId)
        '        Else
        '            Call visitProperty.setProperty(PropertyName, Value, TargetVisitId)
        '        End If
        '    Catch ex As Exception
        '        throw (ex)
        '    End Try
        'End Sub
        ''
        ''========================================================================
        '' -----
        ''========================================================================
        ''
        'Public Function vistorProperty_getText(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "", Optional ByVal TargetVisitorid As Integer = 0) As String
        '    Dim returnProperty As String = DefaultValue
        '    Try
        '        If TargetVisitorid = 0 Then
        '            returnProperty = visitorProperty.getText(PropertyName, DefaultValue, main_VisitorID)
        '        Else
        '            returnProperty = visitorProperty.getText(PropertyName, DefaultValue, TargetVisitorid)
        '        End If
        '    Catch ex As Exception
        '        throw (ex)
        '    End Try
        '    Return returnProperty
        'End Function
        ''
        ''========================================================================
        '' -----
        ''========================================================================
        ''
        'Public Sub visitorProperty_SetProperty(ByVal PropertyName As String, ByVal Value As String, Optional ByVal TargetVisitorid As Integer = 0)
        '    Try
        '        If TargetVisitorid = 0 Then
        '            Call visitorProperty.setProperty(PropertyName, Value, main_VisitorID)
        '        Else
        '            Call visitorProperty.setProperty(PropertyName, Value, TargetVisitorid)
        '        End If
        '    Catch ex As Exception
        '        throw (ex)
        '    End Try
        'End Sub
        '
        '==========================================================================
        '   Add on to the common error message
        '==========================================================================
        '
        Public Sub error_AddUserError(ByVal Message As String)
            '
            If (InStr(1, debug_iUserError, Message, vbTextCompare) = 0) Then
                debug_iUserError = debug_iUserError & cr & "<li class=""ccError"">" & genericController.encodeText(Message) & "</LI>"
            End If
            '
        End Sub
        '
        '==========================================================================
        '   main_Get The user error messages
        '       If there are none, return ""
        '==========================================================================
        '
        Public Function error_GetUserError() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00224")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "main_GetUserError"
            '
            error_GetUserError = genericController.encodeText(debug_iUserError)
            If error_GetUserError <> "" Then
                error_GetUserError = "<ul class=""ccError"">" & genericController.kmaIndent(error_GetUserError) & cr & "</ul>"
                error_GetUserError = UserErrorHeadline & "" & error_GetUserError
                debug_iUserError = ""
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '==========================================================================
        '   main_IsUserError
        '       Returns true if there is a user error
        '==========================================================================
        '
        Public Function error_IsUserError() As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00225")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "main_IsUserError"
            '
            error_IsUserError = (debug_iUserError <> "")
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function

        '
        '==========================================================================
        '   Copy the records from one CS to another
        '==========================================================================
        '
        Public Sub cs_CopyRecord(ByVal CSSource As Object, ByVal CSDestination As Object)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00229")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            '
            MethodName = "main_CopyCSRecord"
            '
            Call db.cs_copyRecord(genericController.EncodeInteger(CSSource), genericController.EncodeInteger(CSDestination))
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function GetContentProperty(ByVal ContentName As String, ByVal PropertyName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentProperty")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim Contentdefinition As cdefModel
            '
            MethodName = "main_GetContentProperty"
            '
            Contentdefinition = metaData.getCdef(genericController.encodeText(ContentName))
            Select Case genericController.vbUCase(genericController.encodeText(PropertyName))
                Case "CONTENTCONTROLCRITERIA"
                    GetContentProperty = Contentdefinition.ContentControlCriteria
                Case "ACTIVEONLY"
                    GetContentProperty = Contentdefinition.ActiveOnly.ToString
                Case "ADMINONLY"
                    GetContentProperty = Contentdefinition.AdminOnly.ToString
                Case "ALIASID"
                    GetContentProperty = Contentdefinition.AliasID
                Case "ALIASNAME"
                    GetContentProperty = Contentdefinition.AliasName
                Case "ALLOWADD"
                    GetContentProperty = Contentdefinition.AllowAdd.ToString
                Case "ALLOWDELETE"
                    GetContentProperty = Contentdefinition.AllowDelete.ToString
                'Case "CHILDIDLIST"
                '    main_GetContentProperty = Contentdefinition.ChildIDList
                Case "DATASOURCEID"
                    GetContentProperty = Contentdefinition.dataSourceId.ToString
                Case "DEFAULTSORTMETHOD"
                    GetContentProperty = Contentdefinition.DefaultSortMethod
                Case "DEVELOPERONLY"
                    GetContentProperty = Contentdefinition.DeveloperOnly.ToString
                Case "FIELDCOUNT"
                    GetContentProperty = Contentdefinition.fields.Count.ToString
                'Case "FIELDPOINTER"
                '    main_GetContentProperty = Contentdefinition.FieldPointer
                Case "ID"
                    GetContentProperty = Contentdefinition.Id.ToString
                Case "IGNORECONTENTCONTROL"
                    GetContentProperty = Contentdefinition.IgnoreContentControl.ToString
                Case "NAME"
                    GetContentProperty = Contentdefinition.Name
                Case "PARENTID"
                    GetContentProperty = Contentdefinition.parentID.ToString
                'Case "SINGLERECORD"
                '    main_GetContentProperty = Contentdefinition.SingleRecord
                Case "CONTENTTABLENAME"
                    GetContentProperty = Contentdefinition.ContentTableName
                Case "CONTENTDATASOURCENAME"
                    GetContentProperty = Contentdefinition.ContentDataSourceName
                Case "AUTHORINGTABLENAME"
                    GetContentProperty = Contentdefinition.AuthoringTableName
                Case "AUTHORINGDATASOURCENAME"
                    GetContentProperty = Contentdefinition.AuthoringDataSourceName
                Case "WHERECLAUSE"
                    GetContentProperty = Contentdefinition.WhereClause
                Case "ALLOWWORKFLOWAUTHORING"
                    GetContentProperty = Contentdefinition.AllowWorkflowAuthoring.ToString
                Case "DROPDOWNFIELDLIST"
                    GetContentProperty = Contentdefinition.DropDownFieldList
                Case "SELECTFIELDLIST"
                    GetContentProperty = Contentdefinition.SelectCommaList
                Case Else
                    Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
            End Select
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function GetContentFieldProperty(ByVal ContentName As String, ByVal FieldName As String, ByVal PropertyName As String) As Object
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00230")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim Contentdefinition As cdefModel
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim UcaseFieldName As String
            '
            MethodName = "main_GetContentFieldProperty"
            '
            GetContentFieldProperty = ""
            If True Then
                UcaseFieldName = genericController.vbUCase(genericController.encodeText(FieldName))
                Contentdefinition = metaData.getCdef(genericController.encodeText(ContentName))
                If (UcaseFieldName = "") Or (Contentdefinition.fields.Count < 1) Then
                    Throw (New ApplicationException("Content Name [" & genericController.encodeText(ContentName) & "] or FieldName [" & genericController.encodeText(FieldName) & "] was not valid")) ' handleLegacyError14(MethodName, "")
                Else
                    For Each keyValuePair As KeyValuePair(Of String, CDefFieldModel) In Contentdefinition.fields
                        Dim field As CDefFieldModel = keyValuePair.Value
                        With field
                            If UcaseFieldName = genericController.vbUCase(.nameLc) Then
                                Select Case genericController.vbUCase(genericController.encodeText(PropertyName))
                                    Case "FIELDTYPE", "TYPE"
                                        GetContentFieldProperty = .fieldTypeId
                                    Case "HTMLCONTENT"
                                        GetContentFieldProperty = .htmlContent
                                    Case "ADMINONLY"
                                        GetContentFieldProperty = .adminOnly
                                    Case "AUTHORABLE"
                                        GetContentFieldProperty = .authorable
                                    Case "CAPTION"
                                        GetContentFieldProperty = .caption
                                    Case "REQUIRED"
                                        GetContentFieldProperty = .Required
                                    Case "UNIQUENAME"
                                        GetContentFieldProperty = .UniqueName
                                    Case "UNIQUE"
                                        '
                                        ' fix for the uniquename screwup - it is not unique name, it is unique value
                                        '
                                        GetContentFieldProperty = .UniqueName
                                    Case "DEFAULT"
                                        GetContentFieldProperty = genericController.encodeText(.defaultValue)
                                    Case "MEMBERSELECTGROUPID"
                                        GetContentFieldProperty = genericController.encodeText(.MemberSelectGroupID)
                                    Case Else
                                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError14(MethodName, "Content Property [" & genericController.encodeText(PropertyName) & "] was not found in content [" & genericController.encodeText(ContentName) & "]")
                                End Select
                                Exit For
                            End If
                        End With
                    Next
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_Get a boolean from request
        '   if empty, returns null
        '   if RequestBlock true, tries only querystring
        '   if "ON", "YES", "TRUE" or non-0 number returns true, otherwise false
        '========================================================================
        '
        Public Function web_ReadStreamBoolean(ByVal Key As String) As Boolean
            Dim returnTrue As Boolean = False
            Try
                Dim ExpressionString As String = web_ReadStreamText(Key)
                If Not genericController.IsNull(ExpressionString) Then
                    If ExpressionString <> "" Then
                        If genericController.vbIsNumeric(ExpressionString) Then
                            If ExpressionString <> "0" Then
                                returnTrue = True
                            Else
                                returnTrue = False
                            End If
                        ElseIf genericController.vbUCase(ExpressionString) = "ON" Then
                            returnTrue = True
                        ElseIf genericController.vbUCase(ExpressionString) = "YES" Then
                            returnTrue = True
                        ElseIf genericController.vbUCase(ExpressionString) = "TRUE" Then
                            returnTrue = True
                        Else
                            returnTrue = False
                        End If
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnTrue
        End Function
        '
        '========================================================================
        ' main_Get a Date from request
        '   if empty, returns null
        '   if not a date, returns null
        '   if RequestBlock true, tries only querystring
        '========================================================================
        '
        Public Function web_ReadStreamDate(ByVal Key As String) As Date
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00233")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim testDate As String
            '
            MethodName = "main_ReadStreamDate"
            '
            testDate = web_ReadStreamText(Key)
            If Not genericController.IsNull(testDate) Then
                If Not IsDate(testDate) Then
                    web_ReadStreamDate = Nothing
                Else
                    web_ReadStreamDate = CDate(testDate)
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_Get a number from request
        '   if empty, returns null
        '   if not a value, returns null
        '   if RequestBlock true, tries only querystring
        '========================================================================
        '
        Public Function web_ReadStreamNumber(ByVal Key As String) As Double
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00234")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim testResults As String
            '
            MethodName = "main_ReadStreamNumber"
            '
            testResults = web_ReadStreamText(Key)
            If Not genericController.IsNull(web_ReadStreamNumber) Then
                If genericController.vbIsNumeric(testResults) Then
                    web_ReadStreamNumber = CDbl(testResults)
                Else
                    web_ReadStreamNumber = 0
                End If
            End If
            '
            debug_testPoint("main_ReadStreamNumber( " & genericController.encodeText(Key) & " )  = " & web_ReadStreamNumber)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '========================================================================
        ' main_Get a Text string from request
        '   if empty, returns null
        '   if RequestBlock true, tries only querystring
        '========================================================================
        '
        Public Function web_ReadStreamText(ByVal Key As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00235")
            '
            'If Not (true) Then Exit Function
            '
            web_ReadStreamText = docProperties.getText(Key)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ReadStreamText")
            '
        End Function
        '
        ' ----- 2.1 compatibility
        '
        Public Function GetContentTablename(ByVal ContentName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentTablename")
            '
            'If Not (true) Then Exit Function
            '
            GetContentTablename = genericController.encodeText(GetContentProperty(genericController.encodeText(ContentName), "ContentTableName"))
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentTablename")
        End Function

        '
        '========================================================================
        ' Opens a Content Definition into a ContentSEt
        '   Returns and integer that points into the ContentSet array
        '   If there was a problem, it returns -1
        '
        '   If authoring mode, as group of records are returned.
        '       The first is the current edit record
        '       The rest are the archive records.
        '========================================================================
        '
        Public Function csOpenRecord(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("OpenCSContentRecord_Internal")
            '
            'If Not (true) Then Exit Function
            '
            csOpenRecord = db.cs_open(ContentName, "(ID=" & db.encodeSQLNumber(RecordID) & ")", , False, authContext.user.ID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_OpenCSContentRecord_Internal")
        End Function
        '
        '========================================================================
        ' Opens a Content Definition into a ContentSEt
        '   Returns and integer that points into the ContentSet array
        '   If there was a problem, it returns -1
        '
        '   If authoring mode, as group of records are returned.
        '       The first is the current edit record
        '       The rest are the archive records.
        '========================================================================
        '
        Public Function csOpen(ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal WorkflowAuthoringMode As Boolean = False, Optional ByVal WorkflowEditingMode As Boolean = False, Optional ByVal SelectFieldList As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("OpenCSContentRecord")
            '
            'If Not (true) Then Exit Function
            '
            csOpen = db.cs_open(genericController.encodeText(ContentName), "(ID=" & db.encodeSQLNumber(RecordID) & ")", , False, authContext.user.ID, WorkflowAuthoringMode, WorkflowEditingMode, SelectFieldList, 1)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_OpenCSContentRecord")
        End Function
        '
        '========================================================================
        '   main_IsWithinContent( ChildContentID, ParentContentID )
        '
        '       Returns true if ChildContentID is in ParentContentID
        '========================================================================
        '
        Public Function IsWithinContent(ByVal ChildContentID As Integer, ByVal ParentContentID As Integer) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("IsWithinContent")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            '
            MethodName = "IsWithinContent"
            '
            IsWithinContent = metaData.isWithinContent(genericController.EncodeInteger(ChildContentID), genericController.EncodeInteger(ParentContentID))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '
        '
        Public Sub IncrementContentField(ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00252")
            '
            'If Not (true) Then Exit Sub
            '
            Dim MethodName As String
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim iFieldName As String
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iFieldName = genericController.encodeText(FieldName)
            '
            MethodName = "main_IncrementContentField( " & iContentName & "," & genericController.encodeText(iRecordID) & "," & iFieldName & " )"
            '
            Call IncrementContentField_Internal(iContentName, iRecordID, iFieldName)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '
        '
        Public Sub IncrementContentField_Internal(ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00253")
            '
            'If Not (true) Then Exit Sub
            '
            Dim SQL As String
            Dim CDef As cdefModel
            '
            CDef = metaData.getCdef(ContentName)
            Call main_IncrementTableField(CDef.ContentTableName, RecordID, FieldName, CDef.ContentDataSourceName)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_IncrementContentField_Internal")
        End Sub
        '
        '
        '
        Public Function EncodeSQLText(ByVal SourceText As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00254")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim iSourceText As String
            '
            iSourceText = genericController.encodeText(SourceText)
            '
            MethodName = "main_EncodeSQLText( " & iSourceText & " )"
            '
            EncodeSQLText = db.encodeSQLText(iSourceText)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function

        '
        '========================================================================
        '   main_cs_getField - Fast
        '       CSv is not checked
        '       All arguments typed
        '========================================================================
        '
        Public Function cs_GetField(ByVal CSPointer As Integer, ByVal FieldName As String) As Object
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getField_Internal")
            '
            'If Not (true) Then Exit Function
            '
            cs_GetField = db.cs_getField(CSPointer, FieldName)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_cs_getField_Internal")
            '
        End Function
        '
        '
        '
        Public Function GetTableID(ByVal TableName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00273")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim CS As Integer
            '
            MethodName = "main_GetTableID"
            '
            GetTableID = -1
            CS = db.cs_openSql("Select ID from ccTables where name=" & db.encodeSQLText(TableName), , 1)
            If db.cs_ok(CS) Then
                GetTableID = db.cs_getInteger(CS, "ID")
            End If
            Call db.cs_Close(CS)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        ' ================================================================================================
        '   conversion pass 2
        ' ================================================================================================
        '
        '
        '
        '
        Public Function main_GetAuthoringStatusMessage(ByVal IsContentWorkflowAuthoring As Boolean, ByVal RecordEditLocked As Boolean, ByVal main_EditLockName As String, ByVal main_EditLockExpires As Date, ByVal RecordApproved As Boolean, ByVal ApprovedBy As String, ByVal RecordSubmitted As Boolean, ByVal SubmittedBy As String, ByVal RecordDeleted As Boolean, ByVal RecordInserted As Boolean, ByVal RecordModified As Boolean, ByVal ModifiedBy As String) As String
            main_GetAuthoringStatusMessage = ""
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00274")
            '
            'If Not (true) Then Exit Function
            '
            Dim MethodName As String
            Dim Copy As String
            Dim Delimiter As String = ""
            Dim main_EditLockExpiresMinutes As Integer
            '
            MethodName = "main_GetAuthoringStatusMessage"
            '
            main_EditLockExpiresMinutes = CInt((main_EditLockExpires - app_startTime).TotalMinutes)
            If Not siteProperties.allowWorkflowAuthoring Then
                '
                ' ----- site does not support workflow authoring
                '
                If RecordEditLocked Then
                    Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                    main_GetAuthoringStatusMessage &= Delimiter & Copy
                    Delimiter = "<BR >"
                End If
                main_GetAuthoringStatusMessage &= Delimiter & Msg_WorkflowDisabled
                Delimiter = "<BR >"
            ElseIf Not IsContentWorkflowAuthoring Then
                '
                ' ----- content does not support workflow authoring
                '
                If RecordEditLocked Then
                    Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                    Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                    main_GetAuthoringStatusMessage &= Delimiter & Copy
                    Delimiter = "<BR >"
                End If
                main_GetAuthoringStatusMessage &= Delimiter & Msg_ContentWorkflowDisabled
                Delimiter = "<BR >"
            Else
                '
                ' ----- Workflow Authoring is supported, check deleted, inserted or modified
                '
                If RecordApproved Then
                    '
                    ' Approved
                    '
                    If authContext.isAuthenticatedAdmin(Me) Then
                        Copy = genericController.vbReplace(Msg_AuthoringApprovedAdmin, "<EDITNAME>", ApprovedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        Delimiter = "<BR >"
                    Else
                        Copy = genericController.vbReplace(Msg_AuthoringApproved, "<EDITNAME>", ApprovedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                ElseIf RecordSubmitted Then
                    '
                    ' Submitted
                    '
                    If authContext.isAuthenticatedAdmin(Me) Then
                        Copy = genericController.vbReplace(Msg_AuthoringSubmittedAdmin, "<EDITNAME>", SubmittedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        Delimiter = "<BR >"
                    Else
                        Copy = genericController.vbReplace(Msg_AuthoringSubmitted, "<EDITNAME>", SubmittedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                ElseIf RecordDeleted Then
                    '
                    ' deleted
                    '
                    main_GetAuthoringStatusMessage &= Delimiter & Msg_AuthoringDeleted
                    Delimiter = "<BR >"
                ElseIf RecordInserted Then
                    '
                    ' inserted
                    '
                    main_GetAuthoringStatusMessage &= Delimiter & Msg_AuthoringInserted
                    Delimiter = "<BR >"
                ElseIf RecordModified Then
                    '
                    ' modified, submitted or approved
                    '
                    If authContext.isAuthenticatedAdmin(Me) Then
                        If RecordEditLocked Then
                            Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                            main_GetAuthoringStatusMessage &= Delimiter & Copy
                            Delimiter = "<BR >"
                        End If
                        Copy = genericController.vbReplace(Msg_AuthoringRecordModifedAdmin, "<EDITNAME>", ModifiedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        'main_GetAuthoringStatusMessage &=  Delimiter & Msg_AuthoringRecordModifedAdmin
                        Delimiter = "<BR >"
                    Else
                        If RecordEditLocked Then
                            Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                            Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                            main_GetAuthoringStatusMessage &= Delimiter & Copy
                            Delimiter = "<BR >"
                        End If
                        Copy = genericController.vbReplace(Msg_AuthoringRecordModifed, "<EDITNAME>", ModifiedBy)
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        'main_GetAuthoringStatusMessage &=  Delimiter & Msg_AuthoringRecordModifed
                        Delimiter = "<BR >"
                    End If
                End If
                '
                ' ----- Check for authoring status messages if it has been modified
                '
                If main_GetAuthoringStatusMessage = "" Then
                    '
                    ' no changes
                    '
                    If RecordEditLocked Then
                        Copy = genericController.vbReplace(Msg_EditLock, "<EDITNAME>", main_EditLockName)
                        Copy = genericController.vbReplace(Copy, "<EDITEXPIRES>", main_EditLockExpires.ToString)
                        Copy = genericController.vbReplace(Copy, "<EDITEXPIRESMINUTES>", genericController.encodeText(main_EditLockExpiresMinutes))
                        main_GetAuthoringStatusMessage &= Delimiter & Copy
                        Delimiter = "<BR >"
                    End If
                    main_GetAuthoringStatusMessage &= Delimiter & Msg_AuthoringRecordNotModifed
                    Delimiter = "<BR >"
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '
        '   Checks the username and password
        '
        Public Function main_IsLoginOK(ByVal Username As String, ByVal Password As String, Optional ByVal ErrorMessage As String = "", Optional ByVal ErrorCode As Integer = 0) As Boolean
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00275")
            '
            'If Not (true) Then Exit Function
            '
            main_IsLoginOK = (authContext.authenticateGetId(Me, Username, Password) <> 0)
            If Not main_IsLoginOK Then
                ErrorMessage = error_GetUserError()
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_IsLoginOK")
            '
        End Function
        '
        '
        '
        Public Function main_GetResourceLibrary(Optional ByVal RootFolderName As String = "", Optional ByVal AllowSelectResource As Boolean = False, Optional ByVal SelectResourceEditorName As String = "") As String
            main_GetResourceLibrary = main_GetResourceLibrary2(RootFolderName, AllowSelectResource, SelectResourceEditorName, "", True)
        End Function
        '
        '
        '
        Public Function main_GetResourceLibrary2(ByVal RootFolderName As String, ByVal AllowSelectResource As Boolean, ByVal SelectResourceEditorName As String, ByVal SelectLinkObjectName As String, ByVal AllowGroupAdd As Boolean) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetResourceLibrary2")
            '
            'If Not (true) Then Exit Function
            '
            Dim ResourceLibrary As Object
            Dim Option_String As String
            Dim addonId As Integer
            '
            Option_String = "" _
                & "RootFolderName=" & RootFolderName _
                & "&AllowSelectResource=" & AllowSelectResource _
                & "&SelectResourceEditorName=" & SelectResourceEditorName _
                & "&SelectLinkObjectName=" & SelectLinkObjectName _
                & "&AllowGroupAdd=" & AllowGroupAdd _
                & ""
            main_GetResourceLibrary2 = addon.execute_legacy4("{564EF3F5-9673-4212-A692-0942DD51FF1A}", Option_String, CPUtilsBaseClass.addonContext.ContextAdmin)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            ResourceLibrary = Nothing
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetResourceLibrary2")
        End Function
        '
        '========================================================================
        ' Read and save a main_GetFormInputCheckList
        '   see main_GetFormInputCheckList for an explaination of the input
        '========================================================================
        '
        Public Sub main_ProcessCheckList(ByVal TagName As String, ByVal PrimaryContentName As String, ByVal PrimaryRecordID As String, ByVal SecondaryContentName As String, ByVal RulesContentName As String, ByVal RulesPrimaryFieldname As String, ByVal RulesSecondaryFieldName As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("ProcessCheckList")
            '
            'If Not (true) Then Exit Sub
            '
            Dim rulesTablename As String
            Dim SQL As String
            'dim dt as datatable
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
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim SecondaryRecordID As Integer
            Dim RuleNeeded As Boolean
            Dim CSRule As Integer
            Dim DateExpires As Date
            Dim DateExpiresVariant As Object
            Dim RuleContentChanged As Boolean
            Dim SupportRuleCopy As Boolean
            Dim RuleCopy As String
            '
            MethodName = "ProcessCheckList"
            '
            ' --- create Rule records for all selected
            '
            GroupCnt = docProperties.getInteger(TagName & ".RowCount")
            If GroupCnt > 0 Then
                '
                ' Test if RuleCopy is supported
                '
                SupportRuleCopy = main_IsContentFieldSupported(RulesContentName, "RuleCopy")
                If SupportRuleCopy Then
                    SupportRuleCopy = SupportRuleCopy And main_IsContentFieldSupported(SecondaryContentName, "AllowRuleCopy")
                    If SupportRuleCopy Then
                        SupportRuleCopy = SupportRuleCopy And main_IsContentFieldSupported(SecondaryContentName, "RuleCopyCaption")
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
                rulesTablename = GetContentTablename(RulesContentName)
                SQL = "select " & RulesSecondaryFieldName & ",id from " & rulesTablename & " where (" & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")and(active<>0) order by " & RulesSecondaryFieldName
                currentRulesCnt = 0
                currentRules = db.executeSql(SQL)
                currentRulesCnt = currentRules.Rows.Count
                For GroupPtr = 0 To GroupCnt - 1
                    '
                    ' ----- Read Response
                    '
                    SecondaryRecordID = docProperties.getInteger(TagName & "." & GroupPtr & ".ID")
                    RuleCopy = docProperties.getText(TagName & "." & GroupPtr & ".RuleCopy")
                    RuleNeeded = docProperties.getBoolean(TagName & "." & GroupPtr)
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
                        SQL = "update " & rulesTablename & " set rulecopy=" & db.encodeSQLText(RuleCopy) & " where id=" & RuleId
                        Call db.executeSql(SQL)
                    ElseIf RuleNeeded And (Not RuleFound) Then
                        '
                        ' No record exists, and one is needed
                        '
                        CSRule = db.cs_insertRecord(RulesContentName)
                        If db.cs_ok(CSRule) Then
                            Call db.cs_set(CSRule, "Active", RuleNeeded)
                            Call db.cs_set(CSRule, RulesPrimaryFieldname, PrimaryRecordID)
                            Call db.cs_set(CSRule, RulesSecondaryFieldName, SecondaryRecordID)
                            If SupportRuleCopy Then
                                Call db.cs_set(CSRule, "RuleCopy", RuleCopy)
                            End If
                        End If
                        Call db.cs_Close(CSRule)
                        RuleContentChanged = True
                    ElseIf (Not RuleNeeded) And RuleFound Then
                        '
                        ' Record exists and it is not needed
                        '
                        SQL = "delete from " & rulesTablename & " where id=" & RuleId
                        Call db.executeSql(SQL)
                        RuleContentChanged = True
                    End If
                Next
                '
                ' delete dups
                '
                If dupRuleIdList <> "" Then
                    SQL = "delete from " & rulesTablename & " where id in (" & Mid(dupRuleIdList, 2) & ")"
                    Call db.executeSql(SQL)
                    RuleContentChanged = True
                End If
                '        For GroupPtr = 0 To GroupCnt - 1
                '            '
                '            ' ----- Read Response
                '            '
                '            SecondaryRecordID = main_GetStreamInteger2(TagName & "." & GroupPtr & ".ID")
                '            RuleCopy = main_GetStreamText2(TagName & "." & GroupPtr & ".RuleCopy")
                '            RuleNeeded = main_GetStreamBoolean2(TagName & "." & GroupPtr)
                '            '
                '            ' ----- Update Record
                '            '
                '            CSRule = app.csOpen(RulesContentName, "(" & RulesPrimaryFieldname & "=" & PrimaryRecordID & ")and(" & RulesSecondaryFieldName & "=" & SecondaryRecordID & ")", , False)
                '            If SupportRuleCopy And RuleNeeded And (app.csv_IsCSOK(CSRule)) Then
                '                '
                '                ' Record exists and is needed, update the rule copy
                '                '
                '                Call app.csv_SetCS(CSRule, "RuleCopy", RuleCopy)
                '            ElseIf RuleNeeded And (Not app.csv_IsCSOK(CSRule)) Then
                '                '
                '                ' No record exists, and one is needed
                '                '
                '                Call app.closeCS(CSRule)
                '                CSRule = app.InsertCSRecord(RulesContentName)
                '                If app.csv_IsCSOK(CSRule) Then
                '                    Call app.csv_SetCS(CSRule, "Active", RuleNeeded)
                '                    Call app.csv_SetCS(CSRule, RulesPrimaryFieldname, PrimaryRecordID)
                '                    Call app.csv_SetCS(CSRule, RulesSecondaryFieldName, SecondaryRecordID)
                '                    If SupportRuleCopy Then
                '                        Call app.csv_SetCS(CSRule, "RuleCopy", RuleCopy)
                '                    End If
                '                End If
                '                RuleContentChanged = True
                '            ElseIf (Not RuleNeeded) And app.csv_IsCSOK(CSRule) Then
                '                '
                '                ' Record exists and it is not needed
                '                '
                '                Call main_DeleteCSRecord(CSRule)
                '                RuleContentChanged = True
                '            End If
                '            Call app.closeCS(CSRule)
                '        Next
            End If
            If RuleContentChanged Then
                Call cache.invalidateContent(RulesContentName)
            End If
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Sub
        '
        '========================================================================
        '   main_cs_get Field, translate all fields to their best text equivalent, and encode for display
        '========================================================================
        '
        Public Function main_cs_getEncodedField(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getEncodedField")
            '
            Dim ContentName As String
            Dim RecordID As Integer
            '
            'If Not (true) Then Exit Function
            '
            If db.cs_isFieldSupported(CSPointer, "id") And db.cs_isFieldSupported(CSPointer, "contentcontrolId") Then
                RecordID = db.cs_getInteger(CSPointer, "id")
                ContentName = metaData.getContentNameByID(db.cs_getInteger(CSPointer, "contentcontrolId"))
            End If
            main_cs_getEncodedField = htmlDoc.html_encodeContent10(db.cs_get(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName)), authContext.user.ID, ContentName, RecordID, 0, False, False, True, True, False, True, "", "http://" & webServer.requestDomain, False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, authContext.isAuthenticated, Nothing, authContext.isEditingAnything(Me))
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_cs_getEncodedField")
        End Function
        '
        '=============================================================================================
        '   main_cs_get calls
        '=============================================================================================
        '
        '        Public Function db.cs_getText(ByVal CSPointer As Integer, ByVal FieldName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getText")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_cs_getText = db.cs_getText(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName))
        '            'main_cs_getText = genericController.encodeText(main_cs_getField_Internal(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName)))
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_cs_getText")
        '        End Function
        '
        '=============================================================================================
        '   main_cs_get calls
        '=============================================================================================
        '
        '        Public Function db.cs_getFilename(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal OriginalFilename As String, Optional ByVal ContentName As String = "") As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getFilename")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            db.cs_getFilename = db.cs_getFilename(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName), genericController.encodeText(OriginalFilename), genericController.encodeEmptyText(ContentName, ""))
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("db.cs_getFilename")
        '        End Function
        '        '
        '        Public Function db.cs_getBoolean(ByVal CSPointer As Integer, ByVal FieldName As String) As Boolean
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getBoolean")
        '            '
        '            main_cs_getBoolean = db.cs_getBoolean((CSPointer), genericController.encodeText(FieldName))
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_cs_getBoolean")
        '        End Function
        '

        ''
        'Public Function db.cs_getNumber(ByVal CSPointer As Integer, ByVal FieldName As String) As Double
        '    xxcs_getNumber = db.cs_getNumber(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName))
        'End Function
        ''
        ''
        ''
        'Public Function db.cs_getLookup(ByVal CSPointer As Integer, ByVal FieldName As String) As String
        '    xxcs_getLookup = db.cs_getLookup(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName))
        'End Function
        '
        '
        '
        Public Function cs_getSource(ByVal CSPointer As Integer) As String
            Dim iCS As Integer
            '
            iCS = genericController.EncodeInteger(CSPointer)
            If Not db.cs_ok(iCS) Then
                Call Err.Raise(ignoreInteger, "dll", "ContentSet is not main_CSOK")
            Else
                cs_getSource = db.cs_getSource(iCS)
            End If
        End Function
        '        '
        '        '========================================================================
        '        '   Aborts any edits for this record
        '        '========================================================================
        '        '
        '        Public Sub workflow.workflow_AbortEdit(ByVal ContentName As String, ByVal RecordID As Integer)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("AbortEdit")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Call db.workflow.workflow_AbortEdit(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID), authcontext.user.userid)
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_AbortEdit")
        '        End Sub

        '        '
        '        '
        '        '
        '        Public Function db.cs_getRow(ByVal CSPointer As Integer) As Object
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cs_getRow")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_cs_getRow = db.cs_getRow(genericController.EncodeInteger(CSPointer))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_cs_getRow")
        '            '
        '        End Function
        ''
        ''
        ''
        'Public Function db.cs_getRows(ByVal CSPointer As Integer) As Object
        '    main_cs_getRows = db.cs_getRows(genericController.EncodeInteger(CSPointer))
        'End Function
        ''
        ''
        ''
        'Public Function main_cs_getRowCount(ByVal CSPointer As Integer) As Integer
        '    main_cs_getRowCount = db.cs_getRowCount(genericController.EncodeInteger(CSPointer))
        'End Function
        ''
        ''   Leave interface
        ''
        'Public ReadOnly Property main_AllowencodeHTML() As Boolean
        '    Get
        '        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("AllowEncodeHTML is not supported")
        '    End Get
        'End Property
        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty.getboolean("AllowHelpIcon")() As Boolean
        '    Get
        '        If authcontext.user.isAuthenticated() Then
        '            If Not property_visit_allowHelpIcon_isLoaded Then
        '                property_visit_allowHelpIcon_Local = genericController.EncodeBoolean(visitProperty.getBoolean("AllowHelpIcon")
        '                property_visit_allowHelpIcon_isLoaded = True
        '            End If
        '        End If
        '        visitProperty.getboolean("AllowHelpIcon") = property_visit_allowHelpIcon_Local

        '    End Get
        'End Property

        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty_AllowLinkAuthoring() As Boolean
        '    Get
        '        visitProperty_AllowLinkAuthoring = visitProperty.getBoolean("AllowEditing")

        '    End Get
        'End Property
        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty_AllowQuickEditor() As Boolean
        '    Get
        '        If authcontext.user.isAuthenticated() Then
        '            If Not property_visit_allowQuickEditor_isLoaded Then
        '                property_visit_allowQuickEditor = genericController.EncodeBoolean(
        '                property_visit_allowQuickEditor_isLoaded = True
        '            End If
        '            visitProperty_AllowQuickEditor = property_visit_allowQuickEditor
        '        End If
        '        '

        '    End Get
        'End Property
        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty_AllowAdvancedEditor() As Boolean
        '    Get
        '        If authcontext.user.isAuthenticated() Then
        '            If Not property_visit_allowAdvancedEditor_isLoaded Then
        '                property_visit_allowAdvancedEditor = visitProperty.getBoolean("AllowAdvancedEditor")
        '                property_visit_allowAdvancedEditor_isLoaded = True
        '            End If
        '            visitProperty_AllowAdvancedEditor = property_visit_allowAdvancedEditor
        '        End If

        '    End Get
        'End Property
        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty_AllowPresentationAuthoring() As Boolean
        '    Get
        '        visitProperty_AllowPresentationAuthoring = visitProperty.getBoolean("AllowQuickEditor")

        '    End Get
        'End Property

        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty_AllowWorkflowRendering() As Boolean
        '    Get
        '        If authcontext.user.isAuthenticated() Then
        '            If Not property_visit_allowWorkflowRendering_isLoaded Then
        '                property_visit_allowWorkflowRendering = genericController.EncodeBoolean(
        '                property_visit_allowWorkflowRendering_isLoaded = True
        '            End If
        '            visitProperty_AllowWorkflowRendering = property_visit_allowWorkflowRendering
        '        End If

        '    End Get
        'End Property
        ''
        ''   Buffered Visit Property
        ''
        'Public ReadOnly Property visitProperty.getBoolean("AllowDebugging")() As Boolean
        '    Get
        '        visitProperty.getBoolean("AllowDebugging") = False
        '        If authcontext.user.isAuthenticated() Then
        '            If Not property_visit_allowDebugging_isLoaded Then
        '                property_visit_allowDebugging_Local = genericController.EncodeBoolean(
        '                property_visit_allowDebugging_isLoaded = True
        '            End If
        '            visitProperty.getBoolean("AllowDebugging") = property_visit_allowDebugging_Local
        '        End If

        '    End Get
        'End Property
        '
        Public Function main_GetYesNo(ByVal InputValue As Boolean) As String
            If InputValue Then
                Return "Yes"
            Else
                Return "No"
            End If
        End Function
        '        '
        '        '========================================================================
        '        '   Returns Content Page fields as strings
        '        '========================================================================
        '        '
        '        Public Function main_GetContentPageField(ByVal FieldName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetContentPageField")
        '            '
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError23("Deprecated Method: main_GetContentPageField")
        '            'dim dt as datatable
        '            Dim returnString As String
        '            Dim rs As DataTable
        '            '
        '            returnString = ""
        '            rs = db.executeSql("select " & genericController.encodeText(FieldName) & " from ccpagecontent where id=" & currentPageID)
        '            If rs.Rows.Count > 0 Then
        '                returnString = genericController.encodeText(rs.Rows(0).Item(0))
        '            End If
        '            '
        '            main_GetContentPageField = returnString
        '            '    '
        '            '    Dim FieldCount as integer
        '            '    ' converted array to dictionary - Dim FieldPointer As Integer
        '            '    Dim UcaseFieldName As String
        '            '    '
        '            '    UcaseFieldName = genericController.vbUCase(genericController.encodeText(FieldName))
        '            '    If Not IsEmpty(main_oldCacheRS_FieldValues) Then
        '            '        FieldCount = UBound(main_oldCacheRS_FieldNames)
        '            '        If FieldCount > 0 Then
        '            '            For FieldPointer = 0 To FieldCount - 1
        '            '                If UcaseFieldName = genericController.vbUCase(main_oldCacheRS_FieldNames(FieldPointer)) Then
        '            '                    main_GetContentPageField = main_oldCacheRS_FieldValues(FieldPointer)
        '            '                    Exit For
        '            '                    End If
        '            '                Next
        '            '            End If
        '            '        End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetContentPageField")
        '        End Function
        ''
        ''========================================================================
        ''   Preloads the ContentPage
        ''       If PageContentCS app.csv_IsCSOK, then do nothing
        ''       else,
        ''========================================================================
        ''
        'Public Sub main_PreloadContentPage(ByVal RootPageName As String, Optional ByVal RootContentName As String = "")
        '    Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError23("Deprecated Method: main_GetAutoSite")
        'End Sub
        '
        '========================================================================
        '   2.1 compatibility
        ' ----- main_Get an Authoring Link Graphic
        '========================================================================
        '
        Public Function web_GetAuthoringLink(ByVal Label As String, ByVal SideCaption As String, ByVal Link As String, ByVal NewWindow As Boolean, Optional ByVal ignore0 As Boolean = False, Optional ByVal Ignore1 As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00340")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentName As String
            Dim MethodName As String
            '
            MethodName = "main_GetAuthoringLink"
            '
            ' Old style non-flyout Authoring Links
            '
            web_GetAuthoringLink = "<div align=""left""><table border=""0"" cellpadding=""1"" cellspacing=""0"" class=""ccAuthoringLink""><tr><td>"
            web_GetAuthoringLink = web_GetAuthoringLink & "<table border=""0"" cellpadding=""1"" cellspacing=""0"" width=""31"" class=""ccAuthoringLink""><tr><td width=""30"" align=""center""><a href=""" & htmlDoc.html_EncodeHTML(Link) & """"
            If NewWindow Then
                web_GetAuthoringLink = web_GetAuthoringLink & " target=""_blank"""
            End If
            web_GetAuthoringLink = web_GetAuthoringLink & " class=""ccAuthoringLink""><span class=""ccAuthoringLink"">" & Label & "</span></a>"
            web_GetAuthoringLink = web_GetAuthoringLink & "<br ><img alt=""space"" src=""/ccLib/image/spacer.gif"" width=""30"" height=""1""></td>"
            If SideCaption <> "" Then
                web_GetAuthoringLink = web_GetAuthoringLink & "<td align=""center"" bgcolor=""#FFFFFF"" width=""1""><nobr><span class=""ccAdminSmall""><font color=""#000000"">&nbsp;" & SideCaption & "&nbsp;</font></span></nobr></td>"
            End If
            web_GetAuthoringLink = web_GetAuthoringLink & "</tr></table></td></tr></table></div>"
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18(MethodName)
            '
        End Function
        '        '
        '        '=============================================================================
        '        ' Returns the connection string for a datasource
        '        '=============================================================================
        '        '
        '        Public Function main_GetConnectionString(ByVal DataSourceName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00342")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_GetConnectionString = GetConnectionString(genericController.encodeText(DataSourceName))
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetConnectionString")
        'End Function
        '
        '
        '
        Public Function GetSortMethodByID(ByVal SortMethodID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00352")
            '
            Dim CS As Integer
            '
            'If Not (true) Then Exit Function
            '
            If SortMethodID > 0 Then
                CS = csOpenRecord("Sort Methods", SortMethodID)
                If db.cs_ok(CS) Then
                    GetSortMethodByID = db.cs_getText(CS, "OrderByClause")
                End If
                Call db.cs_Close(CS)
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetSortMethodByID")
        End Function

        '
        '=============================================================================
        '   Report
        '=============================================================================
        '
        Public Function main_GetReport(ByVal RowCount As Integer, ByVal ColCaption() As String, ByVal ColAlign() As String, ByVal ColWidth() As String, ByVal Cells As String(,), ByVal PageSize As Integer, ByVal PageNumber As Integer, ByVal PreTableCopy As String, ByVal PostTableCopy As String, ByVal DataRowCount As Integer, ByVal ClassStyle As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetReport")
            '
            Dim Adminui As New adminUIController(Me)
            '
            main_GetReport = Adminui.GetReport(RowCount, ColCaption, ColAlign, ColWidth, Cells, PageSize, PageNumber, PreTableCopy, PostTableCopy, DataRowCount, ClassStyle)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetReport")
        End Function
        '
        '
        '
        Public Function main_cs_get2Text(ByVal CSPointer As Integer, ByVal FieldName As String) As String
            main_cs_get2Text = db.cs_get(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName))
        End Function
        '        '
        '        '=============================================================================================
        '        '   main_SetCS
        '        '       Saves the value, encoded correctly for the field type
        '        '=============================================================================================
        '        '
        '        Public Sub app.SetCS(ByVal CSPointer As Integer, ByVal FieldName As String, ByVal FieldValue As String)
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetCS")
        '            '
        '            'If Not (true) Then Exit Sub
        '            '
        '            Call app.SetCS(genericController.EncodeInteger(CSPointer), genericController.encodeText(FieldName), FieldValue)
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_SetCS")
        '        End Sub
        '
        '=============================================================================================
        '   main_GetAutoSite
        '
        '       Site created from BID
        '           bid specifies the PageContent record
        '           PageContent specifies the HTMLTemplate
        '           HTMLTemplate has replacable elements for each content.
        '               replacements are AC tags
        '                   - images are already done out of Resources
        '                   - Add-ons done
        '                       - Add 'server-side' to Add-on so use the .execute function)
        '=============================================================================================
        '
        Public Function main_GetAutoSite() As String
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError23("Deprecated Method: main_GetAutoSite")
        End Function
        '
        '=============================================================================================
        '   main_GetAutoSite_Template
        '
        '       Site created from BID
        '           bid specifies the PageContent record
        '           PageContent specifies the HTMLTemplate
        '           HTMLTemplate has replacable elements for each content.
        '               replacements are AC tags
        '                   - images are already done out of Resources
        '                   - Add-ons done
        '                       - Add 'server-side' to Add-on so use the .execute function)
        '=============================================================================================
        '
        Private Function main_GetAutoSite_Template(ByVal templateId As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00389")
            '
            'If Not (true) Then Exit Function
            '
            Dim CS As Integer
            Dim BakeName As String
            Dim StyleSheetID As Integer
            Dim StyleSheetCopy As String
            '
            BakeName = "AutoSiteTemplate" & templateId
            main_GetAutoSite_Template = genericController.encodeText(cache.getObject(Of String)(BakeName))
            If main_GetAutoSite_Template = "" Then
                If templateId = 0 Then
                    '
                    ' No template Specified, Generate something to host the content
                    '
                    main_GetAutoSite_Template = "" _
                        & vbCrLf & main_docType _
                        & vbCrLf & "<html>" _
                        & cr & "<head>" _
                        & cr2 & "<STYLE type=text/css></STYLE>" _
                        & cr & "</head>" _
                        & cr & "<body>" _
                        & vbCrLf & "<ac Type=""PageBody"">" _
                        & cr & "</body>" _
                        & vbCrLf & "</html>"
                Else
                    '
                    ' Template Specified
                    '
                    CS = csOpen("AutoSite Templates", templateId, , , "Copy,StyleSheetID")
                    If db.cs_ok(CS) Then
                        main_GetAutoSite_Template = db.cs_get(CS, "Copy")
                        StyleSheetID = genericController.EncodeInteger(db.cs_get(CS, "StyleSheetID"))
                    End If
                    Call db.cs_Close(CS)
                    '
                    ' StyleSheet Specified
                    '
                    If StyleSheetID <> 0 Then
                        CS = csOpen("AutoSite Styles", StyleSheetID, , , "Copy")
                        If db.cs_ok(CS) Then
                            StyleSheetCopy = db.cs_get(CS, "Copy")
                        End If
                        Call db.cs_Close(CS)
                    End If
                    '
                    ' Assemble Template
                    '
                End If
                Call cache.setObject(BakeName, main_GetAutoSite_Template, "AutoSite Templates")
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetAutoSite_Template")
        End Function
        '
        '=============================================================================
        '   Sets the MetaContent subsystem so the next call to main_GetLastMeta... returns the correct value
        '       And neither takes much time
        '=============================================================================
        '
        Public Sub main_SetMetaContent(ByVal ContentID As Integer, ByVal RecordID As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetMetaContent")
            '
            Dim KeywordList As String
            Dim CS As Integer
            Dim Criteria As String
            Dim SQL As String
            Dim FieldList As String
            Dim iContentID As Integer
            Dim iRecordID As Integer
            Dim MetaContentID As Integer
            '
            iContentID = genericController.EncodeInteger(ContentID)
            iRecordID = genericController.EncodeInteger(RecordID)
            If (iContentID <> 0) And (iRecordID <> 0) Then
                '
                ' main_Get ID, Description, Title
                '
                Criteria = "(ContentID=" & iContentID & ")and(RecordID=" & iRecordID & ")"
                If False Then '.3.550" Then
                    FieldList = "ID,Name,MetaDescription,'' as OtherHeadTags,'' as MetaKeywordList"
                ElseIf False Then '.3.930" Then
                    FieldList = "ID,Name,MetaDescription,OtherHeadTags,'' as MetaKeywordList"
                Else
                    FieldList = "ID,Name,MetaDescription,OtherHeadTags,MetaKeywordList"
                End If
                CS = db.cs_open("Meta Content", Criteria, , , , ,, FieldList)
                If db.cs_ok(CS) Then
                    MetaContentID = db.cs_getInteger(CS, "ID")
                    Call htmlDoc.main_AddPagetitle2(htmlDoc.html_EncodeHTML(db.cs_getText(CS, "Name")), "page content")
                    Call htmlDoc.main_addMetaDescription2(htmlDoc.html_EncodeHTML(db.cs_getText(CS, "MetaDescription")), "page content")
                    Call htmlDoc.main_AddHeadTag2(db.cs_getText(CS, "OtherHeadTags"), "page content")
                    If True Then
                        KeywordList = genericController.vbReplace(db.cs_getText(CS, "MetaKeywordList"), vbCrLf, ",")
                    End If
                    'main_MetaContent_Title = encodeHTML(app.csv_cs_getText(CS, "Name"))
                    'htmldoc.main_MetaContent_Description = encodeHTML(app.csv_cs_getText(CS, "MetaDescription"))
                    'main_MetaContent_OtherHeadTags = app.csv_cs_getText(CS, "OtherHeadTags")
                End If
                Call db.cs_Close(CS)
                '
                ' main_Get Keyword List
                '
                SQL = "select ccMetaKeywords.Name" _
                    & " From ccMetaKeywords" _
                    & " LEFT JOIN ccMetaKeywordRules on ccMetaKeywordRules.MetaKeywordID=ccMetaKeywords.ID" _
                    & " Where ccMetaKeywordRules.MetaContentID=" & MetaContentID
                CS = db.cs_openSql(SQL)
                Do While db.cs_ok(CS)
                    KeywordList = KeywordList & "," & db.cs_getText(CS, "Name")
                    Call db.cs_goNext(CS)
                Loop
                If KeywordList <> "" Then
                    If Left(KeywordList, 1) = "," Then
                        KeywordList = Mid(KeywordList, 2)
                    End If
                    'KeyWordList = Mid(KeyWordList, 2)
                    KeywordList = htmlDoc.html_EncodeHTML(KeywordList)
                    Call htmlDoc.main_addMetaKeywordList2(KeywordList, "page content")
                End If
                Call db.cs_Close(CS)
                'htmldoc.main_MetaContent_KeyWordList = encodeHTML(KeyWordList)
            End If

            'MetaContentID = 0
            'main_MetaContent_Title = ""
            'htmldoc.main_MetaContent_Description = ""
            'main_MetaContent_OtherHeadTags = ""
            'htmldoc.main_MetaContent_KeyWordList = ""
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_SetMetaContent")
        End Sub
        '
        '=============================================================================
        '   Returns Meta Data
        '=============================================================================
        '
        Public Function main_GetLastMetaTitle() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00392")
            '
            main_GetLastMetaTitle = htmlDoc.main_MetaContent_Title
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLastMetaTitle")
        End Function
        '
        '=============================================================================
        '   Returns Meta Data
        '=============================================================================
        '
        Public Function main_GetLastMetaDescription() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLastMetaDescription")
            '
            main_GetLastMetaDescription = htmlDoc.main_MetaContent_Description
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLastMetaDescription")
        End Function
        '
        '=============================================================================
        '   Returns Meta Data
        '=============================================================================
        '
        Public Function main_GetLastOtherHeadTags() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLastOtherHeadTags")
            '
            main_GetLastOtherHeadTags = htmlDoc.main_MetaContent_OtherHeadTags
            htmlDoc.main_MetaContent_OtherHeadTags = ""
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLastOtherHeadTags")
        End Function
        '
        '=============================================================================
        '   Returns Meta Data
        '=============================================================================
        '
        Public Function main_GetLastMetaKeywordList() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLastMetaKeywordList")
            '
            main_GetLastMetaKeywordList = htmlDoc.main_MetaContent_KeyWordList
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLastMetaKeywordList")
        End Function
        '
        '
        '
        Public Property main_SQLCommandTimeout() As Integer
            Get
                Return db.sqlCommandTimeout
            End Get
            Set(ByVal value As Integer)
                db.sqlCommandTimeout = value
            End Set
        End Property
        '
        '
        '
        Public ReadOnly Property responseRedirect() As String
            Get
                Return webServer.webServerIO_bufferRedirect
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseHeader() As String
            Get
                Return webServer.webServerIO_bufferResponseHeader
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseCookies() As String
            Get
                Return webServer.webServerIO_bufferCookies
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseContentType() As String
            Get
                Return webServer.webServerIO_bufferContentType
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseStatus() As String
            Get
                Return webServer.webServerIO_bufferResponseStatus
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property responseBuffer() As String
            Get
                Return htmlDoc.docBuffer
            End Get
        End Property

        '
        '
        '
        Sub main_EncodePage_SplitBody(ByVal PageSource As String, ByVal PageSourceBody As String, ByVal PageSourcePreBody As String, ByVal PageSourcePostBody As String)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00408")
            '
            'If Not (true) Then Exit Sub
            '
            Dim BodyStart As Integer
            Dim BodyEnd As Integer
            '
            BodyStart = genericController.vbInstr(1, PageSource, "<body", vbTextCompare)
            If BodyStart <> 0 Then
                BodyStart = genericController.vbInstr(BodyStart, PageSource, ">", vbTextCompare)
                If BodyStart <> 0 Then
                    BodyStart = BodyStart + 1
                    BodyEnd = genericController.vbInstr(BodyStart, PageSource, "</body", vbTextCompare)
                    If BodyEnd <> 0 Then
                        PageSourceBody = Mid(PageSource, BodyStart, BodyEnd - BodyStart)
                        PageSourcePreBody = Left(PageSource, BodyStart - 1)
                        PageSourcePostBody = Mid(PageSource, BodyEnd)
                    End If
                End If
            End If
            '
            Exit Sub
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_EncodePage_SplitBody")
        End Sub
        '
        '
        '
        Public Function main_GetBody(ByVal HTMLDoc As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00409")
            '
            Dim ignore0 As String
            Dim Ignore1 As String
            '
            Call main_EncodePage_SplitBody(HTMLDoc, main_GetBody, ignore0, Ignore1)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_EncodePage_SplitBody")
        End Function
        '
        '
        '
        Public ReadOnly Property main_ServerStyleTag() As String
            Get
                Return pages.pageManager_GetStyleTagPublic()
            End Get
        End Property
        '
        ' assemble all the html parts
        '
        Public Function main_assembleHtmlDoc(ByVal docType As String, ByVal head As String, ByVal bodyTag As String, ByVal Body As String) As String
            main_assembleHtmlDoc = "" _
                & docType _
                & vbCrLf & "<html>" _
                & cr & "<head>" _
                & genericController.kmaIndent(head) _
                & cr & "</head>" _
                & cr & bodyTag _
                & genericController.kmaIndent(Body) _
                & cr & "</body>" _
                & vbCrLf & "</html>"
        End Function
        '
        ' main_Get the Head innerHTML for public pages
        '
        Public Function main_GetHTMLHead() As String
            main_GetHTMLHead = webServer.webServerIO_GetHTMLInternalHead(False)
        End Function
        '
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function main_GetRecordID_Internal(ByVal ContentName As String, ByVal RecordName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRecordID_Internal")
            '
            If True Then
                main_GetRecordID_Internal = GetRecordID(genericController.encodeText(ContentName), genericController.encodeText(RecordName))
            End If

            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetRecordID_Internal")
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function main_GetRecordID(ByVal ContentName As String, ByVal RecordName As String) As Integer
            On Error GoTo ErrorTrap
            '
            main_GetRecordID = main_GetRecordID_Internal(genericController.encodeText(ContentName), genericController.encodeText(RecordName))
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetRecordID")
        End Function
        '
        '=============================================================
        '
        '=============================================================
        '
        Public Function content_GetRecordName(ByVal ContentName As String, ByVal RecordID As Integer) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00414")
            '
            content_GetRecordName = GetRecordName(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID))

            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetRecordName")
        End Function
        ''
        ''   Buffered Site Property
        ''
        'Public ReadOnly Property app.dataBuildVersion() As String
        '    Get

        '        If Not app.dataBuildVersion_LocalLoaded Then
        '            app.dataBuildVersion_Local = csv_GetSiteProperty("BuildVersion", "0")
        '            app.dataBuildVersion_LocalLoaded = True
        '        End If
        '        app.dataBuildVersion = app.dataBuildVersion_Local
        '    End Get
        'End Property
        '
        '---------------------------------------------------------------------------
        '   Create the default landing page if it is missing
        '---------------------------------------------------------------------------
        '
        Public Function main_CreatePageGetID(ByVal PageName As String, ByVal ContentName As String, ByVal CreatedBy As Integer, ByVal pageGuid As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("CreatePageGetID")
            '
            Dim CS As Integer
            Dim Id As Integer
            '
            CS = db.cs_insertRecord(ContentName, CreatedBy)
            If db.cs_ok(CS) Then
                Id = db.cs_getInteger(CS, "ID")
                Call db.cs_set(CS, "name", PageName)
                Call db.cs_set(CS, "active", "1")
                If True Then
                    Call db.cs_set(CS, "ccGuid", pageGuid)
                End If
                Call db.cs_save2(CS)
                Call workflow.publishEdit("Page Content", Id)
            End If
            Call db.cs_Close(CS)
            '
            main_CreatePageGetID = Id
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_CreatePageGetID")
        End Function
        '
        '---------------------------------------------------------------------------
        '   Create the default landing page if it is missing
        '       This can only be called when site property AllowAutoHomeSectionOnce is true
        '---------------------------------------------------------------------------
        '
        Private Function main_GetLandingPageID_CreateLandingSectionReturnID(ByVal SectionName As String, ByVal rootPageId As Integer, ByVal SectionContentID As Integer) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLandingPageID_CreateLandingSectionReturnID")
            '
            'If Not (true) Then Exit Function
            '
            Dim SectionTemplateID As Integer
            Dim SectionID As Integer
            Dim DefaultMenuID As Integer
            Dim CS As Integer
            Dim LoopPtr As Integer
            Dim NameOK As Boolean
            Dim SectionNameTry As String
            '
            If (app_errorCount = 0) And siteProperties.getBoolean("AllowAutoHomeSectionOnce") Then
                '
                SectionTemplateID = pages.pageManager_LoadTemplateGetID(0)
                '
                ' main_Get a unique section name
                '
                SectionNameTry = SectionName
                NameOK = False
                LoopPtr = 0
                Do While Not NameOK And (LoopPtr < 10)
                    If LoopPtr <> 0 Then
                        SectionNameTry = SectionName & " " & (LoopPtr + 1)
                    End If
                    CS = db.cs_open("Site Sections", "name=" & db.encodeSQLText(SectionNameTry), , ,, , , "ID")
                    NameOK = Not db.cs_ok(CS)
                    Call db.cs_Close(CS)
                    LoopPtr = LoopPtr + 1
                Loop
                '
                CS = db.cs_insertRecord("Site Sections")
                If db.cs_ok(CS) Then
                    SectionID = db.cs_getInteger(CS, "ID")
                    Call db.cs_set(CS, "Name", SectionNameTry)
                    Call db.cs_set(CS, "Caption", DefaultLandingSectionName)
                    Call db.cs_set(CS, "SortOrder", -1)
                    Call db.cs_set(CS, "HideMenu", False)
                    Call db.cs_set(CS, "contentid", SectionContentID)
                    Call db.cs_set(CS, "TemplateID", SectionTemplateID)
                    If True Then
                        Call db.cs_set(CS, "RootPageID", rootPageId)
                    End If
                    If True Then
                        Call db.cs_set(CS, "ccGuid", DefaultLandingSectionGuid)
                    End If
                End If
                Call db.cs_Close(CS)
                Call pages.pageManager_cache_siteSection_clear()
                '
                ' main_Get the Default Menu ID
                '
                CS = db.cs_open("Dynamic Menus", "(name='Default')", "ID", , ,, , "ID")
                If db.cs_ok(CS) Then
                    DefaultMenuID = db.cs_getInteger(CS, "ID")
                End If
                Call db.cs_Close(CS)
                '
                ' Add the new landing section to the default menu
                '
                CS = db.cs_insertRecord("Dynamic Menu Section Rules")
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "DynamicMenuID", DefaultMenuID)
                    Call db.cs_set(CS, "SectionID", SectionID)
                End If
                Call db.cs_Close(CS)
                '
                main_GetLandingPageID_CreateLandingSectionReturnID = SectionID
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLandingPageID_CreateLandingSectionReturnID")
        End Function
        '
        '
        '
        Public Property main_MetaContentNoFollow() As Boolean
            Get
                Return webServer.webServerIO_response_NoFollow
            End Get
            Set(ByVal value As Boolean)
                webServer.webServerIO_response_NoFollow = value
            End Set
        End Property
        '
        '
        '
        Public Sub main_RollBackCS(ByVal CSPointer As Integer)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Proc00419")
            '
            'If Not (true) Then Exit Sub
            '
            Call db.cs_rollBack(genericController.EncodeInteger(CSPointer))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_RollBackCS")
        End Sub

        '
        '
        '
        Public Function main_isSectionBlocked(ByVal SectionID As Integer, ByVal AllowSectionBlocking As Boolean) As Boolean
            On Error GoTo ErrorTrap 'Const Tn = "isSectionBlocked" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Dim CS As Integer
            Dim SQL As String
            Dim SQLWhere As String
            '
            main_isSectionBlocked = False
            If AllowSectionBlocking Then
                main_isSectionBlocked = True
                If authContext.isAuthenticatedAdmin(Me) Then
                    '
                    ' Admin always main_Gets in
                    '
                    main_isSectionBlocked = False
                ElseIf Not authContext.isAuthenticated() Then
                    '
                    ' not authenticated never main_Gets in
                    '
                Else
                    '
                    ' check if this member is in one of the SectionRule groups
                    '
                    SQLWhere = "" _
                        & " M.MemberID=" & authContext.user.ID _
                        & " and R.SectionID=" & SectionID _
                        & " and M.GroupID=R.GroupID" _
                        & " and R.Active<>0" _
                        & " and M.Active<>0" _
                        & " and ((M.DateExpires is null)or(M.DateExpires>" & db.encodeSQLDate(app_startTime) & "))"
                    SQL = GetSQLSelect("", "ccmemberRules M,ccSectionBlockRules R", "M.ID", SQLWhere, , , 1)
                    CS = db.cs_openSql(SQL)
                    'SQL = "select ID" _
                    '    & " from ccMemberRules M,ccSectionBlockRules R" _
                    '    & " where M.MemberID=" & memberID _
                    '    & " and R.SectionID=" & SectionID _
                    '    & " and M.GroupID=R.GroupID" _
                    '    & " and R.Active<>0" _
                    '    & " and M.Active<>0" _
                    '    & " and ((M.DateExpires is null)or(M.DateExpires>" & main_SQlPageStartTime & "))"
                    'CS = app.openCsSql_rev("default", SQL, 1, 1)
                    'SQL = "select top 1 *" _
                    '    & " from ccMemberRules M,ccSectionBlockRules R" _
                    '    & " where M.MemberID=" & memberID _
                    '    & " and R.SectionID=" & SectionID _
                    '    & " and M.GroupID=R.GroupID" _
                    '    & " and R.Active<>0" _
                    '    & " and M.Active<>0" _
                    '    & " and ((M.DateExpires is null)or(M.DateExpires>" & main_SQlPageStartTime & "))"
                    'CS = app.openCsSql(SQL)
                    main_isSectionBlocked = Not (db.cs_ok(CS))
                    Call db.cs_Close(CS)
                End If
            End If

            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_isSectionBlocked")
        End Function
        '
        '
        '
        Public Sub main_RequestTask(ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String)
            On Error GoTo ErrorTrap 'Const Tn = "RequestTask" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
            '
            Call tasks_RequestTask(genericController.encodeText(Command), genericController.encodeText(SQL), genericController.encodeText(ExportName), genericController.encodeText(Filename), genericController.EncodeInteger(authContext.user.ID))
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_RequestTask")
        End Sub
        '
        '=============================================================================
        '   Returns the link to the page that contains the record designated by the ContentRecordKey
        '       Returns DefaultLink if it can not be determined
        '=============================================================================
        '
        Public Function main_GetLinkByContentRecordKey(ByVal ContentRecordKey As String, Optional ByVal DefaultLink As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetLinkByContentRecordKey")
            '
            'If Not (true) Then Exit Function
            '
            Dim CSPointer As Integer
            Dim KeySplit() As String
            Dim ContentID As Integer
            Dim RecordID As Integer
            Dim ContentName As String
            Dim templateId As Integer
            Dim ParentID As Integer
            Dim DefaultTemplateLink As String
            Dim TableName As String
            Dim DataSource As String
            Dim ParentContentID As Integer
            Dim recordfound As Boolean
            '
            If ContentRecordKey <> "" Then
                '
                ' First try main_ContentWatch table for a link
                '
                CSPointer = db.cs_open("Content Watch", "ContentRecordKey=" & db.encodeSQLText(ContentRecordKey), , , ,, , "Link,Clicks")
                If db.cs_ok(CSPointer) Then
                    main_GetLinkByContentRecordKey = db.cs_getText(CSPointer, "Link")
                End If
                Call db.cs_Close(CSPointer)
                '
                If main_GetLinkByContentRecordKey = "" Then
                    '
                    ' try template for this page
                    '
                    KeySplit = Split(ContentRecordKey, ".")
                    If UBound(KeySplit) = 1 Then
                        ContentID = genericController.EncodeInteger(KeySplit(0))
                        If ContentID <> 0 Then
                            ContentName = metaData.getContentNameByID(ContentID)
                            RecordID = genericController.EncodeInteger(KeySplit(1))
                            If ContentName <> "" And RecordID <> 0 Then
                                If GetContentTablename(ContentName) = "ccPageContent" Then
                                    CSPointer = csOpen(ContentName, RecordID, , , "TemplateID,ParentID")
                                    If db.cs_ok(CSPointer) Then
                                        recordfound = True
                                        templateId = db.cs_getInteger(CSPointer, "TemplateID")
                                        ParentID = db.cs_getInteger(CSPointer, "ParentID")
                                    End If
                                    Call db.cs_Close(CSPointer)
                                    If Not recordfound Then
                                        '
                                        ' This content record does not exist - remove any records with this ContentRecordKey pointer
                                        '
                                        'Call app.DeleteContentRecords("Topic Rules", "ContentRecordKey=" & encodeSQLText(ContentRecordKey))
                                        'Call app.DeleteContentRecords("Topic Habits", "ContentRecordKey=" & encodeSQLText(ContentRecordKey))
                                        Call db.deleteContentRecords("Content Watch", "ContentRecordKey=" & db.encodeSQLText(ContentRecordKey))
                                        Call metaData_DeleteContentTracking(ContentName, RecordID, True)
                                    Else

                                        If templateId <> 0 Then
                                            CSPointer = csOpen("Page Templates", templateId, , , "Link")
                                            If db.cs_ok(CSPointer) Then
                                                main_GetLinkByContentRecordKey = db.cs_getText(CSPointer, "Link")
                                            End If
                                            Call db.cs_Close(CSPointer)
                                        End If
                                        If main_GetLinkByContentRecordKey = "" And ParentID <> 0 Then
                                            TableName = GetContentTablename(ContentName)
                                            DataSource = main_GetContentDataSource(ContentName)
                                            CSPointer = db.cs_openCsSql_rev(DataSource, "Select ContentControlID from " & TableName & " where ID=" & RecordID)
                                            If db.cs_ok(CSPointer) Then
                                                ParentContentID = genericController.EncodeInteger(db.cs_getText(CSPointer, "ContentControlID"))
                                            End If
                                            Call db.cs_Close(CSPointer)
                                            If ParentContentID <> 0 Then
                                                main_GetLinkByContentRecordKey = main_GetLinkByContentRecordKey(CStr(ParentContentID & "." & ParentID), "")
                                            End If
                                        End If
                                        If main_GetLinkByContentRecordKey = "" Then
                                            DefaultTemplateLink = siteProperties.getText("SectionLandingLink", requestAppRootPath & siteProperties.serverPageDefault)
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                    If main_GetLinkByContentRecordKey <> "" Then
                        main_GetLinkByContentRecordKey = genericController.modifyLinkQuery(main_GetLinkByContentRecordKey, "bid", CStr(RecordID), True)
                    End If
                End If
            End If
            '
            If main_GetLinkByContentRecordKey = "" Then
                main_GetLinkByContentRecordKey = DefaultLink
            End If
            '
            main_GetLinkByContentRecordKey = genericController.EncodeAppRootPath(main_GetLinkByContentRecordKey, webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, webServer.requestDomain)
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetLinkByContentRecordKey")
        End Function
        '
        '============================================================================================================
        '   the content control Id for a record, all its edit and archive records, and all its child records
        '   returns records affected
        '   the contentname contains the record, but we do not know that this is the contentcontrol for the record,
        '   read it first to main_Get the correct contentid
        '============================================================================================================
        '
        Public Function content_SetContentControl(ByVal ContentID As Integer, ByVal RecordID As Integer, ByVal NewContentControlID As Integer, Optional ByVal UsedIDString As String = "") As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SetContentControl")
            '
            Dim SQL As String
            Dim CS As Integer
            Dim RecordTableName As String
            Dim AuthoringTableName As String
            Dim ContentName As String
            Dim HasParentID As Boolean
            Dim RecordContentID As Integer
            Dim RecordContentName As String
            Dim DataSourceName As String
            '
            If Not genericController.IsInDelimitedString(UsedIDString, CStr(RecordID), ",") Then
                ContentName = metaData.getContentNameByID(ContentID)
                CS = csOpen(ContentName, RecordID, False, False)
                If db.cs_ok(CS) Then
                    HasParentID = db.cs_isFieldSupported(CS, "ParentID")
                    RecordContentID = db.cs_getInteger(CS, "ContentControlID")
                    RecordContentName = metaData.getContentNameByID(RecordContentID)
                End If
                Call db.cs_Close(CS)
                If RecordContentName <> "" Then
                    '
                    '
                    '
                    DataSourceName = main_GetContentDataSource(RecordContentName)
                    RecordTableName = GetContentTablename(RecordContentName)
                    '
                    ' either Workflow on non-workflow - it changes everything
                    '
                    SQL = "update " & RecordTableName & " set ContentControlID=" & NewContentControlID & " where ID=" & RecordID & " or EditSourceID=" & RecordID
                    Call db.executeSql(SQL, DataSourceName)
                    If HasParentID Then
                        SQL = "select contentcontrolid,ID from " & RecordTableName & " where ParentID=" & RecordID
                        CS = db.cs_openCsSql_rev(DataSourceName, SQL)
                        Do While db.cs_ok(CS)
                            Call content_SetContentControl(db.cs_getInteger(CS, "contentcontrolid"), db.cs_getInteger(CS, "ID"), NewContentControlID, UsedIDString & "," & RecordID)
                            db.cs_goNext(CS)
                        Loop
                        Call db.cs_Close(CS)
                    End If
                    '
                    ' fix content watch
                    '
                    SQL = "update ccContentWatch set ContentID=" & NewContentControlID & ", ContentRecordKey='" & NewContentControlID & "." & RecordID & "' where ContentID=" & ContentID & " and RecordID=" & RecordID
                    Call db.executeSql(SQL)
                    '            '
                    '            ' fix Topic Rules
                    '            '
                    '            SQL = "update ccTopicRules set ContentID=" & NewContentControlID & ", ContentRecordKey='" & NewContentControlID & "." & RecordID & "' where ContentID=" & ContentID & " and RecordID=" & RecordID
                    '            Call app.executeSql( SQL)
                End If

            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_SetContentControl")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function menu_VerifyDynamicMenu(ByVal MenuName As String) As Integer
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("VerifyDynamicMenu")
            '
            'If Not (true) Then Exit Function
            '
            menu_VerifyDynamicMenu = csv_VerifyDynamicMenu(genericController.encodeText(MenuName))
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_VerifyDynamicMenu")
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function menu_GetDynamicMenuACSelect() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetDynamicMenuACSelect")
            '
            'If Not (true) Then Exit Function
            '
            menu_GetDynamicMenuACSelect = csv_GetDynamicMenuACSelect()
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetDynamicMenuACSelect")
        End Function
        '
        ' converted to add-on
        '
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'public Function main_GetDynamicFormACSelect() As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("GetDynamicFormACSelect")
        '    '
        '    'If Not (true) Then Exit Function
        '    '
        '    main_GetDynamicFormACSelect = csv_GetDynamicFormACSelect
        '    '
        '    Exit Function
        'ErrorTrap:
        '    Call main_HandleClassErrorAndResume_TrapPatch1("main_GetDynamicFormACSelect")
        'End Function
        ''
        ''========================================================================
        ''
        ''========================================================================
        ''
        'public Function Mergetemplate(EncodedTemplateHTML As String, EncodedContentHTML As String) As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("Mergetemplate")
        '    '
        '    'If Not (true) Then Exit Function
        '    '
        '    Mergetemplate = csv_Mergetemplate(EncodedTemplateHTML, EncodedContentHTML, memberID)
        '    '
        '    Exit Function
        'ErrorTrap:
        '    Call main_HandleClassErrorAndResume_TrapPatch1("Mergetemplate")
        'End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Function admin_GetAdminFormBody(Caption As String, ButtonListLeft As String, ButtonListRight As String, AllowAdd As Boolean, AllowDelete As Boolean, Description As String, ContentSummary As String, ContentPadding As Integer, Content As String) As String
            Dim Adminui As New adminUIController(Me)
            '
            admin_GetAdminFormBody = Adminui.GetBody(Caption, ButtonListLeft, ButtonListRight, AllowAdd, AllowDelete, Description, ContentSummary, ContentPadding, Content)
        End Function
        '
        ' Verify Registration Form Page
        '
        Public Sub main_VerifyRegistrationFormPage()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_VerifyRegistrationFormPage")
            '
            Dim CS As Integer
            Dim GroupNameList As String
            Dim Copy As String
            '
            Call db.deleteContentRecords("Form Pages", "name=" & db.encodeSQLText("Registration Form"))
            CS = db.cs_open("Form Pages", "name=" & db.encodeSQLText("Registration Form"))
            If Not db.cs_ok(CS) Then
                '
                ' create Version 1 template - just to main_Get it started
                '
                Call db.cs_Close(CS)
                GroupNameList = "Registered"
                CS = db.cs_insertRecord("Form Pages")
                If db.cs_ok(CS) Then
                    Call db.cs_set(CS, "name", "Registration Form")
                    Copy = "" _
                        & vbCrLf & "<table border=""0"" cellpadding=""2"" cellspacing=""0"" width=""100%"">" _
                        & vbCrLf & "{{REPEATSTART}}<tr><td align=right style=""height:22px;"">{{CAPTION}}&nbsp;</td><td align=left>{{FIELD}}</td></tr>{{REPEATEND}}" _
                        & vbCrLf & "<tr><td align=right><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=135 height=1></td><td width=""100%"">&nbsp;</td></tr>" _
                        & vbCrLf & "<tr><td colspan=2>&nbsp;<br>" & main_GetPanelButtons(ButtonRegister, "Button") & "</td></tr>" _
                        & vbCrLf & "</table>"
                    Call db.cs_set(CS, "Body", Copy)
                    Copy = "" _
                        & "1" _
                        & vbCrLf & GroupNameList _
                        & vbCrLf & "true" _
                        & vbCrLf & "1,First Name,true,FirstName" _
                        & vbCrLf & "1,Last Name,true,LastName" _
                        & vbCrLf & "1,Email Address,true,Email" _
                        & vbCrLf & "1,Phone,true,Phone" _
                        & vbCrLf & "2,Please keep me informed of news and events,false,Subscribers" _
                        & ""
                    Call db.cs_set(CS, "Instructions", Copy)
                End If
            End If
            Call db.cs_Close(CS)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_VerifyRegistrationFormPage")
        End Sub
        '
        ' Public Method to main_Get Contact Manager
        '
        Public Function contactManager_GetContactManager(Option_String As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetContactManager")
            '
            contactManager_GetContactManager = addon.execute_legacy5(0, "Contact Manager", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", 0)
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetContactManager")
        End Function
        '
        ' This is a copy of the routine in CSrv -- duplicated so I do not have to make a public until the interface is worked-out
        '
        Public Function GetSQLSelect(ByVal DataSourceName As String, ByVal From As String, Optional ByVal FieldList As String = "", Optional ByVal Where As String = "", Optional ByVal OrderBy As String = "", Optional ByVal GroupBy As String = "", Optional ByVal RecordLimit As Integer = 0) As String
            On Error GoTo ErrorTrap
            '
            'If Not (true) Then Exit Function
            '
            Dim SQL As String
            '
            Select Case db.getDataSourceType(DataSourceName)
                Case DataSourceTypeODBCMySQL
                    SQL = "SELECT"
                    SQL &= " " & FieldList
                    SQL &= " FROM " & From
                    If Where <> "" Then
                        SQL &= " WHERE " & Where
                    End If
                    If OrderBy <> "" Then
                        SQL &= " ORDER BY " & OrderBy
                    End If
                    If GroupBy <> "" Then
                        SQL &= " GROUP BY " & GroupBy
                    End If
                    If RecordLimit <> 0 Then
                        SQL &= " LIMIT " & RecordLimit
                    End If
                Case Else
                    SQL = "SELECT"
                    If RecordLimit <> 0 Then
                        SQL &= " TOP " & RecordLimit
                    End If
                    If FieldList = "" Then
                        SQL &= " *"
                    Else
                        SQL &= " " & FieldList
                    End If
                    SQL &= " FROM " & From
                    If Where <> "" Then
                        SQL &= " WHERE " & Where
                    End If
                    If OrderBy <> "" Then
                        SQL &= " ORDER BY " & OrderBy
                    End If
                    If GroupBy <> "" Then
                        SQL &= " GROUP BY " & GroupBy
                    End If
            End Select
            GetSQLSelect = SQL
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetSQLSelect")
        End Function
        '
        '
        '
        Public Sub cs_setFormInput(ByVal CSPointer As Integer, ByVal FieldName As String, Optional ByVal RequestName As String = "")
            On Error GoTo ErrorTrap
            '
            Dim LocalRequestName As String
            Dim Filename As String
            Dim Path As String
            '
            'If Not (true) Then Exit Sub
            '
            If Not db.cs_ok(CSPointer) Then
                Throw New ApplicationException("ContentSetPointer is invalid, empty, or end-of-file")
            ElseIf Trim(FieldName) = "" Then
                Throw New ApplicationException("FieldName is invalid or blank")
            Else
                LocalRequestName = RequestName
                If LocalRequestName = "" Then
                    LocalRequestName = FieldName
                End If
                Select Case db.cs_getFieldTypeId(CSPointer, FieldName)
                    Case FieldTypeIdBoolean
                        '
                        ' Boolean
                        '
                        Call db.cs_set(CSPointer, FieldName, docProperties.getBoolean(LocalRequestName))
                    Case FieldTypeIdCurrency, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdManyToMany
                        '
                        ' Numbers
                        '
                        Call db.cs_set(CSPointer, FieldName, docProperties.getNumber(LocalRequestName))
                    Case FieldTypeIdDate
                        '
                        ' Date
                        '
                        Call db.cs_set(CSPointer, FieldName, docProperties.getDate(LocalRequestName))
                    Case FieldTypeIdFile, FieldTypeIdFileImage
                        '
                        '
                        '
                        Filename = docProperties.getText(LocalRequestName)
                        If Filename <> "" Then
                            Path = db.cs_getFilename(CSPointer, FieldName, Filename)
                            Call db.cs_set(CSPointer, FieldName, Path)
                            Path = genericController.vbReplace(Path, "\", "/")
                            Path = genericController.vbReplace(Path, "/" & Filename, "")
                            Call appRootFiles.saveUpload(LocalRequestName, Path, Filename)
                        End If
                    Case Else
                        '
                        ' text files
                        '
                        Call db.cs_set(CSPointer, FieldName, docProperties.getText(LocalRequestName))
                End Select
            End If

            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_SetCSFormInput")
        End Sub
        '
        '
        '
        Private Function admin_GetStyleTagAdmin() As String
            Dim StyleSN As Integer
            '
            StyleSN = genericController.EncodeInteger(siteProperties.getText("StylesheetSerialNumber", "0"))
            If StyleSN = 0 Then
                admin_GetStyleTagAdmin = cr & StyleSheetStart & pages.pageManager_GetStyleSheetDefault() & cr & StyleSheetEnd
            ElseIf (siteProperties.dataBuildVersion <> codeVersion()) Then
                admin_GetStyleTagAdmin = cr & "<!-- styles forced inline because database upgrade needed -->" & StyleSheetStart & pages.pageManager_GetStyleSheetDefault() & cr & StyleSheetEnd
            Else
                If StyleSN < 0 Then
                    '
                    ' Linked Styles
                    ' Bump the Style Serial Number so next fetch is not cached
                    '
                    StyleSN = 1
                    Call siteProperties.setProperty("StylesheetSerialNumber", CStr(StyleSN))
                    '
                    ' Save new public stylesheet
                    '
                    'Dim kmafs As New fileSystemClass
                    Call cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Public" & StyleSN & ".css"), csv_getStyleSheetProcessed)
                    Call cdnFiles.saveFile(genericController.convertCdnUrlToCdnPathFilename("templates\Admin" & StyleSN & ".css"), htmlDoc.pageManager_GetStyleSheetDefault2)
                End If
                admin_GetStyleTagAdmin = cr & "<link rel=""stylesheet"" type=""text/css"" href=""" & webServer.webServerIO_requestProtocol & webServer.webServerIO_requestDomain & csv_getVirtualFileLink(serverConfig.appConfig.cdnFilesNetprefix, "templates/Admin" & StyleSN & ".css") & """ >"
            End If
        End Function
        '
        '=======================================================================================================================================
        '   LinkAlias cache
        '=======================================================================================================================================
        '
        Public Sub cache_linkAlias_load()
            On Error GoTo ErrorTrap
            '
            Dim Key As String
            Dim usedKeys As String = ""
            Dim CS As Integer
            'dim dt as datatable
            Dim Ptr As Integer
            Dim LinkAliasPageID As String
            Dim LinkAliasName As String
            Dim LinkAliasQueryStringSuffix As String
            Dim cacheArray() As Object
            ReDim cacheArray(2)
            Dim cacheTest As Object
            Dim bag As String
            '
            cache_linkAlias_PageIdQSSIndex = New keyPtrController
            cache_linkAlias_NameIndex = New keyPtrController
            cache_linkAliasCnt = 0
            '
            ' Load cache
            '
            On Error Resume Next
            cacheTest = cache.getObject(Of Object())(cache_linkAlias_cacheName)
            If Not pages.pagemanager_IsWorkflowRendering() Then
                If Not IsNothing(cacheTest) Then
                    cacheArray = DirectCast(cacheTest, Object())
                    If Not IsNothing(cacheArray) Then
                        cache_linkAlias = DirectCast(cacheArray(0), String(,))
                        If Not IsNothing(cache_linkAlias) Then
                            bag = DirectCast(cacheArray(1), String)
                            If Err.Number = 0 Then
                                Call cache_linkAlias_PageIdQSSIndex.importPropertyBag(bag)
                                If Err.Number = 0 Then
                                    bag = DirectCast(cacheArray(2), String)
                                    If Err.Number = 0 Then
                                        Call cache_linkAlias_NameIndex.importPropertyBag(bag)
                                        If Err.Number = 0 Then
                                            cache_linkAliasCnt = UBound(cache_linkAlias, 2) + 1
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            If cache_linkAliasCnt = 0 Then
                Dim rs As DataTable
                rs = db.executeSql("select " & cache_linkAlias_fieldList & " from ccLinkAliases where (active<>0) order by id desc")
                If rs.Rows.Count > 0 Then
                    cache_linkAlias_NameIndex = New keyPtrController
                    cache_linkAlias_PageIdQSSIndex = New keyPtrController
                    ReDim cache_linkAlias(rs.Rows.Count, 4)
                    For Each row As DataRow In rs.Rows
                        cache_linkAlias(0, Ptr) = row(0).ToString
                        cache_linkAlias(1, Ptr) = row(1).ToString
                        cache_linkAlias(2, Ptr) = row(2).ToString
                        cache_linkAlias(3, Ptr) = row(3).ToString
                        cache_linkAlias(4, Ptr) = row(4).ToString
                        '
                        LinkAliasName = genericController.encodeText(cache_linkAlias(1, Ptr))
                        LinkAliasPageID = genericController.encodeText(cache_linkAlias(3, Ptr))
                        LinkAliasQueryStringSuffix = genericController.encodeText(cache_linkAlias(4, Ptr))
                        Call cache_linkAlias_NameIndex.setPtr(LCase(LinkAliasName), Ptr)
                        Key = genericController.vbLCase(LinkAliasPageID & LinkAliasQueryStringSuffix)
                        If genericController.vbInstr(1, "," & usedKeys & ",", "," & Key & ",") = 0 Then
                            usedKeys = usedKeys & "," & Key
                            Call cache_linkAlias_PageIdQSSIndex.setPtr(Key, Ptr)
                        End If
                    Next
                End If
                '
                ' Load Index
                '
                If cache_linkAliasCnt > 0 Then
                    cache_linkAlias_NameIndex = New keyPtrController
                    cache_linkAlias_PageIdQSSIndex = New keyPtrController
                    For Ptr = 0 To cache_linkAliasCnt - 1
                        LinkAliasName = genericController.encodeText(cache_linkAlias(1, Ptr))
                        LinkAliasPageID = genericController.encodeText(cache_linkAlias(3, Ptr))
                        LinkAliasQueryStringSuffix = genericController.encodeText(cache_linkAlias(4, Ptr))
                        Call cache_linkAlias_NameIndex.setPtr(LCase(LinkAliasName), Ptr)
                        Key = genericController.vbLCase(LinkAliasPageID & LinkAliasQueryStringSuffix)
                        If genericController.vbInstr(1, "," & usedKeys & ",", "," & Key & ",") = 0 Then
                            usedKeys = usedKeys & "," & Key
                            Call cache_linkAlias_PageIdQSSIndex.setPtr(Key, Ptr)
                        End If
                    Next
                End If
                Call cache_linkAlias_save()
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("cache_linkAlias_load")
        End Sub
        '
        '
        '
        Private Sub cache_linkAlias_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.cache_linkAlias_save")
            '
            Dim hint As String
            Dim cacheArray() As Object
            ReDim cacheArray(2)
            '
            Call cache_linkAlias_PageIdQSSIndex.getPtr("test")
            Call cache_linkAlias_NameIndex.getPtr("test")
            '
            cacheArray(0) = cache_linkAlias
            cacheArray(1) = cache_linkAlias_PageIdQSSIndex.exportPropertyBag
            cacheArray(2) = cache_linkAlias_NameIndex.exportPropertyBag
            Call cache.setObject(cache_linkAlias_cacheName, cacheArray, "link aliases")
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("cache_linkAlias_save")
        End Sub
        '
        '
        '
        Public Sub cache_linkAlias_clear()
            On Error GoTo ErrorTrap 'Const Tn = "cache_linkAlias_clear": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            cache_linkAliasCnt = 0
            cache_linkAlias = {}
            Call cache.setObject(cache_linkAlias_cacheName, cache_linkAlias)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "cache_linkAlias_clear", True)
        End Sub
        '
        '
        '
        Public Function cache_linkAlias_getPtrByPageIdQss(PageID As Integer, QueryStringSuffix As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "cache_linkAlias_getPtrByPageIdQss": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Key As String
            '
            cache_linkAlias_getPtrByPageIdQss = -1
            If cache_linkAliasCnt = 0 Then
                Call cache_linkAlias_load()
            End If
            If cache_linkAliasCnt > 0 Then
                Key = genericController.vbLCase(CStr(PageID) & QueryStringSuffix)
                cache_linkAlias_getPtrByPageIdQss = cache_linkAlias_PageIdQSSIndex.getPtr(Key)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "cache_linkAlias_getPtrByPageIdQss", True)
        End Function
        '
        '
        '
        Public Function cache_linkAlias_getPtrByName(aliasName As String) As Integer
            On Error GoTo ErrorTrap 'Const Tn = "cache_linkAlias_getPtrByName": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            Dim Key As String
            '
            cache_linkAlias_getPtrByName = -1
            If cache_linkAliasCnt = 0 Then
                Call cache_linkAlias_load()
            End If
            If cache_linkAliasCnt > 0 Then
                Key = genericController.vbLCase(aliasName)
                cache_linkAlias_getPtrByName = cache_linkAlias_NameIndex.getPtr(Key)
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "cache_linkAlias_getPtrByName", True)
        End Function
        '
        '====================================================================================================
        '   Returns the Alias link (SourceLink) from the actual link (DestinationLink)
        '
        '====================================================================================================
        '
        Public Function main_GetLinkAliasByPageID(PageID As Integer, QueryStringSuffix As String, DefaultLink As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetLinkAliasByPageID")
            '
            Dim CS As Integer
            Dim Ptr As Integer
            Dim Key As String
            '
            main_GetLinkAliasByPageID = DefaultLink
            If siteProperties.allowLinkAlias Then
                Ptr = cache_linkAlias_getPtrByPageIdQss(PageID, QueryStringSuffix)
                If Ptr >= 0 Then
                    main_GetLinkAliasByPageID = genericController.encodeText(cache_linkAlias(1, Ptr))
                    If Mid(main_GetLinkAliasByPageID, 1, 1) <> "/" Then
                        main_GetLinkAliasByPageID = "/" & main_GetLinkAliasByPageID
                    End If
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetLinkAliasByPageID")
        End Function
        '
        '====================================================================================================
        '   returns the actual link (DestinationLink) from the alias link (SourceLink)
        '
        '====================================================================================================
        '
        Public Function main_GetURLRewriteLink(ByVal linkAlias As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetURLRewriteLink")
            '
            Dim Ptr As Integer
            '
            If (True) And (siteProperties.allowLinkAlias) Then
                If cache_linkAliasCnt = 0 Then
                    Call cache_linkAlias_load()
                End If
                If cache_linkAliasCnt > 0 Then
                    Ptr = cache_linkAlias_NameIndex.getPtr(LCase(linkAlias))
                    main_GetURLRewriteLink = genericController.encodeText(cache_linkAlias(2, Ptr))
                End If
            End If
            '
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetURLRewriteLink")
        End Function
        ''
        ''
        ''
        'public Function main_cs_getv() As ContentServerClass
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("cs_getv")
        '    '
        '    main_cs_getv = main_cmc
        '    '
        '    Exit Function
        'ErrorTrap:
        '    Call main_HandleClassErrorAndResume_TrapPatch1("main_cs_getv")
        'End Function
        '
        '
        '
        '
        '=================================================================================
        '   Legacy
        '       see main_executeAddon for explaination of string parsing
        '
        '       use main_GetAddonOption to main_Get a value from an AddonOptionList
        '       use genericController.decodeNvaArgument( main_GetArgument( name, string, default, "&" )) for AddonOptionStrings
        '=================================================================================
        '
        Public Function main_GetAggrOption(Name As String, Option_String As String) As String
            main_GetAggrOption = main_GetAddonOption(Name, Option_String)
        End Function
        '
        '=================================================================================
        '   True if the table exists
        '=================================================================================
        '
        Public Function main_IsSQLTable(DataSourceName As String, TableName As String) As Boolean
            '
            If (True) Then
                main_IsSQLTable = IsSQLTable(DataSourceName, TableName)
            End If
            '
        End Function
        '
        '=================================================================================
        '   True if the table field exists
        '=================================================================================
        '
        Public Function main_IsSQLTableField(DataSourceName As String, TableName As String, FieldName As String) As Boolean
            '
            If (True) Then
                main_IsSQLTableField = IsSQLTableField(DataSourceName, TableName, FieldName)
            End If
            '
        End Function
        '
        '==========================================================================================================================================
        '   Input element for Style Sheets
        '
        '   Opens a temp file in the appcache folder with the styles copies in
        '   click on one of the styles on the left, and main_Get the right pane with AJAX.
        '   then on the next click, save the results in the right page back to the temp file.
        '   on OK or save, first save the right pane results to the temp file, then copy the temp file to the real file
        '   on cancel, just delete the temp file
        '==========================================================================================================================================
        '
        Public Function main_GetFormInputStyles(ByVal TagName As String, ByVal StyleCopy As String, Optional ByVal HtmlId As String = "", Optional ByVal HtmlClass As String = "") As String
            '
            Dim FieldRows As String
            Dim FieldOptionRow As String
            Dim Copy As String
            '
            Copy = htmlDoc.html_EncodeHTML(StyleCopy)
            main_GetFormInputStyles = htmlDoc.html_GetFormInputTextExpandable2(TagName, StyleCopy, 10, , HtmlId, , , HtmlClass)
            'FieldRows = main_GetMemberProperty("StyleEditorRowHeight", 10)
            'FieldOptionRow = "<input TYPE=""Text"" TabIndex=-1 NAME=""" & TagName & "Rows"" SIZE=""3"" VALUE=""" & FieldRows & """ ID=""""  onchange=""" & TagName & ".rows=" & TagName & "Rows.value; return true""> Rows"
            'main_GetFormInputStyles = "<textarea NAME=""" & TagName & """ ROWS=""" & FieldRows & """ ID=""" & TagName & """ STYLE=""width: 600px;"">" & Copy & "</TEXTAREA>" & FieldOptionRow
            Exit Function
            '
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetFormInputStyles")
            '
            Dim JS As String
            Dim LeftPane As String
            Dim RightPane As String
            Dim BakeName As String
            Dim IsAuthoringMode As Boolean
            Dim LinkBase As String
            Dim RightSideHeader As String
            Dim StyleFile As String
            Dim TempFilename As String
            Dim StyleLine As String
            Dim StyleLines() As String
            Dim StyleNameList As String
            Dim Pos As Integer
            Dim Ptr As Integer
            Dim StyleName As String
            Dim StyleDetails As String
            Dim StyleCnt As Integer
            '
            RightSideHeader = "&nbsp;"
            IsAuthoringMode = True
            LinkBase = web_RefreshQueryString
            LeftPane = "List of Styles"
            RightPane = "Style Tag Editor"
            TempFilename = "AppCache\StyleTemp" & genericController.GetRandomInteger() & ".css"
            '
            StyleFile = cdnFiles.readFile(TempFilename)
            If StyleFile <> "" Then
                Call appRootFiles.saveFile(TempFilename, StyleCopy)
                'Call main_CopyVirtualFile(StylesFilename, TempFilename)
                '
                ' remove crlf
                '
                StyleFile = genericController.vbReplace(StyleFile, vbCrLf, vbLf)
                Do
                    Pos = genericController.vbInstr(1, StyleFile, vbLf)
                    If Pos > 0 Then
                        StyleFile = genericController.vbReplace(StyleFile, vbLf, " ")
                    End If
                Loop While Pos > 0
                '
                ' remove double spaces
                '
                Do
                    Pos = genericController.vbInstr(1, StyleFile, "  ")
                    If Pos > 0 Then
                        StyleFile = genericController.vbReplace(StyleFile, "  ", " ")
                    End If
                Loop While Pos > 0
                StyleLines = Split(StyleFile, "}")
                StyleCnt = UBound(StyleLines) + 1
                For Ptr = 0 To StyleCnt - 1
                    StyleLine = StyleLines(Ptr)
                    Pos = genericController.vbInstr(1, StyleLine, "{")
                    If Pos > 0 Then
                        StyleNameList = StyleNameList & vbCrLf & "<div>" & Mid(StyleLine, 1, Pos - 1) & "</div>"
                    End If
                Next
            End If
            Dim StyleEditorPtr As Integer
            StyleNameList = StyleNameList & vbCrLf & "<div><a href=""#"" onClick=""AddStyle();return false;"">Add Style</a></div>"
            StyleNameList = StyleNameList & vbCrLf & "<div>----- end of list</div>"
            StyleNameList = vbCrLf & "<div ID=""StyleEditorListWrapper" & StyleEditorPtr & """>" & vbCrLf & StyleNameList & vbCrLf & "</div>"
            JS = "" _
                & vbCrLf & "<script Language=""JavaScript"" type=""text/javascript"">" _
                & vbCrLf & "function AddStyle() {/* change add link into a text input and a save button */}" _
                & vbCrLf & "function AddStyleSave() {/* send the new style back to the server and save it in the temp file. ON return, replace the text input with a new style. Add a new Add link to the list of styles */}" _
                & vbCrLf & "</script>"

            RightPane = RightPane & main_GetFormInputStyles_Editor(StyleName, StyleDetails)
            '
            main_GetFormInputStyles = JS _
                & "<div style=""border:1px solid #A0A0A0;width:100%;"">" _
                & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" style=""width:100%;"">" _
                & "<tr>" _
                & "<td class=""ccAdminTab"" style=""min-width:100px;padding:5px;text-align:left"">Styles<br ><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=90 height=1></td>" _
                & "<td class=""ccAdminTab"" style=""width:1px;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=1 height=1></td>" _
                & "<td class=""ccAdminTab"" style=""padding:5px;text-align:left"" ID=""" & TagName & ".ContentCaption"">" & genericController.encodeEmptyText(RightSideHeader, "&nbsp;") & "</td>" _
                & "</td></tr>" _
                & "<tr>" _
                & "<td style=""padding:10px;Background-color:white;border:0px solid #808080;vertical-align:top;text-align:left"">" & StyleNameList & "</td>" _
                & "<td class=""ccAdminTab"" style=""width:1px;""></td>" _
                & "<td style=""padding:10px;Background-color:white;border:0px solid #808080;vertical-align:top;text-align:left"">" & RightPane & "</td>" _
                & "</td></tr>" _
                & "</table>" _
                & "</div>"
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFormInputStyles")
        End Function
        '
        '==========================================================================================================================================
        '   main_Get the editor side of the FormInputStyles
        '
        '
        '==========================================================================================================================================
        '
        Public Function main_GetFormInputStyles_Editor(StyleName As String, StyleDetails As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetFormInputStyles_Editor")
            '
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetFormInputStyles_Editor")
        End Function
        '
        '
        '
        Public Function main_GetOnLoadJavascript() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("main_GetOnLoadJavascript")
            '
            'If Not (true) Then Exit Function
            '
            'main_OnLoadJavascript_ToBeAdded = False
            main_GetOnLoadJavascript = htmlDoc.main_OnLoadJavascript
            htmlDoc.main_OnLoadJavascript = ""
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetOnLoadJavascript")
        End Function
        '
        '
        '
        Public Function main_IsContentFieldSupported(ContentName As String, FieldName As String) As Boolean
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("IsContentFieldSupported")
            '
            'If Not (true) Then Exit Function
            '
            main_IsContentFieldSupported = metaData_IsContentFieldSupported(ContentName, FieldName)
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_IsContentFieldSupported")
        End Function
        '
        '
        '
        Public Function main_GetMobileBrowserList() As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetMobileBrowserList")
            '
            'If Not (true) Then Exit Function
            '
            Dim Filename As String
            Dim DefaultMobileBrowserList As String
            Dim DateExpires As Date
            Dim datetext As String
            '
            main_GetMobileBrowserList = genericController.encodeText(cache.getObject(Of String)("MobileBrowserList"))
            If main_GetMobileBrowserList <> "" Then
                datetext = genericController.getLine(main_GetMobileBrowserList)
                If genericController.EncodeDate(datetext) < Now() Then
                    main_GetMobileBrowserList = ""
                End If
            End If
            If main_GetMobileBrowserList = "" Then
                Filename = "config\MobileBrowserList.txt"
                main_GetMobileBrowserList = privateFiles.readFile(Filename)
                If main_GetMobileBrowserList = "" Then
                    main_GetMobileBrowserList = "midp,j2me,avantg,docomo,novarra,palmos,palmsource,240x320,opwv,chtml,pda,windows ce,mmp/,blackberry,mib/,symbian,wireless,nokia,hand,mobi,phone,cdm,up.b,audio,SIE-,SEC-,samsung,HTC,mot-,mitsu,sagem,sony,alcatel,lg,erics,vx,NEC,philips,mmm,xx,panasonic,sharp,wap,sch,rover,pocket,benq,java,pt,pg,vox,amoi,bird,compal,kg,voda,sany,kdd,dbt,sendo,sgh,gradi,jb,moto"
                    main_GetMobileBrowserList = genericController.vbReplace(main_GetMobileBrowserList, ",", vbCrLf)
                    'Call app.publicFiles.SaveFile(Filename, main_GetMobileBrowserList)
                End If
                datetext = DateTime.Now.AddHours(1).ToString
                Call cache.setObject("MobileBrowserList", datetext & vbCrLf & main_GetMobileBrowserList)
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetMobileBrowserList")
        End Function
        '
        '=================================================================================================================
        '   main_GetAddonOption
        '=================================================================================================================
        '
        Public Function main_GetAddonOption(OptionName As String, Option_String As String) As String
            main_GetAddonOption = csv_GetAddonOption(OptionName, Option_String)
        End Function
        '
        '=================================================================================================================
        '   main_GetAddonOptionConstructorValue
        '
        '   I think this might be wrong.
        '       If it decodes AddonOptions delimited by crlf, then it should not need DecodeAddonOptions, b/c this is for instance options
        '
        '
        '   used internally for lists like:
        '
        '   name=value[otherstuff]
        '   name2=value2
        '
        '   Just like main_GetAddonOption, except it trims off the selectors
        '
        '   Used internally to main_GetAddonOption for non-record based Add-ons that can not be called through main_GetAddonContent.
        '   The important difference is this call Decodes Addon Arguments and removes the Selector
        '
        '=================================================================================================================
        '
        Public Function main_GetAddonOptionConstructorValue(OptionName As String, AddonOptionConstructorList As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetAddonOptionConstructorValue")
            '
            Dim Pos As Integer
            Dim s As String
            '
            s = main_GetAddonOption(OptionName, AddonOptionConstructorList)
            Pos = genericController.vbInstr(1, s, "[")
            If Pos > 0 Then
                s = Left(s, Pos - 1)
            End If
            s = genericController.decodeNvaArgument(s)
            '
            main_GetAddonOptionConstructorValue = s
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetAddonOptionConstructorValue")
        End Function
        '
        '========================================================================
        '   Process manual changes needed for Page Content Special Cases
        '       If workflow, only call this routine on a publish - it changes live records
        '========================================================================
        '
        Public Sub main_ProcessSpecialCaseAfterSave(IsDelete As Boolean, ContentName As String, RecordID As Integer, RecordName As String, RecordParentID As Integer, UseContentWatchLink As Boolean)
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("ProcessSpecialCaseAfterSave")
            '
            Dim addonId As Integer
            Dim Option_String As String
            Dim Filename As String
            Dim FilenameExt As String
            Dim FilenameNoExt As String
            Dim FilePath As String
            Dim Pos As Integer
            Dim AltSizeList As String
            'Dim innovaEditor As innovaEditorAddonClassFPO
            Dim sf As imageEditController
            Dim RebuildSizes As Boolean
            Dim AddonStatusOK As Boolean
            Dim pageContentName As String
            Dim PageContentID As Integer
            Dim rootPageId As Integer
            Dim Cmd As String
            Dim CS As Integer
            Dim TableName As String
            Dim PageName As String
            Dim ContentID As Integer
            Dim ActivityLogOrganizationID As Integer
            Dim ActivityLogName As String
            Dim hint As String
            '
            'hint = hint & ",000"
            ContentID = main_GetContentID(ContentName)
            TableName = GetContentTablename(ContentName)
            Call db.markRecordReviewed(ContentName, RecordID)
            '
            ' Test for parentid=id loop
            '
            ' needs to be finished
            '
            '    If (RecordParentID <> 0) And main_IsContentFieldSupported(ContentName, "parentid") Then
            '
            '    End If
            'hint = hint & ",100"
            Select Case genericController.vbLCase(TableName)
                Case "linkaliases"
                    'Call cache_linkAlias_clear
                Case "ccmembers"
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",110"
                    CS = csOpenRecord("people", RecordID, , , "Name,OrganizationID")
                    If db.cs_ok(CS) Then
                        ActivityLogOrganizationID = db.cs_getInteger(CS, "OrganizationID")
                    End If
                    Call db.cs_Close(CS)
                    If IsDelete Then
                        Call log_LogActivity2("deleting user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    Else
                        Call log_LogActivity2("saving changes to user #" & RecordID & " (" & RecordName & ")", RecordID, ActivityLogOrganizationID)
                    End If
                Case "organizations"
                    '
                    ' Log Activity for changes to people and organizattions
                    '
                    'hint = hint & ",120"
                    If IsDelete Then
                        Call log_LogActivity2("deleting organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    Else
                        Call log_LogActivity2("saving changes to organization #" & RecordID & " (" & RecordName & ")", 0, RecordID)
                    End If
                Case "ccsetup"
                    '
                    ' Site Properties
                    '
                    'hint = hint & ",130"
                    Select Case genericController.vbLCase(RecordName)
                        Case "allowlinkalias"
                            Call cache.invalidateContent("Page Content")
                        Case "sectionlandinglink"
                            Call cache.invalidateContent("Page Content")
                        Case siteproperty_serverPageDefault_name
                            Call cache.invalidateContent("Page Content")
                    End Select
                Case "ccpagecontent"
                    '
                    ' set ChildPagesFound true for parent page
                    '
                    'hint = hint & ",140"
                    If RecordParentID > 0 Then
                        Call pages.cache_pageContent_updateRow(RecordParentID, False, False)
                        If Not IsDelete Then
                            Call db.executeSql("update ccpagecontent set ChildPagesfound=1 where ID=" & RecordParentID)
                        End If
                    End If
                    '
                    ' Page Content special cases for delete
                    '
                    If IsDelete Then
                        '
                        ' If this was a section's root page, clear the rootpageid so a new page will be created
                        '
                        Call db.executeSql("update ccsections set RootPageID=0 where RootPageID=" & RecordID)
                        Call pages.pageManager_cache_siteSection_clear()
                        '
                        ' Clear the Landing page and page not found site properties
                        '

                        If genericController.vbLCase(TableName) = "ccpagecontent" Then
                            Call pages.cache_pageContent_removeRow(RecordID, pages.pagemanager_IsWorkflowRendering, False)
                            If RecordID = genericController.EncodeInteger(siteProperties.getText("PageNotFoundPageID", "0")) Then
                                Call siteProperties.setProperty("PageNotFoundPageID", "0")
                            End If
                            If RecordID = siteProperties.landingPageID Then
                                siteProperties.setProperty("landingPageId", "0")
                            End If
                        End If
                        '
                        ' Delete Link Alias entries with this PageID
                        '
                        Call db.executeSql("delete from cclinkAliases where PageID=" & RecordID)
                        Call cache_linkAlias_clear()
                    Else
                        '
                        ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                        '
                        Call pages.cache_pageContent_updateRow(RecordID, False, False)
                    End If
                Case "cctemplates", "ccsharedstyles"
                    '
                    ' Attempt to update the PageContentCache (PCC) array stored in the PeristantVariants
                    '
                    'hint = hint & ",150"
                    Call pages.pageManager_cache_pageTemplate_clear()
                    If Not IsNothing(cache_addonStyleRules) Then
                        Call cache_addonStyleRules.clear()
                    End If

                Case "ccsections"
                    '
                    ' Attempt to update
                    '
                    'hint = hint & ",160"
                    CS = csOpen("Site Sections", RecordID)
                    If db.cs_ok(CS) Then
                        PageContentID = db.cs_getInteger(CS, "ContentID")
                        If PageContentID = 0 Then
                            PageContentID = main_GetContentID("Page Content")
                            Call db.cs_set(CS, "ContentID", PageContentID)
                        End If
                        rootPageId = db.cs_getInteger(CS, "RootPageID")
                        If rootPageId = 0 Then
                            PageName = db.cs_getText(CS, "Name")
                            If PageName = "" Then
                                PageName = "Page " & db.cs_getInteger(CS, "ID")
                            End If
                            pageContentName = metaData.getContentNameByID(PageContentID)
                            If pageContentName = "" Then
                                pageContentName = "Page Content"
                            End If
                            Call db.cs_set(CS, "RootPageID", main_CreatePageGetID(PageName, "Page Content", authContext.user.ID, ""))
                            Call pages.cache_pageContent_clear()
                        End If
                    End If
                    Call db.cs_Close(CS)
                    Call pages.pageManager_cache_siteSection_clear()
                Case "ccaggregatefunctions"
                    '
                    ' Update wysiwyg addon menus
                    '
                    'hint = hint & ",170"
                    Call addonCache.clear()
                    If Not IsNothing(cache_addonStyleRules) Then
                        Call cache_addonStyleRules.clear()
                    End If

                    Call cache_addonIncludeRules_clear()
                Case "ccsharedstylesaddonrules"
                    '
                    ' Update wysiwyg addon menus
                    '
                    'hint = hint & ",175"
                    If Not IsNothing(cache_addonStyleRules) Then
                        Call cache_addonStyleRules.clear()
                    End If

                    Call addonCache.clear()
                Case "cclibraryfiles"
                    '
                    ' if a AltSizeList is blank, make large,medium,small and thumbnails
                    '
                    'hint = hint & ",180"
                    If (siteProperties.getBoolean("ImageAllowSFResize", True)) Then
                        If Not IsDelete Then
                            CS = csOpen("library files", RecordID)
                            If db.cs_ok(CS) Then
                                Filename = db.cs_get(CS, "filename")
                                Pos = InStrRev(Filename, "/")
                                If Pos > 0 Then
                                    FilePath = Mid(Filename, 1, Pos)
                                    Filename = Mid(Filename, Pos + 1)
                                End If
                                Call db.cs_set(CS, "filesize", main_GetFileSize(appRootFiles.rootLocalPath & FilePath & Filename))
                                Pos = InStrRev(Filename, ".")
                                If Pos > 0 Then
                                    FilenameExt = Mid(Filename, Pos + 1)
                                    FilenameNoExt = Mid(Filename, 1, Pos - 1)
                                    If genericController.vbInstr(1, "jpg,gif,png", FilenameExt, vbTextCompare) <> 0 Then
                                        sf = New imageEditController
                                        If sf.load(appRootFiles.rootLocalPath & FilePath & Filename) Then
                                            '
                                            '
                                            '
                                            Call db.cs_set(CS, "height", sf.height)
                                            Call db.cs_set(CS, "width", sf.width)
                                            AltSizeList = db.cs_getText(CS, "AltSizeList")
                                            RebuildSizes = (AltSizeList = "")
                                            If RebuildSizes Then
                                                AltSizeList = ""
                                                '
                                                ' Attempt to make 640x
                                                '
                                                If sf.width >= 640 Then
                                                    sf.height = CInt(sf.height * (640 / sf.width))
                                                    sf.width = 640
                                                    Call sf.save(appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-640x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "640x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 320x
                                                '
                                                If sf.width >= 320 Then
                                                    sf.height = CInt(sf.height * (320 / sf.width))
                                                    sf.width = 320
                                                    Call sf.save(appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-320x" & sf.height & "." & FilenameExt)

                                                    AltSizeList = AltSizeList & vbCrLf & "320x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 160x
                                                '
                                                If sf.width >= 160 Then
                                                    sf.height = CInt(sf.height * (160 / sf.width))
                                                    sf.width = 160
                                                    Call sf.save(appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-160x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "160x" & sf.height
                                                End If
                                                '
                                                ' Attempt to make 80x
                                                '
                                                If sf.width >= 80 Then
                                                    sf.height = CInt(sf.height * (80 / sf.width))
                                                    sf.width = 80
                                                    Call sf.save(appRootFiles.rootLocalPath & FilePath & FilenameNoExt & "-180x" & sf.height & "." & FilenameExt)
                                                    AltSizeList = AltSizeList & vbCrLf & "80x" & sf.height
                                                End If
                                                Call db.cs_set(CS, "AltSizeList", AltSizeList)
                                            End If
                                            Call sf.Dispose()
                                            sf = Nothing
                                        End If
                                        '                                sf.Algorithm = genericController.EncodeInteger(main_GetSiteProperty("ImageResizeSFAlgorithm", "5"))
                                        '                                On Error Resume Next
                                        '                                sf.LoadFromFile (app.publicFiles.rootFullPath & FilePath & Filename)
                                        '                                If Err.Number = 0 Then
                                        '                                    Call app.SetCS(CS, "height", sf.Height)
                                        '                                    Call app.SetCS(CS, "width", sf.Width)
                                        '                                Else
                                        '                                    Err.Clear
                                        '                                End If
                                        '                                AltSizeList = db.cs_getText(CS, "AltSizeList")
                                        '                                RebuildSizes = (AltSizeList = "")
                                        '                                If RebuildSizes Then
                                        '                                    AltSizeList = ""
                                        '                                    '
                                        '                                    ' Attempt to make 640x
                                        '                                    '
                                        '                                    If sf.Width >= 640 Then
                                        '                                        sf.Width = 640
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-640x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "640x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 320x
                                        '                                    '
                                        '                                    If sf.Width >= 320 Then
                                        '                                        sf.Width = 320
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-320x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "320x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 160x
                                        '                                    '
                                        '                                    If sf.Width >= 160 Then
                                        '                                        sf.Width = 160
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-160x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "160x" & sf.Height
                                        '                                    End If
                                        '                                    '
                                        '                                    ' Attempt to make 80x
                                        '                                    '
                                        '                                    If sf.Width >= 80 Then
                                        '                                        sf.Width = 80
                                        '                                        Call sf.DoResize
                                        '                                        Call sf.SaveToFile(app.publicFiles.rootFullPath & FilePath & FilenameNoExt & "-80x" & sf.Height & "." & FilenameExt)
                                        '                                        AltSizeList = AltSizeList & vbCrLf & "80x" & sf.Height
                                        '                                    End If
                                        '                                    Call app.SetCS(CS, "AltSizeList", AltSizeList)
                                        '                                End If
                                        '                                sf = Nothing
                                    End If
                                End If
                            End If
                            Call db.cs_Close(CS)
                        End If
                    End If
                    Call cache_libraryFiles_clear()
                Case Else
                    '
                    '
                    '
            End Select
            '
            ' Process Addons marked to trigger a process call on content change
            '
            'hint = hint & ",190"
            If True Then
                'hint = hint & ",200 content=[" & ContentID & "]"
                CS = db.cs_open("Add-on Content Trigger Rules", "ContentID=" & ContentID, , , , , , "addonid")
                Option_String = "" _
                    & vbCrLf & "action=contentchange" _
                    & vbCrLf & "contentid=" & ContentID _
                    & vbCrLf & "recordid=" & RecordID _
                    & ""
                Do While db.cs_ok(CS)
                    addonId = db.cs_getInteger(CS, "Addonid")
                    'hint = hint & ",210 addonid=[" & addonId & "]"
                    Call addon.executeAddonAsProcess(CStr(addonId), Option_String)
                    Call db.cs_goNext(CS)
                Loop
                Call db.cs_Close(CS)
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_ProcessSpecialCaseAfterSave, Hint=" & hint)
        End Sub
        '
        '================================================================================================================
        '   main_Get SharedStyleFilelist
        '
        '   SharedStyleFilelist is a list of filenames (with conditional comments) that should be included on pages
        '   that call out the SharedFileIDList
        '
        '   Suffix and Prefix are for Conditional Comments around the style tag
        '
        '   SharedStyleFileList is
        '       crlf filename < Prefix< Suffix
        '       crlf filename < Prefix< Suffix
        '       ...
        '       Prefix and Suffix are htmlencoded
        '
        '   SharedStyleMap file
        '       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        '       crlf StyleID tab StyleFilename < Prefix < Suffix, IncludedStyleFilename < Prefix < Suffix, ...
        '       ...
        '       StyleID is 0 if Always include is set
        '       The Prefix and Suffix have had crlf removed, and comma replaced with &#44;
        '================================================================================================================
        '
        Friend Function main_GetSharedStyleFileList(SharedStyleIDList As String, main_IsAdminSite As Boolean) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetSharedStyleFileList")
            '
            Dim Prefix As String
            Dim Suffix As String
            Dim Files() As String
            Dim Pos As Integer
            Dim SrcID As Integer
            Dim Srcs() As String
            Dim SrcCnt As Integer
            Dim IncludedStyleFilename As String
            Dim styleId As Integer
            Dim LastStyleID As Integer
            Dim CS As Integer
            Dim Ptr As Integer
            Dim MapList As String
            Dim Map() As String
            Dim MapCnt As Integer
            Dim MapRow As Integer
            Dim StyleSheetLink As String
            Dim Filename As String
            Dim FileList As String
            Dim SQL As String
            Dim BakeName As String
            '
            If main_IsAdminSite Then
                BakeName = "SharedStyleMap-Admin"
            Else
                BakeName = "SharedStyleMap-Public"
            End If
            MapList = genericController.encodeText(cache.getObject(Of String)(BakeName))
            If MapList = "" Then
                '
                ' BuildMap
                '
                MapList = ""
                If True Then
                    '
                    ' add prefix and suffix conditional comments
                    '
                    SQL = "select s.ID,s.Stylefilename,s.Prefix,s.Suffix,i.StyleFilename as iStylefilename,s.AlwaysInclude,i.Prefix as iPrefix,i.Suffix as iSuffix" _
                        & " from ((ccSharedStyles s" _
                        & " left join ccSharedStylesIncludeRules r on r.StyleID=s.id)" _
                        & " left join ccSharedStyles i on i.id=r.IncludedStyleID)" _
                        & " where ( s.active<>0 )and((i.active is null)or(i.active<>0))"
                End If
                CS = db.cs_openSql(SQL)
                LastStyleID = 0
                Do While db.cs_ok(CS)
                    styleId = db.cs_getInteger(CS, "ID")
                    If styleId <> LastStyleID Then
                        Filename = db.cs_get(CS, "StyleFilename")
                        Prefix = genericController.vbReplace(htmlDoc.main_encodeHTML(db.cs_get(CS, "Prefix")), ",", "&#44;")
                        Suffix = genericController.vbReplace(htmlDoc.main_encodeHTML(db.cs_get(CS, "Suffix")), ",", "&#44;")
                        If (Not main_IsAdminSite) And db.cs_getBoolean(CS, "alwaysinclude") Then
                            MapList = MapList & vbCrLf & "0" & vbTab & Filename & "<" & Prefix & "<" & Suffix
                        Else
                            MapList = MapList & vbCrLf & styleId & vbTab & Filename & "<" & Prefix & "<" & Suffix
                        End If
                    End If
                    IncludedStyleFilename = db.cs_getText(CS, "iStylefilename")
                    Prefix = htmlDoc.main_encodeHTML(db.cs_get(CS, "iPrefix"))
                    Suffix = htmlDoc.main_encodeHTML(db.cs_get(CS, "iSuffix"))
                    If IncludedStyleFilename <> "" Then
                        MapList = MapList & "," & IncludedStyleFilename & "<" & Prefix & "<" & Suffix
                    End If
                    Call db.cs_goNext(CS)
                Loop
                If MapList = "" Then
                    MapList = ","
                End If
                Call cache.setObject(BakeName, MapList, "Shared Styles")
            End If
            If (MapList <> "") And (MapList <> ",") Then
                Srcs = Split(SharedStyleIDList, ",")
                SrcCnt = UBound(Srcs) + 1
                Map = Split(MapList, vbCrLf)
                MapCnt = UBound(Map) + 1
                '
                ' Add stylesheets with AlwaysInclude set (ID is saved as 0 in Map)
                '
                FileList = ""
                For MapRow = 0 To MapCnt - 1
                    If genericController.vbInstr(1, Map(MapRow), "0" & vbTab) = 1 Then
                        Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
                        If Pos > 0 Then
                            FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
                        End If
                    End If
                Next
                '
                ' create a filelist of everything that is needed, might be duplicates
                '
                For Ptr = 0 To SrcCnt - 1
                    SrcID = genericController.EncodeInteger(Srcs(Ptr))
                    If SrcID <> 0 Then
                        For MapRow = 0 To MapCnt - 1
                            If genericController.vbInstr(1, Map(MapRow), SrcID & vbTab) <> 0 Then
                                Pos = genericController.vbInstr(1, Map(MapRow), vbTab)
                                If Pos > 0 Then
                                    FileList = FileList & "," & Mid(Map(MapRow), Pos + 1)
                                End If
                            End If
                        Next
                    End If
                Next
                '
                ' dedup the filelist and convert it to crlf delimited
                '
                If FileList <> "" Then
                    Files = Split(FileList, ",")
                    For Ptr = 0 To UBound(Files)
                        Filename = Files(Ptr)
                        If genericController.vbInstr(1, main_GetSharedStyleFileList, Filename, vbTextCompare) = 0 Then
                            main_GetSharedStyleFileList = main_GetSharedStyleFileList & vbCrLf & Filename
                        End If
                    Next
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetSharedStyleFileList")
        End Function
        '
        '========================================================================
        '   Parse a list of html tags and produce a list of styles
        '========================================================================
        '
        Private Function main_GetStyleListFromHTML(Doc As String, BasePath As String, SourceHost As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetStyleListFromHTML")
            '
            '
            Dim StyleTag As String
            Dim LinkType As String
            Dim Link As String
            Dim ElementCount As Integer
            Dim TagCount As Integer
            Dim TagName As String
            Dim kmaParse As htmlParserController
            Dim ElementPointer As Integer
            Dim Output As New stringBuilderLegacyController
            Dim ElementText As String
            Dim RootRelativeLink As String
            Dim TagDone As Boolean
            '
            kmaParse = New htmlParserController(Me)
            Call kmaParse.Load(Doc)
            ElementPointer = 0
            ElementCount = kmaParse.ElementCount
            '
            Do While ElementPointer < ElementCount
                ElementText = kmaParse.Text(ElementPointer)
                If kmaParse.IsTag(ElementPointer) Then
                    TagCount = TagCount + 1
                    TagName = kmaParse.TagName(ElementPointer)
                    Select Case genericController.vbUCase(TagName)
                        Case "LINK"
                            '
                            Link = kmaParse.ElementAttribute(ElementPointer, "HREF")
                            LinkType = kmaParse.ElementAttribute(ElementPointer, "TYPE")
                            If (genericController.IsLinkToThisHost(SourceHost, Link)) And (LCase(LinkType) = "text/css") Then
                                RootRelativeLink = genericController.ConvertLinkToRootRelative(Link, BasePath)
                                main_GetStyleListFromHTML = main_GetStyleListFromHTML & vbCrLf & main_GetStyleListFromLink(Link, BasePath, SourceHost, "")
                            End If
                        Case "STYLE"
                            '
                            ' Skip to the </Style> TAG, main_Get the stylesheet between for processing
                            '
                            TagDone = False
                            Do While (Not TagDone) And (ElementPointer < ElementCount)
                                '
                                ' Process the next segment
                                '
                                ElementPointer = ElementPointer + 1
                                ElementText = kmaParse.Text(ElementPointer)
                                If kmaParse.IsTag(ElementPointer) Then
                                    '
                                    ' Process a tag (should just be </SCRIPT>, but go until it is
                                    '
                                    TagCount = TagCount + 1
                                    TagDone = (kmaParse.TagName(ElementPointer) = "/" & TagName)
                                End If
                                If Not TagDone Then
                                    StyleTag = StyleTag & ElementText
                                End If
                            Loop
                            main_GetStyleListFromHTML = main_GetStyleListFromHTML & vbCrLf & main_GetStyleListFromStylesheet(StyleTag, BasePath, SourceHost, "")
                    End Select
                End If
                'Output.Add( ElementText
                ElementPointer = ElementPointer + 1
            Loop
            'main_GetStyleListFromHTML = Output.Text
            '
            kmaParse = Nothing
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_GetStyleListFromHTML", True, False)
        End Function
        '
        ' ================================================================================================
        '   conversion pass 3
        ' ================================================================================================
        '
        '
        '
        '
        Private Function main_GetStyleListFromLink(Link As String, BasePath As String, SourceHost As String, BlockRootRelativeLinkList As String) As String
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("GetStyleListFromLink")
            '
            Dim Pos As Integer
            Dim ImportedStyle As String
            Dim HTTP As New httpRequestController()
            Dim Filename As String
            Dim RootRelativeLink As String
            Dim ImportLink As String
            Dim LinkPath As String
            '
            RootRelativeLink = genericController.ConvertLinkToRootRelative(Link, BasePath)
            main_GetStyleListFromLink = ""
            If genericController.vbInstr(1, BlockRootRelativeLinkList, RootRelativeLink, vbTextCompare) = 0 Then
                ImportLink = SourceHost & RootRelativeLink
                ImportedStyle = HTTP.getURL(ImportLink)
                Dim HTTPStatus As String
                HTTPStatus = genericController.getLine(HTTP.responseHeader)
                If genericController.vbInstr(1, HTTPStatus, "200") = 0 Then
                    main_GetStyleListFromLink = ""
                Else
                End If

                Pos = InStrRev(RootRelativeLink, "/")
                If Pos > 0 Then
                    LinkPath = Mid(RootRelativeLink, 1, Pos)
                End If
                main_GetStyleListFromLink = main_GetStyleListFromStylesheet(ImportedStyle, LinkPath, SourceHost, BlockRootRelativeLinkList & "," & RootRelativeLink)
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_GetStyleListFromLink", True, False)
        End Function
        '
        '
        '
        Private Function main_GetStyleListFromStylesheet(StyleSheet As String, BasePath As String, SourceHost As String, BlockRootRelativeLinkList As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetStyleListFromStylesheet")
            '
            Dim PosURLStart As Integer
            Dim PosURLEnd As Integer
            Dim URLCommand As String
            Dim Value As String
            Dim Loopcnt2 As Integer
            Dim Name As String
            Dim NameValue As String
            Dim PtrStart As Integer
            Dim PtrEnd As Integer
            Dim Line As String
            Dim Lines() As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            Dim Ptr As Integer
            Dim Pos As Integer
            Dim PosStart As Integer
            Dim PosEnd As Integer
            Dim Link As String
            Dim RootRelativeLink As String
            Dim LoopCnt As Integer
            Dim ImportedStyle As String
            Dim HTTP As New httpRequestController()
            Dim Output As String
            '
            Pos = 1
            Output = StyleSheet
            '
            ' convert imports
            '
            Do While (Pos <> 0) And LoopCnt < 100
                Pos = genericController.vbInstr(Pos, StyleSheet, "@import", vbTextCompare)
                If Pos <> 0 Then
                    '
                    ' style includes an import -- convert filename and load the file
                    '
                    Pos = genericController.vbInstr(Pos, StyleSheet, "url", vbTextCompare)
                    If Pos <> 0 Then
                        PosStart = genericController.vbInstr(Pos, StyleSheet, "(", vbTextCompare)
                        If PosStart <> 0 Then
                            PosStart = PosStart + 1
                            PosEnd = genericController.vbInstr(PosStart, StyleSheet, ")", vbTextCompare)
                            If PosEnd <> 0 Then
                                PosEnd = PosEnd - 1
                                Link = Mid(StyleSheet, PosStart, PosEnd - PosStart + 1)
                                Output = Output & vbCrLf & main_GetStyleListFromLink(Link, BasePath, SourceHost, BlockRootRelativeLinkList)
                                Pos = PosStart
                            End If
                        End If
                    End If
                End If
                LoopCnt = LoopCnt + 1
            Loop
            '
            ' Done
            '
            main_GetStyleListFromStylesheet = Output
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_GetStyleListFromStylesheet", True, False)
        End Function
        '
        '=================================================================================================================================================
        '   main_AddLinkAlias
        '
        '   Link Alias
        '       A LinkAlias name is a unique string that identifies a page on the site.
        '       A page on the site is generated from the PageID, and the QueryStringSuffix
        '       PageID - obviously, this is the ID of the page
        '       QueryStringSuffix - other things needed on the Query to display the correct content.
        '           The Suffix is needed in cases like when an Add-on is embedded in a page. The URL to that content becomes the pages
        '           Link, plus the suffix needed to find the content.
        '
        '       When you make the menus, look up the most recent Link Alias entry with the pageID, and a blank QueryStringSuffix
        '
        '   The Link Alias table no longer needs the Link field.
        '
        '=================================================================================================================================================
        '
        ' +++++ 9/8/2011 4.1.482, added main_AddLinkAlias to csv and changed main to call
        '
        Public Sub main_AddLinkAlias(ByVal linkAlias As String, ByVal PageID As Integer, ByVal QueryStringSuffix As String, Optional ByVal OverRideDuplicate As Boolean = False, Optional ByVal DupCausesWarning As Boolean = False)
            Try
                Dim warningMessage As String = ""
                Call app_addLinkAlias2(linkAlias, PageID, QueryStringSuffix, OverRideDuplicate, DupCausesWarning, warningMessage)
                If warningMessage <> "" Then
                    Call error_AddUserError(warningMessage)
                End If
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_AddLinkAlias", True, False)
            End Try
        End Sub
        '
        '   Used in reports
        '
        Public Function main_GetPleaseWaitStart() As String
            '
            main_GetPleaseWaitStart = programFiles.readFile("Resources\WaitPageOpen.htm")
            '
        End Function
        '
        '   Used in reports
        '
        Public Sub main_WritePleaseWaitStart()
            '
            If Not main_PleaseWaitStarted Then
                main_PleaseWaitStarted = True
                Call htmlDoc.writeAltBuffer(main_GetPleaseWaitStart)
                Call webServer.webServerIO_FlushStream()
            End If
            '
        End Sub
        '
        '   Used in reports
        '
        Public Function main_GetPleaseWaitEnd() As String
            Return programFiles.readFile("resources\WaitPageClose.htm")
            'main_GetPleaseWaitEnd = cluster.localClusterFiles.readFile("ccLib\Popup\WaitPageClose.htm")
        End Function
        '
        '   Used in reports
        '
        Public Sub main_WritePleaseWaitEnd()
            If main_PleaseWaitStarted Then
                Call htmlDoc.writeAltBuffer(main_GetPleaseWaitEnd)
                Call webServer.webServerIO_FlushStream()
            End If
        End Sub
        ''
        ''   Pass-through to AppService main_LogActivity
        ''
        'Public Sub main_LogActivity(Message As String)
        '    Call log_LogActivity2(Message, 0, 0)
        'End Sub
        '
        '
        '
        Public Sub log_LogActivity2(Message As String, SubjectMemberID As Integer, SubjectOrganizationID As Integer)
            Call log_logActivity(Message, authContext.user.ID, SubjectMemberID, SubjectOrganizationID, webServer.webServerIO_ServerLink, authContext.visitor.ID, authContext.visit.ID)
        End Sub
        '
        '=================================================================================================
        '   Run and return results from a remotequery call from cj.ajax.data(handler,key,args,pagesize,pagenumber)
        '
        '   This routine builds an xml object inside a <result></result> node.
        '       Right now, the response is in JSON format, and conforms to the google data visualization spec 0.5
        '
        '
        '=================================================================================================
        '
        Private Function init_ProcessAjaxData() As String
            Dim result As String = ""
            Try
                Dim SetPairs() As String
                Dim Pos As Integer
                Dim FieldValue As String
                Dim SetPairString As String
                Dim ArgCnt As Integer
                Dim s As New stringBuilderLegacyController
                Dim FieldName As String
                Dim Copy As String
                Dim PageSize As Integer
                Dim ArgArray() As String
                Dim RemoteKey As String
                Dim EncodedArgs As String
                Dim Args As String
                Dim PageNumber As Integer
                Dim CS As Integer
                Dim SQLQuery As String
                Dim maxRows As Integer
                Dim ArgName() As String
                Dim ArgValue() As String
                Dim Ptr As Integer
                Dim QueryType As Integer
                Dim ContentName As String = ""
                Dim Criteria As String
                Dim SortFieldList As String = ""
                Dim AllowInactiveRecords2 As Boolean
                Dim SelectFieldList As String = ""
                Dim gd As New GoogleDataType
                Dim gv As New GoogleVisualizationType
                Dim RemoteFormat As RemoteFormatEnum
                'Dim DataSource As Models.Entity.dataSourceModel
                '
                gv.status = GoogleVisualizationStatusEnum.OK
                gd.IsEmpty = True
                '
                RemoteKey = docProperties.getText("key")
                EncodedArgs = docProperties.getText("args")

                PageSize = docProperties.getInteger("pagesize")
                PageNumber = docProperties.getInteger("pagenumber")
                Select Case genericController.vbLCase(docProperties.getText("responseformat"))
                    Case "jsonnamevalue"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameValue
                    Case "jsonnamearray"
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameArray
                    Case Else 'jsontable
                        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonTable
                End Select
                '
                ' Handle common work
                '
                If PageNumber = 0 Then
                    PageNumber = 1
                End If
                If PageSize = 0 Then
                    PageSize = 100
                End If
                If maxRows <> 0 And PageSize > maxRows Then
                    PageSize = maxRows
                End If
                '
                If EncodedArgs <> "" Then
                    Args = EncodedArgs
                    ArgArray = Split(Args, "&")
                    ArgCnt = UBound(ArgArray) + 1
                    ReDim ArgName(ArgCnt)
                    ReDim ArgValue(ArgCnt)
                    For Ptr = 0 To ArgCnt - 1
                        Pos = genericController.vbInstr(1, ArgArray(Ptr), "=")
                        If Pos > 0 Then
                            ArgName(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), 1, Pos - 1))
                            ArgValue(Ptr) = genericController.DecodeResponseVariable(Mid(ArgArray(Ptr), Pos + 1))
                        End If
                    Next
                End If
                '
                ' main_Get values out of the remote query record
                '
                If gv.status = GoogleVisualizationStatusEnum.OK Then
                    CS = db.cs_open("Remote Queries", "((VisitId=" & authContext.visit.ID & ")and(remotekey=" & db.encodeSQLText(RemoteKey) & "))")
                    If db.cs_ok(CS) Then
                        '
                        ' Use user definied query
                        '
                        SQLQuery = db.cs_getText(CS, "sqlquery")
                        'DataSource = Models.Entity.dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                        maxRows = db.cs_getInteger(CS, "maxrows")
                        QueryType = db.cs_getInteger(CS, "QueryTypeID")
                        ContentName = db.cs_get(CS, "ContentID")
                        Criteria = db.cs_getText(CS, "Criteria")
                        SortFieldList = db.cs_getText(CS, "SortFieldList")
                        AllowInactiveRecords2 = db.cs_getBoolean(CS, "AllowInactiveRecords")
                        SelectFieldList = db.cs_getText(CS, "SelectFieldList")
                        SetPairString = ""
                    Else
                        '
                        ' Try Hardcoded queries
                        '
                        Select Case genericController.vbLCase(RemoteKey)
                            Case "ccfieldhelpupdate"
                                '
                                ' developers editing field help
                                '
                                If Not authContext.user.Developer Then
                                    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                                    If IsArray(gv.errors) Then
                                        Ptr = 0
                                    Else
                                        Ptr = UBound(gv.errors) + 1
                                    End If
                                    ReDim gv.errors(Ptr)
                                    gv.errors(Ptr) = "permission error"
                                Else
                                    QueryType = QueryTypeUpdateContent
                                    ContentName = "Content Field Help"
                                    Criteria = ""
                                    AllowInactiveRecords2 = False
                                End If
                                'Case Else
                                '    '
                                '    ' query not found
                                '    '
                                '    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                                '    If IsArray(gv.errors) Then
                                '        Ptr = 0
                                '    Else
                                '        Ptr = UBound(gv.errors) + 1
                                '    End If
                                '    ReDim gv.errors(Ptr)
                                '    gv.errors(Ptr) = "query not found"
                        End Select
                    End If
                    Call db.cs_Close(CS)
                    '
                    If gv.status = GoogleVisualizationStatusEnum.OK Then
                        Select Case QueryType
                        'Case QueryTypeSQL
                        '    '
                        '    ' ----- Run a SQL
                        '    '
                        '    If SQLQuery <> "" Then
                        '        For Ptr = 0 To ArgCnt - 1
                        '            SQLQuery = genericController.vbReplace(SQLQuery, ArgName(Ptr), ArgValue(Ptr), vbTextCompare)
                        '            'Criteria = genericController.vbReplace(Criteria, ArgName(Ptr), ArgValue(Ptr), vbTextCompare)
                        '        Next
                        '        On Error Resume Next
                        '        RS = main_ExecuteSQLCommand(DataSource, SQLQuery, 30, PageSize, PageNumber)
                        '        ErrorNumber = Err.Number
                        '        ErrorDescription = Err.Description
                        '        Err.Clear()
                        '        On Error GoTo ErrorTrap
                        '        If ErrorNumber <> 0 Then
                        '            '
                        '            ' ----- Error
                        '            '
                        '            gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                        '            Ptr = UBound(gv.errors) + 1
                        '            ReDim gv.errors(Ptr)
                        '            gv.errors(Ptr) = "Error: " & Err.Description
                        '        ElseIf (Not isDataTableOk(rs)) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        ElseIf (RS.State <> 1) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        ElseIf (rs.rows.count = 0) Then
                        '            '
                        '            ' ----- no result
                        '            '
                        '        Else
                        '            PageSize = RS.PageSize
                        '            Cells = RS.GetRows(PageSize)
                        '            '
                        '            gd.IsEmpty = False
                        '            RowMax = UBound(Cells, 2)
                        '            ColMax = UBound(Cells, 1)
                        '            '
                        '            ' Build headers
                        '            '
                        '            ReDim gd.col(ColMax)
                        '            For ColPtr = 0 To ColMax
                        '                RecordField = RS.Fields.Item(ColPtr)
                        '                gd.col(ColPtr).Id = RecordField.Name
                        '                gd.col(ColPtr).Label = RecordField.Name
                        '                gd.col(ColPtr).Type = ConvertRSTypeToGoogleType(RecordField.Type)
                        '            Next
                        '            'RS.Close()
                        '            'RS = Nothing
                        '            '
                        '            ' Build output table
                        '            '
                        '            ReDim gd.row(RowMax)
                        '            For RowPtr = 0 To RowMax
                        '                With gd.row(RowPtr)
                        '                    ReDim .Cell(ColMax)
                        '                    For ColPtr = 0 To ColMax
                        '                        .Cell(ColPtr).v = genericController.encodeText(Cells(ColPtr, RowPtr))
                        '                    Next
                        '                End With
                        '            Next
                        '        End If
                        '        If (isDataTableOk(rs)) Then
                        '            If False Then
                        '                'RS.Close()
                        '            End If
                        '            'RS = Nothing
                        '        End If
                        '    End If
                        'Case QueryTypeOpenContent
                        '    '
                        '    ' Contensive Content Select, args are criteria replacements
                        '    '

                        '    CDef = app.getCdef(ContentName)
                        '    CS = app.csOpen(ContentName, Criteria, SortFieldList, AllowInactiveRecords, , , SelectFieldList)
                        '    Cells = app.csv_cs_getRows(CS)
                        '    FieldList = app.cs_getSelectFieldList(CS)
                        '    '
                        '    RowMax = UBound(Cells, 2)
                        '    ColMax = UBound(Cells, 1)
                        '    If RowMax = 0 And ColMax = 0 Then
                        '        '
                        '        ' Single result, display with no table
                        '        '
                        '        Copy = genericController.encodeText(Cells(0, 0))
                        '    Else
                        '        '
                        '        ' Build headers
                        '        '
                        '        gd.IsEmpty = False
                        '        RowMax = UBound(Cells, 2)
                        '        ColMax = UBound(Cells, 1)
                        '        '
                        '        ' Build headers
                        '        '
                        '        ReDim gd.col(ColMax)
                        '        For ColPtr = 0 To ColMax
                        '            RecordField = RS.Fields.Item(RowPtr)
                        '            gd.col(ColPtr).Id = RecordField.Name
                        '            gd.col(ColPtr).Label = RecordField.Name
                        '            gd.col(ColPtr).Type = ConvertRSTypeToGoogleType(RecordField.Type)
                        '        Next
                        '        '
                        '        ' Build output table
                        '        '
                        '        'RowStart = vbCrLf & "<Row>"
                        '        'Rowend = "</Row>"
                        '        For RowPtr = 0 To RowMax
                        '            With gd.row(RowPtr)
                        '                For ColPtr = 0 To ColMax
                        '                    .Cell(ColPtr).v = Cells(ColPtr, RowPtr)
                        '                Next
                        '            End With
                        '        Next
                        '    End If
                            Case QueryTypeUpdateContent
                                '
                                ' Contensive Content Update, args are field=value updates
                                ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                                '
                                '
                                ' Go though args and main_Get Set and Criteria
                                '
                                SetPairString = ""
                                Criteria = ""
                                For Ptr = 0 To ArgCnt - 1
                                    If genericController.vbLCase(ArgName(Ptr)) = "setpairs" Then
                                        SetPairString = ArgValue(Ptr)
                                    ElseIf genericController.vbLCase(ArgName(Ptr)) = "criteria" Then
                                        Criteria = ArgValue(Ptr)
                                    End If
                                Next
                                '
                                ' Open the content and cycle through each setPair
                                '
                                CS = db.cs_open(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, , ,, SelectFieldList)
                                If db.cs_ok(CS) Then
                                    '
                                    ' update by looping through the args and setting name=values
                                    '
                                    SetPairs = Split(SetPairString, "&")
                                    For Ptr = 0 To UBound(SetPairs)
                                        If SetPairs(Ptr) <> "" Then
                                            Pos = genericController.vbInstr(1, SetPairs(Ptr), "=")
                                            If Pos > 0 Then
                                                FieldValue = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), Pos + 1))
                                                FieldName = genericController.DecodeResponseVariable(Mid(SetPairs(Ptr), 1, Pos - 1))
                                                If Not main_IsContentFieldSupported(ContentName, FieldName) Then
                                                    Dim errorMessage As String = "result, QueryTypeUpdateContent, key [" & RemoteKey & "], bad field [" & FieldName & "] skipped"
                                                    Throw (New ApplicationException(errorMessage))
                                                Else
                                                    Call db.cs_set(CS, FieldName, FieldValue)
                                                End If
                                            End If
                                        End If
                                    Next
                                End If
                                Call db.cs_Close(CS)
                                'Case QueryTypeInsertContent
                                '    '
                                '    ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                                '    '
                                '    '
                                '    ' Contensive Content Insert, args are field=value
                                '    '
                                '    'CS = main_InsertCSContent(ContentName)
                            Case Else
                        End Select
                        '
                        ' output
                        '
                        Copy = main_FormatRemoteQueryOutput(gd, RemoteFormat)
                        Copy = htmlDoc.html_EncodeHTML(Copy)
                        result = "<data>" & Copy & "</data>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return result
        End Function
        '
        '
        '
        Public Function main_GetRemoteQueryKey(ByVal SQL As String, Optional ByVal DataSourceName As String = "", Optional ByVal maxRows As Integer = 1000) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetRemoteQueryKey")
            '
            Dim CS As Integer
            Dim RemoteKey As String
            Dim DataSourceID As Integer
            'Dim GUIDGenerator As New guidClass
            '
            If maxRows = 0 Then
                maxRows = 1000
            End If
            CS = InsertCSContent("Remote Queries")
            If db.cs_ok(CS) Then
                RemoteKey = Guid.NewGuid.ToString()
                DataSourceID = main_GetRecordID("Data Sources", DataSourceName)
                Call db.cs_set(CS, "remotekey", RemoteKey)
                Call db.cs_set(CS, "datasourceid", DataSourceID)
                Call db.cs_set(CS, "sqlquery", SQL)
                Call db.cs_set(CS, "maxRows", maxRows)
                Call db.cs_set(CS, "dateexpires", db.encodeSQLDate(app_startTime.AddDays(1)))
                Call db.cs_set(CS, "QueryTypeID", QueryTypeSQL)
                Call db.cs_set(CS, "VisitId", authContext.visit.ID)
            End If
            Call db.cs_Close(CS)
            '
            main_GetRemoteQueryKey = RemoteKey
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_GetRemoteQueryKey", True, False)
        End Function
        '
        '
        '
        Public Function main_FormatRemoteQueryOutput(gd As GoogleDataType, RemoteFormat As RemoteFormatEnum) As String
            '
            Dim s As stringBuilderLegacyController
            Dim ColDelim As String
            Dim RowDelim As String
            Dim ColPtr As Integer
            Dim RowPtr As Integer
            '
            ' Select output format
            '
            s = New stringBuilderLegacyController
            Select Case RemoteFormat
                Case RemoteFormatEnum.RemoteFormatJsonNameValue
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            Call s.Add(ColDelim & gd.col(ColPtr).Id & ":'" & gd.row(0).Cell(ColPtr).v & "'")
                            ColDelim = ","
                        Next
                    End If
                    Call s.Add("}")
                Case RemoteFormatEnum.RemoteFormatJsonNameArray
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            Call s.Add(ColDelim & gd.col(ColPtr).Id & ":[")
                            ColDelim = ","
                            RowDelim = ""
                            For RowPtr = 0 To UBound(gd.row)
                                With gd.row(RowPtr).Cell(ColPtr)
                                    s.Add(RowDelim & "'" & .v & "'")
                                    RowDelim = ","
                                End With
                            Next
                            Call s.Add("]")
                        Next
                    End If
                    Call s.Add("}")
                Case RemoteFormatEnum.RemoteFormatJsonTable
                    '
                    '
                    '
                    Call s.Add("{")
                    If Not gd.IsEmpty Then
                        Call s.Add("cols: [")
                        ColDelim = ""
                        For ColPtr = 0 To UBound(gd.col)
                            With gd.col(ColPtr)
                                Call s.Add(ColDelim & "{id: '" & genericController.EncodeJavascript(.Id) & "', label: '" & genericController.EncodeJavascript(.Label) & "', type: '" & genericController.EncodeJavascript(.Type) & "'}")
                                ColDelim = ","
                            End With
                        Next
                        Call s.Add("],rows:[")
                        RowDelim = ""
                        For RowPtr = 0 To UBound(gd.row)
                            s.Add(RowDelim & "{c:[")
                            RowDelim = ","
                            ColDelim = ""
                            For ColPtr = 0 To UBound(gd.col)
                                With gd.row(RowPtr).Cell(ColPtr)
                                    Call s.Add(ColDelim & "{v: '" & genericController.EncodeJavascript(.v) & "'}")
                                    ColDelim = ","
                                End With
                            Next
                            s.Add("]}")
                        Next
                        Call s.Add("]")
                    End If
                    Call s.Add("}")
            End Select
            main_FormatRemoteQueryOutput = s.Text
            '
        End Function
        '
        '
        '
        Friend Sub log_appendLogPageNotFound(PageNotFoundLink As String)
            Try
                Call logController.appendLog(Me, """" & FormatDateTime(app_startTime, vbGeneralDate) & """,""App=" & serverConfig.appConfig.name & """,""main_VisitId=" & authContext.visit.ID & """,""" & PageNotFoundLink & """,""Referrer=" & webServer.requestReferrer & """", "performance", "pagenotfound")
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '
        '
        Public ReadOnly Property main_docType() As String
            Get
                Return siteProperties.docTypeDeclaration()
            End Get
        End Property
        '
        '
        '
        Public ReadOnly Property main_DocTypeAdmin() As String
            Get
                Return siteProperties.docTypeDeclarationAdmin
            End Get
        End Property
        '
        '
        '
        'Private Sub main_ErrorTemplate(Argument as object)
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogMethodEnter("Proc00313")
        '    '
        '    'If Not (true) Then Exit Sub
        '    '
        '    Dim iArgument As String
        '    '
        '    iArgument = genericController.encodeText(Argument)
        '    '
        '    Exit Sub
        '    '
        'ErrorTrap:
        '    Call main_HandleClassErrorAndBubble_TrapPatch1("main_ErrorTemplate")
        'End Sub
        '
        '
        '
        '========================================================================
        ' ----- main_Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Function main_GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            On Error GoTo ErrorTrap
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
                        main_GetXMLAttribute = NodeAttribute.Value
                        Found = True
                        Exit For
                    End If
                Next
            Else
                main_GetXMLAttribute = ResultNode.Value
                Found = True
            End If
            If Not Found Then
                main_GetXMLAttribute = DefaultIfNotFound
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError13("main_GetXMLAttribute")
        End Function
        '        '
        '        '=============================================================================================
        '        '   Legacy
        '        '=============================================================================================
        '        '
        '        Public Function main_GetStreamText2(ByVal RequestName As String) As String
        '            On Error GoTo ErrorTrap
        '            '
        '            main_GetStreamText = main_GetStreamText2(genericController.encodeText(RequestName))
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetStreamText")
        '        End Function
        '
        '=============================================================================================
        '   Legacy
        '=============================================================================================
        '
        Public Function main_GetStreamNumber(ByVal RequestName As String) As Double
            On Error GoTo ErrorTrap
            '
            main_GetStreamNumber = genericController.EncodeNumber(docProperties.getText(genericController.encodeText(RequestName)))
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetStreamNumber")
        End Function
        '        '
        '        '========================================================================
        '        ' main_Get a Text string from request
        '        '   if empty, returns null
        '        '   if RequestBlock true, tries only querystring
        '        '========================================================================
        '        '
        '        Public Function doc_getActiveContent(ByVal RequestName As String) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetStreamActiveContent")
        '            '
        '            'Dim innovaEditor As New innovaEditorAddonClassFPO
        '            '
        '            If True Then
        '                doc_getActiveContent = docProperties.getText(RequestName)
        '                If doc_getActiveContent <> "" Then
        '                    doc_getActiveContent = html_RenderActiveContent(genericController.encodeText(doc_getActiveContent))
        '                End If
        '            End If
        '            '
        '            Exit Function
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_GetStreamActiveContent")
        '        End Function
        '
        '
        '
        '
        '
        Public Function main_IsViewingProperty(ByVal PropertyName As String) As Boolean
            Return Not docProperties.containsKey(PropertyName)
        End Function
        '
        '
        '
        Private Function main_GetFileSize(ByVal VirtualFilePathPage As String) As Integer
            Dim files As IO.FileInfo() = appRootFiles.getFileList(VirtualFilePathPage)
            Return CInt(files(0).Length)
        End Function
        '
        '=========================================================================================
        '   In Init(), Print Hard Coded Pages
        '       A Hard coded page replaces the entire output with an HTML compatible page
        '=========================================================================================
        '
        Private Function executeRoute_hardCodedPage(ByVal HardCodedPage As String) As Boolean
            Dim return_allowPostInitExecuteAddon As Boolean = False
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("Init_PrintHardCodedPage")
            '
            Dim blockSiteWithLogin As Boolean
            Dim allowPageWithoutSectionDisplay As Boolean
            Dim InsertTestOK As Boolean
            Dim ConfirmOrderID As Integer
            Dim PageSize As Integer
            Dim PageNumber As Integer
            Dim ContentName As String
            Dim MsgLabel As String
            Dim PageID As Integer
            Dim rootPageId As Integer
            Dim Pos As Integer
            Dim TrapID As Integer
            Dim CS As Integer
            Dim Name As String
            Dim Copy As String
            Dim Ptr As Integer
            Dim ArgList As String
            Dim Args() As String
            Dim ArgNameValue() As String
            Dim gd As GoogleDataType
            Dim PropertyName As String
            Dim PropertyValue As String
            Dim Recipient As String
            Dim Sender As String
            Dim subject As String
            Dim Message As String
            Dim Emailtext As String
            '
            Dim LinkObjectName As String
            Dim EditorObjectName As String
            Dim BodyOpen As String
            Dim AllowChildPage As Boolean
            Dim autoPrintText As String
            Dim RootPageName As String
            Dim PageCopy As String
            Dim OrderByClause As String
            '
            Select Case genericController.vbLCase(HardCodedPage)
                Case HardCodedPageSendPassword
                    '
                    ' send password to the email address in the querystring
                    '
                    Emailtext = docProperties.getText("email")
                    If Emailtext <> "" Then
                        Call email.sendPassword(Emailtext)
                        Copy = "" _
                            & "<div style=""width:300px;margin:100px auto 0 auto;"">" _
                            & "<p>An attempt to send login information for email address '" & Emailtext & "' has been made.</p>" _
                            & "<p><a href=""?" & web_RefreshQueryString & """>Return to the Site.</a></p>" _
                            & "</div>"
                        Call htmlDoc.writeAltBuffer(Copy)
                        executeRoute_hardCodedPage = True
                    Else
                        executeRoute_hardCodedPage = False
                    End If
                'Case HardCodedPagePrinterVersion
                '    '
                '    ' ----- Page Content Printer main_version
                '    '
                '    Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPagePrinterVersion)
                '    htmlDoc.pageManager_printVersion = True
                '    autoPrintText = docProperties.getText("AutoPrint")
                '    '
                '    If ContentName = "" Then
                '        ContentName = "Page Content"
                '    End If
                '    If autoPrintText = "" Then
                '        autoPrintText = siteProperties.getText("AllowAutoPrintDialog", "1")
                '    End If
                '    If RootPageName = "" Then
                '        blockSiteWithLogin = False
                '        PageCopy = pages.pageManager_GetHtmlBody_GetSection(AllowChildPage, False, False, blockSiteWithLogin)
                '        'PageCopy = main_GetSectionPage(AllowChildPage, False)
                '    Else
                '        OrderByClause = docProperties.getText(RequestNameOrderByClause)
                '        PageID = docProperties.getInteger("bid")
                '        '
                '        ' 5/12/2008 - converted to RootPageID call because we do not use RootPageName anymore
                '        '
                '        allowPageWithoutSectionDisplay = siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                '        If Not allowPageWithoutSectionDisplay Then
                '            allowPageWithoutSectionDisplay = authContext.isAuthenticatedContentManager(Me, ContentName)
                '        End If
                '        PageCopy = pages.pageManager_GetHtmlBody_GetSection_GetContent(PageID, rootPageId, ContentName, OrderByClause, False, False, False, 0, siteProperties.useContentWatchLink, allowPageWithoutSectionDisplay)
                '        If pages.redirectLink <> "" Then
                '            Call webServer.webServerIO_Redirect2(pages.redirectLink, pages.pageManager_RedirectReason, False)
                '        End If
                '        'PageCopy = main_GetContentPage(RootPageName, ContentName, OrderByClause, AllowChildPage, False, PageID)
                '    End If
                '    '
                '    If genericController.EncodeBoolean(autoPrintText) Then
                '        Call htmlDoc.main_AddOnLoadJavascript2("window.print(); window.close()", "Print Page")
                '    End If
                '    BodyOpen = "<body class=""ccBodyPrint"">"

                '    'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage")
                '    Call htmlDoc.writeAltBuffer("" _
                '        & main_docType _
                '        & vbCrLf & "<html>" _
                '        & cr & "<head>" & main_GetHTMLHead() _
                '        & cr & "</head>" _
                '        & vbCrLf & BodyOpen _
                '        & cr & "<div align=""left"">" _
                '        & cr2 & "<table border=""0"" cellpadding=""20"" cellspacing=""0"" width=""100%""><tr><td width=""100%"">" _
                '        & cr3 & "<p>" _
                '        & genericController.kmaIndent(PageCopy) _
                '        & cr3 & "</p>" _
                '        & cr2 & "</td></tr></table>" _
                '        & cr & "</div>" _
                '        & genericController.kmaIndent(htmlDoc.html_GetEndOfBody(False, False, False, False)) _
                '        & cr & "</body>" _
                '        & vbCrLf & "</html>" _
                '        & "")

                '    executeRoute_hardCodedPage = True
                ''Case HardCodedPageMyProfile
                ''    '
                ''    ' Print a User Profile page with the current member
                ''    '
                ''    Call web_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageMyProfile)
                ''    Call writeAltBuffer(main_GetMyProfilePage())
                ''    executeRoute_hardCodedPage = True
                Case HardCodedPageResourceLibrary
                    '
                    ' main_Get FormIndex (the index to the InsertImage# function called on selection)
                    '
                    Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageResourceLibrary)
                    EditorObjectName = docProperties.getText("EditorObjectName")
                    LinkObjectName = docProperties.getText("LinkObjectName")
                    If EditorObjectName <> "" Then
                        '
                        ' Open a page compatible with a dialog
                        '
                        Call htmlDoc.webServerIO_addRefreshQueryString("EditorObjectName", EditorObjectName)
                        Call htmlDoc.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                        'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                        Call main_SetMetaContent(0, 0)
                        Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                        Copy = main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2b")
                        Copy = "" _
                            & main_docType _
                            & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(main_GetHTMLHead()) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.kmaIndent(main_GetPanelHeader("Contensive Resource Library")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & cr2 & "<div style=""border-top:1px solid white;border-bottom:1px solid black;height:2px""><img alt=""spacer"" src=""/ccLib/images/spacer.gif"" width=1 height=1></div>" _
                            & genericController.kmaIndent(Copy) _
                            & cr & "</td></tr>" _
                            & cr & "<tr><td>" _
                            & genericController.kmaIndent(htmlDoc.html_GetEndOfBody(False, False, False, False)) _
                            & cr & "</td></tr></table>" _
                            & cr & "<script language=javascript type=""text/javascript"">fixDialog();</script>" _
                            & cr & "</body>" _
                            & "</html>"
                        Call htmlDoc.writeAltBuffer(Copy)
                        executeRoute_hardCodedPage = True
                        'Call main_GetEndOfBody(False, False)
                        ''--- should be disposed by caller --- Call dispose
                        'Call main_CloseStream
                        'true = False
                        'Set main_cmc = Nothing
                        'Exit Sub
                        'Call main_CloseStream
                    ElseIf LinkObjectName <> "" Then
                        '
                        ' Open a page compatible with a dialog
                        '
                        Call htmlDoc.webServerIO_addRefreshQueryString("LinkObjectName", LinkObjectName)
                        Call htmlDoc.main_AddHeadScriptLink("/ccLib/ClientSide/dialogs.js", "Resource Library")
                        'Call AddHeadScript("<script type=""text/javascript"" src=""/ccLib/ClientSide/dialogs.js""></script>")
                        Call main_SetMetaContent(0, 0)
                        Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Resource Library")
                        Copy = main_GetResourceLibrary2("", True, EditorObjectName, LinkObjectName, True)
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2c")
                        Copy = "" _
                            & main_docType _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(main_GetHTMLHead()) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & main_GetPanelHeader("Contensive Resource Library") _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & Copy _
                            & cr & "</td></tr><tr><td>" & htmlDoc.html_GetEndOfBody(False, False, False, False) & "</td></tr></table>" _
                            & cr & "<script language=javascript type=text/javascript>fixDialog();</script>" _
                            & cr & "</body>" _
                            & vbCrLf & "</html>"
                        Call htmlDoc.writeAltBuffer(Copy)
                        executeRoute_hardCodedPage = True
                    End If
                Case HardCodedPageLoginDefault
                    '
                    ' 9/4/2012 added to prevent lockout if login addon fails
                    web_RefreshQueryString = webServer.requestQueryString
                    'Call main_AddRefreshQueryString("method", "")
                    Call htmlDoc.writeAltBuffer(htmlDoc.getLoginPage(True))
                    executeRoute_hardCodedPage = True
                Case HardCodedPageLogin, HardCodedPageLogoutLogin
                    '
                    ' 7/8/9 - Moved from intercept pages
                    '
                    ' Print the Login form as an intercept page
                    ' Special case - set the current URL to the Refresh Query String
                    ' Because you want the form created to save the refresh values
                    '
                    If genericController.vbUCase(HardCodedPage) = "LOGOUTLOGIN" Then
                        Call authContext.logout(Me)
                    End If
                    web_RefreshQueryString = webServer.requestQueryString
                    'Call main_AddRefreshQueryString("method", "")
                    Call htmlDoc.writeAltBuffer(htmlDoc.getLoginPage(False))
                    'Call writeAltBuffer(main_GetLoginPage2(false) & main_GetEndOfBody(False, False, False))
                    executeRoute_hardCodedPage = True
                Case HardCodedPageLogout
                    '
                    ' ----- logout the current member
                    '
                    Call authContext.logout(Me)
                    executeRoute_hardCodedPage = False
                Case HardCodedPageSiteExplorer
                    '
                    ' 7/8/9 - Moved from intercept pages
                    '
                    Call htmlDoc.webServerIO_addRefreshQueryString(RequestNameHardCodedPage, HardCodedPageSiteExplorer)
                    LinkObjectName = docProperties.getText("LinkObjectName")
                    If LinkObjectName <> "" Then
                        '
                        ' Open a page compatible with a dialog
                        '
                        Call htmlDoc.webServerIO_addRefreshQueryString("LinkObjectName", LinkObjectName)
                        Call htmlDoc.main_AddPagetitle("Site Explorer")
                        Call main_SetMetaContent(0, 0)
                        Copy = addon.execute_legacy5(0, "Site Explorer", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", 0)
                        Call htmlDoc.main_AddOnLoadJavascript2("document.body.style.overflow='scroll';", "Site Explorer")
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2d")
                        Copy = "" _
                            & main_docType _
                            & cr & "<html>" _
                            & cr & "<head>" _
                            & genericController.kmaIndent(main_GetHTMLHead()) _
                            & cr & "</head>" _
                            & cr & "<body class=""ccBodyAdmin ccCon"" style=""overflow:scroll"">" _
                            & genericController.kmaIndent(main_GetPanelHeader("Contensive Site Explorer")) _
                            & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%""><tr><td>" _
                            & genericController.kmaIndent(Copy) _
                            & cr & "</td></tr><tr><td>" & htmlDoc.html_GetEndOfBody(False, False, False, False) & "</td></tr></table>" _
                            & cr & "</body>" _
                            & cr & "</html>"
                        'Set Obj = Nothing
                        Call htmlDoc.writeAltBuffer(Copy)
                        executeRoute_hardCodedPage = True
                    End If
                Case HardCodedPageStatus
                    '
                    ' Status call
                    '
                    webServer.webServerIO_BlockClosePageCopyright = True
                    '
                    ' test default data connection
                    '
                    On Error Resume Next
                    Err.Clear()
                    InsertTestOK = False
                    CS = db.cs_insertRecord("Trap Log")
                    If Not db.cs_ok(CS) Then
                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
                    Else
                        InsertTestOK = True
                        TrapID = db.cs_getInteger(CS, "ID")
                    End If
                    Call db.cs_Close(CS)
                    If InsertTestOK Then
                        If TrapID = 0 Then
                            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
                        Else
                            Call DeleteContentRecord("Trap Log", TrapID)
                        End If
                    End If
                    If Err.Number <> 0 Then
                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. After traplog insert, " & genericController.GetErrString(Err), "Init", False, True)
                        Err.Clear()
                    End If
                    '
                    ' Close page
                    '
                    Call main_ClearStream()
                    If app_errorCount = 0 Then
                        Call htmlDoc.writeAltBuffer("Contensive OK")
                    Else
                        Call htmlDoc.writeAltBuffer("Contensive Error Count = " & app_errorCount)
                    End If
                    webServer.webServerIO_BlockClosePageCopyright = True
                    html_BlockClosePageLink = True
                    'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2f")
                    Call htmlDoc.html_GetEndOfBody(False, False, False, False)
                    executeRoute_hardCodedPage = True
                Case HardCodedPageGetJSPage
                    '
                    ' ----- Create a Javascript page that outputs a page content record
                    '
                    Name = docProperties.getText("name")
                    If Name <> "" Then
                        webServer.webServerIO_BlockClosePageCopyright = True
                        '
                        ' Determine bid (PageID) from referer querystring
                        '
                        Copy = webServer.requestReferrer
                        Pos = genericController.vbInstr(1, Copy, "bid=")
                        If Pos <> 0 Then
                            Copy = Trim(Mid(Copy, Pos + 4))
                            Pos = genericController.vbInstr(1, Copy, "&")
                            If Pos <> 0 Then
                                Copy = Trim(Mid(Copy, 1, Pos))
                            End If
                            PageID = genericController.EncodeInteger(Copy)
                        End If
                        '
                        ' main_Get the page
                        '
                        rootPageId = main_GetRecordID("Page Content", Name)
                        allowPageWithoutSectionDisplay = siteProperties.getBoolean(spAllowPageWithoutSectionDisplay, spAllowPageWithoutSectionDisplay_default)
                        If Not allowPageWithoutSectionDisplay Then
                            allowPageWithoutSectionDisplay = authContext.isAuthenticatedContentManager(Me, ContentName)
                        End If
                        Copy = pages.pageManager_GetHtmlBody_GetSection_GetContent(PageID, rootPageId, "Page Content", "", True, True, False, 0, siteProperties.useContentWatchLink, allowPageWithoutSectionDisplay)
                        'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2g")
                        Copy = Copy & htmlDoc.html_GetEndOfBody(False, True, False, False)
                        Copy = genericController.vbReplace(Copy, "'", "'+""'""+'")
                        Copy = genericController.vbReplace(Copy, vbCr, "\n")
                        Copy = genericController.vbReplace(Copy, vbLf, " ")
                        '
                        ' Write the page to the stream, with a javascript wrapper
                        '
                        MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                        Call webServer.webServerIO_setResponseContentType("text/plain")
                        Call htmlDoc.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                        Call htmlDoc.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                    End If
                    executeRoute_hardCodedPage = True
                Case HardCodedPageGetJSLogin
                    '
                    ' ----- Create a Javascript login page
                    '
                    webServer.webServerIO_BlockClosePageCopyright = True
                    Copy = Copy & "<p align=""center""><CENTER>"
                    If Not authContext.isAuthenticated() Then
                        Copy = Copy & htmlDoc.getLoginPanel()
                    ElseIf authContext.isAuthenticatedContentManager(Me, "Page Content") Then
                        'Copy = Copy & main_GetToolsPanel
                    Else
                        Copy = Copy & "You are currently logged in as " & authContext.user.Name & ". To logout, click <a HREF=""" & webServer.webServerIO_ServerFormActionURL & "?Method=logout"" rel=""nofollow"">Here</A>."
                    End If
                    'Call AppendLog("call main_getEndOfBody, from main_init_printhardcodedpage2h")
                    Copy = Copy & htmlDoc.html_GetEndOfBody(True, True, False, False)
                    Copy = Copy & "</CENTER></p>"
                    Copy = genericController.vbReplace(Copy, "'", "'+""'""+'")
                    Copy = genericController.vbReplace(Copy, vbCr, "")
                    Copy = genericController.vbReplace(Copy, vbLf, "")
                    'Copy = "<b>login Page</b>"
                    '
                    ' Write the page to the stream, with a javascript wrapper
                    '
                    MsgLabel = "Msg" & genericController.encodeText(genericController.GetRandomInteger)
                    Call webServer.webServerIO_setResponseContentType("text/plain")
                    Call htmlDoc.writeAltBuffer("var " & MsgLabel & " = '" & Copy & "'; " & vbCrLf)
                    Call htmlDoc.writeAltBuffer("document.write( " & MsgLabel & " ); " & vbCrLf)
                    executeRoute_hardCodedPage = True
                Case HardCodedPageRedirect
                    '
                    ' ----- Redirect with RC and RI
                    '
                    htmlDoc.pageManager_RedirectContentID = docProperties.getInteger(rnRedirectContentId)
                    htmlDoc.pageManager_RedirectRecordID = docProperties.getInteger(rnRedirectRecordId)
                    If htmlDoc.pageManager_RedirectContentID <> 0 And htmlDoc.pageManager_RedirectRecordID <> 0 Then
                        ContentName = metaData.getContentNameByID(htmlDoc.pageManager_RedirectContentID)
                        If ContentName <> "" Then
                            Call main_RedirectByRecord_ReturnStatus(ContentName, htmlDoc.pageManager_RedirectRecordID)
                        End If
                    End If
                    webServer.webServerIO_BlockClosePageCopyright = True
                    html_BlockClosePageLink = True
                    return_allowPostInitExecuteAddon = False '--- should be disposed by caller --- Call dispose
                    executeRoute_hardCodedPage = True
                Case HardCodedPageExportAscii
                    '
                    '----------------------------------------------------
                    '   Should be a remote method in commerce
                    '----------------------------------------------------
                    '
                    If Not authContext.isAuthenticatedAdmin(Me) Then
                        '
                        ' Administrator required
                        '
                        Call htmlDoc.writeAltBuffer("Error: You must be an administrator to use the ExportAscii method")
                    Else
                        webServer.webServerIO_BlockClosePageCopyright = True
                        ContentName = docProperties.getText("content")
                        PageSize = docProperties.getInteger("PageSize")
                        If PageSize = 0 Then
                            PageSize = 20
                        End If
                        PageNumber = docProperties.getInteger("PageNumber")
                        If PageNumber = 0 Then
                            PageNumber = 1
                        End If
                        If (ContentName = "") Then
                            Call htmlDoc.writeAltBuffer("Error: ExportAscii method requires ContentName")
                        Else
                            Call htmlDoc.writeAltBuffer(exportAscii_GetAsciiExport(ContentName, PageSize, PageNumber))
                        End If
                    End If
                    executeRoute_hardCodedPage = True
                    webServer.webServerIO_BlockClosePageCopyright = True
                    html_BlockClosePageLink = True
                    return_allowPostInitExecuteAddon = False '--- should be disposed by caller --- Call dispose
                    executeRoute_hardCodedPage = True
                Case HardCodedPagePayPalConfirm
                    '
                    '
                    '----------------------------------------------------
                    '   Should be a remote method in commerce
                    '----------------------------------------------------
                    '
                    '
                    ConfirmOrderID = docProperties.getInteger("item_name")
                    If ConfirmOrderID <> 0 Then
                        '
                        ' Confirm the order
                        '
                        CS = db.cs_open("Orders", "(ID=" & ConfirmOrderID & ") and ((OrderCompleted=0)or(OrderCompleted is Null))")
                        If db.cs_ok(CS) Then
                            Call db.cs_set(CS, "OrderCompleted", True)
                            Call db.cs_set(CS, "DateCompleted", app_startTime)
                            Call db.cs_set(CS, "ccAuthCode", docProperties.getText("txn_id"))
                            Call db.cs_set(CS, "ccActionCode", docProperties.getText("payment_status"))
                            Call db.cs_set(CS, "ccRefCode", docProperties.getText("pending_reason"))
                            Call db.cs_set(CS, "PayMethod", "PayPal " & docProperties.getText("payment_type"))
                            Call db.cs_set(CS, "ShipName", docProperties.getText("first_name") & " " & docProperties.getText("last_name"))
                            Call db.cs_set(CS, "ShipAddress", docProperties.getText("address_street"))
                            Call db.cs_set(CS, "ShipCity", docProperties.getText("address_city"))
                            Call db.cs_set(CS, "ShipState", docProperties.getText("address_state"))
                            Call db.cs_set(CS, "ShipZip", docProperties.getText("address_zip"))
                            Call db.cs_set(CS, "BilleMail", docProperties.getText("payer_email"))
                            Call db.cs_set(CS, "ContentControlID", main_GetContentID("Orders Completed"))
                            Call db.cs_save2(CS)
                        End If
                        Call db.cs_Close(CS)
                        '
                        ' Empty the cart
                        '
                        CS = db.cs_open("Visitors", "OrderID=" & ConfirmOrderID)
                        If db.cs_ok(CS) Then
                            Call db.cs_set(CS, "OrderID", 0)
                            Call db.cs_save2(CS)
                        End If
                        Call db.cs_Close(CS)
                        '
                        ' TEmp fix until HardCodedPage is complete
                        '
                        Recipient = siteProperties.getText("EmailOrderNotifyAddress", siteProperties.emailAdmin)
                        If genericController.vbInstr(genericController.encodeText(Recipient), "@") = 0 Then
                            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
                        Else
                            Sender = siteProperties.getText("EmailOrderFromAddress")
                            subject = webServer.webServerIO_requestDomain & " Online Order Pending, #" & ConfirmOrderID
                            Message = "<p>An order confirmation has been recieved from PayPal for " & webServer.webServerIO_requestDomain & "</p>"
                            Call email.send_Legacy(Recipient, Sender, subject, Message, , False, True)
                        End If
                    End If
                    webServer.webServerIO_BlockClosePageCopyright = True
                    html_BlockClosePageLink = True
                    return_allowPostInitExecuteAddon = False '--- should be disposed by caller --- Call dispose
                    executeRoute_hardCodedPage = True
            End Select
            '
            '
            Return return_allowPostInitExecuteAddon
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "main_Init_PrintHardCodedPage", True, False)
            Return return_allowPostInitExecuteAddon
        End Function
        '
        '========================================================================
        ' Print the active editor form
        '========================================================================
        '
        Public Function main_GetActiveEditor(ByVal ContentName As String, ByVal RecordID As Integer, ByVal FieldName As String, Optional ByVal FormElements As String = "") As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("GetActiveEditor")
            '
            'If Not (true) Then Exit Function
            '
            Dim ContentID As Integer
            Dim CSPointer As Integer
            Dim Copy As String
            Dim Filename As String
            Dim Stream As String
            Dim ButtonPanel As String
            Dim EditorPanel As String
            Dim PanelCopy As String
            '
            Dim intContentName As String
            Dim intRecordId As Integer
            Dim strFieldName As String
            '
            intContentName = genericController.encodeText(ContentName)
            intRecordId = genericController.EncodeInteger(RecordID)
            strFieldName = genericController.encodeText(FieldName)
            '
            EditorPanel = ""
            ContentID = main_GetContentID(intContentName)
            If (ContentID < 1) Or (intRecordId < 1) Or (strFieldName = "") Then
                PanelCopy = SpanClassAdminNormal & "The information you have selected can not be accessed.</span>"
                EditorPanel = EditorPanel & main_GetPanel(PanelCopy)
            Else
                intContentName = metaData.getContentNameByID(ContentID)
                If intContentName <> "" Then
                    CSPointer = db.cs_open(intContentName, "ID=" & intRecordId)
                    If Not db.cs_ok(CSPointer) Then
                        PanelCopy = SpanClassAdminNormal & "The information you have selected can not be accessed.</span>"
                        EditorPanel = EditorPanel & main_GetPanel(PanelCopy)
                    Else
                        Copy = db.cs_get(CSPointer, strFieldName)
                        EditorPanel = EditorPanel & htmlDoc.html_GetFormInputHidden("Type", FormTypeActiveEditor)
                        EditorPanel = EditorPanel & htmlDoc.html_GetFormInputHidden("cid", ContentID)
                        EditorPanel = EditorPanel & htmlDoc.html_GetFormInputHidden("ID", intRecordId)
                        EditorPanel = EditorPanel & htmlDoc.html_GetFormInputHidden("fn", strFieldName)
                        EditorPanel = EditorPanel & genericController.encodeText(FormElements)
                        EditorPanel = EditorPanel & htmlDoc.html_GetFormInputHTML3("ContentCopy", Copy, "3", "45", False, True)
                        'EditorPanel = EditorPanel & main_GetFormInputActiveContent( "ContentCopy", Copy, 3, 45)
                        ButtonPanel = main_GetPanelButtons(ButtonCancel & "," & ButtonSave, "button")
                        EditorPanel = EditorPanel & ButtonPanel
                    End If
                End If
            End If
            Stream = Stream & main_GetPanelHeader("Contensive Active Content Editor")
            Stream = Stream & main_GetPanel(EditorPanel)
            Stream = htmlDoc.html_GetFormStart() & Stream & htmlDoc.html_GetFormEnd()
            main_GetActiveEditor = Stream
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_GetActiveEditor", "trap")
        End Function
        '
        '========================================================================
        ' ----- Process the active editor form
        '========================================================================
        '
        Public Sub main_ProcessActiveEditor()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("innovaEditorAddonClassFPO.ProcessActiveEditor")
            '
            Dim MethodName As String
            Dim CS As Integer
            Dim Button As String
            Dim ContentID As Integer
            Dim ContentName As String
            Dim RecordID As Integer
            Dim FieldName As String
            Dim ContentCopy As String
            Dim Filename As String
            Dim TableName As String
            '
            MethodName = "main_ProcessActiveEditor()"
            '
            '
            ' ----- Read in Button and process
            '
            Button = docProperties.getText("Button")
            Select Case Button
                Case ButtonCancel
                    '
                    ' ----- Do nothing, the form will reload with the previous contents
                    '
                Case ButtonSave
                    '
                    ' ----- read the form fields
                    '
                    ContentID = docProperties.getInteger("cid")
                    RecordID = docProperties.getInteger("id")
                    FieldName = docProperties.getText("fn")
                    ContentCopy = docProperties.getText("ContentCopy")
                    '
                    ' ----- convert editor active edit icons
                    '
                    ContentCopy = htmlDoc.html_DecodeContent(ContentCopy)
                    '
                    ' ----- save the content
                    '
                    ContentName = metaData.getContentNameByID(ContentID)
                    If ContentName <> "" Then
                        CS = db.cs_open(ContentName, "ID=" & db.encodeSQLNumber(RecordID), , False)
                        If db.cs_ok(CS) Then
                            Call db.cs_set(CS, FieldName, ContentCopy)
                        End If
                        Call db.cs_Close(CS)
                    End If
            End Select
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_ProcessActiveEditor", "trap")
        End Sub
        '
        '
        '
        Public Function main_GetDefaultAddonOption_String(ByVal ArgumentList As String, ByVal AddonGuid As String, ByVal IsInline As Boolean) As String
            'public Function main_GetDefaultAddonOption_String(ArgumentList As String, AddonGuid As String, IsInline As Boolean, cmc As cpCoreClass) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("main_GetDefaultAddonOption_String")
            '
            Dim AddonName As String
            Dim LastAddonName As String
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
                If genericController.vbLCase(AddonGuid) = genericController.vbLCase(ContentBoxGuid) Then
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
                main_GetDefaultAddonOption_String = ""
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
                        NameValuePair = htmlDoc.pageManager_GetAddonSelector(OptionName, OptionValue, OptionSelector)
                        NameValuePair = genericController.EncodeJavascript(NameValuePair)
                        main_GetDefaultAddonOption_String = main_GetDefaultAddonOption_String & "&" & NameValuePair
                        If genericController.vbInstr(1, NameValuePair, "=") = 0 Then
                            main_GetDefaultAddonOption_String = main_GetDefaultAddonOption_String & "="
                        End If
                    End If
                Next
                If main_GetDefaultAddonOption_String <> "" Then
                    ' remove leading "&"
                    main_GetDefaultAddonOption_String = Mid(main_GetDefaultAddonOption_String, 2)
                End If
            End If
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_GetDefaultAddonOption_String", "trap")
        End Function
        ''
        ''========================================================================
        ''   Encode a string to be used as either a name or value in an optionstring (name=value&name=value&etc)
        ''       use this to create a string by encoding the name and then adding to the string
        ''========================================================================
        ''
        'Public Function encodeNvaArgument(ByVal argToEncode As String) As String
        '    encodeNvaArgument = encodeNvaArgument(argToEncode)
        'End Function
        '
        '========================================================================
        ' main_Get FieldEditorList
        '
        '   FieldEditorList is a comma delmited list of addonids, one for each fieldtype
        '   to use it, split the list on comma and use the fieldtype as index
        '========================================================================
        '
        Public Function getFieldTypeDefaultEditorAddonIdList() As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("main_GetDefaultAddonOption_String")
            '
            Dim editorAddonIds() As String
            Dim SQL As String
            Dim RS As DataTable
            Dim fieldTypeID As Integer
            '
            If Not htmlDoc.html_Private_FieldEditorList_Loaded Then
                '
                ' load default editors into editors() - these are the editors used when there is no editorPreference
                '   editors(fieldtypeid) = addonid
                '
                ReDim editorAddonIds(FieldTypeIdMax)
                SQL = "select t.id,t.editorAddonId" _
                    & " from ccFieldTypes t" _
                    & " left join ccaggregatefunctions a on a.id=t.editorAddonId" _
                    & " where (t.active<>0)and(a.active<>0) order by t.id"
                RS = db.executeSql(SQL)
                For Each dr As DataRow In RS.Rows
                    fieldTypeID = genericController.EncodeInteger(dr("id"))
                    If (fieldTypeID <= FieldTypeIdMax) Then
                        editorAddonIds(fieldTypeID) = genericController.encodeText(dr("editorAddonId"))
                    End If
                Next
                htmlDoc.html_Private_FieldEditorList = Join(editorAddonIds, ",")
                htmlDoc.html_Private_FieldEditorList_Loaded = True
            End If
            getFieldTypeDefaultEditorAddonIdList = htmlDoc.html_Private_FieldEditorList
            '
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_GetDefaultAddonOption_String", "trap")
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   encode an argument to be used in a 'name=value&' string
        '       - ohter characters are reserved to do further parsing, see EncodeNvaArgument
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function main_encodeNvaArgument(ByVal Arg As String) As String
            main_encodeNvaArgument = genericController.encodeNvaArgument(Arg)
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   decode an argument that came from parsing a name or value from a 'name=value&' string
        '       split on '&', then on '=', then decode each of the two arguments from either side
        '       - other characters are reserved to do further parsing, see EncodeNvaArgument
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function main_decodeNvaArgument(ByVal EncodedArg As String) As String
            main_decodeNvaArgument = genericController.decodeNvaArgument(EncodedArg)
        End Function
        '
        '=================================================================================================================
        '   main_GetNvaValue
        '       main_Gets the value from a simple 'name=value&' list, assuming it was assembled using encodeNvaArgument
        '=================================================================================================================
        '
        Public Function main_GetNvaValue(ByVal Name As String, ByVal nvaEncodedString As String) As String
            On Error GoTo ErrorTrap
            '
            Dim s As String
            Dim encodedName As String
            '
            encodedName = genericController.encodeNvaArgument(Name)
            s = genericController.getSimpleNameValue(encodedName, nvaEncodedString, "", "&")
            s = genericController.decodeNvaArgument(s)
            main_GetNvaValue = Trim(s)
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError3(serverConfig.appConfig.name, "", "dll", "cpCoreClass", "main_GetNvaValue", Err.Number, Err.Source, Err.Description, True, False, "")
        End Function
        ''
        ''===========================================================================================
        '' main_ServerDomainCrossList
        ''   comma delimited list of domains that should share in cross domain cookie save
        ''===========================================================================================
        ''
        'Public ReadOnly Property main_ServerDomainCrossList() As String
        '    Get
        '        Dim SQL As String
        '        'dim dt as datatable
        '        '
        '        Const cacheName = "Domain Content Cross List Cache"
        '        '
        '        If Not htmlDoc.html_ServerDomainCrossList_Loaded Then
        '            htmlDoc.html_ServerDomainCrossList = genericController.encodeText(cache.getObject(Of String)(cacheName))
        '            If True And (htmlDoc.html_ServerDomainCrossList = "") Then
        '                htmlDoc.html_ServerDomainCrossList = ","
        '                SQL = "select name from ccDomains where (typeId=1)and(allowCrossLogin<>0)"
        '                Dim dt As DataTable = db.executeSql(SQL)
        '                For Each dr As DataRow In dt.Rows
        '                    htmlDoc.html_ServerDomainCrossList &= dr(0).ToString
        '                Next
        '                Call cache.setObject(cacheName, htmlDoc.html_ServerDomainCrossList, "domains")
        '            End If
        '            htmlDoc.html_ServerDomainCrossList_Loaded = True
        '        End If
        '        main_ServerDomainCrossList = serverConfig.appConfig.domainList(0) & htmlDoc.html_ServerDomainCrossList
        '    End Get
        'End Property

        '        '
        '
        '
        Private Function main_GetDefaultTemplateId() As Integer
            On Error GoTo ErrorTrap
            '
            Dim CS As Integer
            '
            CS = db.cs_open("page templates", "name=" & db.encodeSQLText(TemplateDefaultName), "ID", , , , , "id")
            If db.cs_ok(CS) Then
                main_GetDefaultTemplateId = db.cs_getInteger(CS, "ID")
            End If
            Call db.cs_Close(CS)
            '
            ' ----- if default template not found, create a simple default template
            '
            If main_GetDefaultTemplateId = 0 Then
                CS = db.cs_insertRecord("Page Templates")
                If db.cs_ok(CS) Then
                    main_GetDefaultTemplateId = db.cs_getInteger(CS, "ID")
                    Call db.cs_set(CS, "name", TemplateDefaultName)
                    Call db.cs_set(CS, "Link", "")
                    If True Then
                        Call db.cs_set(CS, "BodyHTML", pages.templateBody)
                    End If
                    If True Then
                        Call db.cs_set(CS, "ccGuid", DefaultTemplateGuid)
                    End If
                    Call db.cs_Close(CS)
                End If
            End If
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_GetDefaultTemplateId", "trap")
        End Function
        '        '
        '        '
        '        '
        '        Private Sub main_mergeInStream(ByVal LinkQueryString As String)
        '            On Error GoTo ErrorTrap
        '            '
        '            Dim inStreamPtr As Integer
        '            Dim ampSplit() As String
        '            Dim ampSplitCount As Integer
        '            Dim ampSplitPointer As Integer
        '            Dim newNameValue As String
        '            Dim equalPtr As Integer
        '            Dim NewName As String
        '            Dim NewValue As String
        '            '
        '            ' Merge the Link Querystring (QS in the Link from the Alias lookup)
        '            ' into the QS originally to the right of a ? in the Alias itself (from custom programming)
        '            ' If names match, use the custom programming main_version (originial QS, not the LinkQueryString)
        '            '
        '            ampSplit = Split(LinkQueryString, "&")
        '            ampSplitCount = UBound(ampSplit) + 1
        '            If ampSplitCount > 0 Then
        '                For ampSplitPointer = 0 To ampSplitCount - 1
        '                    newNameValue = ampSplit(ampSplitPointer)
        '                    If newNameValue <> "" Then
        '                        equalPtr = genericController.vbInstr(1, newNameValue, "=")
        '                        If equalPtr > 0 Then
        '                            NewName = main_DecodeUrl(Mid(newNameValue, 1, equalPtr - 1))
        '                            NewValue = Mid(newNameValue, equalPtr + 1)
        '                        Else
        '                            NewValue = ""
        '                            NewName = newNameValue
        '                        End If
        '                        If docPropertiesArrayCount > 0 Then
        '                            For inStreamPtr = 0 To docPropertiesArrayCount - 1
        '                                If genericController.vbLCase(NewName) = genericController.vbLCase(docPropertiesDict(inStreamPtr).Name) Then
        '                                    '
        '                                    ' Current entry found
        '                                    '
        '                                    Exit For
        '                                End If
        '                            Next
        '                        End If
        '                        If inStreamPtr = docPropertiesArrayCount Then
        '                            '
        '                            ' Add a new InStream entry
        '                            '
        '                            If docPropertiesArrayCount >= docPropertiesArraySize Then
        '                                docPropertiesArraySize = docPropertiesArraySize + 10
        '                                ReDim Preserve docPropertiesDict(docPropertiesArraySize)
        '                            End If
        '                            docPropertiesArrayCount = docPropertiesArrayCount + 1
        '                            web.requestQueryString = genericController.ModifyQueryString(web.requestQueryString, NewName, NewValue)
        '                        End If
        '                        '
        '                        ' Populate the entry at InStreamPtr
        '                        '
        '                        With docPropertiesDict(inStreamPtr)
        '                            .NameValue = newNameValue
        '                            .Name = NewName
        '                            .Value = NewValue
        '                            .FileContent = {}
        '                            .IsFile = False
        '                            .IsForm = False
        '                        End With
        '                    End If
        '                Next
        '            End If
        '            '
        '            Exit Sub
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError11("main_mergeInStream", "trap")
        '        End Sub
        '
        '========================================================================
        ' main_encodeCookieName
        '   replace invalid cookie characters with %hex
        '========================================================================
        '
        Public Function main_encodeCookieName(ByVal Source As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("main_encodeCookieName")
            '
            Dim SourcePointer As Integer
            Dim Character As String
            Dim localSource As String
            '
            If Source <> "" Then
                localSource = Source
                For SourcePointer = 1 To Len(localSource)
                    Character = Mid(localSource, SourcePointer, 1)
                    If genericController.vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
                        main_encodeCookieName = main_encodeCookieName & Character
                    Else
                        main_encodeCookieName = main_encodeCookieName & "%" & Hex(Asc(Character))
                    End If
                Next
            End If
            Exit Function
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_encodeCookieName")
        End Function
        ''
        ''========================================================================
        ''   legacy
        ''========================================================================
        ''
        'Public Function main_SendMemberEmail(ByVal ToMemberID As Integer, ByVal From As String, ByVal subject As String, ByVal Body As String, ByVal Immediate As Boolean, ByVal HTML As Boolean) As String
        '    main_SendMemberEmail = main_SendMemberEmail2(ToMemberID, From, subject, Body, Immediate, HTML, 0, "", False)
        'End Function
        '        '
        '        '========================================================================
        '        '   main_SendMemberEmail2( ToMemberID, From, Subject, Body, Immediate, HTML, emailIdForLog ) As String
        '        '       Returns "" if send is OK, otherwise it returns an error message
        '        '========================================================================
        '        '
        '        Public Function main_SendMemberEmail2(ByVal ToMemberID As Integer, ByVal From As String, ByVal subject As String, ByVal Body As String, ByVal Immediate As Boolean, ByVal HTML As Boolean, ByVal emailIdForLog As Integer, template As String, emailAllowLinkEID As Boolean) As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("SendMemberEmail2")
        '            '
        '            'If Not (true) Then Exit Function
        '            '
        '            main_SendMemberEmail2 = csv_SendMemberEmail3(genericController.EncodeInteger(ToMemberID), genericController.encodeText(From), genericController.encodeText(subject), genericController.encodeText(Body), genericController.EncodeBoolean(Immediate), genericController.EncodeBoolean(HTML), emailIdForLog, "", False)
        '            '
        '            Exit Function
        '            '
        'ErrorTrap:
        '            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("main_SendMemberEmail2")
        '        End Function
        ''
        ''========================================================================
        ''   Legacy
        ''========================================================================
        ''
        'Public Function main_SendMemberEmail_Fast(ByVal ToMemberID As Integer, ByVal From As String, ByVal subject As String, ByVal Body As String, ByVal Immediate As Boolean, ByVal HTML As Boolean) As String
        '    main_SendMemberEmail_Fast = csv_SendMemberEmail3(ToMemberID, From, subject, Body, Immediate, HTML, 0, "", False)
        'End Function

        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Function csv_GetXMLAttribute(ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("csv_GetXMLAttribute")
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
                        csv_GetXMLAttribute = NodeAttribute.Value
                        Found = True
                        Exit For
                    End If
                Next
            Else
                csv_GetXMLAttribute = ResultNode.Value
                Found = True
            End If
            If Not Found Then
                csv_GetXMLAttribute = DefaultIfNotFound
            End If
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError6("csv_GetXMLAttribute", "trap")
        End Function
        '
        '   clear addonIncludeRules cache
        '
        Public Sub cache_addonIncludeRules_clear()
            On Error GoTo ErrorTrap 'Const Tn = "cache_addonIncludeRules_clear": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            cache_addonIncludeRules = New addonIncludeRulesClass
            'cache_addonIncludeRules.itemCnt = 0
            'cache_addonIncludeRules.item = {}
            Call cache.setObject(cache_addonIncludeRules_cacheName, cache_addonIncludeRules.item)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "cache_addonIncludeRules_clear", True)
        End Sub
        '
        Public Sub cache_addonIncludeRules_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.cache_addonIncludeRules_save")
            '
            'Dim cacheArray As Object()
            'ReDim cacheArray(1)
            '
            Call cache_addonIncludeRules.addonIdIndex.getPtr("test")
            '
            Call cache.setObject(cache_addonIncludeRules_cacheName, cache_addonIncludeRules)
            'cacheArray(0) = cache_addonIncludeRules.item
            'cacheArray(1) = cache_addonIncludeRules.addonIdIndex.exportPropertyBag
            'Call cache.cache_save(cache_addonIncludeRules_cacheName, cacheArray)
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("cache_addonIncludeRules_save")
        End Sub
        '
        '   load addonIncludeRules cache
        '
        Private Sub cache_addonIncludeRules_load()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("cache_addonIncludeRules_load")
            '
            'Dim cacheLoaded As Boolean
            Dim bag As Object
            Dim Ticks As Integer
            Dim hint As String
            Dim IDList As String
            Dim CS As Integer
            'dim dt as datatable
            Dim Ptr As Integer
            Dim SQL As String
            Dim SelectList As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim Criteria As String
            'dim buildversion As String
            Dim RecordAddonID As Integer
            Dim RecordIncludedAddonID As Integer
            Dim test As Object
            Dim SaveHintToLog As Boolean
            Dim cacheArray() As String
            Dim cacheTest As Object
            '
            SaveHintToLog = True
            'hint = "cache_addonIncludeRules_load, enter"
            '
            ' Load cached addonIncludeRulesCache
            '
            cache_addonIncludeRules = New addonIncludeRulesClass
            cache_addonIncludeRules.addonIdIndex = New keyPtrController
            cache_addonIncludeRules.itemCnt = 0
            '
            On Error Resume Next
            If Not pages.pagemanager_IsWorkflowRendering() Then
                cacheTest = cache.getObject(Of addonIncludeRulesClass)(cache_addonIncludeRules_cacheName)
                If TypeOf cacheTest Is addonIncludeRulesClass Then
                    cache_addonIncludeRules = DirectCast(cacheTest, addonIncludeRulesClass)
                End If
                'If TypeOf cacheTest Is String(,) Then
                '    cache_addonIncludeRules.item = cacheTest(0)
                '    If Not Isempty(cache_addonIncludeRules.item) Then
                '        bag = cacheTest(1)
                '        Call cache_addonIncludeRules.addonIdIndex.importPropertyBag(bag)
                '        cache_addonIncludeRules.itemCnt = UBound(cache_addonIncludeRules.item, 2) + 1
                '    End If
                'End If

                'If Not Isempty(cacheTest) Then
                '    cacheArray = cacheTest
                '    If Not Isempty(cacheArray) Then
                '        cache_addonIncludeRules = cacheArray(0)
                '        If Not Isempty(cache_addonIncludeRules) Then
                '            bag = cacheArray(1)
                '            If Err.Number = 0 Then
                '                Call cache_addonIncludeRulesAddonIdIndex.importPropertyBag(bag)
                '                If Err.Number = 0 Then
                '                    cache_addonIncludeRulesCnt = UBound(cache_addonIncludeRules, 2) + 1
                '                End If
                '            End If
                '        End If
                '    End If
                'End If
            End If
            Err.Clear()
            On Error GoTo ErrorTrap
            'hint = hint & ",check for empty"
            If cache_addonIncludeRules.itemCnt = 0 Then
                '
                ' cache is empty, build it from scratch
                '
                'hint = hint & ",20 cnt=0 rebuild"
                SQL = "select " & cache_addonIncludeRules_fieldList & " from ccaddonIncludeRules where (active<>0) order by id"
                cache_addonIncludeRules.item = db.convertDataTabletoArray(db.executeSql(SQL))
                'hint = hint & ",21"
                If True Then
                    'hint = hint & ",22"
                    cache_addonIncludeRules.itemCnt = UBound(cache_addonIncludeRules.item, 2) + 1
                    If cache_addonIncludeRules.itemCnt > 0 Then
                        'hint = hint & ",23"
                        cache_addonIncludeRules.addonIdIndex = New keyPtrController
                        For Ptr = 0 To cache_addonIncludeRules.itemCnt - 1
                            RecordAddonID = genericController.EncodeInteger(cache_addonIncludeRules.item(addonIncludeRulesCache_addonId, Ptr))
                            'RecordIncludedAddonID = genericController.EncodeInteger(cache_addonIncludeRules(addonIncludeRulesCache_includedAddonId, Ptr))
                            'hint = hint & ",24 set AddonIdIndex recordAddonId[" & RecordAddonID & "] to ptr[" & Ptr & "]"
                            Call cache_addonIncludeRules.addonIdIndex.setPtr(genericController.encodeText(RecordAddonID), Ptr)
                        Next
                        Call cache_addonIncludeRules_save()
                    End If
                End If
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description & " Hint=[" & hint & "]", "cache_addonIncludeRules_load", True, False)
        End Sub
        '
        '   getPtr addonIncludeRules cache
        '
        Public Function cache_addonIncludeRules_getFirstPtr(addonId As Integer) As Integer
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("cache_addonIncludeRules_getFirstPtr")
            '
            Dim returnPtr As Integer
            Dim CS As Integer
            Dim sqlCriteria As String
            Dim Ptr As Integer
            Dim addonIncludeRulesId As Integer
            Dim nameOrGuid As String
            Dim hint As String
            '
            returnPtr = -1
            'hint = hint & ",enter"
            If cache_addonIncludeRules Is Nothing Then
                Call cache_addonIncludeRules_load()
            End If
            'If cache_addonIncludeRules.itemCnt = 0 Then
            '    'hint = hint & ",call load"
            '    Call cache_addonIncludeRules_load()
            'End If
            If cache_addonIncludeRules.itemCnt <= 0 Then
                '
                'hint = hint & ",cnt<=0 exit"
            ElseIf (cache_addonIncludeRules.addonIdIndex Is Nothing) Then
                '
                'hint = hint & ",index is nothing exit"
            Else
                returnPtr = cache_addonIncludeRules.addonIdIndex.getPtr(CStr(addonId))
            End If
            '
            cache_addonIncludeRules_getFirstPtr = returnPtr
            '
            Exit Function
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description, "cache_addonIncludeRules_getFirstPtr, hint=[" & hint & "]", True, False)
        End Function
        '
        '=======================================================================================================================================
        '   link forward cache - different
        '       link forwards are only checked once
        '       instead of a full cache, the load creates a comma delimited string, saved as persistent variant
        '       if there is a hit, then go use the database.
        '=======================================================================================================================================
        '
        Public Sub cache_linkForward_load()
            On Error GoTo ErrorTrap
            '
            Dim RS As DataTable
            Dim cacheValue As String
            Dim bag As Object
            '
            ' Load cache
            '
            cacheValue = DirectCast(cache.getObject(Of String)(cache_linkForward_cacheName), String)
            If cacheValue = "" Then
                RS = db.executeSql("select sourceLink from ccLinkForwards where (sourceLink<>'')and(DestinationLink<>'')and(active<>0) order by id desc")
                For Each dr As DataRow In RS.Rows
                    cacheValue &= "," & dr.Item("sourceLink").ToString
                Next
                If cacheValue <> "" Then
                    cacheValue = genericController.vbReplace(cacheValue, "\", "/")
                    Call cache.setObject(cache_linkForward_cacheName, cacheValue)
                    'Call cache.cache_savex("dummyValue", "dummyKey")
                End If
            End If
            '
            cache_linkForward = cacheValue
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("cache_linkForward_load")
        End Sub
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '
        '   clear addonIncludeRules cache
        '
        Public Sub cache_libraryFiles_clear()
            On Error GoTo ErrorTrap 'Const Tn = "cache_libraryFiles_clear": 'Dim th as integer: th = profileLogMethodEnter(Tn)
            '
            cache_libraryFilesCnt = 0
            cache_libraryFiles = {}
            Call cache.setObject(cache_LibraryFiles_cacheName, cache_libraryFiles)
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError4(Err.Number, Err.Source, Err.Description, "cache_libraryFiles_clear", True)
        End Sub
        '
        '
        '
        Public Sub cache_libraryFiles_save()
            On Error GoTo ErrorTrap 'Dim th as integer: th = profileLogMethodEnter("MainClass.cache_libraryFiles_save")
            '
            Dim cacheArray As Object()
            ReDim cacheArray(1)
            '
            Call cache_libraryFilesIdIndex.getPtr("test")
            '
            cacheArray(0) = cache_libraryFiles
            cacheArray(1) = cache_libraryFilesIdIndex.exportPropertyBag
            Call cache.setObject(cache_LibraryFiles_cacheName, cacheArray)
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError18("cache_libraryFiles_save")
        End Sub
        '
        '   load libraryFiles cache -- only if not loaded or cleared first
        '
        Public Sub cache_libraryFiles_loadIfNeeded()
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogMethodEnter("cache_libraryFiles_loadIfNeeded")
            '
            ' Dim cacheLoaded As Boolean
            Dim bag As String
            Dim Ticks As Integer
            Dim hint As String
            Dim IDList As String
            Dim CS As Integer
            'dim dt as datatable
            Dim Ptr As Integer
            Dim SQL As String
            Dim SelectList As String
            Dim SupportMetaContentNoFollow As Boolean
            Dim Criteria As String
            'dim buildversion As String
            Dim RecordID As Integer
            Dim RecordIncludedAddonID As Integer
            Dim test As Object
            Dim SaveHintToLog As Boolean
            Dim cacheArray() As Object
            Dim cacheTest As Object
            '
            SaveHintToLog = True
            'hint = "cache_libraryFiles_loadIfNeeded, enter"
            '
            If cache_libraryFilesCnt = 0 Then
                '
                ' Load cached libraryFilesCache
                '
                'hint = hint & ",cnt=0, not loaded or cleared"
                cache_libraryFilesIdIndex = New keyPtrController
                cache_libraryFilesCnt = 0
                '
                cacheTest = cache.getObject(Of Object())(cache_LibraryFiles_cacheName)
                If Not IsNothing(cacheTest) Then
                    cacheArray = DirectCast(cacheTest, Object())
                    cache_libraryFiles = DirectCast(cacheArray(0), String(,))
                    bag = genericController.encodeText(cacheArray(1))
                    Call cache_libraryFilesIdIndex.importPropertyBag(bag)
                    cache_libraryFilesCnt = UBound(cache_libraryFiles, 2) + 1
                End If
                'hint = hint & ",cache loaded cache_libraryFilesCnt=" & cache_libraryFilesCnt
                If cache_libraryFilesCnt = 0 Then
                    '
                    ' cache is empty, build it from scratch
                    '
                    'hint = hint & ",cnt=0 rebuild"
                    SQL = "select " & cache_LibraryFiles_fieldList & " from cclibraryFiles where (active<>0) order by id"
                    cache_libraryFiles = db.convertDataTabletoArray(db.executeSql(SQL))
                    '    RS = app.csv_OpenRSSQL_Internal("Default", SQL, 120, 1000, 1, False, CursorLocationEnum.adUseClient, LockTypeEnum.adLockOptimistic, CursorTypeEnum.adOpenForwardOnly)
                    'cacheLoaded = False
                    'If (isDataTableOk(rs)) Then
                    '    If Not rs.rows.count=0 Then
                    '        cache_libraryFiles = RS.GetRows
                    '        cacheLoaded = True
                    '    End If
                    '    If (isDataTableOk(rs)) Then
                    '        If false Then
                    '            RS.Close
                    '        End If
                    '        'RS = Nothing
                    '    End If
                    'End If
                    'hint = hint & ",21"
                    If True Then
                        'hint = hint & ",22"
                        cache_libraryFilesCnt = UBound(cache_libraryFiles, 2) + 1
                        If cache_libraryFilesCnt > 0 Then
                            'hint = hint & ",reloaded cache_libraryFilesCnt=" & cache_libraryFilesCnt
                            cache_libraryFilesIdIndex = New keyPtrController
                            For Ptr = 0 To cache_libraryFilesCnt - 1
                                RecordID = genericController.EncodeInteger(cache_libraryFiles(LibraryFilesCache_Id, Ptr))
                                Call cache_libraryFilesIdIndex.setPtr(genericController.encodeText(RecordID), Ptr)
                            Next
                            Call cache_libraryFiles_save()
                        End If
                    End If
                End If
            End If
            If SaveHintToLog Then
                Call debug_testPoint(hint)
            End If
            '
            Exit Sub
ErrorTrap:
            Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(Err.Number, Err.Source, Err.Description & " Hint=[" & hint & "]", "cache_libraryFiles_loadIfNeeded", True, False)
        End Sub
        '=============================================================================
        ''' <summary>
        ''' Executes the current route (pathPage and/or querystring based). If not found, the default route (addon) is executed. Initially the default route is the pageManager.
        ''' </summary>
        ''' <returns>The doc created by the default addon. (html, json, etc)</returns>
        Public Function executeRoute(Optional route As String = "") As String
            Dim returnResult As String = ""
            Try
                If (serverConfig.appConfig IsNot Nothing) Then
                    '
                    ' -- if app is not configured, cannot execute route
                    Dim pairs() As String
                    Dim pairName As String
                    Dim pairValue As String
                    Dim addonRoute As String = ""
                    Dim routeTest As String
                    Dim workingRoute As String
                    Dim adminRoute As String = serverConfig.appConfig.adminRoute.ToLower
                    Dim AjaxFunction As String = docProperties.getText(RequestNameAjaxFunction)
                    Dim AjaxFastFunction As String = docProperties.getText(RequestNameAjaxFastFunction)
                    Dim RemoteMethodFromQueryString As String = docProperties.getText(RequestNameRemoteMethodAddon)
                    '
                    'debugLog("executeRoute, enter")
                    '
                    ' determine route from either url or querystring 
                    '
                    If (Not String.IsNullOrEmpty(route)) Then
                        '
                        ' route privided as argument
                        '
                        workingRoute = route
                    ElseIf (Not String.IsNullOrEmpty(RemoteMethodFromQueryString)) Then
                        '
                        ' route comes from a remoteMethod=route querystring argument
                        '
                        workingRoute = "/" & RemoteMethodFromQueryString.ToLower()
                    Else
                        '
                        ' routine comes from the url
                        '
                        workingRoute = webServer.requestPathPage.ToLower
                    End If
                    '
                    ' normalize route to /path/page or /path
                    '
                    workingRoute = genericController.normalizeRoute(workingRoute)
                    '
                    ' call with no addon route returns admin site
                    '
                    If False Then
                        ''
                        ''--------------------------------------------------------------------------
                        '' route is admin
                        ''--------------------------------------------------------------------------
                        ''

                        ''If the Then admin route Is taken -- the login panel processing Is bypassed. those methods need To be a different kind Of route, Or it should be an addon
                        ''runAtServerClass in the admin addon.

                        'Dim admin As New adminClass(Me)
                        'returnResult = admin.addonToBe_admin()
                    Else
                        '
                        '------------------------------------------------------------------------------------------
                        '   remote method
                        '       for hardcoded_addons (simple ajax functions) and addons with asajax and inframe
                        '       Eventually replace the hard-coded ajax hood with this process
                        '       so cj. methods can be consolidated into the cj.ajax.addon (can callback) calls
                        '------------------------------------------------------------------------------------------
                        '
                        ' if route is a remote method, use it
                        '
                        routeTest = workingRoute
                        Dim addonPtr As Integer = addonCache.getPtr(routeTest)
                        If addonPtr >= 0 Then
                            If addonCache.addonCache.addonList(addonPtr.ToString).remoteMethod Then
                                addonRoute = routeTest
                            End If
                        Else
                            If (InStr(routeTest, "/", CompareMethod.Text) = 1) Then
                                routeTest = routeTest.Substring(1)
                                addonPtr = addonCache.getPtr(routeTest)
                                If addonPtr >= 0 Then
                                    If addonCache.addonCache.addonList(addonPtr.ToString).remoteMethod Then
                                        addonRoute = routeTest
                                    End If
                                End If
                            End If
                        End If
                        If addonRoute = "" Then
                            '
                            ' if remote method is not in route, get nameGuid from querystring
                            '
                            addonRoute = docProperties.getText(RequestNameRemoteMethodAddon)
                        End If
                        If addonRoute <> "" Then
                            '
                            ' -- this section was added here. it came from an earlier processing section of initApp() but appears to apply only to remote method processing
                            '--------------------------------------------------------------------------
                            '   Verify Add-ons are run from Referrers on the Aggregate Access List
                            '--------------------------------------------------------------------------
                            '
                            If webServer.webServerIO_ReadStreamJSForm Then
                                If webServer.requestReferrer = "" Then
                                    '
                                    ' Allow it to be hand typed
                                    '
                                Else
                                    '
                                    ' Test source site
                                    '
                                    Dim refProtocol As String = ""
                                    Dim refHost As String = ""
                                    Dim refPath As String = ""
                                    Dim refPage As String = ""
                                    Dim refQueryString As String = ""
                                    Dim cs As Integer
                                    Call genericController.SeparateURL(webServer.requestReferrer, refProtocol, refHost, refPath, refPage, refQueryString)
                                    If genericController.vbUCase(refHost) <> genericController.vbUCase(webServer.requestDomain) Then
                                        '
                                        ' Not from this site
                                        '
                                        If siteProperties.getBoolean("AllowAggregateAccessBlocking") Then
                                            cs = db.cs_open("Aggregate Access", "Link=" & EncodeSQLText(refHost), , False, , , , "active")
                                            If Not db.cs_ok(cs) Then
                                                '
                                                ' no record, add an inactive record and throw error
                                                '
                                                Call db.cs_Close(cs)
                                                cs = db.cs_insertRecord("Aggregate Access")
                                                If db.cs_ok(cs) Then
                                                    Call db.cs_set(cs, "Name", refHost)
                                                    Call db.cs_set(cs, "Link", refHost)
                                                    Call db.cs_set(cs, "active", False)
                                                End If
                                                Call db.cs_Close(cs)
                                                handleExceptionAndContinue(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not in the Aggregate Access Content. An inactive record was added. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return htmlDoc.docBuffer
                                            ElseIf Not db.cs_getBoolean(cs, "active") Then
                                                '
                                                ' inactive record, throw error
                                                '
                                                Call db.cs_Close(cs)
                                                handleExceptionAndContinue(New ApplicationException("Add-on call from [" & refHost & "] was blocked because this domain is not active in the Aggregate Access Content. To allow this domain access, mark the record active.")) ' handleLegacyError12("Init", "")
                                                docOpen = False '--- should be disposed by caller --- Call dispose
                                                Return htmlDoc.docBuffer
                                            Else
                                                '
                                                ' Active record, allow hit
                                                '
                                                Call db.cs_Close(cs)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                            '
                            'Call AppendLog("main_init(), 2710 - exit for remote method")
                            '
                            If True Then
                                Dim Option_String As String = ""
                                Dim pos As Integer
                                Dim HostContentName As String
                                Dim hostRecordId As Integer
                                If docProperties.containsKey("Option_String") Then
                                    Option_String = docProperties.getText("Option_String")
                                Else
                                    '
                                    ' convert Querystring encoding to (internal) NVA
                                    '
                                    If webServer.requestQueryString <> "" Then
                                        pairs = Split(webServer.requestQueryString, "&")
                                        For addonPtr = 0 To UBound(pairs)
                                            pairName = pairs(addonPtr)
                                            pairValue = ""
                                            pos = genericController.vbInstr(1, pairName, "=")
                                            If pos > 0 Then
                                                pairValue = genericController.DecodeResponseVariable(Mid(pairName, pos + 1))
                                                pairName = genericController.DecodeResponseVariable(Mid(pairName, 1, pos - 1))
                                            End If
                                            Option_String = Option_String & "&" & genericController.encodeNvaArgument(pairName) & "=" & genericController.encodeNvaArgument(pairValue)
                                        Next
                                        Option_String = Mid(Option_String, 2)
                                    End If
                                End If
                                HostContentName = docProperties.getText("hostcontentname")
                                hostRecordId = docProperties.getInteger("HostRecordID")
                                '
                                ' remote methods are add-ons
                                '
                                Dim AddonStatusOK As Boolean = True
                                '
                                ' REFACTOR -- must know if this is json or html remote before call because it is an argument -- assume this is a json for now -- must deal with it somehow
                                '
                                returnResult = addon.execute(0, addonRoute, Option_String, CPUtilsBaseClass.addonContext.ContextRemoteMethodJson, HostContentName, hostRecordId, "", "0", False, 0, "", AddonStatusOK, Nothing, "", Nothing, "", authContext.user.ID, authContext.isAuthenticated)
                            End If
                            '
                            ' deliver styles, javascript and other head tags as javascript appends
                            '
                            webServer.webServerIO_BlockClosePageCopyright = True
                            html_BlockClosePageLink = True
                            If (webServer.webServerIO_OutStreamDevice = htmlDocController.htmlDoc_OutStreamJavaScript) Then
                                If genericController.vbInstr(1, returnResult, "<form ", vbTextCompare) <> 0 Then
                                    Dim FormSplit As String() = Split(returnResult, "<form ", , vbTextCompare)
                                    returnResult = FormSplit(0)
                                    For addonPtr = 1 To UBound(FormSplit)
                                        Dim FormEndPos As Integer = genericController.vbInstr(1, FormSplit(addonPtr), ">")
                                        Dim FormInner As String = Mid(FormSplit(addonPtr), 1, FormEndPos)
                                        Dim FormSuffix As String = Mid(FormSplit(addonPtr), FormEndPos + 1)
                                        FormInner = genericController.vbReplace(FormInner, "method=""post""", "method=""main_Get""", 1, 99, vbTextCompare)
                                        FormInner = genericController.vbReplace(FormInner, "method=post", "method=""main_Get""", 1, 99, vbTextCompare)
                                        returnResult = returnResult & "<form " & FormInner & FormSuffix
                                    Next
                                End If
                                '
                                Call htmlDoc.writeAltBuffer(returnResult)
                                returnResult = ""
                            End If
                            '
                            ' 20161227 - executeRoute came from old init(), which used the altBuffer to mock a return. Back that out and return the result directly.
                            '
                            Return returnResult
                            'Call AppendLog("call main_getEndOfBody, from main_inite")
                            'returnResult = returnResult & main_GetEndOfBody(False, False, True, False)
                            'Call writeAltBuffer(returnResult)
                            'docOpen = False '--- should be disposed by caller --- Call dispose
                            'Return _docBuffer
                        End If
                        If True Then
                            '
                            '------------------------------------------------------------------------------------------
                            '   These should all be converted to system add-ons
                            '
                            '   AJAX late functions (slower then the early functions, but they include visit state, etc.
                            '------------------------------------------------------------------------------------------
                            '
                            If AjaxFunction <> "" Then
                                returnResult = ""
                                Select Case AjaxFunction
                                    Case ajaxGetFieldEditorPreferenceForm
                                        '
                                        ' When editing in admin site, if a field has multiple editors (addons as editors), you main_Get an icon
                                        '   to click to select the editor. When clicked, a fancybox opens to display a form. The onStart of
                                        '   he fancybox calls this ajax call and puts the return in the div that is displayed. Return a list
                                        '   of addon editors compatible with the field type.
                                        '
                                        Dim addonDefaultEditorName As String = ""
                                        Dim addonDefaultEditorId As Integer = 0
                                        Dim fieldId As Integer = docProperties.getInteger("fieldid")
                                        '
                                        ' main_Get name of default editor
                                        '
                                        Dim Sql As String = "select top 1" _
                                        & " a.name,a.id" _
                                        & " from ccfields f left join ccAggregateFunctions a on a.id=f.editorAddonId" _
                                        & " where" _
                                        & " f.ID = " & fieldId _
                                        & ""
                                        Dim dt As DataTable
                                        dt = db.executeSql(Sql)
                                        If dt.Rows.Count > 0 Then
                                            For Each rsDr As DataRow In dt.Rows
                                                addonDefaultEditorName = "&nbsp;(" & genericController.encodeText(rsDr("name")) & ")"
                                                addonDefaultEditorId = genericController.EncodeInteger(rsDr("id"))
                                            Next
                                        End If
                                        '
                                        Dim radioGroupName As String = "setEditorPreference" & fieldId
                                        Dim currentEditorAddonId As Integer = docProperties.getInteger("currentEditorAddonId")
                                        Dim submitFormId As Integer = docProperties.getInteger("submitFormId")
                                        Sql = "select f.name,c.name,r.addonid,a.name as addonName" _
                                        & " from (((cccontent c" _
                                        & " left join ccfields f on f.contentid=c.id)" _
                                        & " left join ccAddonContentFieldTypeRules r on r.contentFieldTypeID=f.type)" _
                                        & " left join ccAggregateFunctions a on a.id=r.AddonId)" _
                                        & " where f.id=" & fieldId

                                        dt = db.executeSql(Sql)
                                        If dt.Rows.Count > 0 Then
                                            For Each rsDr As DataRow In dt.Rows
                                                Dim addonId As Integer = genericController.EncodeInteger(rsDr("addonid"))
                                                If (addonId <> 0) And (addonId <> addonDefaultEditorId) Then
                                                    returnResult = returnResult _
                                                    & vbCrLf & vbTab & "<div class=""radioCon"">" & htmlDoc.html_GetFormInputRadioBox(radioGroupName, genericController.encodeText(addonId), CStr(currentEditorAddonId)) & "&nbsp;Use " & genericController.encodeText(rsDr("addonName")) & "</div>" _
                                                    & ""
                                                End If

                                            Next
                                        End If

                                        Dim OnClick As String = "" _
                                        & "var a=document.getElementsByName('" & radioGroupName & "');" _
                                        & "for(i=0;i<a.length;i++) {" _
                                        & "if(a[i].checked){var v=a[i].value}" _
                                        & "}" _
                                        & "document.getElementById('fieldEditorPreference').value='" & fieldId & ":'+v;" _
                                        & "cj.admin.saveEmptyFieldList('" & "FormEmptyFieldList');" _
                                        & "document.getElementById('adminEditForm').submit();" _
                                        & ""

                                        returnResult = "" _
                                        & vbCrLf & vbTab & "<h1>Editor Preference</h1>" _
                                        & vbCrLf & vbTab & "<p>Select the editor you will use for this field. Select default if you want to use the current system default.</p>" _
                                        & vbCrLf & vbTab & "<div class=""radioCon"">" & htmlDoc.html_GetFormInputRadioBox("setEditorPreference" & fieldId, "0", "0") & "&nbsp;Use Default Editor" & addonDefaultEditorName & "</div>" _
                                        & vbCrLf & vbTab & returnResult _
                                        & vbCrLf & vbTab & "<div class=""buttonCon"">" _
                                        & vbCrLf & vbTab & "<button type=""button"" onclick=""" & OnClick & """>Select</button>" _
                                        & vbCrLf & vbTab & "</div>" _
                                        & ""
                                    Case AjaxGetDefaultAddonOptionString
                                        '
                                        ' return the addons defult AddonOption_String
                                        ' used in wysiwyg editor - addons in select list have no defaultOption_String
                                        ' because created it is expensive (lookuplists, etc). This is only called
                                        ' when the addon is double-clicked in the editor after being dropped
                                        '
                                        Dim AddonGuid As String = docProperties.getText("guid")
                                        '$$$$$ cache this
                                        Dim CS As Integer = db.cs_open(cnAddons, "ccguid=" & db.encodeSQLText(AddonGuid))
                                        Dim addonArgumentList As String = ""
                                        Dim addonIsInline As Boolean = False
                                        If db.cs_ok(CS) Then
                                            addonArgumentList = db.cs_getText(CS, "argumentlist")
                                            addonIsInline = db.cs_getBoolean(CS, "IsInline")
                                            returnResult = main_GetDefaultAddonOption_String(addonArgumentList, AddonGuid, addonIsInline)
                                        End If
                                        Call db.cs_Close(CS)
                                    Case AjaxSetVisitProperty
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - sets a visit property from the cj object
                                        '
                                        Dim ArgList As String = docProperties.getText("args")
                                        Dim Args As String() = Split(ArgList, "&")
                                        Dim gd As GoogleDataType = New GoogleDataType
                                        gd.IsEmpty = True
                                        For Ptr = 0 To UBound(Args)
                                            Dim ArgNameValue As String() = Split(Args(Ptr), "=")
                                            Dim PropertyName As String = ArgNameValue(0)
                                            Dim PropertyValue As String = ""
                                            If UBound(ArgNameValue) > 0 Then
                                                PropertyValue = ArgNameValue(1)
                                            End If
                                            Call visitProperty.setProperty(PropertyName, PropertyValue)
                                        Next
                                        returnResult = main_FormatRemoteQueryOutput(gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        returnResult = htmlDoc.main_encodeHTML(returnResult)
                                        Call htmlDoc.writeAltBuffer(returnResult)
                                    Case AjaxGetVisitProperty
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - sets a visit property from the cj object
                                        '
                                        Dim ArgList As String = docProperties.getText("args")
                                        Dim Args As String() = Split(ArgList, "&")
                                        Dim gd As GoogleDataType = New GoogleDataType
                                        gd.IsEmpty = False
                                        ReDim gd.row(0)

                                        For Ptr = 0 To UBound(Args)
                                            ReDim Preserve gd.col(Ptr)
                                            ReDim Preserve gd.row(0).Cell(Ptr)
                                            Dim ArgNameValue As String() = Split(Args(Ptr), "=")
                                            Dim PropertyName As String = ArgNameValue(0)
                                            gd.col(Ptr).Id = PropertyName
                                            gd.col(Ptr).Label = PropertyName
                                            gd.col(Ptr).Type = "string"
                                            Dim PropertyValue As String = ""
                                            If UBound(ArgNameValue) > 0 Then
                                                PropertyValue = ArgNameValue(1)
                                            End If
                                            gd.row(0).Cell(Ptr).v = visitProperty.getText(PropertyName, PropertyValue)
                                        Next
                                        returnResult = main_FormatRemoteQueryOutput(gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                                        returnResult = htmlDoc.main_encodeHTML(returnResult)
                                        Call htmlDoc.writeAltBuffer(returnResult)
                                    Case AjaxData
                                        '
                                        ' 7/7/2009 - Moved from HardCodedPages - Run remote query from cj.remote object call, and return results html encoded in a <result></result> block
                                        ' 20050427 - not used
                                        Call htmlDoc.writeAltBuffer(init_ProcessAjaxData())
                                    Case AjaxPing
                                        '
                                        ' returns OK if the server is alive
                                        '
                                        returnResult = "ok"
                                    Case AjaxOpenIndexFilter
                                        Call visitProperty.setProperty("IndexFilterOpen", "1")
                                    Case AjaxOpenIndexFilterGetContent
                                        '
                                        ' should be converted to adminClass remoteMethod
                                        '
                                        Call visitProperty.setProperty("IndexFilterOpen", "1")
                                        Dim adminSite As New Contensive.Addons.addon_AdminSiteClass(cp_forAddonExecutionOnly)
                                        Dim ContentID As Integer = docProperties.getInteger("cid")
                                        If ContentID = 0 Then
                                            returnResult = "No filter is available"
                                        Else
                                            Dim cdef As cdefModel = metaData.getCdef(ContentID)
                                            returnResult = adminSite.GetForm_IndexFilterContent(cdef)
                                        End If
                                        adminSite = Nothing
                                    Case AjaxCloseIndexFilter
                                        Call visitProperty.setProperty("IndexFilterOpen", "0")
                                    Case AjaxOpenAdminNav
                                        Call visitProperty.setProperty("AdminNavOpen", "1")
                                    Case Else
                                End Select
                                '
                                'Call AppendLog("main_init(), 2810 - exit for ajax hook")
                                '
                                webServer.webServerIO_BlockClosePageCopyright = True
                                html_BlockClosePageLink = True
                                'Call AppendLog("call main_getEndOfBody, from main_initf")
                                returnResult = returnResult & htmlDoc.html_GetEndOfBody(False, False, True, False)
                                Call htmlDoc.writeAltBuffer(returnResult)
                                docOpen = False '--- should be disposed by caller --- Call dispose
                                Return htmlDoc.docBuffer
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        '   Process Email Open and Click Intercepts
                        '   works with DropID -> spacer.gif, or DropCssID -> styles.css
                        '--------------------------------------------------------------------------
                        '
                        If True Then
                            Dim recordid As Integer
                            Dim emailDropId As Integer
                            Dim RedirectLink As String
                            Dim EmailMemberID As Integer
                            Dim CSLog As Integer
                            Dim EmailSpamBlock As String
                            recordid = 0
                            emailDropId = docProperties.getInteger(RequestNameEmailOpenFlag)
                            If emailDropId <> 0 Then
                                recordid = emailDropId
                            End If
                            '    End If
                            If (recordid <> 0) Then
                                '
                                ' ----- Email open detected. Log it and redirect to a 1x1 spacer
                                '
                                EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                CSLog = db.cs_insertRecord("Email Log")
                                If db.cs_ok(CSLog) Then
                                    Call db.cs_set(CSLog, "Name", "Opened " & CStr(app_startTime))
                                    Call db.cs_set(CSLog, "EmailDropID", recordid)
                                    Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                    Call db.cs_set(CSLog, "LogType", EmailLogTypeOpen)
                                End If
                                Call db.cs_Close(CSLog)
                                RedirectLink = webServer.webServerIO_requestProtocol & webServer.requestDomain & "/ccLib/images/spacer.gif"
                                Call webServer.webServerIO_Redirect2(RedirectLink, "Group Email Open hit, redirecting to a dummy image", False)
                            End If
                            '
                            emailDropId = docProperties.getInteger(RequestNameEmailClickFlag)
                            EmailSpamBlock = docProperties.getText(RequestNameEmailSpamFlag)
                            If (emailDropId <> 0) And (EmailSpamBlock = "") Then
                                '
                                ' ----- Email click detected. Log it.
                                '
                                EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                CSLog = db.cs_insertRecord("Email Log")
                                If db.cs_ok(CSLog) Then
                                    Call db.cs_set(CSLog, "Name", "Clicked " & CStr(app_startTime))
                                    Call db.cs_set(CSLog, "EmailDropID", emailDropId)
                                    Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                    Call db.cs_set(CSLog, "VisitId", authContext.visit.ID)
                                    Call db.cs_set(CSLog, "LogType", EmailLogTypeClick)
                                End If
                                Call db.cs_Close(CSLog)
                            End If
                            If EmailSpamBlock <> "" Then
                                '
                                ' ----- Email spam footer was clicked, clear the AllowBulkEmail field
                                '
                                Call email.addToBlockList(EmailSpamBlock)
                                '
                                CSLog = db.cs_open("people", "email=" & db.encodeSQLText(EmailSpamBlock), , , , , , "AllowBulkEmail")
                                Do While db.cs_ok(CSLog)
                                    Call db.cs_set(CSLog, "AllowBulkEmail", False)
                                    Call db.cs_goNext(CSLog)
                                Loop
                                Call db.cs_Close(CSLog)
                                '
                                ' ----- Make a log entry to track the result of this email drop
                                '
                                emailDropId = docProperties.getInteger(RequestNameEmailBlockRequestDropID)
                                If emailDropId <> 0 Then
                                    '
                                    ' ----- Email click detected. Log it.
                                    '
                                    EmailMemberID = docProperties.getInteger(RequestNameEmailMemberID)
                                    CSLog = db.cs_insertRecord("Email Log")
                                    If db.cs_ok(CSLog) Then
                                        Call db.cs_set(CSLog, "Name", "Email Block Request " & CStr(app_startTime))
                                        Call db.cs_set(CSLog, "EmailDropID", emailDropId)
                                        Call db.cs_set(CSLog, "MemberID", EmailMemberID)
                                        Call db.cs_set(CSLog, "VisitId", authContext.visit.ID)
                                        Call db.cs_set(CSLog, "LogType", EmailLogTypeBlockRequest)
                                    End If
                                    Call db.cs_Close(CSLog)
                                End If
                                Call webServer.webServerIO_Redirect2(webServer.webServerIO_requestProtocol & webServer.requestDomain & "/ccLib/popup/EmailBlocked.htm", "Group Email Spam Block hit. Redirecting to EmailBlocked page.", False)
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        '   Process Intercept Pages
                        '       must be before main_Get Intercept Pages
                        '       must be before path block, so a login will main_Get you through
                        '       must be before verbose check, so a change is reflected on this page
                        '--------------------------------------------------------------------------
                        '
                        If True Then
                            Dim formType As String
                            Dim StyleSN As Integer
                            formType = docProperties.getText("type")
                            If (formType <> "") Then
                                '
                                ' set the meta content flag to show it is not needed for the head tag
                                '
                                Call main_SetMetaContent(0, 0)
                                Select Case formType
                                    Case FormTypeSiteStyleEditor
                                        If authContext.isAuthenticated() And authContext.isAuthenticatedAdmin(Me) Then
                                            '
                                            ' Save the site sites
                                            '
                                            Call appRootFiles.saveFile(DynamicStylesFilename, docProperties.getText("SiteStyles"))
                                            If docProperties.getBoolean(RequestNameInlineStyles) Then
                                                '
                                                ' Inline Styles
                                                '
                                                Call siteProperties.setProperty("StylesheetSerialNumber", "0")
                                            Else
                                                '
                                                ' Linked Styles
                                                ' Bump the Style Serial Number so next fetch is not cached
                                                '
                                                StyleSN = siteProperties.getinteger("StylesheetSerialNumber", 0)
                                                StyleSN = StyleSN + 1
                                                Call siteProperties.setProperty("StylesheetSerialNumber", genericController.encodeText(StyleSN))
                                                '
                                                ' Save new public stylesheet
                                                '
                                                Call appRootFiles.saveFile("templates\Public" & StyleSN & ".css", pages.pageManager_GetStyleSheet)
                                                Call appRootFiles.saveFile("templates\Admin" & StyleSN & ".css", pages.pageManager_GetStyleSheetDefault)
                                            End If
                                        End If
                                    Case FormTypeAddonStyleEditor
                                        '
                                        ' save custom styles
                                        '
                                        If authContext.isAuthenticated() And authContext.isAuthenticatedAdmin(Me) Then
                                            Dim addonId As Integer
                                            Dim contentName As String = ""
                                            Dim tableName As String
                                            Dim nothingObject As Object = Nothing
                                            Dim cs As Integer
                                            addonId = docProperties.getInteger("AddonID")
                                            cs = csOpen(cnAddons, addonId)
                                            If db.cs_ok(cs) Then
                                                Call db.cs_set(cs, "CustomStylesFilename", docProperties.getText("CustomStyles"))
                                            End If
                                            Call db.cs_Close(cs)
                                            '
                                            ' Clear Caches
                                            '
                                            Call pages.cache_pageContent_clear()
                                            Call pages.pageManager_cache_pageTemplate_clear()
                                            Call pages.pageManager_cache_siteSection_clear()
                                            'Call cache.invalidateObjectList("")
                                            If contentName <> "" Then
                                                Call cache.invalidateContent(contentName)
                                                tableName = GetContentTablename(contentName)
                                                If genericController.vbLCase(tableName) = "cctemplates" Then
                                                    Call cache.setObject(pagesController.cache_pageTemplate_cacheName, nothingObject)
                                                    Call pages.pageManager_cache_pageTemplate_load()
                                                End If
                                            End If
                                        End If
                                    Case FormTypeAddonSettingsEditor
                                        '
                                        '
                                        '
                                        Call htmlDoc.pageManager_ProcessAddonSettingsEditor()
                                    Case FormTypeHelpBubbleEditor
                                        '
                                        '
                                        '
                                        Call htmlDoc.main_ProcessHelpBubbleEditor()
                                    Case FormTypeJoin
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormJoin()
                                    Case FormTypeSendPassword
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormSendPassword()
                                    Case FormTypeLogin, "l09H58a195"
                                        '
                                        '
                                        '
                                        Call htmlDoc.processFormLoginDefault()
                                    Case FormTypeToolsPanel
                                        '
                                        ' ----- Administrator Tools Panel
                                        '
                                        Call htmlDoc.pageManager_ProcessFormToolsPanel()
                                    Case FormTypePageAuthoring
                                        '
                                        ' ----- Page Authoring Tools Panel
                                        '
                                        Call pages.pageManager_ProcessFormQuickEditing()
                                    Case FormTypeActiveEditor
                                        '
                                        ' ----- Active Editor
                                        '
                                        Call main_ProcessActiveEditor()
                                End Select
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' Process HardCoded Methods
                        ' must go after form processing bc some of these pages have forms that are processed
                        '--------------------------------------------------------------------------
                        '
                        Dim HardCodedPage As String
                        HardCodedPage = docProperties.getText(RequestNameHardCodedPage)
                        If (HardCodedPage <> "") Then
                            '
                            'Call AppendLog("main_init(), 3110 - exit for hardcodedpage hook")
                            '
                            Dim ExitNow As Boolean = executeRoute_hardCodedPage(HardCodedPage)
                            If ExitNow Then
                                docOpen = False '--- should be disposed by caller --- Call dispose
                                Return htmlDoc.docBuffer
                            End If
                        End If
                        '
                        '--------------------------------------------------------------------------
                        ' normalize adminRoute and test for hit
                        '--------------------------------------------------------------------------
                        '
                        If (workingRoute = genericController.normalizeRoute(adminRoute)) Then
                            '
                            'debugLog("executeRoute, route is admin")
                            '
                            '--------------------------------------------------------------------------
                            ' route is admin
                            '   If the Then admin route Is taken -- the login panel processing Is bypassed. those methods need To be a different kind Of route, Or it should be an addon
                            '   runAtServerClass in the admin addon.
                            '--------------------------------------------------------------------------
                            '
                            Dim returnStatusOK As Boolean = False
                            '
                            ' REFACTOR -- when admin code is broken cleanly into an addon, run it through execute
                            '
                            'returnResult = executeAddon(0, adminSiteAddonGuid, "", CPUtilsBaseClass.addonContext.ContextAdmin, "", 0, "", "", False, 0, "", returnStatusOK, Nothing, "", Nothing, "", authcontext.user.userid, visit.visitAuthenticated)
                            '
                            ' until then, run it as an internal class
                            '
                            Dim admin As New Contensive.Addons.addon_AdminSiteClass()
                            returnResult = admin.execute(cp_forAddonExecutionOnly).ToString()
                        Else
                            '--------------------------------------------------------------------------
                            ' default routing addon takes what is left
                            '
                            ' Here was read a site property set to the default addon. Might be performanceCloud-type web application. Might be page-manager
                            '
                            '--------------------------------------------------------------------------
                            '
                            'debugLog("executeRoute, route is Default Route AddonId")
                            '
                            Dim defaultAddonId As Integer = siteProperties.getinteger("Default Route AddonId")
                            If (defaultAddonId = 0) Then
                                '
                                ' -- no default route set, assume html hit
                                returnResult = "<p>This site is not configured for website traffic. Please set the default route.</p>"
                            Else
                                Dim addonStatusOk As Boolean = False
                                returnResult = addon.execute(defaultAddonId, "", "", CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", addonStatusOk, Nothing, "", Nothing, "", authContext.user.ID, authContext.visit.VisitAuthenticated)
                                If (Not addonStatusOk) Then
                                    '
                                    ' -- there was an error in the default route addon
                                    returnResult = "<p>This site is temporarily unavailable.</p>"
                                Else
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                handleExceptionLegacyRow2(ex, "cpCoreClass", System.Reflection.MethodInfo.GetCurrentMethod.Name, "Unexpected Exception")
            End Try
            Return returnResult
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' cpCoreClass constructor common tasks.
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Private Sub constructorInitialize()
            Try
                '
                app_startTickCount = GetTickCount
                CPTickCountBase = GetTickCount
                main_ClosePageCounter = 0
                debug_allowDebugLog = True
                app_startTime = DateTime.Now()
                testPointPrinting = True
                '
                ' -- attempt auth load
                If (serverConfig.appConfig Is Nothing) Then
                    authContext = Models.Context.authContextModel.create(Me, False)
                Else
                    authContext = Models.Context.authContextModel.create(Me, siteProperties.allowVisitTracking)
                    '
                    ' debug printed defaults on, so if not on, set it off and clear what was collected
                    If Not visitProperty.getBoolean("AllowDebugging") Then
                        testPointPrinting = False
                        main_testPointMessage = ""
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '
        '====================================================================================================
        ''' <summary>
        ''' version for cpCore assembly
        ''' </summary>
        ''' <remarks></remarks>
        Public Function codeVersion() As String
            Dim myType As Type = GetType(coreClass)
            Dim myAssembly As Assembly = Assembly.GetAssembly(myType)
            Dim myAssemblyname As AssemblyName = myAssembly.GetName()
            Dim myVersion As Version = myAssemblyname.Version
            Return Format(myVersion.Major, "0") & "." & Format(myVersion.Minor, "00") & "." & Format(myVersion.Build, "00000000")
        End Function

        '
        '==========================================================================================
        ''' <summary>
        ''' return an html ul list of each eception produced during this document.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function getDocExceptionHtmlList() As String
            Dim returnHtmlList As String = ""
            Try
                If Not exceptionList Is Nothing Then
                    If exceptionList.Count > 0 Then
                        For Each exMsg As String In exceptionList
                            returnHtmlList &= cr2 & "<li class=""ccExceptionListRow"">" & cr3 & htmlDoc.html_convertText2HTML(exMsg) & cr2 & "</li>"
                        Next
                        returnHtmlList = cr & "<ul class=""ccExceptionList"">" & returnHtmlList & cr & "</ul>"
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnHtmlList
        End Function
        '
        '==========================================================================================
        ''' <summary>
        ''' Generic handle exception. Determines method name and class of caller from stack. 
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="ex"></param>
        ''' <param name="cause"></param>
        ''' <param name="stackPtr">How far down in the stack to look for the method error. Pass 1 if the method calling has the error, 2 if there is an intermediate routine.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Sub handleException(ByVal ex As Exception, ByVal cause As String, stackPtr As Integer)
            If (Not _handlingExceptionRecursionBlock) Then
                _handlingExceptionRecursionBlock = True
                Dim frame As StackFrame = New StackFrame(stackPtr)
                Dim method As System.Reflection.MethodBase = frame.GetMethod()
                Dim type As System.Type = method.DeclaringType()
                Dim methodName As String = method.Name
                Dim errMsg As String = type.Name & "." & methodName & ", cause=[" & cause & "], ex=[" & ex.ToString & "]"
                '
                ' append to application event log
                '
                Dim sSource As String = "Contensive"
                Dim sLog As String = "Application"
                Dim eventId As Integer = 1001
                Try
                    '
                    ' if command line has been run on this server, this will work. Otherwise skip
                    '
                    EventLog.WriteEntry(sSource, errMsg, EventLogEntryType.Error, eventId)
                Catch exEvent As Exception
                    ' ignore error. Can be caused if source has not been created. It is created automatically in command line installation util.
                End Try
                '
                ' append to daily trace log
                '
                logController.appendLog(Me, errMsg)
                '
                ' add to doc exception list to display at top of webpage
                '
                If exceptionList Is Nothing Then
                    exceptionList = New List(Of String)
                End If
                If exceptionList.Count = 10 Then
                    exceptionList.Add("Exception limit exceeded")
                ElseIf exceptionList.Count < 10 Then
                    exceptionList.Add(errMsg)
                End If
                '
                ' write consol for debugging
                '
                Console.WriteLine(errMsg)
                '
                _handlingExceptionRecursionBlock = False
            End If
        End Sub
        Private _handlingExceptionRecursionBlock As Boolean = False
        ''
        ''====================================================================================================
        ''
        'Public Sub throw (ByVal ex As Exception, ByVal cause As String)
        '    Call handleException(ex, cause, 2)
        '    Throw ex
        'End Sub

        '
        Public Sub handleExceptionAndContinue(ByVal ex As Exception, ByVal cause As String)
            Call handleException(ex, cause, 2)
        End Sub
        '
        Public Sub handleExceptionAndContinue(ByVal ex As Exception)
            Call handleException(ex, "n/a", 2)
        End Sub
        '
        '========================================================================
        ''' <summary>
        ''' handle expection with legacy log line (v2)
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="ex"></param>
        ''' <param name="className"></param>
        ''' <param name="methodName"></param>
        ''' <param name="cause"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function handleExceptionLegacyRow2(ByVal ex As Exception, ByVal className As String, ByVal methodName As String, ByVal cause As String) As String
            Call handleException(ex, cause, 2)
            'Try
            '    Dim errMsg As String = className & "." & methodName & ", cause=[" & cause & "], ex=[" & ex.ToString & "]"
            '    Console.WriteLine(errMsg)
            '    appendLog(errMsg)
            'Catch exIgnore As Exception
            '    '
            'End Try
        End Function
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v1
        '''' </summary>
        '''' <param name="cpCore"></param>
        '''' <param name="ClassName"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="ErrNumber"></param>
        '''' <param name="ErrSource"></param>
        '''' <param name="ErrDescription"></param>
        '''' <param name="ErrorTrap"></param>
        '''' <param name="ResumeNext"></param>
        '''' <param name="URL"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError(ByVal ClassName As String, ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean, Optional ByVal URL As String = "")
        '    handleException(New Exception("Legacy error raised, className=[" & ClassName & "], methodName=[" & MethodName & "], url=[" & URL & "] [legacy error #" & ErrNumber & "," & ErrSource & "," & ErrDescription & "]"), "n/a", 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v2
        '''' </summary>
        '''' <param name="cpCore"></param>
        '''' <param name="ClassName"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="Description"></param>
        '''' <param name="ErrorNumber"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError2(ByVal ClassName As String, ByVal MethodName As String, Optional ByVal Description As String = "", Optional ByVal ErrorNumber As Integer = 0)
        '    handleException(New Exception("Legacy error, ClassName=[" & ClassName & "], MethodName=[" & MethodName & "], Description=[" & Description & "], [legacy error #" & ErrorNumber & "," & Description & "]"), "n/a", 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v3
        '''' </summary>
        '''' <param name="cpCore"></param>
        '''' <param name="ContensiveAppName"></param>
        '''' <param name="Context"></param>
        '''' <param name="ProgramName"></param>
        '''' <param name="ClassName"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="ErrNumber"></param>
        '''' <param name="ErrSource"></param>
        '''' <param name="ErrDescription"></param>
        '''' <param name="ErrorTrap"></param>
        '''' <param name="ResumeNext"></param>
        '''' <param name="URL"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError3(ByVal ContensiveAppName As String, ByVal Context As String, ByVal ProgramName As String, ByVal ClassName As String, ByVal MethodName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean, ByVal URL As String)
        '    handleException(New Exception("Legacy error, ContensiveAppName=[" & ContensiveAppName & "], Context=[" & Context & "], ProgramName=[" & ProgramName & "], ClassName=[" & ClassName & "], MethodName=[" & MethodName & "], [legacy error #" & ErrNumber & "," & ErrSource & "," & ErrDescription & "]"), Context, 2)
        '    'Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''======================================================================
        '''' <summary>
        '''' handle legacy errors, v3
        '''' </summary>
        '''' <param name="ErrNumber"></param>
        '''' <param name="ErrSource"></param>
        '''' <param name="ErrDescription"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="ErrorTrap"></param>
        '''' <param name="ResumeNext"></param>
        '''' <param name="Context"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError4(ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ignore As Boolean = False, Optional ByVal Context As String = "")
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], Context=[" & Context & "] raised, [legacy error #" & ErrNumber & "," & ErrSource & "," & ErrDescription & "]"), Context, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''======================================================================
        '''' <summary>
        '''' handle legacy errors, v5
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <param name="Err_Number"></param>
        '''' <param name="Err_Source"></param>
        '''' <param name="Err_Description"></param>
        '''' <param name="ResumeNext"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError5(MethodName As String, Cause As String, Err_Number As Integer, Err_Source As String, Err_Description As String, ignore As Boolean)
        '    handleException(New Exception("Legacy error raised, [legacy error #" & Err_Number & "," & Err_Source & "," & Err_Description & "]"), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''======================================================================
        '''' <summary>
        '''' handle legacy errors, v6
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError6(MethodName As String, Cause As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''======================================================================
        '''' <summary>
        '''' handle legacy errors, v7
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError7(MethodName As String, Cause As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''======================================================================
        '''' <summary>
        '''' handle legacy errors, v8
        '''' </summary>
        '''' <param name="Cause"></param>
        '''' <param name="Source"></param>
        '''' <param name="ResumeNext"></param>
        '''' <returns></returns>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError8(ByVal Cause As String, Optional ByVal ignore As String = "", Optional ByVal ignore2 As Boolean = False)
        '    handleException(New Exception("Legacy error, cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v10
        '''' </summary>
        '''' <param name="Err_Number"></param>
        '''' <param name="Err_Source"></param>
        '''' <param name="Err_Description"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="ErrorTrap"></param>
        '''' <param name="ResumeNext"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError10(ByVal Err_Number As Integer, ByVal Err_Source As String, ByVal Err_Description As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, ByVal ResumeNext As Boolean)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "] [legacy error #" & Err_Number & "," & Err_Source & "," & Err_Description & "]"), "n/a", 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v11
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError11(ByVal MethodName As String, ByVal Cause As String)
        '    handleException(New Exception("Legacy error #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v12
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError12(ByVal MethodName As String, ByVal Cause As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v13
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError13(ByVal MethodName As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), "n/a", 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v14
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError14(ByVal MethodName As String, ByVal Cause As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v15
        '''' </summary>
        '''' <param name="Cause"></param>
        '''' <param name="MethodName"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError15(ByVal Cause As String, ByVal MethodName As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v16
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError16(ByVal MethodName As String, ByVal Cause As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v17
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError17(ByVal MethodName As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), "n/a", 2)
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v18
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError18(ByVal MethodName As String)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), "n/a", 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v19
        '''' </summary>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <param name="Err_Number"></param>
        '''' <param name="Err_Source"></param>
        '''' <param name="Err_Description"></param>
        '''' <param name="ResumeNext"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError19(ByVal MethodName As String, ByVal Cause As String, ByVal Err_Number As Integer, ByVal Err_Source As String, ByVal Err_Description As String, ByVal ResumeNext As Boolean)
        '    handleException(New Exception("Legacy error, MethodName=[" & MethodName & "], cause=[" & Cause & "] #" & Err_Number & "," & Err_Source & "," & Err_Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v20
        '''' </summary>
        '''' <param name="appName"></param>
        '''' <param name="ClassName"></param>
        '''' <param name="MethodName"></param>
        '''' <param name="Cause"></param>
        '''' <param name="Err_Number"></param>
        '''' <param name="Err_Source"></param>
        '''' <param name="Err_Description"></param>
        '''' <param name="WillResumeAfterLogging"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError20(ByVal appName As String, ByVal ClassName As String, ByVal MethodName As String, ByVal Cause As String, ByVal Err_Number As Integer, ByVal Err_Source As String, ByVal Err_Description As String, ByVal WillResumeAfterLogging As Boolean)
        '    handleException(New Exception("Legacy error, app=[" & appName & "], classname=[" & ClassName & "], methodname=[" & MethodName & "], cause=[" & Cause & "] #" & Err_Number & "," & Err_Source & "," & Err_Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        '''
        '''========================================================================
        ''''' <summary>
        ''''' handle legacy errors, v21
        ''''' </summary>
        ''''' <param name="ErrorObject"></param>
        ''''' <param name="Cause"></param>
        ''''' <remarks></remarks>
        ''Public Sub handleLegacyError21(ByVal ErrorObject As Object, ByVal Cause As String)
        ''    handleException(New Exception("Legacy error, cause=[" & Cause & "] #" & ErrorObject.Number & "," & ErrorObject.Source & "," & ErrorObject.Description & ""), Cause, 2)
        ''    Throw New ApplicationException("handleLegacyError")
        ''End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v22
        '''' </summary>
        '''' <param name="ErrorObject"></param>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Friend Sub handleLegacyError22(ByVal ErrorObject As ErrObject, ByVal Cause As String)
        '    handleException(New Exception("Legacy error, cause=[" & Cause & "] #" & ErrorObject.Number & "," & ErrorObject.Source & "," & ErrorObject.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        ''
        ''========================================================================
        '''' <summary>
        '''' handle legacy errors, v23
        '''' </summary>
        '''' <param name="Cause"></param>
        '''' <remarks></remarks>
        'Public Sub handleLegacyError23(ByVal Cause As String)
        '    handleException(New Exception("Legacy error, cause=[" & Cause & "] #" & Err.Number & "," & Err.Source & "," & Err.Description & ""), Cause, 2)
        '    Throw New ApplicationException("handleLegacyError")
        'End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        ''' </summary>
        ''' <param name="groupName"></param>
        ''' <returns></returns>
        Public Function group_add(ByVal groupName As String) As Integer
            Dim returnGroupId As Integer = 0
            Try
                Dim dt As DataTable
                Dim sql As String
                Dim createkey As Integer
                Dim cid As Integer
                Dim sqlGroupName As String = db.encodeSQLText(groupName)
                '
                dt = db.executeSql("SELECT ID FROM CCGROUPS WHERE NAME=" & sqlGroupName & "")
                If dt.Rows.Count > 0 Then
                    returnGroupId = genericController.EncodeInteger(dt.Rows(0).Item("ID"))
                Else
                    cid = metaData.getContentId("groups")
                    createkey = genericController.GetRandomInteger()
                    sql = "insert into ccgroups (contentcontrolid,active,createkey,name,caption) values (" & cid & ",1," & createkey & "," & sqlGroupName & "," & sqlGroupName & ")"
                    Call db.executeSql(sql)
                    '
                    sql = "select top 1 id from ccgroups where createkey=" & createkey & " order by id desc"
                    dt = db.executeSql(sql)
                    If dt.Rows.Count > 0 Then
                        returnGroupId = genericController.EncodeInteger(dt.Rows(0).Item(0))
                    End If
                End If
                dt.Dispose()
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnGroupId
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Create a group and return its Id. If the group already exists, the groups Id is returned. If the group cannot be added a 0 is returned.
        ''' </summary>
        ''' <param name="GroupNameOrGuid"></param>
        ''' <param name="groupCaption"></param>
        ''' <returns></returns>
        Public Function group_add2(ByVal GroupNameOrGuid As String, Optional ByVal groupCaption As String = "") As Integer
            Dim returnGroupId As Integer = 0
            Try
                '
                Dim cs As New csController(Me)
                Dim IsAlreadyThere As Boolean = False
                Dim sqlCriteria As String = db.getNameIdOrGuidSqlCriteria(GroupNameOrGuid)
                Dim groupName As String
                Dim groupGuid As String
                '
                If (GroupNameOrGuid = "") Then
                    Throw (New ApplicationException("A group cannot be added with a blank name"))
                Else
                    cs.open("Groups", sqlCriteria, , False, "id")
                    IsAlreadyThere = cs.ok
                    Call cs.Close()
                    If Not IsAlreadyThere Then
                        Call cs.Insert("Groups")
                        If Not cs.ok Then
                            Throw (New ApplicationException("There was an error inserting a new group record"))
                        Else
                            returnGroupId = cs.getInteger("id")
                            If genericController.isGuid(GroupNameOrGuid) Then
                                groupName = "Group " & cs.getInteger("id")
                                groupGuid = GroupNameOrGuid
                            Else
                                groupName = GroupNameOrGuid
                                groupGuid = Guid.NewGuid().ToString()
                            End If
                            If groupCaption = "" Then
                                groupCaption = groupName
                            End If
                            Call cs.setField("name", groupName)
                            Call cs.setField("caption", groupCaption)
                            Call cs.setField("ccGuid", groupGuid)
                            Call cs.setField("active", "1")
                        End If
                        Call cs.Close()
                    End If
                End If
            Catch ex As Exception
                Throw New ApplicationException("Unexpected error in cp.group.add()", ex)
            End Try
            Return returnGroupId
        End Function
        '
        '====================================================================================================
        '
        ' Add User
        '
        Public Sub group_addUser(ByVal groupId As Integer, Optional ByVal userid As Integer = 0, Optional ByVal dateExpires As Date = #12:00:00 AM#)
            Try
                '
                Dim groupName As String
                '
                If True Then
                    If (groupId < 1) Then
                        Throw (New ApplicationException("Could not find or create the group with id [" & groupId & "]"))
                    Else
                        If userid = 0 Then
                            userid = authContext.user.ID
                        End If
                        Using cs As New csController(Me)
                            cs.open("Member Rules", "(MemberID=" & userid.ToString & ")and(GroupID=" & groupId.ToString & ")", , False)
                            If Not cs.ok Then
                                Call cs.Close()
                                Call cs.Insert("Member Rules")
                            End If
                            If Not cs.ok Then
                                groupName = GetRecordName("groups", groupId)
                                Throw (New ApplicationException("Could not find or create the Member Rule to add this member [" & userid & "] to the Group [" & groupId & ", " & groupName & "]"))
                            Else
                                Call cs.setField("active", "1")
                                Call cs.setField("memberid", userid.ToString)
                                Call cs.setField("groupid", groupId.ToString)
                                If dateExpires <> #12:00:00 AM# Then
                                    Call cs.setField("DateExpires", dateExpires.ToString)
                                Else
                                    Call cs.setField("DateExpires", "")
                                End If
                            End If
                            Call cs.Close()
                        End Using
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Sub group_AddUser(ByVal groupNameOrGuid As String, Optional ByVal userid As Integer = 0, Optional ByVal dateExpires As Date = #12:00:00 AM#)
            Try
                '
                Dim GroupID As Integer
                '
                If groupNameOrGuid <> "" Then
                    GroupID = db.getRecordID("groups", groupNameOrGuid)
                    If (GroupID < 1) Then
                        Call group_add2(groupNameOrGuid)
                        GroupID = db.getRecordID("groups", groupNameOrGuid)
                    End If
                    If (GroupID < 1) Then
                        Throw (New ApplicationException("Could not find or create the group [" & groupNameOrGuid & "]"))
                    Else
                        If userid = 0 Then
                            userid = authContext.user.ID
                        End If
                        Using cs As New csController(Me)
                            cs.open("Member Rules", "(MemberID=" & userid.ToString & ")and(GroupID=" & GroupID.ToString & ")", , False)
                            If Not cs.ok Then
                                Call cs.Close()
                                Call cs.Insert("Member Rules")
                            End If
                            If Not cs.ok Then
                                Throw (New ApplicationException("Could not find or create the Member Rule to add this member [" & userid & "] to the Group [" & GroupID & ", " & groupNameOrGuid & "]"))
                            Else
                                Call cs.setField("active", "1")
                                Call cs.setField("memberid", userid.ToString)
                                Call cs.setField("groupid", GroupID.ToString)
                                If dateExpires <> #12:00:00 AM# Then
                                    Call cs.setField("DateExpires", dateExpires.ToString)
                                Else
                                    Call cs.setField("DateExpires", "")
                                End If
                            End If
                            Call cs.Close()
                        End Using
                    End If
                End If
            Catch ex As Exception
                Throw (ex)
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns true if the argument is a string in guid compatible format
        ''' </summary>
        ''' <param name="guid"></param>
        ''' <returns></returns>
        Public Function common_isGuid(guid As String) As Boolean
            Dim returnIsGuid As Boolean = False
            Try
                returnIsGuid = (Len(guid) = 38) And (Left(guid, 1) = "{") And (Right(guid, 1) = "}")
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnIsGuid
        End Function
        '
        '============================================================================
        '
        Public Function common_getHttpRequest(url As String) As IO.Stream
            Dim returnstream As IO.Stream = Nothing
            Try
                Dim rq As System.Net.WebRequest
                Dim response As System.Net.WebResponse
                '
                rq = System.Net.WebRequest.Create(url)
                rq.Timeout = 60000
                response = rq.GetResponse()
                returnstream = response.GetResponseStream()
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnstream
        End Function
        '
        '============================================================================
        '
        Public Shared Function encodeSqlTableName(sourceName As String) As String
            Dim returnName As String = ""
            Const FirstCharSafeString As String = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ"
            Const SafeString As String = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ_@#"
            Try
                Dim src As String
                Dim TestChr As String
                Dim Ptr As Integer = 0
                '
                ' remove nonsafe URL characters
                '
                src = sourceName
                returnName = ""
                ' first character
                Do While Ptr < src.Length
                    TestChr = src.Substring(Ptr, 1)
                    Ptr += 1
                    If FirstCharSafeString.IndexOf(TestChr) >= 0 Then
                        returnName &= TestChr
                        Exit Do
                    End If
                Loop
                ' non-first character
                Do While Ptr < src.Length
                    TestChr = src.Substring(Ptr, 1)
                    Ptr += 1
                    If SafeString.IndexOf(TestChr) >= 0 Then
                        returnName &= TestChr
                    End If
                Loop
            Catch ex As Exception
                ' shared method, rethrow error
                Throw New ApplicationException("Exception in encodeSqlTableName(" & sourceName & ")", ex)
            End Try
            Return returnName
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return the docProperties collection as the legacy optionString
        ''' </summary>
        ''' <returns></returns>
        Public Function getLegacyOptionStringFromVar() As String
            Dim returnString As String = ""
            Try
                For Each key As String In docProperties.getKeyList
                    With docProperties.getProperty(key)
                        returnString &= "" & "&" & encodeLegacyAddonOptionArgument(key) & "=" & encodeLegacyAddonOptionArgument(.Value)
                    End With
                Next
            Catch ex As Exception
                Throw (ex)
            End Try
            Return returnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        ''' Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        ''' call this before parsing them together
        ''' call decodeAddonOptionArgument after parsing them apart
        ''' </summary>
        ''' <param name="Arg"></param>
        ''' <returns></returns>
        '------------------------------------------------------------------------------------------------------------
        '
        Private Function encodeLegacyAddonOptionArgument(ByVal Arg As String) As String
            Dim a As String = ""
            If Not String.IsNullOrEmpty(Arg) Then
                a = Arg
                a = genericController.vbReplace(a, vbCrLf, "#0013#")
                a = genericController.vbReplace(a, vbLf, "#0013#")
                a = genericController.vbReplace(a, vbCr, "#0013#")
                a = genericController.vbReplace(a, "&", "#0038#")
                a = genericController.vbReplace(a, "=", "#0061#")
                a = genericController.vbReplace(a, ",", "#0044#")
                a = genericController.vbReplace(a, """", "#0034#")
                a = genericController.vbReplace(a, "'", "#0039#")
                a = genericController.vbReplace(a, "|", "#0124#")
                a = genericController.vbReplace(a, "[", "#0091#")
                a = genericController.vbReplace(a, "]", "#0093#")
                a = genericController.vbReplace(a, ":", "#0058#")
            End If
            Return a
        End Function
        '
        '====================================================================================================
        '
        Public Function createGuid() As String
            Return "{" & Guid.NewGuid().ToString & "}"
        End Function
        '
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
            'Exit Sub

            Dim SQL As String
            Dim ViewingName As String
            Dim CSMax As Integer
            Dim PageID As Integer
            Dim FieldNames As String
            Dim Form As String
            '
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    ' delete tmp files
                    '
                    If deleteOnDisposeFileList.Count > 0 Then
                        For Each filename As String In deleteOnDisposeFileList
                            privateFiles.deleteFile(filename)
                        Next
                    End If
                    '
                    ' ----- Block all output from underlying routines
                    '
                    blockExceptionReporting = True
                    'docOpen = False
                    Call doc_close()
                    '
                    ' content server object is valid
                    '
                    If (serverConfig.appConfig IsNot Nothing) Then
                        If siteProperties.allowVisitTracking Then
                            '
                            ' If visit tracking, save the viewing record
                            '
                            ViewingName = Left(authContext.visit.ID & "." & authContext.visit.PageVisits, 10)
                            PageID = pages.currentPageID
                            FieldNames = "Name,VisitId,MemberID,Host,Path,Page,QueryString,Form,Referer,DateAdded,StateOK,ContentControlID,pagetime,Active,CreateKey,RecordID"
                            FieldNames = FieldNames & ",ExcludeFromAnalytics"
                            FieldNames = FieldNames & ",pagetitle"
                            SQL = "INSERT INTO ccViewings (" _
                                & FieldNames _
                                & ")VALUES(" _
                                & " " & db.encodeSQLText(ViewingName) _
                                & "," & db.encodeSQLNumber(authContext.visit.ID) _
                                & "," & db.encodeSQLNumber(authContext.user.ID) _
                                & "," & db.encodeSQLText(webServer.requestDomain) _
                                & "," & db.encodeSQLText(webServer.webServerIO_requestPath) _
                                & "," & db.encodeSQLText(webServer.webServerIO_requestPage) _
                                & "," & db.encodeSQLText(Left(webServer.requestQueryString, 255)) _
                                & "," & db.encodeSQLText(Left(Form, 255)) _
                                & "," & db.encodeSQLText(Left(webServer.requestReferrer, 255)) _
                                & "," & db.encodeSQLDate(app_startTime) _
                                & "," & db.encodeSQLBoolean(authContext.visit_stateOK) _
                                & "," & db.encodeSQLNumber(main_GetContentID("Viewings")) _
                                & "," & db.encodeSQLNumber(appStopWatch.ElapsedMilliseconds) _
                                & ",1" _
                                & "," & db.encodeSQLNumber(CSMax) _
                                & "," & db.encodeSQLNumber(PageID)
                            SQL &= "," & db.encodeSQLBoolean(webServer.webServerIO_PageExcludeFromAnalytics)
                            SQL &= "," & db.encodeSQLText(htmlDoc.main_MetaContent_Title)
                            SQL &= ");"
                            Call db.executeSql(SQL)
                            'Call db.executeSqlAsync(SQL)
                        End If
                    End If
                    '
                    ' ----- dispose objects created here
                    '
                    If Not (_addonCache Is Nothing) Then
                        ' no dispose
                        'Call _addonCache.Dispose()
                        _addonCache = Nothing
                    End If
                    '
                    If Not (_addon Is Nothing) Then
                        Call _addon.Dispose()
                        _addon = Nothing
                    End If
                    '
                    If Not (_db Is Nothing) Then
                        Call _db.Dispose()
                        _db = Nothing
                    End If
                    '
                    If Not (_metaData Is Nothing) Then
                        Call _metaData.Dispose()
                        _metaData = Nothing
                    End If
                    '
                    If Not (_cache Is Nothing) Then
                        Call _cache.Dispose()
                        _cache = Nothing
                    End If
                    '
                    If Not (_workflow Is Nothing) Then
                        Call _workflow.Dispose()
                        _workflow = Nothing
                    End If
                    '
                    If Not (_siteProperties Is Nothing) Then
                        ' no dispose
                        'Call _siteProperties.Dispose()
                        _siteProperties = Nothing
                    End If
                    '
                    If Not (_json Is Nothing) Then
                        ' no dispose
                        'Call _json.Dispose()
                        _json = Nothing
                    End If
                    ''
                    'If Not (_user Is Nothing) Then
                    '    ' no dispose
                    '    'Call _user.Dispose()
                    '    _user = Nothing
                    'End If
                    '
                    If Not (_domains Is Nothing) Then
                        ' no dispose
                        'Call _domains.Dispose()
                        _domains = Nothing
                    End If
                    '
                    If Not (_doc Is Nothing) Then
                        ' no dispose
                        'Call _doc.Dispose()
                        _doc = Nothing
                    End If
                    '
                    If Not (_security Is Nothing) Then
                        ' no dispose
                        'Call _security.Dispose()
                        _security = Nothing
                    End If
                    '
                    If Not (_webServer Is Nothing) Then
                        ' no dispose
                        'Call _webServer.Dispose()
                        _webServer = Nothing
                    End If
                    '
                    If Not (_menuFlyout Is Nothing) Then
                        ' no dispose
                        'Call _menuFlyout.Dispose()
                        _menuFlyout = Nothing
                    End If
                    '
                    If Not (_visitProperty Is Nothing) Then
                        ' no dispose
                        'Call _visitProperty.Dispose()
                        _visitProperty = Nothing
                    End If
                    '
                    If Not (_visitorProperty Is Nothing) Then
                        ' no dispose
                        'Call _visitorProperty.Dispose()
                        _visitorProperty = Nothing
                    End If
                    '
                    If Not (_userProperty Is Nothing) Then
                        ' no dispose
                        'Call _userProperty.Dispose()
                        _userProperty = Nothing
                    End If
                    '
                    If Not (_db Is Nothing) Then
                        Call _db.Dispose()
                        _db = Nothing
                    End If
                    '
                    If Not (_metaData Is Nothing) Then
                        _metaData.Dispose()
                        _metaData = Nothing
                    End If
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region        '
    End Class

End Namespace
