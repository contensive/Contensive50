
Option Explicit On
Option Strict On

Imports System.Runtime.InteropServices
Imports System.Net

Imports System.Text.RegularExpressions
'
Namespace Contensive.Core
    '
    Public Module taskQueueCommandEnumModule
        Public Const runAddon As String = "runaddon"
    End Module
    '
    Public Class cmdDetailClass
        Public addonId As Integer
        Public addonName As String
        Public docProperties As Dictionary(Of String, String)
    End Class
    '
    '-----------------------------------------------------------------------
    '   legacy mainClass arguments
    '   REFACTOR - organize and rename
    '-----------------------------------------------------------------------
    '
    Public Structure NameValuePrivateType
        Dim Name As String
        Dim Value As String
    End Structure
    '
    '====================================================================================================
    '
    Public Class docPropertiesClass
        Public Name As String
        Public Value As String
        Public NameValue As String
        Public IsForm As Boolean
        Public IsFile As Boolean
        'Public FileContent() As Byte
        Public tmpPrivatePathfilename As String
        Public FileSize As Integer
        Public fileType As String
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' application configuration class
    ''' </summary>
    Public Class appConfigClass
        Public name As String
        Public enabled As Boolean
        Public privateKey As String                     ' rename hashKey
        Public defaultConnectionString As String
        Public appRootFilesPath As String               ' local file path to the appRoot (i.e. d:\inetpub\myApp\wwwRoot\)
        Public cdnFilesPath As String                   ' local file path to the content files (i.e. d:\inetpub\myApp\files\)
        Public privateFilesPath As String               ' local file path to the content files (i.e. d:\inetpub\myApp\private\)
        Public cdnFilesNetprefix As String              ' in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/appName/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
        Public allowSiteMonitor As Boolean
        Public domainList As New List(Of String)        ' primary domain is the first item in the list
        Public enableCache As Boolean
        Public adminRoute As String                                          ' The url pathpath that executes the addon site
    End Class    '
    '
    '====================================================================================================
    ''' <summary>
    ''' Holds location on the server of the clusterConfig file. Physically stored at programDataFolder/clib/serverConfig.json
    ''' </summary>
    Public Class serverConfigClass
        'Public clusterPath As String
        Public allowTaskRunnerService As Boolean
        Public allowTaskSchedulerService As Boolean
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' cluster configuration class - deserialized configration file
    ''' </summary>
    ''' <remarks></remarks>
    Public Class clusterConfigClass
        Public isLocal As Boolean = True
        Public name As String = ""
        '
        ' local caching using dotnet framework, flushes on appPool
        '
        Public isLocalCache As Boolean = False
        '
        ' AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object.
        ' this is the srver:port to the config file it uses.
        '
        Public awsElastiCacheConfigurationEndpoint As String
        '
        ' datasource for the cluster
        '
        Public defaultDataSourceType As dataSourceTypeEnum
        '
        ' odbc
        '
        Public defaultDataSourceODBCConnectionString As String
        '
        ' native
        '
        Public defaultDataSourceAddress As String = ""
        '
        ' user for creating new databases, and creating the new user for the database during site create, and saved to appconfig
        '
        Public defaultDataSourceUsername As String = ""
        Public defaultDataSourcePassword As String = ""
        '
        ' endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
        '
        Public clusterFilesEndpoint As String
        '
        ' configuration of async command listener on render machines (not sure if used still)
        '
        Public serverListenerPort As Integer = Port_ContentServerControlDefault
        Public maxConcurrentTasksPerServer As Integer = 5
        ' ayncCmd server authentication -- change this to a key later
        Public username As String = ""
        Public password As String = ""
        '
        ' This is the root path to the localCluster files, typically getLocalDataFolder (d:\inetpub)
        '   if isLocal, the cluster runs from these files
        '   if not, this is the local mirror of the cluster files
        '
        'Public clusterPhysicalPath As String
        '
        'Public domainRoutes As Dictionary(Of String, String)
        '
        Public appPattern As String
        '
        '
        '
        Public apps As New Dictionary(Of String, appConfigClass)
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
    ''' </summary>
    Public Class MiniCollectionClass
        Implements ICloneable
        '
        ' Content Definitions (some data in CDef, some in the CDef extension)
        '
        Public isBaseCollection As Boolean                     ' true only for the one collection created from the base file. This property does not transfer during addSrcToDst
        Public CDef As New Dictionary(Of String, coreMetaDataClass.CDefClass)
        Public SQLIndexCnt As Integer
        Public SQLIndexes() As SQLIndexType
        Public Structure SQLIndexType
            Dim DataSourceName As String
            Dim TableName As String
            Dim IndexName As String
            Dim FieldNameList As String
            Dim dataChanged As Boolean
        End Structure
        Public MenuCnt As Integer
        Public Menus() As MenusType
        Public Structure MenusType
            Dim Name As String
            Dim IsNavigator As Boolean
            Dim menuNameSpace As String
            Dim ParentName As String
            Dim ContentName As String
            Dim LinkPage As String
            Dim SortOrder As String
            Dim AdminOnly As Boolean
            Dim DeveloperOnly As Boolean
            Dim NewWindow As Boolean
            Dim Active As Boolean
            Dim AddonName As String
            Dim dataChanged As Boolean
            Dim Guid As String
            Dim NavIconType As String
            Dim NavIconTitle As String
            Dim Key As String
        End Structure
        Public AddOns() As AddOnType
        Public Structure AddOnType
            Dim Name As String
            Dim Link As String
            Dim ObjectProgramID As String
            Dim ArgumentList As String
            Dim SortOrder As String
            Dim Copy As String
            Dim dataChanged As Boolean
        End Structure
        Public AddOnCnt As Integer
        Public Styles() As StyleType
        Public Structure StyleType
            Dim Name As String
            Dim Overwrite As Boolean
            Dim Copy As String
            Dim dataChanged As Boolean
        End Structure
        Public StyleCnt As Integer
        Public StyleSheet As String
        Public ImportCnt As Integer
        Public collectionImports() As ImportCollectionType
        Public Structure ImportCollectionType
            Dim Name As String
            Dim Guid As String
        End Structure
        Public PageTemplateCnt As Integer
        Public PageTemplates() As PageTemplateType
        '   Page Template - started, but CDef2 and LoadDataToCDef are all that is done do far
        Public Structure PageTemplateType
            Dim Name As String
            Dim Copy As String
            Dim Guid As String
            Dim Style As String
        End Structure
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone
        End Function
    End Class
    '
    ' ---------------------------------------------------------------------------------------------------
    ' ----- DataSourceType
    '       class not structure because it has to marshall to vb6
    ' ---------------------------------------------------------------------------------------------------
    '
    Public Class dataSourceClass
        Public name As String
        Public nameLower As String
        Public id As Integer
        Public dataSourceType As dataSourceTypeEnum
        Public endPoint As String
        Public username As String
        Public password As String
        Public connectionStringOLEDB As String
    End Class
    '
    Public Enum dataSourceTypeEnum
        sqlServerOdbc = 1
        sqlServerNative = 2
        mySqlNative = 3
    End Enum
    '------------------------------------------------------------------------
    ' Moved here from ccCommon so it could be used in argument to csv_getStyleSheet2
    '------------------------------------------------------------------------
    '
    ' renamed for now because error : ambiguous name if pubic is csv and legacycmc.csv_
    ' csv class holds the real public version.
    ' later, rename backfile

    Public Module coreCommonModule
        '
        Public Const sqlAddonStyles As String = "select addonid,styleid from ccSharedStylesAddonRules where (active<>0) order by id"
        '
        Public Const cacheNameAddonStyleRules As String = "addon styles"
        '
        Public Const ALLOWLEGACYAPI As Boolean = False
        Public Const ALLOWPROFILING As Boolean = False
        '
        ' put content definitions here
        '
        '
        Public Structure NameValuePairType
            Public Name As String
            Public Value As String
        End Structure
        '
        '========================================================================
        '   defined errors (event log eventId)
        '       1000-1999 Contensive
        '       2000-2999 Datatree
        '
        '   see kmaErrorDescription() for transations
        '========================================================================
        '
        Const Error_DataTree_RootNodeNext = 2000
        Const Error_DataTree_NoGoNext = 2001
        '
        '========================================================================
        '
        '========================================================================
        '
        Declare Function GetTickCount Lib "kernel32" () As Integer
        Declare Function GetCurrentProcessId Lib "kernel32" () As Integer
        'Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        '
        '========================================================================
        '   Declarations for SetPiorityClass
        '========================================================================
        '
        'Const THREAD_BASE_PRIORITY_IDLE = -15
        'Const THREAD_BASE_PRIORITY_LOWRT = 15
        'Const THREAD_BASE_PRIORITY_MIN = -2
        'Const THREAD_BASE_PRIORITY_MAX = 2
        'Const THREAD_PRIORITY_LOWEST = THREAD_BASE_PRIORITY_MIN
        'Const THREAD_PRIORITY_HIGHEST = THREAD_BASE_PRIORITY_MAX
        'Const THREAD_PRIORITY_BELOW_NORMAL = (THREAD_PRIORITY_LOWEST + 1)
        'Const THREAD_PRIORITY_ABOVE_NORMAL = (THREAD_PRIORITY_HIGHEST - 1)
        'Const THREAD_PRIORITY_IDLE = THREAD_BASE_PRIORITY_IDLE
        'Const THREAD_PRIORITY_NORMAL = 0
        'Const THREAD_PRIORITY_TIME_CRITICAL = THREAD_BASE_PRIORITY_LOWRT
        'Const HIGH_PRIORITY_CLASS = &H80
        'Const IDLE_PRIORITY_CLASS = &H40
        'Const NORMAL_PRIORITY_CLASS = &H20
        'Const REALTIME_PRIORITY_CLASS = &H100
        '
        'Private Declare Function SetThreadPriority Lib "kernel32" (ByVal hThread As Integer, ByVal nPriority As Integer) As Integer
        'Private Declare Function SetPriorityClass Lib "kernel32" (ByVal hProcess As Integer, ByVal dwPriorityClass As Integer) As Integer
        'Private Declare Function GetThreadPriority Lib "kernel32" (ByVal hThread As Integer) As Integer
        'Private Declare Function GetPriorityClass Lib "kernel32" (ByVal hProcess As Integer) As Integer
        'Private Declare Function GetCurrentThread Lib "kernel32" () As Integer
        'Private Declare Function GetCurrentProcess Lib "kernel32" () As Integer
        '

        '
        '========================================================================
        'Converts unsafe characters,
        'such as spaces, into their
        'corresponding escape sequences.
        '========================================================================
        '
        'Declare Function UrlEscape Lib "shlwapi" _
        '   Alias "UrlEscapeA" _
        '  (ByVal pszURL As String, _
        '   ByVal pszEscaped As String, _
        '   ByVal pcchEscaped As Integer, _
        '   ByVal dwFlags As Integer) As Integer
        ''
        ''Converts escape sequences back into
        ''ordinary characters.
        ''
        'Declare Function UrlUnescape Lib "shlwapi" _
        '   Alias "UrlUnescapeA" _
        '  (ByVal pszURL As String, _
        '   ByVal pszUnescaped As String, _
        '   ByVal pcchUnescaped As Integer, _
        '   ByVal dwFlags As Integer) As Integer

        '
        '   Error reporting strategy
        '       Popups are pop-up boxes that tell the user what to do
        '       Logs are files with error details for developers to use debugging
        '
        '       Attended Programs
        '           - errors that do not effect the operation, resume next
        '           - all errors trickle up to the user interface level
        '           - User Errors, like file not found, return "UserError" code and a description
        '           - Internal Errors, like div-by-0, User should see no detail, log gets everything
        '           - Dependant Object Errors, codes that return from objects:
        '               - If UserError, translate ErrSource for raise, but log all original info
        '               - If InternalError, log info and raise InternalError
        '               - If you can not tell, call it InternalError
        '
        '       UnattendedMode
        '           The same, except each routine decides when
        '
        '       When an error happens in-line (bad condition without a raise)
        '           Log the error
        '           Raise the appropriate Code/Description in the current Source
        '
        '       When an ErrorTrap occurs
        '           If ErrSource is not AppTitle, it is a dependantObjectError, log and translate code
        '           If ErrNumber is not an ObjectError, call it internal error, log and translate code
        '           Error must be either "InternalError" or "UserError", just raise it again
        '
        ' old - If an error is raised that is not a KmaCode, it is logged and translated
        ' old - If an error is raised and the soure is not he current "dll", it is logged and translated
        '
        'Public Const ignoreInteger = vbObjectError                 ' Base on which Internal errors should start
        ''
        ''Public Const KmaError_UnderlyingObject = vbObjectError + 1     ' An error occurec in an underlying object
        ''Public Const KmaccErrorServiceStopped = vbObjectError + 2       ' The service is not running
        ''Public Const KmaError_BadObject = vbObjectError + 3            ' The Server Pointer is not valid
        ''Public Const KmaError_UpgradeInProgress = vbObjectError + 4    ' page is blocked because an upgrade is in progress
        ''Public Const KmaError_InvalidArgument = vbObjectError + 5      ' and input argument is not valid. Put details at end of description
        ''
        'Public Const ignoreInteger = ignoreInteger + 16                   ' Generic Error code that passes the description back to the user
        'Public Const ignoreInteger = ignoreInteger + 17               ' Internal error which the user should not see
        'Public Const KmaErrorPage = ignoreInteger + 18                   ' Error from the page which called Contensive
        ''
        'Public Const KmaObjectError = ignoreInteger + 256                ' Internal error which the user should not see
        '
        Public Const SQLTrue As String = "1"
        Public Const SQLFalse As String = "0"
        '
        Public Const dateMinValue As Date = #12/30/1899#
        '
        '
        Public Const kmaEndTable As String = "</table >"
        Public Const kmaEndTableCell As String = "</td>"
        Public Const kmaEndTableRow As String = "</tr>"
        '
        Public Enum csv_contentTypeEnum
            contentTypeWeb = 1
            contentTypeEmail = 2
            contentTypeWebTemplate = 3
            contentTypeEmailTemplate = 4
        End Enum
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefAdminColumnType
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefAdminColumnType
        '    Public Name As String
        '    Public FieldPointer As Integer
        '    Public Width As Integer
        '    Public SortPriority As Integer
        '    Public SortDirection As Integer
        'End Structure
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefFieldType
        ''       class not structure because it has to marshall to vb6
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefFieldType
        '    Public Name As String                      ' The name of the field
        '    Public ValueVariant As Object             ' The value carried to and from the database
        '    Public Id As Integer                          ' the ID in the ccContentFields Table that this came from
        '    Public active As Boolean                   ' if the field is available in the admin area
        '    Public fieldType As Integer                   ' The type of data the field holds
        '    Public Caption As String                   ' The caption for displaying the field
        '    Public ReadOnlyField As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
        '    Public NotEditable As Boolean              ' if true, you can only edit new records
        '    Public LookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        '    Public Required As Boolean                 ' if true, this field must be entered
        '    Public DefaultValueObject As Object      ' default value on a new record
        '    Public HelpMessage As String               ' explaination of this field
        '    Public UniqueName As Boolean               '
        '    Public TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
        '    Public Password As Boolean                 ' for text boxes, sets the password attribute
        '    Public RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
        '    Public RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
        '    Public RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        '    Public IndexColumn As Integer                 ' the column desired in the admin index form
        '    Public IndexWidth As String                ' either number or percentage
        '    Public IndexSortOrder As String            ' alpha sort on index page
        '    Public IndexSortDirection As Integer          ' 1 sorts forward, -1 backward
        '    Public Changed As Boolean                  ' if true, field value needs to be saved to database
        '    Public AdminOnly As Boolean                ' This field is only available to administrators
        '    Public DeveloperOnly As Boolean            ' This field is only available to administrators
        '    Public BlockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
        '    '   false - custom field, is not altered during upgrade, Help message comes from the local record
        '    '   true - upgrade modifies the field definition, help message comes from support.contensive.com
        '    Public htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
        '    Public Authorable As Boolean               ' true if it can be seen in the admin form
        '    Public Inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
        '    Public ContentID As Integer                   ' This is the ID of the Content Def that defines these properties
        '    Public EditSortPriority As Integer            ' The Admin Edit Sort Order
        '    Public ManyToManyContentID As Integer         ' Content containing Secondary Records
        '    Public ManyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
        '    Public ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
        '    Public ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
        '    '
        '    ' - new
        '    '
        '    Public RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
        '    Public RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
        '    Public EditTab As String                   ' Editing group - used for the tabs
        '    Public Scramble As Boolean                 ' save the field scrambled in the Db
        '    Public MemberSelectGroupID As Integer         ' If the Type is TypeMemberSelect, this is the group that the member will be selected from
        '    Public LookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
        'End Structure
        ''
        '' ---------------------------------------------------------------------------------------------------
        '' ----- CDefType
        ''       class not structure because it has to marshall to vb6
        '' ---------------------------------------------------------------------------------------------------
        ''
        'Public Structure cdefServices.CDefType
        '    Public Name As String                       ' Name of Content
        '    Public Id As Integer                           ' index in content table
        '    Public ContentTableName As String           ' the name of the content table
        '    Public ContentDataSourceName As String      '
        '    Public AuthoringTableName As String         ' the name of the authoring table
        '    Public AuthoringDataSourceName As String    '
        '    Public AllowAdd As Boolean                  ' Allow adding records
        '    Public AllowDelete As Boolean               ' Allow deleting records
        '    Public WhereClause As String                ' Used to filter records in the admin area
        '    Public ParentID As Integer                     ' if not IgnoreContentControl, the ID of the parent content
        '    Public ChildIDList As String                ' Comma separated list of child content definition IDs
        '    Public ChildPointerList As String           ' Comma separated list of child content definition Pointers
        '    Public DefaultSortMethod As String          ' FieldName Direction, ....
        '    Public ActiveOnly As Boolean                ' When true
        '    Public AdminOnly As Boolean                 ' Only allow administrators to modify content
        '    Public DeveloperOnly As Boolean             ' Only allow developers to modify content
        '    Public AllowWorkflowAuthoring As Boolean    ' if true, treat this content with authoring proceses
        '    Public DropDownFieldList As String          ' String used to populate select boxes
        '    Public SelectFieldList As String            ' Field list used in OpenCSContent calls (all active field definitions)
        '    Public EditorGroupName As String            ' Group of members who administer Workflow Authoring
        '    '
        '    ' array of cdefFieldType throws a vb6 error, data or method problem
        '    ' public property does not work (msn article -- very slow because it marshals he entire array, not a pointer )
        '    ' public function works to read, but cannot write
        '    ' possible -- public fields() function for reads
        '    '   -- public setFields() function for writes
        '    '
        '    Public fields as appServices_CdefClass.CDefFieldType()
        '    Public FieldPointer As Integer                 ' Current Field for FirstField / NextField calls
        '    Public FieldCount As Integer                   ' The number of fields loaded (from ccFields, or Set calls)
        '    Public FieldSize As Integer                    ' The number of initialize Field()
        '    '
        '    ' same as fields
        '    '
        '    Public adminColumns as appServices_CdefClass.CDefAdminColumnType()
        '    'Public AdminColumnLocal As CDefAdminColumnType()  ' The Admins in this content
        '    Public AdminColumnPointer As Integer                 ' Current Admin for FirstAdminColumn / NextAdminColumn calls
        '    Public AdminColumnCount As Integer                   ' The number of AdminColumns loaded (from ccAdminColumns, or Set calls)
        '    Public AdminColumnSize As Integer                    ' The number of initialize Admin()s
        '    'AdminColumn As CDefAdminCollClass
        '    '
        '    Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
        '    '
        '    ' ----- future
        '    '
        '    Public IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
        '    Public AliasName As String                 ' Field Name of the required "name" field
        '    Public AliasID As String                   ' Field Name of the required "id" field
        '    '
        '    ' ----- removed
        '    '
        '    Public SingleRecord As Boolean             ' removeme
        '    '    Type as integer                        ' removeme
        '    Public TableName As String                 ' removeme
        '    Public DataSourceID As Integer                ' removeme
        '    '
        '    ' ----- new
        '    '
        '    Public AllowTopicRules As Boolean          ' For admin edit page
        '    Public AllowContentTracking As Boolean     ' For admin edit page
        '    Public AllowCalendarEvents As Boolean      ' For admin edit page
        '    Public AllowMetaContent As Boolean         ' For admin edit page - Adds the Meta Content Section
        '    Public TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
        'End Structure


        'Private Type AdminColumnType
        '    FieldPointer as integer
        '    FieldWidth as integer
        '    FieldSortDirection as integer
        'End Type
        '
        'Private Type AdminType
        '    ColumnCount as integer
        '    Columns() As AdminColumnType
        'End Type
        '
        '-----------------------------------------------------------------------
        '   Messages
        '-----------------------------------------------------------------------
        '
        Public Const Msg_AuthoringDeleted = "<b>Record Deleted</b><br>" & SpanClassAdminSmall _
                                                & "This record was deleted and will be removed when publishing is complete.</SPAN>"
        Public Const Msg_AuthoringInserted = "<b>Record Added</b><br>" & SpanClassAdminSmall _
                                                & "This record was added and will display when publishing is complete.</span>"
        Public Const Msg_EditLock = "<b>Edit Locked</b><br>" & SpanClassAdminSmall _
                                                & "This record is currently being edited by <EDITNAME>.<br>" _
                                                & "This lock will be released when the user releases the record, or at <EDITEXPIRES> (about <EDITEXPIRESMINUTES> minutes).</span>"
        Public Const Msg_WorkflowDisabled = "<b>Immediate Authoring</b><br>" & SpanClassAdminSmall _
                                                & "Changes made will be reflected on the web site immediately.</span>"
        Public Const Msg_ContentWorkflowDisabled = "<b>Immediate Authoring Content Definition</b><br>" & SpanClassAdminSmall _
                                                & "Changes made will be reflected on the web site immediately.</span>"
        Public Const Msg_AuthoringRecordNotModifed = "" & SpanClassAdminSmall _
                                                & "No changes have been saved to this record.</span>"
        Public Const Msg_AuthoringRecordModifed = "<b>Edits Pending</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited by <EDITNAME>.<br>" _
                                                & "To publish these edits, submit for publishing, or have an administrator 'Publish Changes'.</span>"
        Public Const Msg_AuthoringRecordModifedAdmin = "<b>Edits Pending</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited by <EDITNAME>.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To submit these changes for workflow publishing, hit 'Submit for Publishing'.</span>"
        Public Const Msg_AuthoringSubmitted = "<b>Edits Submitted for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and was submitted for publishing by <EDITNAME>.</span>"
        Public Const Msg_AuthoringSubmittedAdmin = "<b>Edits Submitted for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and was submitted for publishing by <EDITNAME>.<br>" _
                                                & "As an administrator, you can make changes to this submitted record.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To deny these edits, hit 'Abort Edits'.<br>" _
                                                & "To approve these edits for workflow publishing, hit 'Approve for Publishing'." _
                                                & "</span>"
        Public Const Msg_AuthoringApproved = "<b>Edits Approved for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and approved for publishing.<br>" _
                                                & "No further changes can be made to this record until an administrator publishs, or aborts publishing." _
                                                & "</span>"
        Public Const Msg_AuthoringApprovedAdmin = "<b>Edits Approved for Publishing</b><br>" & SpanClassAdminSmall _
                                                & "This record has been edited and approved for publishing.<br>" _
                                                & "No further changes can be made to this record until an administrator publishs, or aborts publishing.<br>" _
                                                & "To publish these edits immediately, hit 'Publish Changes'.<br>" _
                                                & "To deny these edits, hit 'Abort Edits'." _
                                                & "</span>"
        Public Const Msg_AuthoringSubmittedNotification = "The following Content has been submitted for publication. Instructions on how to publish this content to web site are at the bottom of this message.<br>" _
                                                & "<br>" _
                                                & "website: <DOMAINNAME><br>" _
                                                & "Name: <RECORDNAME><br>" _
                                                & "<br>" _
                                                & "Content: <CONTENTNAME><br>" _
                                                & "Record Number: <RECORDID><br>" _
                                                & "<br>" _
                                                & "Submitted: <SUBMITTEDDATE><br>" _
                                                & "Submitted By: <SUBMITTEDNAME><br>" _
                                                & "<br>" _
                                                & "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because you are a member of the Editors Group for the content that has been changed.</p>" _
                                                & "<p>To publish this content immediately, click on the website link above, check off this record in the box to the far left and click the ""Publish Selected Records"" button.</p>" _
                                                & "<p>To edit the record, click the ""edit"" link for this record, make the desired changes and click the ""Publish"" button.</p>"
        '& "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because your account on this system is a member of the Editors Group for the content that has been changed.</p>" _
        '& "<p>To publish this content immediately, click on the website link above and locate this record in the list of modified records presented. his content has been modified and was submitted for publication by the individual shown above. Click the 'Admin Site' link to edit the record, and hit the publish button.</p>" _
        '& "<p>To approved this record for workflow publishing, locate the record as described above, and hit the 'Approve for Publishing' button.</p>" _
        '& "<p>To publish all content records approved, go to the Workflow Publishing Screen (on the Administration Menu or the Administration Site) and hit the 'Publish Approved Records' button.</p>"
        Public Const PageNotAvailable_Msg = "This page is not currently available. <br>" _
                            & "Please use your back button to return to the previous page. <br>"
        Public Const NewPage_Msg = ""
        '    '
        '    '==========================================================================
        '    '   Convert a variant to an long (long)
        '    '   returns 0 if the input is not an integer
        '    '   if float, rounds to integer
        '    '==========================================================================
        '    '
        '    Public Function encodeInteger(ExpressionVariant As Object) As Integer
        '        ' 7/14/2009 - cover the overflow case, return 0
        '        On Error Resume Next
        '        '
        '        If Not IsArray(ExpressionVariant) Then
        '            If Not IsMissing(ExpressionVariant) Then
        '                If Not IsNull(ExpressionVariant) Then
        '                    If ExpressionVariant <> "" Then
        '                        If vbIsNumeric(ExpressionVariant) Then
        '                            encodeInteger = CLng(ExpressionVariant)
        '                        End If
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeInteger = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a number (double)
        '    '   returns 0 if the input is not a number
        '    '==========================================================================
        '    '
        '    Public Function encodeNumber(ExpressionVariant As Object) As Double
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeNumber = 0
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If vbIsNumeric(ExpressionVariant) Then
        '                        encodeNumber = ExpressionVariant
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeNumber = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a date
        '    '   returns 0 if the input is not a number
        '    '==========================================================================
        '    '
        '    Public Function encodeDate(ExpressionVariant As Object) As Date
        '        On Error GoTo ErrorTrap
        '        '
        '        '    encodeDate = CDate(ExpressionVariant)
        '        '    encodeDate = CDate("1/1/1980")
        '        'encodeDate = Date.MinValue
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If IsDate(ExpressionVariant) Then
        '                        encodeDate = ExpressionVariant
        '                    End If
        '                End If
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeDate = Date.MinValue
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a boolean
        '    '   Returns true if input is not false, else false
        '    '==========================================================================
        '    '
        '    Public Function encodeBoolean(ExpressionVariant As Object) As Boolean
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeBoolean = False
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                If ExpressionVariant <> "" Then
        '                    If vbIsNumeric(ExpressionVariant) Then
        '                        If ExpressionVariant <> "0" Then
        '                            If ExpressionVariant <> 0 Then
        '                                encodeBoolean = True
        '                            End If
        '                        End If
        '                    ElseIf vbUCase(ExpressionVariant) = "ON" Then
        '                        encodeBoolean = True
        '                    ElseIf vbUCase(ExpressionVariant) = "YES" Then
        '                        encodeBoolean = True
        '                    ElseIf vbUCase(ExpressionVariant) = "TRUE" Then
        '                        encodeBoolean = True
        '                    Else
        '                        encodeBoolean = False
        '                    End If
        '                End If
        '            End If
        '        End If
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeBoolean = False
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant into 0 or 1
        '    '   Returns 1 if input is not false, else 0
        '    '==========================================================================
        '    '
        '    Public Function encodeBit(ExpressionVariant As Object) As Integer
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeBit = 0
        '        If encodeBoolean(ExpressionVariant) Then
        '            encodeBit = 1
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeBit = 0
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Convert a variant to a string
        '    '   returns emptystring if the input is not a string
        '    '==========================================================================
        '    '
        '    Public Function encodeText(ExpressionVariant As Object) As String
        '        On Error GoTo ErrorTrap
        '        '
        '        'encodeText = ""
        '        If Not IsMissing(ExpressionVariant) Then
        '            If Not IsNull(ExpressionVariant) Then
        '                encodeText = CStr(ExpressionVariant)
        '            End If
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '        encodeText = ""
        '    End Function
        '    '
        '    '==========================================================================
        '    '   Converts a possibly missing value to variant
        '    '==========================================================================
        '    '
        '    Public Function encodeMissingText(ExpressionVariant As Object, DefaultVariant As Object) As Object
        '        'On Error GoTo ErrorTrap
        '        '
        '        If IsMissing(ExpressionVariant) Then
        '            encodeMissing = DefaultVariant
        '        Else
        '            encodeMissing = ExpressionVariant
        '        End If
        '        '
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Function
        '
        '
        '
        Public Function encodeEmptyText(ByVal sourceText As String, ByVal DefaultText As String) As String
            Dim returnText As String = sourceText
            If (returnText = "") Then
                returnText = DefaultText
            End If
            Return returnText
        End Function
        '
        '
        '
        Public Function encodeEmptyInteger(ByVal sourceText As String, ByVal DefaultInteger As Integer) As Integer
            Return EncodeInteger(encodeEmptyText(sourceText, DefaultInteger.ToString))
        End Function
        '
        '
        '
        Public Function encodeEmptyDate(ByVal sourceText As String, ByVal DefaultDate As Date) As Date
            Return EncodeDate(encodeEmptyText(sourceText, DefaultDate.ToString))
        End Function
        '
        '
        '
        Public Function encodeEmptyNumber(ByVal sourceText As String, ByVal DefaultNumber As Double) As Double
            Return EncodeNumber(encodeEmptyText(sourceText, DefaultNumber.ToString))
        End Function
        '
        '
        '
        Public Function encodeEmptyBoolean(ByVal sourceText As String, ByVal DefaultState As Boolean) As Boolean
            Return EncodeBoolean(encodeEmptyText(sourceText, DefaultState.ToString))
        End Function
        '    '
        '    '================================================================================================================
        '    '   Separate a URL into its host, path, page parts
        '    '================================================================================================================
        '    '
        '    Public Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        '        'On Error GoTo ErrorTrap
        '        '
        '        '   Divide the URL into URLHost, URLPath, and URLPage
        '        '
        '        Dim WorkingURL As String
        '        Dim Position As Integer
        '        '
        '        ' Get Protocol (before the first :)
        '        '
        '        WorkingURL = SourceURL
        '        Position = vbInstr(1, WorkingURL, ":")
        '        'Position = vbInstr(1, WorkingURL, "://")
        '        If Position <> 0 Then
        '            Protocol = Mid(WorkingURL, 1, Position + 2)
        '            WorkingURL = Mid(WorkingURL, Position + 3)
        '        End If
        '        '
        '        ' compatibility fix
        '        '
        '        If vbInstr(1, WorkingURL, "//") = 1 Then
        '            If Protocol = "" Then
        '                Protocol = "http:"
        '            End If
        '            Protocol = Protocol & "//"
        '            WorkingURL = Mid(WorkingURL, 3)
        '        End If
        '        '
        '        ' Get QueryString
        '        '
        '        Position = vbInstr(1, WorkingURL, "?")
        '        If Position > 0 Then
        '            QueryString = Mid(WorkingURL, Position)
        '            WorkingURL = Mid(WorkingURL, 1, Position - 1)
        '        End If
        '        '
        '        ' separate host from pathpage
        '        '
        '        'iURLHost = WorkingURL
        '        Position = vbInstr(WorkingURL, "/")
        '        If (Position = 0) And (Protocol = "") Then
        '            '
        '            ' Page without path or host
        '            '
        '            Page = WorkingURL
        '            Path = ""
        '            Host = ""
        '        ElseIf (Position = 0) Then
        '            '
        '            ' host, without path or page
        '            '
        '            Page = ""
        '            Path = "/"
        '            Host = WorkingURL
        '        Else
        '            '
        '            ' host with a path (at least)
        '            '
        '            Path = Mid(WorkingURL, Position)
        '            Host = Mid(WorkingURL, 1, Position - 1)
        '            '
        '            ' separate page from path
        '            '
        '            Position = InStrRev(Path, "/")
        '            If Position = 0 Then
        '                '
        '                ' no path, just a page
        '                '
        '                Page = Path
        '                Path = "/"
        '            Else
        '                Page = Mid(Path, Position + 1)
        '                Path = Mid(Path, 1, Position)
        '            End If
        '        End If
        '        Exit Sub
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Sub
        '    '
        '    '================================================================================================================
        '    '   Separate a URL into its host, path, page parts
        '    '================================================================================================================
        '    '
        '    Public Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
        '        'On Error GoTo ErrorTrap
        '        '
        '        '   Divide the URL into URLHost, URLPath, and URLPage
        '        '
        '        Dim iURLWorking As String               ' internal storage for GetURL functions
        '        Dim iURLProtocol As String
        '        Dim iURLHost As String
        '        Dim iURLPort As String
        '        Dim iURLPath As String
        '        Dim iURLPage As String
        '        Dim iURLQueryString As String
        '        Dim Position As Integer
        '        '
        '        iURLWorking = SourceURL
        '        Position = vbInstr(1, iURLWorking, "://")
        '        If Position <> 0 Then
        '            iURLProtocol = Mid(iURLWorking, 1, Position + 2)
        '            iURLWorking = Mid(iURLWorking, Position + 3)
        '        End If
        '        '
        '        ' separate Host:Port from pathpage
        '        '
        '        iURLHost = iURLWorking
        '        Position = vbInstr(iURLHost, "/")
        '        If Position = 0 Then
        '            '
        '            ' just host, no path or page
        '            '
        '            iURLPath = "/"
        '            iURLPage = ""
        '        Else
        '            iURLPath = Mid(iURLHost, Position)
        '            iURLHost = Mid(iURLHost, 1, Position - 1)
        '            '
        '            ' separate page from path
        '            '
        '            Position = InStrRev(iURLPath, "/")
        '            If Position = 0 Then
        '                '
        '                ' no path, just a page
        '                '
        '                iURLPage = iURLPath
        '                iURLPath = "/"
        '            Else
        '                iURLPage = Mid(iURLPath, Position + 1)
        '                iURLPath = Mid(iURLPath, 1, Position)
        '            End If
        '        End If
        '        '
        '        ' Divide Host from Port
        '        '
        '        Position = vbInstr(iURLHost, ":")
        '        If Position = 0 Then
        '            '
        '            ' host not given, take a guess
        '            '
        '            Select Case vbUCase(iURLProtocol)
        '                Case "FTP://"
        '                    iURLPort = "21"
        '                Case "HTTP://", "HTTPS://"
        '                    iURLPort = "80"
        '                Case Else
        '                    iURLPort = "80"
        '            End Select
        '        Else
        '            iURLPort = Mid(iURLHost, Position + 1)
        '            iURLHost = Mid(iURLHost, 1, Position - 1)
        '        End If
        '        Position = vbInstr(1, iURLPage, "?")
        '        If Position > 0 Then
        '            iURLQueryString = Mid(iURLPage, Position)
        '            iURLPage = Mid(iURLPage, 1, Position - 1)
        '        End If
        '        Protocol = iURLProtocol
        '        Host = iURLHost
        '        Port = iURLPort
        '        Path = iURLPath
        '        Page = iURLPage
        '        QueryString = iURLQueryString
        '        Exit Sub
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '        Err.Clear()
        '    End Sub
        '    '
        '    '
        '    '
        '    Function DecodeGMTDate(GMTDate As String) As Date
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim WorkString As String
        '        DecodeGMTDate = 0
        '        If GMTDate <> "" Then
        '            WorkString = Mid(GMTDate, 6, 11)
        '            If IsDate(WorkString) Then
        '                DecodeGMTDate = CDate(WorkString)
        '                WorkString = Mid(GMTDate, 18, 8)
        '                If IsDate(WorkString) Then
        '                    DecodeGMTDate = DecodeGMTDate + CDate(WorkString) + 4 / 24
        '                End If
        '            End If
        '        End If
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '
        '    '
        '    Function EncodeGMTDate(MSDate As Date) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim WorkString As String
        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Renamed to catch all the cases that used it in addons
        '    '
        '    '   Do not use this routine in Addons to get the addon option string value
        '    '   to get the value in an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '    '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '    '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '    '   There can be no extra spaces between the delimiter, the name and the "="
        '    '=================================================================================
        '    '
        '    Function getSimpleNameValue(Name As String, ArgumentString As String, DefaultValue As String, Delimiter As String) As String
        '        'Function getArgument(Name As String, ArgumentString As String, Optional DefaultValue as object, Optional Delimiter As String) As String
        '        '
        '        Dim WorkingString As String
        '        Dim iDefaultValue As String
        '        Dim NameLength As Integer
        '        Dim ValueStart As Integer
        '        Dim ValueEnd As Integer
        '        Dim IsQuoted As Boolean
        '        '
        '        ' determine delimiter
        '        '
        '        If Delimiter = "" Then
        '            '
        '            ' If not explicit
        '            '
        '            If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
        '                '
        '                ' crlf can only be here if it is the delimiter
        '                '
        '                Delimiter = vbCrLf
        '            Else
        '                '
        '                ' either only one option, or it is the legacy '&' delimit
        '                '
        '                Delimiter = "&"
        '            End If
        '        End If
        '        iDefaultValue = encodeMissingText(DefaultValue, "")
        '        WorkingString = ArgumentString
        '        getSimpleNameValue = iDefaultValue
        '        If WorkingString <> "" Then
        '            WorkingString = Delimiter & WorkingString & Delimiter
        '            ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
        '            If ValueStart <> 0 Then
        '                NameLength = Len(Name)
        '                ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
        '                If Mid(WorkingString, ValueStart, 1) = """" Then
        '                    IsQuoted = True
        '                    ValueStart = ValueStart + 1
        '                End If
        '                If IsQuoted Then
        '                    ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
        '                Else
        '                    ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
        '                End If
        '                If ValueEnd = 0 Then
        '                    getSimpleNameValue = Mid(WorkingString, ValueStart)
        '                Else
        '                    getSimpleNameValue = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
        '                End If
        '            End If
        '        End If
        '        '

        '        Exit Function
        '        '
        '        ' ----- ErrorTrap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Do not use this code
        '    '
        '    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    '   This was left here to work through any code issues that might arrise during
        '    '   the conversion.
        '    '
        '    '   Return the value from a name value pair, parsed with =,&[|].
        '    '   For example:
        '    '       name=Jay[Jay|Josh|Dwayne]
        '    '       the answer is Jay. If a select box is displayed, it is a dropdown of all three
        '    '=================================================================================
        '    '
        '    Public Function GetAggrOption_old(Name As String, SegmentCMDArgs As String) As String
        '        '
        '        Dim Pos As Integer
        '        '
        '        GetAggrOption_old = getSimpleNameValue(Name, SegmentCMDArgs, "", vbCrLf)
        '        '
        '        ' remove the manual select list syntax "answer[choice1|choice2]"
        '        '
        '        Pos = vbInstr(1, GetAggrOption_old, "[")
        '        If Pos <> 0 Then
        '            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
        '        End If
        '        '
        '        ' remove any function syntax "answer{selectcontentname RSS Feeds}"
        '        '
        '        Pos = vbInstr(1, GetAggrOption_old, "{")
        '        If Pos <> 0 Then
        '            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
        '        End If
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Do not use this code
        '    '
        '    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
        '    '
        '    '   This was left here to work through any code issues that might arrise during
        '    '   Compatibility for GetArgument
        '    '=================================================================================
        '    '
        'Function getNameValue_old(Name As String, ArgumentString As String, Optional DefaultValue as string = "") As String
        '        getNameValue_old = getSimpleNameValue(Name, ArgumentString, DefaultValue, vbCrLf)
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLText
        '    '========================================================================
        '    '
        '    Public Function encodeSQLText(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        '
        '        'MethodName = "encodeSQLText"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLText = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLText = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLText = "null"
        '        Else
        '            encodeSQLText = CStr(ExpressionVariant)
        '            ' ??? this should not be here -- to correct a field used in a CDef, truncate in SaveCS by fieldtype
        '            'encodeSQLText = Left(ExpressionVariant, 255)
        '            'remove-can not find a case where | is not allowed to be saved.
        '            'encodeSQLText = vbReplace(encodeSQLText, "|", "_")
        '            encodeSQLText = "'" & vbReplace(encodeSQLText, "'", "''") & "'"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLLongText
        '    '========================================================================
        '    '
        '    Public Function encodeSQLLongText(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        '
        '        'MethodName = "encodeSQLLongText"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLLongText = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLLongText = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLLongText = "null"
        '        Else
        '            encodeSQLLongText = ExpressionVariant
        '            'encodeSQLLongText = vbReplace(ExpressionVariant, "|", "_")
        '            encodeSQLLongText = "'" & vbReplace(encodeSQLLongText, "'", "''") & "'"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLDate
        '    '       encode a date variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Public Function encodeSQLDate(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        Dim TimeVar As Date
        '        Dim TimeValuething As Single
        '        Dim TimeHours As Integer
        '        Dim TimeMinutes As Integer
        '        Dim TimeSeconds As Integer
        '        'Dim MethodName As String
        '        ''
        '        'MethodName = "encodeSQLDate"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLDate = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLDate = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLDate = "null"
        '        ElseIf IsDate(ExpressionVariant) Then
        '            TimeVar = CDate(ExpressionVariant)
        '            If TimeVar = 0 Then
        '                encodeSQLDate = "null"
        '            Else
        '                TimeValuething = 86400.0! * (TimeVar - Int(TimeVar + 0.000011!))
        '                TimeHours = Int(TimeValuething / 3600.0!)
        '                If TimeHours >= 24 Then
        '                    TimeHours = 23
        '                End If
        '                TimeMinutes = Int(TimeValuething / 60.0!) - (TimeHours * 60)
        '                If TimeMinutes >= 60 Then
        '                    TimeMinutes = 59
        '                End If
        '                TimeSeconds = TimeValuething - (TimeHours * 3600.0!) - (TimeMinutes * 60.0!)
        '                If TimeSeconds >= 60 Then
        '                    TimeSeconds = 59
        '                End If
        '                encodeSQLDate = "{ts '" & Year(ExpressionVariant) & "-" & Right("0" & Month(ExpressionVariant), 2) & "-" & Right("0" & Day(ExpressionVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2) & "'}"
        '            End If
        '        Else
        '            encodeSQLDate = "null"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLNumber
        '    '       encode a number variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Function encodeSQLNumber(ExpressionVariant As Object) As String
        '        'On Error GoTo ErrorTrap
        '        '
        '        'Dim MethodName As String
        '        ''
        '        'MethodName = "encodeSQLNumber"
        '        '
        '        If IsNull(ExpressionVariant) Then
        '            encodeSQLNumber = "null"
        '        ElseIf IsMissing(ExpressionVariant) Then
        '            encodeSQLNumber = "null"
        '        ElseIf ExpressionVariant = "" Then
        '            encodeSQLNumber = "null"
        '        ElseIf vbIsNumeric(ExpressionVariant) Then
        '            Select Case VarType(ExpressionVariant)
        '                Case vbBoolean
        '                    If ExpressionVariant Then
        '                        encodeSQLNumber = SQLTrue
        '                    Else
        '                        encodeSQLNumber = SQLFalse
        '                    End If
        '                Case Else
        '                    encodeSQLNumber = ExpressionVariant
        '            End Select
        '        Else
        '            encodeSQLNumber = "null"
        '        End If
        '        Exit Function
        '        '
        '        ' ----- Error Trap
        '        '
        'ErrorTrap:
        '    End Function
        '    '
        '    '========================================================================
        '    '   encodeSQLBoolean
        '    '       encode a boolean variable to go in an sql expression
        '    '========================================================================
        '    '
        '    Public Function encodeSQLBoolean(ExpressionVariant As Object) As String
        '        '
        '        Dim src As String
        '        '
        '        encodeSQLBoolean = SQLFalse
        '        If encodeBoolean(ExpressionVariant) Then
        '            encodeSQLBoolean = SQLTrue
        '        End If
        '        '    If Not IsNull(ExpressionVariant) Then
        '        '        If Not IsMissing(ExpressionVariant) Then
        '        '            If ExpressionVariant <> False Then
        '        '                    encodeSQLBoolean = SQLTrue
        '        '                End If
        '        '            End If
        '        '        End If
        '        '    End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    '   Gets the next line from a string, and removes the line
        '    '========================================================================
        '    '
        '    Public Function getLine(Body As String) As String
        '        Dim EOL As String
        '        Dim NextCR As Integer
        '        Dim NextLF As Integer
        '        Dim BOL As Integer
        '        '
        '        NextCR = vbInstr(1, Body, vbCr)
        '        NextLF = vbInstr(1, Body, vbLf)

        '        If NextCR <> 0 Or NextLF <> 0 Then
        '            If NextCR <> 0 Then
        '                If NextLF <> 0 Then
        '                    If NextCR < NextLF Then
        '                        EOL = NextCR - 1
        '                        If NextLF = NextCR + 1 Then
        '                            BOL = NextLF + 1
        '                        Else
        '                            BOL = NextCR + 1
        '                        End If

        '                    Else
        '                        EOL = NextLF - 1
        '                        BOL = NextLF + 1
        '                    End If
        '                Else
        '                    EOL = NextCR - 1
        '                    BOL = NextCR + 1
        '                End If
        '            Else
        '                EOL = NextLF - 1
        '                BOL = NextLF + 1
        '            End If
        '            getLine = Mid(Body, 1, EOL)
        '            Body = Mid(Body, BOL)
        '        Else
        '            getLine = Body
        '            Body = ""
        '        End If

        '        'EOL = vbInstr(1, Body, vbCrLf)

        '        'If EOL <> 0 Then
        '        '    getLine = Mid(Body, 1, EOL - 1)
        '        '    Body = Mid(Body, EOL + 2)
        '        '    End If
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '   Get a Random Long Value
        '    '=================================================================================
        '    '
        '    Public Function GetRandomInteger() As Integer
        '        '
        '        Dim RandomBase As Integer
        '        Dim RandomLimit As Integer
        '        '
        '        RandomBase =Threading.Thread.CurrentThread.ManagedThreadId
        '        RandomBase = RandomBase And ((2 ^ 30) - 1)
        '        RandomLimit = (2 ^ 31) - RandomBase - 1
        '        Randomize()
        '        GetRandomInteger = RandomBase + (Rnd * RandomLimit)
        '        '
        '    End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public Function isDataTableOk(RS As Object) As Boolean
        '        isDataTableOk = False
        '        If (isDataTableOk(rs)) Then
        '            If true Then
        '                If Not rs.rows.count=0 Then
        '                    isDataTableOk = True
        '                End If
        '            End If
        '        End If
        '    End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public Sub closeDataTable(RS As Object)
        '        If (isDataTableOk(rs)) Then
        '            If true Then
        '                Call 'RS.Close()
        '            End If
        '        End If
        '    End Sub
        '
        '=============================================================================
        ' Create the part of the sql where clause that is modified by the user
        '   WorkingQuery is the original querystring to change
        '   QueryName is the name part of the name pair to change
        '   If the QueryName is not found in the string, ignore call
        '=============================================================================
        '
        Public Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            '
            If WorkingQuery.IndexOf("?") > 0 Then
                ModifyQueryString = modifyLinkQuery(WorkingQuery, QueryName, QueryValue, AddIfMissing)
            Else
                ModifyQueryString = Mid(modifyLinkQuery("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
            End If
        End Function
        '
        Public Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As Integer, Optional ByVal AddIfMissing As Boolean = True) As String
            Return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing)
        End Function
        '
        Public Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As Boolean, Optional ByVal AddIfMissing As Boolean = True) As String
            Return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing)
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' Modify the querystring at the end of a link. If there is no, question mark, the link argument is assumed to be a link, not the querysting
        ''' </summary>
        ''' <param name="Link"></param>
        ''' <param name="QueryName"></param>
        ''' <param name="QueryValue"></param>
        ''' <param name="AddIfMissing"></param>
        ''' <returns></returns>
        Public Function modifyLinkQuery(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            Try
                Dim Element() As String = {}
                Dim ElementCount As Integer
                Dim ElementPointer As Integer
                Dim NameValue() As String
                Dim UcaseQueryName As String
                Dim ElementFound As Boolean
                Dim iAddIfMissing As Boolean
                Dim QueryString As String
                '
                iAddIfMissing = AddIfMissing
                If vbInstr(1, Link, "?") <> 0 Then
                    modifyLinkQuery = Mid(Link, 1, vbInstr(1, Link, "?") - 1)
                    QueryString = Mid(Link, Len(modifyLinkQuery) + 2)
                Else
                    modifyLinkQuery = Link
                    QueryString = ""
                End If
                UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName))
                If QueryString <> "" Then
                    Element = Split(QueryString, "&")
                    ElementCount = UBound(Element) + 1
                    For ElementPointer = 0 To ElementCount - 1
                        NameValue = Split(Element(ElementPointer), "=")
                        If UBound(NameValue) = 1 Then
                            If vbUCase(NameValue(0)) = UcaseQueryName Then
                                If QueryValue = "" Then
                                    Element(ElementPointer) = ""
                                Else
                                    Element(ElementPointer) = QueryName & "=" & QueryValue
                                End If
                                ElementFound = True
                                Exit For
                            End If
                        End If
                    Next
                End If
                If Not ElementFound And (QueryValue <> "") Then
                    '
                    ' element not found, it needs to be added
                    '
                    If iAddIfMissing Then
                        If QueryString = "" Then
                            QueryString = EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                        Else
                            QueryString = QueryString & "&" & EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                        End If
                    End If
                Else
                    '
                    ' element found
                    '
                    QueryString = Join(Element, "&")
                    If (QueryString <> "") And (QueryValue = "") Then
                        '
                        ' element found and needs to be removed
                        '
                        QueryString = vbReplace(QueryString, "&&", "&")
                        If Left(QueryString, 1) = "&" Then
                            QueryString = Mid(QueryString, 2)
                        End If
                        If Right(QueryString, 1) = "&" Then
                            QueryString = Mid(QueryString, 1, Len(QueryString) - 1)
                        End If
                    End If
                End If
                If (QueryString <> "") Then
                    modifyLinkQuery = modifyLinkQuery & "?" & QueryString
                End If
            Catch ex As Exception
                Throw New ApplicationException("Exception in modifyLinkQuery", ex)
            End Try
            '
        End Function
        '    '
        '    '=================================================================================
        '    '
        '    '=================================================================================
        '    '
        '    Public Function GetIntegerString(Value As Integer, DigitCount As Integer) As String
        '        If Len(Value) <= DigitCount Then
        '        GetIntegerString = String(DigitCount - Len(CStr(Value)), "0") & CStr(Value)
        '        Else
        '            GetIntegerString = CStr(Value)
        '        End If
        '    End Function
        '    '
        '    '==========================================================================================
        '    '   the current process to a high priority
        '    '       Should be called once from the objects parent when it is first created.
        '    '
        '    '   taken from an example labeled
        '    '       KPD-Team 2000
        '    '       URL: http://www.allapi.net/
        '    '       Email: KPDTeam@Allapi.net
        '    '==========================================================================================
        '    '
        '    Public Sub SetProcessHighPriority()
        '        Dim hProcess As Integer
        '        '
        '        'set the new priority class
        '        '
        '        hProcess = GetCurrentProcess
        '        Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
        '        '
        '    End Sub
        ''
        ''==========================================================================================
        ''   Format the current error object into a standard string
        ''==========================================================================================
        ''
        'Public Function GetErrString(Optional ErrorObject As Object) As String
        '    Dim Copy As String
        '    If ErrorObject Is Nothing Then
        '        If Err.Number = 0 Then
        '            GetErrString = "[no error]"
        '        Else
        '            Copy = Err.Description
        '            Copy = vbReplace(Copy, vbCrLf, "-")
        '            Copy = vbReplace(Copy, vbLf, "-")
        '            Copy = vbReplace(Copy, vbCrLf, "")
        '            GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
        '        End If
        '    Else
        '        If ErrorObject.Number = 0 Then
        '            GetErrString = "[no error]"
        '        Else
        '            Copy = ErrorObject.Description
        '            Copy = vbReplace(Copy, vbCrLf, "-")
        '            Copy = vbReplace(Copy, vbLf, "-")
        '            Copy = vbReplace(Copy, vbCrLf, "")
        '            GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
        '        End If
        '    End If
        '    '
        'End Function
        '    '
        '    '==========================================================================================
        '    '   Format the current error object into a standard string
        '    '==========================================================================================
        '    '
        '    Public Function GetProcessID() As Integer
        '        GetProcessID = GetCurrentProcessId
        '    End Function
        '    '
        '    '==========================================================================================
        '    '   Test if a test string is in a delimited string
        '    '==========================================================================================
        '    '
        '    Public Function IsInDelimitedString(DelimitedString As String, TestString As String, Delimiter As String) As Boolean
        '        IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeURL
        '    '
        '    '   Encodes only what is to the left of the first ?
        '    '   All URL path characters are assumed to be correct (/:#)
        '    '========================================================================
        '    '
        '    Function encodeURL(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim URLSplit() As String
        '        Dim LeftSide As String
        '        Dim RightSide As String
        '        '
        '        encodeURL = Source
        '        If Source <> "" Then
        '            URLSplit = Split(Source, "?")
        '            encodeURL = URLSplit(0)
        '            encodeURL = vbReplace(encodeURL, "%", "%25")
        '            '
        '            encodeURL = vbReplace(encodeURL, """", "%22")
        '            encodeURL = vbReplace(encodeURL, " ", "%20")
        '            encodeURL = vbReplace(encodeURL, "$", "%24")
        '            encodeURL = vbReplace(encodeURL, "+", "%2B")
        '            encodeURL = vbReplace(encodeURL, ",", "%2C")
        '            encodeURL = vbReplace(encodeURL, ";", "%3B")
        '            encodeURL = vbReplace(encodeURL, "<", "%3C")
        '            encodeURL = vbReplace(encodeURL, "=", "%3D")
        '            encodeURL = vbReplace(encodeURL, ">", "%3E")
        '            encodeURL = vbReplace(encodeURL, "@", "%40")
        '            If UBound(URLSplit) > 0 Then
        '                encodeURL = encodeURL & "?" & encodeQueryString(URLSplit(1))
        '            End If
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeQueryString
        '    '
        '    '   This routine encodes the URL QueryString to conform to rules
        '    '========================================================================
        '    '
        '    Function encodeQueryString(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim QSSplit() As String
        '        Dim QSPointer As Integer
        '        Dim NVSplit() As String
        '        Dim NV As String
        '        '
        '        encodeQueryString = ""
        '        If Source <> "" Then
        '            QSSplit = Split(Source, "&")
        '            For QSPointer = 0 To UBound(QSSplit)
        '                NV = QSSplit(QSPointer)
        '                If NV <> "" Then
        '                    NVSplit = Split(NV, "=")
        '                    If UBound(NVSplit) = 0 Then
        '                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        '                        encodeQueryString = encodeQueryString & "&" & NVSplit(0)
        '                    Else
        '                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        '                        NVSplit(1) = encodeRequestVariable(NVSplit(1))
        '                        encodeQueryString = encodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
        '                    End If
        '                End If
        '            Next
        '            If encodeQueryString <> "" Then
        '                encodeQueryString = Mid(encodeQueryString, 2)
        '            End If
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeRequestVariable
        '    '
        '    '   This routine encodes a request variable for a URL Query String
        '    '       ...can be the requestname or the requestvalue
        '    '========================================================================
        '    '
        '    Function encodeRequestVariable(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim SourcePointer As Integer
        '        Dim Character As String
        '        Dim LocalSource As String
        '        '
        '        If Source <> "" Then
        '            LocalSource = Source
        '            ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
        '            'LocalSource = vbReplace(LocalSource, " ", "+")
        '            For SourcePointer = 1 To Len(LocalSource)
        '                Character = Mid(LocalSource, SourcePointer, 1)
        '                ' "%" added so if this is called twice, it will not destroy "%20" values
        '                'If Character = " " Then
        '                '    encodeRequestVariable = encodeRequestVariable & "+"
        '                If vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
        '                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
        '                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
        '                    encodeRequestVariable = encodeRequestVariable & Character
        '                Else
        '                    encodeRequestVariable = encodeRequestVariable & "%" & Hex(Asc(Character))
        '                End If
        '            Next
        '        End If
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' encodeHTML
        '    '
        '    '   Convert all characters that are not allowed in HTML to their Text equivalent
        '    '   in preperation for use on an HTML page
        '    '========================================================================
        '    '
        '    Function encodeHTML(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        encodeHTML = Source
        '        encodeHTML = vbReplace(encodeHTML, "&", "&amp;")
        '        encodeHTML = vbReplace(encodeHTML, "<", "&lt;")
        '        encodeHTML = vbReplace(encodeHTML, ">", "&gt;")
        '        encodeHTML = vbReplace(encodeHTML, """", "&quot;")
        '        encodeHTML = vbReplace(encodeHTML, "'", "&#39;")
        '        'encodeHTML = vbReplace(encodeHTML, "'", "&apos;")
        '        '
        '    End Function
        '
        '========================================================================
        ' decodeHtml
        '
        '   Convert HTML equivalent characters to their equivalents
        '========================================================================
        '
        Function decodeHtml(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim Pos As Integer
            Dim s As String
            Dim CharCodeString As String
            Dim CharCode As Integer
            Dim posEnd As Integer
            '
            ' 11/26/2009 - basically re-wrote it, I commented the old one out below
            '
            decodeHtml = ""
            If Source <> "" Then
                s = Source
                '
                Pos = Len(s)
                Pos = InStrRev(s, "&#", Pos)
                Do While Pos <> 0
                    CharCodeString = ""
                    If Mid(s, Pos + 3, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 1)
                        posEnd = Pos + 4
                    ElseIf Mid(s, Pos + 4, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 2)
                        posEnd = Pos + 5
                    ElseIf Mid(s, Pos + 5, 1) = ";" Then
                        CharCodeString = Mid(s, Pos + 2, 3)
                        posEnd = Pos + 6
                    End If
                    If CharCodeString <> "" Then
                        If vbIsNumeric(CharCodeString) Then
                            CharCode = EncodeInteger(CharCodeString)
                            s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, posEnd)
                        End If
                    End If
                    '
                    Pos = InStrRev(s, "&#", Pos)
                Loop
                '
                ' replace out all common names (at least the most common for now)
                '
                s = vbReplace(s, "&lt;", "<")
                s = vbReplace(s, "&gt;", ">")
                s = vbReplace(s, "&quot;", """")
                s = vbReplace(s, "&apos;", "'")
                '
                ' Always replace the amp last
                '
                s = vbReplace(s, "&amp;", "&")
                '
                decodeHtml = s
            End If
            ' pre-11/26/2009
            'decodeHtml = Source
            'decodeHtml = vbReplace(decodeHtml, "&amp;", "&")
            'decodeHtml = vbReplace(decodeHtml, "&lt;", "<")
            'decodeHtml = vbReplace(decodeHtml, "&gt;", ">")
            'decodeHtml = vbReplace(decodeHtml, "&quot;", """")
            'decodeHtml = vbReplace(decodeHtml, "&nbsp;", " ")
            '
        End Function
        '    '
        '    '   Indent every line by 1 tab
        '    '
        Public Function kmaIndent(ByVal Source As String) As String
            Dim posStart As Integer
            Dim posEnd As Integer
            Dim pre As String
            Dim post As String
            Dim target As String
            '
            posStart = vbInstr(1, Source, "<![CDATA[", CompareMethod.Text)
            If posStart = 0 Then
                '
                ' no cdata
                '
                posStart = vbInstr(1, Source, "<textarea", CompareMethod.Text)
                If posStart = 0 Then
                    '
                    ' no textarea
                    '
                    kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
                Else
                    '
                    ' text area found, isolate it and indent before and after
                    '
                    posEnd = vbInstr(posStart, Source, "</textarea>", CompareMethod.Text)
                    pre = Mid(Source, 1, posStart - 1)
                    If posEnd = 0 Then
                        target = Mid(Source, posStart)
                        post = ""
                    Else
                        target = Mid(Source, posStart, posEnd - posStart + Len("</textarea>"))
                        post = Mid(Source, posEnd + Len("</textarea>"))
                    End If
                    kmaIndent = kmaIndent(pre) & target & kmaIndent(post)
                End If
            Else
                '
                ' cdata found, isolate it and indent before and after
                '
                posEnd = vbInstr(posStart, Source, "]]>", CompareMethod.Text)
                pre = Mid(Source, 1, posStart - 1)
                If posEnd = 0 Then
                    target = Mid(Source, posStart)
                    post = ""
                Else
                    target = Mid(Source, posStart, posEnd - posStart + Len("]]>"))
                    post = Mid(Source, posEnd + 3)
                End If
                kmaIndent = kmaIndent(pre) & target & kmaIndent(post)
            End If
            '    kmaIndent = Source
            '    If vbInstr(1, kmaIndent, "<textarea", vbTextCompare) = 0 Then
            '        kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
            '    End If
        End Function
        '
        '========================================================================================================
        'Place code in a form module
        'Add a Command button.
        '========================================================================================================
        '
        Public Function kmaByteArrayToString(ByVal Bytes() As Byte) As String
            Return System.Text.UTF8Encoding.ASCII.GetString(Bytes)

            'Dim iUnicode As Integer, i As Integer, j As Integer

            'On Error Resume Next
            'i = UBound(Bytes)

            'If (i < 1) Then
            '    'ANSI, just convert to unicode and return
            '    kmaByteArrayToString = StrConv(Bytes, VbStrConv.vbUnicode)
            '    Exit Function
            'End If
            'i = i + 1

            ''Examine the first two bytes
            'CopyMemory(iUnicode, Bytes(0), 2)

            'If iUnicode = Bytes(0) Then 'Unicode
            '    'Account for terminating null
            '    If (i Mod 2) Then i = i - 1
            '    'Set up a buffer to recieve the string
            '    kmaByteArrayToString = String$(i / 2, 0)
            '    'Copy to string
            '    CopyMemory ByVal StrPtr(kmaByteArrayToString), Bytes(0), i
            'Else 'ANSI
            '    kmaByteArrayToString = StrConv(Bytes, vbUnicode)
            'End If

        End Function
        '    '
        '    '========================================================================================================
        '    '
        '    '========================================================================================================
        '    '
        '    Public Function kmaStringToByteArray(strInput As String, _
        '                                    Optional bReturnAsUnicode As Boolean = True, _
        '                                    Optional bAddNullTerminator As Boolean = False) As Byte()

        '        Dim lRet As Integer
        '        Dim bytBuffer() As Byte
        '        Dim lLenB As Integer

        '        If bReturnAsUnicode Then
        '            'Number of bytes
        '            lLenB = LenB(strInput)
        '            'Resize buffer, do we want terminating null?
        '            If bAddNullTerminator Then
        '                ReDim bytBuffer(lLenB)
        '            Else
        '                ReDim bytBuffer(lLenB - 1)
        '            End If
        '            'Copy characters from string to byte array
        '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB
        '        Else
        '            'METHOD ONE
        '            '        'Get rid of embedded nulls
        '            '        strRet = StrConv(strInput, vbFromUnicode)
        '            '        lLenB = LenB(strRet)
        '            '        If bAddNullTerminator Then
        '            '            ReDim bytBuffer(lLenB)
        '            '        Else
        '            '            ReDim bytBuffer(lLenB - 1)
        '            '        End If
        '            '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB

        '            'METHOD TWO
        '            'Num of characters
        '            lLenB = Len(strInput)
        '            If bAddNullTerminator Then
        '                ReDim bytBuffer(lLenB)
        '            Else
        '                ReDim bytBuffer(lLenB - 1)
        '            End If
        '        lRet = WideCharToMultiByte(CP_ACP, 0&, ByVal StrPtr(strInput), -1, ByVal VarPtr(bytBuffer(0)), lLenB, 0&, 0&)
        '        End If

        '        kmaStringToByteArray = bytBuffer

        '    End Function
        '    '
        '    '========================================================================================================
        '    '   Sample kmaStringToByteArray
        '    '========================================================================================================
        '    '
        '    Private Sub SampleStringToByteArray()
        '        Dim bAnsi() As Byte
        '        Dim bUni() As Byte
        '        Dim str As String
        '        Dim i As Integer
        '        '
        '        str = "Convert"
        '        bAnsi = kmaStringToByteArray(str, False)
        '        bUni = kmaStringToByteArray(str)
        '        '
        '        For i = 0 To UBound(bAnsi)
        '            Debug.Print("=" & bAnsi(i))
        '        Next
        '        '
        '        Debug.Print("========")
        '        '
        '        For i = 0 To UBound(bUni)
        '            Debug.Print("=" & bUni(i))
        '        Next
        '        '
        '        Debug.Print("ANSI= " & kmaByteArrayToString(bAnsi))
        '        Debug.Print("UNICODE= " & kmaByteArrayToString(bUni))
        '        'Using StrConv to convert a Unicode character array directly
        '        'will cause the resultant string to have extra embedded nulls
        '        'reason, StrConv does not know the difference between Unicode and ANSI
        '        Debug.Print("Resull= " & StrConv(bUni, vbUnicode))
        '    End Sub
        '
        '=======================================================================
        '   sitepropertyNames
        '=======================================================================
        '
        Public Const siteproperty_serverPageDefault_name = "serverPageDefault"
        Public Const siteproperty_serverPageDefault_defaultValue = "default.aspx"
        Public Const spAllowPageWithoutSectionDisplay As String = "Allow Page Without Section Display"
        Public Const spAllowPageWithoutSectionDisplay_default As Boolean = True
        '
        '=======================================================================
        '   content replacements
        '=======================================================================
        '
        Public Const contentReplaceEscapeStart = "{%"
        Public Const contentReplaceEscapeEnd = "%}"
        '
        Public Class fieldEditorType
            Public fieldId As Integer
            Public addonid As Integer
        End Class
        '
        Public Const protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,EDITSOURCEID,EDITARCHIVE,EDITBLANK,CONTENTCONTROLID"
        'Public Const protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,EDITSOURCEID,EDITARCHIVE,EDITBLANK"
        '
        Public Const HTMLEditorDefaultCopyStartMark = "<!-- cc -->"
        Public Const HTMLEditorDefaultCopyEndMark = "<!-- /cc -->"
        Public Const HTMLEditorDefaultCopyNoCr = HTMLEditorDefaultCopyStartMark & "<p><br></p>" & HTMLEditorDefaultCopyEndMark
        Public Const HTMLEditorDefaultCopyNoCr2 = "<p><br></p>"
        '
        Public Const IconWidthHeight = " width=21 height=22 "
        'Public Const IconWidthHeight = " width=18 height=22 "
        '
        Public Const baseCollectionGuid As String = "{7C6601A7-9D52-40A3-9570-774D0D43D758}"        ' part of software dist - base cdef plus addons with classes in in core library, plus depenancy on coreCollection
        Public Const CoreCollectionGuid = "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}"                  ' contains core Contensive objects, loaded from Library
        Public Const ApplicationCollectionGuid = "{C58A76E2-248B-4DE8-BF9C-849A960F79C6}"           ' exported from application during upgrade
        '
        Public Const adminSiteAddonGuid As String = "{c2de2acf-ca39-4668-b417-aa491e7d8460}"
        Public Const adminCommonAddonGuid = "{76E7F79E-489F-4B0F-8EE5-0BAC3E4CD782}"
        Public Const DashboardAddonGuid = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}"
        Public Const PersonalizationGuid = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}"
        Public Const TextBoxGuid = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}"
        Public Const ContentBoxGuid = "{E341695F-C444-4E10-9295-9BEEC41874D8}"
        Public Const DynamicMenuGuid = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}"
        Public Const ChildListGuid = "{D291F133-AB50-4640-9A9A-18DB68FF363B}"
        Public Const DynamicFormGuid = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}"
        Public Const AddonManagerGuid = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}"
        Public Const FormWizardGuid = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}"
        Public Const ImportWizardGuid = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}"
        Public Const JQueryGuid = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}"
        Public Const JQueryUIGuid = "{840B9AEF-9470-4599-BD47-7EC0C9298614}"
        Public Const ImportProcessAddonGuid = "{5254FAC6-A7A6-4199-8599-0777CC014A13}"
        Public Const StructuredDataProcessorGuid = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}"
        Public Const jQueryFancyBoxGuid = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}"
        '
        Public Const DefaultLandingPageGuid = "{925F4A57-32F7-44D9-9027-A91EF966FB0D}"
        Public Const DefaultLandingSectionGuid = "{D882ED77-DB8F-4183-B12C-F83BD616E2E1}"
        Public Const DefaultTemplateGuid = "{47BE95E4-5D21-42CC-9193-A343241E2513}"
        Public Const DefaultDynamicMenuGuid = "{E8D575B9-54AE-4BF9-93B7-C7E7FE6F2DB3}"
        '
        Public Const fpoContentBox = "{1571E62A-972A-4BFF-A161-5F6075720791}"
        '
        Public Const sfImageExtList = "jpg,jpeg,gif,png"
        '
        Public Const PageChildListInstanceID = "{ChildPageList}"
        '
        Public Const cr = vbCrLf & vbTab
        Public Const cr2 = cr & vbTab
        Public Const cr3 = cr2 & vbTab
        Public Const cr4 = cr3 & vbTab
        Public Const cr5 = cr4 & vbTab
        Public Const cr6 = cr5 & vbTab
        '
        Public Const AddonOptionConstructor_BlockNoAjax = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Block = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" & vbCrLf & "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        Public Const AddonOptionConstructor_Inline = "As Ajax=[If Add-on is Ajax:0|Yes:1]" & vbCrLf & "css Container id" & vbCrLf & "css Container class"
        '
        ' Constants used as arguments to SiteBuilderClass.CreateNewSite
        '
        Public Const SiteTypeBaseAsp = 1
        Public Const sitetypebaseaspx = 2
        Public Const SiteTypeDemoAsp = 3
        Public Const SiteTypeBasePhp = 4
        '
        'Public Const AddonNewParse = True
        '
        Public Const AddonOptionConstructor_ForBlockText = "AllowGroups=[listid(groups)]checkbox"
        Public Const AddonOptionConstructor_ForBlockTextEnd = ""
        Public Const BlockTextStartMarker = "<!-- BLOCKTEXTSTART -->"
        Public Const BlockTextEndMarker = "<!-- BLOCKTEXTEND -->"
        '
        'Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
        'Private Declare Function GetExitCodeProcess Lib "kernel32" (ByVal hProcess As Integer, ByVal lpExitCode As Integer) As Integer
        'Private Declare Function timeGetTime Lib "winmm.dll" () As Integer
        'Private Declare Function OpenProcess Lib "kernel32" (ByVal dwDesiredAccess As Integer, ByVal bInheritHandle As Integer, ByVal dwProcessId As Integer) As Integer
        'Private Declare Function CloseHandle Lib "kernel32" (ByVal hObject As Integer) As Integer
        '
        Public Const InstallFolderName = "Install"
        Public Const DownloadFileRootNode = "collectiondownload"
        Public Const CollectionFileRootNode = "collection"
        Public Const CollectionFileRootNodeOld = "addoncollection"
        Public Const CollectionListRootNode = "collectionlist"
        '
        Public Const LegacyLandingPageName = "Landing Page Content"
        Public Const DefaultNewLandingPageName = "Home"
        Public Const DefaultLandingSectionName = "Home"
        '
        '
        '
        Public Const ignoreInteger As Integer = 0
        Public Const ignoreString As String = ""
        '
        ' ----- Errors Specific to the Contensive Objects
        '
        'Public Const ignoreInteger = KmaObjectError + 1
        'Public Const KmaccErrorServiceStopped = KmaObjectError + 2
        '
        Public Const UserErrorHeadline = "<p class=""ccError"">There was a problem with this page.</p>"
        ''
        '' ----- Errors connecting to server
        ''
        'Public Const ccError_InvalidAppName = 100
        'Public Const ccError_ErrorAddingApp = 101
        'Public Const ccError_ErrorDeletingApp = 102
        'Public Const ccError_InvalidFieldName = 103     ' Invalid parameter name
        'Public Const ignoreString = 104
        'Public Const ignoreString = 105
        'Public Const ccError_NotConnected = 106             ' Attempt to execute a command without a connection
        ''
        '
        '
        'Public Const ccStatusCode_Base = ignoreInteger
        'Public Const ccStatusCode_ControllerCreateFailed = ccStatusCode_Base + 1
        'Public Const ccStatusCode_ControllerInProcess = ccStatusCode_Base + 2
        'Public Const ccStatusCode_ControllerStartedWithoutService = ccStatusCode_Base + 3
        '
        ' ----- Previous errors, can be replaced
        '
        'Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg = "Server Object is not valid."
        'Public Const ignoreString = "Server is busy with internal builder."
        '
        'Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
        'Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
        'Public Const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running."
        'Public Const KmaError_BadObject_Msg = "Server Object is not valid."
        'Public Const ignoreString = "Server is busy with internal builder."
        'Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
        '
        '-----------------------------------------------------------------------
        '   GetApplicationList indexes
        '-----------------------------------------------------------------------
        '
        Public Const AppList_Name = 0
        Public Const AppList_Status = 1
        Public Const AppList_ConnectionsActive = 2
        Public Const AppList_ConnectionString = 3
        Public Const AppList_DataBuildVersion = 4
        Public Const AppList_LicenseKey = 5
        Public Const AppList_RootPath = 6
        Public Const AppList_PhysicalFilePath = 7
        Public Const AppList_DomainName = 8
        Public Const AppList_DefaultPage = 9
        Public Const AppList_AllowSiteMonitor = 10
        Public Const AppList_HitCounter = 11
        Public Const AppList_ErrorCount = 12
        Public Const AppList_DateStarted = 13
        Public Const AppList_AutoStart = 14
        Public Const AppList_Progress = 15
        Public Const AppList_PhysicalWWWPath = 16
        Public Const AppListCount = 17
        '
        '-----------------------------------------------------------------------
        '   System MemberID - when the system does an update, it uses this member
        '-----------------------------------------------------------------------
        '
        Public Const SystemMemberID = 0
        '
        '-----------------------------------------------------------------------
        ' ----- old (OptionKeys for available Options)
        '-----------------------------------------------------------------------
        '
        Public Const OptionKeyProductionLicense = 0
        Public Const OptionKeyDeveloperLicense = 1
        '
        '-----------------------------------------------------------------------
        ' ----- LicenseTypes, replaced OptionKeys
        '-----------------------------------------------------------------------
        '
        Public Const LicenseTypeInvalid = -1
        Public Const LicenseTypeProduction = 0
        Public Const LicenseTypeTrial = 1
        '
        '-----------------------------------------------------------------------
        ' ----- Active Content Definitions
        '-----------------------------------------------------------------------
        '
        Public Const ACTypeDate = "DATE"
        Public Const ACTypeVisit = "VISIT"
        Public Const ACTypeVisitor = "VISITOR"
        Public Const ACTypeMember = "MEMBER"
        Public Const ACTypeOrganization = "ORGANIZATION"
        Public Const ACTypeChildList = "CHILDLIST"
        Public Const ACTypeContact = "CONTACT"
        Public Const ACTypeFeedback = "FEEDBACK"
        Public Const ACTypeLanguage = "LANGUAGE"
        Public Const ACTypeAggregateFunction = "AGGREGATEFUNCTION"
        Public Const ACTypeAddon = "ADDON"
        Public Const ACTypeImage = "IMAGE"
        Public Const ACTypeDownload = "DOWNLOAD"
        Public Const ACTypeEnd = "END"
        Public Const ACTypeTemplateContent = "CONTENT"
        Public Const ACTypeTemplateText = "TEXT"
        Public Const ACTypeDynamicMenu = "DYNAMICMENU"
        Public Const ACTypeWatchList = "WATCHLIST"
        Public Const ACTypeRSSLink = "RSSLINK"
        Public Const ACTypePersonalization = "PERSONALIZATION"
        Public Const ACTypeDynamicForm = "DYNAMICFORM"
        '
        Public Const ACTagEnd = "<ac type=""" & ACTypeEnd & """>"
        '
        ' ----- PropertyType Definitions
        '
        Public Const PropertyTypeMember = 0
        Public Const PropertyTypeVisit = 1
        Public Const PropertyTypeVisitor = 2
        '
        '-----------------------------------------------------------------------
        ' ----- Port Assignments
        '-----------------------------------------------------------------------
        '
        Public Const WinsockPortWebOut = 4000
        Public Const WinsockPortServerFromWeb = 4001
        Public Const WinsockPortServerToClient = 4002
        '
        Public Const Port_ContentServerControlDefault = 4531
        Public Const Port_SiteMonitorDefault = 4532
        '
        Public Const RMBMethodHandShake = 1
        Public Const RMBMethodMessage = 3
        Public Const RMBMethodTestPoint = 4
        Public Const RMBMethodInit = 5
        Public Const RMBMethodClosePage = 6
        Public Const RMBMethodOpenCSContent = 7
        '
        ' ----- Position equates for the Remote Method Block
        '
        Const RMBPositionLength = 0             ' Length of the RMB
        Const RMBPositionSourceHandle = 4       ' Handle generated by the source of the command
        Const RMBPositionMethod = 8             ' Method in the method block
        Const RMBPositionArgumentCount = 12     ' The number of arguments in the Block
        Const RMBPositionFirstArgument = 16     ' The offset to the first argu
        '
        '-----------------------------------------------------------------------
        '   Remote Connections
        '   List of current remove connections for Remote Monitoring/administration
        '-----------------------------------------------------------------------
        '
        Public Class RemoteAdministratorType
            Dim RemoteIP As String
            Dim RemotePort As Integer
        End Class
        '
        ' Default username/password
        '
        Public Const DefaultServerUsername = "root"
        Public Const DefaultServerPassword = "contensive"
        '
        '-----------------------------------------------------------------------
        '   Form Contension Strategy
        '        '       elements of the form are named  "ElementName"
        '
        '       This prevents form elements from different forms from interfearing
        '       with each other, and with developer generated forms.
        '
        '       All forms requires:
        '           a FormId (text), containing the formid string
        '           a [formid]Type (text), as defined in FormTypexxx in CommonModule
        '
        '       Forms have two primary sections: GetForm and ProcessForm
        '
        '       Any form that has a GetForm method, should have the process form
        '       in the cmc.main_init, selected with this [formid]type hidden (not the
        '       GetForm method). This is so the process can alter the stream
        '       output for areas before the GetForm call.
        '
        '       System forms, like tools panel, that may appear on any page, have
        '       their process call in the cmc.main_init.
        '
        '       Popup forms, like ImageSelector have their processform call in the
        '       cmc.main_init because no .asp page exists that might contain a call
        '       the process section.
        '
        '-----------------------------------------------------------------------
        '
        Public Const FormTypeToolsPanel = "do30a8vl29"
        Public Const FormTypeActiveEditor = "l1gk70al9n"
        Public Const FormTypeImageSelector = "ila9c5s01m"
        Public Const FormTypePageAuthoring = "2s09lmpalb"
        Public Const FormTypeMyProfile = "89aLi180j5"
        Public Const FormTypeLogin = "login"
        'Public Const FormTypeLogin = "l09H58a195"
        Public Const FormTypeSendPassword = "lk0q56am09"
        Public Const FormTypeJoin = "6df38abv00"
        Public Const FormTypeHelpBubbleEditor = "9df019d77sA"
        Public Const FormTypeAddonSettingsEditor = "4ed923aFGw9d"
        Public Const FormTypeAddonStyleEditor = "ar5028jklkfd0s"
        Public Const FormTypeSiteStyleEditor = "fjkq4w8794kdvse"
        'Public Const FormTypeAggregateFunctionProperties = "9wI751270"
        '
        '-----------------------------------------------------------------------
        '   Hardcoded profile form const
        '-----------------------------------------------------------------------
        '
        Public Const rnMyProfileTopics = "profileTopics"
        '
        '-----------------------------------------------------------------------
        ' Legacy - replaced with HardCodedPages
        '   Intercept Page Strategy
        '
        '       RequestnameInterceptpage = InterceptPage number from the input stream
        '       InterceptPage = Global variant with RequestnameInterceptpage value read during early Init
        '
        '       Intercept pages are complete pages that appear instead of what
        '       the physical page calls.
        '-----------------------------------------------------------------------
        '
        'Public Const RequestNameInterceptpage = "ccIPage"
        '
        Public Const LegacyInterceptPageSNResourceLibrary = "s033l8dm15"
        Public Const LegacyInterceptPageSNSiteExplorer = "kdif3318sd"
        Public Const LegacyInterceptPageSNImageUpload = "ka983lm039"
        Public Const LegacyInterceptPageSNMyProfile = "k09ddk9105"
        Public Const LegacyInterceptPageSNLogin = "6ge42an09a"
        Public Const LegacyInterceptPageSNPrinterVersion = "l6d09a10sP"
        Public Const LegacyInterceptPageSNUploadEditor = "k0hxp2aiOZ"
        '
        '-----------------------------------------------------------------------
        ' Ajax functions intercepted during init, answered and response closed
        '   These are hard-coded internal Contensive functions
        '   These should eventually be replaced with (HardcodedAddons) remote methods
        '   They should all be prefixed "cc"
        '   They are called with cj.ajax.qs(), setting RequestNameAjaxFunction=name in the qs
        '   These name=value pairs go in the QueryString argument of the javascript cj.ajax.qs() function
        '-----------------------------------------------------------------------
        '
        'Public Const RequestNameOpenSettingPage = "settingpageid"
        Public Const RequestNameAjaxFunction = "ajaxfn"
        Public Const RequestNameAjaxFastFunction = "ajaxfastfn"
        '
        Public Const AjaxOpenAdminNav = "aps89102kd"
        Public Const AjaxOpenAdminNavGetContent = "d8475jkdmfj2"
        Public Const AjaxCloseAdminNav = "3857fdjdskf91"
        Public Const AjaxAdminNavOpenNode = "8395j2hf6jdjf"
        Public Const AjaxAdminNavOpenNodeGetContent = "eieofdwl34efvclaeoi234598"
        Public Const AjaxAdminNavCloseNode = "w325gfd73fhdf4rgcvjk2"
        '
        Public Const AjaxCloseIndexFilter = "k48smckdhorle0"
        Public Const AjaxOpenIndexFilter = "Ls8jCDt87kpU45YH"
        Public Const AjaxOpenIndexFilterGetContent = "llL98bbJQ38JC0KJm"
        Public Const AjaxStyleEditorAddStyle = "ajaxstyleeditoradd"
        Public Const AjaxPing = "ajaxalive"
        Public Const AjaxGetFormEditTabContent = "ajaxgetformedittabcontent"
        Public Const AjaxData = "data"
        Public Const AjaxGetVisitProperty = "getvisitproperty"
        Public Const AjaxSetVisitProperty = "setvisitproperty"
        Public Const AjaxGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString"
        Public Const ajaxGetFieldEditorPreferenceForm = "ajaxgetfieldeditorpreference"
        '
        '-----------------------------------------------------------------------
        '
        ' no - for now just use ajaxfn in the cj.ajax.qs call
        '   this is more work, and I do not see why it buys anything new or better
        '
        '   Hard-coded addons
        '       these are internal Contensive functions
        '       can be called with just /addonname?querystring
        '       call them with cj.ajax.addon() or cj.ajax.addonCallback()
        '       are first in the list of checks when a URL rewrite is detected in Init()
        '       should all be prefixed with 'cc'
        '-----------------------------------------------------------------------
        '
        'Public Const HardcodedAddonGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString"
        '
        '-----------------------------------------------------------------------
        '   Remote Methods
        '       ?RemoteMethodAddon=string
        '       calls an addon (if marked to run as a remote method)
        '       blocks all other Contensive output (tools panel, javascript, etc)
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameRemoteMethodAddon = "remotemethodaddon"
        '
        '-----------------------------------------------------------------------
        '   Hard Coded Pages
        '       ?Method=string
        '       Querystring based so they can be added to URLs, preserving the current page for a return
        '       replaces output stream with html output
        '-----------------------------------------------------------------------
        '
        Public Const RequestNameHardCodedPage = "method"
        '
        Public Const HardCodedPageLogin = "login"
        Public Const HardCodedPageLoginDefault = "logindefault"
        Public Const HardCodedPageMyProfile = "myprofile"
        Public Const HardCodedPagePrinterVersion = "printerversion"
        Public Const HardCodedPageResourceLibrary = "resourcelibrary"
        Public Const HardCodedPageLogoutLogin = "logoutlogin"
        Public Const HardCodedPageLogout = "logout"
        Public Const HardCodedPageSiteExplorer = "siteexplorer"
        'Public Const HardCodedPageForceMobile = "forcemobile"
        'Public Const HardCodedPageForceNonMobile = "forcenonmobile"
        Public Const HardCodedPageNewOrder = "neworderpage"
        Public Const HardCodedPageStatus = "status"
        Public Const HardCodedPageGetJSPage = "getjspage"
        Public Const HardCodedPageGetJSLogin = "getjslogin"
        Public Const HardCodedPageRedirect = "redirect"
        Public Const HardCodedPageExportAscii = "exportascii"
        Public Const HardCodedPagePayPalConfirm = "paypalconfirm"
        Public Const HardCodedPageSendPassword = "sendpassword"
        '
        '-----------------------------------------------------------------------
        '   Option values
        '       does not effect output directly
        '-----------------------------------------------------------------------
        '
        Public Const RequestNamePageOptions = "ccoptions"
        '
        Public Const PageOptionForceMobile = "forcemobile"
        Public Const PageOptionForceNonMobile = "forcenonmobile"
        Public Const PageOptionLogout = "logout"
        Public Const PageOptionPrinterVersion = "printerversion"
        '
        ' convert to options later
        '
        Public Const RequestNameDashboardReset = "ResetDashboard"
        '
        '-----------------------------------------------------------------------
        '   DataSource constants
        '-----------------------------------------------------------------------
        '
        Public Const DefaultDataSourceID = -1
        '
        '-----------------------------------------------------------------------
        ' ----- Type compatibility between databases
        '       Boolean
        '           Access      YesNo       true=1, false=0
        '           SQL Server  bit         true=1, false=0
        '           MySQL       bit         true=1, false=0
        '           Oracle      integer(1)  true=1, false=0
        '           Note: false does not equal NOT true
        '       Integer (Number)
        '           Access      Long        8 bytes, about E308
        '           SQL Server  int
        '           MySQL       integer
        '           Oracle      integer(8)
        '       Float
        '           Access      Double      8 bytes, about E308
        '           SQL Server  Float
        '           MySQL
        '           Oracle
        '       Text
        '           Access
        '           SQL Server
        '           MySQL
        '           Oracle
        '-----------------------------------------------------------------------
        '
        'Public Const SQLFalse = "0"
        'Public Const SQLTrue = "1"
        '
        '-----------------------------------------------------------------------
        ' ----- Style sheet definitions
        '-----------------------------------------------------------------------
        '
        Public Const defaultStyleFilename = "ccDefault.r5.css"
        Public Const StyleSheetStart = "<STYLE TYPE=""text/css"">"
        Public Const StyleSheetEnd = "</STYLE>"
        '
        Public Const SpanClassAdminNormal = "<span class=""ccAdminNormal"">"
        Public Const SpanClassAdminSmall = "<span class=""ccAdminSmall"">"
        '
        ' remove these from ccWebx
        '
        Public Const SpanClassNormal = "<span class=""ccNormal"">"
        Public Const SpanClassSmall = "<span class=""ccSmall"">"
        Public Const SpanClassLarge = "<span class=""ccLarge"">"
        Public Const SpanClassHeadline = "<span class=""ccHeadline"">"
        Public Const SpanClassList = "<span class=""ccList"">"
        Public Const SpanClassListCopy = "<span class=""ccListCopy"">"
        Public Const SpanClassError = "<span class=""ccError"">"
        Public Const SpanClassSeeAlso = "<span class=""ccSeeAlso"">"
        Public Const SpanClassEnd = "</span>"
        '
        '-----------------------------------------------------------------------
        ' ----- XHTML definitions
        '-----------------------------------------------------------------------
        '
        Public Const DTDTransitional = "<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">"
        '
        Public Const BR = "<br>"
        '
        '-----------------------------------------------------------------------
        ' AuthoringControl Types
        '-----------------------------------------------------------------------
        '
        Public Const AuthoringControlsEditing = 1
        Public Const AuthoringControlsSubmitted = 2
        Public Const AuthoringControlsApproved = 3
        Public Const AuthoringControlsModified = 4
        '
        '-----------------------------------------------------------------------
        ' ----- Panel and header colors
        '-----------------------------------------------------------------------
        '
        'Public Const "ccPanel" = "#E0E0E0"    ' The background color of a panel (black copy visible on it)
        'Public Const "ccPanelHilite" = "#F8F8F8"  '
        'Public Const "ccPanelShadow" = "#808080"  '
        '
        'Public Const HeaderColorBase = "#0320B0"   ' The background color of a panel header (reverse copy visible)
        'Public Const "ccPanelHeaderHilite" = "#8080FF" '
        'Public Const "ccPanelHeaderShadow" = "#000000" '
        '
        '-----------------------------------------------------------------------
        ' ----- Field type Definitions
        '       Field Types are numeric values that describe how to treat values
        '       stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
        '-----------------------------------------------------------------------
        '
        Public Const FieldTypeIdInteger = 1       ' An long number
        Public Const FieldTypeIdText = 2          ' A text field (up to 255 characters)
        Public Const FieldTypeIdLongText = 3      ' A memo field (up to 8000 characters)
        Public Const FieldTypeIdBoolean = 4       ' A yes/no field
        Public Const FieldTypeIdDate = 5          ' A date field
        Public Const FieldTypeIdFile = 6          ' A filename of a file in the files directory.
        Public Const FieldTypeIdLookup = 7        ' A lookup is a FieldTypeInteger that indexes into another table
        Public Const FieldTypeIdRedirect = 8      ' creates a link to another section
        Public Const FieldTypeIdCurrency = 9      ' A Float that prints in dollars
        Public Const FieldTypeIdFileTextPrivate = 10     ' Text saved in a file in the files area.
        Public Const FieldTypeIdFileImage = 11        ' A filename of a file in the files directory.
        Public Const FieldTypeIdFloat = 12        ' A float number
        Public Const FieldTypeIdAutoIdIncrement = 13 'long that automatically increments with the new record
        Public Const FieldTypeIdManyToMany = 14    ' no database field - sets up a relationship through a Rule table to another table
        Public Const FieldTypeIdMemberSelect = 15 ' This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
        Public Const FieldTypeIdFileCSS = 16      ' A filename of a CSS compatible file
        Public Const FieldTypeIdFileXML = 17      ' the filename of an XML compatible file
        Public Const FieldTypeIdFileJavascript = 18 ' the filename of a javascript compatible file
        Public Const FieldTypeIdLink = 19           ' Links used in href tags -- can go to pages or resources
        Public Const FieldTypeIdResourceLink = 20   ' Links used in resources, link <img or <object. Should not be pages
        Public Const FieldTypeIdHTML = 21           ' LongText field that expects HTML content
        Public Const FieldTypeIdFileHTMLPrivate = 22       ' TextFile field that expects HTML content
        Public Const FieldTypeIdMax = 22
        '
        ' ----- Field Descriptors for these type
        '       These are what are publicly displayed for each type
        '       See GetFieldTypeNameByType and vise-versa to translater
        '
        Public Const FieldTypeNameInteger = "Integer"
        Public Const FieldTypeNameText = "Text"
        Public Const FieldTypeNameLongText = "LongText"
        Public Const FieldTypeNameBoolean = "Boolean"
        Public Const FieldTypeNameDate = "Date"
        Public Const FieldTypeNameFile = "File"
        Public Const FieldTypeNameLookup = "Lookup"
        Public Const FieldTypeNameRedirect = "Redirect"
        Public Const FieldTypeNameCurrency = "Currency"
        Public Const FieldTypeNameImage = "Image"
        Public Const FieldTypeNameFloat = "Float"
        Public Const FieldTypeNameManyToMany = "ManyToMany"
        Public Const FieldTypeNameTextFile = "TextFile"
        Public Const FieldTypeNameCSSFile = "CSSFile"
        Public Const FieldTypeNameXMLFile = "XMLFile"
        Public Const FieldTypeNameJavascriptFile = "JavascriptFile"
        Public Const FieldTypeNameLink = "Link"
        Public Const FieldTypeNameResourceLink = "ResourceLink"
        Public Const FieldTypeNameMemberSelect = "MemberSelect"
        Public Const FieldTypeNameHTML = "HTML"
        Public Const FieldTypeNameHTMLFile = "HTMLFile"
        '
        Public Const FieldTypeNameLcaseInteger = "integer"
        Public Const FieldTypeNameLcaseText = "text"
        Public Const FieldTypeNameLcaseLongText = "longtext"
        Public Const FieldTypeNameLcaseBoolean = "boolean"
        Public Const FieldTypeNameLcaseDate = "date"
        Public Const FieldTypeNameLcaseFile = "file"
        Public Const FieldTypeNameLcaseLookup = "lookup"
        Public Const FieldTypeNameLcaseRedirect = "redirect"
        Public Const FieldTypeNameLcaseCurrency = "currency"
        Public Const FieldTypeNameLcaseImage = "image"
        Public Const FieldTypeNameLcaseFloat = "float"
        Public Const FieldTypeNameLcaseManyToMany = "manytomany"
        Public Const FieldTypeNameLcaseTextFile = "textfile"
        Public Const FieldTypeNameLcaseCSSFile = "cssfile"
        Public Const FieldTypeNameLcaseXMLFile = "xmlfile"
        Public Const FieldTypeNameLcaseJavascriptFile = "javascriptfile"
        Public Const FieldTypeNameLcaseLink = "link"
        Public Const FieldTypeNameLcaseResourceLink = "resourcelink"
        Public Const FieldTypeNameLcaseMemberSelect = "memberselect"
        Public Const FieldTypeNameLcaseHTML = "html"
        Public Const FieldTypeNameLcaseHTMLFile = "htmlfile"
        '
        '------------------------------------------------------------------------
        ' ----- Payment Options
        '------------------------------------------------------------------------
        '
        Public Const PayTypeCreditCardOnline = 0   ' Pay by credit card online
        Public Const PayTypeCreditCardByPhone = 1  ' Phone in a credit card
        Public Const PayTypeCreditCardByFax = 9    ' Phone in a credit card
        Public Const PayTypeCHECK = 2              ' pay by check to be mailed
        Public Const PayTypeCREDIT = 3             ' pay on account
        Public Const PayTypeNONE = 4               ' order total is $0.00. Nothing due
        Public Const PayTypeCHECKCOMPANY = 5       ' pay by company check
        Public Const PayTypeNetTerms = 6
        Public Const PayTypeCODCompanyCheck = 7
        Public Const PayTypeCODCertifiedFunds = 8
        Public Const PayTypePAYPAL = 10
        Public Const PAYDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' ----- Credit card options
        '------------------------------------------------------------------------
        '
        Public Const CCTYPEVISA = 0                ' Visa
        Public Const CCTYPEMC = 1                  ' MasterCard
        Public Const CCTYPEAMEX = 2                ' American Express
        Public Const CCTYPEDISCOVER = 3            ' Discover
        Public Const CCTYPENOVUS = 4               ' Novus Card
        Public Const CCTYPEDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' ----- Shipping Options
        '------------------------------------------------------------------------
        '
        Public Const SHIPGROUND = 0                ' ground
        Public Const SHIPOVERNIGHT = 1             ' overnight
        Public Const SHIPSTANDARD = 2              ' standard, whatever that is
        Public Const SHIPOVERSEAS = 3              ' to overseas
        Public Const SHIPCANADA = 4                ' to Canada
        Public Const SHIPDEFAULT = 0
        '
        '------------------------------------------------------------------------
        ' Debugging info
        '------------------------------------------------------------------------
        '
        Public Const TestPointTab = 2
        Public Const debug_TestPointTabChr As Char = "-"c
        Public CPTickCountBase As Double
        '
        '------------------------------------------------------------------------
        '   project width button defintions
        '------------------------------------------------------------------------
        '
        Public Const ButtonApply = "  Apply "
        Public Const ButtonLogin = "  Login  "
        Public Const ButtonLogout = "  Logout  "
        Public Const ButtonSendPassword = "  Send Password  "
        Public Const ButtonJoin = "   Join   "
        Public Const ButtonSave = "  Save  "
        Public Const ButtonOK = "     OK     "
        Public Const ButtonReset = "  Reset  "
        Public Const ButtonSaveAddNew = " Save + Add "
        'Public Const ButtonSaveAddNew = " Save > Add "
        Public Const ButtonCancel = " Cancel "
        Public Const ButtonRestartContensiveApplication = " Restart Contensive Application "
        Public Const ButtonCancelAll = "  Cancel  "
        Public Const ButtonFind = "   Find   "
        Public Const ButtonDelete = "  Delete  "
        Public Const ButtonDeletePerson = " Delete Person "
        Public Const ButtonDeleteRecord = " Delete Record "
        Public Const ButtonDeleteEmail = " Delete Email "
        Public Const ButtonDeletePage = " Delete Page "
        Public Const ButtonFileChange = "   Upload   "
        Public Const ButtonFileDelete = "    Delete    "
        Public Const ButtonClose = "  Close   "
        Public Const ButtonAdd = "   Add    "
        Public Const ButtonAddChildPage = " Add Child "
        Public Const ButtonAddSiblingPage = " Add Sibling "
        Public Const ButtonContinue = " Continue >> "
        Public Const ButtonBack = "  << Back  "
        Public Const ButtonNext = "   Next   "
        Public Const ButtonPrevious = " Previous "
        Public Const ButtonFirst = "  First   "
        Public Const ButtonSend = "  Send   "
        Public Const ButtonSendTest = "Send Test"
        Public Const ButtonCreateDuplicate = " Create Duplicate "
        Public Const ButtonActivate = "  Activate   "
        Public Const ButtonDeactivate = "  Deactivate   "
        Public Const ButtonOpenActiveEditor = "Active Edit"
        Public Const ButtonPublish = " Publish Changes "
        Public Const ButtonAbortEdit = " Abort Edits "
        Public Const ButtonPublishSubmit = " Submit for Publishing "
        Public Const ButtonPublishApprove = " Approve for Publishing "
        Public Const ButtonPublishDeny = " Deny for Publishing "
        Public Const ButtonWorkflowPublishApproved = " Publish Approved Records "
        Public Const ButtonWorkflowPublishSelected = " Publish Selected Records "
        Public Const ButtonSetHTMLEdit = " Edit WYSIWYG "
        Public Const ButtonSetTextEdit = " Edit HTML "
        Public Const ButtonRefresh = " Refresh "
        Public Const ButtonOrder = " Order "
        Public Const ButtonSearch = " Search "
        Public Const ButtonSpellCheck = " Spell Check "
        Public Const ButtonLibraryUpload = " Upload "
        Public Const ButtonCreateReport = " Create Report "
        Public Const ButtonClearTrapLog = " Clear Trap Log "
        Public Const ButtonNewSearch = " New Search "
        Public Const ButtonSaveandInvalidateCache = " Save and Invalidate Cache "
        Public Const ButtonImportTemplates = " Import Templates "
        Public Const ButtonRSSRefresh = " Update RSS Feeds Now "
        Public Const ButtonRequestDownload = " Request Download "
        Public Const ButtonFinish = " Finish "
        Public Const ButtonRegister = " Register "
        Public Const ButtonBegin = "Begin"
        Public Const ButtonAbort = "Abort"
        Public Const ButtonCreateGUID = " Create GUID "
        Public Const ButtonEnable = " Enable "
        Public Const ButtonDisable = " Disable "
        Public Const ButtonMarkReviewed = " Mark Reviewed "
        '
        '------------------------------------------------------------------------
        '   member actions
        '------------------------------------------------------------------------
        '
        Public Const MemberActionNOP = 0
        Public Const MemberActionLogin = 1
        Public Const MemberActionLogout = 2
        Public Const MemberActionForceLogin = 3
        Public Const MemberActionSendPassword = 4
        Public Const MemberActionForceLogout = 5
        Public Const MemberActionToolsApply = 6
        Public Const MemberActionJoin = 7
        Public Const MemberActionSaveProfile = 8
        Public Const MemberActionEditProfile = 9
        '
        '-----------------------------------------------------------------------
        ' ----- note pad info
        '-----------------------------------------------------------------------
        '
        Public Const NoteFormList = 1
        Public Const NoteFormRead = 2
        '
        Public Const NoteButtonPrevious = " Previous "
        Public Const NoteButtonNext = "   Next   "
        Public Const NoteButtonDelete = "  Delete  "
        Public Const NoteButtonClose = "  Close   "
        '                       ' Submit button is created in CommonDim, so it is simple
        Public Const NoteButtonSubmit = "Submit"
        '
        '-----------------------------------------------------------------------
        ' ----- Admin site storage
        '-----------------------------------------------------------------------
        '
        Public Const AdminMenuModeHidden = 0       '   menu is hidden
        Public Const AdminMenuModeLeft = 1     '   menu on the left
        Public Const AdminMenuModeTop = 2      '   menu as dropdowns from the top
        '
        ' ----- AdminActions - describes the form processing to do
        '
        Public Const AdminActionNop = 0            ' do nothing
        Public Const AdminActionDelete = 4         ' delete record
        Public Const AdminActionFind = 5           '
        Public Const AdminActionDeleteFilex = 6        '
        Public Const AdminActionUpload = 7         '
        Public Const AdminActionSaveNormal = 3         ' save fields to database
        Public Const AdminActionSaveEmail = 8          ' save email record (and update EmailGroups) to database
        Public Const AdminActionSaveMember = 11        '
        Public Const AdminActionSaveSystem = 12
        Public Const AdminActionSavePaths = 13     ' Save a record that is in the BathBlocking Format
        Public Const AdminActionSendEmail = 9          '
        Public Const AdminActionSendEmailTest = 10     '
        Public Const AdminActionNext = 14               '
        Public Const AdminActionPrevious = 15           '
        Public Const AdminActionFirst = 16              '
        Public Const AdminActionSaveContent = 17        '
        Public Const AdminActionSaveField = 18          ' Save a single field, fieldname = fn input
        Public Const AdminActionPublish = 19            ' Publish record live
        Public Const AdminActionAbortEdit = 20          ' Publish record live
        Public Const AdminActionPublishSubmit = 21      ' Submit for Workflow Publishing
        Public Const AdminActionPublishApprove = 22     ' Approve for Workflow Publishing
        Public Const AdminActionWorkflowPublishApproved = 23    ' Publish what was approved
        Public Const AdminActionSetHTMLEdit = 24        ' Set Member Property for this field to HTML Edit
        Public Const AdminActionSetTextEdit = 25        ' Set Member Property for this field to Text Edit
        Public Const AdminActionSave = 26               ' Save Record
        Public Const AdminActionActivateEmail = 27      ' Activate a Conditional Email
        Public Const AdminActionDeactivateEmail = 28    ' Deactivate a conditional email
        Public Const AdminActionDuplicate = 29          ' Duplicate the (sent email) record
        Public Const AdminActionDeleteRows = 30         ' Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
        Public Const AdminActionSaveAddNew = 31         ' Save Record and add a new record
        Public Const AdminActionReloadCDef = 32         ' Load Content Definitions
        Public Const AdminActionWorkflowPublishSelected = 33 ' Publish what was selected
        Public Const AdminActionMarkReviewed = 34       ' Mark the record reviewed without making any changes
        Public Const AdminActionEditRefresh = 35        ' reload the page just like a save, but do not save
        '
        ' ----- Adminforms (0-99)
        '
        Public Const AdminFormRoot = 0             ' intro page
        Public Const AdminFormIndex = 1            ' record list page
        Public Const AdminFormHelp = 2             ' popup help window
        Public Const AdminFormUpload = 3           ' encoded file upload form
        Public Const AdminFormEdit = 4             ' Edit form for system format records
        Public Const AdminFormEditSystem = 5       ' Edit form for system format records
        Public Const AdminFormEditNormal = 6       ' record edit page
        Public Const AdminFormEditEmail = 7        ' Edit form for Email format records
        Public Const AdminFormEditMember = 8       ' Edit form for Member format records
        Public Const AdminFormEditPaths = 9        ' Edit form for Paths format records
        Public Const AdminFormClose = 10           ' Special Case - do a window close instead of displaying a form
        Public Const AdminFormReports = 12         ' Call Reports form (admin only)
        'Public Const AdminFormSpider = 13          ' Call Spider
        Public Const AdminFormEditContent = 14     ' Edit form for Content records
        Public Const AdminFormDHTMLEdit = 15       ' ActiveX DHTMLEdit form
        Public Const AdminFormEditPageContent = 16 '
        Public Const AdminFormPublishing = 17       ' Workflow Authoring Publish Control form
        Public Const AdminFormQuickStats = 18       ' Quick Stats (from Admin root)
        Public Const AdminFormResourceLibrary = 19  ' Resource Library without Selects
        Public Const AdminFormEDGControl = 20       ' Control Form for the EDG publishing controls
        Public Const AdminFormSpiderControl = 21    ' Control Form for the Content Spider
        Public Const AdminFormContentChildTool = 22 ' Admin Create Content Child tool
        Public Const AdminformPageContentMap = 23   ' Map all content to a single map
        Public Const AdminformHousekeepingControl = 24 ' Housekeeping control
        Public Const AdminFormCommerceControl = 25
        Public Const AdminFormContactManager = 26
        Public Const AdminFormStyleEditor = 27
        Public Const AdminFormEmailControl = 28
        Public Const AdminFormCommerceInterface = 29
        Public Const AdminFormDownloads = 30
        Public Const AdminformRSSControl = 31
        Public Const AdminFormMeetingSmart = 32
        Public Const AdminFormMemberSmart = 33
        Public Const AdminFormEmailWizard = 34
        Public Const AdminFormImportWizard = 35
        Public Const AdminFormCustomReports = 36
        Public Const AdminFormFormWizard = 37
        Public Const AdminFormLegacyAddonManager = 38
        Public Const AdminFormIndex_SubFormAdvancedSearch = 39
        Public Const AdminFormIndex_SubFormSetColumns = 40
        Public Const AdminFormPageControl = 41
        Public Const AdminFormSecurityControl = 42
        Public Const AdminFormEditorConfig = 43
        Public Const AdminFormBuilderCollection = 44
        Public Const AdminFormClearCache = 45
        Public Const AdminFormMobileBrowserControl = 46
        Public Const AdminFormMetaKeywordTool = 47
        Public Const AdminFormIndex_SubFormExport = 48
        '
        ' ----- AdminFormTools (11,100-199)
        '
        Public Const AdminFormTools = 11           ' Call Tools form (developer only)
        Public Const AdminFormToolRoot = 11         ' These should match for compatibility
        Public Const AdminFormToolCreateContentDefinition = 101
        Public Const AdminFormToolContentTest = 102
        Public Const AdminFormToolConfigureMenu = 103
        Public Const AdminFormToolConfigureListing = 104
        Public Const AdminFormToolConfigureEdit = 105
        Public Const AdminFormToolManualQuery = 106
        Public Const AdminFormToolWriteUpdateMacro = 107
        Public Const AdminFormToolDuplicateContent = 108
        Public Const AdminFormToolDuplicateDataSource = 109
        Public Const AdminFormToolDefineContentFieldsFromTable = 110
        Public Const AdminFormToolContentDiagnostic = 111
        Public Const AdminFormToolCreateChildContent = 112
        Public Const AdminFormToolClearContentWatchLink = 113
        Public Const AdminFormToolSyncTables = 114
        Public Const AdminFormToolBenchmark = 115
        Public Const AdminFormToolSchema = 116
        Public Const AdminFormToolContentFileView = 117
        Public Const AdminFormToolDbIndex = 118
        Public Const AdminFormToolContentDbSchema = 119
        Public Const AdminFormToolLogFileView = 120
        Public Const AdminFormToolLoadCDef = 121
        Public Const AdminFormToolLoadTemplates = 122
        Public Const AdminformToolFindAndReplace = 123
        Public Const AdminformToolCreateGUID = 124
        Public Const AdminformToolIISReset = 125
        Public Const AdminFormToolRestart = 126
        Public Const AdminFormToolWebsiteFileView = 127
        '
        ' ----- Define the index column structure
        '       IndexColumnVariant( 0, n ) is the first column on the left
        '       IndexColumnVariant( 0, IndexColumnField ) = the index into the fields array
        '
        Public Const IndexColumnField = 0          ' The field displayed in the column
        Public Const IndexColumnWIDTH = 1          ' The width of the column
        Public Const IndexColumnSORTPRIORITY = 2       ' lowest columns sorts first
        Public Const IndexColumnSORTDIRECTION = 3      ' direction of the sort on this column
        Public Const IndexColumnSATTRIBUTEMAX = 3      ' the number of attributes here
        Public Const IndexColumnsMax = 50
        '
        ' ----- ReportID Constants, moved to ccCommonModule
        '
        Public Const ReportFormRoot = 1
        Public Const ReportFormDailyVisits = 2
        Public Const ReportFormWeeklyVisits = 12
        Public Const ReportFormSitePath = 4
        Public Const ReportFormSearchKeywords = 5
        Public Const ReportFormReferers = 6
        Public Const ReportFormBrowserList = 8
        Public Const ReportFormAddressList = 9
        Public Const ReportFormContentProperties = 14
        Public Const ReportFormSurveyList = 15
        Public Const ReportFormOrdersList = 13
        Public Const ReportFormOrderDetails = 21
        Public Const ReportFormVisitorList = 11
        Public Const ReportFormMemberDetails = 16
        Public Const ReportFormPageList = 10
        Public Const ReportFormVisitList = 3
        Public Const ReportFormVisitDetails = 17
        Public Const ReportFormVisitorDetails = 20
        Public Const ReportFormSpiderDocList = 22
        Public Const ReportFormSpiderErrorList = 23
        Public Const ReportFormEDGDocErrors = 24
        Public Const ReportFormDownloadLog = 25
        Public Const ReportFormSpiderDocDetails = 26
        Public Const ReportFormSurveyDetails = 27
        Public Const ReportFormEmailDropList = 28
        Public Const ReportFormPageTraffic = 29
        Public Const ReportFormPagePerformance = 30
        Public Const ReportFormEmailDropDetails = 31
        Public Const ReportFormEmailOpenDetails = 32
        Public Const ReportFormEmailClickDetails = 33
        Public Const ReportFormGroupList = 34
        Public Const ReportFormGroupMemberList = 35
        Public Const ReportFormTrapList = 36
        Public Const ReportFormCount = 36
        '
        '=============================================================================
        ' Page Scope Meetings Related Storage
        '=============================================================================
        '
        Public Const MeetingFormIndex = 0
        Public Const MeetingFormAttendees = 1
        Public Const MeetingFormLinks = 2
        Public Const MeetingFormFacility = 3
        Public Const MeetingFormHotel = 4
        Public Const MeetingFormDetails = 5
        '
        '------------------------------------------------------------------------------
        ' Form actions
        '------------------------------------------------------------------------------
        '
        ' ----- DataSource Types
        '
        Public Const DataSourceTypeODBCSQL99 = 0
        Public Const DataSourceTypeODBCAccess = 1
        Public Const DataSourceTypeODBCSQLServer = 2
        Public Const DataSourceTypeODBCMySQL = 3
        Public Const DataSourceTypeXMLFile = 4      ' Use MSXML Interface to open a file
        '
        '------------------------------------------------------------------------------
        '   Application Status
        '------------------------------------------------------------------------------
        '
        <ComVisible(True)>
        Public Enum applicationStatusEnum
            ApplicationStatusNotFound = 0
            ApplicationStatusNotEnabled = 1
            ApplicationStatusReady = 2
            ApplicationStatusLoading = 3
            ApplicationStatusUpgrading = 4
            ' ApplicationStatusConnectionBusy = 5    ' can not open connection because already open
            ApplicationStatusKernelFailure = 6     ' can not create Kernel
            ApplicationStatusNoHostService = 7     ' host service process ID not set
            ApplicationStatusLicenseFailure = 8    ' failed to start because of License failure
            ApplicationStatusDbNotFound = 9         ' failed to start because ccSetup table not found
            ApplicationStatusFailedToInitialize = 10   ' failed to start because of unknown error, see trace log
            ApplicationStatusDbBad = 11            ' ccContent,ccFields no records found
            ApplicationStatusConnectionObjectFailure = 12 ' Connection Object FAiled
            ApplicationStatusConnectionStringFailure = 13 ' Connection String FAiled to open the ODBC connection
            ApplicationStatusDataSourceFailure = 14 ' DataSource failed to open
            ApplicationStatusDuplicateDomains = 15 ' Can not locate application because there are 1+ apps that match the domain
            ApplicationStatusPaused = 16           ' Running, but all activity is blocked (for backup)
            ApplicationStatusAppConfigNotFound = 17
            ApplicationStatusAppConfigNotValid = 18
            ApplicationStatusDbFoundButContentMetaMissing = 19
        End Enum

        '
        ' Document (HTML, graphic, etc) retrieved from site
        '
        Public Const ResponseHeaderCountMax = 20
        Public Const ResponseCookieCountMax = 20
        '
        ' ----- text delimiter that divides the text and html parts of an email message stored in the queue folder
        '
        Public Const EmailTextHTMLDelimiter = vbCrLf & " ----- End Text Begin HTML -----" & vbCrLf
        '
        '------------------------------------------------------------------------
        '   Common RequestName Variables
        '------------------------------------------------------------------------
        '
        Public Const RequestNameDynamicFormID = "dformid"
        '
        Public Const RequestNameRunAddon = "addonid"
        Public Const RequestNameEditReferer = "EditReferer"
        'Public Const RequestNameRefreshBlock = "ccFormRefreshBlockSN"
        Public Const RequestNameCatalogOrder = "CatalogOrderID"
        Public Const RequestNameCatalogCategoryID = "CatalogCatID"
        Public Const RequestNameCatalogForm = "CatalogFormID"
        Public Const RequestNameCatalogItemID = "CatalogItemID"
        Public Const RequestNameCatalogItemAge = "CatalogItemAge"
        Public Const RequestNameCatalogRecordTop = "CatalogTop"
        Public Const RequestNameCatalogFeatured = "CatalogFeatured"
        Public Const RequestNameCatalogSpan = "CatalogSpan"
        Public Const RequestNameCatalogKeywords = "CatalogKeywords"
        Public Const RequestNameCatalogSource = "CatalogSource"
        '
        Public Const RequestNameLibraryFileID = "fileEID"
        Public Const RequestNameDownloadID = "downloadid"
        Public Const RequestNameLibraryUpload = "LibraryUpload"
        Public Const RequestNameLibraryName = "LibraryName"
        Public Const RequestNameLibraryDescription = "LibraryDescription"

        Public Const RequestNameRootPage = "RootPageName"
        Public Const RequestNameRootPageID = "RootPageID"
        Public Const RequestNameContent = "ContentName"
        Public Const RequestNameOrderByClause = "OrderByClause"
        Public Const RequestNameAllowChildPageList = "AllowChildPageList"
        '
        Public Const RequestNameCRKey = "crkey"
        Public Const RequestNameAdminForm = "af"
        Public Const RequestNameAdminSubForm = "subform"
        Public Const RequestNameButton = "button"
        Public Const RequestNameAdminSourceForm = "asf"
        Public Const RequestNameAdminFormSpelling = "SpellingRequest"
        Public Const RequestNameInlineStyles = "InlineStyles"
        Public Const RequestNameAllowCSSReset = "AllowCSSReset"
        '
        Public Const RequestNameReportForm = "rid"
        '
        Public Const RequestNameToolContentID = "ContentID"
        '
        Public Const RequestNameCut = "a904o2pa0cut"
        Public Const RequestNamePaste = "dp29a7dsa6paste"
        Public Const RequestNamePasteParentContentID = "dp29a7dsa6cid"
        Public Const RequestNamePasteParentRecordID = "dp29a7dsa6rid"
        Public Const RequestNamePasteFieldList = "dp29a7dsa6key"
        Public Const RequestNameCutClear = "dp29a7dsa6clear"
        '
        Public Const RequestNameRequestBinary = "RequestBinary"
        ' removed -- this was an old method of blocking form input for file uploads
        'Public Const RequestNameFormBlock = "RB"
        Public Const RequestNameJSForm = "RequestJSForm"
        Public Const RequestNameJSProcess = "ProcessJSForm"
        '
        Public Const RequestNameFolderID = "FolderID"
        '
        Public Const RequestNameEmailMemberID = "emi8s9Kj"
        Public Const RequestNameEmailOpenFlag = "eof9as88"
        Public Const RequestNameEmailOpenCssFlag = "8aa41pM3"
        Public Const RequestNameEmailClickFlag = "ecf34Msi"
        Public Const RequestNameEmailSpamFlag = "9dq8Nh61"
        Public Const RequestNameEmailBlockRequestDropID = "BlockEmailRequest"
        Public Const RequestNameVisitTracking = "s9lD1088"
        Public Const RequestNameBlockContentTracking = "BlockContentTracking"
        Public Const RequestNameCookieDetectVisitID = "f92vo2a8d"

        Public Const RequestNamePageNumber = "PageNumber"
        Public Const RequestNamePageSize = "PageSize"
        '
        Public Const RequestValueNull = "[NULL]"
        '
        Public Const SpellCheckUserDictionaryFilename = "SpellCheck\UserDictionary.txt"
        '
        Public Const RequestNameStateString = "vstate"
        '
        ' ----- Dataset for graphing
        '
        Public Class ColumnDataType
            Dim Name As String
            Dim row() As Integer
        End Class
        '
        Public Class ChartDataType
            Dim Title As String
            Dim XLabel As String
            Dim YLabel As String
            Dim RowCount As Integer
            Dim RowLabel() As String
            Dim ColumnCount As Integer
            Dim Column() As ColumnDataType
        End Class
        ''
        ' PrivateStorage to hold the DebugTimer
        '
        Class TimerStackType
            Dim Label As String
            Dim StartTicks As Integer
        End Class
        Private Const TimerStackMax = 20
        Private TimerStack(TimerStackMax) As TimerStackType
        Private TimerStackCount As Integer
        '
        Public Const TextSearchStartTagDefault = "<!--TextSearchStart-->"
        Public Const TextSearchEndTagDefault = "<!--TextSearchEnd-->"
        '
        '-------------------------------------------------------------------------------------
        '   IPDaemon communication objects
        '-------------------------------------------------------------------------------------
        '
        Class IPDaemonConnectionType
            Dim ConnectionID As Integer
            Dim BytesToSend As Integer
            Dim HTTPVersion As String
            Dim HTTPMethod As String
            Dim Path As String
            Dim Query As String
            Dim Headers As String
            Dim PostData As String
            Dim SendData As Boolean
            Dim State As Integer
            Dim ContentLength As Integer
        End Class
        '
        '-------------------------------------------------------------------------------------
        '   Email
        '-------------------------------------------------------------------------------------
        '
        Public Const EmailLogTypeDrop = 1                   ' Email was dropped
        Public Const EmailLogTypeOpen = 2                   ' System detected the email was opened
        Public Const EmailLogTypeClick = 3                  ' System detected a click from a link on the email
        Public Const EmailLogTypeBounce = 4                 ' Email was processed by bounce processing
        Public Const EmailLogTypeBlockRequest = 5           ' recipient asked us to stop sending email
        Public Const EmailLogTypeImmediateSend = 6        ' Email was dropped
        '
        Public Const DefaultSpamFooter = "<p>To block future emails from this site, <link>click here</link></p>"
        '
        Public Const FeedbackFormNotSupportedComment = "<!--" & vbCrLf & "Feedback form is not supported in this context" & vbCrLf & "-->"
        '
        '-------------------------------------------------------------------------------------
        '   Page Content constants
        '-------------------------------------------------------------------------------------
        '
        Public Const ContentBlockCopyName = "Content Block Copy"
        '
        Public Const BubbleCopy_AdminAddPage = "Use the Add page to create new content records. The save button puts you in edit mode. The OK button creates the record and exits."
        Public Const BubbleCopy_AdminIndexPage = "Use the Admin Listing page to locate content records through the Admin Site."
        Public Const BubbleCopy_SpellCheckPage = "Use the Spell Check page to verify and correct spelling throught the content."
        Public Const BubbleCopy_AdminEditPage = "Use the Edit page to add and modify content."
        '
        '
        Public Const TemplateDefaultName = "Default"
        'Public Const TemplateDefaultBody = "<!--" & vbCrLf & "Default Template - edit this Page Template, or select a different template for your page or section" & vbCrLf & "-->{{DYNAMICMENU?MENU=}}<br>{{CONTENT}}"
        Public Const TemplateDefaultBody = "" _
            & vbCrLf & vbTab & "<!--" _
            & vbCrLf & vbTab & "Default Template - edit this Page Template, or select a different template for your page or section" _
            & vbCrLf & vbTab & "-->" _
            & vbCrLf & vbTab & "<ac type=""AGGREGATEFUNCTION"" name=""Dynamic Menu"" querystring=""Menu Name=Default"" acinstanceid=""{6CBADABB-5B0D-43E1-B3CA-46A3D60DA3E1}"" >" _
            & vbCrLf & vbTab & "<ac type=""AGGREGATEFUNCTION"" name=""Content Box"" acinstanceid=""{49E0D0C0-D323-49B6-B211-B9599673A265}"" >"
        Public Const TemplateDefaultBodyTag = "<body class=""ccBodyWeb"">"
        '
        '=======================================================================
        '   Internal Tab interface storage
        '=======================================================================
        '
        Private Class TabType
            Dim Caption As String
            Dim Link As String
            Dim StylePrefix As String
            Dim IsHit As Boolean
            Dim LiveBody As String
        End Class
        '
        Private Tabs() As TabType
        Private TabsCnt As Integer
        Private TabsSize As Integer
        '
        ' Admin Navigator Nodes
        '
        Public Const NavigatorNodeCollectionList = -1
        Public Const NavigatorNodeAddonList = -1
        ''
        '' Pointers into index of PCC (Page Content Cache) array
        ''
        'Public Const PCC_ID = 0
        'Public Const PCC_Active = 1
        'Public Const PCC_ParentID = 2
        'Public Const PCC_Name = 3
        'Public Const PCC_Headline = 4
        'Public Const PCC_MenuHeadline = 5
        'Public Const PCC_DateArchive = 6
        'Public Const PCC_DateExpires = 7
        'Public Const PCC_PubDate = 8
        'Public Const PCC_ChildListSortMethodID = 9
        'Public Const PCC_ContentControlID = 10
        'Public Const PCC_TemplateID = 11
        'Public Const PCC_BlockContent = 12
        'Public Const PCC_BlockPage = 13
        'Public Const PCC_Link = 14
        'Public Const PCC_RegistrationGroupID = 15
        'Public Const PCC_BlockSourceID = 16
        'Public Const PCC_CustomBlockMessageFilename = 17
        'Public Const PCC_JSOnLoad = 18
        'Public Const PCC_JSHead = 19
        'Public Const PCC_JSEndBody = 20
        'Public Const PCC_Viewings = 21
        'Public Const PCC_ContactMemberID = 22
        'Public Const PCC_AllowHitNotification = 23
        'Public Const PCC_TriggerSendSystemEmailID = 24
        'Public Const PCC_TriggerConditionID = 25
        'Public Const PCC_TriggerConditionGroupID = 26
        'Public Const PCC_TriggerAddGroupID = 27
        'Public Const PCC_TriggerRemoveGroupID = 28
        'Public Const PCC_AllowMetaContentNoFollow = 29
        'Public Const PCC_ParentListName = 30
        'Public Const PCC_CopyFilename = 31
        'Public Const PCC_BriefFilename = 32
        'Public Const PCC_AllowChildListDisplay = 33
        'Public Const PCC_SortOrder = 34
        'Public Const PCC_DateAdded = 35
        'Public Const PCC_ModifiedDate = 36
        'Public Const PCC_ChildPagesFound = 37
        'Public Const PCC_AllowInMenus = 38
        'Public Const PCC_AllowInChildLists = 39
        'Public Const PCC_JSFilename = 40
        'Public Const PCC_ChildListInstanceOptions = 41
        'Public Const PCC_IsSecure = 42
        'Public Const PCC_AllowBrief = 43
        'Public Const PCC_ColCnt = 44
        '
        ' Indexes into the SiteSectionCache
        ' Created from "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody"
        '
        Public Const SSC_ID = 0
        Public Const SSC_Name = 1
        Public Const SSC_TemplateID = 2
        Public Const SSC_ContentID = 3
        Public Const SSC_MenuImageFilename = 4
        Public Const SSC_Caption = 5
        Public Const SSC_MenuImageOverFilename = 6
        Public Const SSC_HideMenu = 7
        Public Const SSC_BlockSection = 8
        Public Const SSC_RootPageID = 9
        Public Const SSC_JSOnLoad = 10
        Public Const SSC_JSHead = 11
        Public Const SSC_JSEndBody = 12
        Public Const SSC_JSFilename = 13
        Public Const SSC_cnt = 14
        '
        ' Indexes into the TemplateCache
        ' Created from "t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID"
        '
        Public Const TC_ID = 0
        Public Const TC_Name = 1
        Public Const TC_Link = 2
        Public Const TC_BodyHTML = 3
        Public Const TC_JSOnLoad = 4
        Public Const TC_JSInHeadLegacy = 5
        'Public Const TC_JSHead = 5
        Public Const TC_JSEndBody = 6
        Public Const TC_StylesFilename = 7
        Public Const TC_SharedStylesIDList = 8
        Public Const TC_MobileBodyHTML = 9
        Public Const TC_MobileStylesFilename = 10
        Public Const TC_OtherHeadTags = 11
        Public Const TC_BodyTag = 12
        Public Const TC_JSInHeadFilename = 13
        'Public Const TC_JSFilename = 13
        Public Const TC_IsSecure = 14
        Public Const TC_DomainIdList = 15
        ' for now, Mobile templates do not have shared styles
        'Public Const TC_MobileSharedStylesIDList = 11
        Public Const TC_cnt = 16
        '
        ' DTD
        '
        Public Const DTDDefault = "<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">"
        Public Const DTDDefaultAdmin = "<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.01 Transitional//EN"" ""http://www.w3.org/TR/html4/loose.dtd"">"
        '
        ' innova Editor feature list
        '
        Public Const InnovaEditorFeaturefilename = "innova\EditorConfig.txt"
        Public Const InnovaEditorFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Image,Flash,Media,CustomObject,CustomTag,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
        Public Const InnovaEditorPublicFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor"
        ''
        '' Content Type
        ''
        'Enum contentTypeEnum
        '    contentTypeWeb = 1
        '    ContentTypeEmail = 2
        '    contentTypeWebTemplate = 3
        '    contentTypeEmailTemplate = 4
        'End Enum
        'Public EditorContext As contentTypeEnum
        'Enum EditorContextEnum
        '    contentTypeWeb = 1
        '    contentTypeEmail = 2
        'End Enum
        'Public EditorContext As EditorContextEnum
        ''
        'Public Const EditorAddonMenuEmailTemplateFilename = "templates/EditorAddonMenuTemplateEmail.js"
        'Public Const EditorAddonMenuEmailContentFilename = "templates/EditorAddonMenuContentEmail.js"
        'Public Const EditorAddonMenuWebTemplateFilename = "templates/EditorAddonMenuTemplateWeb.js"
        'Public Const EditorAddonMenuWebContentFilename = "templates/EditorAddonMenuContentWeb.js"
        '
        Public Const DynamicStylesFilename = "templates/styles.css"
        Public Const AdminSiteStylesFilename = "templates/AdminSiteStyles.css"
        Public Const EditorStyleRulesFilenamePattern = "templates/EditorStyleRules$TemplateID$.js"
        ' deprecated 11/24/3009 - StyleRules destinction between web/email not needed b/c body background blocked
        'Public Const EditorStyleWebRulesFilename = "templates/EditorStyleWebRules.js"
        'Public Const EditorStyleEmailRulesFilename = "templates/EditorStyleEmailRules.js"
        '
        ' ----- ccGroupRules storage for list of Content that a group can author
        '
        Public Class ContentGroupRuleType
            Dim ContentID As Integer
            Dim GroupID As Integer
            Dim AllowAdd As Boolean
            Dim AllowDelete As Boolean
        End Class
        '
        ' ----- This should match the Lookup List in the NavIconType field in the Navigator Entry content definition
        '
        Public Const navTypeIDList = "Add-on,Report,Setting,Tool"
        Public Const NavTypeIDAddon = 1
        Public Const NavTypeIDReport = 2
        Public Const NavTypeIDSetting = 3
        Public Const NavTypeIDTool = 4
        '
        Public Const NavIconTypeList = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help"
        Public Const NavIconTypeCustom = 1
        Public Const NavIconTypeAdvanced = 2
        Public Const NavIconTypeContent = 3
        Public Const NavIconTypeFolder = 4
        Public Const NavIconTypeEmail = 5
        Public Const NavIconTypeUser = 6
        Public Const NavIconTypeReport = 7
        Public Const NavIconTypeSetting = 8
        Public Const NavIconTypeTool = 9
        Public Const NavIconTypeRecord = 10
        Public Const NavIconTypeAddon = 11
        Public Const NavIconTypeHelp = 12
        '
        Public Const QueryTypeSQL = 1
        Public Const QueryTypeOpenContent = 2
        Public Const QueryTypeUpdateContent = 3
        Public Const QueryTypeInsertContent = 4
        '
        ' Google Data Object construction in GetRemoteQuery
        '
        Public Class ColsType
            Public Type As String
            Public Id As String
            Public Label As String
            Public Pattern As String
        End Class
        '
        Public Class CellType
            Public v As String
            Public f As String
            Public p As String
        End Class
        '
        Public Class RowsType
            Public Cell() As CellType
        End Class
        '
        Public Class GoogleDataType
            Public IsEmpty As Boolean
            Public col() As ColsType
            Public row() As RowsType
        End Class
        '
        Public Enum GoogleVisualizationStatusEnum
            OK = 1
            warning = 2
            ErrorStatus = 3
        End Enum
        '
        Public Class GoogleVisualizationType
            Public version As String
            Public reqid As String
            Public status As GoogleVisualizationStatusEnum
            Public warnings() As String
            Public errors() As String
            Public sig As String
            Public table As GoogleDataType
        End Class

        'Public Const ReturnFormatTypeGoogleTable = 1
        'Public Const ReturnFormatTypeNameValue = 2

        Public Enum RemoteFormatEnum
            RemoteFormatJsonTable = 1
            RemoteFormatJsonNameArray = 2
            RemoteFormatJsonNameValue = 3
        End Enum
        'Private Enum GoogleVisualizationStatusEnum
        '    OK = 1
        '    warning = 2
        '    ErrorStatus = 3
        'End Enum
        ''
        'Private Structure GoogleVisualizationType
        '    Dim version As String
        '    Dim reqid As String
        '    Dim status As GoogleVisualizationStatusEnum
        '    Dim warnings() As String
        '    Dim errors() As String
        '    Dim sig As String
        '    Dim table As GoogleDataType
        'End Structure        '
        '
        '
        Public Declare Function RegCloseKey& Lib "advapi32.dll" (ByVal hKey&)
        Public Declare Function RegOpenKeyExA& Lib "advapi32.dll" (ByVal hKey&, ByVal lpszSubKey$, ByVal dwOptions&, ByVal samDesired&, ByVal lpHKey&)
        Public Declare Function RegQueryValueExA& Lib "advapi32.dll" (ByVal hKey&, ByVal lpszValueName$, ByVal lpdwRes&, ByVal lpdwType&, ByVal lpDataBuff$, ByVal nSize&)
        Public Declare Function RegQueryValueEx& Lib "advapi32.dll" Alias "RegQueryValueExA" (ByVal hKey&, ByVal lpszValueName$, ByVal lpdwRes&, ByVal lpdwType&, ByVal lpDataBuff&, ByVal nSize&)

        Public Const HKEY_CLASSES_ROOT = &H80000000
        Public Const HKEY_CURRENT_USER = &H80000001
        Public Const HKEY_LOCAL_MACHINE = &H80000002
        Public Const HKEY_USERS = &H80000003

        Public Const ERROR_SUCCESS = 0&
        Public Const REG_SZ = 1&                          ' Unicode nul terminated string
        Public Const REG_DWORD = 4&                       ' 32-bit number

        Public Const KEY_QUERY_VALUE = &H1&
        Public Const KEY_SET_VALUE = &H2&
        Public Const KEY_CREATE_SUB_KEY = &H4&
        Public Const KEY_ENUMERATE_SUB_KEYS = &H8&
        Public Const KEY_NOTIFY = &H10&
        Public Const KEY_CREATE_LINK = &H20&
        Public Const READ_CONTROL = &H20000
        Public Const WRITE_DAC = &H40000
        Public Const WRITE_OWNER = &H80000
        Public Const SYNCHRONIZE = &H100000
        Public Const STANDARD_RIGHTS_REQUIRED = &HF0000
        Public Const STANDARD_RIGHTS_READ = READ_CONTROL
        Public Const STANDARD_RIGHTS_WRITE = READ_CONTROL
        Public Const STANDARD_RIGHTS_EXECUTE = READ_CONTROL
        Public Const KEY_READ = STANDARD_RIGHTS_READ Or KEY_QUERY_VALUE Or KEY_ENUMERATE_SUB_KEYS Or KEY_NOTIFY
        Public Const KEY_WRITE = STANDARD_RIGHTS_WRITE Or KEY_SET_VALUE Or KEY_CREATE_SUB_KEY
        Public Const KEY_EXECUTE = KEY_READ

        '    '======================================================================================
        '    '
        '    '======================================================================================
        '    '
        '    Public Sub StartDebugTimer(Enabled As Boolean, Label As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        If Enabled Then
        '            If TimerStackCount < TimerStackMax Then
        '                TimerStack(TimerStackCount).Label = Label
        '                TimerStack(TimerStackCount).StartTicks = GetTickCount
        '            Else
        '                Call AppendLogFile("dll" & ".?.StartDebugTimer, " & "Timer Stack overflow, attempting push # [" & TimerStackCount & "], but max = [" & TimerStackMax & "]")
        '            End If
        '            TimerStackCount = TimerStackCount + 1
        '        End If
        '    End Sub
        '    '
        '    Public Sub StopDebugTimer(Enabled As Boolean, Label As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        If Enabled Then
        '            If TimerStackCount <= 0 Then
        '                Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, attempting to Pop, but the stack is empty")
        '            Else
        '                If TimerStackCount <= TimerStackMax Then
        '                    If TimerStack(TimerStackCount - 1).Label = Label Then
        '                    Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer [" & String(2 * TimerStackCount, ".") & Label & "] took " & (GetTickCount - TimerStack(TimerStackCount - 1).StartTicks) & " msec")
        '                    Else
        '                        Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, [" & Label & "] was popped, but [" & TimerStack(TimerStackCount).Label & "] was on the top of the stack")
        '                    End If
        '                End If
        '                TimerStackCount = TimerStackCount - 1
        '            End If
        '        End If
        '    End Sub
        '    '
        '    '
        '    '
        '    Public Function PayString(Index) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        Select Case Index
        '            Case PayTypeCreditCardOnline
        '                PayString = "Credit Card"
        '            Case PayTypeCreditCardByPhone
        '                PayString = "Credit Card by phone"
        '            Case PayTypeCreditCardByFax
        '                PayString = "Credit Card by fax"
        '            Case PayTypeCHECK
        '                PayString = "Personal Check"
        '            Case PayTypeCHECKCOMPANY
        '                PayString = "Company Check"
        '            Case PayTypeCREDIT
        '                PayString = "You will be billed"
        '            Case PayTypeNetTerms
        '                PayString = "Net Terms (Approved customers only)"
        '            Case PayTypeCODCompanyCheck
        '                PayString = "COD- Pre-Approved Only"
        '            Case PayTypeCODCertifiedFunds
        '                PayString = "COD- Certified Funds"
        '            Case PayTypePAYPAL
        '                PayString = "PayPal"
        '            Case Else
        '                ' Case PayTypeNONE
        '                PayString = "No payment required"
        '        End Select
        '    End Function
        '    '
        '    '
        '    '
        '    Public Function CCString(Index) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        Select Case Index
        '            Case CCTYPEVISA
        '                CCString = "Visa"
        '            Case CCTYPEMC
        '                CCString = "MasterCard"
        '            Case CCTYPEAMEX
        '                CCString = "American Express"
        '            Case CCTYPEDISCOVER
        '                CCString = "Discover"
        '            Case Else
        '                ' Case CCTYPENOVUS
        '                CCString = "Novus Card"
        '        End Select
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a CommandPacket
        '    '   position+0, 4 byte value
        '    '========================================================================
        '    '
        '    Public Function GetLongFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        GetLongFromByteArray = ByteArray(Position + 3)
        '        GetLongFromByteArray = ByteArray(Position + 2) + (256 * GetLongFromByteArray)
        '        GetLongFromByteArray = ByteArray(Position + 1) + (256 * GetLongFromByteArray)
        '        GetLongFromByteArray = ByteArray(Position + 0) + (256 * GetLongFromByteArray)
        '        Position = Position + 4
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a byte array
        '    '   position+0, 4 byte size of the number
        '    '   position+3, start of the number
        '    '========================================================================
        '    '
        '    Public Function GetNumberFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim ArgumentCount As Integer
        '        Dim ArgumentLength As Integer
        '        '
        '        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        '        '
        '        If ArgumentLength > 0 Then
        '            GetNumberFromByteArray = 0
        '            For ArgumentCount = ArgumentLength - 1 To 0 Step -1
        '                GetNumberFromByteArray = ByteArray(Position + ArgumentCount) + (256 * GetNumberFromByteArray)
        '            Next
        '        End If
        '        Position = Position + ArgumentLength
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a String a byte array
        '    '   position+0, 4 byte length of the string
        '    '   position+3, start of the string
        '    '========================================================================
        '    '
        '    Public Function GetStringFromByteArray(ByteArray() As Byte, Position As Integer) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Pointer As Integer
        '        Dim ArgumentLength As Integer
        '        '
        '        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        '        '
        '        GetStringFromByteArray = ""
        '        If ArgumentLength > 0 Then
        '            For Pointer = 0 To ArgumentLength - 1
        '                GetStringFromByteArray = GetStringFromByteArray & chr(ByteArray(Position + Pointer))
        '            Next
        '        End If
        '        Position = Position + ArgumentLength
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' Get a Long from a byte array
        '    '========================================================================
        '    '
        '    Public Sub SetLongByteArray(ByRef ByteArray() As Byte, Position As Integer, LongValue As Integer)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        ByteArray(Position + 0) = LongValue And (&HFF)
        '        ByteArray(Position + 1) = Int(LongValue / 256) And (&HFF)
        '        ByteArray(Position + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        '        ByteArray(Position + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        '        Position = Position + 4
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    ' Set a string in a byte array
        '    '========================================================================
        '    '
        '    Public Sub SetStringByteArray(ByRef ByteArray() As Byte, Position As Integer, StringValue As String)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Pointer As Integer
        '        Dim LenStringValue As Integer
        '        '
        '        LenStringValue = Len(StringValue)
        '        If LenStringValue > 0 Then
        '            For Pointer = 0 To LenStringValue - 1
        '                ByteArray(Position + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        '            Next
        '            Position = Position + LenStringValue
        '        End If
        '        '
        '    End Sub

        '    '
        '    '========================================================================
        '    '   a Long long on the end of a RMB (Remote Method Block)
        '    '       You determine the position, or it will add it to the end
        '    '========================================================================
        '    '
        'Public Sub SetRMBLong(ByRef ByteArray() As Byte, LongValue As Integer, Optional Position)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Temp As Integer
        '        Dim MyPosition As Integer
        '        Dim ByteArraySize As Integer
        '        '
        '        ' ----- determine the position
        '        '
        '        If Not IsMissing(Position) Then
        '            MyPosition = Position
        '        Else
        '            '
        '            ' ----- Add it to the end, determine length
        '            '
        '            MyPosition = ByteArray(RMBPositionLength + 3)
        '            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        '            '
        '            ' ----- adjust size of array if necessary
        '            '
        '            ByteArraySize = UBound(ByteArray)
        '            If ByteArraySize < (MyPosition + 8) Then
        '                ReDim Preserve ByteArray(ByteArraySize + 8)
        '            End If
        '        End If
        '        '
        '        ' ----- set the length
        '        '
        '        'ByteArray(MyPosition + 0) = 4
        '        'ByteArray(MyPosition + 1) = 0
        '        'ByteArray(MyPosition + 2) = 0
        '        'ByteArray(MyPosition + 3) = 0
        '        'MyPosition = MyPosition + 4
        '        '
        '        ' ----- set the value
        '        '
        '        ByteArray(MyPosition + 0) = LongValue And (&HFF)
        '        ByteArray(MyPosition + 1) = Int(LongValue / 256) And (&HFF)
        '        ByteArray(MyPosition + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        '        ByteArray(MyPosition + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        '        MyPosition = MyPosition + 4
        '        '
        '        If IsMissing(Position) Then
        '            '
        '            ' ----- Adjust the RMB length if length not given
        '            '
        '            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        '            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        '            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        '            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        '        End If
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    '   a Long long on the end of a RMB (Remote Method Block)
        '    '       You determine the position, or it will add it to the end
        '    '========================================================================
        '    '
        'Public Sub SetRMBString(ByRef ByteArray() As Byte, StringValue As String, Optional Position)
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Temp As Integer
        '        Dim MyPosition As Integer
        '        Dim ByteArraySize As Integer
        '        '
        '        ' ----- determine the position
        '        '
        '        If Not IsMissing(Position) Then
        '            MyPosition = Position
        '        Else
        '            '
        '            ' ----- Add it to the end, determine length
        '            '
        '            MyPosition = ByteArray(RMBPositionLength + 3)
        '            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        '            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        '            '
        '            ' ----- adjust size of array if necessary
        '            '
        '            ByteArraySize = UBound(ByteArray)
        '            If ByteArraySize < (MyPosition + 8) Then
        '                ReDim Preserve ByteArray(ByteArraySize + 4 + Len(StringValue))
        '            End If
        '        End If
        '        '
        '        ' ----- set the value
        '        '

        '        '
        '        Dim Pointer As Integer
        '        Dim LenStringValue As Integer
        '        '
        '        LenStringValue = Len(StringValue)
        '        If LenStringValue > 0 Then
        '            For Pointer = 0 To LenStringValue - 1
        '                ByteArray(MyPosition + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        '            Next
        '            MyPosition = MyPosition + LenStringValue
        '        End If
        '        '
        '        If IsMissing(Position) Then
        '            '
        '            ' ----- Adjust the RMB length if length not given
        '            '
        '            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        '            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        '            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        '            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        '        End If
        '        '
        '    End Sub
        '    '
        '    '========================================================================
        '    '   IsTrue
        '    '       returns true or false depending on the state of the variant input
        '    '========================================================================
        '    '
        '    Function IsTrue(ValueVariant) As Boolean
        '        IsTrue = encodeBoolean(ValueVariant)
        '    End Function
        '    '
        '    '========================================================================
        '    ' EncodeXML
        '    '
        '    '========================================================================
        '    '
        '    Function EncodeXML(ValueVariant As Object, fieldType As Integer) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim TimeValuething As Single
        '        Dim TimeHours As Integer
        '        Dim TimeMinutes As Integer
        '        Dim TimeSeconds As Integer
        '        '
        '        Select Case fieldType
        '            Case FieldTypeInteger, FieldTypeLookup, FieldTypeRedirect, FieldTypeManyToMany, FieldTypeMemberSelect
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = Int(ValueVariant)
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeBoolean
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "0"
        '                ElseIf ValueVariant <> False Then
        '                    EncodeXML = "1"
        '                Else
        '                    EncodeXML = "0"
        '                End If
        '            Case FieldTypeCurrency
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = ValueVariant
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeFloat
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf vbIsNumeric(ValueVariant) Then
        '                    EncodeXML = ValueVariant
        '                Else
        '                    EncodeXML = "null"
        '                End If
        '            Case FieldTypeDate
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = "null"
        '                ElseIf IsDate(ValueVariant) Then
        '                    'TimeVar = CDate(ValueVariant)
        '                    'TimeValuething = 86400! * (TimeVar - Int(TimeVar))
        '                    'TimeHours = Int(TimeValuething / 3600!)
        '                    'TimeMinutes = Int(TimeValuething / 60!) - (TimeHours * 60)
        '                    'TimeSeconds = TimeValuething - (TimeHours * 3600!) - (TimeMinutes * 60!)
        '                    'EncodeXML = Year(ValueVariant) & "-" & Right("0" & Month(ValueVariant), 2) & "-" & Right("0" & Day(ValueVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2)
        '                    EncodeXML = encodeText(ValueVariant)
        '                End If
        '            Case Else
        '                '
        '                ' ----- FieldTypeText
        '                ' ----- FieldTypeLongText
        '                ' ----- FieldTypeFile
        '                ' ----- FieldTypeImage
        '                ' ----- FieldTypeTextFile
        '                ' ----- FieldTypeCSSFile
        '                ' ----- FieldTypeXMLFile
        '                ' ----- FieldTypeJavascriptFile
        '                ' ----- FieldTypeLink
        '                ' ----- FieldTypeResourceLink
        '                ' ----- FieldTypeHTML
        '                ' ----- FieldTypeHTMLFile
        '                '
        '                If IsNull(ValueVariant) Then
        '                    EncodeXML = "null"
        '                ElseIf ValueVariant = "" Then
        '                    EncodeXML = ""
        '                Else
        '                    'EncodeXML = ASPServer.HTMLEncode(ValueVariant)
        '                    'EncodeXML = vbReplace(ValueVariant, "&", "&lt;")
        '                    'EncodeXML = vbReplace(ValueVariant, "<", "&lt;")
        '                    'EncodeXML = vbReplace(EncodeXML, ">", "&gt;")
        '                End If
        '        End Select
        '        '
        '    End Function
        '
        '========================================================================
        ' EncodeFilename
        '
        '========================================================================
        '
        Public Function encodeFilename(ByVal Source As String) As String
            Dim allowed As String
            Dim chr As String
            Dim Ptr As Integer
            Dim Cnt As Integer
            Dim returnString As String
            '
            returnString = ""
            Cnt = Len(Source)
            If Cnt > 254 Then
                Cnt = 254
            End If
            allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_"
            For Ptr = 1 To Cnt
                chr = Mid(Source, Ptr, 1)
                If (InStr(1, allowed, chr, vbBinaryCompare) >= 0) Then
                    returnString = returnString & chr
                End If
            Next
            encodeFilename = returnString
        End Function
        '    '
        '    'Function encodeFilename(Filename As String) As String
        '    '    ' ##### removed to catch err<>0 problem on error resume next
        '    '    '
        '    '    Dim Source() as object
        '    '    Dim Replacement() as object
        '    '    '
        '    '    Source = Array("""", "*", "/", ":", "<", ">", "?", "\", "|", "=")
        '    '    Replacement = Array("_", "_", "_", "_", "_", "_", "_", "_", "_", "_")
        '    '    '
        '    '    encodeFilename = ReplaceMany(Filename, Source, Replacement)
        '    '    If Len(encodeFilename) > 254 Then
        '    '        encodeFilename = Left(encodeFilename, 254)
        '    '    End If
        '    '    encodeFilename = vbReplace(encodeFilename, vbCr, "_")
        '    '    encodeFilename = vbReplace(encodeFilename, vbLf, "_")
        '    '    '
        '    '    End Function
        '    '
        '    '
        '    '

        '    '
        '    '========================================================================
        '    ' DecodeHTML
        '    '
        '    '========================================================================
        '    '
        '    Function DecodeHTML(Source As String) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        DecodeHTML = decodeHtml(Source)
        '        'Dim SourceChr() as object
        '        'Dim ReplacementChr() as object
        '        ''
        '        'SourceChr = Array("&gt;", "&lt;", "&nbsp;", "&amp;")
        '        'ReplacementChr = Array(">", "<", " ", "&")
        '        ''
        '        'DecodeHTML = ReplaceMany(Source, SourceChr, ReplacementChr)
        '        '
        '    End Function
        '    '
        '    '========================================================================
        '    ' EncodeFilename
        '    '
        '    '========================================================================
        '    '
        '    Function ReplaceMany(Source As String, ArrayOfSource() As Object, ArrayOfReplacement() As Object) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim Count As Integer
        '        Dim Pointer As Integer
        '        '
        '        Count = UBound(ArrayOfSource) + 1
        '        ReplaceMany = Source
        '        For Pointer = 0 To Count - 1
        '            ReplaceMany = vbReplace(ReplaceMany, ArrayOfSource(Pointer), ArrayOfReplacement(Pointer))
        '        Next
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Public Function GetURIHost(URI) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        '   Divide the URI into URIHost, URIPath, and URIPage
        '        '
        '        Dim URIWorking As String
        '        Dim Slash As Integer
        '        Dim LastSlash As Integer
        '        Dim URIHost As String
        '        Dim URIPath As String
        '        Dim URIPage As String
        '        URIWorking = URI
        '        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        '            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        '        End If
        '        URIHost = URIWorking
        '        Slash = vbInstr(1, URIHost, "/")
        '        If Slash = 0 Then
        '            URIPath = "/"
        '            URIPage = ""
        '        Else
        '            URIPath = Mid(URIHost, Slash)
        '            URIHost = Mid(URIHost, 1, Slash - 1)
        '            Slash = vbInstr(1, URIPath, "/")
        '            Do While Slash <> 0
        '                LastSlash = Slash
        '                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        '                '''DoEvents()
        '            Loop
        '            URIPage = Mid(URIPath, LastSlash + 1)
        '            URIPath = Mid(URIPath, 1, LastSlash)
        '        End If
        '        GetURIHost = URIHost
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Public Function GetURIPage(URI) As String
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        '   Divide the URI into URIHost, URIPath, and URIPage
        '        '
        '        Dim Slash As Integer
        '        Dim LastSlash As Integer
        '        Dim URIHost As String
        '        Dim URIPath As String
        '        Dim URIPage As String
        '        Dim URIWorking As String
        '        URIWorking = URI
        '        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        '            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        '        End If
        '        URIHost = URIWorking
        '        Slash = vbInstr(1, URIHost, "/")
        '        If Slash = 0 Then
        '            URIPath = "/"
        '            URIPage = ""
        '        Else
        '            URIPath = Mid(URIHost, Slash)
        '            URIHost = Mid(URIHost, 1, Slash - 1)
        '            Slash = vbInstr(1, URIPath, "/")
        '            Do While Slash <> 0
        '                LastSlash = Slash
        '                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        '                '''DoEvents()
        '            Loop
        '            URIPage = Mid(URIPath, LastSlash + 1)
        '            URIPath = Mid(URIPath, 1, LastSlash)
        '        End If
        '        GetURIPage = URIPage
        '        '
        '    End Function
        '    '
        '    '
        '    '
        '    Function GetDateFromGMT(GMTDate As String) As Date
        '        ' ##### removed to catch err<>0 problem on error resume next
        '        '
        '        Dim WorkString As String
        '        GetDateFromGMT = 0
        '        If GMTDate <> "" Then
        '            WorkString = Mid(GMTDate, 6, 11)
        '            If IsDate(WorkString) Then
        '                GetDateFromGMT = CDate(WorkString)
        '                WorkString = Mid(GMTDate, 18, 8)
        '                If IsDate(WorkString) Then
        '                    GetDateFromGMT = GetDateFromGMT + CDate(WorkString) + 4 / 24
        '                End If
        '            End If
        '        End If
        '        '
        '    End Function
        '
        ' Wdy, DD-Mon-YYYY HH:MM:SS GMT
        '
        Function GetGMTFromDate(ByVal DateValue As Date) As String
            '
            Dim WorkString As String
            Dim WorkLong As Integer
            '
            If IsDate(DateValue) Then
                Select Case Weekday(DateValue)
                    Case vbSunday
                        GetGMTFromDate = "Sun, "
                    Case vbMonday
                        GetGMTFromDate = "Mon, "
                    Case vbTuesday
                        GetGMTFromDate = "Tue, "
                    Case vbWednesday
                        GetGMTFromDate = "Wed, "
                    Case vbThursday
                        GetGMTFromDate = "Thu, "
                    Case vbFriday
                        GetGMTFromDate = "Fri, "
                    Case vbSaturday
                        GetGMTFromDate = "Sat, "
                End Select
                '
                WorkLong = Day(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & " "
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & " "
                End If
                '
                Select Case Month(DateValue)
                    Case 1
                        GetGMTFromDate = GetGMTFromDate & "Jan "
                    Case 2
                        GetGMTFromDate = GetGMTFromDate & "Feb "
                    Case 3
                        GetGMTFromDate = GetGMTFromDate & "Mar "
                    Case 4
                        GetGMTFromDate = GetGMTFromDate & "Apr "
                    Case 5
                        GetGMTFromDate = GetGMTFromDate & "May "
                    Case 6
                        GetGMTFromDate = GetGMTFromDate & "Jun "
                    Case 7
                        GetGMTFromDate = GetGMTFromDate & "Jul "
                    Case 8
                        GetGMTFromDate = GetGMTFromDate & "Aug "
                    Case 9
                        GetGMTFromDate = GetGMTFromDate & "Sep "
                    Case 10
                        GetGMTFromDate = GetGMTFromDate & "Oct "
                    Case 11
                        GetGMTFromDate = GetGMTFromDate & "Nov "
                    Case 12
                        GetGMTFromDate = GetGMTFromDate & "Dec "
                End Select
                '
                GetGMTFromDate = GetGMTFromDate & CStr(Year(DateValue)) & " "
                '
                WorkLong = Hour(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & ":"
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & ":"
                End If
                '
                WorkLong = Minute(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong) & ":"
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong) & ":"
                End If
                '
                WorkLong = Second(DateValue)
                If WorkLong < 10 Then
                    GetGMTFromDate = GetGMTFromDate & "0" & CStr(WorkLong)
                Else
                    GetGMTFromDate = GetGMTFromDate & CStr(WorkLong)
                End If
                '
                GetGMTFromDate = GetGMTFromDate & " GMT"
            End If
            '
        End Function
        ''    '
        ''    '========================================================================
        ''    '   EncodeSQL
        ''    '       encode a variable to go in an sql expression
        ''    '       NOT supported
        ''    '========================================================================
        ''    '
        'Public Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    Dim iFieldType As Integer
        '    Dim MethodName As String
        '    '
        '    MethodName = "EncodeSQL"
        '    '
        '    iFieldType = fieldType
        '    Select Case iFieldType
        '        Case FieldTypeIdBoolean
        '            EncodeSQL = app.EncodeSQLBoolean(expression)
        '        Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
        '            EncodeSQL = app.EncodeSQLNumber(expression)
        '        Case FieldTypeIdDate
        '            EncodeSQL = app.EncodeSQLDate(expression)
        '        Case FieldTypeIdLongText, FieldTypeIdHTML
        '            EncodeSQL = app.EncodeSQLText(expression)
        '        Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
        '            EncodeSQL = app.EncodeSQLText(expression)
        '        Case Else
        '            EncodeSQL = app.EncodeSQLText(expression)
        '            On Error GoTo 0
        '            Call Err.Raise(ignoreInteger, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
        '    End Select
        '    '
        'End Function
        ''
        ''=====================================================================================================
        ''   a value in a name/value pair
        ''=====================================================================================================
        ''
        'Public Sub SetNameValueArrays(ByVal InputName As String, ByVal InputValue As String, ByRef SQLName() As String, ByRef SQLValue() As String, ByRef Index As Integer)
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    SQLName(Index) = InputName
        '    SQLValue(Index) = InputValue
        '    Index = Index + 1
        '    '
        'End Sub
        '    '
        '    '
        '    '
        Public Function GetApplicationStatusMessage(ByVal ApplicationStatus As applicationStatusEnum) As String
            Select Case ApplicationStatus
                Case applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing
                    GetApplicationStatusMessage = "The database for this application was found, but content meta table could not be read."
                Case applicationStatusEnum.ApplicationStatusAppConfigNotValid
                    GetApplicationStatusMessage = "The application configuration file on this front-end server is not valid."
                Case applicationStatusEnum.ApplicationStatusAppConfigNotFound
                    GetApplicationStatusMessage = "The application configuration file was not be found on this front-end server."
                Case applicationStatusEnum.ApplicationStatusNoHostService
                    GetApplicationStatusMessage = "Contensive server not running"
                Case applicationStatusEnum.ApplicationStatusNotFound
                    GetApplicationStatusMessage = "Contensive application not found"
                Case applicationStatusEnum.ApplicationStatusNotEnabled
                    GetApplicationStatusMessage = "Contensive application not running"
                Case applicationStatusEnum.ApplicationStatusReady
                    GetApplicationStatusMessage = "Contensive application running"
                Case applicationStatusEnum.ApplicationStatusLoading
                    GetApplicationStatusMessage = "Contensive application starting"
                Case applicationStatusEnum.ApplicationStatusUpgrading
                    GetApplicationStatusMessage = "Contensive database upgrading"
                Case applicationStatusEnum.ApplicationStatusDbBad
                    GetApplicationStatusMessage = "Error verifying core database records"
                Case applicationStatusEnum.ApplicationStatusDbNotFound
                    GetApplicationStatusMessage = "Error opening application database"
                Case applicationStatusEnum.ApplicationStatusKernelFailure
                    GetApplicationStatusMessage = "Error contacting Contensive kernel services"
                Case applicationStatusEnum.ApplicationStatusLicenseFailure
                    GetApplicationStatusMessage = "Error verifying Contensive site license, see Http://www.Contensive.com/License"
                Case applicationStatusEnum.ApplicationStatusConnectionObjectFailure
                    GetApplicationStatusMessage = "Error creating ODBC Connection object"
                Case applicationStatusEnum.ApplicationStatusConnectionStringFailure
                    GetApplicationStatusMessage = "ODBC Data Source connection failed"
                Case applicationStatusEnum.ApplicationStatusDataSourceFailure
                    GetApplicationStatusMessage = "Error opening default data source"
                Case applicationStatusEnum.ApplicationStatusDuplicateDomains
                    GetApplicationStatusMessage = "Can not determine application because there are multiple applications with domain names that match this site's domain (See Application Manager)"
                Case applicationStatusEnum.ApplicationStatusFailedToInitialize
                    GetApplicationStatusMessage = "Application failed to initialize, see trace log for details"
                    'Case applicationStatusEnum.ApplicationStatusPaused
                    '    GetApplicationStatusMessage = "Contensive application paused"
                Case Else
                    GetApplicationStatusMessage = "Unknown status code [" & ApplicationStatus & "], see trace log for details"
            End Select
        End Function
        '    '
        '    '
        '    '
        '    Public Function GetFormInputSelectNameValue(SelectName As String, NameValueArray() As NameValuePairType) As String
        '        Dim Pointer As Integer
        '        Dim Source() As NameValuePairType
        '        '
        '        Source = NameValueArray
        '        GetFormInputSelectNameValue = "<SELECT name=""" & SelectName & """ Size=""1"">"
        '        For Pointer = 0 To UBound(NameValueArray)
        '            GetFormInputSelectNameValue = GetFormInputSelectNameValue & "<OPTION value=""" & Source(Pointer).Value & """>" & Source(Pointer).Name & "</OPTION>"
        '        Next
        '        GetFormInputSelectNameValue = GetFormInputSelectNameValue & "</SELECT>"
        '    End Function
        '
        '
        '
        Public Function getSpacer(ByVal Width As Integer, ByVal Height As Integer) As String
            getSpacer = "<img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""" & Width & """ height=""" & Height & """ border=""0"">"
        End Function
        '
        '
        '
        Public Function processReplacement(ByVal NameValueLines As Object, ByVal Source As Object) As String
            '
            Dim iNameValueLines As String
            Dim Lines() As String
            Dim LineCnt As Integer
            Dim LinePtr As Integer
            '
            Dim Names() As String
            Dim Values() As String
            Dim PairPtr As Integer
            Dim PairCnt As Integer
            Dim Splits() As String
            '
            ' ----- read pairs in from NameValueLines
            '
            iNameValueLines = EncodeText(NameValueLines)
            If vbInstr(1, iNameValueLines, "=") <> 0 Then
                PairCnt = 0
                Lines = SplitCRLF(iNameValueLines)
                For LinePtr = 0 To UBound(Lines)
                    If vbInstr(1, Lines(LinePtr), "=") <> 0 Then
                        Splits = Split(Lines(LinePtr), "=")
                        ReDim Preserve Names(PairCnt)
                        ReDim Preserve Names(PairCnt)
                        ReDim Preserve Values(PairCnt)
                        Names(PairCnt) = Trim(Splits(0))
                        Names(PairCnt) = vbReplace(Names(PairCnt), vbTab, "")
                        Splits(0) = ""
                        Values(PairCnt) = Trim(Splits(1))
                        PairCnt = PairCnt + 1
                    End If
                Next
            End If
            '
            ' ----- Process replacements on Source
            '
            processReplacement = EncodeText(Source)
            If PairCnt > 0 Then
                For PairPtr = 0 To PairCnt - 1
                    processReplacement = vbReplace(processReplacement, Names(PairPtr), Values(PairPtr), 1, 99, vbTextCompare)
                Next
            End If
            '
        End Function
        '    '
        '    '==========================================================================================================================
        '    '   To convert from site license to server licenses, we still need the URLEncoder in the site license
        '    '   This routine generates a site license that is just the URL encoder.
        '    '==========================================================================================================================
        '    '
        '    Public Function GetURLEncoder() As String
        '        Randomize()
        '        GetURLEncoder = CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1000000000 + (Rnd() * 899999999)))
        '    End Function
        '    '
        '    '==========================================================================================================================
        '    '   To convert from site license to server licenses, we still need the URLEncoder in the site license
        '    '   This routine generates a site license that is just the URL encoder.
        '    '==========================================================================================================================
        '    '
        '    Public Function GetSiteLicenseKey() As String
        '        GetSiteLicenseKey = "00000-00000-00000-" & GetURLEncoder
        '    End Function
        '    '
        '    '
        '    '
        'Public Sub ccAddTabEntry(Caption As String, Link As String, IsHit As Boolean, Optional StylePrefix as string = "", Optional LiveBody as string = "")
        '        On Error GoTo ErrorTrap
        '        '
        '        If TabsCnt <= TabsSize Then
        '            TabsSize = TabsSize + 10
        '            ReDim Preserve Tabs(TabsSize)
        '        End If
        '        With Tabs(TabsCnt)
        '            .Caption = Caption
        '            .Link = Link
        '            .IsHit = IsHit
        '            .StylePrefix = encodeMissingText(StylePrefix, "cc")
        '            .LiveBody = encodeMissingText(LiveBody, "")
        '        End With
        '        TabsCnt = TabsCnt + 1
        '        '
        '        Exit Sub
        '        '
        'ErrorTrap:
        '        Call Err.Raise(Err.Number, Err.Source, "Error in ccAddTabEntry-" & Err.Description)
        '    End Sub
        '    '
        '    '
        '    '
        '    Public Function OldccGetTabs() As String
        '        On Error GoTo ErrorTrap
        '        '
        '        Dim TabPtr As Integer
        '        Dim HitPtr As Integer
        '        Dim IsLiveTab As Boolean
        '        Dim TabBody As String
        '        Dim TabLink As String
        '        Dim TabID As String
        '        Dim FirstLiveBodyShown As Boolean
        '        '
        '        If TabsCnt > 0 Then
        '            HitPtr = 0
        '            '
        '            ' Create TabBar
        '            '
        '            OldccGetTabs = OldccGetTabs & "<table border=0 cellspacing=0 cellpadding=0 align=center ><tr>"
        '            For TabPtr = 0 To TabsCnt - 1
        '                TabID = CStr(GetRandomInteger)
        '                If Tabs(TabPtr).LiveBody = "" Then
        '                    '
        '                    ' This tab is linked to a page
        '                    '
        '                    TabLink = encodeHTML(Tabs(TabPtr).Link)
        '                Else
        '                    '
        '                    ' This tab has a live body
        '                    '
        '                    TabLink = encodeHTML(Tabs(TabPtr).Link)
        '                    If Not FirstLiveBodyShown Then
        '                        FirstLiveBodyShown = True
        '                        TabBody = TabBody & "<div style=""visibility: visible; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
        '                    Else
        '                        TabBody = TabBody & "<div style=""visibility: hidden; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
        '                    End If
        '                End If
        '                OldccGetTabs = OldccGetTabs & "<td valign=bottom>"
        '                If Tabs(TabPtr).IsHit And (HitPtr = 0) Then
        '                    HitPtr = TabPtr
        '                    '
        '                    ' This tab is hit
        '                    '
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<table cellspacing=0 cellPadding=0 border=0>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=2 height=1 width=2></td>" _
        '                        & "<td colspan=1 height=1 bgcolor=black></td>" _
        '                        & "<td colspan=3 height=1 width=3></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=1 height=1 width=1></td>" _
        '                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
        '                        & "<td colspan=1 height=1></td>" _
        '                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
        '                        & "<td colspan=2 height=1 width=2></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=1 height=2 bgcolor=black></td>" _
        '                        & "<td colspan=1 height=2></td>" _
        '                        & "<td colspan=1 height=2></td>" _
        '                        & "<td colspan=1 height=2></td>" _
        '                        & "<td colspan=1 height=2 width=1 bgcolor=black></td>" _
        '                        & "<td colspan=1 height=2 width=1></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td></td>" _
        '                        & "<td>" _
        '                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
        '                        & "<td Class=""ccTabHit"">&nbsp;<a href=""" & TabLink & """ Class=""ccTabHit"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
        '                        & "</tr></table >" _
        '                        & "</td>" _
        '                        & "<td></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td></td>" _
        '                        & "<td></td>" _
        '                        & "<td></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "</tr>" _
        '                        & "</table >"
        '                Else
        '                    '
        '                    ' This tab is not hit
        '                    '
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<table cellspacing=0 cellPadding=0 border=0>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=6 height=1></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=2 height=1></td>" _
        '                        & "<td colspan=1 height=1 bgcolor=black></td>" _
        '                        & "<td colspan=3 height=1></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td width=1></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=2 colspan=2></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=1></td>" _
        '                        & "<td nowrap>" _
        '                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
        '                        & "<td Class=""ccTab"">&nbsp;<a href=""" & TabLink & """ Class=""ccTab"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
        '                        & "</tr></table >" _
        '                        & "</td>" _
        '                        & "<td width=1></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=1></td>" _
        '                        & "</tr>"
        '                    OldccGetTabs = OldccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=6 height=1 bgcolor=black></td>" _
        '                        & "</tr>" _
        '                        & "</table >"
        '                End If
        '                OldccGetTabs = OldccGetTabs & "</td>"
        '            Next
        '            OldccGetTabs = OldccGetTabs & "<td class=""ccTabEnd"">&nbsp;</td></tr>"
        '            If TabBody <> "" Then
        '                OldccGetTabs = OldccGetTabs & "<tr><td colspan=6>" & TabBody & "</td></tr>"
        '            End If
        '            OldccGetTabs = OldccGetTabs & "</tr></table >"
        '            TabsCnt = 0
        '        End If
        '        '
        '        Exit Function
        '        '
        'ErrorTrap:
        '        Call Err.Raise(Err.Number, Err.Source, "Error in OldccGetTabs-" & Err.Description)
        '    End Function


        '    '
        '    '
        '    '
        '    Public Function ccGetTabs() As String
        '        On Error GoTo ErrorTrap
        '        '
        '        Dim TabPtr As Integer
        '        Dim HitPtr As Integer
        '        Dim IsLiveTab As Boolean
        '        Dim TabBody As String
        '        Dim TabLink As String
        '        Dim TabID As String
        '        Dim FirstLiveBodyShown As Boolean
        '        '
        '        If TabsCnt > 0 Then
        '            HitPtr = 0
        '            '
        '            ' Create TabBar
        '            '
        '            ccGetTabs = ccGetTabs & "<table border=0 cellspacing=0 cellpadding=0 align=center ><tr>"
        '            For TabPtr = 0 To TabsCnt - 1
        '                TabID = CStr(GetRandomInteger)
        '                If Tabs(TabPtr).LiveBody = "" Then
        '                    '
        '                    ' This tab is linked to a page
        '                    '
        '                    TabLink = encodeHTML(Tabs(TabPtr).Link)
        '                Else
        '                    '
        '                    ' This tab has a live body
        '                    '
        '                    TabLink = encodeHTML(Tabs(TabPtr).Link)
        '                    If Not FirstLiveBodyShown Then
        '                        FirstLiveBodyShown = True
        '                        TabBody = TabBody & "<div style=""visibility: visible; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """>" & Tabs(TabPtr).LiveBody & "</div>"
        '                    Else
        '                        TabBody = TabBody & "<div style=""visibility: hidden; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """>" & Tabs(TabPtr).LiveBody & "</div>"
        '                    End If
        '                End If
        '                ccGetTabs = ccGetTabs & "<td valign=bottom>"
        '                If Tabs(TabPtr).IsHit And (HitPtr = 0) Then
        '                    HitPtr = TabPtr
        '                    '
        '                    ' This tab is hit
        '                    '
        '                    ccGetTabs = ccGetTabs _
        '                        & "<table cellspacing=0 cellPadding=0 border=0>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=2 height=1 width=2></td>" _
        '                        & "<td colspan=1 height=1 bgcolor=black></td>" _
        '                        & "<td colspan=3 height=1 width=3></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=1 height=1 width=1></td>" _
        '                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
        '                        & "<td Class=""ccTabHit"" colspan=1 height=1></td>" _
        '                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
        '                        & "<td colspan=2 height=1 width=2></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=1 height=2 bgcolor=black></td>" _
        '                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
        '                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
        '                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
        '                        & "<td colspan=1 height=2 bgcolor=black></td>" _
        '                        & "<td colspan=1 height=2></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td Class=""ccTabHit""></td>" _
        '                        & "<td Class=""ccTabHit"">" _
        '                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
        '                        & "<td Class=""ccTabHit"">&nbsp;<a href=""" & TabLink & """ Class=""ccTabHit"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
        '                        & "</tr></table >" _
        '                        & "</td>" _
        '                        & "<td Class=""ccTabHit""></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td Class=""ccTabHit""></td>" _
        '                        & "<td Class=""ccTabHit""></td>" _
        '                        & "<td Class=""ccTabHit""></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "<td bgcolor=black></td>" _
        '                        & "</tr>" _
        '                        & "</table >"
        '                Else
        '                    '
        '                    ' This tab is not hit
        '                    '
        '                    ccGetTabs = ccGetTabs _
        '                        & "<table cellspacing=0 cellPadding=0 border=0>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=6 height=1></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=2 height=1></td>" _
        '                        & "<td colspan=1 height=1 bgcolor=black></td>" _
        '                        & "<td colspan=3 height=1></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td width=1></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td Class=""ccTab""></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=2 colspan=2></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=1 Class=""ccTab""></td>" _
        '                        & "<td nowrap Class=""ccTab"">" _
        '                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
        '                        & "<td Class=""ccTab"">&nbsp;<a href=""" & TabLink & """ Class=""ccTab"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
        '                        & "</tr></table >" _
        '                        & "</td>" _
        '                        & "<td width=1 Class=""ccTab""></td>" _
        '                        & "<td width=1 bgcolor=black></td>" _
        '                        & "<td width=1></td>" _
        '                        & "</tr>"
        '                    ccGetTabs = ccGetTabs _
        '                        & "<tr>" _
        '                        & "<td colspan=6 height=1 bgcolor=black></td>" _
        '                        & "</tr>" _
        '                        & "</table >"
        '                End If
        '                ccGetTabs = ccGetTabs & "</td>"
        '            Next
        '            ccGetTabs = ccGetTabs & "<td class=""ccTabEnd"">&nbsp;</td></tr>"
        '            If TabBody <> "" Then
        '                ccGetTabs = ccGetTabs & "<tr><td colspan=6>" & TabBody & "</td></tr>"
        '            End If
        '            ccGetTabs = ccGetTabs & "</tr></table >"
        '            TabsCnt = 0
        '        End If
        '        '
        '        Exit Function
        '        '
        'ErrorTrap:
        '        Call Err.Raise(Err.Number, Err.Source, "Error in ccGetTabs-" & Err.Description)
        '    End Function
        '
        '
        '
        Public Function ConvertLinksToAbsolute(ByVal Source As String, ByVal RootLink As String) As String
            On Error GoTo ErrorTrap
            '
            Dim s As String
            '
            s = Source
            '
            s = vbReplace(s, " href=""", " href=""/", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=""/http", " href=""http", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=""/mailto", " href=""mailto", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=""//", " href=""" & RootLink, 1, 99, vbTextCompare)
            s = vbReplace(s, " href=""/?", " href=""" & RootLink & "?", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=""/", " href=""" & RootLink, 1, 99, vbTextCompare)
            '
            s = vbReplace(s, " href=", " href=/", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=/""", " href=""", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=/http", " href=http", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=//", " href=" & RootLink, 1, 99, vbTextCompare)
            s = vbReplace(s, " href=/?", " href=" & RootLink & "?", 1, 99, vbTextCompare)
            s = vbReplace(s, " href=/", " href=" & RootLink, 1, 99, vbTextCompare)
            '
            s = vbReplace(s, " src=""", " src=""/", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=""/http", " src=""http", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=""//", " src=""" & RootLink, 1, 99, vbTextCompare)
            s = vbReplace(s, " src=""/?", " src=""" & RootLink & "?", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=""/", " src=""" & RootLink, 1, 99, vbTextCompare)
            '
            s = vbReplace(s, " src=", " src=/", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=/""", " src=""", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=/http", " src=http", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=//", " src=" & RootLink, 1, 99, vbTextCompare)
            s = vbReplace(s, " src=/?", " src=" & RootLink & "?", 1, 99, vbTextCompare)
            s = vbReplace(s, " src=/", " src=" & RootLink, 1, 99, vbTextCompare)
            '
            ConvertLinksToAbsolute = s
            '
            Exit Function
            '
ErrorTrap:
            Call Err.Raise(Err.Number, Err.Source, "Error in ConvertLinksToAbsolute-" & Err.Description)
        End Function
        '    '
        '    '
        '    '
        'Public Function getAppPath() As String
        '    Dim Ptr As Integer
        '    getAppPath = App.Path
        '    Ptr = vbInstr(1, getAppPath, "\github\", vbTextCompare)
        '    If Ptr <> 0 Then
        '        ' for ...\github\contensive4?\bin"
        '        Ptr = vbInstr(Ptr + 8, getAppPath, "\")
        '        getAppPath = Left(getAppPath, Ptr) & "bin"
        '    End If
        'End Function
        ''
        ''
        ''
        'Public Function GetAddonRootPath() As String
        '    Dim testPath As String
        '    '
        '    GetAddonRootPath = getAppPath & "\addons"
        '    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        '        '
        '        ' debugging - change program path to dummy path so addon builds all copy to
        '        '
        '        testPath = Environ$("programfiles(x86)")
        '        If testPath = "" Then
        '            testPath = Environ$("programfiles")
        '        End If
        '        GetAddonRootPath = testPath & "\kma\contensive\addons"
        '    End If
        'End Function
        '
        '
        '
        Public Function GetHTMLComment(ByVal Comment As String) As String
            GetHTMLComment = "<!-- " & Comment & " -->"
        End Function
        '
        '
        '
        Public Function SplitCRLF(ByVal Expression As String) As String()
            Dim Args() As String
            Dim Ptr As Integer
            '
            If vbInstr(1, Expression, vbCrLf) <> 0 Then
                SplitCRLF = Split(Expression, vbCrLf, , vbTextCompare)
            ElseIf vbInstr(1, Expression, vbCr) <> 0 Then
                SplitCRLF = Split(Expression, vbCr, , vbTextCompare)
            ElseIf vbInstr(1, Expression, vbLf) <> 0 Then
                SplitCRLF = Split(Expression, vbLf, , vbTextCompare)
            Else
                ReDim SplitCRLF(0)
                SplitCRLF = Split(Expression, vbCrLf)
            End If
        End Function
        '    '
        '    '
        '    '
        'Public Sub runProcess(cp.core,Cmd As String, Optional ByVal eWindowStyle As VBA.VbAppWinStyle = vbHide, Optional WaitForReturn As Boolean)
        '        On Error GoTo ErrorTrap
        '        '
        '        Dim ShellObj As Object
        '        '
        '        ShellObj = CreateObject("WScript.Shell")
        '        If Not (ShellObj Is Nothing) Then
        '            Call ShellObj.Run(Cmd, 0, WaitForReturn)
        '        End If
        '        ShellObj = Nothing
        '        Exit Sub
        '        '
        'ErrorTrap:
        '        Call AppendLogFile("ErrorTrap, runProcess running command [" & Cmd & "], WaitForReturn=" & WaitForReturn & ", err=" & GetErrString(Err))
        '    End Sub
        '
        '------------------------------------------------------------------------------------------------------------
        '   Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        '       call this before parsing them together
        '       call decodeAddonConstructorArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function EncodeAddonConstructorArgument(ByVal Arg As String) As String
            Dim a As String
            If Arg <> "" Then
                a = Arg
                If True Then
                    'If AddonNewParse Then
                    a = vbReplace(a, "\", "\\")
                    a = vbReplace(a, vbCrLf, "\n")
                    a = vbReplace(a, vbTab, "\t")
                    a = vbReplace(a, "&", "\&")
                    a = vbReplace(a, "=", "\=")
                    a = vbReplace(a, ",", "\,")
                    a = vbReplace(a, """", "\""")
                    a = vbReplace(a, "'", "\'")
                    a = vbReplace(a, "|", "\|")
                    a = vbReplace(a, "[", "\[")
                    a = vbReplace(a, "]", "\]")
                    a = vbReplace(a, ":", "\:")
                End If
                EncodeAddonConstructorArgument = a
            End If
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   Decodes an argument parsed from an AddonConstructorString for all non-allowed characters
        '       AddonConstructorString is a & delimited string of name=value[selector]descriptor
        '
        '       to get a value from an AddonConstructorString, first use getargument() to get the correct value[selector]descriptor
        '       then remove everything to the right of any '['
        '
        '       call encodeAddonConstructorargument before parsing them together
        '       call decodeAddonConstructorArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function DecodeAddonConstructorArgument(ByVal EncodedArg As String) As String
            Dim a As String
            Dim Pos As Integer
            '
            a = EncodedArg
            If True Then
                'If AddonNewParse Then
                a = vbReplace(a, "\:", ":")
                a = vbReplace(a, "\]", "]")
                a = vbReplace(a, "\[", "[")
                a = vbReplace(a, "\|", "|")
                a = vbReplace(a, "\'", "'")
                a = vbReplace(a, "\""", """")
                a = vbReplace(a, "\,", ",")
                a = vbReplace(a, "\=", "=")
                a = vbReplace(a, "\&", "&")
                a = vbReplace(a, "\t", vbTab)
                a = vbReplace(a, "\n", vbCrLf)
                a = vbReplace(a, "\\", "\")
            End If
            DecodeAddonConstructorArgument = a
        End Function
        '    '
        '    '------------------------------------------------------------------------------------------------------------
        '    '   use only internally
        '    '
        '    '   encode an argument to be used in a name=value& (N-V-A) string
        '    '
        '    '   an argument can be any one of these is this format:
        '    '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
        '    '
        '    '   to create an nva string
        '    '       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
        '    '
        '    '   to decode an nva string
        '    '       split on ampersand then on equal, and decodeNvaArgument() each part
        '    '
        '    '------------------------------------------------------------------------------------------------------------
        '    '
        '    Public Function encodeNvaArgument(Arg As String) As String
        '        Dim a As String
        '        a = Arg
        '        If a <> "" Then
        '            a = vbReplace(a, vbCrLf, "#0013#")
        '            a = vbReplace(a, vbLf, "#0013#")
        '            a = vbReplace(a, vbCr, "#0013#")
        '            a = vbReplace(a, "&", "#0038#")
        '            a = vbReplace(a, "=", "#0061#")
        '            a = vbReplace(a, ",", "#0044#")
        '            a = vbReplace(a, """", "#0034#")
        '            a = vbReplace(a, "'", "#0039#")
        '            a = vbReplace(a, "|", "#0124#")
        '            a = vbReplace(a, "[", "#0091#")
        '            a = vbReplace(a, "]", "#0093#")
        '            a = vbReplace(a, ":", "#0058#")
        '        End If
        '        encodeNvaArgument = a
        '    End Function
        '    '
        '    '------------------------------------------------------------------------------------------------------------
        '    '   use only internally
        '    '       decode an argument removed from a name=value& string
        '    '       see encodeNvaArgument for details on how to use this
        '    '------------------------------------------------------------------------------------------------------------
        '    '
        '    Public Function decodeNvaArgument(EncodedArg As String) As String
        '        Dim a As String
        '        '
        '        a = EncodedArg
        '        a = vbReplace(a, "#0058#", ":")
        '        a = vbReplace(a, "#0093#", "]")
        '        a = vbReplace(a, "#0091#", "[")
        '        a = vbReplace(a, "#0124#", "|")
        '        a = vbReplace(a, "#0039#", "'")
        '        a = vbReplace(a, "#0034#", """")
        '        a = vbReplace(a, "#0044#", ",")
        '        a = vbReplace(a, "#0061#", "=")
        '        a = vbReplace(a, "#0038#", "&")
        '        a = vbReplace(a, "#0013#", vbCrLf)
        '        decodeNvaArgument = a
        '    End Function
        '    '
        '    ' returns true of the link is a valid link on the source host
        '    '
        Public Function IsLinkToThisHost(ByVal Host As String, ByVal Link As String) As Boolean
            '
            Dim LinkHost As String
            Dim Pos As Integer
            '
            If Trim(Link) = "" Then
                '
                ' Blank is not a link
                '
                IsLinkToThisHost = False
            ElseIf vbInstr(1, Link, "://") <> 0 Then
                '
                ' includes protocol, may be link to another site
                '
                LinkHost = vbLCase(Link)
                Pos = 1
                Pos = vbInstr(Pos, LinkHost, "://")
                If Pos > 0 Then
                    Pos = vbInstr(Pos + 3, LinkHost, "/")
                    If Pos > 0 Then
                        LinkHost = Mid(LinkHost, 1, Pos - 1)
                    End If
                    IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                    If Not IsLinkToThisHost Then
                        '
                        ' try combinations including/excluding www.
                        '
                        If vbInstr(1, LinkHost, "www.", vbTextCompare) <> 0 Then
                            '
                            ' remove it
                            '
                            LinkHost = vbReplace(LinkHost, "www.", "", 1, 99, vbTextCompare)
                            IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                        Else
                            '
                            ' add it
                            '
                            LinkHost = vbReplace(LinkHost, "://", "://www.", 1, 99, vbTextCompare)
                            IsLinkToThisHost = (vbLCase(Host) = LinkHost)
                        End If
                    End If
                End If
            ElseIf vbInstr(1, Link, "#") = 1 Then
                '
                ' Is a bookmark, not a link
                '
                IsLinkToThisHost = False
            Else
                '
                ' all others are links on the source
                '
                IsLinkToThisHost = True
            End If
            If Not IsLinkToThisHost Then
                Link = Link
            End If
        End Function
        '
        '========================================================================================================
        '   ConvertLinkToRootRelative
        '
        '   /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
        '   http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
        '   images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
        '   logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
        '
        '========================================================================================================
        '
        Public Function ConvertLinkToRootRelative(ByVal Link As String, ByVal BasePath As String) As String
            '
            Dim Pos As Integer
            '
            ConvertLinkToRootRelative = Link
            If vbInstr(1, Link, "/") = 1 Then
                '
                '   case /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
                '
            ElseIf vbInstr(1, Link, "://") <> 0 Then
                '
                '   case http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
                '
                Pos = vbInstr(1, Link, "://")
                If Pos > 0 Then
                    Pos = vbInstr(Pos + 3, Link, "/")
                    If Pos > 0 Then
                        ConvertLinkToRootRelative = Mid(Link, Pos)
                    Else
                        '
                        ' This is just the domain name, RootRelative is the root
                        '
                        ConvertLinkToRootRelative = "/"
                    End If
                End If
            Else
                '
                '   case images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
                '   case logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
                '
                ConvertLinkToRootRelative = BasePath & Link
            End If
            '
        End Function
        '
        '
        '
        Public Function GetAddonIconImg(ByVal AdminURL As String, ByVal IconWidth As Integer, ByVal IconHeight As Integer, ByVal IconSprites As Integer, ByVal IconIsInline As Boolean, ByVal IconImgID As String, ByVal IconFilename As String, ByVal serverFilePath As String, ByVal IconAlt As String, ByVal IconTitle As String, ByVal ACInstanceID As String, ByVal IconSpriteColumn As Integer) As String
            '
            Dim ImgStyle As String
            Dim IconHeightNumeric As Integer
            '
            If IconAlt = "" Then
                IconAlt = "Add-on"
            End If
            If IconTitle = "" Then
                IconTitle = "Rendered as Add-on"
            End If
            If IconFilename = "" Then
                '
                ' No icon given, use the default
                '
                If IconIsInline Then
                    IconFilename = "/ccLib/images/IconAddonInlineDefault.png"
                    IconWidth = 62
                    IconHeight = 17
                    IconSprites = 0
                Else
                    IconFilename = "/ccLib/images/IconAddonBlockDefault.png"
                    IconWidth = 57
                    IconHeight = 59
                    IconSprites = 4
                End If
            ElseIf vbInstr(1, IconFilename, "://") <> 0 Then
                '
                ' icon is an Absolute URL - leave it
                '
            ElseIf Left(IconFilename, 1) = "/" Then
                '
                ' icon is Root Relative, leave it
                '
            Else
                '
                ' icon is a virtual file, add the serverfilepath
                '
                IconFilename = serverFilePath & IconFilename
            End If
            'IconFilename = encodeJavascript(IconFilename)
            If (IconWidth = 0) Or (IconHeight = 0) Then
                IconSprites = 0
            End If

            If IconSprites = 0 Then
                '
                ' just the icon
                '
                GetAddonIconImg = "<img" _
                    & " border=0" _
                    & " id=""" & IconImgID & """" _
                    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');""" _
                    & " alt=""" & IconAlt & """" _
                    & " title=""" & IconTitle & """" _
                    & " src=""" & IconFilename & """"
                'GetAddonIconImg = "<img" _
                '    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
                '    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
                '    & " alt=""" & IconAlt & """" _
                '    & " title=""" & IconTitle & """" _
                '    & " src=""" & IconFilename & """"
                If IconWidth <> 0 Then
                    GetAddonIconImg = GetAddonIconImg & " width=""" & IconWidth & "px"""
                End If
                If IconHeight <> 0 Then
                    GetAddonIconImg = GetAddonIconImg & " height=""" & IconHeight & "px"""
                End If
                If IconIsInline Then
                    GetAddonIconImg = GetAddonIconImg & " style=""vertical-align:middle;display:inline;"" "
                Else
                    GetAddonIconImg = GetAddonIconImg & " style=""display:block"" "
                End If
                If ACInstanceID <> "" Then
                    GetAddonIconImg = GetAddonIconImg & " ACInstanceID=""" & ACInstanceID & """"
                End If
                GetAddonIconImg = GetAddonIconImg & ">"
            Else
                '
                ' Sprite Icon
                '
                GetAddonIconImg = GetIconSprite(IconImgID, IconSpriteColumn, IconFilename, IconWidth, IconHeight, IconAlt, IconTitle, "window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');", IconIsInline, ACInstanceID)
                '        GetAddonIconImg = "<img" _
                '            & " border=0" _
                '            & " id=""" & IconImgID & """" _
                '            & " onMouseOver=""this.style.backgroundPosition='" & (-1 * IconSpriteColumn * IconWidth) & "px -" & (2 * IconHeight) & "px'""" _
                '            & " onMouseOut=""this.style.backgroundPosition='" & (-1 * IconSpriteColumn * IconWidth) & "px 0px'""" _
                '            & " onDblClick=""window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');""" _
                '            & " alt=""" & IconAlt & """" _
                '            & " title=""" & IconTitle & """" _
                '            & " src=""/ccLib/images/spacer.gif"""
                '        ImgStyle = "background:url(" & IconFilename & ") " & (-1 * IconSpriteColumn * IconWidth) & "px 0px no-repeat;"
                '        ImgStyle = ImgStyle & "width:" & IconWidth & "px;"
                '        ImgStyle = ImgStyle & "height:" & IconHeight & "px;"
                '        If IconIsInline Then
                '            'GetAddonIconImg = GetAddonIconImg & " align=""middle"""
                '            ImgStyle = ImgStyle & "vertical-align:middle;display:inline;"
                '        Else
                '            ImgStyle = ImgStyle & "display:block;"
                '        End If
                '
                '
                '        'Return_IconStyleMenuEntries = Return_IconStyleMenuEntries & vbCrLf & ",["".icon" & AddonID & """,false,"".icon" & AddonID & """,""background:url(" & IconFilename & ") 0px 0px no-repeat;"
                '        'GetAddonIconImg = "<img" _
                '        '    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
                '        '    & " onMouseOver=""this.style.backgroundPosition=\'0px -" & (2 * IconHeight) & "px\'""" _
                '        '    & " onMouseOut=""this.style.backgroundPosition=\'0px 0px\'""" _
                '        '    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
                '        '    & " alt=""" & IconAlt & """" _
                '        '    & " title=""" & IconTitle & """" _
                '        '    & " src=""/ccLib/images/spacer.gif"""
                '        If ACInstanceID <> "" Then
                '            GetAddonIconImg = GetAddonIconImg & " ACInstanceID=""" & ACInstanceID & """"
                '        End If
                '        GetAddonIconImg = GetAddonIconImg & " style=""" & ImgStyle & """>"
                '        'Return_IconStyleMenuEntries = Return_IconStyleMenuEntries & """]"
            End If
        End Function
        '
        '
        '
        Public Function ConvertRSTypeToGoogleType(ByVal RecordFieldType As Integer) As String
            Select Case RecordFieldType
                Case 2, 3, 4, 5, 6, 14, 16, 17, 18, 19, 20, 21, 131
                    ConvertRSTypeToGoogleType = "number"
                Case Else
                    ConvertRSTypeToGoogleType = "string"
            End Select
        End Function
        '    '
        '    '
        '    '
        Public Function GetIconSprite(ByVal TagID As String, ByVal SpriteColumn As Integer, ByVal IconSrc As String, ByVal IconWidth As Integer, ByVal IconHeight As Integer, ByVal IconAlt As String, ByVal IconTitle As String, ByVal onDblClick As String, ByVal IconIsInline As Boolean, ByVal ACInstanceID As String) As String
            '
            Dim ImgStyle As String
            '
            GetIconSprite = "<img" _
                & " border=0" _
                & " id=""" & TagID & """" _
                & " onMouseOver=""this.style.backgroundPosition='" & (-1 * SpriteColumn * IconWidth) & "px -" & (2 * IconHeight) & "px';""" _
                & " onMouseOut=""this.style.backgroundPosition='" & (-1 * SpriteColumn * IconWidth) & "px 0px'""" _
                & " onDblClick=""" & onDblClick & """" _
                & " alt=""" & IconAlt & """" _
                & " title=""" & IconTitle & """" _
                & " src=""/ccLib/images/spacer.gif"""
            ImgStyle = "background:url(" & IconSrc & ") " & (-1 * SpriteColumn * IconWidth) & "px 0px no-repeat;"
            ImgStyle = ImgStyle & "width:" & IconWidth & "px;"
            ImgStyle = ImgStyle & "height:" & IconHeight & "px;"
            If IconIsInline Then
                ImgStyle = ImgStyle & "vertical-align:middle;display:inline;"
            Else
                ImgStyle = ImgStyle & "display:block;"
            End If
            If ACInstanceID <> "" Then
                GetIconSprite = GetIconSprite & " ACInstanceID=""" & ACInstanceID & """"
            End If
            GetIconSprite = GetIconSprite & " style=""" & ImgStyle & """>"
        End Function
        '
        '================================================================================================================
        '   Separate a URL into its host, path, page parts
        '================================================================================================================
        '
        Public Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            '
            '   Divide the URL into URLHost, URLPath, and URLPage
            '
            Dim WorkingURL As String
            Dim Position As Integer
            '
            ' Get Protocol (before the first :)
            '
            WorkingURL = SourceURL
            Position = vbInstr(1, WorkingURL, ":")
            'Position = vbInstr(1, WorkingURL, "://")
            If Position <> 0 Then
                Protocol = Mid(WorkingURL, 1, Position + 2)
                WorkingURL = Mid(WorkingURL, Position + 3)
            End If
            '
            ' compatibility fix
            '
            If vbInstr(1, WorkingURL, "//") = 1 Then
                If Protocol = "" Then
                    Protocol = "http:"
                End If
                Protocol = Protocol & "//"
                WorkingURL = Mid(WorkingURL, 3)
            End If
            '
            ' Get QueryString
            '
            Position = vbInstr(1, WorkingURL, "?")
            If Position > 0 Then
                QueryString = Mid(WorkingURL, Position)
                WorkingURL = Mid(WorkingURL, 1, Position - 1)
            End If
            '
            ' separate host from pathpage
            '
            'iURLHost = WorkingURL
            Position = vbInstr(WorkingURL, "/")
            If (Position = 0) And (Protocol = "") Then
                '
                ' Page without path or host
                '
                Page = WorkingURL
                Path = ""
                Host = ""
            ElseIf (Position = 0) Then
                '
                ' host, without path or page
                '
                Page = ""
                Path = "/"
                Host = WorkingURL
            Else
                '
                ' host with a path (at least)
                '
                Path = Mid(WorkingURL, Position)
                Host = Mid(WorkingURL, 1, Position - 1)
                '
                ' separate page from path
                '
                Position = InStrRev(Path, "/")
                If Position = 0 Then
                    '
                    ' no path, just a page
                    '
                    Page = Path
                    Path = "/"
                Else
                    Page = Mid(Path, Position + 1)
                    Path = Mid(Path, 1, Position)
                End If
            End If
        End Sub
        '
        '================================================================================================================
        '   Separate a URL into its host, path, page parts
        '================================================================================================================
        '
        Public Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
            '
            '   Divide the URL into URLHost, URLPath, and URLPage
            '
            Dim iURLWorking As String = ""
            Dim iURLProtocol As String = ""
            Dim iURLHost As String = ""
            Dim iURLPort As String = ""
            Dim iURLPath As String = ""
            Dim iURLPage As String = ""
            Dim iURLQueryString As String = ""
            Dim Position As Integer = 0
            '
            iURLWorking = SourceURL
            Position = vbInstr(1, iURLWorking, "://")
            If Position <> 0 Then
                iURLProtocol = Mid(iURLWorking, 1, Position + 2)
                iURLWorking = Mid(iURLWorking, Position + 3)
            End If
            '
            ' separate Host:Port from pathpage
            '
            iURLHost = iURLWorking
            Position = vbInstr(iURLHost, "/")
            If Position = 0 Then
                '
                ' just host, no path or page
                '
                iURLPath = "/"
                iURLPage = ""
            Else
                iURLPath = Mid(iURLHost, Position)
                iURLHost = Mid(iURLHost, 1, Position - 1)
                '
                ' separate page from path
                '
                Position = InStrRev(iURLPath, "/")
                If Position = 0 Then
                    '
                    ' no path, just a page
                    '
                    iURLPage = iURLPath
                    iURLPath = "/"
                Else
                    iURLPage = Mid(iURLPath, Position + 1)
                    iURLPath = Mid(iURLPath, 1, Position)
                End If
            End If
            '
            ' Divide Host from Port
            '
            Position = vbInstr(iURLHost, ":")
            If Position = 0 Then
                '
                ' host not given, take a guess
                '
                Select Case vbUCase(iURLProtocol)
                    Case "FTP://"
                        iURLPort = "21"
                    Case "HTTP://", "HTTPS://"
                        iURLPort = "80"
                    Case Else
                        iURLPort = "80"
                End Select
            Else
                iURLPort = Mid(iURLHost, Position + 1)
                iURLHost = Mid(iURLHost, 1, Position - 1)
            End If
            Position = vbInstr(1, iURLPage, "?")
            If Position > 0 Then
                iURLQueryString = Mid(iURLPage, Position)
                iURLPage = Mid(iURLPage, 1, Position - 1)
            End If
            Protocol = iURLProtocol
            Host = iURLHost
            Port = iURLPort
            Path = iURLPath
            Page = iURLPage
            QueryString = iURLQueryString
        End Sub
        '
        '
        '
        Public Function DecodeGMTDate(ByVal GMTDate As String) As Date
            '
            Dim YearPart As Double
            Dim HourPart As Double
            '
            DecodeGMTDate = #12:00:00 AM#
            If GMTDate <> "" Then
                HourPart = EncodeNumber(Mid(GMTDate, 6, 11))
                If IsDate(HourPart) Then
                    YearPart = EncodeNumber(Mid(GMTDate, 18, 8))
                    If IsDate(YearPart) Then
                        DecodeGMTDate = Date.FromOADate(YearPart + (HourPart + 4) / 24)
                    End If
                End If
            End If
        End Function
        ''
        ''
        ''
        'Public Function  EncodeGMTDate(ByVal MSDate As Date) As String
        '    EncodeGMTDate = ""
        'End Function
        ''
        '=================================================================================
        ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '   There can be no extra spaces between the delimiter, the name and the "="
        '=================================================================================
        '
        Public Function GetArgument(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "", Optional ByVal Delimiter As String = "") As String
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            '
            ' determine delimiter
            '
            If Delimiter = "" Then
                '
                ' If not explicit
                '
                If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
                    '
                    ' crlf can only be here if it is the delimiter
                    '
                    Delimiter = vbCrLf
                Else
                    '
                    ' either only one option, or it is the legacy '&' delimit
                    '
                    Delimiter = "&"
                End If
            End If
            iDefaultValue = DefaultValue
            WorkingString = ArgumentString
            GetArgument = iDefaultValue
            If WorkingString <> "" Then
                WorkingString = Delimiter & WorkingString & Delimiter
                ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
                If ValueStart <> 0 Then
                    NameLength = Len(Name)
                    ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
                    If Mid(WorkingString, ValueStart, 1) = """" Then
                        IsQuoted = True
                        ValueStart = ValueStart + 1
                    End If
                    If IsQuoted Then
                        ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
                    Else
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
                    End If
                    If ValueEnd = 0 Then
                        GetArgument = Mid(WorkingString, ValueStart)
                    Else
                        GetArgument = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
                    End If
                End If
            End If
            '

            Exit Function
            '
            ' ----- ErrorTrap
            '
ErrorTrap:
        End Function
        ''
        ''=================================================================================
        ''   Return the value from a name value pair, parsed with =,&[|].
        ''   For example:
        ''       name=Jay[Jay|Josh|Dwayne]
        ''       the answer is Jay. If a select box is displayed, it is a dropdown of all three
        ''=================================================================================
        ''
        'Public Function  GetAggrOption(ByVal Name As String, ByVal SegmentCMDArgs As String) As String
        '    '
        '    Dim Pos As Integer
        '    '
        '    GetAggrOption = GetArgument(Name, SegmentCMDArgs)
        '    '
        '    ' remove the manual select list syntax "answer[choice1|choice2]"
        '    '
        '    Pos = vbInstr(1, GetAggrOption, "[")
        '    If Pos <> 0 Then
        '        GetAggrOption = Left(GetAggrOption, Pos - 1)
        '    End If
        '    '
        '    ' remove any function syntax "answer{selectcontentname RSS Feeds}"
        '    '
        '    Pos = vbInstr(1, GetAggrOption, "{")
        '    If Pos <> 0 Then
        '        GetAggrOption = Left(GetAggrOption, Pos - 1)
        '    End If
        '    '
        'End Function
        ''
        ''=================================================================================
        ''   Compatibility for GetArgument
        ''=================================================================================
        ''
        'Public Function  GetNameValue(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "") As String
        '    getNameValue = GetArgument(Name, ArgumentString, DefaultValue)
        'End Function
        ''
        ''========================================================================
        ''   Gets the next line from a string, and removes the line
        ''========================================================================
        ''
        'Public Function GetLine(ByVal Body As String) As String
        '    Dim EOL As String
        '    Dim NextCR As Integer
        '    Dim NextLF As Integer
        '    Dim BOL As Integer
        '    '
        '    NextCR = vbInstr(1, Body, vbCr)
        '    NextLF = vbInstr(1, Body, vbLf)

        '    If NextCR <> 0 Or NextLF <> 0 Then
        '        If NextCR <> 0 Then
        '            If NextLF <> 0 Then
        '                If NextCR < NextLF Then
        '                    EOL = NextCR - 1
        '                    If NextLF = NextCR + 1 Then
        '                        BOL = NextLF + 1
        '                    Else
        '                        BOL = NextCR + 1
        '                    End If

        '                Else
        '                    EOL = NextLF - 1
        '                    BOL = NextLF + 1
        '                End If
        '            Else
        '                EOL = NextCR - 1
        '                BOL = NextCR + 1
        '            End If
        '        Else
        '            EOL = NextLF - 1
        '            BOL = NextLF + 1
        '        End If
        '        GetLine = Mid(Body, 1, EOL)
        '        Body = Mid(Body, BOL)
        '    Else
        '        GetLine = Body
        '        Body = ""
        '    End If
        'End Function
        '
        '=================================================================================
        '   Get a Random Long Value
        '=================================================================================
        '
        Public Function GetRandomInteger() As Integer
            '
            Dim RandomBase As Integer
            Dim RandomLimit As Integer
            '
            RandomBase = CInt((2 ^ 30) - 1)
            RandomLimit = CInt((2 ^ 31) - RandomBase - 1)
            Randomize()
            GetRandomInteger = CInt(RandomBase + (Rnd() * RandomLimit))
            '
        End Function
        '
        '=================================================================================
        ' fix for isDataTableOk
        '=================================================================================
        '
        Public Function isDataTableOk(ByVal dt As DataTable) As Boolean
            Return (dt.Rows.Count > 0)
        End Function
        '
        '=================================================================================
        ' fix for closeRS
        '=================================================================================
        '
        Public Sub closeDataTable(ByVal dt As DataTable)
            ' nothing needed
            'dt.Clear()
            dt.Dispose()
        End Sub
        ''
        ''=============================================================================
        '' Create the part of the sql where clause that is modified by the user
        ''   WorkingQuery is the original querystring to change
        ''   QueryName is the name part of the name pair to change
        ''   If the QueryName is not found in the string, ignore call
        ''=============================================================================
        ''
        'Public Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
        '    '
        '    If vbInstr(1, WorkingQuery, "?") Then
        '        ModifyQueryString = ModifyLinkQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
        '    Else
        '        ModifyQueryString = Mid(ModifyLinkQueryString("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
        '    End If
        'End Function
        '
        '=============================================================================
        '   Modify a querystring name/value pair in a Link
        '=============================================================================
        '
        Public Function ModifyLinkQueryString(ByVal Link As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
            '
            Dim Element() As String = Split("", ",")
            Dim ElementCount As Integer
            Dim ElementPointer As Integer
            Dim NameValue() As String
            Dim UcaseQueryName As String
            Dim ElementFound As Boolean
            Dim QueryString As String
            '
            If vbInstr(1, Link, "?") <> 0 Then
                ModifyLinkQueryString = Mid(Link, 1, vbInstr(1, Link, "?") - 1)
                QueryString = Mid(Link, Len(ModifyLinkQueryString) + 2)
            Else
                ModifyLinkQueryString = Link
                QueryString = ""
            End If
            UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName))
            If QueryString <> "" Then
                Element = Split(QueryString, "&")
                ElementCount = UBound(Element) + 1
                For ElementPointer = 0 To ElementCount - 1
                    NameValue = Split(Element(ElementPointer), "=")
                    If UBound(NameValue) = 1 Then
                        If vbUCase(NameValue(0)) = UcaseQueryName Then
                            If QueryValue = "" Then
                                Element(ElementPointer) = ""
                            Else
                                Element(ElementPointer) = QueryName & "=" & QueryValue
                            End If
                            ElementFound = True
                            Exit For
                        End If
                    End If
                Next
            End If
            If Not ElementFound And (QueryValue <> "") Then
                '
                ' element not found, it needs to be added
                '
                If AddIfMissing Then
                    If QueryString = "" Then
                        QueryString = EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                    Else
                        QueryString = QueryString & "&" & EncodeRequestVariable(QueryName) & "=" & EncodeRequestVariable(QueryValue)
                    End If
                End If
            Else
                '
                ' element found
                '
                QueryString = Join(Element, "&")
                If (QueryString <> "") And (QueryValue = "") Then
                    '
                    ' element found and needs to be removed
                    '
                    QueryString = vbReplace(QueryString, "&&", "&")
                    If Left(QueryString, 1) = "&" Then
                        QueryString = Mid(QueryString, 2)
                    End If
                    If Right(QueryString, 1) = "&" Then
                        QueryString = Mid(QueryString, 1, Len(QueryString) - 1)
                    End If
                End If
            End If
            If (QueryString <> "") Then
                ModifyLinkQueryString = ModifyLinkQueryString & "?" & QueryString
            End If
        End Function
        '
        '=================================================================================
        '
        '=================================================================================
        '
        Public Function GetIntegerString(ByVal Value As Integer, ByVal DigitCount As Integer) As String
            If Len(Value) <= DigitCount Then
                GetIntegerString = New String("0"c, DigitCount - Len(CStr(Value))) & CStr(Value)
            Else
                GetIntegerString = CStr(Value)
            End If
        End Function
        ''
        ''==========================================================================================
        ''   the current process to a high priority
        ''       Should be called once from the objects parent when it is first created.
        ''
        ''   taken from an example labeled
        ''       KPD-Team 2000
        ''       URL: http://www.allapi.net/
        ''       Email: KPDTeam@Allapi.net
        ''==========================================================================================
        ''
        'Public sub SetProcessHighPriority()
        '    Dim hProcess As Integer
        '    '
        '    'set the new priority class
        '    '
        '    hProcess = GetCurrentProcess
        '    Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
        '    '
        'End Sub
        ''
        '==========================================================================================
        '   Format the current error object into a standard string
        '==========================================================================================
        '
        Public Function GetErrString(Optional ByVal ErrorObject As ErrObject = Nothing) As String
            Dim Copy As String
            If ErrorObject Is Nothing Then
                If Err.Number = 0 Then
                    GetErrString = "[no error]"
                Else
                    Copy = Err.Description
                    Copy = vbReplace(Copy, vbCrLf, "-")
                    Copy = vbReplace(Copy, vbLf, "-")
                    Copy = vbReplace(Copy, vbCrLf, "")
                    GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
                End If
            Else
                If ErrorObject.Number = 0 Then
                    GetErrString = "[no error]"
                Else
                    Copy = ErrorObject.Description
                    Copy = vbReplace(Copy, vbCrLf, "-")
                    Copy = vbReplace(Copy, vbLf, "-")
                    Copy = vbReplace(Copy, vbCrLf, "")
                    GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
                End If
            End If
            '
        End Function
        '
        '==========================================================================================
        '   Format the current error object into a standard string
        '==========================================================================================
        '
        Public Function GetProcessID() As Integer
            Dim Instance As Process = Process.GetCurrentProcess()
            '
            GetProcessID = Instance.Id
        End Function
        '
        '==========================================================================================
        '   Test if a test string is in a delimited string
        '==========================================================================================
        '
        Public Function IsInDelimitedString(ByVal DelimitedString As String, ByVal TestString As String, ByVal Delimiter As String) As Boolean
            IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
        End Function
        '
        '========================================================================
        ' EncodeURL
        '
        '   Encodes only what is to the left of the first ?
        '   All URL path characters are assumed to be correct (/:#)
        '========================================================================
        '
        Public Function EncodeURL(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim URLSplit() As String
            'Dim LeftSide As String
            'Dim RightSide As String
            '
            EncodeURL = Source
            If Source <> "" Then
                URLSplit = Split(Source, "?")
                EncodeURL = URLSplit(0)
                EncodeURL = vbReplace(EncodeURL, "%", "%25")
                '
                EncodeURL = vbReplace(EncodeURL, """", "%22")
                EncodeURL = vbReplace(EncodeURL, " ", "%20")
                EncodeURL = vbReplace(EncodeURL, "$", "%24")
                EncodeURL = vbReplace(EncodeURL, "+", "%2B")
                EncodeURL = vbReplace(EncodeURL, ",", "%2C")
                EncodeURL = vbReplace(EncodeURL, ";", "%3B")
                EncodeURL = vbReplace(EncodeURL, "<", "%3C")
                EncodeURL = vbReplace(EncodeURL, "=", "%3D")
                EncodeURL = vbReplace(EncodeURL, ">", "%3E")
                EncodeURL = vbReplace(EncodeURL, "@", "%40")
                If UBound(URLSplit) > 0 Then
                    EncodeURL = EncodeURL & "?" & EncodeQueryString(URLSplit(1))
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' EncodeQueryString
        '
        '   This routine encodes the URL QueryString to conform to rules
        '========================================================================
        '
        Public Function EncodeQueryString(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim QSSplit() As String
            Dim QSPointer As Integer
            Dim NVSplit() As String
            Dim NV As String
            '
            EncodeQueryString = ""
            If Source <> "" Then
                QSSplit = Split(Source, "&")
                For QSPointer = 0 To UBound(QSSplit)
                    NV = QSSplit(QSPointer)
                    If NV <> "" Then
                        NVSplit = Split(NV, "=")
                        If UBound(NVSplit) = 0 Then
                            NVSplit(0) = EncodeRequestVariable(NVSplit(0))
                            EncodeQueryString = EncodeQueryString & "&" & NVSplit(0)
                        Else
                            NVSplit(0) = EncodeRequestVariable(NVSplit(0))
                            NVSplit(1) = EncodeRequestVariable(NVSplit(1))
                            EncodeQueryString = EncodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
                        End If
                    End If
                Next
                If EncodeQueryString <> "" Then
                    EncodeQueryString = Mid(EncodeQueryString, 2)
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' EncodeRequestVariable
        '
        '   This routine encodes a request variable for a URL Query String
        '       ...can be the requestname or the requestvalue
        '========================================================================
        '
        Public Function EncodeRequestVariable(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim SourcePointer As Integer
            Dim Character As String
            Dim LocalSource As String
            '
            EncodeRequestVariable = ""
            If Source <> "" Then
                LocalSource = Source
                ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
                'LocalSource = vbReplace(LocalSource, " ", "+")
                For SourcePointer = 1 To Len(LocalSource)
                    Character = Mid(LocalSource, SourcePointer, 1)
                    ' "%" added so if this is called twice, it will not destroy "%20" values
                    If False Then
                        'End If
                        'If Character = " " Then
                        EncodeRequestVariable += "+"
                    ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
                        'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
                        EncodeRequestVariable += Character
                    Else
                        EncodeRequestVariable += "%" & Hex(Asc(Character))
                    End If
                Next
            End If
            '
        End Function
        ''
        ''========================================================================
        '' DecodeHTML
        ''
        ''   Convert HTML equivalent characters to their equivalents
        ''========================================================================
        ''
        'Public Function DecodeHTML(ByVal Source As String) As String
        '    ' ##### removed to catch err<>0 problem on error resume next
        '    '
        '    Dim Pos As Integer
        '    Dim s As String
        '    Dim CharCodeString As String
        '    Dim CharCode As Integer
        '    Dim PosEnd As Integer
        '    '
        '    ' 11/26/2009 - basically re-wrote it, I commented the old one out below
        '    '
        '    s = Source
        '    '
        '    ' numeric entities
        '    '
        '    Pos = Len(s)
        '    Pos = InStrRev(s, "&#", Pos)
        '    Do While Pos <> 0
        '        CharCodeString = ""
        '        If Mid(s, Pos + 3, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 1)
        '            PosEnd = Pos + 4
        '        ElseIf Mid(s, Pos + 4, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 2)
        '            PosEnd = Pos + 5
        '        ElseIf Mid(s, Pos + 5, 1) = ";" Then
        '            CharCodeString = Mid(s, Pos + 2, 3)
        '            PosEnd = Pos + 6
        '        End If
        '        If CharCodeString <> "" Then
        '            If vbIsNumeric(CharCodeString) Then
        '                CharCode = CLng(CharCodeString)
        '                s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, PosEnd)
        '            End If
        '        End If
        '        '
        '        Pos = InStrRev(s, "&#", Pos)
        '    Loop
        '    '
        '    ' character entities (at least the most common )
        '    '
        '    s = vbReplace(s, "&lt;", "<")
        '    s = vbReplace(s, "&gt;", ">")
        '    s = vbReplace(s, "&quot;", """")
        '    s = vbReplace(s, "&apos;", "'")
        '    '
        '    ' always last
        '    '
        '    s = vbReplace(s, "&amp;", "&")
        '    '
        '    DecodeHTML = s
        '    '
        'End Function
        '
        '========================================================================
        ' AddSpanClass
        '
        '   Adds a span around the copy with the class name provided
        '========================================================================
        '
        Public Function AddSpan(ByVal Copy As String, ByVal ClassName As String) As String
            '
            AddSpan = "<SPAN Class=""" & ClassName & """>" & Copy & "</SPAN>"
            '
        End Function
        '
        '========================================================================
        ' DecodeResponseVariable
        '
        '   Converts a querystring name or value back into the characters it represents
        '   This is the same code as the decodeurl
        '========================================================================
        '
        Public Function DecodeResponseVariable(ByVal Source As String) As String
            '
            Dim Position As Integer
            Dim ESCString As String
            Dim ESCValue As Integer
            Dim Digit0 As String
            Dim Digit1 As String
            'Dim iURL As String
            '
            'iURL = Source
            ' plus to space only applies for query component of a URL, but %99 encoding works for both
            'DecodeResponseVariable = vbReplace(iURL, "+", " ")
            DecodeResponseVariable = Source
            Position = vbInstr(1, DecodeResponseVariable, "%")
            Do While Position <> 0
                ESCString = Mid(DecodeResponseVariable, Position, 3)
                Digit0 = vbUCase(Mid(ESCString, 2, 1))
                Digit1 = vbUCase(Mid(ESCString, 3, 1))
                If ((Digit0 >= "0") And (Digit0 <= "9")) Or ((Digit0 >= "A") And (Digit0 <= "F")) Then
                    If ((Digit1 >= "0") And (Digit1 <= "9")) Or ((Digit1 >= "A") And (Digit1 <= "F")) Then
                        ESCValue = CInt("&H" & Mid(ESCString, 2))
                        DecodeResponseVariable = Mid(DecodeResponseVariable, 1, Position - 1) & Chr(ESCValue) & Mid(DecodeResponseVariable, Position + 3)
                        '  & vbReplace(DecodeResponseVariable, ESCString, Chr(ESCValue), Position, 1)
                    End If
                End If
                Position = vbInstr(Position + 1, DecodeResponseVariable, "%")
            Loop
            '
        End Function
        '
        '========================================================================
        ' DecodeURL
        '   Converts a querystring from an Encoded URL (with %20 and +), to non incoded (with spaced)
        '========================================================================
        '
        Public Function DecodeURL(ByVal Source As String) As String
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim Position As Integer
            Dim ESCString As String
            Dim ESCValue As Integer
            Dim Digit0 As String
            Dim Digit1 As String
            'Dim iURL As String
            '
            'iURL = Source
            ' plus to space only applies for query component of a URL, but %99 encoding works for both
            'DecodeURL = vbReplace(iURL, "+", " ")
            DecodeURL = Source
            Position = vbInstr(1, DecodeURL, "%")
            Do While Position <> 0
                ESCString = Mid(DecodeURL, Position, 3)
                Digit0 = vbUCase(Mid(ESCString, 2, 1))
                Digit1 = vbUCase(Mid(ESCString, 3, 1))
                If ((Digit0 >= "0") And (Digit0 <= "9")) Or ((Digit0 >= "A") And (Digit0 <= "F")) Then
                    If ((Digit1 >= "0") And (Digit1 <= "9")) Or ((Digit1 >= "A") And (Digit1 <= "F")) Then
                        ESCValue = CInt("&H" & Mid(ESCString, 2))
                        DecodeURL = vbReplace(DecodeURL, ESCString, Chr(ESCValue))
                    End If
                End If
                Position = vbInstr(Position + 1, DecodeURL, "%")
            Loop
            '
        End Function
        '
        '========================================================================
        ' GetFirstNonZeroDate
        '
        '   Converts a querystring name or value back into the characters it represents
        '========================================================================
        '
        Public Function GetFirstNonZeroDate(ByVal Date0 As Date, ByVal Date1 As Date) As Date
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim NullDate As Date
            '
            NullDate = Date.MinValue
            If Date0 = NullDate Then
                If Date1 = NullDate Then
                    '
                    ' Both 0, return 0
                    '
                    GetFirstNonZeroDate = NullDate
                Else
                    '
                    ' Date0 is NullDate, return Date1
                    '
                    GetFirstNonZeroDate = Date1
                End If
            Else
                If Date1 = NullDate Then
                    '
                    ' Date1 is nulldate, return Date0
                    '
                    GetFirstNonZeroDate = Date0
                ElseIf Date0 < Date1 Then
                    '
                    ' Date0 is first
                    '
                    GetFirstNonZeroDate = Date0
                Else
                    '
                    ' Date1 is first
                    '
                    GetFirstNonZeroDate = Date1
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' getFirstposition
        '
        '   returns 0 if both are zero
        '   returns 1 if the first integer is non-zero and less then the second
        '   returns 2 if the second integer is non-zero and less then the first
        '========================================================================
        '
        Public Function GetFirstNonZeroInteger(ByVal Integer1 As Integer, ByVal Integer2 As Integer) As Integer
            ' ##### removed to catch err<>0 problem on error resume next
            '
            If Integer1 = 0 Then
                If Integer2 = 0 Then
                    '
                    ' Both 0, return 0
                    '
                    GetFirstNonZeroInteger = 0
                Else
                    '
                    ' Integer1 is 0, return Integer2
                    '
                    GetFirstNonZeroInteger = 2
                End If
            Else
                If Integer2 = 0 Then
                    '
                    ' Integer2 is 0, return Integer1
                    '
                    GetFirstNonZeroInteger = 1
                ElseIf Integer1 < Integer2 Then
                    '
                    ' Integer1 is first
                    '
                    GetFirstNonZeroInteger = 1
                Else
                    '
                    ' Integer2 is first
                    '
                    GetFirstNonZeroInteger = 2
                End If
            End If
            '
        End Function
        '
        '========================================================================
        ' splitDelimited
        '   returns the result of a Split, except it honors quoted text
        '   if a quote is found, it is assumed to also be a delimiter ( 'this"that"theother' = 'this "that" theother' )
        '========================================================================
        '
        Public Function SplitDelimited(ByVal WordList As String, ByVal Delimiter As String) As String()
            ' ##### removed to catch err<>0 problem on error resume next
            '
            Dim QuoteSplit() As String
            Dim QuoteSplitCount As Integer
            Dim QuoteSplitPointer As Integer
            Dim InQuote As Boolean
            Dim Out() As String
            Dim OutPointer As Integer
            Dim OutSize As Integer
            Dim SpaceSplit() As String
            Dim SpaceSplitCount As Integer
            Dim SpaceSplitPointer As Integer
            Dim Fragment As String
            '
            OutPointer = 0
            ReDim Out(0)
            OutSize = 1
            If WordList <> "" Then
                QuoteSplit = Split(WordList, """")
                QuoteSplitCount = UBound(QuoteSplit) + 1
                InQuote = (Mid(WordList, 1, 1) = "")
                For QuoteSplitPointer = 0 To QuoteSplitCount - 1
                    Fragment = QuoteSplit(QuoteSplitPointer)
                    If Fragment = "" Then
                        '
                        ' empty fragment
                        ' this is a quote at the end, or two quotes together
                        ' do not skip to the next out pointer
                        '
                        If OutPointer >= OutSize Then
                            OutSize = OutSize + 10
                            ReDim Preserve Out(OutSize)
                        End If
                        'OutPointer = OutPointer + 1
                    Else
                        If Not InQuote Then
                            SpaceSplit = Split(Fragment, Delimiter)
                            SpaceSplitCount = UBound(SpaceSplit) + 1
                            For SpaceSplitPointer = 0 To SpaceSplitCount - 1
                                If OutPointer >= OutSize Then
                                    OutSize = OutSize + 10
                                    ReDim Preserve Out(OutSize)
                                End If
                                Out(OutPointer) = Out(OutPointer) & SpaceSplit(SpaceSplitPointer)
                                If (SpaceSplitPointer <> (SpaceSplitCount - 1)) Then
                                    '
                                    ' divide output between splits
                                    '
                                    OutPointer = OutPointer + 1
                                    If OutPointer >= OutSize Then
                                        OutSize = OutSize + 10
                                        ReDim Preserve Out(OutSize)
                                    End If
                                End If
                            Next
                        Else
                            Out(OutPointer) = Out(OutPointer) & """" & Fragment & """"
                        End If
                    End If
                    InQuote = Not InQuote
                Next
            End If
            ReDim Preserve Out(OutPointer)
            '
            '
            SplitDelimited = Out
            '
        End Function
        '
        '
        '
        Public Function GetYesNo(ByVal Key As Boolean) As String
            If Key Then
                GetYesNo = "Yes"
            Else
                GetYesNo = "No"
            End If
        End Function
        ''
        ''
        ''
        'Public Function GetFilename(ByVal PathFilename As String) As String
        '    Dim Position As Integer
        '    '
        '    GetFilename = PathFilename
        '    Position = InStrRev(GetFilename, "/")
        '    If Position <> 0 Then
        '        GetFilename = Mid(GetFilename, Position + 1)
        '    End If
        'End Function
        '        '
        '        '
        '        '
        Public Function StartTable(ByVal Padding As Integer, ByVal Spacing As Integer, ByVal Border As Integer, Optional ByVal ClassStyle As String = "") As String
            StartTable = "<table border=""" & Border & """ cellpadding=""" & Padding & """ cellspacing=""" & Spacing & """ class=""" & ClassStyle & """ width=""100%"">"
        End Function
        '        '
        '        '
        '        '
        Public Function StartTableRow() As String
            StartTableRow = "<tr>"
        End Function
        '        '
        '        '
        '        '
        Public Function StartTableCell(Optional ByVal Width As String = "", Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False, Optional ByVal Align As String = "", Optional ByVal BGColor As String = "") As String
            If Width <> "" Then
                StartTableCell = " width=""" & Width & """"
            End If
            If BGColor <> "" Then
                StartTableCell = StartTableCell & " bgcolor=""" & BGColor & """"
            ElseIf EvenRow Then
                StartTableCell = StartTableCell & " class=""ccPanelRowEven"""
            Else
                StartTableCell = StartTableCell & " class=""ccPanelRowOdd"""
            End If
            If ColSpan <> 0 Then
                StartTableCell = StartTableCell & " colspan=""" & ColSpan & """"
            End If
            If Align <> "" Then
                StartTableCell = StartTableCell & " align=""" & Align & """"
            End If
            StartTableCell = "<TD" & StartTableCell & ">"
        End Function
        '        '
        '        '
        '        '
        Public Function GetTableCell(ByVal Copy As String, Optional ByVal Width As String = "", Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False, Optional ByVal Align As String = "", Optional ByVal BGColor As String = "") As String
            GetTableCell = StartTableCell(Width, ColSpan, EvenRow, Align, BGColor) & Copy & kmaEndTableCell
        End Function
        '        '
        '        '
        '        '
        Public Function GetTableRow(ByVal Cell As String, Optional ByVal ColSpan As Integer = 0, Optional ByVal EvenRow As Boolean = False) As String
            GetTableRow = StartTableRow() & GetTableCell(Cell, "100%", ColSpan, EvenRow) & kmaEndTableRow
        End Function
        '
        ' remove the host and approotpath, leaving the "active" path and all else
        '
        Public Function ConvertShortLinkToLink(ByVal URL As String, ByVal PathPagePrefix As String) As String
            ConvertShortLinkToLink = URL
            If URL <> "" And PathPagePrefix <> "" Then
                If vbInstr(1, ConvertShortLinkToLink, PathPagePrefix, vbTextCompare) = 1 Then
                    ConvertShortLinkToLink = Mid(ConvertShortLinkToLink, Len(PathPagePrefix) + 1)
                End If
            End If
        End Function
        '
        ' ------------------------------------------------------------------------------------------------------
        '   Preserve URLs that do not start HTTP or HTTPS
        '   Preserve URLs from other sites (offsite)
        '   Preserve HTTP://ServerHost/ServerVirtualPath/Files/ in all cases
        '   Convert HTTP://ServerHost/ServerVirtualPath/folder/page -> /folder/page
        '   Convert HTTP://ServerHost/folder/page -> /folder/page
        ' ------------------------------------------------------------------------------------------------------
        '
        Public Function ConvertLinkToShortLink(ByVal URL As String, ByVal ServerHost As String, ByVal ServerVirtualPath As String) As String
            '
            Dim BadString As String = ""
            Dim GoodString As String = ""
            Dim Protocol As String = ""
            Dim WorkingLink As String = ""
            '
            WorkingLink = URL
            '
            ' ----- Determine Protocol
            '
            If vbInstr(1, WorkingLink, "HTTP://", vbTextCompare) = 1 Then
                '
                ' HTTP
                '
                Protocol = Mid(WorkingLink, 1, 7)
            ElseIf vbInstr(1, WorkingLink, "HTTPS://", vbTextCompare) = 1 Then
                '
                ' HTTPS
                '
                ' try this -- a ssl link can not be shortened
                ConvertLinkToShortLink = WorkingLink
                Exit Function
                Protocol = Mid(WorkingLink, 1, 8)
            End If
            If Protocol <> "" Then
                '
                ' ----- Protcol found, determine if is local
                '
                GoodString = Protocol & ServerHost
                If (InStr(1, WorkingLink, GoodString, vbTextCompare) <> 0) Then
                    '
                    ' URL starts with Protocol ServerHost
                    '
                    GoodString = Protocol & ServerHost & ServerVirtualPath & "/files/"
                    If (InStr(1, WorkingLink, GoodString, vbTextCompare) <> 0) Then
                        '
                        ' URL is in the virtual files directory
                        '
                        BadString = GoodString
                        GoodString = ServerVirtualPath & "/files/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                    Else
                        '
                        ' URL is not in files virtual directory
                        '
                        BadString = Protocol & ServerHost & ServerVirtualPath & "/"
                        GoodString = "/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                        '
                        BadString = Protocol & ServerHost & "/"
                        GoodString = "/"
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, vbTextCompare)
                    End If
                End If
            End If
            ConvertLinkToShortLink = WorkingLink
        End Function
        '
        ' Correct the link for the virtual path, either add it or remove it
        '
        Public Function EncodeAppRootPath(ByVal Link As String, ByVal VirtualPath As String, ByVal AppRootPath As String, ByVal ServerHost As String) As String
            '
            Dim Protocol As String = ""
            Dim Host As String = ""
            Dim Path As String = ""
            Dim Page As String = ""
            Dim QueryString As String = ""
            Dim VirtualHosted As Boolean = False
            '
            EncodeAppRootPath = Link
            If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) Or (InStr(1, Link, "/") = 1) Then
                'If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) And (InStr(1, Link, "/") <> 0) Then
                '
                ' This link is onsite and has a path
                '
                VirtualHosted = (InStr(1, AppRootPath, VirtualPath, vbTextCompare) <> 0)
                If VirtualHosted And (InStr(1, Link, AppRootPath, vbTextCompare) = 1) Then
                    '
                    ' quick - virtual hosted and link starts at AppRootPath
                    '
                ElseIf (Not VirtualHosted) And (Mid(Link, 1, 1) = "/") And (InStr(1, Link, AppRootPath, vbTextCompare) = 1) Then
                    '
                    ' quick - not virtual hosted and link starts at Root
                    '
                Else
                    Call SeparateURL(Link, Protocol, Host, Path, Page, QueryString)
                    If VirtualHosted Then
                        '
                        ' Virtual hosted site, add VirualPath if it is not there
                        '
                        If vbInstr(1, Path, AppRootPath, vbTextCompare) = 0 Then
                            If Path = "/" Then
                                Path = AppRootPath
                            Else
                                Path = AppRootPath & Mid(Path, 2)
                            End If
                        End If
                    Else
                        '
                        ' Root hosted site, remove virtual path if it is there
                        '
                        If vbInstr(1, Path, AppRootPath, vbTextCompare) <> 0 Then
                            Path = vbReplace(Path, AppRootPath, "/")
                        End If
                    End If
                    EncodeAppRootPath = Protocol & Host & Path & Page & QueryString
                End If
            End If
        End Function
        '
        ' Return just the tablename from a tablename reference (database.object.tablename->tablename)
        '
        Public Function GetDbObjectTableName(ByVal DbObject As String) As String
            Dim Position As Integer
            '
            GetDbObjectTableName = DbObject
            Position = InStrRev(GetDbObjectTableName, ".")
            If Position > 0 Then
                GetDbObjectTableName = Mid(GetDbObjectTableName, Position + 1)
            End If
        End Function
        '
        '
        '
        Public Function GetLinkedText(ByVal AnchorTag As String, ByVal AnchorText As String) As String
            '
            Dim UcaseAnchorText As String
            Dim LinkPosition As Integer
            Dim MethodName As String
            Dim iAnchorTag As String
            Dim iAnchorText As String
            '
            MethodName = "GetLinkedText"
            '
            GetLinkedText = ""
            iAnchorTag = AnchorTag
            iAnchorText = AnchorText
            UcaseAnchorText = vbUCase(iAnchorText)
            If (iAnchorTag <> "") And (iAnchorText <> "") Then
                LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", -1)
                If LinkPosition = 0 Then
                    GetLinkedText = iAnchorTag & iAnchorText & "</A>"
                Else
                    GetLinkedText = iAnchorText
                    LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", -1)
                    Do While LinkPosition > 1
                        GetLinkedText = Mid(GetLinkedText, 1, LinkPosition - 1) & "</A>" & Mid(GetLinkedText, LinkPosition + 7)
                        LinkPosition = InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1)
                        If LinkPosition <> 0 Then
                            GetLinkedText = Mid(GetLinkedText, 1, LinkPosition - 1) & iAnchorTag & Mid(GetLinkedText, LinkPosition + 6)
                        End If
                        LinkPosition = InStrRev(UcaseAnchorText, "</LINK>", LinkPosition)
                    Loop
                End If
            End If
            '
        End Function
        '
        Public Function EncodeInitialCaps(ByVal Source As String) As String
            Dim SegSplit() As String
            Dim SegPtr As Integer
            Dim SegMax As Integer
            '
            EncodeInitialCaps = ""
            If Source <> "" Then
                SegSplit = Split(Source, " ")
                SegMax = UBound(SegSplit)
                If SegMax >= 0 Then
                    For SegPtr = 0 To SegMax
                        SegSplit(SegPtr) = vbUCase(Left(SegSplit(SegPtr), 1)) & vbLCase(Mid(SegSplit(SegPtr), 2))
                    Next
                End If
                EncodeInitialCaps = Join(SegSplit, " ")
            End If
        End Function
        '
        '
        '
        Public Function RemoveTag(ByVal Source As String, ByVal TagName As String) As String
            Dim Pos As Integer
            Dim PosEnd As Integer
            RemoveTag = Source
            Pos = vbInstr(1, Source, "<" & TagName, vbTextCompare)
            If Pos <> 0 Then
                PosEnd = vbInstr(Pos, Source, ">")
                If PosEnd > 0 Then
                    RemoveTag = Left(Source, Pos - 1) & Mid(Source, PosEnd + 1)
                End If
            End If
        End Function
        '
        '
        '
        Public Function RemoveStyleTags(ByVal Source As String) As String
            RemoveStyleTags = Source
            Do While vbInstr(1, RemoveStyleTags, "<style", vbTextCompare) <> 0
                RemoveStyleTags = RemoveTag(RemoveStyleTags, "style")
            Loop
            Do While vbInstr(1, RemoveStyleTags, "</style", vbTextCompare) <> 0
                RemoveStyleTags = RemoveTag(RemoveStyleTags, "/style")
            Loop
        End Function
        '
        '
        '
        Public Function GetSingular(ByVal PluralSource As String) As String
            '
            Dim UpperCase As Boolean
            Dim LastCharacter As String
            '
            GetSingular = PluralSource
            If Len(GetSingular) > 1 Then
                LastCharacter = Right(GetSingular, 1)
                If LastCharacter <> vbUCase(LastCharacter) Then
                    UpperCase = True
                End If
                If vbUCase(Right(GetSingular, 3)) = "IES" Then
                    If UpperCase Then
                        GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 3) & "Y"
                    Else
                        GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 3) & "y"
                    End If
                ElseIf vbUCase(Right(GetSingular, 2)) = "SS" Then
                    ' nothing
                ElseIf vbUCase(Right(GetSingular, 1)) = "S" Then
                    GetSingular = Mid(GetSingular, 1, Len(GetSingular) - 1)
                Else
                    ' nothing
                End If
            End If
        End Function
        '
        '
        '
        Public Function EncodeJavascript(ByVal Source As String) As String
            '
            EncodeJavascript = Source
            EncodeJavascript = vbReplace(EncodeJavascript, "\", "\\")
            EncodeJavascript = vbReplace(EncodeJavascript, "'", "\'")
            'EncodeJavascript = vbReplace(EncodeJavascript, "'", "'+""'""+'")
            EncodeJavascript = vbReplace(EncodeJavascript, vbCrLf, "\n")
            EncodeJavascript = vbReplace(EncodeJavascript, vbCr, "\n")
            EncodeJavascript = vbReplace(EncodeJavascript, vbLf, "\n")
            '
        End Function
        ''' <summary>
        ''' returns a 1-based index into the comma seperated ListOfItems where Item is found
        ''' </summary>
        ''' <param name="Item"></param>
        ''' <param name="ListOfItems"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetListIndex(ByVal Item As String, ByVal ListOfItems As String) As Integer
            '
            Dim Items() As String
            Dim LcaseItem As String
            Dim LcaseList As String
            Dim Ptr As Integer
            '
            GetListIndex = 0
            If ListOfItems <> "" Then
                LcaseItem = vbLCase(Item)
                LcaseList = vbLCase(ListOfItems)
                Items = SplitDelimited(LcaseList, ",")
                For Ptr = 0 To UBound(Items)
                    If Items(Ptr) = LcaseItem Then
                        GetListIndex = Ptr + 1
                        Exit For
                    End If
                Next
            End If
            '
        End Function
        '
        '========================================================================================================
        '
        ' Finds all tags matching the input, and concatinates them into the output
        ' does NOT account for nested tags, use for body, script, style
        '
        ' ReturnAll - if true, it returns all the occurances, back-to-back
        '
        '========================================================================================================
        '
        Public Function GetTagInnerHTML(ByVal PageSource As String, ByVal Tag As String, ByVal ReturnAll As Boolean) As String
            On Error GoTo ErrorTrap
            '
            Dim TagStart As Integer
            Dim TagEnd As Integer
            Dim LoopCnt As Integer
            Dim WB As String
            Dim Pos As Integer
            Dim PosEnd As Integer
            Dim CommentPos As Integer
            Dim ScriptPos As Integer
            '
            GetTagInnerHTML = ""
            Pos = 1
            Do While (Pos > 0) And (LoopCnt < 100)
                TagStart = vbInstr(Pos, PageSource, "<" & Tag, vbTextCompare)
                If TagStart = 0 Then
                    Pos = 0
                Else
                    '
                    ' tag found, skip any comments that start between current position and the tag
                    '
                    CommentPos = vbInstr(Pos, PageSource, "<!--")
                    If (CommentPos <> 0) And (CommentPos < TagStart) Then
                        '
                        ' skip comment and start again
                        '
                        Pos = vbInstr(CommentPos, PageSource, "-->")
                    Else
                        ScriptPos = vbInstr(Pos, PageSource, "<script")
                        If (ScriptPos <> 0) And (ScriptPos < TagStart) Then
                            '
                            ' skip comment and start again
                            '
                            Pos = vbInstr(ScriptPos, PageSource, "</script")
                        Else
                            '
                            ' Get the tags innerHTML
                            '
                            TagStart = vbInstr(TagStart, PageSource, ">", vbTextCompare)
                            Pos = TagStart
                            If TagStart <> 0 Then
                                TagStart = TagStart + 1
                                TagEnd = vbInstr(TagStart, PageSource, "</" & Tag, vbTextCompare)
                                If TagEnd <> 0 Then
                                    GetTagInnerHTML &= Mid(PageSource, TagStart, TagEnd - TagStart)
                                End If
                            End If
                        End If
                    End If
                    LoopCnt = LoopCnt + 1
                    If ReturnAll Then
                        TagStart = vbInstr(TagEnd, PageSource, "<" & Tag, vbTextCompare)
                    Else
                        TagStart = 0
                    End If
                End If
            Loop
            '
            Exit Function
            '
ErrorTrap:
        End Function
        '
        '
        '
        Public Function EncodeInteger(ByVal Expression As Object) As Integer
            '
            EncodeInteger = 0
            If vbIsNumeric(Expression) Then
                EncodeInteger = CInt(Expression)
            ElseIf TypeOf Expression Is Boolean Then
                If DirectCast(Expression, Boolean) Then
                    EncodeInteger = 1
                End If
            End If
        End Function
        '
        '
        '
        Public Function EncodeNumber(ByVal Expression As Object) As Double
            EncodeNumber = 0
            If vbIsNumeric(Expression) Then
                EncodeNumber = CDbl(Expression)
            ElseIf TypeOf Expression Is Boolean Then
                If DirectCast(Expression, Boolean) Then
                    EncodeNumber = 1
                End If
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Function EncodeText(ByVal Expression As Object) As String
            If Not (TypeOf Expression Is DBNull) Then
                If (Expression IsNot Nothing) Then
                    Return Expression.ToString()
                End If
            End If
            Return String.Empty
        End Function
        '
        '====================================================================================================
        '
        Public Function EncodeBoolean(ByVal Expression As Object) As Boolean
            EncodeBoolean = False
            If TypeOf Expression Is Boolean Then
                EncodeBoolean = DirectCast(Expression, Boolean)
            ElseIf vbIsNumeric(Expression) Then
                EncodeBoolean = (CStr(Expression) <> "0")
            ElseIf TypeOf Expression Is String Then
                Select Case Expression.ToString.ToLower.Trim
                    Case "on", "yes", "true"
                        EncodeBoolean = True
                End Select
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Function EncodeDate(ByVal Expression As Object) As Date
            EncodeDate = Date.MinValue
            If IsDate(Expression) Then
                EncodeDate = CDate(Expression)
                'If EncodeDate < #1/1/1990# Then
                '    EncodeDate = Date.MinValue
                'End If
            End If
        End Function
        '
        '========================================================================
        '   Gets the next line from a string, and removes the line
        '========================================================================
        '
        Public Function getLine(ByRef Body As String) As String
            Dim returnFirstLine As String = Body
            Try
                Dim EOL As Integer
                Dim NextCR As Integer
                Dim NextLF As Integer
                Dim BOL As Integer
                '
                NextCR = vbInstr(1, Body, vbCr)
                NextLF = vbInstr(1, Body, vbLf)

                If NextCR <> 0 Or NextLF <> 0 Then
                    If NextCR <> 0 Then
                        If NextLF <> 0 Then
                            If NextCR < NextLF Then
                                EOL = NextCR - 1
                                If NextLF = NextCR + 1 Then
                                    BOL = NextLF + 1
                                Else
                                    BOL = NextCR + 1
                                End If

                            Else
                                EOL = NextLF - 1
                                BOL = NextLF + 1
                            End If
                        Else
                            EOL = NextCR - 1
                            BOL = NextCR + 1
                        End If
                    Else
                        EOL = NextLF - 1
                        BOL = NextLF + 1
                    End If
                    returnFirstLine = Mid(Body, 1, EOL)
                    Body = Mid(Body, BOL)
                Else
                    returnFirstLine = Body
                    Body = ""
                End If
            Catch ex As Exception
                '
                '
                '
            End Try
            Return returnFirstLine
        End Function
        '
        '
        '
        Public Function runProcess(cpCore As coreClass, ByVal Cmd As String, Optional ByVal Arguments As String = "", Optional ByVal WaitForReturn As Boolean = False) As String
            Dim returnResult As String = ""
            Dim p As Process = New Process()
            '
            cpCore.log_appendLog("ccCommonModule.runProcess, cmd=[" & Cmd & "], Arguments=[" & Arguments & "], WaitForReturn=[" & WaitForReturn & "]")
            '
            p.StartInfo.FileName = Cmd
            p.StartInfo.Arguments = Arguments
            p.StartInfo.UseShellExecute = False
            p.StartInfo.CreateNoWindow = True
            p.StartInfo.ErrorDialog = False
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
            If WaitForReturn Then
                p.StartInfo.RedirectStandardOutput = True
            End If
            p.Start()
            If WaitForReturn Then
                p.WaitForExit(1000 * 60 * 5)
                returnResult = p.StandardOutput.ReadToEnd()
            End If

            p.Dispose()
            '
            Return returnResult
        End Function
        ''
        'Public Sub runProcess(cp.core,cmd As String, Optional ignore As Object = "", Optional waitforreturn As Boolean = False)
        '    Call runProcess(cp.core,cmd, waitforreturn)
        '    'Dim ShellObj As Object
        '    'ShellObj = CreateObject("WScript.Shell")
        '    'If Not (ShellObj Is Nothing) Then
        '    '    Call ShellObj.Run(Cmd, 0, WaitForReturn)
        '    'End If
        '    'ShellObj = Nothing
        'End Sub
        '
        '------------------------------------------------------------------------------------------------------------
        '   use only internally
        '
        '   encode an argument to be used in a name=value& (N-V-A) string
        '
        '   an argument can be any one of these is this format:
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
        '
        '   to create an nva string
        '       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
        '
        '   to decode an nva string
        '       split on ampersand then on equal, and decodeNvaArgument() each part
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function encodeNvaArgument(ByVal Arg As String) As String
            Dim a As String
            a = Arg
            If a <> "" Then
                a = vbReplace(a, vbCrLf, "#0013#")
                a = vbReplace(a, vbLf, "#0013#")
                a = vbReplace(a, vbCr, "#0013#")
                a = vbReplace(a, "&", "#0038#")
                a = vbReplace(a, "=", "#0061#")
                a = vbReplace(a, ",", "#0044#")
                a = vbReplace(a, """", "#0034#")
                a = vbReplace(a, "'", "#0039#")
                a = vbReplace(a, "|", "#0124#")
                a = vbReplace(a, "[", "#0091#")
                a = vbReplace(a, "]", "#0093#")
                a = vbReplace(a, ":", "#0058#")
            End If
            encodeNvaArgument = a
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   use only internally
        '       decode an argument removed from a name=value& string
        '       see encodeNvaArgument for details on how to use this
        '------------------------------------------------------------------------------------------------------------
        '
        Public Function decodeNvaArgument(ByVal EncodedArg As String) As String
            Dim a As String
            '
            a = EncodedArg
            a = vbReplace(a, "#0058#", ":")
            a = vbReplace(a, "#0093#", "]")
            a = vbReplace(a, "#0091#", "[")
            a = vbReplace(a, "#0124#", "|")
            a = vbReplace(a, "#0039#", "'")
            a = vbReplace(a, "#0034#", """")
            a = vbReplace(a, "#0044#", ",")
            a = vbReplace(a, "#0061#", "=")
            a = vbReplace(a, "#0038#", "&")
            a = vbReplace(a, "#0013#", vbCrLf)
            decodeNvaArgument = a
        End Function
        '
        '
        Public Function LogFileCopyPrep(ByVal Source As String) As String
            Dim Copy As String
            Copy = Source
            Copy = vbReplace(Copy, vbCrLf, " ")
            Copy = vbReplace(Copy, vbLf, " ")
            Copy = vbReplace(Copy, vbCr, " ")
            LogFileCopyPrep = Copy
        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLText
        '        '========================================================================
        '        '
        '        Public Function app.EncodeSQLText(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            Else
        '                returnString = EncodeText(expression)
        '                If returnString = "" Then
        '                    returnString = "null"
        '                Else
        '                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLLongText
        '        '========================================================================
        '        '
        '        Public Function app.EncodeSQLText(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            Else
        '                returnString = EncodeText(expression)
        '                If returnString = "" Then
        '                    returnString = "null"
        '                Else
        '                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLDate
        '        '       encode a date variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public Function app.EncodeSQLDate(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            Dim expressionDate As Date = Date.MinValue
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            ElseIf Not IsDate(expression) Then
        '                returnString = "null"
        '            Else
        '                If IsDBNull(expression) Then
        '                    returnString = "null"
        '                Else
        '                    expressionDate = EncodeDate(expression)
        '                    If (expressionDate = Date.MinValue) Then
        '                        returnString = "null"
        '                    Else
        '                        returnString = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
        '                    End If
        '                End If
        '            End If
        '            Return returnString
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLNumber
        '        '       encode a number variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public Function app.EncodeSQLNumber(ByVal expression As Object) As String
        '            Dim returnString As String = ""
        '            Dim expressionNumber As Double = 0
        '            If expression Is Nothing Then
        '                returnString = "null"
        '            ElseIf VarType(expression) = vbBoolean Then
        '                If expression Then
        '                    returnString = SQLTrue
        '                Else
        '                    returnString = SQLFalse
        '                End If
        '            ElseIf Not vbIsNumeric(expression) Then
        '                returnString = "null"
        '            Else
        '                returnString = expression.ToString
        '            End If
        '            Return returnString
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '        End Function
        '        '
        '        '========================================================================
        '        '   encodeSQLBoolean
        '        '       encode a boolean variable to go in an sql expression
        '        '========================================================================
        '        '
        '        Public Function app.EncodeSQLBoolean(ByVal ExpressionVariant As Object) As String
        '            '
        '            'Dim src As String
        '            '
        '            app.EncodeSQLBoolean = SQLFalse
        '            If EncodeBoolean(ExpressionVariant) Then
        '                app.EncodeSQLBoolean = SQLTrue
        '            End If
        '        End Function
        '
        '=================================================================================
        '   Renamed to catch all the cases that used it in addons
        '
        '   Do not use this routine in Addons to get the addon option string value
        '   to get the value in an option string, use cmc.csv_getAddonOption("name")
        '
        ' Get the value of a name in a string of name value pairs parsed with vrlf and =
        '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        '   new format is "name1=value1 crlf name2=value2 crlf ..."
        '   There can be no extra spaces between the delimiter, the name and the "="
        '=================================================================================
        '
        Public Function getSimpleNameValue(ByVal Name As String, ByVal ArgumentString As String, ByVal DefaultValue As String, ByVal Delimiter As String) As String
            '
            Dim WorkingString As String
            Dim iDefaultValue As String
            Dim NameLength As Integer
            Dim ValueStart As Integer
            Dim ValueEnd As Integer
            Dim IsQuoted As Boolean
            '
            ' determine delimiter
            '
            If Delimiter = "" Then
                '
                ' If not explicit
                '
                If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
                    '
                    ' crlf can only be here if it is the delimiter
                    '
                    Delimiter = vbCrLf
                Else
                    '
                    ' either only one option, or it is the legacy '&' delimit
                    '
                    Delimiter = "&"
                End If
            End If
            iDefaultValue = DefaultValue
            WorkingString = ArgumentString
            getSimpleNameValue = iDefaultValue
            If WorkingString <> "" Then
                WorkingString = Delimiter & WorkingString & Delimiter
                ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
                If ValueStart <> 0 Then
                    NameLength = Len(Name)
                    ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
                    If Mid(WorkingString, ValueStart, 1) = """" Then
                        IsQuoted = True
                        ValueStart = ValueStart + 1
                    End If
                    If IsQuoted Then
                        ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
                    Else
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
                    End If
                    If ValueEnd = 0 Then
                        getSimpleNameValue = Mid(WorkingString, ValueStart)
                    Else
                        getSimpleNameValue = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
                    End If
                End If
            End If
            '

            Exit Function
            '
            ' ----- ErrorTrap
            '
ErrorTrap:
        End Function
        '==========================================================================================================================
        '   To convert from site license to server licenses, we still need the URLEncoder in the site license
        '   This routine generates a site license that is just the URL encoder.
        '==========================================================================================================================
        '
        Public Function GetURLEncoder() As String
            Randomize()
            GetURLEncoder = CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1000000000 + (Rnd() * 899999999)))
        End Function
        '
        '
        '
        Public Function getIpAddressList() As String
            Dim ipAddressList As String = ""
            Dim host As IPHostEntry
            '
            host = Dns.GetHostEntry(Dns.GetHostName())
            For Each ip As IPAddress In host.AddressList
                If (ip.AddressFamily = Sockets.AddressFamily.InterNetwork) Then
                    ipAddressList &= "," & ip.ToString
                End If
            Next
            If ipAddressList <> "" Then
                ipAddressList = ipAddressList.Substring(1)
            End If
            Return ipAddressList
        End Function
        ''
        ''
        ''
        'Public Function GetAddonRootPath() As String
        '    Dim testPath As String
        '    '
        '    GetAddonRootPath = "addons"
        '    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        '        '
        '        ' debugging - change program path to dummy path so addon builds all copy to
        '        '
        '        testPath = Environ$("programfiles(x86)")
        '        If testPath = "" Then
        '            testPath = Environ$("programfiles")
        '        End If
        '        GetAddonRootPath = testPath & "\kma\contensive\addons"
        '    End If
        'End Function
        '
        '
        '
        Public Function IsNull(ByVal source As Object) As Boolean
            Return (source Is Nothing)
        End Function
        ''
        ''
        ''
        'Public Function isNothing(ByVal source As Object) As Boolean
        '    Return IsNothing(source)
        '    'Dim returnIsEmpty As Boolean = True
        '    'Try
        '    '    If Not IsNothing(source) Then

        '    '    End If
        '    'Catch ex As Exception
        '    '    '
        '    'End Try
        '    'Return returnIsEmpty
        'End Function
        '
        '
        '
        Public Function isMissing(ByVal source As Object) As Boolean
            Return False
        End Function
        '
        ' convert date to number of seconds since 1/1/1990
        '
        Public Function dateToSeconds(ByVal sourceDate As Date) As Integer
            Dim returnSeconds As Integer
            Dim oldDate = New Date(1900, 1, 1)
            If sourceDate.CompareTo(oldDate) > 0 Then
                returnSeconds = CInt(sourceDate.Subtract(oldDate).TotalSeconds)
            End If
            Return returnSeconds
        End Function
        Public Const maxLongValue = 2147483647
        '
        ' Error Definitions
        '
        Public Const ERR_UNKNOWN As Integer = vbObjectError + 101
        Public Const ERR_FIELD_DOES_NOT_EXIST As Integer = vbObjectError + 102
        Public Const ERR_FILESIZE_NOT_ALLOWED As Integer = vbObjectError + 103
        Public Const ERR_FOLDER_DOES_NOT_EXIST As Integer = vbObjectError + 104
        Public Const ERR_FILE_ALREADY_EXISTS As Integer = vbObjectError + 105
        Public Const ERR_FILE_TYPE_NOT_ALLOWED As Integer = vbObjectError + 106
        '
        ' page content cache
        '
        Public Const pageManager_cache_pageContent_cacheName = "cache_pageContent"
        Public Const pageManager_cache_pageContent_fieldList = "ID,Active,ParentID,Name,Headline,MenuHeadline,DateArchive,DateExpires,PubDate,ChildListSortMethodID,ContentControlID,TemplateID" _
                & ",BlockContent,BlockPage,Link,RegistrationGroupID,BlockSourceID,CustomBlockMessage,JSOnLoad,JSHead,JSEndBody,Viewings,ContactMemberID" _
                & ",AllowHitNotification,TriggerSendSystemEmailID,TriggerConditionID,TriggerConditionGroupID,TriggerAddGroupID,TriggerRemoveGroupID,AllowMetaContentNoFollow" _
                & ",ParentListName,copyFilename,BriefFilename,AllowChildListDisplay,SortOrder,DateAdded,ModifiedDate,ChildPagesFound,AllowInMenus,AllowInChildLists" _
                & ",JSFilename,ChildListInstanceOptions,IsSecure,AllowBrief,allowReturnLinkDisplay,AllowPrinterVersion,allowEmailPage,allowSeeAlso,allowMoreInfo" _
                & ",allowFeedback,allowLastModifiedFooter,modifiedBy,DateReviewed,ReviewedBy,allowReviewedFooter,allowMessageFooter,contentpadding"
        Public Const PCC_ID = 0
        Public Const PCC_Active = 1
        Public Const PCC_ParentID = 2
        Public Const PCC_Name = 3
        Public Const PCC_Headline = 4
        Public Const PCC_MenuHeadline = 5
        Public Const PCC_DateArchive = 6
        Public Const PCC_DateExpires = 7
        Public Const PCC_PubDate = 8
        Public Const PCC_ChildListSortMethodID = 9
        Public Const PCC_ContentControlID = 10
        Public Const PCC_TemplateID = 11
        Public Const PCC_BlockContent = 12
        Public Const PCC_BlockPage = 13
        Public Const PCC_Link = 14
        Public Const PCC_RegistrationGroupID = 15
        Public Const PCC_BlockSourceID = 16
        Public Const PCC_CustomBlockMessageFilename = 17
        Public Const PCC_JSOnLoad = 18
        Public Const PCC_JSHead = 19
        Public Const PCC_JSEndBody = 20
        Public Const PCC_Viewings = 21
        Public Const PCC_ContactMemberID = 22
        Public Const PCC_AllowHitNotification = 23
        Public Const PCC_TriggerSendSystemEmailID = 24
        Public Const PCC_TriggerConditionID = 25
        Public Const PCC_TriggerConditionGroupID = 26
        Public Const PCC_TriggerAddGroupID = 27
        Public Const PCC_TriggerRemoveGroupID = 28
        Public Const PCC_AllowMetaContentNoFollow = 29
        Public Const PCC_ParentListName = 30
        Public Const PCC_CopyFilename = 31
        Public Const PCC_BriefFilename = 32
        Public Const PCC_AllowChildListDisplay = 33
        Public Const PCC_SortOrder = 34
        Public Const PCC_DateAdded = 35
        Public Const PCC_ModifiedDate = 36
        Public Const PCC_ChildPagesFound = 37
        Public Const PCC_AllowInMenus = 38
        Public Const PCC_AllowInChildLists = 39
        Public Const PCC_JSFilename = 40
        Public Const PCC_ChildListInstanceOptions = 41
        Public Const PCC_IsSecure = 42
        Public Const PCC_AllowBrief = 43
        Public Const PCC_allowReturnLinkDisplay = 44
        Public Const pcc_allowPrinterVersion = 45
        Public Const pcc_allowEmailPage = 46
        Public Const pcc_allowSeeAlso = 47
        Public Const pcc_allowMoreInfo = 48
        Public Const pcc_allowFeedback = 49
        Public Const pcc_allowLastModifiedFooter = 50
        Public Const PCC_ModifiedBy = 51
        Public Const PCC_DateReviewed = 52
        Public Const PCC_ReviewedBy = 53
        Public Const PCC_allowReviewedFooter = 54
        Public Const PCC_allowMessageFooter = 55
        Public Const PCC_ContentPadding = 56
        Public Const PCC_ColCnt = 57
        '
        ' addonIncludeRules cache
        '
        Public Const cache_sharedStylesAddonRules_cacheName = "cache_sharedStylesAddonRules"
        Public Const cache_sharedStylesAddonRules_fieldList = "addonId,styleId"
        Public Const sharedStylesAddonRulesCache_addonId = 0
        Public Const sharedStylesAddonRulesCache_styleId = 1
        Public Const sharedStylesAddonRulesCacheColCnt = 2
        '
        ' linkalias cache
        '
        Public Const cache_linkAlias_fieldList = "ID,Name,Link,PageID,QueryStringSuffix"
        Public Const linkAliasCache_id = 0
        Public Const linkAliasCache_name = 1
        Public Const linkAliasCache_link = 2
        Public Const linkAliasCache_pageId = 3
        Public Const linkAliasCache_queryStringSuffix = 4
        Public Const linkAliasCacheColCnt = 5
        '
        '   addonCache
        '
        'Public Const cache_addon_cacheName = "cache_addon"
        'Public Const cache_addon_fieldList = "id,active,name,ccguid,collectionid,Copy,ccguid,Link,ObjectProgramID,DotNetClass,ArgumentList,CopyText,IsInline,BlockDefaultStyles,StylesFilename,CustomStylesFilename,formxml,RemoteAssetLink,AsAjax,InFrame,ScriptingEntryPoint,ScriptingLanguageID,ScriptingCode,BlockEditTools,ScriptingTimeout,inlineScript,help,helplink,JavaScriptOnLoad,JavaScriptBodyEnd,PageTitle,MetaDescription,MetaKeywordList,OtherHeadTags,JSFilename,remoteMethod,onBodyStart,onBodyEnd,OnPageStartEvent,OnPageEndEvent,robotsTxt"
        'Public Const addonCache_Id = 0
        'Public Const addonCache_active = 1
        'Public Const addonCache_name = 2
        'Public Const addonCache_guid = 3
        'Public Const addonCache_collectionid = 4
        'Public Const addonCache_Copy = 5
        'Public Const addonCache_ccguid = 6
        'Public Const addonCache_Link = 7
        'Public Const addonCache_ObjectProgramID = 8
        'Public Const addonCache_DotNetClass = 9
        'Public Const addonCache_ArgumentList = 10
        'Public Const addonCache_CopyText = 11
        'Public Const addonCache_IsInline = 12
        'Public Const addonCache_BlockDefaultStyles = 13
        'Public Const addonCache_StylesFilename = 14
        'Public Const addonCache_CustomStylesFilename = 15
        'Public Const addonCache_formxml = 16
        'Public Const addonCache_RemoteAssetLink = 17
        'Public Const addonCache_AsAjax = 18
        'Public Const addonCache_InFrame = 19
        'Public Const addonCache_ScriptingEntryPoint = 20
        'Public Const addonCache_ScriptingLanguageID = 21
        'Public Const addonCache_ScriptingCode = 22
        'Public Const addonCache_BlockEditTools = 23
        'Public Const addonCache_ScriptingTimeout = 24
        'Public Const addonCache_inlineScript = 25
        'Public Const addonCache_help = 26
        'Public Const addonCache_helpLink = 27
        'Public Const addonCache_JavaScriptOnLoad = 28
        'Public Const addonCache_JavaScriptBodyEnd = 29
        'Public Const addonCache_PageTitle = 30
        'Public Const addonCache_MetaDescription = 31
        'Public Const addonCache_MetaKeywordList = 32
        'Public Const addonCache_OtherHeadTags = 33
        'Public Const addonCache_JSFilename = 34
        'Public Const addonCache_remoteMethod = 35
        'Public Const addoncache_OnBodyStart = 36
        'Public Const addoncache_OnBodyEnd = 37
        'Public Const addoncache_OnPageStart = 38
        'Public Const addoncache_OnPageEnd = 39
        'Public Const addoncache_robotsTxt = 40
        'Public Const addonCacheColCnt = 41
        '
        ' addonIncludeRules cache
        '
        Public Const cache_addonIncludeRules_cacheName = "cache_addonIncludeRules"
        Public Const cache_addonIncludeRules_fieldList = "addonId,includedAddonId"
        Public Const addonIncludeRulesCache_addonId = 0
        Public Const addonIncludeRulesCache_includedAddonId = 1
        Public Const addonIncludeRulesCacheColCnt = 2
        '
        ' addonIncludeRules cache
        '
        Public Const cache_LibraryFiles_cacheName = "cache_LibraryFiles"
        Public Const cache_LibraryFiles_fieldList = "id,ccguid,clicks,filename,width,height,AltText,altsizelist"
        Public Const LibraryFilesCache_Id = 0
        Public Const LibraryFilesCache_ccGuid = 1
        Public Const LibraryFilesCache_clicks = 2
        Public Const LibraryFilesCache_filename = 3
        Public Const LibraryFilesCache_width = 4
        Public Const LibraryFilesCache_height = 5
        Public Const LibraryFilesCache_alttext = 6
        Public Const LibraryFilesCache_altsizelist = 7
        Public Const LibraryFilesCacheColCnt = 8
        '
        ' link forward cache
        '
        Public Const cache_linkForward_cacheName = "cache_linkForward"
        '
        ' ==============================================================================
        ' true if there is a previous instance of this application running
        ' ==============================================================================
        '
        Public Function PrevInstance() As Boolean
            If UBound(Diagnostics.Process.GetProcessesByName _
               (Diagnostics.Process.GetCurrentProcess.ProcessName)) _
               > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Encode a date to minvalue, if date is < minVAlue,m set it to minvalue, if date < 1/1/1990 (the beginning of time), it returns date.minvalue
        ''' </summary>
        ''' <param name="sourceDate"></param>
        ''' <returns></returns>
        Public Function encodeDateMinValue(ByVal sourceDate As Date) As Date
            If sourceDate <= #1/1/1000# Then
                Return Date.MinValue
            End If
            Return sourceDate
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Return true if a date is the mindate, else return false 
        ''' </summary>
        ''' <param name="sourceDate"></param>
        ''' <returns></returns>
        Public Function isMinDate(sourceDate As Date) As Boolean
            Return encodeDateMinValue(sourceDate) = Date.MinValue
        End Function

        '
        '========================================================================
        ' ----- Create a filename for the Virtual Directory
        '   Do not allow spaces.
        '   If the content supports authoring, the filename returned will be for the
        '   current authoring record.
        '========================================================================
        '
        Public Function csv_GetVirtualFilenameByTable(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer, ByVal OriginalFilename As String, ByVal fieldType As Integer) As String
            '
            Dim RecordIDString As String
            Dim iTableName As String
            Dim iFieldName As String
            Dim iRecordID As Integer
            Dim MethodName As String
            Dim iOriginalFilename As String
            '
            MethodName = "csv_GetVirtualFilenameByTable"
            '
            iTableName = TableName
            iTableName = vbReplace(iTableName, " ", "_")
            iTableName = vbReplace(iTableName, ".", "_")
            '
            iFieldName = FieldName
            iFieldName = vbReplace(FieldName, " ", "_")
            iFieldName = vbReplace(iFieldName, ".", "_")
            '
            iOriginalFilename = OriginalFilename
            iOriginalFilename = vbReplace(iOriginalFilename, " ", "_")
            iOriginalFilename = vbReplace(iOriginalFilename, ".", "_")
            '
            RecordIDString = CStr(RecordID)
            If RecordID = 0 Then
                RecordIDString = CStr(getRandomLong())
                RecordIDString = New String("0"c, 12 - Len(RecordIDString)) & RecordIDString
            Else
                RecordIDString = New String("0"c, 12 - Len(RecordIDString)) & RecordIDString
            End If
            '
            If OriginalFilename <> "" Then
                csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & "/" & OriginalFilename
            Else
                Select Case fieldType
                    Case FieldTypeIdFileCSS
                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".css"
                    Case FieldTypeIdFileXML
                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".xml"
                    Case FieldTypeIdFileJavascript
                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".js"
                    Case FieldTypeIdFileHTMLPrivate
                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".html"
                    Case Else
                        csv_GetVirtualFilenameByTable = iTableName & "/" & iFieldName & "/" & RecordIDString & ".txt"
                End Select
            End If
        End Function
        '
        '=================================================================================
        '   Get a Random Long Value
        '=================================================================================
        '
        Public Function getRandomLong() As Integer
            '
            Dim RandomBase As Integer
            Dim RandomLimit As Integer
            Dim MethodName As String
            '
            MethodName = "getRandomLong"
            '
            RandomBase = Threading.Thread.CurrentThread.ManagedThreadId
            RandomBase = EncodeInteger(RandomBase + ((2 ^ 30) - 1))
            RandomLimit = EncodeInteger((2 ^ 31) - RandomBase - 1)
            Randomize()
            getRandomLong = EncodeInteger(RandomBase + (Rnd() * RandomLimit))
        End Function
        ''
        ''====================================================================================================
        '' the root folder where all app data is stored.
        ''   In local mode, this folder is the backup target
        ''   -- path means no trailing slash
        ''====================================================================================================
        ''
        'Public Function getProgramDataPath() As String
        '    Return coreFileSystemClass.normalizePath(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)) & "clib\"
        'End Function
        '
        '====================================================================================================
        ' the the c:\program files x86) path 
        '   -- path means no trailing slash
        '====================================================================================================
        '
        Public Function getProgramFilesPath() As String
            Dim returnPath As String = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)
            Dim ptr As Integer
            '
            ptr = vbInstr(1, returnPath, "\github\", vbTextCompare)
            If ptr <> 0 Then
                ' for ...\github\contensive4?\bin"
                ptr = vbInstr(ptr + 8, returnPath, "\")
                returnPath = Left(returnPath, ptr) & "bin\"
            End If

            Return returnPath
        End Function
        '
        '====================================================================================================
        ' the the \cclib path 
        '   -- path means no trailing slash
        '====================================================================================================
        '
        Public Function getccLibPath() As String
            Return getProgramFilesPath() & "ccLib\"
        End Function
        '
        '====================================================================================================
        ' the the name of the current executable
        '====================================================================================================
        '
        Public Function getAppExeName() As String
            Return System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath)
        End Function
        '
        '====================================================================================================
        ' convert a dtaTable to a comma delimited list of column 0
        '====================================================================================================
        '
        Public Function convertDataTableColumntoItemList(dt As DataTable) As String
            Dim returnString As String = ""
            For Each dr As DataRow In dt.Rows
                returnString &= "," & dr.Item(0).ToString
            Next
            If returnString.Length > 0 Then
                returnString = returnString.Substring(1)
            End If
            Return returnString
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' returns true or false if a string is located within another string. Similar to indexof but returns true/false 
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="haystack"></param>
        ''' <param name="needle"></param>
        ''' <param name="ignore"></param>
        ''' <returns></returns>
        Public Function isInStr(start As Integer, haystack As String, needle As String, Optional ignore As CompareMethod = CompareMethod.Text) As Boolean
            Return (InStr(start, haystack, needle, vbTextCompare) >= 0)
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Convert a route to the anticipated format (leading /, lowercase, no trailing slash)
        ''' </summary>
        ''' <param name="route"></param>
        ''' <returns></returns>
        Public Function normalizeRoute(route As String) As String
            Dim normalizedRoute As String = route.ToLower()
            Try
                If String.IsNullOrEmpty(normalizedRoute) Then
                    normalizedRoute = "/"
                Else
                    normalizedRoute = normalizedRoute.Replace("\", "/")
                    Do While normalizedRoute.IndexOf("//") >= 0
                        normalizedRoute = normalizedRoute.Replace("//", "/")
                    Loop
                    If (normalizedRoute.Substring(0, 1) <> "/") Then
                        normalizedRoute = "/" & normalizedRoute
                    End If
                    If (normalizedRoute.Substring(normalizedRoute.Length - 1, 1) = "/") Then
                        normalizedRoute = normalizedRoute.Substring(0, normalizedRoute.Length - 1)
                    End If
                End If
            Catch ex As Exception
                Throw New ApplicationException("Unexpected exception in normalizeRoute(route=[" & route & "])", ex)
            End Try
            Return normalizedRoute
        End Function
        '
        '========================================================================
        '   converts a virtual file into a filename
        '       - in local mode, the cdnFiles can be mapped to a virtual folder in appRoot
        '           -- see appConfig.cdnFilesVirtualFolder
        '       convert all / to \
        '       if it includes "://", it is a root file
        '       if it starts with "/", it is already root relative
        '       else (if it start with a file or a path), add the publicFileContentPathPrefix
        '========================================================================
        '
        Public Function convertCdnUrlToCdnPathFilename(ByVal cdnUrl As String) As String
            '
            ' this routine was originally written to handle modes that were not adopted (content file absolute and relative URLs)
            ' leave it here as a simple slash converter in case other conversions are needed later
            '
            Return vbReplace(cdnUrl, "/", "\")
        End Function
        '
        '==============================================================================
        Public Function isGuid(ignore As Object, Source As String) As Boolean
            Dim returnValue As Boolean = False
            Try
                If (Len(Source) = 38) And (Left(Source, 1) = "{") And (Right(Source, 1) = "}") Then
                    '
                    ' Good to go
                    '
                    returnValue = True
                ElseIf (Len(Source) = 36) And (InStr(1, Source, " ") = 0) Then
                    '
                    ' might be valid with the brackets, add them
                    '
                    returnValue = True
                    'source = "{" & source & "}"
                ElseIf (Len(Source) = 32) Then
                    '
                    ' might be valid with the brackets and the dashes, add them
                    '
                    returnValue = True
                    'source = "{" & Mid(source, 1, 8) & "-" & Mid(source, 9, 4) & "-" & Mid(source, 13, 4) & "-" & Mid(source, 17, 4) & "-" & Mid(source, 21) & "}"
                Else
                    '
                    ' not valid
                    '
                    returnValue = False
                    '        source = ""
                End If
            Catch ex As Exception
                Throw New ApplicationException("Exception in isGuid", ex)
            End Try
            Return returnValue
        End Function
        '
        '====================================================================================================
        '
        Public Function vbInstr(string1 As String, string2 As String, compare As CompareMethod) As Integer
            Return vbInstr(1, string1, string2, compare)
        End Function
        '
        Public Function vbInstr(string1 As String, string2 As String) As Integer
            Return vbInstr(1, string1, string2, CompareMethod.Binary)
        End Function
        '
        Public Function vbInstr(start As Integer, string1 As String, string2 As String) As Integer
            Return vbInstr(start, string1, string2, CompareMethod.Binary)
        End Function
        '
        Public Function vbInstr(start As Integer, string1 As String, string2 As String, compare As CompareMethod) As Integer
            If (String.IsNullOrEmpty(string1)) Then
                Return 0
            Else
                If (start < 1) Then
                    Throw New ArgumentException("Instr() start must be > 0.")
                Else
                    If (compare = CompareMethod.Binary) Then
                        Return string1.IndexOf(string2, start - 1, StringComparison.CurrentCulture) + 1
                    Else
                        Return string1.IndexOf(string2, start - 1, StringComparison.CurrentCultureIgnoreCase) + 1
                    End If
                End If
            End If
        End Function
        '
        '====================================================================================================
        '
        Public Function vbReplace(expression As String, find As String, replacement As String) As String
            Return vbReplace(expression, find, replacement, 1, 9999, CompareMethod.Binary)
        End Function
        '
        Public Function vbReplace(expression As String, find As String, replacement As String, startIgnore As Integer, countIgnore As Integer, compare As CompareMethod) As String
            If String.IsNullOrEmpty(expression) Then
                Return expression
            ElseIf String.IsNullOrEmpty(find) Then
                Return expression
            Else
                If compare = CompareMethod.Binary Then
                    Return expression.Replace(find, replacement)
                ElseIf String.IsNullOrEmpty(replacement) Then
                    Return Regex.Replace(expression, find, "", RegexOptions.IgnoreCase)
                Else
                    Return Regex.Replace(expression, find, replacement, RegexOptions.IgnoreCase)
                End If
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic UCase
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function vbUCase(source As String) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.ToUpper
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic LCase
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function vbLCase(source As String) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.ToLower
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' visual basic Left()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Function vbLeft(source As String, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Right()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Function vbRight(source As String, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(source.Length - length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Len()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Public Function vbLen(source As String) As Integer
            If (String.IsNullOrEmpty(source)) Then
                Return 0
            Else
                Return source.Length
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Mid()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="startIndex"></param>
        ''' <returns></returns>
        Public Function vbMid(source As String, startIndex As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(startIndex)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' Visual Basic Mid()
        ''' </summary>
        ''' <param name="source"></param>
        ''' <param name="startIndex"></param>
        ''' <param name="length"></param>
        ''' <returns></returns>
        Public Function vbMid(source As String, startIndex As Integer, length As Integer) As String
            If (String.IsNullOrEmpty(source)) Then
                Return ""
            Else
                Return source.Substring(startIndex, length)
            End If
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' replacement for visual basic isNumeric
        ''' </summary>
        ''' <param name="Expression"></param>
        ''' <returns></returns>
        Public Function vbIsNumeric(Expression As Object) As Boolean
            If (TypeOf Expression Is DateTime) Then
                Return False
            ElseIf (Expression Is Nothing) Then
                Return False
            ElseIf (TypeOf Expression Is Integer) Or (TypeOf Expression Is Int16) Or (TypeOf Expression Is Int32) Or (TypeOf Expression Is Int64) Or (TypeOf Expression Is Decimal) Or (TypeOf Expression Is Single) Or (TypeOf Expression Is Double) Or (TypeOf Expression Is Boolean) Then
                Return True
            ElseIf (TypeOf Expression Is String) Then
                Dim output As Double
                Return Double.TryParse(DirectCast(Expression, String), output)
            Else
                Return False
            End If
        End Function
    End Module

End Namespace
