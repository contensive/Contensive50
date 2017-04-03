
//using Microsoft.VisualBasic;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;


//using System.Runtime.InteropServices;
//using System.Net;
//using Xunit;
//using System.Text.RegularExpressions;
////
//namespace Contensive.Core
//{
//    //
//    public static class taskQueueCommandEnumModule
//    {
//        public const string runAddon = "runaddon";
//    }
//    //
//    public class cmdDetailClass
//    {
//        public int addonId;
//        public string addonName;
//        public Dictionary<string, string> docProperties;
//    }
//    //
//    //====================================================================================================
//    //
//    public class docPropertiesClass
//    {
//        public string Name;
//        public string Value;
//        public string NameValue;
//        public bool IsForm;
//        public bool IsFile;
//        public byte[] FileContent;
//        public string tmpPrivatefile;
//        public int FileSize;
//        public string fileType;
//    }
//    //
//    //====================================================================================================
//    /// <summary>
//    /// application configuration class
//    /// </summary>
//    public class Models.Entity.serverConfigModel.appConfigModel
//    {
//        public string name;
//        public bool enabled;
//        // rename hashKey
//        public string privateKey;
//        public string defaultConnectionString;
//        // path relative to clusterPhysicalPath
//        public string appRootFilesPath;
//        // path relative to clusterPhysicalPath
//        public string cdnFilesPath;
//        // path relative to clusterPhysicalPath
//        public string privateFilesPath;
//        // in some cases (like legacy), cdnFiles are iis virtual folder mapped to appRoot (/files/). Some cases this is a URL (http:\\cdn.domain.com pointing to s3)
//        public string cdnFilesNetprefix;
//        public bool allowSiteMonitor;
//        // primary domain is the first item in the list
//        public List<string> domainList = new List<string>();
//        public bool enableCache;
//        // The url pathpath that executes the addon site
//        public string adminRoute;
//    }
//    //
//    //
//    //====================================================================================================
//    /// <summary>
//    /// Holds location on the server of the clusterConfig file. Physically stored at programDataFolder/clib/serverConfig.json
//    /// </summary>
//    public class serverConfigClass
//    {
//        public string clusterPath;
//        public bool allowTaskRunnerService;
//        public bool allowTaskSchedulerService;
//    }

//    //
//    //====================================================================================================
//    /// <summary>
//    /// cluster configuration class - deserialized configration file
//    /// </summary>
//    /// <remarks></remarks>
//    public class clusterConfigClass
//    {
//        public bool isLocal = true;
//        public string name = "";
//        //
//        // local caching using dotnet framework, flushes on appPool
//        //
//        public bool isLocalCache = false;
//        //
//        // AWS dotnet elaticcache client wraps enyim, and provides node autodiscovery through the configuration object.
//        // this is the srver:port to the config file it uses.
//        //
//        public string awsElastiCacheConfigurationEndpoint;
//        //
//        // datasource for the cluster
//        //
//        public dataSourceTypeEnum defaultDataSourceType;
//        //
//        // odbc
//        //
//        public string defaultDataSourceODBCConnectionString;
//        //
//        // native
//        //
//        public string defaultDataSourceAddress = "";
//        //
//        // user for creating new databases, and creating the new user for the database during site create, and saved to appconfig
//        //
//        public string defaultDataSourceUsername = "";
//        public string defaultDataSourcePassword = "";
//        //
//        // endpoint for cluster files (not sure how it works, maybe this will be an object taht includes permissions, for now an fpo)
//        //
//        public string clusterFilesEndpoint;
//        //
//        // configuration of async command listener on render machines (not sure if used still)
//        //
//        public int serverListenerPort = coreCommonModule.Port_ContentServerControlDefault;
//        public int maxConcurrentTasksPerServer = 5;
//        // ayncCmd server authentication -- change this to a key later
//        public string username = "";
//        public string password = "";
//        //
//        // This is the root path to the localCluster files, typically getLocalDataFolder (d:\inetpub)
//        //   if isLocal, the cluster runs from these files
//        //   if not, this is the local mirror of the cluster files
//        //
//        public string clusterPhysicalPath;
//        //
//        //Public domainRoutes As Dictionary(Of String, String)
//        //
//        public string appPattern;
//        //
//        //
//        //
//        public Dictionary<string, Models.Entity.serverConfigModel.appConfigModel> apps = new Dictionary<string, Models.Entity.serverConfigModel.appConfigModel>();
//    }
//    //
//    //====================================================================================================
//    /// <summary>
//    /// miniCollection - This is an old collection object used in part to load the cdef part xml files. REFACTOR this into CollectionWantList and werialization into jscon
//    /// </summary>
//    public class MiniCollectionClass : ICloneable
//    {
//        //
//        // Content Definitions (some data in CDef, some in the CDef extension)
//        //
//        // true only for the one collection created from the base file. This property does not transfer during addSrcToDst
//        public bool isBaseCollection;
//        public Dictionary<string, coreMetaDataClass.CDefClass> CDef = new Dictionary<string, coreMetaDataClass.CDefClass>();
//        public int SQLIndexCnt;
//        public SQLIndexType[] SQLIndexes;
//        public struct SQLIndexType
//        {
//            public string DataSourceName;
//            public string TableName;
//            public string IndexName;
//            public string FieldNameList;
//            public bool dataChanged;
//        }
//        public int MenuCnt;
//        public MenusType[] Menus;
//        public struct MenusType
//        {
//            public string Name;
//            public bool IsNavigator;
//            public string menuNameSpace;
//            public string ParentName;
//            public string ContentName;
//            public string LinkPage;
//            public string SortOrder;
//            public bool AdminOnly;
//            public bool DeveloperOnly;
//            public bool NewWindow;
//            public bool Active;
//            public string AddonName;
//            public bool dataChanged;
//            public string Guid;
//            public string NavIconType;
//            public string NavIconTitle;
//            public string Key;
//        }
//        public AddOnType[] AddOns;
//        public struct AddOnType
//        {
//            public string Name;
//            public string Link;
//            public string ObjectProgramID;
//            public string ArgumentList;
//            public string SortOrder;
//            public string Copy;
//            public bool dataChanged;
//        }
//        public int AddOnCnt;
//        public StyleType[] Styles;
//        public struct StyleType
//        {
//            public string Name;
//            public bool Overwrite;
//            public string Copy;
//            public bool dataChanged;
//        }
//        public int StyleCnt;
//        public string StyleSheet;
//        public int ImportCnt;
//        public ImportCollectionType[] collectionImports;
//        public struct ImportCollectionType
//        {
//            public string Name;
//            public string Guid;
//        }
//        public int PageTemplateCnt;
//        public PageTemplateType[] PageTemplates;
//        //   Page Template - started, but CDef2 and LoadDataToCDef are all that is done do far
//        public struct PageTemplateType
//        {
//            public string Name;
//            public string Copy;
//            public string Guid;
//            public string Style;
//        }
//        public object Clone()
//        {
//            return this.MemberwiseClone;
//        }
//    }
//    //
//    // ---------------------------------------------------------------------------------------------------
//    // ----- DataSourceType
//    //       class not structure because it has to marshall to vb6
//    // ---------------------------------------------------------------------------------------------------
//    //
//    public class dataSourceClass
//    {
//        public string NameLower;
//        public int Id;
//        public dataSourceTypeEnum dataSourceType;
//        public string endPoint;
//        public string username;
//        public string password;
//        public string odbcConnectionString;
//    }
//    //
//    public enum dataSourceTypeEnum
//    {
//        sqlServerOdbc = 1,
//        sqlServerNative = 2,
//        mySqlNative = 3
//    }
//    //------------------------------------------------------------------------
//    // Moved here from ccCommon so it could be used in argument to csv_getStyleSheet2
//    //------------------------------------------------------------------------
//    //
//    // renamed for now because error : ambiguous name if pubic is csv and legacycmc.csv_
//    // csv class holds the real public version.
//    // later, rename backfile

//    public static class coreCommonModule
//    {
//        //
//        public const sqlAddonStyles = "select addonid,styleid from ccSharedStylesAddonRules where (active<>0) order by id";
//        //
//        public const cacheNameAddonStyleRules = "addon styles";
//        //
//        public const bool ALLOWLEGACYAPI = false;
//        public const bool ALLOWPROFILING = false;
//        //
//        // put content definitions here
//        //
//        //
//        public struct NameValuePairType
//        {
//            public string Name;
//            public string Value;
//        }
//        //
//        //========================================================================
//        //   defined errors (event log eventId)
//        //       1000-1999 Contensive
//        //       2000-2999 Datatree
//        //
//        //   see kmaErrorDescription() for transations
//        //========================================================================
//        //
//        const Error_DataTree_RootNodeNext = 2000;
//        const Error_DataTree_NoGoNext = 2001;
//        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        //
//        //========================================================================
//        //
//        //========================================================================
//        //
//        public static extern int GetTickCount();
//        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        public static extern int GetCurrentProcessId();
//        //Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
//        //
//        //========================================================================
//        //   Declarations for SetPiorityClass
//        //========================================================================
//        //
//        //Const THREAD_BASE_PRIORITY_IDLE = -15
//        //Const THREAD_BASE_PRIORITY_LOWRT = 15
//        //Const THREAD_BASE_PRIORITY_MIN = -2
//        //Const THREAD_BASE_PRIORITY_MAX = 2
//        //Const THREAD_PRIORITY_LOWEST = THREAD_BASE_PRIORITY_MIN
//        //Const THREAD_PRIORITY_HIGHEST = THREAD_BASE_PRIORITY_MAX
//        //Const THREAD_PRIORITY_BELOW_NORMAL = (THREAD_PRIORITY_LOWEST + 1)
//        //Const THREAD_PRIORITY_ABOVE_NORMAL = (THREAD_PRIORITY_HIGHEST - 1)
//        //Const THREAD_PRIORITY_IDLE = THREAD_BASE_PRIORITY_IDLE
//        //Const THREAD_PRIORITY_NORMAL = 0
//        //Const THREAD_PRIORITY_TIME_CRITICAL = THREAD_BASE_PRIORITY_LOWRT
//        //Const HIGH_PRIORITY_CLASS = &H80
//        //Const IDLE_PRIORITY_CLASS = &H40
//        //Const NORMAL_PRIORITY_CLASS = &H20
//        //Const REALTIME_PRIORITY_CLASS = &H100
//        //
//        //Private Declare Function SetThreadPriority Lib "kernel32" (ByVal hThread As Integer, ByVal nPriority As Integer) As Integer
//        //Private Declare Function SetPriorityClass Lib "kernel32" (ByVal hProcess As Integer, ByVal dwPriorityClass As Integer) As Integer
//        //Private Declare Function GetThreadPriority Lib "kernel32" (ByVal hThread As Integer) As Integer
//        //Private Declare Function GetPriorityClass Lib "kernel32" (ByVal hProcess As Integer) As Integer
//        //Private Declare Function GetCurrentThread Lib "kernel32" () As Integer
//        //Private Declare Function GetCurrentProcess Lib "kernel32" () As Integer
//        //

//        //
//        //========================================================================
//        //Converts unsafe characters,
//        //such as spaces, into their
//        //corresponding escape sequences.
//        //========================================================================
//        //
//        //Declare Function UrlEscape Lib "shlwapi" _
//        //   Alias "UrlEscapeA" _
//        //  (ByVal pszURL As String, _
//        //   ByVal pszEscaped As String, _
//        //   ByVal pcchEscaped As Integer, _
//        //   ByVal dwFlags As Integer) As Integer
//        //'
//        //'Converts escape sequences back into
//        //'ordinary characters.
//        //'
//        //Declare Function UrlUnescape Lib "shlwapi" _
//        //   Alias "UrlUnescapeA" _
//        //  (ByVal pszURL As String, _
//        //   ByVal pszUnescaped As String, _
//        //   ByVal pcchUnescaped As Integer, _
//        //   ByVal dwFlags As Integer) As Integer

//        //
//        //   Error reporting strategy
//        //       Popups are pop-up boxes that tell the user what to do
//        //       Logs are files with error details for developers to use debugging
//        //
//        //       Attended Programs
//        //           - errors that do not effect the operation, resume next
//        //           - all errors trickle up to the user interface level
//        //           - User Errors, like file not found, return "UserError" code and a description
//        //           - Internal Errors, like div-by-0, User should see no detail, log gets everything
//        //           - Dependant Object Errors, codes that return from objects:
//        //               - If UserError, translate ErrSource for raise, but log all original info
//        //               - If InternalError, log info and raise InternalError
//        //               - If you can not tell, call it InternalError
//        //
//        //       UnattendedMode
//        //           The same, except each routine decides when
//        //
//        //       When an error happens in-line (bad condition without a raise)
//        //           Log the error
//        //           Raise the appropriate Code/Description in the current Source
//        //
//        //       When an ErrorTrap occurs
//        //           If ErrSource is not AppTitle, it is a dependantObjectError, log and translate code
//        //           If ErrNumber is not an ObjectError, call it internal error, log and translate code
//        //           Error must be either "InternalError" or "UserError", just raise it again
//        //
//        // old - If an error is raised that is not a KmaCode, it is logged and translated
//        // old - If an error is raised and the soure is not he current "dll", it is logged and translated
//        //
//        // Base on which Internal errors should start
//        public const ignoreInteger = Constants.vbObjectError;
//        //
//        //Public Const KmaError_UnderlyingObject = vbObjectError + 1     ' An error occurec in an underlying object
//        //Public Const KmaccErrorServiceStopped = vbObjectError + 2       ' The service is not running
//        //Public Const KmaError_BadObject = vbObjectError + 3            ' The Server Pointer is not valid
//        //Public Const KmaError_UpgradeInProgress = vbObjectError + 4    ' page is blocked because an upgrade is in progress
//        //Public Const KmaError_InvalidArgument = vbObjectError + 5      ' and input argument is not valid. Put details at end of description
//        //
//        // Generic Error code that passes the description back to the user
//        public const ignoreInteger = ignoreInteger + 16;
//        // Internal error which the user should not see
//        public const ignoreInteger = ignoreInteger + 17;
//        // Error from the page which called Contensive
//        public const KmaErrorPage = ignoreInteger + 18;
//        //
//        // Internal error which the user should not see
//        public const KmaObjectError = ignoreInteger + 256;
//        //
//        public const SQLTrue = "1";
//        public const SQLFalse = "0";
//        //
//        public const System.DateTime dateMinValue = 12 / 30 / 1899 12:00:00 AM;
//		//
//		//
//		public const kmaEndTable = "</table >";
//        public const kmaEndTableCell = "</td>";
//        public const kmaEndTableRow = "</tr>";
//        //
//        public enum csv_contentTypeEnum
//        {
//            contentTypeWeb = 1,
//            contentTypeEmail = 2,
//            contentTypeWebTemplate = 3,
//            contentTypeEmailTemplate = 4
//        }
//        //'
//        //' ---------------------------------------------------------------------------------------------------
//        //' ----- CDefAdminColumnType
//        //' ---------------------------------------------------------------------------------------------------
//        //'
//        //Public Structure cdefServices.CDefAdminColumnType
//        //    Public Name As String
//        //    Public FieldPointer As Integer
//        //    Public Width As Integer
//        //    Public SortPriority As Integer
//        //    Public SortDirection As Integer
//        //End Structure
//        //'
//        //' ---------------------------------------------------------------------------------------------------
//        //' ----- CDefFieldType
//        //'       class not structure because it has to marshall to vb6
//        //' ---------------------------------------------------------------------------------------------------
//        //'
//        //Public Structure cdefServices.CDefFieldType
//        //    Public Name As String                      ' The name of the field
//        //    Public ValueVariant As Object             ' The value carried to and from the database
//        //    Public Id As Integer                          ' the ID in the ccContentFields Table that this came from
//        //    Public active As Boolean                   ' if the field is available in the admin area
//        //    Public fieldType As Integer                   ' The type of data the field holds
//        //    Public Caption As String                   ' The caption for displaying the field
//        //    Public ReadOnlyField As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
//        //    Public NotEditable As Boolean              ' if true, you can only edit new records
//        //    Public LookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
//        //    Public Required As Boolean                 ' if true, this field must be entered
//        //    Public DefaultValueObject As Object      ' default value on a new record
//        //    Public HelpMessage As String               ' explaination of this field
//        //    Public UniqueName As Boolean               '
//        //    Public TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
//        //    Public Password As Boolean                 ' for text boxes, sets the password attribute
//        //    Public RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
//        //    Public RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
//        //    Public RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
//        //    Public IndexColumn As Integer                 ' the column desired in the admin index form
//        //    Public IndexWidth As String                ' either number or percentage
//        //    Public IndexSortOrder As String            ' alpha sort on index page
//        //    Public IndexSortDirection As Integer          ' 1 sorts forward, -1 backward
//        //    Public Changed As Boolean                  ' if true, field value needs to be saved to database
//        //    Public AdminOnly As Boolean                ' This field is only available to administrators
//        //    Public DeveloperOnly As Boolean            ' This field is only available to administrators
//        //    Public BlockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
//        //    '   false - custom field, is not altered during upgrade, Help message comes from the local record
//        //    '   true - upgrade modifies the field definition, help message comes from support.contensive.com
//        //    Public htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
//        //    Public Authorable As Boolean               ' true if it can be seen in the admin form
//        //    Public Inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
//        //    Public ContentID As Integer                   ' This is the ID of the Content Def that defines these properties
//        //    Public EditSortPriority As Integer            ' The Admin Edit Sort Order
//        //    Public ManyToManyContentID As Integer         ' Content containing Secondary Records
//        //    Public ManyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
//        //    Public ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
//        //    Public ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
//        //    '
//        //    ' - new
//        //    '
//        //    Public RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
//        //    Public RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
//        //    Public EditTab As String                   ' Editing group - used for the tabs
//        //    Public Scramble As Boolean                 ' save the field scrambled in the Db
//        //    Public MemberSelectGroupID As Integer         ' If the Type is TypeMemberSelect, this is the group that the member will be selected from
//        //    Public LookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
//        //End Structure
//        //'
//        //' ---------------------------------------------------------------------------------------------------
//        //' ----- CDefType
//        //'       class not structure because it has to marshall to vb6
//        //' ---------------------------------------------------------------------------------------------------
//        //'
//        //Public Structure cdefServices.CDefType
//        //    Public Name As String                       ' Name of Content
//        //    Public Id As Integer                           ' index in content table
//        //    Public ContentTableName As String           ' the name of the content table
//        //    Public ContentDataSourceName As String      '
//        //    Public AuthoringTableName As String         ' the name of the authoring table
//        //    Public AuthoringDataSourceName As String    '
//        //    Public AllowAdd As Boolean                  ' Allow adding records
//        //    Public AllowDelete As Boolean               ' Allow deleting records
//        //    Public WhereClause As String                ' Used to filter records in the admin area
//        //    Public ParentID As Integer                     ' if not IgnoreContentControl, the ID of the parent content
//        //    Public ChildIDList As String                ' Comma separated list of child content definition IDs
//        //    Public ChildPointerList As String           ' Comma separated list of child content definition Pointers
//        //    Public DefaultSortMethod As String          ' FieldName Direction, ....
//        //    Public ActiveOnly As Boolean                ' When true
//        //    Public AdminOnly As Boolean                 ' Only allow administrators to modify content
//        //    Public DeveloperOnly As Boolean             ' Only allow developers to modify content
//        //    Public AllowWorkflowAuthoring As Boolean    ' if true, treat this content with authoring proceses
//        //    Public DropDownFieldList As String          ' String used to populate select boxes
//        //    Public SelectFieldList As String            ' Field list used in OpenCSContent calls (all active field definitions)
//        //    Public EditorGroupName As String            ' Group of members who administer Workflow Authoring
//        //    '
//        //    ' array of cdefFieldType throws a vb6 error, data or method problem
//        //    ' public property does not work (msn article -- very slow because it marshals he entire array, not a pointer )
//        //    ' public function works to read, but cannot write
//        //    ' possible -- public fields() function for reads
//        //    '   -- public setFields() function for writes
//        //    '
//        //    Public fields as appServices_CdefClass.CDefFieldType()
//        //    Public FieldPointer As Integer                 ' Current Field for FirstField / NextField calls
//        //    Public FieldCount As Integer                   ' The number of fields loaded (from ccFields, or Set calls)
//        //    Public FieldSize As Integer                    ' The number of initialize Field()
//        //    '
//        //    ' same as fields
//        //    '
//        //    Public adminColumns as appServices_CdefClass.CDefAdminColumnType()
//        //    'Public AdminColumnLocal As CDefAdminColumnType()  ' The Admins in this content
//        //    Public AdminColumnPointer As Integer                 ' Current Admin for FirstAdminColumn / NextAdminColumn calls
//        //    Public AdminColumnCount As Integer                   ' The number of AdminColumns loaded (from ccAdminColumns, or Set calls)
//        //    Public AdminColumnSize As Integer                    ' The number of initialize Admin()s
//        //    'AdminColumn As CDefAdminCollClass
//        //    '
//        //    Public ContentControlCriteria As String     ' String created from ParentIDs used to select records
//        //    '
//        //    ' ----- future
//        //    '
//        //    Public IgnoreContentControl As Boolean     ' if true, all records in the source are in this content
//        //    Public AliasName As String                 ' Field Name of the required "name" field
//        //    Public AliasID As String                   ' Field Name of the required "id" field
//        //    '
//        //    ' ----- removed
//        //    '
//        //    Public SingleRecord As Boolean             ' removeme
//        //    '    Type as integer                        ' removeme
//        //    Public TableName As String                 ' removeme
//        //    Public DataSourceID As Integer                ' removeme
//        //    '
//        //    ' ----- new
//        //    '
//        //    Public AllowTopicRules As Boolean          ' For admin edit page
//        //    Public AllowContentTracking As Boolean     ' For admin edit page
//        //    Public AllowCalendarEvents As Boolean      ' For admin edit page
//        //    Public AllowMetaContent As Boolean         ' For admin edit page - Adds the Meta Content Section
//        //    Public TimeStamp As String                 ' string that changes if any record in Content Definition changes, in memory only
//        //End Structure


//        //Private Type AdminColumnType
//        //    FieldPointer as integer
//        //    FieldWidth as integer
//        //    FieldSortDirection as integer
//        //End Type
//        //
//        //Private Type AdminType
//        //    ColumnCount as integer
//        //    Columns() As AdminColumnType
//        //End Type
//        //
//        //-----------------------------------------------------------------------
//        //   Messages
//        //-----------------------------------------------------------------------
//        //
//        public const Msg_AuthoringDeleted = "<b>Record Deleted</b><br>" + SpanClassAdminSmall + "This record was deleted and will be removed when publishing is complete.</SPAN>";
//        public const Msg_AuthoringInserted = "<b>Record Added</b><br>" + SpanClassAdminSmall + "This record was added and will display when publishing is complete.</span>";
//        public const Msg_EditLock = "<b>Edit Locked</b><br>" + SpanClassAdminSmall + "This record is currently being edited by <EDITNAME>.<br>" + "This lock will be released when the user releases the record, or at <EDITEXPIRES> (about <EDITEXPIRESMINUTES> minutes).</span>";
//        public const Msg_WorkflowDisabled = "<b>Immediate Authoring</b><br>" + SpanClassAdminSmall + "Changes made will be reflected on the web site immediately.</span>";
//        public const Msg_ContentWorkflowDisabled = "<b>Immediate Authoring Content Definition</b><br>" + SpanClassAdminSmall + "Changes made will be reflected on the web site immediately.</span>";
//        public const Msg_AuthoringRecordNotModifed = "" + SpanClassAdminSmall + "No changes have been saved to this record.</span>";
//        public const Msg_AuthoringRecordModifed = "<b>Edits Pending</b><br>" + SpanClassAdminSmall + "This record has been edited by <EDITNAME>.<br>" + "To publish these edits, submit for publishing, or have an administrator 'Publish Changes'.</span>";
//        public const Msg_AuthoringRecordModifedAdmin = "<b>Edits Pending</b><br>" + SpanClassAdminSmall + "This record has been edited by <EDITNAME>.<br>" + "To publish these edits immediately, hit 'Publish Changes'.<br>" + "To submit these changes for workflow publishing, hit 'Submit for Publishing'.</span>";
//        public const Msg_AuthoringSubmitted = "<b>Edits Submitted for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and was submitted for publishing by <EDITNAME>.</span>";
//        public const Msg_AuthoringSubmittedAdmin = "<b>Edits Submitted for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and was submitted for publishing by <EDITNAME>.<br>" + "As an administrator, you can make changes to this submitted record.<br>" + "To publish these edits immediately, hit 'Publish Changes'.<br>" + "To deny these edits, hit 'Abort Edits'.<br>" + "To approve these edits for workflow publishing, hit 'Approve for Publishing'." + "</span>";
//        public const Msg_AuthoringApproved = "<b>Edits Approved for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and approved for publishing.<br>" + "No further changes can be made to this record until an administrator publishs, or aborts publishing." + "</span>";
//        public const Msg_AuthoringApprovedAdmin = "<b>Edits Approved for Publishing</b><br>" + SpanClassAdminSmall + "This record has been edited and approved for publishing.<br>" + "No further changes can be made to this record until an administrator publishs, or aborts publishing.<br>" + "To publish these edits immediately, hit 'Publish Changes'.<br>" + "To deny these edits, hit 'Abort Edits'." + "</span>";
//        public const Msg_AuthoringSubmittedNotification = "The following Content has been submitted for publication. Instructions on how to publish this content to web site are at the bottom of this message.<br>" + "<br>" + "website: <DOMAINNAME><br>" + "Name: <RECORDNAME><br>" + "<br>" + "Content: <CONTENTNAME><br>" + "Record Number: <RECORDID><br>" + "<br>" + "Submitted: <SUBMITTEDDATE><br>" + "Submitted By: <SUBMITTEDNAME><br>" + "<br>" + "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because you are a member of the Editors Group for the content that has been changed.</p>" + "<p>To publish this content immediately, click on the website link above, check off this record in the box to the far left and click the \"Publish Selected Records\" button.</p>" + "<p>To edit the record, click the \"edit\" link for this record, make the desired changes and click the \"Publish\" button.</p>";
//        //& "<p>This content has been modified and was submitted for publication by the individual shown above. You were sent this notification because your account on this system is a member of the Editors Group for the content that has been changed.</p>" _
//        //& "<p>To publish this content immediately, click on the website link above and locate this record in the list of modified records presented. his content has been modified and was submitted for publication by the individual shown above. Click the 'Admin Site' link to edit the record, and hit the publish button.</p>" _
//        //& "<p>To approved this record for workflow publishing, locate the record as described above, and hit the 'Approve for Publishing' button.</p>" _
//        //& "<p>To publish all content records approved, go to the Workflow Publishing Screen (on the Administration Menu or the Administration Site) and hit the 'Publish Approved Records' button.</p>"
//        public const PageNotAvailable_Msg = "This page is not currently available. <br>" + "Please use your back button to return to the previous page. <br>";
//        public const NewPage_Msg = "";
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant to an long (long)
//        //    '   returns 0 if the input is not an integer
//        //    '   if float, rounds to integer
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeInteger(ExpressionVariant As Object) As Integer
//        //        ' 7/14/2009 - cover the overflow case, return 0
//        //        On Error Resume Next
//        //        '
//        //        If Not IsArray(ExpressionVariant) Then
//        //            If Not IsMissing(ExpressionVariant) Then
//        //                If Not IsNull(ExpressionVariant) Then
//        //                    If ExpressionVariant <> "" Then
//        //                        If vbIsNumeric(ExpressionVariant) Then
//        //                            encodeInteger = CLng(ExpressionVariant)
//        //                        End If
//        //                    End If
//        //                End If
//        //            End If
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeInteger = 0
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant to a number (double)
//        //    '   returns 0 if the input is not a number
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeNumber(ExpressionVariant As Object) As Double
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        'encodeNumber = 0
//        //        If Not IsMissing(ExpressionVariant) Then
//        //            If Not IsNull(ExpressionVariant) Then
//        //                If ExpressionVariant <> "" Then
//        //                    If vbIsNumeric(ExpressionVariant) Then
//        //                        encodeNumber = ExpressionVariant
//        //                    End If
//        //                End If
//        //            End If
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeNumber = 0
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant to a date
//        //    '   returns 0 if the input is not a number
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeDate(ExpressionVariant As Object) As Date
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        '    encodeDate = CDate(ExpressionVariant)
//        //        '    encodeDate = CDate("1/1/1980")
//        //        'encodeDate = Date.MinValue
//        //        If Not IsMissing(ExpressionVariant) Then
//        //            If Not IsNull(ExpressionVariant) Then
//        //                If ExpressionVariant <> "" Then
//        //                    If IsDate(ExpressionVariant) Then
//        //                        encodeDate = ExpressionVariant
//        //                    End If
//        //                End If
//        //            End If
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeDate = Date.MinValue
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant to a boolean
//        //    '   Returns true if input is not false, else false
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeBoolean(ExpressionVariant As Object) As Boolean
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        'encodeBoolean = False
//        //        If Not IsMissing(ExpressionVariant) Then
//        //            If Not IsNull(ExpressionVariant) Then
//        //                If ExpressionVariant <> "" Then
//        //                    If vbIsNumeric(ExpressionVariant) Then
//        //                        If ExpressionVariant <> "0" Then
//        //                            If ExpressionVariant <> 0 Then
//        //                                encodeBoolean = True
//        //                            End If
//        //                        End If
//        //                    ElseIf vbUCase(ExpressionVariant) = "ON" Then
//        //                        encodeBoolean = True
//        //                    ElseIf vbUCase(ExpressionVariant) = "YES" Then
//        //                        encodeBoolean = True
//        //                    ElseIf vbUCase(ExpressionVariant) = "TRUE" Then
//        //                        encodeBoolean = True
//        //                    Else
//        //                        encodeBoolean = False
//        //                    End If
//        //                End If
//        //            End If
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeBoolean = False
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant into 0 or 1
//        //    '   Returns 1 if input is not false, else 0
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeBit(ExpressionVariant As Object) As Integer
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        'encodeBit = 0
//        //        If encodeBoolean(ExpressionVariant) Then
//        //            encodeBit = 1
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeBit = 0
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Convert a variant to a string
//        //    '   returns emptystring if the input is not a string
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeText(ExpressionVariant As Object) As String
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        'encodeText = ""
//        //        If Not IsMissing(ExpressionVariant) Then
//        //            If Not IsNull(ExpressionVariant) Then
//        //                encodeText = CStr(ExpressionVariant)
//        //            End If
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //        encodeText = ""
//        //    End Function
//        //    '
//        //    '==========================================================================
//        //    '   Converts a possibly missing value to variant
//        //    '==========================================================================
//        //    '
//        //    Public Function encodeMissingText(ExpressionVariant As Object, DefaultVariant As Object) As Object
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        If IsMissing(ExpressionVariant) Then
//        //            encodeMissing = DefaultVariant
//        //        Else
//        //            encodeMissing = ExpressionVariant
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //    End Function
//        //
//        //
//        //
//        public static string encodeEmptyText(string sourceText, string DefaultText)
//        {
//            string returnText = sourceText;
//            if ((string.IsNullOrEmpty(returnText)))
//            {
//                returnText = DefaultText;
//            }
//            return returnText;
//        }
//        //
//        //
//        //
//        public static int encodeEmptyInteger(string sourceText, int DefaultInteger)
//        {
//            return EncodeInteger(encodeEmptyText(sourceText, DefaultInteger.ToString));
//        }
//        //
//        //
//        //
//        public static System.DateTime encodeEmptyDate(string sourceText, System.DateTime DefaultDate)
//        {
//            return EncodeDate(encodeEmptyText(sourceText, DefaultDate.ToString));
//        }
//        //
//        //
//        //
//        public static double encodeEmptyNumber(string sourceText, double DefaultNumber)
//        {
//            return EncodeNumber(encodeEmptyText(sourceText, DefaultNumber.ToString));
//        }
//        //
//        //
//        //
//        public static bool encodeEmptyBoolean(string sourceText, bool DefaultState)
//        {
//            return EncodeBoolean(encodeEmptyText(sourceText, DefaultState.ToString));
//        }
//        //    '
//        //    '================================================================================================================
//        //    '   Separate a URL into its host, path, page parts
//        //    '================================================================================================================
//        //    '
//        //    Public Sub SeparateURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        '   Divide the URL into URLHost, URLPath, and URLPage
//        //        '
//        //        Dim WorkingURL As String
//        //        Dim Position As Integer
//        //        '
//        //        ' Get Protocol (before the first :)
//        //        '
//        //        WorkingURL = SourceURL
//        //        Position = vbInstr(1, WorkingURL, ":")
//        //        'Position = vbInstr(1, WorkingURL, "://")
//        //        If Position <> 0 Then
//        //            Protocol = Mid(WorkingURL, 1, Position + 2)
//        //            WorkingURL = Mid(WorkingURL, Position + 3)
//        //        End If
//        //        '
//        //        ' compatibility fix
//        //        '
//        //        If vbInstr(1, WorkingURL, "//") = 1 Then
//        //            If Protocol = "" Then
//        //                Protocol = "http:"
//        //            End If
//        //            Protocol = Protocol & "//"
//        //            WorkingURL = Mid(WorkingURL, 3)
//        //        End If
//        //        '
//        //        ' Get QueryString
//        //        '
//        //        Position = vbInstr(1, WorkingURL, "?")
//        //        If Position > 0 Then
//        //            QueryString = Mid(WorkingURL, Position)
//        //            WorkingURL = Mid(WorkingURL, 1, Position - 1)
//        //        End If
//        //        '
//        //        ' separate host from pathpage
//        //        '
//        //        'iURLHost = WorkingURL
//        //        Position = vbInstr(WorkingURL, "/")
//        //        If (Position = 0) And (Protocol = "") Then
//        //            '
//        //            ' Page without path or host
//        //            '
//        //            Page = WorkingURL
//        //            Path = ""
//        //            Host = ""
//        //        ElseIf (Position = 0) Then
//        //            '
//        //            ' host, without path or page
//        //            '
//        //            Page = ""
//        //            Path = "/"
//        //            Host = WorkingURL
//        //        Else
//        //            '
//        //            ' host with a path (at least)
//        //            '
//        //            Path = Mid(WorkingURL, Position)
//        //            Host = Mid(WorkingURL, 1, Position - 1)
//        //            '
//        //            ' separate page from path
//        //            '
//        //            Position = InStrRev(Path, "/")
//        //            If Position = 0 Then
//        //                '
//        //                ' no path, just a page
//        //                '
//        //                Page = Path
//        //                Path = "/"
//        //            Else
//        //                Page = Mid(Path, Position + 1)
//        //                Path = Mid(Path, 1, Position)
//        //            End If
//        //        End If
//        //        Exit Sub
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //    End Sub
//        //    '
//        //    '================================================================================================================
//        //    '   Separate a URL into its host, path, page parts
//        //    '================================================================================================================
//        //    '
//        //    Public Sub ParseURL(ByVal SourceURL As String, ByRef Protocol As String, ByRef Host As String, ByRef Port As String, ByRef Path As String, ByRef Page As String, ByRef QueryString As String)
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        '   Divide the URL into URLHost, URLPath, and URLPage
//        //        '
//        //        Dim iURLWorking As String               ' internal storage for GetURL functions
//        //        Dim iURLProtocol As String
//        //        Dim iURLHost As String
//        //        Dim iURLPort As String
//        //        Dim iURLPath As String
//        //        Dim iURLPage As String
//        //        Dim iURLQueryString As String
//        //        Dim Position As Integer
//        //        '
//        //        iURLWorking = SourceURL
//        //        Position = vbInstr(1, iURLWorking, "://")
//        //        If Position <> 0 Then
//        //            iURLProtocol = Mid(iURLWorking, 1, Position + 2)
//        //            iURLWorking = Mid(iURLWorking, Position + 3)
//        //        End If
//        //        '
//        //        ' separate Host:Port from pathpage
//        //        '
//        //        iURLHost = iURLWorking
//        //        Position = vbInstr(iURLHost, "/")
//        //        If Position = 0 Then
//        //            '
//        //            ' just host, no path or page
//        //            '
//        //            iURLPath = "/"
//        //            iURLPage = ""
//        //        Else
//        //            iURLPath = Mid(iURLHost, Position)
//        //            iURLHost = Mid(iURLHost, 1, Position - 1)
//        //            '
//        //            ' separate page from path
//        //            '
//        //            Position = InStrRev(iURLPath, "/")
//        //            If Position = 0 Then
//        //                '
//        //                ' no path, just a page
//        //                '
//        //                iURLPage = iURLPath
//        //                iURLPath = "/"
//        //            Else
//        //                iURLPage = Mid(iURLPath, Position + 1)
//        //                iURLPath = Mid(iURLPath, 1, Position)
//        //            End If
//        //        End If
//        //        '
//        //        ' Divide Host from Port
//        //        '
//        //        Position = vbInstr(iURLHost, ":")
//        //        If Position = 0 Then
//        //            '
//        //            ' host not given, take a guess
//        //            '
//        //            Select Case vbUCase(iURLProtocol)
//        //                Case "FTP://"
//        //                    iURLPort = "21"
//        //                Case "HTTP://", "HTTPS://"
//        //                    iURLPort = "80"
//        //                Case Else
//        //                    iURLPort = "80"
//        //            End Select
//        //        Else
//        //            iURLPort = Mid(iURLHost, Position + 1)
//        //            iURLHost = Mid(iURLHost, 1, Position - 1)
//        //        End If
//        //        Position = vbInstr(1, iURLPage, "?")
//        //        If Position > 0 Then
//        //            iURLQueryString = Mid(iURLPage, Position)
//        //            iURLPage = Mid(iURLPage, 1, Position - 1)
//        //        End If
//        //        Protocol = iURLProtocol
//        //        Host = iURLHost
//        //        Port = iURLPort
//        //        Path = iURLPath
//        //        Page = iURLPage
//        //        QueryString = iURLQueryString
//        //        Exit Sub
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //        Err.Clear()
//        //    End Sub
//        //    '
//        //    '
//        //    '
//        //    Function DecodeGMTDate(GMTDate As String) As Date
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        Dim WorkString As String
//        //        DecodeGMTDate = 0
//        //        If GMTDate <> "" Then
//        //            WorkString = Mid(GMTDate, 6, 11)
//        //            If IsDate(WorkString) Then
//        //                DecodeGMTDate = CDate(WorkString)
//        //                WorkString = Mid(GMTDate, 18, 8)
//        //                If IsDate(WorkString) Then
//        //                    DecodeGMTDate = DecodeGMTDate + CDate(WorkString) + 4 / 24
//        //                End If
//        //            End If
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //    Function EncodeGMTDate(MSDate As Date) As String
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        Dim WorkString As String
//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '   Renamed to catch all the cases that used it in addons
//        //    '
//        //    '   Do not use this routine in Addons to get the addon option string value
//        //    '   to get the value in an option string, use cmc.csv_getAddonOption("name")
//        //    '
//        //    ' Get the value of a name in a string of name value pairs parsed with vrlf and =
//        //    '   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
//        //    '   new format is "name1=value1 crlf name2=value2 crlf ..."
//        //    '   There can be no extra spaces between the delimiter, the name and the "="
//        //    '=================================================================================
//        //    '
//        //    Function getSimpleNameValue(Name As String, ArgumentString As String, DefaultValue As String, Delimiter As String) As String
//        //        'Function getArgument(Name As String, ArgumentString As String, Optional DefaultValue as object, Optional Delimiter As String) As String
//        //        '
//        //        Dim WorkingString As String
//        //        Dim iDefaultValue As String
//        //        Dim NameLength As Integer
//        //        Dim ValueStart As Integer
//        //        Dim ValueEnd As Integer
//        //        Dim IsQuoted As Boolean
//        //        '
//        //        ' determine delimiter
//        //        '
//        //        If Delimiter = "" Then
//        //            '
//        //            ' If not explicit
//        //            '
//        //            If vbInstr(1, ArgumentString, vbCrLf) <> 0 Then
//        //                '
//        //                ' crlf can only be here if it is the delimiter
//        //                '
//        //                Delimiter = vbCrLf
//        //            Else
//        //                '
//        //                ' either only one option, or it is the legacy '&' delimit
//        //                '
//        //                Delimiter = "&"
//        //            End If
//        //        End If
//        //        iDefaultValue = encodeMissingText(DefaultValue, "")
//        //        WorkingString = ArgumentString
//        //        getSimpleNameValue = iDefaultValue
//        //        If WorkingString <> "" Then
//        //            WorkingString = Delimiter & WorkingString & Delimiter
//        //            ValueStart = vbInstr(1, WorkingString, Delimiter & Name & "=", vbTextCompare)
//        //            If ValueStart <> 0 Then
//        //                NameLength = Len(Name)
//        //                ValueStart = ValueStart + Len(Delimiter) + NameLength + 1
//        //                If Mid(WorkingString, ValueStart, 1) = """" Then
//        //                    IsQuoted = True
//        //                    ValueStart = ValueStart + 1
//        //                End If
//        //                If IsQuoted Then
//        //                    ValueEnd = vbInstr(ValueStart, WorkingString, """" & Delimiter)
//        //                Else
//        //                    ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter)
//        //                End If
//        //                If ValueEnd = 0 Then
//        //                    getSimpleNameValue = Mid(WorkingString, ValueStart)
//        //                Else
//        //                    getSimpleNameValue = Mid(WorkingString, ValueStart, ValueEnd - ValueStart)
//        //                End If
//        //            End If
//        //        End If
//        //        '

//        //        Exit Function
//        //        '
//        //        ' ----- ErrorTrap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '   Do not use this code
//        //    '
//        //    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
//        //    '
//        //    '   This was left here to work through any code issues that might arrise during
//        //    '   the conversion.
//        //    '
//        //    '   Return the value from a name value pair, parsed with =,&[|].
//        //    '   For example:
//        //    '       name=Jay[Jay|Josh|Dwayne]
//        //    '       the answer is Jay. If a select box is displayed, it is a dropdown of all three
//        //    '=================================================================================
//        //    '
//        //    Public Function GetAggrOption_old(Name As String, SegmentCMDArgs As String) As String
//        //        '
//        //        Dim Pos As Integer
//        //        '
//        //        GetAggrOption_old = getSimpleNameValue(Name, SegmentCMDArgs, "", vbCrLf)
//        //        '
//        //        ' remove the manual select list syntax "answer[choice1|choice2]"
//        //        '
//        //        Pos = vbInstr(1, GetAggrOption_old, "[")
//        //        If Pos <> 0 Then
//        //            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
//        //        End If
//        //        '
//        //        ' remove any function syntax "answer{selectcontentname RSS Feeds}"
//        //        '
//        //        Pos = vbInstr(1, GetAggrOption_old, "{")
//        //        If Pos <> 0 Then
//        //            GetAggrOption_old = Left(GetAggrOption_old, Pos - 1)
//        //        End If
//        //        '
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '   Do not use this code
//        //    '
//        //    '   To retrieve a value from an option string, use cmc.csv_getAddonOption("name")
//        //    '
//        //    '   This was left here to work through any code issues that might arrise during
//        //    '   Compatibility for GetArgument
//        //    '=================================================================================
//        //    '
//        //Function getNameValue_old(Name As String, ArgumentString As String, Optional DefaultValue as string = "") As String
//        //        getNameValue_old = getSimpleNameValue(Name, ArgumentString, DefaultValue, vbCrLf)
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   encodeSQLText
//        //    '========================================================================
//        //    '
//        //    Public Function encodeSQLText(ExpressionVariant As Object) As String
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        'Dim MethodName As String
//        //        '
//        //        'MethodName = "encodeSQLText"
//        //        '
//        //        If IsNull(ExpressionVariant) Then
//        //            encodeSQLText = "null"
//        //        ElseIf IsMissing(ExpressionVariant) Then
//        //            encodeSQLText = "null"
//        //        ElseIf ExpressionVariant = "" Then
//        //            encodeSQLText = "null"
//        //        Else
//        //            encodeSQLText = CStr(ExpressionVariant)
//        //            ' ??? this should not be here -- to correct a field used in a CDef, truncate in SaveCS by fieldtype
//        //            'encodeSQLText = Left(ExpressionVariant, 255)
//        //            'remove-can not find a case where | is not allowed to be saved.
//        //            'encodeSQLText = vbReplace(encodeSQLText, "|", "_")
//        //            encodeSQLText = "'" & vbReplace(encodeSQLText, "'", "''") & "'"
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- Error Trap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   encodeSQLLongText
//        //    '========================================================================
//        //    '
//        //    Public Function encodeSQLLongText(ExpressionVariant As Object) As String
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        'Dim MethodName As String
//        //        '
//        //        'MethodName = "encodeSQLLongText"
//        //        '
//        //        If IsNull(ExpressionVariant) Then
//        //            encodeSQLLongText = "null"
//        //        ElseIf IsMissing(ExpressionVariant) Then
//        //            encodeSQLLongText = "null"
//        //        ElseIf ExpressionVariant = "" Then
//        //            encodeSQLLongText = "null"
//        //        Else
//        //            encodeSQLLongText = ExpressionVariant
//        //            'encodeSQLLongText = vbReplace(ExpressionVariant, "|", "_")
//        //            encodeSQLLongText = "'" & vbReplace(encodeSQLLongText, "'", "''") & "'"
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- Error Trap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   encodeSQLDate
//        //    '       encode a date variable to go in an sql expression
//        //    '========================================================================
//        //    '
//        //    Public Function encodeSQLDate(ExpressionVariant As Object) As String
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        Dim TimeVar As Date
//        //        Dim TimeValuething As Single
//        //        Dim TimeHours As Integer
//        //        Dim TimeMinutes As Integer
//        //        Dim TimeSeconds As Integer
//        //        'Dim MethodName As String
//        //        ''
//        //        'MethodName = "encodeSQLDate"
//        //        '
//        //        If IsNull(ExpressionVariant) Then
//        //            encodeSQLDate = "null"
//        //        ElseIf IsMissing(ExpressionVariant) Then
//        //            encodeSQLDate = "null"
//        //        ElseIf ExpressionVariant = "" Then
//        //            encodeSQLDate = "null"
//        //        ElseIf IsDate(ExpressionVariant) Then
//        //            TimeVar = CDate(ExpressionVariant)
//        //            If TimeVar = 0 Then
//        //                encodeSQLDate = "null"
//        //            Else
//        //                TimeValuething = 86400.0! * (TimeVar - Int(TimeVar + 0.000011!))
//        //                TimeHours = Int(TimeValuething / 3600.0!)
//        //                If TimeHours >= 24 Then
//        //                    TimeHours = 23
//        //                End If
//        //                TimeMinutes = Int(TimeValuething / 60.0!) - (TimeHours * 60)
//        //                If TimeMinutes >= 60 Then
//        //                    TimeMinutes = 59
//        //                End If
//        //                TimeSeconds = TimeValuething - (TimeHours * 3600.0!) - (TimeMinutes * 60.0!)
//        //                If TimeSeconds >= 60 Then
//        //                    TimeSeconds = 59
//        //                End If
//        //                encodeSQLDate = "{ts '" & Year(ExpressionVariant) & "-" & Right("0" & Month(ExpressionVariant), 2) & "-" & Right("0" & Day(ExpressionVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2) & "'}"
//        //            End If
//        //        Else
//        //            encodeSQLDate = "null"
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- Error Trap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   encodeSQLNumber
//        //    '       encode a number variable to go in an sql expression
//        //    '========================================================================
//        //    '
//        //    Function encodeSQLNumber(ExpressionVariant As Object) As String
//        //        'On Error GoTo ErrorTrap
//        //        '
//        //        'Dim MethodName As String
//        //        ''
//        //        'MethodName = "encodeSQLNumber"
//        //        '
//        //        If IsNull(ExpressionVariant) Then
//        //            encodeSQLNumber = "null"
//        //        ElseIf IsMissing(ExpressionVariant) Then
//        //            encodeSQLNumber = "null"
//        //        ElseIf ExpressionVariant = "" Then
//        //            encodeSQLNumber = "null"
//        //        ElseIf vbIsNumeric(ExpressionVariant) Then
//        //            Select Case VarType(ExpressionVariant)
//        //                Case vbBoolean
//        //                    If ExpressionVariant Then
//        //                        encodeSQLNumber = SQLTrue
//        //                    Else
//        //                        encodeSQLNumber = SQLFalse
//        //                    End If
//        //                Case Else
//        //                    encodeSQLNumber = ExpressionVariant
//        //            End Select
//        //        Else
//        //            encodeSQLNumber = "null"
//        //        End If
//        //        Exit Function
//        //        '
//        //        ' ----- Error Trap
//        //        '
//        //ErrorTrap:
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   encodeSQLBoolean
//        //    '       encode a boolean variable to go in an sql expression
//        //    '========================================================================
//        //    '
//        //    Public Function encodeSQLBoolean(ExpressionVariant As Object) As String
//        //        '
//        //        Dim src As String
//        //        '
//        //        encodeSQLBoolean = SQLFalse
//        //        If encodeBoolean(ExpressionVariant) Then
//        //            encodeSQLBoolean = SQLTrue
//        //        End If
//        //        '    If Not IsNull(ExpressionVariant) Then
//        //        '        If Not IsMissing(ExpressionVariant) Then
//        //        '            If ExpressionVariant <> False Then
//        //        '                    encodeSQLBoolean = SQLTrue
//        //        '                End If
//        //        '            End If
//        //        '        End If
//        //        '    End If
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    '   Gets the next line from a string, and removes the line
//        //    '========================================================================
//        //    '
//        //    Public Function getLine(Body As String) As String
//        //        Dim EOL As String
//        //        Dim NextCR As Integer
//        //        Dim NextLF As Integer
//        //        Dim BOL As Integer
//        //        '
//        //        NextCR = vbInstr(1, Body, vbCr)
//        //        NextLF = vbInstr(1, Body, vbLf)

//        //        If NextCR <> 0 Or NextLF <> 0 Then
//        //            If NextCR <> 0 Then
//        //                If NextLF <> 0 Then
//        //                    If NextCR < NextLF Then
//        //                        EOL = NextCR - 1
//        //                        If NextLF = NextCR + 1 Then
//        //                            BOL = NextLF + 1
//        //                        Else
//        //                            BOL = NextCR + 1
//        //                        End If

//        //                    Else
//        //                        EOL = NextLF - 1
//        //                        BOL = NextLF + 1
//        //                    End If
//        //                Else
//        //                    EOL = NextCR - 1
//        //                    BOL = NextCR + 1
//        //                End If
//        //            Else
//        //                EOL = NextLF - 1
//        //                BOL = NextLF + 1
//        //            End If
//        //            getLine = Mid(Body, 1, EOL)
//        //            Body = Mid(Body, BOL)
//        //        Else
//        //            getLine = Body
//        //            Body = ""
//        //        End If

//        //        'EOL = vbInstr(1, Body, vbCrLf)

//        //        'If EOL <> 0 Then
//        //        '    getLine = Mid(Body, 1, EOL - 1)
//        //        '    Body = Mid(Body, EOL + 2)
//        //        '    End If
//        //        '
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '   Get a Random Long Value
//        //    '=================================================================================
//        //    '
//        //    Public Function GetRandomInteger() As Integer
//        //        '
//        //        Dim RandomBase As Integer
//        //        Dim RandomLimit As Integer
//        //        '
//        //        RandomBase =Threading.Thread.CurrentThread.ManagedThreadId
//        //        RandomBase = RandomBase And ((2 ^ 30) - 1)
//        //        RandomLimit = (2 ^ 31) - RandomBase - 1
//        //        Randomize()
//        //        GetRandomInteger = RandomBase + (Rnd * RandomLimit)
//        //        '
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '
//        //    '=================================================================================
//        //    '
//        //    Public Function isDataTableOk(RS As Object) As Boolean
//        //        isDataTableOk = False
//        //        If (isDataTableOk(rs)) Then
//        //            If true Then
//        //                If Not rs.rows.count=0 Then
//        //                    isDataTableOk = True
//        //                End If
//        //            End If
//        //        End If
//        //    End Function
//        //    '
//        //    '=================================================================================
//        //    '
//        //    '=================================================================================
//        //    '
//        //    Public Sub closeDataTable(RS As Object)
//        //        If (isDataTableOk(rs)) Then
//        //            If true Then
//        //                Call 'RS.Close()
//        //            End If
//        //        End If
//        //    End Sub
//        //
//        //=============================================================================
//        // Create the part of the sql where clause that is modified by the user
//        //   WorkingQuery is the original querystring to change
//        //   QueryName is the name part of the name pair to change
//        //   If the QueryName is not found in the string, ignore call
//        //=============================================================================
//        //
//        public static string ModifyQueryString(string WorkingQuery, string QueryName, string QueryValue, bool AddIfMissing = true)
//        {
//            string functionReturnValue = null;
//            //
//            if (WorkingQuery.IndexOf("?") > 0)
//            {
//                functionReturnValue = modifyLinkQuery(WorkingQuery, QueryName, QueryValue, AddIfMissing);
//            }
//            else {
//                functionReturnValue = Strings.Mid(modifyLinkQuery("?" + WorkingQuery, QueryName, QueryValue, AddIfMissing), 2);
//            }
//            return functionReturnValue;
//        }
//        //
//        public static string ModifyQueryString(string WorkingQuery, string QueryName, int QueryValue, bool AddIfMissing = true)
//        {
//            return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing);
//        }
//        //
//        public static string ModifyQueryString(string WorkingQuery, string QueryName, bool QueryValue, bool AddIfMissing = true)
//        {
//            return ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString, AddIfMissing);
//        }
//        //
//        //=============================================================================
//        /// <summary>
//        /// Modify the querystring at the end of a link. If there is no, question mark, the link argument is assumed to be a link, not the querysting
//        /// </summary>
//        /// <param name="Link"></param>
//        /// <param name="QueryName"></param>
//        /// <param name="QueryValue"></param>
//        /// <param name="AddIfMissing"></param>
//        /// <returns></returns>
//        public static string modifyLinkQuery(string Link, string QueryName, string QueryValue, bool AddIfMissing = true)
//        {
//            string functionReturnValue = null;
//            try
//            {
//                string[] Element = {

//                };
//                int ElementCount = 0;
//                int ElementPointer = 0;
//                string[] NameValue = null;
//                string UcaseQueryName = null;
//                bool ElementFound = false;
//                bool iAddIfMissing = false;
//                string QueryString = null;
//                //
//                iAddIfMissing = AddIfMissing;
//                if (vbInstr(1, Link, "?") != 0)
//                {
//                    functionReturnValue = Strings.Mid(Link, 1, vbInstr(1, Link, "?") - 1);
//                    QueryString = Strings.Mid(Link, Strings.Len(modifyLinkQuery()) + 2);
//                }
//                else {
//                    functionReturnValue = Link;
//                    QueryString = "";
//                }
//                UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName));
//                if (!string.IsNullOrEmpty(QueryString))
//                {
//                    Element = Strings.Split(QueryString, "&");
//                    ElementCount = Information.UBound(Element) + 1;
//                    for (ElementPointer = 0; ElementPointer <= ElementCount - 1; ElementPointer++)
//                    {
//                        NameValue = Strings.Split(Element(ElementPointer), "=");
//                        if (Information.UBound(NameValue) == 1)
//                        {
//                            if (vbUCase(NameValue(0)) == UcaseQueryName)
//                            {
//                                if (string.IsNullOrEmpty(QueryValue))
//                                {
//                                    Element(ElementPointer) = "";
//                                }
//                                else {
//                                    Element(ElementPointer) = QueryName + "=" + QueryValue;
//                                }
//                                ElementFound = true;
//                                break; // TODO: might not be correct. Was : Exit For
//                            }
//                        }
//                    }
//                }
//                if (!ElementFound & (!string.IsNullOrEmpty(QueryValue)))
//                {
//                    //
//                    // element not found, it needs to be added
//                    //
//                    if (iAddIfMissing)
//                    {
//                        if (string.IsNullOrEmpty(QueryString))
//                        {
//                            QueryString = EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
//                        }
//                        else {
//                            QueryString = QueryString + "&" + EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
//                        }
//                    }
//                }
//                else {
//                    //
//                    // element found
//                    //
//                    QueryString = Strings.Join(Element, "&");
//                    if ((!string.IsNullOrEmpty(QueryString)) & (string.IsNullOrEmpty(QueryValue)))
//                    {
//                        //
//                        // element found and needs to be removed
//                        //
//                        QueryString = vbReplace(QueryString, "&&", "&");
//                        if (Strings.Left(QueryString, 1) == "&")
//                        {
//                            QueryString = Strings.Mid(QueryString, 2);
//                        }
//                        if (Strings.Right(QueryString, 1) == "&")
//                        {
//                            QueryString = Strings.Mid(QueryString, 1, Strings.Len(QueryString) - 1);
//                        }
//                    }
//                }
//                if ((!string.IsNullOrEmpty(QueryString)))
//                {
//                    functionReturnValue = functionReturnValue + "?" + QueryString;
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("Exception in modifyLinkQuery", ex);
//            }
//            return functionReturnValue;
//            //
//        }
//        //    '
//        //    '=================================================================================
//        //    '
//        //    '=================================================================================
//        //    '
//        //    Public Function GetIntegerString(Value As Integer, DigitCount As Integer) As String
//        //        If Len(Value) <= DigitCount Then
//        //        GetIntegerString = String(DigitCount - Len(CStr(Value)), "0") & CStr(Value)
//        //        Else
//        //            GetIntegerString = CStr(Value)
//        //        End If
//        //    End Function
//        //    '
//        //    '==========================================================================================
//        //    '   the current process to a high priority
//        //    '       Should be called once from the objects parent when it is first created.
//        //    '
//        //    '   taken from an example labeled
//        //    '       KPD-Team 2000
//        //    '       URL: http://www.allapi.net/
//        //    '       Email: KPDTeam@Allapi.net
//        //    '==========================================================================================
//        //    '
//        //    Public Sub SetProcessHighPriority()
//        //        Dim hProcess As Integer
//        //        '
//        //        'set the new priority class
//        //        '
//        //        hProcess = GetCurrentProcess
//        //        Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
//        //        '
//        //    End Sub
//        //    '
//        //    '==========================================================================================
//        //    '   Format the current error object into a standard string
//        //    '==========================================================================================
//        //    '
//        //Public Function GetErrString(Optional ErrorObject As Object) As String
//        //        Dim Copy As String
//        //        If ErrorObject Is Nothing Then
//        //            If Err.Number = 0 Then
//        //                GetErrString = "[no error]"
//        //            Else
//        //                Copy = Err.Description
//        //                Copy = vbReplace(Copy, vbCrLf, "-")
//        //                Copy = vbReplace(Copy, vbLf, "-")
//        //                Copy = vbReplace(Copy, vbCrLf, "")
//        //                GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
//        //            End If
//        //        Else
//        //            If ErrorObject.Number = 0 Then
//        //                GetErrString = "[no error]"
//        //            Else
//        //                Copy = ErrorObject.Description
//        //                Copy = vbReplace(Copy, vbCrLf, "-")
//        //                Copy = vbReplace(Copy, vbLf, "-")
//        //                Copy = vbReplace(Copy, vbCrLf, "")
//        //                GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
//        //            End If
//        //        End If
//        //        '
//        //    End Function
//        //    '
//        //    '==========================================================================================
//        //    '   Format the current error object into a standard string
//        //    '==========================================================================================
//        //    '
//        //    Public Function GetProcessID() As Integer
//        //        GetProcessID = GetCurrentProcessId
//        //    End Function
//        //    '
//        //    '==========================================================================================
//        //    '   Test if a test string is in a delimited string
//        //    '==========================================================================================
//        //    '
//        //    Public Function IsInDelimitedString(DelimitedString As String, TestString As String, Delimiter As String) As Boolean
//        //        IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' encodeURL
//        //    '
//        //    '   Encodes only what is to the left of the first ?
//        //    '   All URL path characters are assumed to be correct (/:#)
//        //    '========================================================================
//        //    '
//        //    Function encodeURL(Source As String) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim URLSplit() As String
//        //        Dim LeftSide As String
//        //        Dim RightSide As String
//        //        '
//        //        encodeURL = Source
//        //        If Source <> "" Then
//        //            URLSplit = Split(Source, "?")
//        //            encodeURL = URLSplit(0)
//        //            encodeURL = vbReplace(encodeURL, "%", "%25")
//        //            '
//        //            encodeURL = vbReplace(encodeURL, """", "%22")
//        //            encodeURL = vbReplace(encodeURL, " ", "%20")
//        //            encodeURL = vbReplace(encodeURL, "$", "%24")
//        //            encodeURL = vbReplace(encodeURL, "+", "%2B")
//        //            encodeURL = vbReplace(encodeURL, ",", "%2C")
//        //            encodeURL = vbReplace(encodeURL, ";", "%3B")
//        //            encodeURL = vbReplace(encodeURL, "<", "%3C")
//        //            encodeURL = vbReplace(encodeURL, "=", "%3D")
//        //            encodeURL = vbReplace(encodeURL, ">", "%3E")
//        //            encodeURL = vbReplace(encodeURL, "@", "%40")
//        //            If UBound(URLSplit) > 0 Then
//        //                encodeURL = encodeURL & "?" & encodeQueryString(URLSplit(1))
//        //            End If
//        //        End If
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' encodeQueryString
//        //    '
//        //    '   This routine encodes the URL QueryString to conform to rules
//        //    '========================================================================
//        //    '
//        //    Function encodeQueryString(Source As String) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim QSSplit() As String
//        //        Dim QSPointer As Integer
//        //        Dim NVSplit() As String
//        //        Dim NV As String
//        //        '
//        //        encodeQueryString = ""
//        //        If Source <> "" Then
//        //            QSSplit = Split(Source, "&")
//        //            For QSPointer = 0 To UBound(QSSplit)
//        //                NV = QSSplit(QSPointer)
//        //                If NV <> "" Then
//        //                    NVSplit = Split(NV, "=")
//        //                    If UBound(NVSplit) = 0 Then
//        //                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
//        //                        encodeQueryString = encodeQueryString & "&" & NVSplit(0)
//        //                    Else
//        //                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
//        //                        NVSplit(1) = encodeRequestVariable(NVSplit(1))
//        //                        encodeQueryString = encodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
//        //                    End If
//        //                End If
//        //            Next
//        //            If encodeQueryString <> "" Then
//        //                encodeQueryString = Mid(encodeQueryString, 2)
//        //            End If
//        //        End If
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' encodeRequestVariable
//        //    '
//        //    '   This routine encodes a request variable for a URL Query String
//        //    '       ...can be the requestname or the requestvalue
//        //    '========================================================================
//        //    '
//        //    Function encodeRequestVariable(Source As String) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim SourcePointer As Integer
//        //        Dim Character As String
//        //        Dim LocalSource As String
//        //        '
//        //        If Source <> "" Then
//        //            LocalSource = Source
//        //            ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
//        //            'LocalSource = vbReplace(LocalSource, " ", "+")
//        //            For SourcePointer = 1 To Len(LocalSource)
//        //                Character = Mid(LocalSource, SourcePointer, 1)
//        //                ' "%" added so if this is called twice, it will not destroy "%20" values
//        //                'If Character = " " Then
//        //                '    encodeRequestVariable = encodeRequestVariable & "+"
//        //                If vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
//        //                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
//        //                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
//        //                    encodeRequestVariable = encodeRequestVariable & Character
//        //                Else
//        //                    encodeRequestVariable = encodeRequestVariable & "%" & Hex(Asc(Character))
//        //                End If
//        //            Next
//        //        End If
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' encodeHTML
//        //    '
//        //    '   Convert all characters that are not allowed in HTML to their Text equivalent
//        //    '   in preperation for use on an HTML page
//        //    '========================================================================
//        //    '
//        //    Function encodeHTML(Source As String) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        encodeHTML = Source
//        //        encodeHTML = vbReplace(encodeHTML, "&", "&amp;")
//        //        encodeHTML = vbReplace(encodeHTML, "<", "&lt;")
//        //        encodeHTML = vbReplace(encodeHTML, ">", "&gt;")
//        //        encodeHTML = vbReplace(encodeHTML, """", "&quot;")
//        //        encodeHTML = vbReplace(encodeHTML, "'", "&#39;")
//        //        'encodeHTML = vbReplace(encodeHTML, "'", "&apos;")
//        //        '
//        //    End Function
//        //
//        //========================================================================
//        // decodeHtml
//        //
//        //   Convert HTML equivalent characters to their equivalents
//        //========================================================================
//        //
//        public static string decodeHtml(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            int Pos = 0;
//            string s = null;
//            string CharCodeString = null;
//            int CharCode = 0;
//            int posEnd = 0;
//            //
//            // 11/26/2009 - basically re-wrote it, I commented the old one out below
//            //
//            functionReturnValue = "";
//            if (!string.IsNullOrEmpty(Source))
//            {
//                s = Source;
//                //
//                Pos = Strings.Len(s);
//                Pos = Strings.InStrRev(s, "&#", Pos);
//                while (Pos != 0)
//                {
//                    CharCodeString = "";
//                    if (Strings.Mid(s, Pos + 3, 1) == ";")
//                    {
//                        CharCodeString = Strings.Mid(s, Pos + 2, 1);
//                        posEnd = Pos + 4;
//                    }
//                    else if (Strings.Mid(s, Pos + 4, 1) == ";")
//                    {
//                        CharCodeString = Strings.Mid(s, Pos + 2, 2);
//                        posEnd = Pos + 5;
//                    }
//                    else if (Strings.Mid(s, Pos + 5, 1) == ";")
//                    {
//                        CharCodeString = Strings.Mid(s, Pos + 2, 3);
//                        posEnd = Pos + 6;
//                    }
//                    if (!string.IsNullOrEmpty(CharCodeString))
//                    {
//                        if (vbIsNumeric(CharCodeString))
//                        {
//                            CharCode = EncodeInteger(CharCodeString);
//                            s = Strings.Mid(s, 1, Pos - 1) + Strings.Chr(CharCode) + Strings.Mid(s, posEnd);
//                        }
//                    }
//                    //
//                    Pos = Strings.InStrRev(s, "&#", Pos);
//                }
//                //
//                // replace out all common names (at least the most common for now)
//                //
//                s = vbReplace(s, "&lt;", "<");
//                s = vbReplace(s, "&gt;", ">");
//                s = vbReplace(s, "&quot;", "\"");
//                s = vbReplace(s, "&apos;", "'");
//                //
//                // Always replace the amp last
//                //
//                s = vbReplace(s, "&amp;", "&");
//                //
//                functionReturnValue = s;
//            }
//            return functionReturnValue;
//            // pre-11/26/2009
//            //decodeHtml = Source
//            //decodeHtml = vbReplace(decodeHtml, "&amp;", "&")
//            //decodeHtml = vbReplace(decodeHtml, "&lt;", "<")
//            //decodeHtml = vbReplace(decodeHtml, "&gt;", ">")
//            //decodeHtml = vbReplace(decodeHtml, "&quot;", """")
//            //decodeHtml = vbReplace(decodeHtml, "&nbsp;", " ")
//            //
//        }
//        //    '
//        //    '   Indent every line by 1 tab
//        //    '
//        public static string kmaIndent(string Source)
//        {
//            string functionReturnValue = null;
//            int posStart = 0;
//            int posEnd = 0;
//            string pre = null;
//            string post = null;
//            string target = null;
//            //
//            posStart = vbInstr(1, Source, "<![CDATA[", CompareMethod.Text);
//            if (posStart == 0)
//            {
//                //
//                // no cdata
//                //
//                posStart = vbInstr(1, Source, "<textarea", CompareMethod.Text);
//                if (posStart == 0)
//                {
//                    //
//                    // no textarea
//                    //
//                    functionReturnValue = vbReplace(Source, Constants.vbCrLf + Constants.vbTab, Constants.vbCrLf + Constants.vbTab + Constants.vbTab);
//                }
//                else {
//                    //
//                    // text area found, isolate it and indent before and after
//                    //
//                    posEnd = vbInstr(posStart, Source, "</textarea>", CompareMethod.Text);
//                    pre = Strings.Mid(Source, 1, posStart - 1);
//                    if (posEnd == 0)
//                    {
//                        target = Strings.Mid(Source, posStart);
//                        post = "";
//                    }
//                    else {
//                        target = Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("</textarea>"));
//                        post = Strings.Mid(Source, posEnd + Strings.Len("</textarea>"));
//                    }
//                    functionReturnValue = kmaIndent(pre) + target + kmaIndent(post);
//                }
//            }
//            else {
//                //
//                // cdata found, isolate it and indent before and after
//                //
//                posEnd = vbInstr(posStart, Source, "]]>", CompareMethod.Text);
//                pre = Strings.Mid(Source, 1, posStart - 1);
//                if (posEnd == 0)
//                {
//                    target = Strings.Mid(Source, posStart);
//                    post = "";
//                }
//                else {
//                    target = Strings.Mid(Source, posStart, posEnd - posStart + Strings.Len("]]>"));
//                    post = Strings.Mid(Source, posEnd + 3);
//                }
//                functionReturnValue = kmaIndent(pre) + target + kmaIndent(post);
//            }
//            return functionReturnValue;
//            //    kmaIndent = Source
//            //    If vbInstr(1, kmaIndent, "<textarea", vbTextCompare) = 0 Then
//            //        kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
//            //    End If
//        }
//        //
//        //========================================================================================================
//        //Place code in a form module
//        //Add a Command button.
//        //========================================================================================================
//        //
//        public static string kmaByteArrayToString(byte[] Bytes)
//        {
//            return System.Text.UTF8Encoding.ASCII.GetString(Bytes);

//            //Dim iUnicode As Integer, i As Integer, j As Integer

//            //On Error Resume Next
//            //i = UBound(Bytes)

//            //If (i < 1) Then
//            //    'ANSI, just convert to unicode and return
//            //    kmaByteArrayToString = StrConv(Bytes, VbStrConv.vbUnicode)
//            //    Exit Function
//            //End If
//            //i = i + 1

//            //'Examine the first two bytes
//            //CopyMemory(iUnicode, Bytes(0), 2)

//            //If iUnicode = Bytes(0) Then 'Unicode
//            //    'Account for terminating null
//            //    If (i Mod 2) Then i = i - 1
//            //    'Set up a buffer to recieve the string
//            //    kmaByteArrayToString = String$(i / 2, 0)
//            //    'Copy to string
//            //    CopyMemory ByVal StrPtr(kmaByteArrayToString), Bytes(0), i
//            //Else 'ANSI
//            //    kmaByteArrayToString = StrConv(Bytes, vbUnicode)
//            //End If

//        }
//        //    '
//        //    '========================================================================================================
//        //    '
//        //    '========================================================================================================
//        //    '
//        //    Public Function kmaStringToByteArray(strInput As String, _
//        //                                    Optional bReturnAsUnicode As Boolean = True, _
//        //                                    Optional bAddNullTerminator As Boolean = False) As Byte()

//        //        Dim lRet As Integer
//        //        Dim bytBuffer() As Byte
//        //        Dim lLenB As Integer

//        //        If bReturnAsUnicode Then
//        //            'Number of bytes
//        //            lLenB = LenB(strInput)
//        //            'Resize buffer, do we want terminating null?
//        //            If bAddNullTerminator Then
//        //                ReDim bytBuffer(lLenB)
//        //            Else
//        //                ReDim bytBuffer(lLenB - 1)
//        //            End If
//        //            'Copy characters from string to byte array
//        //        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB
//        //        Else
//        //            'METHOD ONE
//        //            '        'Get rid of embedded nulls
//        //            '        strRet = StrConv(strInput, vbFromUnicode)
//        //            '        lLenB = LenB(strRet)
//        //            '        If bAddNullTerminator Then
//        //            '            ReDim bytBuffer(lLenB)
//        //            '        Else
//        //            '            ReDim bytBuffer(lLenB - 1)
//        //            '        End If
//        //            '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB

//        //            'METHOD TWO
//        //            'Num of characters
//        //            lLenB = Len(strInput)
//        //            If bAddNullTerminator Then
//        //                ReDim bytBuffer(lLenB)
//        //            Else
//        //                ReDim bytBuffer(lLenB - 1)
//        //            End If
//        //        lRet = WideCharToMultiByte(CP_ACP, 0&, ByVal StrPtr(strInput), -1, ByVal VarPtr(bytBuffer(0)), lLenB, 0&, 0&)
//        //        End If

//        //        kmaStringToByteArray = bytBuffer

//        //    End Function
//        //    '
//        //    '========================================================================================================
//        //    '   Sample kmaStringToByteArray
//        //    '========================================================================================================
//        //    '
//        //    Private Sub SampleStringToByteArray()
//        //        Dim bAnsi() As Byte
//        //        Dim bUni() As Byte
//        //        Dim str As String
//        //        Dim i As Integer
//        //        '
//        //        str = "Convert"
//        //        bAnsi = kmaStringToByteArray(str, False)
//        //        bUni = kmaStringToByteArray(str)
//        //        '
//        //        For i = 0 To UBound(bAnsi)
//        //            Debug.Print("=" & bAnsi(i))
//        //        Next
//        //        '
//        //        Debug.Print("========")
//        //        '
//        //        For i = 0 To UBound(bUni)
//        //            Debug.Print("=" & bUni(i))
//        //        Next
//        //        '
//        //        Debug.Print("ANSI= " & kmaByteArrayToString(bAnsi))
//        //        Debug.Print("UNICODE= " & kmaByteArrayToString(bUni))
//        //        'Using StrConv to convert a Unicode character array directly
//        //        'will cause the resultant string to have extra embedded nulls
//        //        'reason, StrConv does not know the difference between Unicode and ANSI
//        //        Debug.Print("Resull= " & StrConv(bUni, vbUnicode))
//        //    End Sub
//        //
//        //=======================================================================
//        //   sitepropertyNames
//        //=======================================================================
//        //
//        public const siteproperty_serverPageDefault_name = "serverPageDefault";
//        public const siteproperty_serverPageDefault_defaultValue = "default.aspx";
//        public const string spAllowPageWithoutSectionDisplay = "Allow Page Without Section Display";
//        public const bool spAllowPageWithoutSectionDisplay_default = true;
//        //
//        //=======================================================================
//        //   content replacements
//        //=======================================================================
//        //
//        public const contentReplaceEscapeStart = "{%";
//        public const contentReplaceEscapeEnd = "%}";
//        //
//        public class fieldEditorType
//        {
//            public int fieldId;
//            public int addonid;
//        }
//        //
//        public const protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,EDITSOURCEID,EDITARCHIVE,EDITBLANK,CONTENTCONTROLID";
//        //Public Const protectedContentSetControlFieldList = "ID,CREATEDBY,DATEADDED,MODIFIEDBY,MODIFIEDDATE,EDITSOURCEID,EDITARCHIVE,EDITBLANK"
//        //
//        public const HTMLEditorDefaultCopyStartMark = "<!-- cc -->";
//        public const HTMLEditorDefaultCopyEndMark = "<!-- /cc -->";
//        public const HTMLEditorDefaultCopyNoCr = HTMLEditorDefaultCopyStartMark + "<p><br></p>" + HTMLEditorDefaultCopyEndMark;
//        public const HTMLEditorDefaultCopyNoCr2 = "<p><br></p>";
//        //
//        public const IconWidthHeight = " width=21 height=22 ";
//        //Public Const IconWidthHeight = " width=18 height=22 "
//        //
//        // part of software dist - base cdef plus addons with classes in in core library, plus depenancy on coreCollection
//        public const string baseCollectionGuid = "{7C6601A7-9D52-40A3-9570-774D0D43D758}";
//        // contains core Contensive objects, loaded from Library
//        public const CoreCollectionGuid = "{8DAABAE6-8E45-4CEE-A42C-B02D180E799B}";
//        // exported from application during upgrade
//        public const ApplicationCollectionGuid = "{C58A76E2-248B-4DE8-BF9C-849A960F79C6}";
//        //
//        public const string adminSiteAddonGuid = "{c2de2acf-ca39-4668-b417-aa491e7d8460}";
//        public const adminCommonAddonGuid = "{76E7F79E-489F-4B0F-8EE5-0BAC3E4CD782}";
//        public const DashboardAddonGuid = "{4BA7B4A2-ED6C-46C5-9C7B-8CE251FC8FF5}";
//        public const PersonalizationGuid = "{C82CB8A6-D7B9-4288-97FF-934080F5FC9C}";
//        public const TextBoxGuid = "{7010002E-5371-41F7-9C77-0BBFF1F8B728}";
//        public const ContentBoxGuid = "{E341695F-C444-4E10-9295-9BEEC41874D8}";
//        public const DynamicMenuGuid = "{DB1821B3-F6E4-4766-A46E-48CA6C9E4C6E}";
//        public const ChildListGuid = "{D291F133-AB50-4640-9A9A-18DB68FF363B}";
//        public const DynamicFormGuid = "{8284FA0C-6C9D-43E1-9E57-8E9DD35D2DCC}";
//        public const AddonManagerGuid = "{1DC06F61-1837-419B-AF36-D5CC41E1C9FD}";
//        public const FormWizardGuid = "{2B1384C4-FD0E-4893-B3EA-11C48429382F}";
//        public const ImportWizardGuid = "{37F66F90-C0E0-4EAF-84B1-53E90A5B3B3F}";
//        public const JQueryGuid = "{9C882078-0DAC-48E3-AD4B-CF2AA230DF80}";
//        public const JQueryUIGuid = "{840B9AEF-9470-4599-BD47-7EC0C9298614}";
//        public const ImportProcessAddonGuid = "{5254FAC6-A7A6-4199-8599-0777CC014A13}";
//        public const StructuredDataProcessorGuid = "{65D58FE9-8B76-4490-A2BE-C863B372A6A4}";
//        public const jQueryFancyBoxGuid = "{24C2DBCF-3D84-44B6-A5F7-C2DE7EFCCE3D}";
//        //
//        public const DefaultLandingPageGuid = "{925F4A57-32F7-44D9-9027-A91EF966FB0D}";
//        public const DefaultLandingSectionGuid = "{D882ED77-DB8F-4183-B12C-F83BD616E2E1}";
//        public const DefaultTemplateGuid = "{47BE95E4-5D21-42CC-9193-A343241E2513}";
//        public const DefaultDynamicMenuGuid = "{E8D575B9-54AE-4BF9-93B7-C7E7FE6F2DB3}";
//        //
//        public const fpoContentBox = "{1571E62A-972A-4BFF-A161-5F6075720791}";
//        //
//        public const sfImageExtList = "jpg,jpeg,gif,png";
//        //
//        public const PageChildListInstanceID = "{ChildPageList}";
//        //
//        public const cr = Constants.vbCrLf + Constants.vbTab;
//        public const cr2 = cr + Constants.vbTab;
//        public const cr3 = cr2 + Constants.vbTab;
//        public const cr4 = cr3 + Constants.vbTab;
//        public const cr5 = cr4 + Constants.vbTab;
//        public const cr6 = cr5 + Constants.vbTab;
//        //
//        public const AddonOptionConstructor_BlockNoAjax = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" + Constants.vbCrLf + "css Container id" + Constants.vbCrLf + "css Container class";
//        public const AddonOptionConstructor_Block = "Wrapper=[Default:0|None:-1|ListID(Wrappers)]" + Constants.vbCrLf + "As Ajax=[If Add-on is Ajax:0|Yes:1]" + Constants.vbCrLf + "css Container id" + Constants.vbCrLf + "css Container class";
//        public const AddonOptionConstructor_Inline = "As Ajax=[If Add-on is Ajax:0|Yes:1]" + Constants.vbCrLf + "css Container id" + Constants.vbCrLf + "css Container class";
//        //
//        // Constants used as arguments to SiteBuilderClass.CreateNewSite
//        //
//        public const SiteTypeBaseAsp = 1;
//        public const sitetypebaseaspx = 2;
//        public const SiteTypeDemoAsp = 3;
//        public const SiteTypeBasePhp = 4;
//        //
//        //Public Const AddonNewParse = True
//        //
//        public const AddonOptionConstructor_ForBlockText = "AllowGroups=[listid(groups)]checkbox";
//        public const AddonOptionConstructor_ForBlockTextEnd = "";
//        public const BlockTextStartMarker = "<!-- BLOCKTEXTSTART -->";
//        public const BlockTextEndMarker = "<!-- BLOCKTEXTEND -->";
//        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        //
//        //Private Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Integer)
//        private static extern int GetExitCodeProcess(int hProcess, int lpExitCode);
//        [DllImport("winmm.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        private static extern int timeGetTime();
//        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        private static extern int OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);
//        [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        private static extern int CloseHandle(int hObject);
//        //
//        public const InstallFolderName = "Install";
//        public const DownloadFileRootNode = "collectiondownload";
//        public const CollectionFileRootNode = "collection";
//        public const CollectionFileRootNodeOld = "addoncollection";
//        public const CollectionListRootNode = "collectionlist";
//        //
//        public const LegacyLandingPageName = "Landing Page Content";
//        public const DefaultNewLandingPageName = "Home";
//        public const DefaultLandingSectionName = "Home";
//        //
//        // ----- Errors Specific to the Contensive Objects
//        //
//        public const ignoreInteger = KmaObjectError + 1;
//        public const KmaccErrorServiceStopped = KmaObjectError + 2;
//        //
//        public const UserErrorHeadline = "<p class=\"ccError\">There was a problem with this page.</p>";
//        //
//        // ----- Errors connecting to server
//        //
//        public const ccError_InvalidAppName = 100;
//        public const ccError_ErrorAddingApp = 101;
//        public const ccError_ErrorDeletingApp = 102;
//        // Invalid parameter name
//        public const ccError_InvalidFieldName = 103;
//        public const ignoreString = 104;
//        public const ignoreString = 105;
//        // Attempt to execute a command without a connection
//        public const ccError_NotConnected = 106;
//        //
//        //
//        //
//        public const ccStatusCode_Base = ignoreInteger;
//        public const ccStatusCode_ControllerCreateFailed = ccStatusCode_Base + 1;
//        public const ccStatusCode_ControllerInProcess = ccStatusCode_Base + 2;
//        public const ccStatusCode_ControllerStartedWithoutService = ccStatusCode_Base + 3;
//        //
//        // ----- Previous errors, can be replaced
//        //
//        //Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
//        public const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running.";
//        public const KmaError_BadObject_Msg = "Server Object is not valid.";
//        public const ignoreString = "Server is busy with internal builder.";
//        //
//        //Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
//        //Public Const KmaError_UnderlyingObject_Msg = "An error occurred in an underlying routine."
//        //Public Const KmaccErrorServiceStopped_Msg = "The Contensive CSv Service is not running."
//        //Public Const KmaError_BadObject_Msg = "Server Object is not valid."
//        //Public Const ignoreString = "Server is busy with internal builder."
//        //Public Const KmaError_InvalidArgument_Msg = "Invalid Argument"
//        //
//        //-----------------------------------------------------------------------
//        //   GetApplicationList indexes
//        //-----------------------------------------------------------------------
//        //
//        public const AppList_Name = 0;
//        public const AppList_Status = 1;
//        public const AppList_ConnectionsActive = 2;
//        public const AppList_ConnectionString = 3;
//        public const AppList_DataBuildVersion = 4;
//        public const AppList_LicenseKey = 5;
//        public const AppList_RootPath = 6;
//        public const AppList_PhysicalFilePath = 7;
//        public const AppList_DomainName = 8;
//        public const AppList_DefaultPage = 9;
//        public const AppList_AllowSiteMonitor = 10;
//        public const AppList_HitCounter = 11;
//        public const AppList_ErrorCount = 12;
//        public const AppList_DateStarted = 13;
//        public const AppList_AutoStart = 14;
//        public const AppList_Progress = 15;
//        public const AppList_PhysicalWWWPath = 16;
//        public const AppListCount = 17;
//        //
//        //-----------------------------------------------------------------------
//        //   System MemberID - when the system does an update, it uses this member
//        //-----------------------------------------------------------------------
//        //
//        public const SystemMemberID = 0;
//        //
//        //-----------------------------------------------------------------------
//        // ----- old (OptionKeys for available Options)
//        //-----------------------------------------------------------------------
//        //
//        public const OptionKeyProductionLicense = 0;
//        public const OptionKeyDeveloperLicense = 1;
//        //
//        //-----------------------------------------------------------------------
//        // ----- LicenseTypes, replaced OptionKeys
//        //-----------------------------------------------------------------------
//        //
//        public const LicenseTypeInvalid = -1;
//        public const LicenseTypeProduction = 0;
//        public const LicenseTypeTrial = 1;
//        //
//        //-----------------------------------------------------------------------
//        // ----- Active Content Definitions
//        //-----------------------------------------------------------------------
//        //
//        public const ACTypeDate = "DATE";
//        public const ACTypeVisit = "VISIT";
//        public const ACTypeVisitor = "VISITOR";
//        public const ACTypeMember = "MEMBER";
//        public const ACTypeOrganization = "ORGANIZATION";
//        public const ACTypeChildList = "CHILDLIST";
//        public const ACTypeContact = "CONTACT";
//        public const ACTypeFeedback = "FEEDBACK";
//        public const ACTypeLanguage = "LANGUAGE";
//        public const ACTypeAggregateFunction = "AGGREGATEFUNCTION";
//        public const ACTypeAddon = "ADDON";
//        public const ACTypeImage = "IMAGE";
//        public const ACTypeDownload = "DOWNLOAD";
//        public const ACTypeEnd = "END";
//        public const ACTypeTemplateContent = "CONTENT";
//        public const ACTypeTemplateText = "TEXT";
//        public const ACTypeDynamicMenu = "DYNAMICMENU";
//        public const ACTypeWatchList = "WATCHLIST";
//        public const ACTypeRSSLink = "RSSLINK";
//        public const ACTypePersonalization = "PERSONALIZATION";
//        public const ACTypeDynamicForm = "DYNAMICFORM";
//        //
//        public const ACTagEnd = "<ac type=\"" + ACTypeEnd + "\">";
//        //
//        // ----- PropertyType Definitions
//        //
//        public const PropertyTypeMember = 0;
//        public const PropertyTypeVisit = 1;
//        public const PropertyTypeVisitor = 2;
//        //
//        //-----------------------------------------------------------------------
//        // ----- Port Assignments
//        //-----------------------------------------------------------------------
//        //
//        public const WinsockPortWebOut = 4000;
//        public const WinsockPortServerFromWeb = 4001;
//        public const WinsockPortServerToClient = 4002;
//        //
//        public const Port_ContentServerControlDefault = 4531;
//        public const Port_SiteMonitorDefault = 4532;
//        //
//        public const RMBMethodHandShake = 1;
//        public const RMBMethodMessage = 3;
//        public const RMBMethodTestPoint = 4;
//        public const RMBMethodInit = 5;
//        public const RMBMethodClosePage = 6;
//        public const RMBMethodOpenCSContent = 7;
//        //
//        // ----- Position equates for the Remote Method Block
//        //
//        // Length of the RMB
//        const RMBPositionLength = 0;
//        // Handle generated by the source of the command
//        const RMBPositionSourceHandle = 4;
//        // Method in the method block
//        const RMBPositionMethod = 8;
//        // The number of arguments in the Block
//        const RMBPositionArgumentCount = 12;
//        // The offset to the first argu
//        const RMBPositionFirstArgument = 16;
//        //
//        //-----------------------------------------------------------------------
//        //   Remote Connections
//        //   List of current remove connections for Remote Monitoring/administration
//        //-----------------------------------------------------------------------
//        //
//        public class RemoteAdministratorType
//        {
//            string RemoteIP;
//            int RemotePort;
//        }
//        //
//        // Default username/password
//        //
//        public const DefaultServerUsername = "root";
//        public const DefaultServerPassword = "contensive";
//        //
//        //-----------------------------------------------------------------------
//        //   Form Contension Strategy
//        //        '       elements of the form are named  "ElementName"
//        //
//        //       This prevents form elements from different forms from interfearing
//        //       with each other, and with developer generated forms.
//        //
//        //       All forms requires:
//        //           a FormId (text), containing the formid string
//        //           a [formid]Type (text), as defined in FormTypexxx in CommonModule
//        //
//        //       Forms have two primary sections: GetForm and ProcessForm
//        //
//        //       Any form that has a GetForm method, should have the process form
//        //       in the cmc.main_init, selected with this [formid]type hidden (not the
//        //       GetForm method). This is so the process can alter the stream
//        //       output for areas before the GetForm call.
//        //
//        //       System forms, like tools panel, that may appear on any page, have
//        //       their process call in the cmc.main_init.
//        //
//        //       Popup forms, like ImageSelector have their processform call in the
//        //       cmc.main_init because no .asp page exists that might contain a call
//        //       the process section.
//        //
//        //-----------------------------------------------------------------------
//        //
//        public const FormTypeToolsPanel = "do30a8vl29";
//        public const FormTypeActiveEditor = "l1gk70al9n";
//        public const FormTypeImageSelector = "ila9c5s01m";
//        public const FormTypePageAuthoring = "2s09lmpalb";
//        public const FormTypeMyProfile = "89aLi180j5";
//        public const FormTypeLogin = "login";
//        //Public Const FormTypeLogin = "l09H58a195"
//        public const FormTypeSendPassword = "lk0q56am09";
//        public const FormTypeJoin = "6df38abv00";
//        public const FormTypeHelpBubbleEditor = "9df019d77sA";
//        public const FormTypeAddonSettingsEditor = "4ed923aFGw9d";
//        public const FormTypeAddonStyleEditor = "ar5028jklkfd0s";
//        public const FormTypeSiteStyleEditor = "fjkq4w8794kdvse";
//        //Public Const FormTypeAggregateFunctionProperties = "9wI751270"
//        //
//        //-----------------------------------------------------------------------
//        //   Hardcoded profile form const
//        //-----------------------------------------------------------------------
//        //
//        public const rnMyProfileTopics = "profileTopics";
//        //
//        //-----------------------------------------------------------------------
//        // Legacy - replaced with HardCodedPages
//        //   Intercept Page Strategy
//        //
//        //       RequestnameInterceptpage = InterceptPage number from the input stream
//        //       InterceptPage = Global variant with RequestnameInterceptpage value read during early Init
//        //
//        //       Intercept pages are complete pages that appear instead of what
//        //       the physical page calls.
//        //-----------------------------------------------------------------------
//        //
//        //Public Const RequestNameInterceptpage = "ccIPage"
//        //
//        public const LegacyInterceptPageSNResourceLibrary = "s033l8dm15";
//        public const LegacyInterceptPageSNSiteExplorer = "kdif3318sd";
//        public const LegacyInterceptPageSNImageUpload = "ka983lm039";
//        public const LegacyInterceptPageSNMyProfile = "k09ddk9105";
//        public const LegacyInterceptPageSNLogin = "6ge42an09a";
//        public const LegacyInterceptPageSNPrinterVersion = "l6d09a10sP";
//        public const LegacyInterceptPageSNUploadEditor = "k0hxp2aiOZ";
//        //
//        //-----------------------------------------------------------------------
//        // Ajax functions intercepted during init, answered and response closed
//        //   These are hard-coded internal Contensive functions
//        //   These should eventually be replaced with (HardcodedAddons) remote methods
//        //   They should all be prefixed "cc"
//        //   They are called with cj.ajax.qs(), setting RequestNameAjaxFunction=name in the qs
//        //   These name=value pairs go in the QueryString argument of the javascript cj.ajax.qs() function
//        //-----------------------------------------------------------------------
//        //
//        //Public Const RequestNameOpenSettingPage = "settingpageid"
//        public const RequestNameAjaxFunction = "ajaxfn";
//        public const RequestNameAjaxFastFunction = "ajaxfastfn";
//        //
//        public const AjaxOpenAdminNav = "aps89102kd";
//        public const AjaxOpenAdminNavGetContent = "d8475jkdmfj2";
//        public const AjaxCloseAdminNav = "3857fdjdskf91";
//        public const AjaxAdminNavOpenNode = "8395j2hf6jdjf";
//        public const AjaxAdminNavOpenNodeGetContent = "eieofdwl34efvclaeoi234598";
//        public const AjaxAdminNavCloseNode = "w325gfd73fhdf4rgcvjk2";
//        //
//        public const AjaxCloseIndexFilter = "k48smckdhorle0";
//        public const AjaxOpenIndexFilter = "Ls8jCDt87kpU45YH";
//        public const AjaxOpenIndexFilterGetContent = "llL98bbJQ38JC0KJm";
//        public const AjaxStyleEditorAddStyle = "ajaxstyleeditoradd";
//        public const AjaxPing = "ajaxalive";
//        public const AjaxGetFormEditTabContent = "ajaxgetformedittabcontent";
//        public const AjaxData = "data";
//        public const AjaxGetVisitProperty = "getvisitproperty";
//        public const AjaxSetVisitProperty = "setvisitproperty";
//        public const AjaxGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString";
//        public const ajaxGetFieldEditorPreferenceForm = "ajaxgetfieldeditorpreference";
//        //
//        //-----------------------------------------------------------------------
//        //
//        // no - for now just use ajaxfn in the cj.ajax.qs call
//        //   this is more work, and I do not see why it buys anything new or better
//        //
//        //   Hard-coded addons
//        //       these are internal Contensive functions
//        //       can be called with just /addonname?querystring
//        //       call them with cj.ajax.addon() or cj.ajax.addonCallback()
//        //       are first in the list of checks when a URL rewrite is detected in Init()
//        //       should all be prefixed with 'cc'
//        //-----------------------------------------------------------------------
//        //
//        //Public Const HardcodedAddonGetDefaultAddonOptionString = "ccGetDefaultAddonOptionString"
//        //
//        //-----------------------------------------------------------------------
//        //   Remote Methods
//        //       ?RemoteMethodAddon=string
//        //       calls an addon (if marked to run as a remote method)
//        //       blocks all other Contensive output (tools panel, javascript, etc)
//        //-----------------------------------------------------------------------
//        //
//        public const RequestNameRemoteMethodAddon = "remotemethodaddon";
//        //
//        //-----------------------------------------------------------------------
//        //   Hard Coded Pages
//        //       ?Method=string
//        //       Querystring based so they can be added to URLs, preserving the current page for a return
//        //       replaces output stream with html output
//        //-----------------------------------------------------------------------
//        //
//        public const RequestNameHardCodedPage = "method";
//        //
//        public const HardCodedPageLogin = "login";
//        public const HardCodedPageLoginDefault = "logindefault";
//        public const HardCodedPageMyProfile = "myprofile";
//        public const HardCodedPagePrinterVersion = "printerversion";
//        public const HardCodedPageResourceLibrary = "resourcelibrary";
//        public const HardCodedPageLogoutLogin = "logoutlogin";
//        public const HardCodedPageLogout = "logout";
//        public const HardCodedPageSiteExplorer = "siteexplorer";
//        //Public Const HardCodedPageForceMobile = "forcemobile"
//        //Public Const HardCodedPageForceNonMobile = "forcenonmobile"
//        public const HardCodedPageNewOrder = "neworderpage";
//        public const HardCodedPageStatus = "status";
//        public const HardCodedPageGetJSPage = "getjspage";
//        public const HardCodedPageGetJSLogin = "getjslogin";
//        public const HardCodedPageRedirect = "redirect";
//        public const HardCodedPageExportAscii = "exportascii";
//        public const HardCodedPagePayPalConfirm = "paypalconfirm";
//        public const HardCodedPageSendPassword = "sendpassword";
//        //
//        //-----------------------------------------------------------------------
//        //   Option values
//        //       does not effect output directly
//        //-----------------------------------------------------------------------
//        //
//        public const RequestNamePageOptions = "ccoptions";
//        //
//        public const PageOptionForceMobile = "forcemobile";
//        public const PageOptionForceNonMobile = "forcenonmobile";
//        public const PageOptionLogout = "logout";
//        public const PageOptionPrinterVersion = "printerversion";
//        //
//        // convert to options later
//        //
//        public const RequestNameDashboardReset = "ResetDashboard";
//        //
//        //-----------------------------------------------------------------------
//        //   DataSource constants
//        //-----------------------------------------------------------------------
//        //
//        public const DefaultDataSourceID = -1;
//        //
//        //-----------------------------------------------------------------------
//        // ----- Type compatibility between databases
//        //       Boolean
//        //           Access      YesNo       true=1, false=0
//        //           SQL Server  bit         true=1, false=0
//        //           MySQL       bit         true=1, false=0
//        //           Oracle      integer(1)  true=1, false=0
//        //           Note: false does not equal NOT true
//        //       Integer (Number)
//        //           Access      Long        8 bytes, about E308
//        //           SQL Server  int
//        //           MySQL       integer
//        //           Oracle      integer(8)
//        //       Float
//        //           Access      Double      8 bytes, about E308
//        //           SQL Server  Float
//        //           MySQL
//        //           Oracle
//        //       Text
//        //           Access
//        //           SQL Server
//        //           MySQL
//        //           Oracle
//        //-----------------------------------------------------------------------
//        //
//        //Public Const SQLFalse = "0"
//        //Public Const SQLTrue = "1"
//        //
//        //-----------------------------------------------------------------------
//        // ----- Style sheet definitions
//        //-----------------------------------------------------------------------
//        //
//        public const defaultStyleFilename = "ccDefault.r5.css";
//        public const StyleSheetStart = "<STYLE TYPE=\"text/css\">";
//        public const StyleSheetEnd = "</STYLE>";
//        //
//        public const SpanClassAdminNormal = "<span class=\"ccAdminNormal\">";
//        public const SpanClassAdminSmall = "<span class=\"ccAdminSmall\">";
//        //
//        // remove these from ccWebx
//        //
//        public const SpanClassNormal = "<span class=\"ccNormal\">";
//        public const SpanClassSmall = "<span class=\"ccSmall\">";
//        public const SpanClassLarge = "<span class=\"ccLarge\">";
//        public const SpanClassHeadline = "<span class=\"ccHeadline\">";
//        public const SpanClassList = "<span class=\"ccList\">";
//        public const SpanClassListCopy = "<span class=\"ccListCopy\">";
//        public const SpanClassError = "<span class=\"ccError\">";
//        public const SpanClassSeeAlso = "<span class=\"ccSeeAlso\">";
//        public const SpanClassEnd = "</span>";
//        //
//        //-----------------------------------------------------------------------
//        // ----- XHTML definitions
//        //-----------------------------------------------------------------------
//        //
//        public const DTDTransitional = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
//        //
//        public const BR = "<br>";
//        //
//        //-----------------------------------------------------------------------
//        // AuthoringControl Types
//        //-----------------------------------------------------------------------
//        //
//        public const AuthoringControlsEditing = 1;
//        public const AuthoringControlsSubmitted = 2;
//        public const AuthoringControlsApproved = 3;
//        public const AuthoringControlsModified = 4;
//        //
//        //-----------------------------------------------------------------------
//        // ----- Panel and header colors
//        //-----------------------------------------------------------------------
//        //
//        //Public Const "ccPanel" = "#E0E0E0"    ' The background color of a panel (black copy visible on it)
//        //Public Const "ccPanelHilite" = "#F8F8F8"  '
//        //Public Const "ccPanelShadow" = "#808080"  '
//        //
//        //Public Const HeaderColorBase = "#0320B0"   ' The background color of a panel header (reverse copy visible)
//        //Public Const "ccPanelHeaderHilite" = "#8080FF" '
//        //Public Const "ccPanelHeaderShadow" = "#000000" '
//        //
//        //-----------------------------------------------------------------------
//        // ----- Field type Definitions
//        //       Field Types are numeric values that describe how to treat values
//        //       stored as ContentFieldDefinitionType (FieldType property of FieldType Type.. ;)
//        //-----------------------------------------------------------------------
//        //
//        // An long number
//        public const FieldTypeIdInteger = 1;
//        // A text field (up to 255 characters)
//        public const FieldTypeIdText = 2;
//        // A memo field (up to 8000 characters)
//        public const FieldTypeIdLongText = 3;
//        // A yes/no field
//        public const FieldTypeIdBoolean = 4;
//        // A date field
//        public const FieldTypeIdDate = 5;
//        // A filename of a file in the files directory.
//        public const FieldTypeIdFile = 6;
//        // A lookup is a FieldTypeInteger that indexes into another table
//        public const FieldTypeIdLookup = 7;
//        // creates a link to another section
//        public const FieldTypeIdRedirect = 8;
//        // A Float that prints in dollars
//        public const FieldTypeIdCurrency = 9;
//        // Text saved in a file in the files area.
//        public const FieldTypeIdFileTextPrivate = 10;
//        // A filename of a file in the files directory.
//        public const FieldTypeIdFileImage = 11;
//        // A float number
//        public const FieldTypeIdFloat = 12;
//        //long that automatically increments with the new record
//        public const FieldTypeIdAutoIdIncrement = 13;
//        // no database field - sets up a relationship through a Rule table to another table
//        public const FieldTypeIdManyToMany = 14;
//        // This ID is a ccMembers record in a group defined by the MemberSelectGroupID field
//        public const FieldTypeIdMemberSelect = 15;
//        // A filename of a CSS compatible file
//        public const FieldTypeIdFileCSS = 16;
//        // the filename of an XML compatible file
//        public const FieldTypeIdFileXML = 17;
//        // the filename of a javascript compatible file
//        public const FieldTypeIdFileJavascript = 18;
//        // Links used in href tags -- can go to pages or resources
//        public const FieldTypeIdLink = 19;
//        // Links used in resources, link <img or <object. Should not be pages
//        public const FieldTypeIdResourceLink = 20;
//        // LongText field that expects HTML content
//        public const FieldTypeIdHTML = 21;
//        // TextFile field that expects HTML content
//        public const FieldTypeIdFileHTMLPrivate = 22;
//        public const FieldTypeIdMax = 22;
//        //
//        // ----- Field Descriptors for these type
//        //       These are what are publicly displayed for each type
//        //       See GetFieldTypeNameByType and vise-versa to translater
//        //
//        public const FieldTypeNameInteger = "Integer";
//        public const FieldTypeNameText = "Text";
//        public const FieldTypeNameLongText = "LongText";
//        public const FieldTypeNameBoolean = "Boolean";
//        public const FieldTypeNameDate = "Date";
//        public const FieldTypeNameFile = "File";
//        public const FieldTypeNameLookup = "Lookup";
//        public const FieldTypeNameRedirect = "Redirect";
//        public const FieldTypeNameCurrency = "Currency";
//        public const FieldTypeNameImage = "Image";
//        public const FieldTypeNameFloat = "Float";
//        public const FieldTypeNameManyToMany = "ManyToMany";
//        public const FieldTypeNameTextFile = "TextFile";
//        public const FieldTypeNameCSSFile = "CSSFile";
//        public const FieldTypeNameXMLFile = "XMLFile";
//        public const FieldTypeNameJavascriptFile = "JavascriptFile";
//        public const FieldTypeNameLink = "Link";
//        public const FieldTypeNameResourceLink = "ResourceLink";
//        public const FieldTypeNameMemberSelect = "MemberSelect";
//        public const FieldTypeNameHTML = "HTML";
//        public const FieldTypeNameHTMLFile = "HTMLFile";
//        //
//        public const FieldTypeNameLcaseInteger = "integer";
//        public const FieldTypeNameLcaseText = "text";
//        public const FieldTypeNameLcaseLongText = "longtext";
//        public const FieldTypeNameLcaseBoolean = "boolean";
//        public const FieldTypeNameLcaseDate = "date";
//        public const FieldTypeNameLcaseFile = "file";
//        public const FieldTypeNameLcaseLookup = "lookup";
//        public const FieldTypeNameLcaseRedirect = "redirect";
//        public const FieldTypeNameLcaseCurrency = "currency";
//        public const FieldTypeNameLcaseImage = "image";
//        public const FieldTypeNameLcaseFloat = "float";
//        public const FieldTypeNameLcaseManyToMany = "manytomany";
//        public const FieldTypeNameLcaseTextFile = "textfile";
//        public const FieldTypeNameLcaseCSSFile = "cssfile";
//        public const FieldTypeNameLcaseXMLFile = "xmlfile";
//        public const FieldTypeNameLcaseJavascriptFile = "javascriptfile";
//        public const FieldTypeNameLcaseLink = "link";
//        public const FieldTypeNameLcaseResourceLink = "resourcelink";
//        public const FieldTypeNameLcaseMemberSelect = "memberselect";
//        public const FieldTypeNameLcaseHTML = "html";
//        public const FieldTypeNameLcaseHTMLFile = "htmlfile";
//        //
//        //------------------------------------------------------------------------
//        // ----- Payment Options
//        //------------------------------------------------------------------------
//        //
//        // Pay by credit card online
//        public const PayTypeCreditCardOnline = 0;
//        // Phone in a credit card
//        public const PayTypeCreditCardByPhone = 1;
//        // Phone in a credit card
//        public const PayTypeCreditCardByFax = 9;
//        // pay by check to be mailed
//        public const PayTypeCHECK = 2;
//        // pay on account
//        public const PayTypeCREDIT = 3;
//        // order total is $0.00. Nothing due
//        public const PayTypeNONE = 4;
//        // pay by company check
//        public const PayTypeCHECKCOMPANY = 5;
//        public const PayTypeNetTerms = 6;
//        public const PayTypeCODCompanyCheck = 7;
//        public const PayTypeCODCertifiedFunds = 8;
//        public const PayTypePAYPAL = 10;
//        public const PAYDEFAULT = 0;
//        //
//        //------------------------------------------------------------------------
//        // ----- Credit card options
//        //------------------------------------------------------------------------
//        //
//        // Visa
//        public const CCTYPEVISA = 0;
//        // MasterCard
//        public const CCTYPEMC = 1;
//        // American Express
//        public const CCTYPEAMEX = 2;
//        // Discover
//        public const CCTYPEDISCOVER = 3;
//        // Novus Card
//        public const CCTYPENOVUS = 4;
//        public const CCTYPEDEFAULT = 0;
//        //
//        //------------------------------------------------------------------------
//        // ----- Shipping Options
//        //------------------------------------------------------------------------
//        //
//        // ground
//        public const SHIPGROUND = 0;
//        // overnight
//        public const SHIPOVERNIGHT = 1;
//        // standard, whatever that is
//        public const SHIPSTANDARD = 2;
//        // to overseas
//        public const SHIPOVERSEAS = 3;
//        // to Canada
//        public const SHIPCANADA = 4;
//        public const SHIPDEFAULT = 0;
//        //
//        //------------------------------------------------------------------------
//        // Debugging info
//        //------------------------------------------------------------------------
//        //
//        public const TestPointTab = 2;
//        public const char TestPointTabChr = '-';
//        public static double CPTickCountBase;
//        //
//        //------------------------------------------------------------------------
//        //   project width button defintions
//        //------------------------------------------------------------------------
//        //
//        public const ButtonApply = "  Apply ";
//        public const ButtonLogin = "  Login  ";
//        public const ButtonLogout = "  Logout  ";
//        public const ButtonSendPassword = "  Send Password  ";
//        public const ButtonJoin = "   Join   ";
//        public const ButtonSave = "  Save  ";
//        public const ButtonOK = "     OK     ";
//        public const ButtonReset = "  Reset  ";
//        public const ButtonSaveAddNew = " Save + Add ";
//        //Public Const ButtonSaveAddNew = " Save > Add "
//        public const ButtonCancel = " Cancel ";
//        public const ButtonRestartContensiveApplication = " Restart Contensive Application ";
//        public const ButtonCancelAll = "  Cancel  ";
//        public const ButtonFind = "   Find   ";
//        public const ButtonDelete = "  Delete  ";
//        public const ButtonDeletePerson = " Delete Person ";
//        public const ButtonDeleteRecord = " Delete Record ";
//        public const ButtonDeleteEmail = " Delete Email ";
//        public const ButtonDeletePage = " Delete Page ";
//        public const ButtonFileChange = "   Upload   ";
//        public const ButtonFileDelete = "    Delete    ";
//        public const ButtonClose = "  Close   ";
//        public const ButtonAdd = "   Add    ";
//        public const ButtonAddChildPage = " Add Child ";
//        public const ButtonAddSiblingPage = " Add Sibling ";
//        public const ButtonContinue = " Continue >> ";
//        public const ButtonBack = "  << Back  ";
//        public const ButtonNext = "   Next   ";
//        public const ButtonPrevious = " Previous ";
//        public const ButtonFirst = "  First   ";
//        public const ButtonSend = "  Send   ";
//        public const ButtonSendTest = "Send Test";
//        public const ButtonCreateDuplicate = " Create Duplicate ";
//        public const ButtonActivate = "  Activate   ";
//        public const ButtonDeactivate = "  Deactivate   ";
//        public const ButtonOpenActiveEditor = "Active Edit";
//        public const ButtonPublish = " Publish Changes ";
//        public const ButtonAbortEdit = " Abort Edits ";
//        public const ButtonPublishSubmit = " Submit for Publishing ";
//        public const ButtonPublishApprove = " Approve for Publishing ";
//        public const ButtonPublishDeny = " Deny for Publishing ";
//        public const ButtonWorkflowPublishApproved = " Publish Approved Records ";
//        public const ButtonWorkflowPublishSelected = " Publish Selected Records ";
//        public const ButtonSetHTMLEdit = " Edit WYSIWYG ";
//        public const ButtonSetTextEdit = " Edit HTML ";
//        public const ButtonRefresh = " Refresh ";
//        public const ButtonOrder = " Order ";
//        public const ButtonSearch = " Search ";
//        public const ButtonSpellCheck = " Spell Check ";
//        public const ButtonLibraryUpload = " Upload ";
//        public const ButtonCreateReport = " Create Report ";
//        public const ButtonClearTrapLog = " Clear Trap Log ";
//        public const ButtonNewSearch = " New Search ";
//        public const ButtonSaveandInvalidateCache = " Save and Invalidate Cache ";
//        public const ButtonImportTemplates = " Import Templates ";
//        public const ButtonRSSRefresh = " Update RSS Feeds Now ";
//        public const ButtonRequestDownload = " Request Download ";
//        public const ButtonFinish = " Finish ";
//        public const ButtonRegister = " Register ";
//        public const ButtonBegin = "Begin";
//        public const ButtonAbort = "Abort";
//        public const ButtonCreateGUID = " Create GUID ";
//        public const ButtonEnable = " Enable ";
//        public const ButtonDisable = " Disable ";
//        public const ButtonMarkReviewed = " Mark Reviewed ";
//        //
//        //------------------------------------------------------------------------
//        //   member actions
//        //------------------------------------------------------------------------
//        //
//        public const MemberActionNOP = 0;
//        public const MemberActionLogin = 1;
//        public const MemberActionLogout = 2;
//        public const MemberActionForceLogin = 3;
//        public const MemberActionSendPassword = 4;
//        public const MemberActionForceLogout = 5;
//        public const MemberActionToolsApply = 6;
//        public const MemberActionJoin = 7;
//        public const MemberActionSaveProfile = 8;
//        public const MemberActionEditProfile = 9;
//        //
//        //-----------------------------------------------------------------------
//        // ----- note pad info
//        //-----------------------------------------------------------------------
//        //
//        public const NoteFormList = 1;
//        public const NoteFormRead = 2;
//        //
//        public const NoteButtonPrevious = " Previous ";
//        public const NoteButtonNext = "   Next   ";
//        public const NoteButtonDelete = "  Delete  ";
//        public const NoteButtonClose = "  Close   ";
//        //                       ' Submit button is created in CommonDim, so it is simple
//        public const NoteButtonSubmit = "Submit";
//        //
//        //-----------------------------------------------------------------------
//        // ----- Admin site storage
//        //-----------------------------------------------------------------------
//        //
//        //   menu is hidden
//        public const AdminMenuModeHidden = 0;
//        //   menu on the left
//        public const AdminMenuModeLeft = 1;
//        //   menu as dropdowns from the top
//        public const AdminMenuModeTop = 2;
//        //
//        // ----- AdminActions - describes the form processing to do
//        //
//        // do nothing
//        public const AdminActionNop = 0;
//        // delete record
//        public const AdminActionDelete = 4;
//        //
//        public const AdminActionFind = 5;
//        //
//        public const AdminActionDeleteFilex = 6;
//        //
//        public const AdminActionUpload = 7;
//        // save fields to database
//        public const AdminActionSaveNormal = 3;
//        // save email record (and update EmailGroups) to database
//        public const AdminActionSaveEmail = 8;
//        //
//        public const AdminActionSaveMember = 11;
//        public const AdminActionSaveSystem = 12;
//        // Save a record that is in the BathBlocking Format
//        public const AdminActionSavePaths = 13;
//        //
//        public const AdminActionSendEmail = 9;
//        //
//        public const AdminActionSendEmailTest = 10;
//        //
//        public const AdminActionNext = 14;
//        //
//        public const AdminActionPrevious = 15;
//        //
//        public const AdminActionFirst = 16;
//        //
//        public const AdminActionSaveContent = 17;
//        // Save a single field, fieldname = fn input
//        public const AdminActionSaveField = 18;
//        // Publish record live
//        public const AdminActionPublish = 19;
//        // Publish record live
//        public const AdminActionAbortEdit = 20;
//        // Submit for Workflow Publishing
//        public const AdminActionPublishSubmit = 21;
//        // Approve for Workflow Publishing
//        public const AdminActionPublishApprove = 22;
//        // Publish what was approved
//        public const AdminActionWorkflowPublishApproved = 23;
//        // Set Member Property for this field to HTML Edit
//        public const AdminActionSetHTMLEdit = 24;
//        // Set Member Property for this field to Text Edit
//        public const AdminActionSetTextEdit = 25;
//        // Save Record
//        public const AdminActionSave = 26;
//        // Activate a Conditional Email
//        public const AdminActionActivateEmail = 27;
//        // Deactivate a conditional email
//        public const AdminActionDeactivateEmail = 28;
//        // Duplicate the (sent email) record
//        public const AdminActionDuplicate = 29;
//        // Delete from rows of records, row0 is boolean, rowid0 is ID, rowcnt is count
//        public const AdminActionDeleteRows = 30;
//        // Save Record and add a new record
//        public const AdminActionSaveAddNew = 31;
//        // Load Content Definitions
//        public const AdminActionReloadCDef = 32;
//        // Publish what was selected
//        public const AdminActionWorkflowPublishSelected = 33;
//        // Mark the record reviewed without making any changes
//        public const AdminActionMarkReviewed = 34;
//        // reload the page just like a save, but do not save
//        public const AdminActionEditRefresh = 35;
//        //
//        // ----- Adminforms (0-99)
//        //
//        // intro page
//        public const AdminFormRoot = 0;
//        // record list page
//        public const AdminFormIndex = 1;
//        // popup help window
//        public const AdminFormHelp = 2;
//        // encoded file upload form
//        public const AdminFormUpload = 3;
//        // Edit form for system format records
//        public const AdminFormEdit = 4;
//        // Edit form for system format records
//        public const AdminFormEditSystem = 5;
//        // record edit page
//        public const AdminFormEditNormal = 6;
//        // Edit form for Email format records
//        public const AdminFormEditEmail = 7;
//        // Edit form for Member format records
//        public const AdminFormEditMember = 8;
//        // Edit form for Paths format records
//        public const AdminFormEditPaths = 9;
//        // Special Case - do a window close instead of displaying a form
//        public const AdminFormClose = 10;
//        // Call Reports form (admin only)
//        public const AdminFormReports = 12;
//        //Public Const AdminFormSpider = 13          ' Call Spider
//        // Edit form for Content records
//        public const AdminFormEditContent = 14;
//        // ActiveX DHTMLEdit form
//        public const AdminFormDHTMLEdit = 15;
//        //
//        public const AdminFormEditPageContent = 16;
//        // Workflow Authoring Publish Control form
//        public const AdminFormPublishing = 17;
//        // Quick Stats (from Admin root)
//        public const AdminFormQuickStats = 18;
//        // Resource Library without Selects
//        public const AdminFormResourceLibrary = 19;
//        // Control Form for the EDG publishing controls
//        public const AdminFormEDGControl = 20;
//        // Control Form for the Content Spider
//        public const AdminFormSpiderControl = 21;
//        // Admin Create Content Child tool
//        public const AdminFormContentChildTool = 22;
//        // Map all content to a single map
//        public const AdminformPageContentMap = 23;
//        // Housekeeping control
//        public const AdminformHousekeepingControl = 24;
//        public const AdminFormCommerceControl = 25;
//        public const AdminFormContactManager = 26;
//        public const AdminFormStyleEditor = 27;
//        public const AdminFormEmailControl = 28;
//        public const AdminFormCommerceInterface = 29;
//        public const AdminFormDownloads = 30;
//        public const AdminformRSSControl = 31;
//        public const AdminFormMeetingSmart = 32;
//        public const AdminFormMemberSmart = 33;
//        public const AdminFormEmailWizard = 34;
//        public const AdminFormImportWizard = 35;
//        public const AdminFormCustomReports = 36;
//        public const AdminFormFormWizard = 37;
//        public const AdminFormLegacyAddonManager = 38;
//        public const AdminFormIndex_SubFormAdvancedSearch = 39;
//        public const AdminFormIndex_SubFormSetColumns = 40;
//        public const AdminFormPageControl = 41;
//        public const AdminFormSecurityControl = 42;
//        public const AdminFormEditorConfig = 43;
//        public const AdminFormBuilderCollection = 44;
//        public const AdminFormClearCache = 45;
//        public const AdminFormMobileBrowserControl = 46;
//        public const AdminFormMetaKeywordTool = 47;
//        public const AdminFormIndex_SubFormExport = 48;
//        //
//        // ----- AdminFormTools (11,100-199)
//        //
//        // Call Tools form (developer only)
//        public const AdminFormTools = 11;
//        // These should match for compatibility
//        public const AdminFormToolRoot = 11;
//        public const AdminFormToolCreateContentDefinition = 101;
//        public const AdminFormToolContentTest = 102;
//        public const AdminFormToolConfigureMenu = 103;
//        public const AdminFormToolConfigureListing = 104;
//        public const AdminFormToolConfigureEdit = 105;
//        public const AdminFormToolManualQuery = 106;
//        public const AdminFormToolWriteUpdateMacro = 107;
//        public const AdminFormToolDuplicateContent = 108;
//        public const AdminFormToolDuplicateDataSource = 109;
//        public const AdminFormToolDefineContentFieldsFromTable = 110;
//        public const AdminFormToolContentDiagnostic = 111;
//        public const AdminFormToolCreateChildContent = 112;
//        public const AdminFormToolClearContentWatchLink = 113;
//        public const AdminFormToolSyncTables = 114;
//        public const AdminFormToolBenchmark = 115;
//        public const AdminFormToolSchema = 116;
//        public const AdminFormToolContentFileView = 117;
//        public const AdminFormToolDbIndex = 118;
//        public const AdminFormToolContentDbSchema = 119;
//        public const AdminFormToolLogFileView = 120;
//        public const AdminFormToolLoadCDef = 121;
//        public const AdminFormToolLoadTemplates = 122;
//        public const AdminformToolFindAndReplace = 123;
//        public const AdminformToolCreateGUID = 124;
//        public const AdminformToolIISReset = 125;
//        public const AdminFormToolRestart = 126;
//        public const AdminFormToolWebsiteFileView = 127;
//        //
//        // ----- Define the index column structure
//        //       IndexColumnVariant( 0, n ) is the first column on the left
//        //       IndexColumnVariant( 0, IndexColumnField ) = the index into the fields array
//        //
//        // The field displayed in the column
//        public const IndexColumnField = 0;
//        // The width of the column
//        public const IndexColumnWIDTH = 1;
//        // lowest columns sorts first
//        public const IndexColumnSORTPRIORITY = 2;
//        // direction of the sort on this column
//        public const IndexColumnSORTDIRECTION = 3;
//        // the number of attributes here
//        public const IndexColumnSATTRIBUTEMAX = 3;
//        public const IndexColumnsMax = 50;
//        //
//        // ----- ReportID Constants, moved to ccCommonModule
//        //
//        public const ReportFormRoot = 1;
//        public const ReportFormDailyVisits = 2;
//        public const ReportFormWeeklyVisits = 12;
//        public const ReportFormSitePath = 4;
//        public const ReportFormSearchKeywords = 5;
//        public const ReportFormReferers = 6;
//        public const ReportFormBrowserList = 8;
//        public const ReportFormAddressList = 9;
//        public const ReportFormContentProperties = 14;
//        public const ReportFormSurveyList = 15;
//        public const ReportFormOrdersList = 13;
//        public const ReportFormOrderDetails = 21;
//        public const ReportFormVisitorList = 11;
//        public const ReportFormMemberDetails = 16;
//        public const ReportFormPageList = 10;
//        public const ReportFormVisitList = 3;
//        public const ReportFormVisitDetails = 17;
//        public const ReportFormVisitorDetails = 20;
//        public const ReportFormSpiderDocList = 22;
//        public const ReportFormSpiderErrorList = 23;
//        public const ReportFormEDGDocErrors = 24;
//        public const ReportFormDownloadLog = 25;
//        public const ReportFormSpiderDocDetails = 26;
//        public const ReportFormSurveyDetails = 27;
//        public const ReportFormEmailDropList = 28;
//        public const ReportFormPageTraffic = 29;
//        public const ReportFormPagePerformance = 30;
//        public const ReportFormEmailDropDetails = 31;
//        public const ReportFormEmailOpenDetails = 32;
//        public const ReportFormEmailClickDetails = 33;
//        public const ReportFormGroupList = 34;
//        public const ReportFormGroupMemberList = 35;
//        public const ReportFormTrapList = 36;
//        public const ReportFormCount = 36;
//        //
//        //=============================================================================
//        // Page Scope Meetings Related Storage
//        //=============================================================================
//        //
//        public const MeetingFormIndex = 0;
//        public const MeetingFormAttendees = 1;
//        public const MeetingFormLinks = 2;
//        public const MeetingFormFacility = 3;
//        public const MeetingFormHotel = 4;
//        public const MeetingFormDetails = 5;
//        //
//        //------------------------------------------------------------------------------
//        // Form actions
//        //------------------------------------------------------------------------------
//        //
//        // ----- DataSource Types
//        //
//        public const DataSourceTypeODBCSQL99 = 0;
//        public const DataSourceTypeODBCAccess = 1;
//        public const DataSourceTypeODBCSQLServer = 2;
//        public const DataSourceTypeODBCMySQL = 3;
//        // Use MSXML Interface to open a file
//        public const DataSourceTypeXMLFile = 4;
//        //
//        //------------------------------------------------------------------------------
//        //   Application Status
//        //------------------------------------------------------------------------------
//        //
//        [ComVisible(true)]
//        public enum applicationStatusEnum
//        {
//            ApplicationStatusNotFound = 0,
//            ApplicationStatusNotEnabled = 1,
//            ApplicationStatusReady = 2,
//            ApplicationStatusLoading = 3,
//            ApplicationStatusUpgrading = 4,
//            // ApplicationStatusConnectionBusy = 5    ' can not open connection because already open
//            ApplicationStatusKernelFailure = 6,
//            // can not create Kernel
//            ApplicationStatusNoHostService = 7,
//            // host service process ID not set
//            ApplicationStatusLicenseFailure = 8,
//            // failed to start because of License failure
//            ApplicationStatusDbNotFound = 9,
//            // failed to start because ccSetup table not found
//            ApplicationStatusFailedToInitialize = 10,
//            // failed to start because of unknown error, see trace log
//            ApplicationStatusDbBad = 11,
//            // ccContent,ccFields no records found
//            ApplicationStatusConnectionObjectFailure = 12,
//            // Connection Object FAiled
//            ApplicationStatusConnectionStringFailure = 13,
//            // Connection String FAiled to open the ODBC connection
//            ApplicationStatusDataSourceFailure = 14,
//            // DataSource failed to open
//            ApplicationStatusDuplicateDomains = 15,
//            // Can not locate application because there are 1+ apps that match the domain
//            ApplicationStatusPaused = 16,
//            // Running, but all activity is blocked (for backup)
//            ApplicationStatusAppConfigNotFound = 17,
//            ApplicationStatusAppConfigNotValid = 18,
//            ApplicationStatusDbFoundButContentMetaMissing = 19
//        }

//        //
//        // Document (HTML, graphic, etc) retrieved from site
//        //
//        public const ResponseHeaderCountMax = 20;
//        public const ResponseCookieCountMax = 20;
//        //
//        // ----- text delimiter that divides the text and html parts of an email message stored in the queue folder
//        //
//        public const EmailTextHTMLDelimiter = Constants.vbCrLf + " ----- End Text Begin HTML -----" + Constants.vbCrLf;
//        //
//        //------------------------------------------------------------------------
//        //   Common RequestName Variables
//        //------------------------------------------------------------------------
//        //
//        public const RequestNameDynamicFormID = "dformid";
//        //
//        public const RequestNameRunAddon = "addonid";
//        public const RequestNameEditReferer = "EditReferer";
//        //Public Const RequestNameRefreshBlock = "ccFormRefreshBlockSN"
//        public const RequestNameCatalogOrder = "CatalogOrderID";
//        public const RequestNameCatalogCategoryID = "CatalogCatID";
//        public const RequestNameCatalogForm = "CatalogFormID";
//        public const RequestNameCatalogItemID = "CatalogItemID";
//        public const RequestNameCatalogItemAge = "CatalogItemAge";
//        public const RequestNameCatalogRecordTop = "CatalogTop";
//        public const RequestNameCatalogFeatured = "CatalogFeatured";
//        public const RequestNameCatalogSpan = "CatalogSpan";
//        public const RequestNameCatalogKeywords = "CatalogKeywords";
//        public const RequestNameCatalogSource = "CatalogSource";
//        //
//        public const RequestNameLibraryFileID = "fileEID";
//        public const RequestNameDownloadID = "downloadid";
//        public const RequestNameLibraryUpload = "LibraryUpload";
//        public const RequestNameLibraryName = "LibraryName";

//        public const RequestNameLibraryDescription = "LibraryDescription";
//        public const RequestNameRootPage = "RootPageName";
//        public const RequestNameRootPageID = "RootPageID";
//        public const RequestNameContent = "ContentName";
//        public const RequestNameOrderByClause = "OrderByClause";
//        public const RequestNameAllowChildPageList = "AllowChildPageList";
//        //
//        public const RequestNameCRKey = "crkey";
//        public const RequestNameAdminForm = "af";
//        public const RequestNameAdminSubForm = "subform";
//        public const RequestNameButton = "button";
//        public const RequestNameAdminSourceForm = "asf";
//        public const RequestNameAdminFormSpelling = "SpellingRequest";
//        public const RequestNameInlineStyles = "InlineStyles";
//        public const RequestNameAllowCSSReset = "AllowCSSReset";
//        //
//        public const RequestNameReportForm = "rid";
//        //
//        public const RequestNameToolContentID = "ContentID";
//        //
//        public const RequestNameCut = "a904o2pa0cut";
//        public const RequestNamePaste = "dp29a7dsa6paste";
//        public const RequestNamePasteParentContentID = "dp29a7dsa6cid";
//        public const RequestNamePasteParentRecordID = "dp29a7dsa6rid";
//        public const RequestNamePasteFieldList = "dp29a7dsa6key";
//        public const RequestNameCutClear = "dp29a7dsa6clear";
//        //
//        public const RequestNameRequestBinary = "RequestBinary";
//        // removed -- this was an old method of blocking form input for file uploads
//        //Public Const RequestNameFormBlock = "RB"
//        public const RequestNameJSForm = "RequestJSForm";
//        public const RequestNameJSProcess = "ProcessJSForm";
//        //
//        public const RequestNameFolderID = "FolderID";
//        //
//        public const RequestNameEmailMemberID = "emi8s9Kj";
//        public const RequestNameEmailOpenFlag = "eof9as88";
//        public const RequestNameEmailOpenCssFlag = "8aa41pM3";
//        public const RequestNameEmailClickFlag = "ecf34Msi";
//        public const RequestNameEmailSpamFlag = "9dq8Nh61";
//        public const RequestNameEmailBlockRequestDropID = "BlockEmailRequest";
//        public const RequestNameVisitTracking = "s9lD1088";
//        public const RequestNameBlockContentTracking = "BlockContentTracking";

//        public const RequestNameCookieDetectVisitID = "f92vo2a8d";
//        public const RequestNamePageNumber = "PageNumber";
//        public const RequestNamePageSize = "PageSize";
//        //
//        public const RequestValueNull = "[NULL]";
//        //
//        public const SpellCheckUserDictionaryFilename = "SpellCheck\\UserDictionary.txt";
//        //
//        public const RequestNameStateString = "vstate";
//        //
//        // ----- Dataset for graphing
//        //
//        public class ColumnDataType
//        {
//            string Name;
//            int[] row;
//        }
//        //
//        public class ChartDataType
//        {
//            string Title;
//            string XLabel;
//            string YLabel;
//            int RowCount;
//            string[] RowLabel;
//            int ColumnCount;
//            ColumnDataType[] Column;
//        }
//        //'
//        // PrivateStorage to hold the DebugTimer
//        //
//        public class TimerStackType
//        {
//            string Label;
//            int StartTicks;
//        }
//        private const TimerStackMax = 20;
//        private static TimerStackType[] TimerStack = new TimerStackType[TimerStackMax + 1];
//        private static int TimerStackCount;
//        //
//        public const TextSearchStartTagDefault = "<!--TextSearchStart-->";
//        public const TextSearchEndTagDefault = "<!--TextSearchEnd-->";
//        //
//        //-------------------------------------------------------------------------------------
//        //   IPDaemon communication objects
//        //-------------------------------------------------------------------------------------
//        //
//        public class IPDaemonConnectionType
//        {
//            int ConnectionID;
//            int BytesToSend;
//            string HTTPVersion;
//            string HTTPMethod;
//            string Path;
//            string Query;
//            string Headers;
//            string PostData;
//            bool SendData;
//            int State;
//            int ContentLength;
//        }
//        //
//        //-------------------------------------------------------------------------------------
//        //   Email
//        //-------------------------------------------------------------------------------------
//        //
//        // Email was dropped
//        public const EmailLogTypeDrop = 1;
//        // System detected the email was opened
//        public const EmailLogTypeOpen = 2;
//        // System detected a click from a link on the email
//        public const EmailLogTypeClick = 3;
//        // Email was processed by bounce processing
//        public const EmailLogTypeBounce = 4;
//        // recipient asked us to stop sending email
//        public const EmailLogTypeBlockRequest = 5;
//        // Email was dropped
//        public const EmailLogTypeImmediateSend = 6;
//        //
//        public const DefaultSpamFooter = "<p>To block future emails from this site, <link>click here</link></p>";
//        //
//        public const FeedbackFormNotSupportedComment = "<!--" + Constants.vbCrLf + "Feedback form is not supported in this context" + Constants.vbCrLf + "-->";
//        //
//        //-------------------------------------------------------------------------------------
//        //   Page Content constants
//        //-------------------------------------------------------------------------------------
//        //
//        public const ContentBlockCopyName = "Content Block Copy";
//        //
//        public const BubbleCopy_AdminAddPage = "Use the Add page to create new content records. The save button puts you in edit mode. The OK button creates the record and exits.";
//        public const BubbleCopy_AdminIndexPage = "Use the Admin Listing page to locate content records through the Admin Site.";
//        public const BubbleCopy_SpellCheckPage = "Use the Spell Check page to verify and correct spelling throught the content.";
//        public const BubbleCopy_AdminEditPage = "Use the Edit page to add and modify content.";
//        //
//        //
//        public const TemplateDefaultName = "Default";
//        //Public Const TemplateDefaultBody = "<!--" & vbCrLf & "Default Template - edit this Page Template, or select a different template for your page or section" & vbCrLf & "-->{{DYNAMICMENU?MENU=}}<br>{{CONTENT}}"
//        public const TemplateDefaultBody = "" + Constants.vbCrLf + Constants.vbTab + "<!--" + Constants.vbCrLf + Constants.vbTab + "Default Template - edit this Page Template, or select a different template for your page or section" + Constants.vbCrLf + Constants.vbTab + "-->" + Constants.vbCrLf + Constants.vbTab + "<ac type=\"AGGREGATEFUNCTION\" name=\"Dynamic Menu\" querystring=\"Menu Name=Default\" acinstanceid=\"{6CBADABB-5B0D-43E1-B3CA-46A3D60DA3E1}\" >" + Constants.vbCrLf + Constants.vbTab + "<ac type=\"AGGREGATEFUNCTION\" name=\"Content Box\" acinstanceid=\"{49E0D0C0-D323-49B6-B211-B9599673A265}\" >";
//        public const TemplateDefaultBodyTag = "<body class=\"ccBodyWeb\">";
//        //
//        //=======================================================================
//        //   Internal Tab interface storage
//        //=======================================================================
//        //
//        private class TabType
//        {
//            string Caption;
//            string Link;
//            string StylePrefix;
//            bool IsHit;
//            string LiveBody;
//        }
//        //
//        private static TabType[] Tabs;
//        private static int TabsCnt;
//        private static int TabsSize;
//        //
//        // Admin Navigator Nodes
//        //
//        public const NavigatorNodeCollectionList = -1;
//        public const NavigatorNodeAddonList = -1;
//        //'
//        //' Pointers into index of PCC (Page Content Cache) array
//        //'
//        //Public Const PCC_ID = 0
//        //Public Const PCC_Active = 1
//        //Public Const PCC_ParentID = 2
//        //Public Const PCC_Name = 3
//        //Public Const PCC_Headline = 4
//        //Public Const PCC_MenuHeadline = 5
//        //Public Const PCC_DateArchive = 6
//        //Public Const PCC_DateExpires = 7
//        //Public Const PCC_PubDate = 8
//        //Public Const PCC_ChildListSortMethodID = 9
//        //Public Const PCC_ContentControlID = 10
//        //Public Const PCC_TemplateID = 11
//        //Public Const PCC_BlockContent = 12
//        //Public Const PCC_BlockPage = 13
//        //Public Const PCC_Link = 14
//        //Public Const PCC_RegistrationGroupID = 15
//        //Public Const PCC_BlockSourceID = 16
//        //Public Const PCC_CustomBlockMessageFilename = 17
//        //Public Const PCC_JSOnLoad = 18
//        //Public Const PCC_JSHead = 19
//        //Public Const PCC_JSEndBody = 20
//        //Public Const PCC_Viewings = 21
//        //Public Const PCC_ContactMemberID = 22
//        //Public Const PCC_AllowHitNotification = 23
//        //Public Const PCC_TriggerSendSystemEmailID = 24
//        //Public Const PCC_TriggerConditionID = 25
//        //Public Const PCC_TriggerConditionGroupID = 26
//        //Public Const PCC_TriggerAddGroupID = 27
//        //Public Const PCC_TriggerRemoveGroupID = 28
//        //Public Const PCC_AllowMetaContentNoFollow = 29
//        //Public Const PCC_ParentListName = 30
//        //Public Const PCC_CopyFilename = 31
//        //Public Const PCC_BriefFilename = 32
//        //Public Const PCC_AllowChildListDisplay = 33
//        //Public Const PCC_SortOrder = 34
//        //Public Const PCC_DateAdded = 35
//        //Public Const PCC_ModifiedDate = 36
//        //Public Const PCC_ChildPagesFound = 37
//        //Public Const PCC_AllowInMenus = 38
//        //Public Const PCC_AllowInChildLists = 39
//        //Public Const PCC_JSFilename = 40
//        //Public Const PCC_ChildListInstanceOptions = 41
//        //Public Const PCC_IsSecure = 42
//        //Public Const PCC_AllowBrief = 43
//        //Public Const PCC_ColCnt = 44
//        //
//        // Indexes into the SiteSectionCache
//        // Created from "ID, Name,TemplateID,ContentID,MenuImageFilename,Caption,MenuImageOverFilename,HideMenu,BlockSection,RootPageID,JSOnLoad,JSHead,JSEndBody"
//        //
//        public const SSC_ID = 0;
//        public const SSC_Name = 1;
//        public const SSC_TemplateID = 2;
//        public const SSC_ContentID = 3;
//        public const SSC_MenuImageFilename = 4;
//        public const SSC_Caption = 5;
//        public const SSC_MenuImageOverFilename = 6;
//        public const SSC_HideMenu = 7;
//        public const SSC_BlockSection = 8;
//        public const SSC_RootPageID = 9;
//        public const SSC_JSOnLoad = 10;
//        public const SSC_JSHead = 11;
//        public const SSC_JSEndBody = 12;
//        public const SSC_JSFilename = 13;
//        public const SSC_cnt = 14;
//        //
//        // Indexes into the TemplateCache
//        // Created from "t.ID,t.Name,t.Link,t.BodyHTML,t.JSOnLoad,t.JSHead,t.JSEndBody,t.StylesFilename,r.StyleID"
//        //
//        public const TC_ID = 0;
//        public const TC_Name = 1;
//        public const TC_Link = 2;
//        public const TC_BodyHTML = 3;
//        public const TC_JSOnLoad = 4;
//        public const TC_JSInHeadLegacy = 5;
//        //Public Const TC_JSHead = 5
//        public const TC_JSEndBody = 6;
//        public const TC_StylesFilename = 7;
//        public const TC_SharedStylesIDList = 8;
//        public const TC_MobileBodyHTML = 9;
//        public const TC_MobileStylesFilename = 10;
//        public const TC_OtherHeadTags = 11;
//        public const TC_BodyTag = 12;
//        public const TC_JSInHeadFilename = 13;
//        //Public Const TC_JSFilename = 13
//        public const TC_IsSecure = 14;
//        public const TC_DomainIdList = 15;
//        // for now, Mobile templates do not have shared styles
//        //Public Const TC_MobileSharedStylesIDList = 11
//        public const TC_cnt = 16;
//        //
//        // DTD
//        //
//        public const DTDDefault = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
//        public const DTDDefaultAdmin = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01 Transitional//EN\" \"http://www.w3.org/TR/html4/loose.dtd\">";
//        //
//        // innova Editor feature list
//        //
//        public const InnovaEditorFeaturefilename = "Config\\EditorCongif.txt";
//        public const InnovaEditorFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Image,Flash,Media,CustomObject,CustomTag,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor";
//        public const InnovaEditorPublicFeatureList = "FullScreen,Preview,Print,Search,Cut,Copy,Paste,PasteWord,PasteText,SpellCheck,Undo,Redo,Bookmark,Hyperlink,HTMLSource,XHTMLSource,Numbering,Bullets,Indent,Outdent,JustifyLeft,JustifyCenter,JustifyRight,JustifyFull,Table,Guidelines,Absolute,Characters,Line,Form,RemoveFormat,ClearAll,StyleAndFormatting,TextFormatting,ListFormatting,BoxFormatting,ParagraphFormatting,CssText,Styles,Paragraph,FontName,FontSize,Bold,Italic,Underline,Strikethrough,Superscript,Subscript,ForeColor,BackColor";
//        //'
//        //' Content Type
//        //'
//        //Enum contentTypeEnum
//        //    contentTypeWeb = 1
//        //    ContentTypeEmail = 2
//        //    contentTypeWebTemplate = 3
//        //    contentTypeEmailTemplate = 4
//        //End Enum
//        //Public EditorContext As contentTypeEnum
//        //Enum EditorContextEnum
//        //    contentTypeWeb = 1
//        //    contentTypeEmail = 2
//        //End Enum
//        //Public EditorContext As EditorContextEnum
//        //'
//        //Public Const EditorAddonMenuEmailTemplateFilename = "templates/EditorAddonMenuTemplateEmail.js"
//        //Public Const EditorAddonMenuEmailContentFilename = "templates/EditorAddonMenuContentEmail.js"
//        //Public Const EditorAddonMenuWebTemplateFilename = "templates/EditorAddonMenuTemplateWeb.js"
//        //Public Const EditorAddonMenuWebContentFilename = "templates/EditorAddonMenuContentWeb.js"
//        //
//        public const DynamicStylesFilename = "templates/styles.css";
//        public const AdminSiteStylesFilename = "templates/AdminSiteStyles.css";
//        public const EditorStyleRulesFilenamePattern = "templates/EditorStyleRules$TemplateID$.js";
//        // deprecated 11/24/3009 - StyleRules destinction between web/email not needed b/c body background blocked
//        //Public Const EditorStyleWebRulesFilename = "templates/EditorStyleWebRules.js"
//        //Public Const EditorStyleEmailRulesFilename = "templates/EditorStyleEmailRules.js"
//        //
//        // ----- ccGroupRules storage for list of Content that a group can author
//        //
//        public class ContentGroupRuleType
//        {
//            int ContentID;
//            int GroupID;
//            bool AllowAdd;
//            bool AllowDelete;
//        }
//        //
//        // ----- This should match the Lookup List in the NavIconType field in the Navigator Entry content definition
//        //
//        public const navTypeIDList = "Add-on,Report,Setting,Tool";
//        public const NavTypeIDAddon = 1;
//        public const NavTypeIDReport = 2;
//        public const NavTypeIDSetting = 3;
//        public const NavTypeIDTool = 4;
//        //
//        public const NavIconTypeList = "Custom,Advanced,Content,Folder,Email,User,Report,Setting,Tool,Record,Addon,help";
//        public const NavIconTypeCustom = 1;
//        public const NavIconTypeAdvanced = 2;
//        public const NavIconTypeContent = 3;
//        public const NavIconTypeFolder = 4;
//        public const NavIconTypeEmail = 5;
//        public const NavIconTypeUser = 6;
//        public const NavIconTypeReport = 7;
//        public const NavIconTypeSetting = 8;
//        public const NavIconTypeTool = 9;
//        public const NavIconTypeRecord = 10;
//        public const NavIconTypeAddon = 11;
//        public const NavIconTypeHelp = 12;
//        //
//        public const QueryTypeSQL = 1;
//        public const QueryTypeOpenContent = 2;
//        public const QueryTypeUpdateContent = 3;
//        public const QueryTypeInsertContent = 4;
//        //
//        // Google Data Object construction in GetRemoteQuery
//        //
//        public class ColsType
//        {
//            public string Type;
//            public string Id;
//            public string Label;
//            public string Pattern;
//        }
//        //
//        public class CellType
//        {
//            public string v;
//            public string f;
//            public string p;
//        }
//        //
//        public class RowsType
//        {
//            public CellType[] Cell;
//        }
//        //
//        public class GoogleDataType
//        {
//            public bool IsEmpty;
//            public ColsType[] col;
//            public RowsType[] row;
//        }
//        //
//        public enum GoogleVisualizationStatusEnum
//        {
//            OK = 1,
//            warning = 2,
//            ErrorStatus = 3
//        }
//        //
//        public class GoogleVisualizationType
//        {
//            public string version;
//            public string reqid;
//            public GoogleVisualizationStatusEnum status;
//            public string[] warnings;
//            public string[] errors;
//            public string sig;
//            public GoogleDataType table;
//        }

//        //Public Const ReturnFormatTypeGoogleTable = 1
//        //Public Const ReturnFormatTypeNameValue = 2

//        public enum RemoteFormatEnum
//        {
//            RemoteFormatJsonTable = 1,
//            RemoteFormatJsonNameArray = 2,
//            RemoteFormatJsonNameValue = 3
//        }
//        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        //Private Enum GoogleVisualizationStatusEnum
//        //    OK = 1
//        //    warning = 2
//        //    ErrorStatus = 3
//        //End Enum
//        //'
//        //Private Structure GoogleVisualizationType
//        //    Dim version As String
//        //    Dim reqid As String
//        //    Dim status As GoogleVisualizationStatusEnum
//        //    Dim warnings() As String
//        //    Dim errors() As String
//        //    Dim sig As String
//        //    Dim table As GoogleDataType
//        //End Structure        '
//        //
//        //
//        public static extern void RegCloseKey(hKey);
//        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        public static extern void RegOpenKeyExA(hKey, lpszSubKey, dwOptions, samDesired, lpHKey);
//        [DllImport("advapi32.dll", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        public static extern void RegQueryValueExA(hKey, lpszValueName, lpdwRes, lpdwType, lpDataBuff, nSize);
//        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueExA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
//        public static extern void RegQueryValueEx(hKey, lpszValueName, lpdwRes, lpdwType, lpDataBuff, nSize);

//        public const HKEY_CLASSES_ROOT = 0x80000000;
//        public const HKEY_CURRENT_USER = 0x80000001;
//        public const HKEY_LOCAL_MACHINE = 0x80000002;

//        public const HKEY_USERS = 0x80000003;
//        public const ERROR_SUCCESS = 0L;
//        // Unicode nul terminated string
//        public const REG_SZ = 1L;
//        // 32-bit number
//        public const REG_DWORD = 4L;

//        public const KEY_QUERY_VALUE = 0x1L;
//        public const KEY_SET_VALUE = 0x2L;
//        public const KEY_CREATE_SUB_KEY = 0x4L;
//        public const KEY_ENUMERATE_SUB_KEYS = 0x8L;
//        public const KEY_NOTIFY = 0x10L;
//        public const KEY_CREATE_LINK = 0x20L;
//        public const READ_CONTROL = 0x20000;
//        public const WRITE_DAC = 0x40000;
//        public const WRITE_OWNER = 0x80000;
//        public const SYNCHRONIZE = 0x100000;
//        public const STANDARD_RIGHTS_REQUIRED = 0xf0000;
//        public const STANDARD_RIGHTS_READ = READ_CONTROL;
//        public const STANDARD_RIGHTS_WRITE = READ_CONTROL;
//        public const STANDARD_RIGHTS_EXECUTE = READ_CONTROL;
//        public const KEY_READ = STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_ENUMERATE_SUB_KEYS | KEY_NOTIFY;
//        public const KEY_WRITE = STANDARD_RIGHTS_WRITE | KEY_SET_VALUE | KEY_CREATE_SUB_KEY;

//        public const KEY_EXECUTE = KEY_READ;
//        //    '======================================================================================
//        //    '
//        //    '======================================================================================
//        //    '
//        //    Public Sub StartDebugTimer(Enabled As Boolean, Label As String)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        If Enabled Then
//        //            If TimerStackCount < TimerStackMax Then
//        //                TimerStack(TimerStackCount).Label = Label
//        //                TimerStack(TimerStackCount).StartTicks = GetTickCount
//        //            Else
//        //                Call AppendLogFile("dll" & ".?.StartDebugTimer, " & "Timer Stack overflow, attempting push # [" & TimerStackCount & "], but max = [" & TimerStackMax & "]")
//        //            End If
//        //            TimerStackCount = TimerStackCount + 1
//        //        End If
//        //    End Sub
//        //    '
//        //    Public Sub StopDebugTimer(Enabled As Boolean, Label As String)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        If Enabled Then
//        //            If TimerStackCount <= 0 Then
//        //                Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, attempting to Pop, but the stack is empty")
//        //            Else
//        //                If TimerStackCount <= TimerStackMax Then
//        //                    If TimerStack(TimerStackCount - 1).Label = Label Then
//        //                    Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer [" & String(2 * TimerStackCount, ".") & Label & "] took " & (GetTickCount - TimerStack(TimerStackCount - 1).StartTicks) & " msec")
//        //                    Else
//        //                        Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, [" & Label & "] was popped, but [" & TimerStack(TimerStackCount).Label & "] was on the top of the stack")
//        //                    End If
//        //                End If
//        //                TimerStackCount = TimerStackCount - 1
//        //            End If
//        //        End If
//        //    End Sub
//        //    '
//        //    '
//        //    '
//        //    Public Function PayString(Index) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        Select Case Index
//        //            Case PayTypeCreditCardOnline
//        //                PayString = "Credit Card"
//        //            Case PayTypeCreditCardByPhone
//        //                PayString = "Credit Card by phone"
//        //            Case PayTypeCreditCardByFax
//        //                PayString = "Credit Card by fax"
//        //            Case PayTypeCHECK
//        //                PayString = "Personal Check"
//        //            Case PayTypeCHECKCOMPANY
//        //                PayString = "Company Check"
//        //            Case PayTypeCREDIT
//        //                PayString = "You will be billed"
//        //            Case PayTypeNetTerms
//        //                PayString = "Net Terms (Approved customers only)"
//        //            Case PayTypeCODCompanyCheck
//        //                PayString = "COD- Pre-Approved Only"
//        //            Case PayTypeCODCertifiedFunds
//        //                PayString = "COD- Certified Funds"
//        //            Case PayTypePAYPAL
//        //                PayString = "PayPal"
//        //            Case Else
//        //                ' Case PayTypeNONE
//        //                PayString = "No payment required"
//        //        End Select
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //    Public Function CCString(Index) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        Select Case Index
//        //            Case CCTYPEVISA
//        //                CCString = "Visa"
//        //            Case CCTYPEMC
//        //                CCString = "MasterCard"
//        //            Case CCTYPEAMEX
//        //                CCString = "American Express"
//        //            Case CCTYPEDISCOVER
//        //                CCString = "Discover"
//        //            Case Else
//        //                ' Case CCTYPENOVUS
//        //                CCString = "Novus Card"
//        //        End Select
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' Get a Long from a CommandPacket
//        //    '   position+0, 4 byte value
//        //    '========================================================================
//        //    '
//        //    Public Function GetLongFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        GetLongFromByteArray = ByteArray(Position + 3)
//        //        GetLongFromByteArray = ByteArray(Position + 2) + (256 * GetLongFromByteArray)
//        //        GetLongFromByteArray = ByteArray(Position + 1) + (256 * GetLongFromByteArray)
//        //        GetLongFromByteArray = ByteArray(Position + 0) + (256 * GetLongFromByteArray)
//        //        Position = Position + 4
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' Get a Long from a byte array
//        //    '   position+0, 4 byte size of the number
//        //    '   position+3, start of the number
//        //    '========================================================================
//        //    '
//        //    Public Function GetNumberFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim ArgumentCount As Integer
//        //        Dim ArgumentLength As Integer
//        //        '
//        //        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
//        //        '
//        //        If ArgumentLength > 0 Then
//        //            GetNumberFromByteArray = 0
//        //            For ArgumentCount = ArgumentLength - 1 To 0 Step -1
//        //                GetNumberFromByteArray = ByteArray(Position + ArgumentCount) + (256 * GetNumberFromByteArray)
//        //            Next
//        //        End If
//        //        Position = Position + ArgumentLength
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' Get a String a byte array
//        //    '   position+0, 4 byte length of the string
//        //    '   position+3, start of the string
//        //    '========================================================================
//        //    '
//        //    Public Function GetStringFromByteArray(ByteArray() As Byte, Position As Integer) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim Pointer As Integer
//        //        Dim ArgumentLength As Integer
//        //        '
//        //        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
//        //        '
//        //        GetStringFromByteArray = ""
//        //        If ArgumentLength > 0 Then
//        //            For Pointer = 0 To ArgumentLength - 1
//        //                GetStringFromByteArray = GetStringFromByteArray & chr(ByteArray(Position + Pointer))
//        //            Next
//        //        End If
//        //        Position = Position + ArgumentLength
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' Get a Long from a byte array
//        //    '========================================================================
//        //    '
//        //    Public Sub SetLongByteArray(ByRef ByteArray() As Byte, Position As Integer, LongValue As Integer)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        ByteArray(Position + 0) = LongValue And (&HFF)
//        //        ByteArray(Position + 1) = Int(LongValue / 256) And (&HFF)
//        //        ByteArray(Position + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
//        //        ByteArray(Position + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
//        //        Position = Position + 4
//        //        '
//        //    End Sub
//        //    '
//        //    '========================================================================
//        //    ' Set a string in a byte array
//        //    '========================================================================
//        //    '
//        //    Public Sub SetStringByteArray(ByRef ByteArray() As Byte, Position As Integer, StringValue As String)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim Pointer As Integer
//        //        Dim LenStringValue As Integer
//        //        '
//        //        LenStringValue = Len(StringValue)
//        //        If LenStringValue > 0 Then
//        //            For Pointer = 0 To LenStringValue - 1
//        //                ByteArray(Position + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
//        //            Next
//        //            Position = Position + LenStringValue
//        //        End If
//        //        '
//        //    End Sub

//        //    '
//        //    '========================================================================
//        //    '   a Long long on the end of a RMB (Remote Method Block)
//        //    '       You determine the position, or it will add it to the end
//        //    '========================================================================
//        //    '
//        //Public Sub SetRMBLong(ByRef ByteArray() As Byte, LongValue As Integer, Optional Position)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim Temp As Integer
//        //        Dim MyPosition As Integer
//        //        Dim ByteArraySize As Integer
//        //        '
//        //        ' ----- determine the position
//        //        '
//        //        If Not IsMissing(Position) Then
//        //            MyPosition = Position
//        //        Else
//        //            '
//        //            ' ----- Add it to the end, determine length
//        //            '
//        //            MyPosition = ByteArray(RMBPositionLength + 3)
//        //            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
//        //            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
//        //            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
//        //            '
//        //            ' ----- adjust size of array if necessary
//        //            '
//        //            ByteArraySize = UBound(ByteArray)
//        //            If ByteArraySize < (MyPosition + 8) Then
//        //                ReDim Preserve ByteArray(ByteArraySize + 8)
//        //            End If
//        //        End If
//        //        '
//        //        ' ----- set the length
//        //        '
//        //        'ByteArray(MyPosition + 0) = 4
//        //        'ByteArray(MyPosition + 1) = 0
//        //        'ByteArray(MyPosition + 2) = 0
//        //        'ByteArray(MyPosition + 3) = 0
//        //        'MyPosition = MyPosition + 4
//        //        '
//        //        ' ----- set the value
//        //        '
//        //        ByteArray(MyPosition + 0) = LongValue And (&HFF)
//        //        ByteArray(MyPosition + 1) = Int(LongValue / 256) And (&HFF)
//        //        ByteArray(MyPosition + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
//        //        ByteArray(MyPosition + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
//        //        MyPosition = MyPosition + 4
//        //        '
//        //        If IsMissing(Position) Then
//        //            '
//        //            ' ----- Adjust the RMB length if length not given
//        //            '
//        //            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
//        //            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
//        //            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
//        //            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
//        //        End If
//        //        '
//        //    End Sub
//        //    '
//        //    '========================================================================
//        //    '   a Long long on the end of a RMB (Remote Method Block)
//        //    '       You determine the position, or it will add it to the end
//        //    '========================================================================
//        //    '
//        //Public Sub SetRMBString(ByRef ByteArray() As Byte, StringValue As String, Optional Position)
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim Temp As Integer
//        //        Dim MyPosition As Integer
//        //        Dim ByteArraySize As Integer
//        //        '
//        //        ' ----- determine the position
//        //        '
//        //        If Not IsMissing(Position) Then
//        //            MyPosition = Position
//        //        Else
//        //            '
//        //            ' ----- Add it to the end, determine length
//        //            '
//        //            MyPosition = ByteArray(RMBPositionLength + 3)
//        //            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
//        //            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
//        //            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
//        //            '
//        //            ' ----- adjust size of array if necessary
//        //            '
//        //            ByteArraySize = UBound(ByteArray)
//        //            If ByteArraySize < (MyPosition + 8) Then
//        //                ReDim Preserve ByteArray(ByteArraySize + 4 + Len(StringValue))
//        //            End If
//        //        End If
//        //        '
//        //        ' ----- set the value
//        //        '

//        //        '
//        //        Dim Pointer As Integer
//        //        Dim LenStringValue As Integer
//        //        '
//        //        LenStringValue = Len(StringValue)
//        //        If LenStringValue > 0 Then
//        //            For Pointer = 0 To LenStringValue - 1
//        //                ByteArray(MyPosition + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
//        //            Next
//        //            MyPosition = MyPosition + LenStringValue
//        //        End If
//        //        '
//        //        If IsMissing(Position) Then
//        //            '
//        //            ' ----- Adjust the RMB length if length not given
//        //            '
//        //            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
//        //            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
//        //            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
//        //            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
//        //        End If
//        //        '
//        //    End Sub
//        //    '
//        //    '========================================================================
//        //    '   IsTrue
//        //    '       returns true or false depending on the state of the variant input
//        //    '========================================================================
//        //    '
//        //    Function IsTrue(ValueVariant) As Boolean
//        //        IsTrue = encodeBoolean(ValueVariant)
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' EncodeXML
//        //    '
//        //    '========================================================================
//        //    '
//        //    Function EncodeXML(ValueVariant As Object, fieldType As Integer) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim TimeValuething As Single
//        //        Dim TimeHours As Integer
//        //        Dim TimeMinutes As Integer
//        //        Dim TimeSeconds As Integer
//        //        '
//        //        Select Case fieldType
//        //            Case FieldTypeInteger, FieldTypeLookup, FieldTypeRedirect, FieldTypeManyToMany, FieldTypeMemberSelect
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "null"
//        //                ElseIf ValueVariant = "" Then
//        //                    EncodeXML = "null"
//        //                ElseIf vbIsNumeric(ValueVariant) Then
//        //                    EncodeXML = Int(ValueVariant)
//        //                Else
//        //                    EncodeXML = "null"
//        //                End If
//        //            Case FieldTypeBoolean
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "0"
//        //                ElseIf ValueVariant <> False Then
//        //                    EncodeXML = "1"
//        //                Else
//        //                    EncodeXML = "0"
//        //                End If
//        //            Case FieldTypeCurrency
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "null"
//        //                ElseIf ValueVariant = "" Then
//        //                    EncodeXML = "null"
//        //                ElseIf vbIsNumeric(ValueVariant) Then
//        //                    EncodeXML = ValueVariant
//        //                Else
//        //                    EncodeXML = "null"
//        //                End If
//        //            Case FieldTypeFloat
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "null"
//        //                ElseIf ValueVariant = "" Then
//        //                    EncodeXML = "null"
//        //                ElseIf vbIsNumeric(ValueVariant) Then
//        //                    EncodeXML = ValueVariant
//        //                Else
//        //                    EncodeXML = "null"
//        //                End If
//        //            Case FieldTypeDate
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "null"
//        //                ElseIf ValueVariant = "" Then
//        //                    EncodeXML = "null"
//        //                ElseIf IsDate(ValueVariant) Then
//        //                    'TimeVar = CDate(ValueVariant)
//        //                    'TimeValuething = 86400! * (TimeVar - Int(TimeVar))
//        //                    'TimeHours = Int(TimeValuething / 3600!)
//        //                    'TimeMinutes = Int(TimeValuething / 60!) - (TimeHours * 60)
//        //                    'TimeSeconds = TimeValuething - (TimeHours * 3600!) - (TimeMinutes * 60!)
//        //                    'EncodeXML = Year(ValueVariant) & "-" & Right("0" & Month(ValueVariant), 2) & "-" & Right("0" & Day(ValueVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2)
//        //                    EncodeXML = encodeText(ValueVariant)
//        //                End If
//        //            Case Else
//        //                '
//        //                ' ----- FieldTypeText
//        //                ' ----- FieldTypeLongText
//        //                ' ----- FieldTypeFile
//        //                ' ----- FieldTypeImage
//        //                ' ----- FieldTypeTextFile
//        //                ' ----- FieldTypeCSSFile
//        //                ' ----- FieldTypeXMLFile
//        //                ' ----- FieldTypeJavascriptFile
//        //                ' ----- FieldTypeLink
//        //                ' ----- FieldTypeResourceLink
//        //                ' ----- FieldTypeHTML
//        //                ' ----- FieldTypeHTMLFile
//        //                '
//        //                If IsNull(ValueVariant) Then
//        //                    EncodeXML = "null"
//        //                ElseIf ValueVariant = "" Then
//        //                    EncodeXML = ""
//        //                Else
//        //                    'EncodeXML = ASPServer.HTMLEncode(ValueVariant)
//        //                    'EncodeXML = vbReplace(ValueVariant, "&", "&lt;")
//        //                    'EncodeXML = vbReplace(ValueVariant, "<", "&lt;")
//        //                    'EncodeXML = vbReplace(EncodeXML, ">", "&gt;")
//        //                End If
//        //        End Select
//        //        '
//        //    End Function
//        //
//        //========================================================================
//        // EncodeFilename
//        //
//        //========================================================================
//        //
//        public static string encodeFilename(string Source)
//        {
//            string allowed = null;
//            string chr = null;
//            int Ptr = 0;
//            int Cnt = 0;
//            string returnString = null;
//            //
//            returnString = "";
//            Cnt = Strings.Len(Source);
//            if (Cnt > 254)
//            {
//                Cnt = 254;
//            }
//            allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_";
//            for (Ptr = 1; Ptr <= Cnt; Ptr++)
//            {
//                chr = Strings.Mid(Source, Ptr, 1);
//                if ((Strings.InStr(1, allowed, chr, Constants.vbBinaryCompare) >= 0))
//                {
//                    returnString = returnString + chr;
//                }
//            }
//            return returnString;
//        }
//        //    '
//        //    'Function encodeFilename(Filename As String) As String
//        //    '    ' ##### removed to catch err<>0 problem on error resume next
//        //    '    '
//        //    '    Dim Source() as object
//        //    '    Dim Replacement() as object
//        //    '    '
//        //    '    Source = Array("""", "*", "/", ":", "<", ">", "?", "\", "|", "=")
//        //    '    Replacement = Array("_", "_", "_", "_", "_", "_", "_", "_", "_", "_")
//        //    '    '
//        //    '    encodeFilename = ReplaceMany(Filename, Source, Replacement)
//        //    '    If Len(encodeFilename) > 254 Then
//        //    '        encodeFilename = Left(encodeFilename, 254)
//        //    '    End If
//        //    '    encodeFilename = vbReplace(encodeFilename, vbCr, "_")
//        //    '    encodeFilename = vbReplace(encodeFilename, vbLf, "_")
//        //    '    '
//        //    '    End Function
//        //    '
//        //    '
//        //    '

//        //    '
//        //    '========================================================================
//        //    ' DecodeHTML
//        //    '
//        //    '========================================================================
//        //    '
//        //    Function DecodeHTML(Source As String) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        DecodeHTML = decodeHtml(Source)
//        //        'Dim SourceChr() as object
//        //        'Dim ReplacementChr() as object
//        //        ''
//        //        'SourceChr = Array("&gt;", "&lt;", "&nbsp;", "&amp;")
//        //        'ReplacementChr = Array(">", "<", " ", "&")
//        //        ''
//        //        'DecodeHTML = ReplaceMany(Source, SourceChr, ReplacementChr)
//        //        '
//        //    End Function
//        //    '
//        //    '========================================================================
//        //    ' EncodeFilename
//        //    '
//        //    '========================================================================
//        //    '
//        //    Function ReplaceMany(Source As String, ArrayOfSource() As Object, ArrayOfReplacement() As Object) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim Count As Integer
//        //        Dim Pointer As Integer
//        //        '
//        //        Count = UBound(ArrayOfSource) + 1
//        //        ReplaceMany = Source
//        //        For Pointer = 0 To Count - 1
//        //            ReplaceMany = vbReplace(ReplaceMany, ArrayOfSource(Pointer), ArrayOfReplacement(Pointer))
//        //        Next
//        //        '
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //    Public Function GetURIHost(URI) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        '   Divide the URI into URIHost, URIPath, and URIPage
//        //        '
//        //        Dim URIWorking As String
//        //        Dim Slash As Integer
//        //        Dim LastSlash As Integer
//        //        Dim URIHost As String
//        //        Dim URIPath As String
//        //        Dim URIPage As String
//        //        URIWorking = URI
//        //        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
//        //            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
//        //        End If
//        //        URIHost = URIWorking
//        //        Slash = vbInstr(1, URIHost, "/")
//        //        If Slash = 0 Then
//        //            URIPath = "/"
//        //            URIPage = ""
//        //        Else
//        //            URIPath = Mid(URIHost, Slash)
//        //            URIHost = Mid(URIHost, 1, Slash - 1)
//        //            Slash = vbInstr(1, URIPath, "/")
//        //            Do While Slash <> 0
//        //                LastSlash = Slash
//        //                Slash = vbInstr(LastSlash + 1, URIPath, "/")
//        //                '''DoEvents()
//        //            Loop
//        //            URIPage = Mid(URIPath, LastSlash + 1)
//        //            URIPath = Mid(URIPath, 1, LastSlash)
//        //        End If
//        //        GetURIHost = URIHost
//        //        '
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //    Public Function GetURIPage(URI) As String
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        '   Divide the URI into URIHost, URIPath, and URIPage
//        //        '
//        //        Dim Slash As Integer
//        //        Dim LastSlash As Integer
//        //        Dim URIHost As String
//        //        Dim URIPath As String
//        //        Dim URIPage As String
//        //        Dim URIWorking As String
//        //        URIWorking = URI
//        //        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
//        //            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
//        //        End If
//        //        URIHost = URIWorking
//        //        Slash = vbInstr(1, URIHost, "/")
//        //        If Slash = 0 Then
//        //            URIPath = "/"
//        //            URIPage = ""
//        //        Else
//        //            URIPath = Mid(URIHost, Slash)
//        //            URIHost = Mid(URIHost, 1, Slash - 1)
//        //            Slash = vbInstr(1, URIPath, "/")
//        //            Do While Slash <> 0
//        //                LastSlash = Slash
//        //                Slash = vbInstr(LastSlash + 1, URIPath, "/")
//        //                '''DoEvents()
//        //            Loop
//        //            URIPage = Mid(URIPath, LastSlash + 1)
//        //            URIPath = Mid(URIPath, 1, LastSlash)
//        //        End If
//        //        GetURIPage = URIPage
//        //        '
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //    Function GetDateFromGMT(GMTDate As String) As Date
//        //        ' ##### removed to catch err<>0 problem on error resume next
//        //        '
//        //        Dim WorkString As String
//        //        GetDateFromGMT = 0
//        //        If GMTDate <> "" Then
//        //            WorkString = Mid(GMTDate, 6, 11)
//        //            If IsDate(WorkString) Then
//        //                GetDateFromGMT = CDate(WorkString)
//        //                WorkString = Mid(GMTDate, 18, 8)
//        //                If IsDate(WorkString) Then
//        //                    GetDateFromGMT = GetDateFromGMT + CDate(WorkString) + 4 / 24
//        //                End If
//        //            End If
//        //        End If
//        //        '
//        //    End Function
//        //
//        // Wdy, DD-Mon-YYYY HH:MM:SS GMT
//        //
//        public static string GetGMTFromDate(System.DateTime DateValue)
//        {
//            string functionReturnValue = null;
//            //
//            string WorkString = null;
//            int WorkLong = 0;
//            //
//            if (Information.IsDate(DateValue))
//            {
//                switch (Weekday(DateValue))
//                {
//                    case Constants.vbSunday:
//                        functionReturnValue = "Sun, ";
//                        break;
//                    case Constants.vbMonday:
//                        functionReturnValue = "Mon, ";
//                        break;
//                    case Constants.vbTuesday:
//                        functionReturnValue = "Tue, ";
//                        break;
//                    case Constants.vbWednesday:
//                        functionReturnValue = "Wed, ";
//                        break;
//                    case Constants.vbThursday:
//                        functionReturnValue = "Thu, ";
//                        break;
//                    case Constants.vbFriday:
//                        functionReturnValue = "Fri, ";
//                        break;
//                    case Constants.vbSaturday:
//                        functionReturnValue = "Sat, ";
//                        break;
//                }
//                //
//                WorkLong = Day(DateValue);
//                if (WorkLong < 10)
//                {
//                    functionReturnValue = functionReturnValue + "0" + Convert.ToString(WorkLong) + " ";
//                }
//                else {
//                    functionReturnValue = functionReturnValue + Convert.ToString(WorkLong) + " ";
//                }
//                //
//                switch (Month(DateValue))
//                {
//                    case 1:
//                        functionReturnValue = functionReturnValue + "Jan ";
//                        break;
//                    case 2:
//                        functionReturnValue = functionReturnValue + "Feb ";
//                        break;
//                    case 3:
//                        functionReturnValue = functionReturnValue + "Mar ";
//                        break;
//                    case 4:
//                        functionReturnValue = functionReturnValue + "Apr ";
//                        break;
//                    case 5:
//                        functionReturnValue = functionReturnValue + "May ";
//                        break;
//                    case 6:
//                        functionReturnValue = functionReturnValue + "Jun ";
//                        break;
//                    case 7:
//                        functionReturnValue = functionReturnValue + "Jul ";
//                        break;
//                    case 8:
//                        functionReturnValue = functionReturnValue + "Aug ";
//                        break;
//                    case 9:
//                        functionReturnValue = functionReturnValue + "Sep ";
//                        break;
//                    case 10:
//                        functionReturnValue = functionReturnValue + "Oct ";
//                        break;
//                    case 11:
//                        functionReturnValue = functionReturnValue + "Nov ";
//                        break;
//                    case 12:
//                        functionReturnValue = functionReturnValue + "Dec ";
//                        break;
//                }
//                //
//                functionReturnValue = functionReturnValue + Convert.ToString(Year(DateValue)) + " ";
//                //
//                WorkLong = Hour(DateValue);
//                if (WorkLong < 10)
//                {
//                    functionReturnValue = functionReturnValue + "0" + Convert.ToString(WorkLong) + ":";
//                }
//                else {
//                    functionReturnValue = functionReturnValue + Convert.ToString(WorkLong) + ":";
//                }
//                //
//                WorkLong = Minute(DateValue);
//                if (WorkLong < 10)
//                {
//                    functionReturnValue = functionReturnValue + "0" + Convert.ToString(WorkLong) + ":";
//                }
//                else {
//                    functionReturnValue = functionReturnValue + Convert.ToString(WorkLong) + ":";
//                }
//                //
//                WorkLong = Second(DateValue);
//                if (WorkLong < 10)
//                {
//                    functionReturnValue = functionReturnValue + "0" + Convert.ToString(WorkLong);
//                }
//                else {
//                    functionReturnValue = functionReturnValue + Convert.ToString(WorkLong);
//                }
//                //
//                functionReturnValue = functionReturnValue + " GMT";
//            }
//            return functionReturnValue;
//            //
//        }
//        //'    '
//        //'    '========================================================================
//        //'    '   EncodeSQL
//        //'    '       encode a variable to go in an sql expression
//        //'    '       NOT supported
//        //'    '========================================================================
//        //'    '
//        //Public Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
//        //    ' ##### removed to catch err<>0 problem on error resume next
//        //    '
//        //    Dim iFieldType As Integer
//        //    Dim MethodName As String
//        //    '
//        //    MethodName = "EncodeSQL"
//        //    '
//        //    iFieldType = fieldType
//        //    Select Case iFieldType
//        //        Case FieldTypeIdBoolean
//        //            EncodeSQL = app.EncodeSQLBoolean(expression)
//        //        Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
//        //            EncodeSQL = app.EncodeSQLNumber(expression)
//        //        Case FieldTypeIdDate
//        //            EncodeSQL = app.EncodeSQLDate(expression)
//        //        Case FieldTypeIdLongText, FieldTypeIdHTML
//        //            EncodeSQL = app.EncodeSQLText(expression)
//        //        Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
//        //            EncodeSQL = app.EncodeSQLText(expression)
//        //        Case Else
//        //            EncodeSQL = app.EncodeSQLText(expression)
//        //            On Error GoTo 0
//        //            Call Err.Raise(ignoreInteger, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
//        //    End Select
//        //    '
//        //End Function
//        //'
//        //'=====================================================================================================
//        //'   a value in a name/value pair
//        //'=====================================================================================================
//        //'
//        //Public Sub SetNameValueArrays(ByVal InputName As String, ByVal InputValue As String, ByRef SQLName() As String, ByRef SQLValue() As String, ByRef Index As Integer)
//        //    ' ##### removed to catch err<>0 problem on error resume next
//        //    '
//        //    SQLName(Index) = InputName
//        //    SQLValue(Index) = InputValue
//        //    Index = Index + 1
//        //    '
//        //End Sub
//        //    '
//        //    '
//        //    '
//        public static string GetApplicationStatusMessage(applicationStatusEnum ApplicationStatus)
//        {
//            string functionReturnValue = null;
//            switch (ApplicationStatus)
//            {
//                case applicationStatusEnum.ApplicationStatusDbFoundButContentMetaMissing:
//                    functionReturnValue = "The database for this application was found, but content meta table could not be read.";
//                    break;
//                case applicationStatusEnum.ApplicationStatusAppConfigNotValid:
//                    functionReturnValue = "The application configuration file on this front-end server is not valid.";
//                    break;
//                case applicationStatusEnum.ApplicationStatusAppConfigNotFound:
//                    functionReturnValue = "The application configuration file was not be found on this front-end server.";
//                    break;
//                case applicationStatusEnum.ApplicationStatusNoHostService:
//                    functionReturnValue = "Contensive server not running";
//                    break;
//                case applicationStatusEnum.ApplicationStatusNotFound:
//                    functionReturnValue = "Contensive application not found";
//                    break;
//                case applicationStatusEnum.ApplicationStatusNotEnabled:
//                    functionReturnValue = "Contensive application not running";
//                    break;
//                case applicationStatusEnum.ApplicationStatusReady:
//                    functionReturnValue = "Contensive application running";
//                    break;
//                case applicationStatusEnum.ApplicationStatusLoading:
//                    functionReturnValue = "Contensive application starting";
//                    break;
//                case applicationStatusEnum.ApplicationStatusUpgrading:
//                    functionReturnValue = "Contensive database upgrading";
//                    break;
//                case applicationStatusEnum.ApplicationStatusDbBad:
//                    functionReturnValue = "Error verifying core database records";
//                    break;
//                case applicationStatusEnum.ApplicationStatusDbNotFound:
//                    functionReturnValue = "Error opening application database";
//                    break;
//                case applicationStatusEnum.ApplicationStatusKernelFailure:
//                    functionReturnValue = "Error contacting Contensive kernel services";
//                    break;
//                case applicationStatusEnum.ApplicationStatusLicenseFailure:
//                    functionReturnValue = "Error verifying Contensive site license, see Http://www.Contensive.com/License";
//                    break;
//                case applicationStatusEnum.ApplicationStatusConnectionObjectFailure:
//                    functionReturnValue = "Error creating ODBC Connection object";
//                    break;
//                case applicationStatusEnum.ApplicationStatusConnectionStringFailure:
//                    functionReturnValue = "ODBC Data Source connection failed";
//                    break;
//                case applicationStatusEnum.ApplicationStatusDataSourceFailure:
//                    functionReturnValue = "Error opening default data source";
//                    break;
//                case applicationStatusEnum.ApplicationStatusDuplicateDomains:
//                    functionReturnValue = "Can not determine application because there are multiple applications with domain names that match this site's domain (See Application Manager)";
//                    break;
//                case applicationStatusEnum.ApplicationStatusFailedToInitialize:
//                    functionReturnValue = "Application failed to initialize, see trace log for details";
//                    break;
//                //Case applicationStatusEnum.ApplicationStatusPaused
//                //    GetApplicationStatusMessage = "Contensive application paused"
//                default:
//                    functionReturnValue = "Unknown status code [" + ApplicationStatus + "], see trace log for details";
//                    break;
//            }
//            return functionReturnValue;
//        }
//        //    '
//        //    '
//        //    '
//        //    Public Function GetFormInputSelectNameValue(SelectName As String, NameValueArray() As NameValuePairType) As String
//        //        Dim Pointer As Integer
//        //        Dim Source() As NameValuePairType
//        //        '
//        //        Source = NameValueArray
//        //        GetFormInputSelectNameValue = "<SELECT name=""" & SelectName & """ Size=""1"">"
//        //        For Pointer = 0 To UBound(NameValueArray)
//        //            GetFormInputSelectNameValue = GetFormInputSelectNameValue & "<OPTION value=""" & Source(Pointer).Value & """>" & Source(Pointer).Name & "</OPTION>"
//        //        Next
//        //        GetFormInputSelectNameValue = GetFormInputSelectNameValue & "</SELECT>"
//        //    End Function
//        //
//        //
//        //
//        public static string getSpacer(int Width, int Height)
//        {
//            return "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + Width + "\" height=\"" + Height + "\" border=\"0\">";
//        }
//        //
//        //
//        //
//        public static string processReplacement(object NameValueLines, object Source)
//        {
//            string functionReturnValue = null;
//            //
//            string iNameValueLines = null;
//            string[] Lines = null;
//            int LineCnt = 0;
//            int LinePtr = 0;
//            //
//            string[] Names = null;
//            string[] Values = null;
//            int PairPtr = 0;
//            int PairCnt = 0;
//            string[] Splits = null;
//            //
//            // ----- read pairs in from NameValueLines
//            //
//            iNameValueLines = EncodeText(NameValueLines);
//            if (vbInstr(1, iNameValueLines, "=") != 0)
//            {
//                PairCnt = 0;
//                Lines = SplitCRLF(iNameValueLines);
//                for (LinePtr = 0; LinePtr <= Information.UBound(Lines); LinePtr++)
//                {
//                    if (vbInstr(1, Lines(LinePtr), "=") != 0)
//                    {
//                        Splits = Strings.Split(Lines(LinePtr), "=");
//                        Array.Resize(ref Names, PairCnt + 1);
//                        Array.Resize(ref Names, PairCnt + 1);
//                        Array.Resize(ref Values, PairCnt + 1);
//                        Names(PairCnt) = Strings.Trim(Splits(0));
//                        Names(PairCnt) = vbReplace(Names(PairCnt), Constants.vbTab, "");
//                        Splits(0) = "";
//                        Values(PairCnt) = Strings.Trim(Splits(1));
//                        PairCnt = PairCnt + 1;
//                    }
//                }
//            }
//            //
//            // ----- Process replacements on Source
//            //
//            functionReturnValue = EncodeText(Source);
//            if (PairCnt > 0)
//            {
//                for (PairPtr = 0; PairPtr <= PairCnt - 1; PairPtr++)
//                {
//                    functionReturnValue = vbReplace(processReplacement(), Names(PairPtr), Values(PairPtr), 1, 99, Constants.vbTextCompare);
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //    '
//        //    '==========================================================================================================================
//        //    '   To convert from site license to server licenses, we still need the URLEncoder in the site license
//        //    '   This routine generates a site license that is just the URL encoder.
//        //    '==========================================================================================================================
//        //    '
//        //    Public Function GetURLEncoder() As String
//        //        Randomize()
//        //        GetURLEncoder = CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1 + (Rnd() * 8))) & CStr(Int(1000000000 + (Rnd() * 899999999)))
//        //    End Function
//        //    '
//        //    '==========================================================================================================================
//        //    '   To convert from site license to server licenses, we still need the URLEncoder in the site license
//        //    '   This routine generates a site license that is just the URL encoder.
//        //    '==========================================================================================================================
//        //    '
//        //    Public Function GetSiteLicenseKey() As String
//        //        GetSiteLicenseKey = "00000-00000-00000-" & GetURLEncoder
//        //    End Function
//        //    '
//        //    '
//        //    '
//        //Public Sub ccAddTabEntry(Caption As String, Link As String, IsHit As Boolean, Optional StylePrefix as string = "", Optional LiveBody as string = "")
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        If TabsCnt <= TabsSize Then
//        //            TabsSize = TabsSize + 10
//        //            ReDim Preserve Tabs(TabsSize)
//        //        End If
//        //        With Tabs(TabsCnt)
//        //            .Caption = Caption
//        //            .Link = Link
//        //            .IsHit = IsHit
//        //            .StylePrefix = encodeMissingText(StylePrefix, "cc")
//        //            .LiveBody = encodeMissingText(LiveBody, "")
//        //        End With
//        //        TabsCnt = TabsCnt + 1
//        //        '
//        //        Exit Sub
//        //        '
//        //ErrorTrap:
//        //        Call Err.Raise(Err.Number, Err.Source, "Error in ccAddTabEntry-" & Err.Description)
//        //    End Sub
//        //    '
//        //    '
//        //    '
//        //    Public Function OldccGetTabs() As String
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        Dim TabPtr As Integer
//        //        Dim HitPtr As Integer
//        //        Dim IsLiveTab As Boolean
//        //        Dim TabBody As String
//        //        Dim TabLink As String
//        //        Dim TabID As String
//        //        Dim FirstLiveBodyShown As Boolean
//        //        '
//        //        If TabsCnt > 0 Then
//        //            HitPtr = 0
//        //            '
//        //            ' Create TabBar
//        //            '
//        //            OldccGetTabs = OldccGetTabs & "<table border=0 cellspacing=0 cellpadding=0 align=center ><tr>"
//        //            For TabPtr = 0 To TabsCnt - 1
//        //                TabID = CStr(GetRandomInteger)
//        //                If Tabs(TabPtr).LiveBody = "" Then
//        //                    '
//        //                    ' This tab is linked to a page
//        //                    '
//        //                    TabLink = encodeHTML(Tabs(TabPtr).Link)
//        //                Else
//        //                    '
//        //                    ' This tab has a live body
//        //                    '
//        //                    TabLink = encodeHTML(Tabs(TabPtr).Link)
//        //                    If Not FirstLiveBodyShown Then
//        //                        FirstLiveBodyShown = True
//        //                        TabBody = TabBody & "<div style=""visibility: visible; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
//        //                    Else
//        //                        TabBody = TabBody & "<div style=""visibility: hidden; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """></div>"
//        //                    End If
//        //                End If
//        //                OldccGetTabs = OldccGetTabs & "<td valign=bottom>"
//        //                If Tabs(TabPtr).IsHit And (HitPtr = 0) Then
//        //                    HitPtr = TabPtr
//        //                    '
//        //                    ' This tab is hit
//        //                    '
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<table cellspacing=0 cellPadding=0 border=0>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=2 height=1 width=2></td>" _
//        //                        & "<td colspan=1 height=1 bgcolor=black></td>" _
//        //                        & "<td colspan=3 height=1 width=3></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=1 height=1 width=1></td>" _
//        //                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
//        //                        & "<td colspan=1 height=1></td>" _
//        //                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
//        //                        & "<td colspan=2 height=1 width=2></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=1 height=2 bgcolor=black></td>" _
//        //                        & "<td colspan=1 height=2></td>" _
//        //                        & "<td colspan=1 height=2></td>" _
//        //                        & "<td colspan=1 height=2></td>" _
//        //                        & "<td colspan=1 height=2 width=1 bgcolor=black></td>" _
//        //                        & "<td colspan=1 height=2 width=1></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td></td>" _
//        //                        & "<td>" _
//        //                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
//        //                        & "<td Class=""ccTabHit"">&nbsp;<a href=""" & TabLink & """ Class=""ccTabHit"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
//        //                        & "</tr></table >" _
//        //                        & "</td>" _
//        //                        & "<td></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td></td>" _
//        //                        & "<td></td>" _
//        //                        & "<td></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "</tr>" _
//        //                        & "</table >"
//        //                Else
//        //                    '
//        //                    ' This tab is not hit
//        //                    '
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<table cellspacing=0 cellPadding=0 border=0>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=6 height=1></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=2 height=1></td>" _
//        //                        & "<td colspan=1 height=1 bgcolor=black></td>" _
//        //                        & "<td colspan=3 height=1></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td width=1></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=2 colspan=2></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=1></td>" _
//        //                        & "<td nowrap>" _
//        //                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
//        //                        & "<td Class=""ccTab"">&nbsp;<a href=""" & TabLink & """ Class=""ccTab"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
//        //                        & "</tr></table >" _
//        //                        & "</td>" _
//        //                        & "<td width=1></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=1></td>" _
//        //                        & "</tr>"
//        //                    OldccGetTabs = OldccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=6 height=1 bgcolor=black></td>" _
//        //                        & "</tr>" _
//        //                        & "</table >"
//        //                End If
//        //                OldccGetTabs = OldccGetTabs & "</td>"
//        //            Next
//        //            OldccGetTabs = OldccGetTabs & "<td class=""ccTabEnd"">&nbsp;</td></tr>"
//        //            If TabBody <> "" Then
//        //                OldccGetTabs = OldccGetTabs & "<tr><td colspan=6>" & TabBody & "</td></tr>"
//        //            End If
//        //            OldccGetTabs = OldccGetTabs & "</tr></table >"
//        //            TabsCnt = 0
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //ErrorTrap:
//        //        Call Err.Raise(Err.Number, Err.Source, "Error in OldccGetTabs-" & Err.Description)
//        //    End Function


//        //    '
//        //    '
//        //    '
//        //    Public Function ccGetTabs() As String
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        Dim TabPtr As Integer
//        //        Dim HitPtr As Integer
//        //        Dim IsLiveTab As Boolean
//        //        Dim TabBody As String
//        //        Dim TabLink As String
//        //        Dim TabID As String
//        //        Dim FirstLiveBodyShown As Boolean
//        //        '
//        //        If TabsCnt > 0 Then
//        //            HitPtr = 0
//        //            '
//        //            ' Create TabBar
//        //            '
//        //            ccGetTabs = ccGetTabs & "<table border=0 cellspacing=0 cellpadding=0 align=center ><tr>"
//        //            For TabPtr = 0 To TabsCnt - 1
//        //                TabID = CStr(GetRandomInteger)
//        //                If Tabs(TabPtr).LiveBody = "" Then
//        //                    '
//        //                    ' This tab is linked to a page
//        //                    '
//        //                    TabLink = encodeHTML(Tabs(TabPtr).Link)
//        //                Else
//        //                    '
//        //                    ' This tab has a live body
//        //                    '
//        //                    TabLink = encodeHTML(Tabs(TabPtr).Link)
//        //                    If Not FirstLiveBodyShown Then
//        //                        FirstLiveBodyShown = True
//        //                        TabBody = TabBody & "<div style=""visibility: visible; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """>" & Tabs(TabPtr).LiveBody & "</div>"
//        //                    Else
//        //                        TabBody = TabBody & "<div style=""visibility: hidden; position: absolute; left: 0px;"" class=""" & Tabs(TabPtr).StylePrefix & "Body"" id=""" & TabID & """>" & Tabs(TabPtr).LiveBody & "</div>"
//        //                    End If
//        //                End If
//        //                ccGetTabs = ccGetTabs & "<td valign=bottom>"
//        //                If Tabs(TabPtr).IsHit And (HitPtr = 0) Then
//        //                    HitPtr = TabPtr
//        //                    '
//        //                    ' This tab is hit
//        //                    '
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<table cellspacing=0 cellPadding=0 border=0>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=2 height=1 width=2></td>" _
//        //                        & "<td colspan=1 height=1 bgcolor=black></td>" _
//        //                        & "<td colspan=3 height=1 width=3></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=1 height=1 width=1></td>" _
//        //                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
//        //                        & "<td Class=""ccTabHit"" colspan=1 height=1></td>" _
//        //                        & "<td colspan=1 height=1 width=1 bgcolor=black></td>" _
//        //                        & "<td colspan=2 height=1 width=2></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=1 height=2 bgcolor=black></td>" _
//        //                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
//        //                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
//        //                        & "<td Class=""ccTabHit"" colspan=1 height=2></td>" _
//        //                        & "<td colspan=1 height=2 bgcolor=black></td>" _
//        //                        & "<td colspan=1 height=2></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td Class=""ccTabHit""></td>" _
//        //                        & "<td Class=""ccTabHit"">" _
//        //                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
//        //                        & "<td Class=""ccTabHit"">&nbsp;<a href=""" & TabLink & """ Class=""ccTabHit"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
//        //                        & "</tr></table >" _
//        //                        & "</td>" _
//        //                        & "<td Class=""ccTabHit""></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td Class=""ccTabHit""></td>" _
//        //                        & "<td Class=""ccTabHit""></td>" _
//        //                        & "<td Class=""ccTabHit""></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "<td bgcolor=black></td>" _
//        //                        & "</tr>" _
//        //                        & "</table >"
//        //                Else
//        //                    '
//        //                    ' This tab is not hit
//        //                    '
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<table cellspacing=0 cellPadding=0 border=0>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=6 height=1></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=2 height=1></td>" _
//        //                        & "<td colspan=1 height=1 bgcolor=black></td>" _
//        //                        & "<td colspan=3 height=1></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td width=1></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td Class=""ccTab""></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=2 colspan=2></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=1 Class=""ccTab""></td>" _
//        //                        & "<td nowrap Class=""ccTab"">" _
//        //                        & "<table cellspacing=0 cellPadding=2 border=0><tr>" _
//        //                        & "<td Class=""ccTab"">&nbsp;<a href=""" & TabLink & """ Class=""ccTab"">" & Tabs(TabPtr).Caption & "</a>&nbsp;</td>" _
//        //                        & "</tr></table >" _
//        //                        & "</td>" _
//        //                        & "<td width=1 Class=""ccTab""></td>" _
//        //                        & "<td width=1 bgcolor=black></td>" _
//        //                        & "<td width=1></td>" _
//        //                        & "</tr>"
//        //                    ccGetTabs = ccGetTabs _
//        //                        & "<tr>" _
//        //                        & "<td colspan=6 height=1 bgcolor=black></td>" _
//        //                        & "</tr>" _
//        //                        & "</table >"
//        //                End If
//        //                ccGetTabs = ccGetTabs & "</td>"
//        //            Next
//        //            ccGetTabs = ccGetTabs & "<td class=""ccTabEnd"">&nbsp;</td></tr>"
//        //            If TabBody <> "" Then
//        //                ccGetTabs = ccGetTabs & "<tr><td colspan=6>" & TabBody & "</td></tr>"
//        //            End If
//        //            ccGetTabs = ccGetTabs & "</tr></table >"
//        //            TabsCnt = 0
//        //        End If
//        //        '
//        //        Exit Function
//        //        '
//        //ErrorTrap:
//        //        Call Err.Raise(Err.Number, Err.Source, "Error in ccGetTabs-" & Err.Description)
//        //    End Function
//        //
//        //
//        //
//        public static string ConvertLinksToAbsolute(string Source, string RootLink)
//        {
//            string functionReturnValue = null;
//            // ERROR: Not supported in C#: OnErrorStatement

//            //
//            string s = null;
//            //
//            s = Source;
//            //
//            s = vbReplace(s, " href=\"", " href=\"/", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=\"/http", " href=\"http", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=\"/mailto", " href=\"mailto", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=\"//", " href=\"" + RootLink, 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=\"/?", " href=\"" + RootLink + "?", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=\"/", " href=\"" + RootLink, 1, 99, Constants.vbTextCompare);
//            //
//            s = vbReplace(s, " href=", " href=/", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=/\"", " href=\"", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=/http", " href=http", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=//", " href=" + RootLink, 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=/?", " href=" + RootLink + "?", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " href=/", " href=" + RootLink, 1, 99, Constants.vbTextCompare);
//            //
//            s = vbReplace(s, " src=\"", " src=\"/", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=\"/http", " src=\"http", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=\"//", " src=\"" + RootLink, 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=\"/?", " src=\"" + RootLink + "?", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=\"/", " src=\"" + RootLink, 1, 99, Constants.vbTextCompare);
//            //
//            s = vbReplace(s, " src=", " src=/", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=/\"", " src=\"", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=/http", " src=http", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=//", " src=" + RootLink, 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=/?", " src=" + RootLink + "?", 1, 99, Constants.vbTextCompare);
//            s = vbReplace(s, " src=/", " src=" + RootLink, 1, 99, Constants.vbTextCompare);
//            //
//            functionReturnValue = s;
//            return functionReturnValue;
//            ErrorTrap:
//            //
//            //
//            Err.Raise(Err.Number, Err.Source, "Error in ConvertLinksToAbsolute-" + Err.Description);
//            return functionReturnValue;
//        }
//        //    '
//        //    '
//        //    '
//        //Public Function getAppPath() As String
//        //    Dim Ptr As Integer
//        //    getAppPath = App.Path
//        //    Ptr = vbInstr(1, getAppPath, "\github\", vbTextCompare)
//        //    If Ptr <> 0 Then
//        //        ' for ...\github\contensive4?\bin"
//        //        Ptr = vbInstr(Ptr + 8, getAppPath, "\")
//        //        getAppPath = Left(getAppPath, Ptr) & "bin"
//        //    End If
//        //End Function
//        //'
//        //'
//        //'
//        //Public Function GetAddonRootPath() As String
//        //    Dim testPath As String
//        //    '
//        //    GetAddonRootPath = getAppPath & "\addons"
//        //    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
//        //        '
//        //        ' debugging - change program path to dummy path so addon builds all copy to
//        //        '
//        //        testPath = Environ$("programfiles(x86)")
//        //        If testPath = "" Then
//        //            testPath = Environ$("programfiles")
//        //        End If
//        //        GetAddonRootPath = testPath & "\kma\contensive\addons"
//        //    End If
//        //End Function
//        //
//        //
//        //
//        public static string GetHTMLComment(string Comment)
//        {
//            return "<!-- " + Comment + " -->";
//        }
//        //
//        //
//        //
//        public static string[] SplitCRLF(string Expression)
//        {
//            string[] functionReturnValue = null;
//            string[] Args = null;
//            int Ptr = 0;
//            //
//            if (vbInstr(1, Expression, Constants.vbCrLf) != 0)
//            {
//                functionReturnValue = Strings.Split(Expression, Constants.vbCrLf, , Constants.vbTextCompare);
//            }
//            else if (vbInstr(1, Expression, Constants.vbCr) != 0)
//            {
//                functionReturnValue = Strings.Split(Expression, Constants.vbCr, , Constants.vbTextCompare);
//            }
//            else if (vbInstr(1, Expression, Constants.vbLf) != 0)
//            {
//                functionReturnValue = Strings.Split(Expression, Constants.vbLf, , Constants.vbTextCompare);
//            }
//            else {
//                // ERROR: Not supported in C#: ReDimStatement

//                functionReturnValue = Strings.Split(Expression, Constants.vbCrLf);
//            }
//            return functionReturnValue;
//        }
//        //    '
//        //    '
//        //    '
//        //Public Sub runProcess(cp.core,Cmd As String, Optional ByVal eWindowStyle As VBA.VbAppWinStyle = vbHide, Optional WaitForReturn As Boolean)
//        //        On Error GoTo ErrorTrap
//        //        '
//        //        Dim ShellObj As Object
//        //        '
//        //        ShellObj = CreateObject("WScript.Shell")
//        //        If Not (ShellObj Is Nothing) Then
//        //            Call ShellObj.Run(Cmd, 0, WaitForReturn)
//        //        End If
//        //        ShellObj = Nothing
//        //        Exit Sub
//        //        '
//        //ErrorTrap:
//        //        Call AppendLogFile("ErrorTrap, runProcess running command [" & Cmd & "], WaitForReturn=" & WaitForReturn & ", err=" & GetErrString(Err))
//        //    End Sub
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //   Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
//        //       call this before parsing them together
//        //       call decodeAddonConstructorArgument after parsing them apart
//        //
//        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
//        //
//        //       This routine is needed for all Arg, Name, Value, Option values
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //
//        public static string EncodeAddonConstructorArgument(string Arg)
//        {
//            string functionReturnValue = null;
//            string a = null;
//            if (!string.IsNullOrEmpty(Arg))
//            {
//                a = Arg;
//                if (true)
//                {
//                    //If AddonNewParse Then
//                    a = vbReplace(a, "\\", "\\\\");
//                    a = vbReplace(a, Constants.vbCrLf, "\\n");
//                    a = vbReplace(a, Constants.vbTab, "\\t");
//                    a = vbReplace(a, "&", "\\&");
//                    a = vbReplace(a, "=", "\\=");
//                    a = vbReplace(a, ",", "\\,");
//                    a = vbReplace(a, "\"", "\\\"");
//                    a = vbReplace(a, "'", "\\'");
//                    a = vbReplace(a, "|", "\\|");
//                    a = vbReplace(a, "[", "\\[");
//                    a = vbReplace(a, "]", "\\]");
//                    a = vbReplace(a, ":", "\\:");
//                }
//                functionReturnValue = a;
//            }
//            return functionReturnValue;
//        }
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //   Decodes an argument parsed from an AddonConstructorString for all non-allowed characters
//        //       AddonConstructorString is a & delimited string of name=value[selector]descriptor
//        //
//        //       to get a value from an AddonConstructorString, first use getargument() to get the correct value[selector]descriptor
//        //       then remove everything to the right of any '['
//        //
//        //       call encodeAddonConstructorargument before parsing them together
//        //       call decodeAddonConstructorArgument after parsing them apart
//        //
//        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
//        //
//        //       This routine is needed for all Arg, Name, Value, Option values
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //
//        public static string DecodeAddonConstructorArgument(string EncodedArg)
//        {
//            string a = null;
//            int Pos = 0;
//            //
//            a = EncodedArg;
//            if (true)
//            {
//                //If AddonNewParse Then
//                a = vbReplace(a, "\\:", ":");
//                a = vbReplace(a, "\\]", "]");
//                a = vbReplace(a, "\\[", "[");
//                a = vbReplace(a, "\\|", "|");
//                a = vbReplace(a, "\\'", "'");
//                a = vbReplace(a, "\\\"", "\"");
//                a = vbReplace(a, "\\,", ",");
//                a = vbReplace(a, "\\=", "=");
//                a = vbReplace(a, "\\&", "&");
//                a = vbReplace(a, "\\t", Constants.vbTab);
//                a = vbReplace(a, "\\n", Constants.vbCrLf);
//                a = vbReplace(a, "\\\\", "\\");
//            }
//            return a;
//        }
//        //    '
//        //    '------------------------------------------------------------------------------------------------------------
//        //    '   use only internally
//        //    '
//        //    '   encode an argument to be used in a name=value& (N-V-A) string
//        //    '
//        //    '   an argument can be any one of these is this format:
//        //    '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
//        //    '
//        //    '   to create an nva string
//        //    '       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
//        //    '
//        //    '   to decode an nva string
//        //    '       split on ampersand then on equal, and decodeNvaArgument() each part
//        //    '
//        //    '------------------------------------------------------------------------------------------------------------
//        //    '
//        //    Public Function encodeNvaArgument(Arg As String) As String
//        //        Dim a As String
//        //        a = Arg
//        //        If a <> "" Then
//        //            a = vbReplace(a, vbCrLf, "#0013#")
//        //            a = vbReplace(a, vbLf, "#0013#")
//        //            a = vbReplace(a, vbCr, "#0013#")
//        //            a = vbReplace(a, "&", "#0038#")
//        //            a = vbReplace(a, "=", "#0061#")
//        //            a = vbReplace(a, ",", "#0044#")
//        //            a = vbReplace(a, """", "#0034#")
//        //            a = vbReplace(a, "'", "#0039#")
//        //            a = vbReplace(a, "|", "#0124#")
//        //            a = vbReplace(a, "[", "#0091#")
//        //            a = vbReplace(a, "]", "#0093#")
//        //            a = vbReplace(a, ":", "#0058#")
//        //        End If
//        //        encodeNvaArgument = a
//        //    End Function
//        //    '
//        //    '------------------------------------------------------------------------------------------------------------
//        //    '   use only internally
//        //    '       decode an argument removed from a name=value& string
//        //    '       see encodeNvaArgument for details on how to use this
//        //    '------------------------------------------------------------------------------------------------------------
//        //    '
//        //    Public Function decodeNvaArgument(EncodedArg As String) As String
//        //        Dim a As String
//        //        '
//        //        a = EncodedArg
//        //        a = vbReplace(a, "#0058#", ":")
//        //        a = vbReplace(a, "#0093#", "]")
//        //        a = vbReplace(a, "#0091#", "[")
//        //        a = vbReplace(a, "#0124#", "|")
//        //        a = vbReplace(a, "#0039#", "'")
//        //        a = vbReplace(a, "#0034#", """")
//        //        a = vbReplace(a, "#0044#", ",")
//        //        a = vbReplace(a, "#0061#", "=")
//        //        a = vbReplace(a, "#0038#", "&")
//        //        a = vbReplace(a, "#0013#", vbCrLf)
//        //        decodeNvaArgument = a
//        //    End Function
//        //    '
//        //    ' returns true of the link is a valid link on the source host
//        //    '
//        public static bool IsLinkToThisHost(string Host, string Link)
//        {
//            bool functionReturnValue = false;
//            //
//            string LinkHost = null;
//            int Pos = 0;
//            //
//            if (string.IsNullOrEmpty(Strings.Trim(Link)))
//            {
//                //
//                // Blank is not a link
//                //
//                functionReturnValue = false;
//            }
//            else if (vbInstr(1, Link, "://") != 0)
//            {
//                //
//                // includes protocol, may be link to another site
//                //
//                LinkHost = vbLCase(Link);
//                Pos = 1;
//                Pos = vbInstr(Pos, LinkHost, "://");
//                if (Pos > 0)
//                {
//                    Pos = vbInstr(Pos + 3, LinkHost, "/");
//                    if (Pos > 0)
//                    {
//                        LinkHost = Strings.Mid(LinkHost, 1, Pos - 1);
//                    }
//                    functionReturnValue = (vbLCase(Host) == LinkHost);
//                    if (!functionReturnValue)
//                    {
//                        //
//                        // try combinations including/excluding www.
//                        //
//                        if (vbInstr(1, LinkHost, "www.", Constants.vbTextCompare) != 0)
//                        {
//                            //
//                            // remove it
//                            //
//                            LinkHost = vbReplace(LinkHost, "www.", "", 1, 99, Constants.vbTextCompare);
//                            functionReturnValue = (vbLCase(Host) == LinkHost);
//                        }
//                        else {
//                            //
//                            // add it
//                            //
//                            LinkHost = vbReplace(LinkHost, "://", "://www.", 1, 99, Constants.vbTextCompare);
//                            functionReturnValue = (vbLCase(Host) == LinkHost);
//                        }
//                    }
//                }
//            }
//            else if (vbInstr(1, Link, "#") == 1)
//            {
//                //
//                // Is a bookmark, not a link
//                //
//                functionReturnValue = false;
//            }
//            else {
//                //
//                // all others are links on the source
//                //
//                functionReturnValue = true;
//            }
//            if (!functionReturnValue)
//            {
//                Link = Link;
//            }
//            return functionReturnValue;
//        }
//        //
//        //========================================================================================================
//        //   ConvertLinkToRootRelative
//        //
//        //   /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
//        //   http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
//        //   images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
//        //   logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
//        //
//        //========================================================================================================
//        //
//        public static string ConvertLinkToRootRelative(string Link, string BasePath)
//        {
//            string functionReturnValue = null;
//            //
//            int Pos = 0;
//            //
//            functionReturnValue = Link;
//            if (vbInstr(1, Link, "/") == 1)
//            {
//                //
//                //   case /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
//                //
//            }
//            else if (vbInstr(1, Link, "://") != 0)
//            {
//                //
//                //   case http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
//                //
//                Pos = vbInstr(1, Link, "://");
//                if (Pos > 0)
//                {
//                    Pos = vbInstr(Pos + 3, Link, "/");
//                    if (Pos > 0)
//                    {
//                        functionReturnValue = Strings.Mid(Link, Pos);
//                    }
//                    else {
//                        //
//                        // This is just the domain name, RootRelative is the root
//                        //
//                        functionReturnValue = "/";
//                    }
//                }
//            }
//            else {
//                //
//                //   case images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
//                //   case logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
//                //
//                functionReturnValue = BasePath + Link;
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //
//        //
//        public static string GetAddonIconImg(string AdminURL, int IconWidth, int IconHeight, int IconSprites, bool IconIsInline, string IconImgID, string IconFilename, string serverFilePath, string IconAlt, string IconTitle,
//        string ACInstanceID, int IconSpriteColumn)
//        {
//            string functionReturnValue = null;
//            //
//            string ImgStyle = null;
//            int IconHeightNumeric = 0;
//            //
//            if (string.IsNullOrEmpty(IconAlt))
//            {
//                IconAlt = "Add-on";
//            }
//            if (string.IsNullOrEmpty(IconTitle))
//            {
//                IconTitle = "Rendered as Add-on";
//            }
//            if (string.IsNullOrEmpty(IconFilename))
//            {
//                //
//                // No icon given, use the default
//                //
//                if (IconIsInline)
//                {
//                    IconFilename = "/ccLib/images/IconAddonInlineDefault.png";
//                    IconWidth = 62;
//                    IconHeight = 17;
//                    IconSprites = 0;
//                }
//                else {
//                    IconFilename = "/ccLib/images/IconAddonBlockDefault.png";
//                    IconWidth = 57;
//                    IconHeight = 59;
//                    IconSprites = 4;
//                }
//            }
//            else if (vbInstr(1, IconFilename, "://") != 0)
//            {
//                //
//                // icon is an Absolute URL - leave it
//                //
//            }
//            else if (Strings.Left(IconFilename, 1) == "/")
//            {
//                //
//                // icon is Root Relative, leave it
//                //
//            }
//            else {
//                //
//                // icon is a virtual file, add the serverfilepath
//                //
//                IconFilename = serverFilePath + IconFilename;
//            }
//            //IconFilename = encodeJavascript(IconFilename)
//            if ((IconWidth == 0) | (IconHeight == 0))
//            {
//                IconSprites = 0;
//            }

//            if (IconSprites == 0)
//            {
//                //
//                // just the icon
//                //
//                functionReturnValue = "<img" + " border=0" + " id=\"" + IconImgID + "\"" + " onDblClick=\"window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');\"" + " alt=\"" + IconAlt + "\"" + " title=\"" + IconTitle + "\"" + " src=\"" + IconFilename + "\"";
//                //GetAddonIconImg = "<img" _
//                //    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
//                //    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
//                //    & " alt=""" & IconAlt & """" _
//                //    & " title=""" & IconTitle & """" _
//                //    & " src=""" & IconFilename & """"
//                if (IconWidth != 0)
//                {
//                    functionReturnValue = functionReturnValue + " width=\"" + IconWidth + "px\"";
//                }
//                if (IconHeight != 0)
//                {
//                    functionReturnValue = functionReturnValue + " height=\"" + IconHeight + "px\"";
//                }
//                if (IconIsInline)
//                {
//                    functionReturnValue = functionReturnValue + " style=\"vertical-align:middle;display:inline;\" ";
//                }
//                else {
//                    functionReturnValue = functionReturnValue + " style=\"display:block\" ";
//                }
//                if (!string.IsNullOrEmpty(ACInstanceID))
//                {
//                    functionReturnValue = functionReturnValue + " ACInstanceID=\"" + ACInstanceID + "\"";
//                }
//                functionReturnValue = functionReturnValue + ">";
//            }
//            else {
//                //
//                // Sprite Icon
//                //
//                functionReturnValue = GetIconSprite(IconImgID, IconSpriteColumn, IconFilename, IconWidth, IconHeight, IconAlt, IconTitle, "window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');", IconIsInline, ACInstanceID);
//                //        GetAddonIconImg = "<img" _
//                //            & " border=0" _
//                //            & " id=""" & IconImgID & """" _
//                //            & " onMouseOver=""this.style.backgroundPosition='" & (-1 * IconSpriteColumn * IconWidth) & "px -" & (2 * IconHeight) & "px'""" _
//                //            & " onMouseOut=""this.style.backgroundPosition='" & (-1 * IconSpriteColumn * IconWidth) & "px 0px'""" _
//                //            & " onDblClick=""window.parent.OpenAddonPropertyWindow(this,'" & AdminURL & "');""" _
//                //            & " alt=""" & IconAlt & """" _
//                //            & " title=""" & IconTitle & """" _
//                //            & " src=""/ccLib/images/spacer.gif"""
//                //        ImgStyle = "background:url(" & IconFilename & ") " & (-1 * IconSpriteColumn * IconWidth) & "px 0px no-repeat;"
//                //        ImgStyle = ImgStyle & "width:" & IconWidth & "px;"
//                //        ImgStyle = ImgStyle & "height:" & IconHeight & "px;"
//                //        If IconIsInline Then
//                //            'GetAddonIconImg = GetAddonIconImg & " align=""middle"""
//                //            ImgStyle = ImgStyle & "vertical-align:middle;display:inline;"
//                //        Else
//                //            ImgStyle = ImgStyle & "display:block;"
//                //        End If
//                //
//                //
//                //        'Return_IconStyleMenuEntries = Return_IconStyleMenuEntries & vbCrLf & ",["".icon" & AddonID & """,false,"".icon" & AddonID & """,""background:url(" & IconFilename & ") 0px 0px no-repeat;"
//                //        'GetAddonIconImg = "<img" _
//                //        '    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
//                //        '    & " onMouseOver=""this.style.backgroundPosition=\'0px -" & (2 * IconHeight) & "px\'""" _
//                //        '    & " onMouseOut=""this.style.backgroundPosition=\'0px 0px\'""" _
//                //        '    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
//                //        '    & " alt=""" & IconAlt & """" _
//                //        '    & " title=""" & IconTitle & """" _
//                //        '    & " src=""/ccLib/images/spacer.gif"""
//                //        If ACInstanceID <> "" Then
//                //            GetAddonIconImg = GetAddonIconImg & " ACInstanceID=""" & ACInstanceID & """"
//                //        End If
//                //        GetAddonIconImg = GetAddonIconImg & " style=""" & ImgStyle & """>"
//                //        'Return_IconStyleMenuEntries = Return_IconStyleMenuEntries & """]"
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string ConvertRSTypeToGoogleType(int RecordFieldType)
//        {
//            string functionReturnValue = null;
//            switch (RecordFieldType)
//            {
//                case 2:
//                case 3:
//                case 4:
//                case 5:
//                case 6:
//                case 14:
//                case 16:
//                case 17:
//                case 18:
//                case 19:
//                case 20:
//                case 21:
//                case 131:
//                    functionReturnValue = "number";
//                    break;
//                default:
//                    functionReturnValue = "string";
//                    break;
//            }
//            return functionReturnValue;
//        }
//        //    '
//        //    '
//        //    '
//        public static string GetIconSprite(string TagID, int SpriteColumn, string IconSrc, int IconWidth, int IconHeight, string IconAlt, string IconTitle, string onDblClick, bool IconIsInline, string ACInstanceID)
//        {
//            string functionReturnValue = null;
//            //
//            string ImgStyle = null;
//            //
//            functionReturnValue = "<img" + " border=0" + " id=\"" + TagID + "\"" + " onMouseOver=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px -" + (2 * IconHeight) + "px';\"" + " onMouseOut=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px 0px'\"" + " onDblClick=\"" + onDblClick + "\"" + " alt=\"" + IconAlt + "\"" + " title=\"" + IconTitle + "\"" + " src=\"/ccLib/images/spacer.gif\"";
//            ImgStyle = "background:url(" + IconSrc + ") " + (-1 * SpriteColumn * IconWidth) + "px 0px no-repeat;";
//            ImgStyle = ImgStyle + "width:" + IconWidth + "px;";
//            ImgStyle = ImgStyle + "height:" + IconHeight + "px;";
//            if (IconIsInline)
//            {
//                ImgStyle = ImgStyle + "vertical-align:middle;display:inline;";
//            }
//            else {
//                ImgStyle = ImgStyle + "display:block;";
//            }
//            if (!string.IsNullOrEmpty(ACInstanceID))
//            {
//                functionReturnValue = functionReturnValue + " ACInstanceID=\"" + ACInstanceID + "\"";
//            }
//            functionReturnValue = functionReturnValue + " style=\"" + ImgStyle + "\">";
//            return functionReturnValue;
//        }
//        //
//        //================================================================================================================
//        //   Separate a URL into its host, path, page parts
//        //================================================================================================================
//        //
//        public static void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString)
//        {
//            //
//            //   Divide the URL into URLHost, URLPath, and URLPage
//            //
//            string WorkingURL = null;
//            int Position = 0;
//            //
//            // Get Protocol (before the first :)
//            //
//            WorkingURL = SourceURL;
//            Position = vbInstr(1, WorkingURL, ":");
//            //Position = vbInstr(1, WorkingURL, "://")
//            if (Position != 0)
//            {
//                Protocol = Strings.Mid(WorkingURL, 1, Position + 2);
//                WorkingURL = Strings.Mid(WorkingURL, Position + 3);
//            }
//            //
//            // compatibility fix
//            //
//            if (vbInstr(1, WorkingURL, "//") == 1)
//            {
//                if (string.IsNullOrEmpty(Protocol))
//                {
//                    Protocol = "http:";
//                }
//                Protocol = Protocol + "//";
//                WorkingURL = Strings.Mid(WorkingURL, 3);
//            }
//            //
//            // Get QueryString
//            //
//            Position = vbInstr(1, WorkingURL, "?");
//            if (Position > 0)
//            {
//                QueryString = Strings.Mid(WorkingURL, Position);
//                WorkingURL = Strings.Mid(WorkingURL, 1, Position - 1);
//            }
//            //
//            // separate host from pathpage
//            //
//            //iURLHost = WorkingURL
//            Position = vbInstr(WorkingURL, "/");
//            if ((Position == 0) & (string.IsNullOrEmpty(Protocol)))
//            {
//                //
//                // Page without path or host
//                //
//                Page = WorkingURL;
//                Path = "";
//                Host = "";
//            }
//            else if ((Position == 0))
//            {
//                //
//                // host, without path or page
//                //
//                Page = "";
//                Path = "/";
//                Host = WorkingURL;
//            }
//            else {
//                //
//                // host with a path (at least)
//                //
//                Path = Strings.Mid(WorkingURL, Position);
//                Host = Strings.Mid(WorkingURL, 1, Position - 1);
//                //
//                // separate page from path
//                //
//                Position = Strings.InStrRev(Path, "/");
//                if (Position == 0)
//                {
//                    //
//                    // no path, just a page
//                    //
//                    Page = Path;
//                    Path = "/";
//                }
//                else {
//                    Page = Strings.Mid(Path, Position + 1);
//                    Path = Strings.Mid(Path, 1, Position);
//                }
//            }
//        }
//        //
//        //================================================================================================================
//        //   Separate a URL into its host, path, page parts
//        //================================================================================================================
//        //
//        public static void ParseURL(string SourceURL, ref string Protocol, ref string Host, ref string Port, ref string Path, ref string Page, ref string QueryString)
//        {
//            //
//            //   Divide the URL into URLHost, URLPath, and URLPage
//            //
//            string iURLWorking = "";
//            string iURLProtocol = "";
//            string iURLHost = "";
//            string iURLPort = "";
//            string iURLPath = "";
//            string iURLPage = "";
//            string iURLQueryString = "";
//            int Position = 0;
//            //
//            iURLWorking = SourceURL;
//            Position = vbInstr(1, iURLWorking, "://");
//            if (Position != 0)
//            {
//                iURLProtocol = Strings.Mid(iURLWorking, 1, Position + 2);
//                iURLWorking = Strings.Mid(iURLWorking, Position + 3);
//            }
//            //
//            // separate Host:Port from pathpage
//            //
//            iURLHost = iURLWorking;
//            Position = vbInstr(iURLHost, "/");
//            if (Position == 0)
//            {
//                //
//                // just host, no path or page
//                //
//                iURLPath = "/";
//                iURLPage = "";
//            }
//            else {
//                iURLPath = Strings.Mid(iURLHost, Position);
//                iURLHost = Strings.Mid(iURLHost, 1, Position - 1);
//                //
//                // separate page from path
//                //
//                Position = Strings.InStrRev(iURLPath, "/");
//                if (Position == 0)
//                {
//                    //
//                    // no path, just a page
//                    //
//                    iURLPage = iURLPath;
//                    iURLPath = "/";
//                }
//                else {
//                    iURLPage = Strings.Mid(iURLPath, Position + 1);
//                    iURLPath = Strings.Mid(iURLPath, 1, Position);
//                }
//            }
//            //
//            // Divide Host from Port
//            //
//            Position = vbInstr(iURLHost, ":");
//            if (Position == 0)
//            {
//                //
//                // host not given, take a guess
//                //
//                switch (vbUCase(iURLProtocol))
//                {
//                    case "FTP://":
//                        iURLPort = "21";
//                        break;
//                    case "HTTP://":
//                    case "HTTPS://":
//                        iURLPort = "80";
//                        break;
//                    default:
//                        iURLPort = "80";
//                        break;
//                }
//            }
//            else {
//                iURLPort = Strings.Mid(iURLHost, Position + 1);
//                iURLHost = Strings.Mid(iURLHost, 1, Position - 1);
//            }
//            Position = vbInstr(1, iURLPage, "?");
//            if (Position > 0)
//            {
//                iURLQueryString = Strings.Mid(iURLPage, Position);
//                iURLPage = Strings.Mid(iURLPage, 1, Position - 1);
//            }
//            Protocol = iURLProtocol;
//            Host = iURLHost;
//            Port = iURLPort;
//            Path = iURLPath;
//            Page = iURLPage;
//            QueryString = iURLQueryString;
//        }
//        //
//        //
//        //
//        public static System.DateTime DecodeGMTDate(string GMTDate)
//        {
//            System.DateTime functionReturnValue = default(System.DateTime);
//            //
//            double YearPart = 0;
//            double HourPart = 0;
//            //
//            functionReturnValue = 1 / 1 / 0001 12:00:00 AM;
//            if (!string.IsNullOrEmpty(GMTDate))
//            {
//                HourPart = EncodeNumber(Strings.Mid(GMTDate, 6, 11));
//                if (Information.IsDate(HourPart))
//                {
//                    YearPart = EncodeNumber(Strings.Mid(GMTDate, 18, 8));
//                    if (Information.IsDate(YearPart))
//                    {
//                        functionReturnValue = System.DateTime.FromOADate(YearPart + (HourPart + 4) / 24);
//                    }
//                }
//            }
//            return functionReturnValue;
//        }
//        //'
//        //'
//        //'
//        //Public Function  EncodeGMTDate(ByVal MSDate As Date) As String
//        //    EncodeGMTDate = ""
//        //End Function
//        //'
//        //=================================================================================
//        // Get the value of a name in a string of name value pairs parsed with vrlf and =
//        //   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
//        //   new format is "name1=value1 crlf name2=value2 crlf ..."
//        //   There can be no extra spaces between the delimiter, the name and the "="
//        //=================================================================================
//        //
//        public static string GetArgument(string Name, string ArgumentString, string DefaultValue = "", string Delimiter = "")
//        {
//            string functionReturnValue = null;
//            //
//            string WorkingString = null;
//            string iDefaultValue = null;
//            int NameLength = 0;
//            int ValueStart = 0;
//            int ValueEnd = 0;
//            bool IsQuoted = false;
//            //
//            // determine delimiter
//            //
//            if (string.IsNullOrEmpty(Delimiter))
//            {
//                //
//                // If not explicit
//                //
//                if (vbInstr(1, ArgumentString, Constants.vbCrLf) != 0)
//                {
//                    //
//                    // crlf can only be here if it is the delimiter
//                    //
//                    Delimiter = Constants.vbCrLf;
//                }
//                else {
//                    //
//                    // either only one option, or it is the legacy '&' delimit
//                    //
//                    Delimiter = "&";
//                }
//            }
//            iDefaultValue = DefaultValue;
//            WorkingString = ArgumentString;
//            functionReturnValue = iDefaultValue;
//            if (!string.IsNullOrEmpty(WorkingString))
//            {
//                WorkingString = Delimiter + WorkingString + Delimiter;
//                ValueStart = vbInstr(1, WorkingString, Delimiter + Name + "=", Constants.vbTextCompare);
//                if (ValueStart != 0)
//                {
//                    NameLength = Strings.Len(Name);
//                    ValueStart = ValueStart + Strings.Len(Delimiter) + NameLength + 1;
//                    if (Strings.Mid(WorkingString, ValueStart, 1) == "\"")
//                    {
//                        IsQuoted = true;
//                        ValueStart = ValueStart + 1;
//                    }
//                    if (IsQuoted)
//                    {
//                        ValueEnd = vbInstr(ValueStart, WorkingString, "\"" + Delimiter);
//                    }
//                    else {
//                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter);
//                    }
//                    if (ValueEnd == 0)
//                    {
//                        functionReturnValue = Strings.Mid(WorkingString, ValueStart);
//                    }
//                    else {
//                        functionReturnValue = Strings.Mid(WorkingString, ValueStart, ValueEnd - ValueStart);
//                    }
//                }
//            }
//            return functionReturnValue;
//            ErrorTrap:
//            return functionReturnValue;
//            //

//            //
//            // ----- ErrorTrap
//            //
//        }
//        //'
//        //'=================================================================================
//        //'   Return the value from a name value pair, parsed with =,&[|].
//        //'   For example:
//        //'       name=Jay[Jay|Josh|Dwayne]
//        //'       the answer is Jay. If a select box is displayed, it is a dropdown of all three
//        //'=================================================================================
//        //'
//        //Public Function  GetAggrOption(ByVal Name As String, ByVal SegmentCMDArgs As String) As String
//        //    '
//        //    Dim Pos As Integer
//        //    '
//        //    GetAggrOption = GetArgument(Name, SegmentCMDArgs)
//        //    '
//        //    ' remove the manual select list syntax "answer[choice1|choice2]"
//        //    '
//        //    Pos = vbInstr(1, GetAggrOption, "[")
//        //    If Pos <> 0 Then
//        //        GetAggrOption = Left(GetAggrOption, Pos - 1)
//        //    End If
//        //    '
//        //    ' remove any function syntax "answer{selectcontentname RSS Feeds}"
//        //    '
//        //    Pos = vbInstr(1, GetAggrOption, "{")
//        //    If Pos <> 0 Then
//        //        GetAggrOption = Left(GetAggrOption, Pos - 1)
//        //    End If
//        //    '
//        //End Function
//        //'
//        //'=================================================================================
//        //'   Compatibility for GetArgument
//        //'=================================================================================
//        //'
//        //Public Function  GetNameValue(ByVal Name As String, ByVal ArgumentString As String, Optional ByVal DefaultValue As String = "") As String
//        //    getNameValue = GetArgument(Name, ArgumentString, DefaultValue)
//        //End Function
//        //'
//        //'========================================================================
//        //'   Gets the next line from a string, and removes the line
//        //'========================================================================
//        //'
//        //Public Function GetLine(ByVal Body As String) As String
//        //    Dim EOL As String
//        //    Dim NextCR As Integer
//        //    Dim NextLF As Integer
//        //    Dim BOL As Integer
//        //    '
//        //    NextCR = vbInstr(1, Body, vbCr)
//        //    NextLF = vbInstr(1, Body, vbLf)

//        //    If NextCR <> 0 Or NextLF <> 0 Then
//        //        If NextCR <> 0 Then
//        //            If NextLF <> 0 Then
//        //                If NextCR < NextLF Then
//        //                    EOL = NextCR - 1
//        //                    If NextLF = NextCR + 1 Then
//        //                        BOL = NextLF + 1
//        //                    Else
//        //                        BOL = NextCR + 1
//        //                    End If

//        //                Else
//        //                    EOL = NextLF - 1
//        //                    BOL = NextLF + 1
//        //                End If
//        //            Else
//        //                EOL = NextCR - 1
//        //                BOL = NextCR + 1
//        //            End If
//        //        Else
//        //            EOL = NextLF - 1
//        //            BOL = NextLF + 1
//        //        End If
//        //        GetLine = Mid(Body, 1, EOL)
//        //        Body = Mid(Body, BOL)
//        //    Else
//        //        GetLine = Body
//        //        Body = ""
//        //    End If
//        //End Function
//        //
//        //=================================================================================
//        //   Get a Random Long Value
//        //=================================================================================
//        //
//        public static int GetRandomInteger()
//        {
//            //
//            int RandomBase = 0;
//            int RandomLimit = 0;
//            //
//            RandomBase = Convert.ToInt32((Math.Pow(2, 30)) - 1);
//            RandomLimit = Convert.ToInt32((Math.Pow(2, 31)) - RandomBase - 1);
//            VBMath.Randomize();
//            return Convert.ToInt32(RandomBase + (VBMath.Rnd() * RandomLimit));
//            //
//        }
//        //
//        //=================================================================================
//        // fix for isDataTableOk
//        //=================================================================================
//        //
//        public static bool isDataTableOk(DataTable dt)
//        {
//            return (dt.Rows.Count > 0);
//        }
//        //
//        //=================================================================================
//        // fix for closeRS
//        //=================================================================================
//        //
//        public static void closeDataTable(DataTable dt)
//        {
//            // nothing needed
//            //dt.Clear()
//            dt.Dispose();
//        }
//        //'
//        //'=============================================================================
//        //' Create the part of the sql where clause that is modified by the user
//        //'   WorkingQuery is the original querystring to change
//        //'   QueryName is the name part of the name pair to change
//        //'   If the QueryName is not found in the string, ignore call
//        //'=============================================================================
//        //'
//        //Public Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
//        //    '
//        //    If vbInstr(1, WorkingQuery, "?") Then
//        //        ModifyQueryString = ModifyLinkQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
//        //    Else
//        //        ModifyQueryString = Mid(ModifyLinkQueryString("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
//        //    End If
//        //End Function
//        //
//        //=============================================================================
//        //   Modify a querystring name/value pair in a Link
//        //=============================================================================
//        //
//        public static string ModifyLinkQueryString(string Link, string QueryName, string QueryValue, bool AddIfMissing = true)
//        {
//            string functionReturnValue = null;
//            //
//            string[] Element = Strings.Split("", ",");
//            int ElementCount = 0;
//            int ElementPointer = 0;
//            string[] NameValue = null;
//            string UcaseQueryName = null;
//            bool ElementFound = false;
//            string QueryString = null;
//            //
//            if (vbInstr(1, Link, "?") != 0)
//            {
//                functionReturnValue = Strings.Mid(Link, 1, vbInstr(1, Link, "?") - 1);
//                QueryString = Strings.Mid(Link, Strings.Len(ModifyLinkQueryString()) + 2);
//            }
//            else {
//                functionReturnValue = Link;
//                QueryString = "";
//            }
//            UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName));
//            if (!string.IsNullOrEmpty(QueryString))
//            {
//                Element = Strings.Split(QueryString, "&");
//                ElementCount = Information.UBound(Element) + 1;
//                for (ElementPointer = 0; ElementPointer <= ElementCount - 1; ElementPointer++)
//                {
//                    NameValue = Strings.Split(Element(ElementPointer), "=");
//                    if (Information.UBound(NameValue) == 1)
//                    {
//                        if (vbUCase(NameValue(0)) == UcaseQueryName)
//                        {
//                            if (string.IsNullOrEmpty(QueryValue))
//                            {
//                                Element(ElementPointer) = "";
//                            }
//                            else {
//                                Element(ElementPointer) = QueryName + "=" + QueryValue;
//                            }
//                            ElementFound = true;
//                            break; // TODO: might not be correct. Was : Exit For
//                        }
//                    }
//                }
//            }
//            if (!ElementFound & (!string.IsNullOrEmpty(QueryValue)))
//            {
//                //
//                // element not found, it needs to be added
//                //
//                if (AddIfMissing)
//                {
//                    if (string.IsNullOrEmpty(QueryString))
//                    {
//                        QueryString = EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
//                    }
//                    else {
//                        QueryString = QueryString + "&" + EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
//                    }
//                }
//            }
//            else {
//                //
//                // element found
//                //
//                QueryString = Strings.Join(Element, "&");
//                if ((!string.IsNullOrEmpty(QueryString)) & (string.IsNullOrEmpty(QueryValue)))
//                {
//                    //
//                    // element found and needs to be removed
//                    //
//                    QueryString = vbReplace(QueryString, "&&", "&");
//                    if (Strings.Left(QueryString, 1) == "&")
//                    {
//                        QueryString = Strings.Mid(QueryString, 2);
//                    }
//                    if (Strings.Right(QueryString, 1) == "&")
//                    {
//                        QueryString = Strings.Mid(QueryString, 1, Strings.Len(QueryString) - 1);
//                    }
//                }
//            }
//            if ((!string.IsNullOrEmpty(QueryString)))
//            {
//                functionReturnValue = functionReturnValue + "?" + QueryString;
//            }
//            return functionReturnValue;
//        }
//        //
//        //=================================================================================
//        //
//        //=================================================================================
//        //
//        public static string GetIntegerString(int Value, int DigitCount)
//        {
//            string functionReturnValue = null;
//            if (Strings.Len(Value) <= DigitCount)
//            {
//                functionReturnValue = new string('0', DigitCount - Strings.Len(Convert.ToString(Value))) + Convert.ToString(Value);
//            }
//            else {
//                functionReturnValue = Convert.ToString(Value);
//            }
//            return functionReturnValue;
//        }
//        //'
//        //'==========================================================================================
//        //'   the current process to a high priority
//        //'       Should be called once from the objects parent when it is first created.
//        //'
//        //'   taken from an example labeled
//        //'       KPD-Team 2000
//        //'       URL: http://www.allapi.net/
//        //'       Email: KPDTeam@Allapi.net
//        //'==========================================================================================
//        //'
//        //Public sub SetProcessHighPriority()
//        //    Dim hProcess As Integer
//        //    '
//        //    'set the new priority class
//        //    '
//        //    hProcess = GetCurrentProcess
//        //    Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
//        //    '
//        //End Sub
//        //'
//        //==========================================================================================
//        //   Format the current error object into a standard string
//        //==========================================================================================
//        //
//        public static string GetErrString(ErrObject ErrorObject = null)
//        {
//            string functionReturnValue = null;
//            string Copy = null;
//            if (ErrorObject == null)
//            {
//                if (Err.Number == 0)
//                {
//                    functionReturnValue = "[no error]";
//                }
//                else {
//                    Copy = Err.Description;
//                    Copy = vbReplace(Copy, Constants.vbCrLf, "-");
//                    Copy = vbReplace(Copy, Constants.vbLf, "-");
//                    Copy = vbReplace(Copy, Constants.vbCrLf, "");
//                    functionReturnValue = "[" + Err.Source + " #" + Err.Number + ", " + Copy + "]";
//                }
//            }
//            else {
//                if (ErrorObject.Number == 0)
//                {
//                    functionReturnValue = "[no error]";
//                }
//                else {
//                    Copy = ErrorObject.Description;
//                    Copy = vbReplace(Copy, Constants.vbCrLf, "-");
//                    Copy = vbReplace(Copy, Constants.vbLf, "-");
//                    Copy = vbReplace(Copy, Constants.vbCrLf, "");
//                    functionReturnValue = "[" + ErrorObject.Source + " #" + ErrorObject.Number + ", " + Copy + "]";
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //==========================================================================================
//        //   Format the current error object into a standard string
//        //==========================================================================================
//        //
//        public static int GetProcessID()
//        {
//            Process Instance = Process.GetCurrentProcess();
//            //
//            return Instance.Id;
//        }
//        //
//        //==========================================================================================
//        //   Test if a test string is in a delimited string
//        //==========================================================================================
//        //
//        public static bool IsInDelimitedString(string DelimitedString, string TestString, string Delimiter)
//        {
//            return (0 != vbInstr(1, Delimiter + DelimitedString + Delimiter, Delimiter + TestString + Delimiter, Constants.vbTextCompare));
//        }
//        //
//        //========================================================================
//        // EncodeURL
//        //
//        //   Encodes only what is to the left of the first ?
//        //   All URL path characters are assumed to be correct (/:#)
//        //========================================================================
//        //
//        public static string EncodeURL(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            string[] URLSplit = null;
//            //Dim LeftSide As String
//            //Dim RightSide As String
//            //
//            functionReturnValue = Source;
//            if (!string.IsNullOrEmpty(Source))
//            {
//                URLSplit = Strings.Split(Source, "?");
//                functionReturnValue = URLSplit(0);
//                functionReturnValue = vbReplace(EncodeURL(), "%", "%25");
//                //
//                functionReturnValue = vbReplace(EncodeURL(), "\"", "%22");
//                functionReturnValue = vbReplace(EncodeURL(), " ", "%20");
//                functionReturnValue = vbReplace(EncodeURL(), "$", "%24");
//                functionReturnValue = vbReplace(EncodeURL(), "+", "%2B");
//                functionReturnValue = vbReplace(EncodeURL(), ",", "%2C");
//                functionReturnValue = vbReplace(EncodeURL(), ";", "%3B");
//                functionReturnValue = vbReplace(EncodeURL(), "<", "%3C");
//                functionReturnValue = vbReplace(EncodeURL(), "=", "%3D");
//                functionReturnValue = vbReplace(EncodeURL(), ">", "%3E");
//                functionReturnValue = vbReplace(EncodeURL(), "@", "%40");
//                if (Information.UBound(URLSplit) > 0)
//                {
//                    functionReturnValue = functionReturnValue + "?" + EncodeQueryString(URLSplit(1));
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // EncodeQueryString
//        //
//        //   This routine encodes the URL QueryString to conform to rules
//        //========================================================================
//        //
//        public static string EncodeQueryString(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            string[] QSSplit = null;
//            int QSPointer = 0;
//            string[] NVSplit = null;
//            string NV = null;
//            //
//            functionReturnValue = "";
//            if (!string.IsNullOrEmpty(Source))
//            {
//                QSSplit = Strings.Split(Source, "&");
//                for (QSPointer = 0; QSPointer <= Information.UBound(QSSplit); QSPointer++)
//                {
//                    NV = QSSplit(QSPointer);
//                    if (!string.IsNullOrEmpty(NV))
//                    {
//                        NVSplit = Strings.Split(NV, "=");
//                        if (Information.UBound(NVSplit) == 0)
//                        {
//                            NVSplit(0) = EncodeRequestVariable(NVSplit(0));
//                            functionReturnValue = functionReturnValue + "&" + NVSplit(0);
//                        }
//                        else {
//                            NVSplit(0) = EncodeRequestVariable(NVSplit(0));
//                            NVSplit(1) = EncodeRequestVariable(NVSplit(1));
//                            functionReturnValue = functionReturnValue + "&" + NVSplit(0) + "=" + NVSplit(1);
//                        }
//                    }
//                }
//                if (!string.IsNullOrEmpty(functionReturnValue))
//                {
//                    functionReturnValue = Strings.Mid(EncodeQueryString(), 2);
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // EncodeRequestVariable
//        //
//        //   This routine encodes a request variable for a URL Query String
//        //       ...can be the requestname or the requestvalue
//        //========================================================================
//        //
//        public static string EncodeRequestVariable(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            int SourcePointer = 0;
//            string Character = null;
//            string LocalSource = null;
//            //
//            functionReturnValue = "";
//            if (!string.IsNullOrEmpty(Source))
//            {
//                LocalSource = Source;
//                // "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
//                //LocalSource = vbReplace(LocalSource, " ", "+")
//                for (SourcePointer = 1; SourcePointer <= Strings.Len(LocalSource); SourcePointer++)
//                {
//                    Character = Strings.Mid(LocalSource, SourcePointer, 1);
//                    // "%" added so if this is called twice, it will not destroy "%20" values
//                    if (false)
//                    {
//                        //End If
//                        //If Character = " " Then
//                        functionReturnValue += "+";
//                    }
//                    else if (vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, Constants.vbTextCompare) != 0)
//                    {
//                        //ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
//                        functionReturnValue += Character;
//                    }
//                    else {
//                        functionReturnValue += "%" + Conversion.Hex(Strings.Asc(Character));
//                    }
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // EncodeHTML
//        //
//        //   Convert all characters that are not allowed in HTML to their Text equivalent
//        //   in preperation for use on an HTML page
//        //========================================================================
//        //
//        public static string cpcore.html.html_EncodeHTML(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            functionReturnValue = Source;
//            functionReturnValue = vbReplace(cpcore.html.html_EncodeHTML(), "&", "&amp;");
//            functionReturnValue = vbReplace(cpcore.html.html_EncodeHTML(), "<", "&lt;");
//            functionReturnValue = vbReplace(cpcore.html.html_EncodeHTML(), ">", "&gt;");
//            functionReturnValue = vbReplace(cpcore.html.html_EncodeHTML(), "\"", "&quot;");
//            functionReturnValue = vbReplace(cpcore.html.html_EncodeHTML(), "'", "&apos;");
//            return functionReturnValue;
//            //
//        }
//        //'
//        //'========================================================================
//        //' DecodeHTML
//        //'
//        //'   Convert HTML equivalent characters to their equivalents
//        //'========================================================================
//        //'
//        //Public Function DecodeHTML(ByVal Source As String) As String
//        //    ' ##### removed to catch err<>0 problem on error resume next
//        //    '
//        //    Dim Pos As Integer
//        //    Dim s As String
//        //    Dim CharCodeString As String
//        //    Dim CharCode As Integer
//        //    Dim PosEnd As Integer
//        //    '
//        //    ' 11/26/2009 - basically re-wrote it, I commented the old one out below
//        //    '
//        //    s = Source
//        //    '
//        //    ' numeric entities
//        //    '
//        //    Pos = Len(s)
//        //    Pos = InStrRev(s, "&#", Pos)
//        //    Do While Pos <> 0
//        //        CharCodeString = ""
//        //        If Mid(s, Pos + 3, 1) = ";" Then
//        //            CharCodeString = Mid(s, Pos + 2, 1)
//        //            PosEnd = Pos + 4
//        //        ElseIf Mid(s, Pos + 4, 1) = ";" Then
//        //            CharCodeString = Mid(s, Pos + 2, 2)
//        //            PosEnd = Pos + 5
//        //        ElseIf Mid(s, Pos + 5, 1) = ";" Then
//        //            CharCodeString = Mid(s, Pos + 2, 3)
//        //            PosEnd = Pos + 6
//        //        End If
//        //        If CharCodeString <> "" Then
//        //            If vbIsNumeric(CharCodeString) Then
//        //                CharCode = CLng(CharCodeString)
//        //                s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, PosEnd)
//        //            End If
//        //        End If
//        //        '
//        //        Pos = InStrRev(s, "&#", Pos)
//        //    Loop
//        //    '
//        //    ' character entities (at least the most common )
//        //    '
//        //    s = vbReplace(s, "&lt;", "<")
//        //    s = vbReplace(s, "&gt;", ">")
//        //    s = vbReplace(s, "&quot;", """")
//        //    s = vbReplace(s, "&apos;", "'")
//        //    '
//        //    ' always last
//        //    '
//        //    s = vbReplace(s, "&amp;", "&")
//        //    '
//        //    DecodeHTML = s
//        //    '
//        //End Function
//        //
//        //========================================================================
//        // AddSpanClass
//        //
//        //   Adds a span around the copy with the class name provided
//        //========================================================================
//        //
//        public static string AddSpan(string Copy, string ClassName)
//        {
//            //
//            return "<SPAN Class=\"" + ClassName + "\">" + Copy + "</SPAN>";
//            //
//        }
//        //
//        //========================================================================
//        // DecodeResponseVariable
//        //
//        //   Converts a querystring name or value back into the characters it represents
//        //   This is the same code as the decodeurl
//        //========================================================================
//        //
//        public static string DecodeResponseVariable(string Source)
//        {
//            string functionReturnValue = null;
//            //
//            int Position = 0;
//            string ESCString = null;
//            int ESCValue = 0;
//            string Digit0 = null;
//            string Digit1 = null;
//            //Dim iURL As String
//            //
//            //iURL = Source
//            // plus to space only applies for query component of a URL, but %99 encoding works for both
//            //DecodeResponseVariable = vbReplace(iURL, "+", " ")
//            functionReturnValue = Source;
//            Position = vbInstr(1, DecodeResponseVariable(), "%");
//            while (Position != 0)
//            {
//                ESCString = Strings.Mid(DecodeResponseVariable(), Position, 3);
//                Digit0 = vbUCase(Strings.Mid(ESCString, 2, 1));
//                Digit1 = vbUCase(Strings.Mid(ESCString, 3, 1));
//                if (((Digit0 >= "0") & (Digit0 <= "9")) | ((Digit0 >= "A") & (Digit0 <= "F")))
//                {
//                    if (((Digit1 >= "0") & (Digit1 <= "9")) | ((Digit1 >= "A") & (Digit1 <= "F")))
//                    {
//                        ESCValue = Convert.ToInt32("&H" + Strings.Mid(ESCString, 2));
//                        functionReturnValue = Strings.Mid(DecodeResponseVariable(), 1, Position - 1) + Strings.Chr(ESCValue) + Strings.Mid(DecodeResponseVariable(), Position + 3);
//                        //  & vbReplace(DecodeResponseVariable, ESCString, Chr(ESCValue), Position, 1)
//                    }
//                }
//                Position = vbInstr(Position + 1, DecodeResponseVariable(), "%");
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // DecodeURL
//        //   Converts a querystring from an Encoded URL (with %20 and +), to non incoded (with spaced)
//        //========================================================================
//        //
//        public static string DecodeURL(string Source)
//        {
//            string functionReturnValue = null;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            int Position = 0;
//            string ESCString = null;
//            int ESCValue = 0;
//            string Digit0 = null;
//            string Digit1 = null;
//            //Dim iURL As String
//            //
//            //iURL = Source
//            // plus to space only applies for query component of a URL, but %99 encoding works for both
//            //DecodeURL = vbReplace(iURL, "+", " ")
//            functionReturnValue = Source;
//            Position = vbInstr(1, DecodeURL(), "%");
//            while (Position != 0)
//            {
//                ESCString = Strings.Mid(DecodeURL(), Position, 3);
//                Digit0 = vbUCase(Strings.Mid(ESCString, 2, 1));
//                Digit1 = vbUCase(Strings.Mid(ESCString, 3, 1));
//                if (((Digit0 >= "0") & (Digit0 <= "9")) | ((Digit0 >= "A") & (Digit0 <= "F")))
//                {
//                    if (((Digit1 >= "0") & (Digit1 <= "9")) | ((Digit1 >= "A") & (Digit1 <= "F")))
//                    {
//                        ESCValue = Convert.ToInt32("&H" + Strings.Mid(ESCString, 2));
//                        functionReturnValue = vbReplace(DecodeURL(), ESCString, Strings.Chr(ESCValue));
//                    }
//                }
//                Position = vbInstr(Position + 1, DecodeURL(), "%");
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // GetFirstNonZeroDate
//        //
//        //   Converts a querystring name or value back into the characters it represents
//        //========================================================================
//        //
//        public static System.DateTime GetFirstNonZeroDate(System.DateTime Date0, System.DateTime Date1)
//        {
//            System.DateTime functionReturnValue = default(System.DateTime);
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            System.DateTime NullDate = default(System.DateTime);
//            //
//            NullDate = System.DateTime.MinValue;
//            if (Date0 == NullDate)
//            {
//                if (Date1 == NullDate)
//                {
//                    //
//                    // Both 0, return 0
//                    //
//                    functionReturnValue = NullDate;
//                }
//                else {
//                    //
//                    // Date0 is NullDate, return Date1
//                    //
//                    functionReturnValue = Date1;
//                }
//            }
//            else {
//                if (Date1 == NullDate)
//                {
//                    //
//                    // Date1 is nulldate, return Date0
//                    //
//                    functionReturnValue = Date0;
//                }
//                else if (Date0 < Date1)
//                {
//                    //
//                    // Date0 is first
//                    //
//                    functionReturnValue = Date0;
//                }
//                else {
//                    //
//                    // Date1 is first
//                    //
//                    functionReturnValue = Date1;
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // getFirstposition
//        //
//        //   returns 0 if both are zero
//        //   returns 1 if the first integer is non-zero and less then the second
//        //   returns 2 if the second integer is non-zero and less then the first
//        //========================================================================
//        //
//        public static int GetFirstNonZeroInteger(int Integer1, int Integer2)
//        {
//            int functionReturnValue = 0;
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            if (Integer1 == 0)
//            {
//                if (Integer2 == 0)
//                {
//                    //
//                    // Both 0, return 0
//                    //
//                    functionReturnValue = 0;
//                }
//                else {
//                    //
//                    // Integer1 is 0, return Integer2
//                    //
//                    functionReturnValue = 2;
//                }
//            }
//            else {
//                if (Integer2 == 0)
//                {
//                    //
//                    // Integer2 is 0, return Integer1
//                    //
//                    functionReturnValue = 1;
//                }
//                else if (Integer1 < Integer2)
//                {
//                    //
//                    // Integer1 is first
//                    //
//                    functionReturnValue = 1;
//                }
//                else {
//                    //
//                    // Integer2 is first
//                    //
//                    functionReturnValue = 2;
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================
//        // splitDelimited
//        //   returns the result of a Split, except it honors quoted text
//        //   if a quote is found, it is assumed to also be a delimiter ( 'this"that"theother' = 'this "that" theother' )
//        //========================================================================
//        //
//        public static string[] SplitDelimited(string WordList, string Delimiter)
//        {
//            // ##### removed to catch err<>0 problem on error resume next
//            //
//            string[] QuoteSplit = null;
//            int QuoteSplitCount = 0;
//            int QuoteSplitPointer = 0;
//            bool InQuote = false;
//            string[] Out = null;
//            int OutPointer = 0;
//            int OutSize = 0;
//            string[] SpaceSplit = null;
//            int SpaceSplitCount = 0;
//            int SpaceSplitPointer = 0;
//            string Fragment = null;
//            //
//            OutPointer = 0;
//            Out = new string[1];
//            OutSize = 1;
//            if (!string.IsNullOrEmpty(WordList))
//            {
//                QuoteSplit = Strings.Split(WordList, "\"");
//                QuoteSplitCount = Information.UBound(QuoteSplit) + 1;
//                InQuote = (string.IsNullOrEmpty(Strings.Mid(WordList, 1, 1)));
//                for (QuoteSplitPointer = 0; QuoteSplitPointer <= QuoteSplitCount - 1; QuoteSplitPointer++)
//                {
//                    Fragment = QuoteSplit(QuoteSplitPointer);
//                    if (string.IsNullOrEmpty(Fragment))
//                    {
//                        //
//                        // empty fragment
//                        // this is a quote at the end, or two quotes together
//                        // do not skip to the next out pointer
//                        //
//                        if (OutPointer >= OutSize)
//                        {
//                            OutSize = OutSize + 10;
//                            Array.Resize(ref Out, OutSize + 1);
//                        }
//                        //OutPointer = OutPointer + 1
//                    }
//                    else {
//                        if (!InQuote)
//                        {
//                            SpaceSplit = Strings.Split(Fragment, Delimiter);
//                            SpaceSplitCount = Information.UBound(SpaceSplit) + 1;
//                            for (SpaceSplitPointer = 0; SpaceSplitPointer <= SpaceSplitCount - 1; SpaceSplitPointer++)
//                            {
//                                if (OutPointer >= OutSize)
//                                {
//                                    OutSize = OutSize + 10;
//                                    Array.Resize(ref Out, OutSize + 1);
//                                }
//                                Out(OutPointer) = Out(OutPointer) + SpaceSplit(SpaceSplitPointer);
//                                if ((SpaceSplitPointer != (SpaceSplitCount - 1)))
//                                {
//                                    //
//                                    // divide output between splits
//                                    //
//                                    OutPointer = OutPointer + 1;
//                                    if (OutPointer >= OutSize)
//                                    {
//                                        OutSize = OutSize + 10;
//                                        Array.Resize(ref Out, OutSize + 1);
//                                    }
//                                }
//                            }
//                        }
//                        else {
//                            Out(OutPointer) = Out(OutPointer) + "\"" + Fragment + "\"";
//                        }
//                    }
//                    InQuote = !InQuote;
//                }
//            }
//            Array.Resize(ref Out, OutPointer + 1);
//            //
//            //
//            return Out;
//            //
//        }
//        //
//        //
//        //
//        public static string GetYesNo(bool Key)
//        {
//            string functionReturnValue = null;
//            if (Key)
//            {
//                functionReturnValue = "Yes";
//            }
//            else {
//                functionReturnValue = "No";
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string GetFilename(string PathFilename)
//        {
//            string functionReturnValue = null;
//            int Position = 0;
//            //
//            functionReturnValue = PathFilename;
//            Position = Strings.InStrRev(GetFilename(), "/");
//            if (Position != 0)
//            {
//                functionReturnValue = Strings.Mid(GetFilename(), Position + 1);
//            }
//            return functionReturnValue;
//        }
//        //        '
//        //        '
//        //        '
//        public static string StartTable(int Padding, int Spacing, int Border, string ClassStyle = "")
//        {
//            return "<table border=\"" + Border + "\" cellpadding=\"" + Padding + "\" cellspacing=\"" + Spacing + "\" class=\"" + ClassStyle + "\" width=\"100%\">";
//        }
//        //        '
//        //        '
//        //        '
//        public static string StartTableRow()
//        {
//            return "<tr>";
//        }
//        //        '
//        //        '
//        //        '
//        public static string StartTableCell(string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "")
//        {
//            string functionReturnValue = null;
//            if (!string.IsNullOrEmpty(Width))
//            {
//                functionReturnValue = " width=\"" + Width + "\"";
//            }
//            if (!string.IsNullOrEmpty(BGColor))
//            {
//                functionReturnValue = functionReturnValue + " bgcolor=\"" + BGColor + "\"";
//            }
//            else if (EvenRow)
//            {
//                functionReturnValue = functionReturnValue + " class=\"ccPanelRowEven\"";
//            }
//            else {
//                functionReturnValue = functionReturnValue + " class=\"ccPanelRowOdd\"";
//            }
//            if (ColSpan != 0)
//            {
//                functionReturnValue = functionReturnValue + " colspan=\"" + ColSpan + "\"";
//            }
//            if (!string.IsNullOrEmpty(Align))
//            {
//                functionReturnValue = functionReturnValue + " align=\"" + Align + "\"";
//            }
//            functionReturnValue = "<TD" + functionReturnValue + ">";
//            return functionReturnValue;
//        }
//        //        '
//        //        '
//        //        '
//        public static string GetTableCell(string Copy, string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "")
//        {
//            return StartTableCell(Width, ColSpan, EvenRow, Align, BGColor) + Copy + kmaEndTableCell;
//        }
//        //        '
//        //        '
//        //        '
//        public static string GetTableRow(string Cell, int ColSpan = 0, bool EvenRow = false)
//        {
//            return StartTableRow() + GetTableCell(Cell, "100%", ColSpan, EvenRow) + kmaEndTableRow;
//        }
//        //
//        // remove the host and approotpath, leaving the "active" path and all else
//        //
//        public static string ConvertShortLinkToLink(string URL, string PathPagePrefix)
//        {
//            string functionReturnValue = null;
//            functionReturnValue = URL;
//            if (!string.IsNullOrEmpty(URL) & !string.IsNullOrEmpty(PathPagePrefix))
//            {
//                if (vbInstr(1, ConvertShortLinkToLink(), PathPagePrefix, Constants.vbTextCompare) == 1)
//                {
//                    functionReturnValue = Strings.Mid(ConvertShortLinkToLink(), Strings.Len(PathPagePrefix) + 1);
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        // ------------------------------------------------------------------------------------------------------
//        //   Preserve URLs that do not start HTTP or HTTPS
//        //   Preserve URLs from other sites (offsite)
//        //   Preserve HTTP://ServerHost/ServerVirtualPath/Files/ in all cases
//        //   Convert HTTP://ServerHost/ServerVirtualPath/folder/page -> /folder/page
//        //   Convert HTTP://ServerHost/folder/page -> /folder/page
//        // ------------------------------------------------------------------------------------------------------
//        //
//        public static string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath)
//        {
//            string functionReturnValue = null;
//            //
//            string BadString = "";
//            string GoodString = "";
//            string Protocol = "";
//            string WorkingLink = "";
//            //
//            WorkingLink = URL;
//            //
//            // ----- Determine Protocol
//            //
//            if (vbInstr(1, WorkingLink, "HTTP://", Constants.vbTextCompare) == 1)
//            {
//                //
//                // HTTP
//                //
//                Protocol = Strings.Mid(WorkingLink, 1, 7);
//            }
//            else if (vbInstr(1, WorkingLink, "HTTPS://", Constants.vbTextCompare) == 1)
//            {
//                //
//                // HTTPS
//                //
//                // try this -- a ssl link can not be shortened
//                functionReturnValue = WorkingLink;
//                return functionReturnValue;
//                Protocol = Strings.Mid(WorkingLink, 1, 8);
//            }
//            if (!string.IsNullOrEmpty(Protocol))
//            {
//                //
//                // ----- Protcol found, determine if is local
//                //
//                GoodString = Protocol + ServerHost;
//                if ((Strings.InStr(1, WorkingLink, GoodString, Constants.vbTextCompare) != 0))
//                {
//                    //
//                    // URL starts with Protocol ServerHost
//                    //
//                    GoodString = Protocol + ServerHost + ServerVirtualPath + "/files/";
//                    if ((Strings.InStr(1, WorkingLink, GoodString, Constants.vbTextCompare) != 0))
//                    {
//                        //
//                        // URL is in the virtual files directory
//                        //
//                        BadString = GoodString;
//                        GoodString = ServerVirtualPath + "/files/";
//                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, Constants.vbTextCompare);
//                    }
//                    else {
//                        //
//                        // URL is not in files virtual directory
//                        //
//                        BadString = Protocol + ServerHost + ServerVirtualPath + "/";
//                        GoodString = "/";
//                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, Constants.vbTextCompare);
//                        //
//                        BadString = Protocol + ServerHost + "/";
//                        GoodString = "/";
//                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, Constants.vbTextCompare);
//                    }
//                }
//            }
//            functionReturnValue = WorkingLink;
//            return functionReturnValue;
//        }
//        //
//        // Correct the link for the virtual path, either add it or remove it
//        //
//        public static string EncodeAppRootPath(string Link, string VirtualPath, string AppRootPath, string ServerHost)
//        {
//            string functionReturnValue = null;
//            //
//            string Protocol = "";
//            string Host = "";
//            string Path = "";
//            string Page = "";
//            string QueryString = "";
//            bool VirtualHosted = false;
//            //
//            functionReturnValue = Link;
//            if ((Strings.InStr(1, EncodeAppRootPath(), ServerHost, Constants.vbTextCompare) != 0) | (Strings.InStr(1, Link, "/") == 1))
//            {
//                //If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) And (InStr(1, Link, "/") <> 0) Then
//                //
//                // This link is onsite and has a path
//                //
//                VirtualHosted = (Strings.InStr(1, AppRootPath, VirtualPath, Constants.vbTextCompare) != 0);
//                if (VirtualHosted & (Strings.InStr(1, Link, AppRootPath, Constants.vbTextCompare) == 1))
//                {
//                    //
//                    // quick - virtual hosted and link starts at AppRootPath
//                    //
//                }
//                else if ((!VirtualHosted) & (Strings.Mid(Link, 1, 1) == "/") & (Strings.InStr(1, Link, AppRootPath, Constants.vbTextCompare) == 1))
//                {
//                    //
//                    // quick - not virtual hosted and link starts at Root
//                    //
//                }
//                else {
//                    SeparateURL(Link, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
//                    if (VirtualHosted)
//                    {
//                        //
//                        // Virtual hosted site, add VirualPath if it is not there
//                        //
//                        if (vbInstr(1, Path, AppRootPath, Constants.vbTextCompare) == 0)
//                        {
//                            if (Path == "/")
//                            {
//                                Path = AppRootPath;
//                            }
//                            else {
//                                Path = AppRootPath + Strings.Mid(Path, 2);
//                            }
//                        }
//                    }
//                    else {
//                        //
//                        // Root hosted site, remove virtual path if it is there
//                        //
//                        if (vbInstr(1, Path, AppRootPath, Constants.vbTextCompare) != 0)
//                        {
//                            Path = vbReplace(Path, AppRootPath, "/");
//                        }
//                    }
//                    functionReturnValue = Protocol + Host + Path + Page + QueryString;
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        // Return just the tablename from a tablename reference (database.object.tablename->tablename)
//        //
//        public static string GetDbObjectTableName(string DbObject)
//        {
//            string functionReturnValue = null;
//            int Position = 0;
//            //
//            functionReturnValue = DbObject;
//            Position = Strings.InStrRev(GetDbObjectTableName(), ".");
//            if (Position > 0)
//            {
//                functionReturnValue = Strings.Mid(GetDbObjectTableName(), Position + 1);
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string GetLinkedText(string AnchorTag, string AnchorText)
//        {
//            string functionReturnValue = null;
//            //
//            string UcaseAnchorText = null;
//            int LinkPosition = 0;
//            string MethodName = null;
//            string iAnchorTag = null;
//            string iAnchorText = null;
//            //
//            MethodName = "GetLinkedText";
//            //
//            functionReturnValue = "";
//            iAnchorTag = AnchorTag;
//            iAnchorText = AnchorText;
//            UcaseAnchorText = vbUCase(iAnchorText);
//            if ((!string.IsNullOrEmpty(iAnchorTag)) & (!string.IsNullOrEmpty(iAnchorText)))
//            {
//                LinkPosition = Strings.InStrRev(UcaseAnchorText, "<LINK>", -1);
//                if (LinkPosition == 0)
//                {
//                    functionReturnValue = iAnchorTag + iAnchorText + "</A>";
//                }
//                else {
//                    functionReturnValue = iAnchorText;
//                    LinkPosition = Strings.InStrRev(UcaseAnchorText, "</LINK>", -1);
//                    while (LinkPosition > 1)
//                    {
//                        functionReturnValue = Strings.Mid(GetLinkedText(), 1, LinkPosition - 1) + "</A>" + Strings.Mid(GetLinkedText(), LinkPosition + 7);
//                        LinkPosition = Strings.InStrRev(UcaseAnchorText, "<LINK>", LinkPosition - 1);
//                        if (LinkPosition != 0)
//                        {
//                            functionReturnValue = Strings.Mid(GetLinkedText(), 1, LinkPosition - 1) + iAnchorTag + Strings.Mid(GetLinkedText(), LinkPosition + 6);
//                        }
//                        LinkPosition = Strings.InStrRev(UcaseAnchorText, "</LINK>", LinkPosition);
//                    }
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        public static string EncodeInitialCaps(string Source)
//        {
//            string functionReturnValue = null;
//            string[] SegSplit = null;
//            int SegPtr = 0;
//            int SegMax = 0;
//            //
//            functionReturnValue = "";
//            if (!string.IsNullOrEmpty(Source))
//            {
//                SegSplit = Strings.Split(Source, " ");
//                SegMax = Information.UBound(SegSplit);
//                if (SegMax >= 0)
//                {
//                    for (SegPtr = 0; SegPtr <= SegMax; SegPtr++)
//                    {
//                        SegSplit(SegPtr) = vbUCase(Strings.Left(SegSplit(SegPtr), 1)) + vbLCase(Strings.Mid(SegSplit(SegPtr), 2));
//                    }
//                }
//                functionReturnValue = Strings.Join(SegSplit, " ");
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string RemoveTag(string Source, string TagName)
//        {
//            string functionReturnValue = null;
//            int Pos = 0;
//            int PosEnd = 0;
//            functionReturnValue = Source;
//            Pos = vbInstr(1, Source, "<" + TagName, Constants.vbTextCompare);
//            if (Pos != 0)
//            {
//                PosEnd = vbInstr(Pos, Source, ">");
//                if (PosEnd > 0)
//                {
//                    functionReturnValue = Strings.Left(Source, Pos - 1) + Strings.Mid(Source, PosEnd + 1);
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string RemoveStyleTags(string Source)
//        {
//            string functionReturnValue = null;
//            functionReturnValue = Source;
//            while (vbInstr(1, RemoveStyleTags(), "<style", Constants.vbTextCompare) != 0)
//            {
//                functionReturnValue = RemoveTag(RemoveStyleTags(), "style");
//            }
//            while (vbInstr(1, RemoveStyleTags(), "</style", Constants.vbTextCompare) != 0)
//            {
//                functionReturnValue = RemoveTag(RemoveStyleTags(), "/style");
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string GetSingular(string PluralSource)
//        {
//            string functionReturnValue = null;
//            //
//            bool UpperCase = false;
//            string LastCharacter = null;
//            //
//            functionReturnValue = PluralSource;
//            if (Strings.Len(GetSingular()) > 1)
//            {
//                LastCharacter = Strings.Right(GetSingular(), 1);
//                if (LastCharacter != vbUCase(LastCharacter))
//                {
//                    UpperCase = true;
//                }
//                if (vbUCase(Strings.Right(GetSingular(), 3)) == "IES")
//                {
//                    if (UpperCase)
//                    {
//                        functionReturnValue = Strings.Mid(GetSingular(), 1, Strings.Len(GetSingular()) - 3) + "Y";
//                    }
//                    else {
//                        functionReturnValue = Strings.Mid(GetSingular(), 1, Strings.Len(GetSingular()) - 3) + "y";
//                    }
//                }
//                else if (vbUCase(Strings.Right(GetSingular(), 2)) == "SS")
//                {
//                    // nothing
//                }
//                else if (vbUCase(Strings.Right(GetSingular(), 1)) == "S")
//                {
//                    functionReturnValue = Strings.Mid(GetSingular(), 1, Strings.Len(GetSingular()) - 1);
//                }
//                else {
//                    // nothing
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static string EncodeJavascript(string Source)
//        {
//            string functionReturnValue = null;
//            //
//            functionReturnValue = Source;
//            functionReturnValue = vbReplace(EncodeJavascript(), "\\", "\\\\");
//            functionReturnValue = vbReplace(EncodeJavascript(), "'", "\\'");
//            //EncodeJavascript = vbReplace(EncodeJavascript, "'", "'+""'""+'")
//            functionReturnValue = vbReplace(EncodeJavascript(), Constants.vbCrLf, "\\n");
//            functionReturnValue = vbReplace(EncodeJavascript(), Constants.vbCr, "\\n");
//            functionReturnValue = vbReplace(EncodeJavascript(), Constants.vbLf, "\\n");
//            return functionReturnValue;
//            //
//        }
//        /// <summary>
//        /// returns a 1-based index into the comma seperated ListOfItems where Item is found
//        /// </summary>
//        /// <param name="Item"></param>
//        /// <param name="ListOfItems"></param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        public static int GetListIndex(string Item, string ListOfItems)
//        {
//            int functionReturnValue = 0;
//            //
//            string[] Items = null;
//            string LcaseItem = null;
//            string LcaseList = null;
//            int Ptr = 0;
//            //
//            functionReturnValue = 0;
//            if (!string.IsNullOrEmpty(ListOfItems))
//            {
//                LcaseItem = vbLCase(Item);
//                LcaseList = vbLCase(ListOfItems);
//                Items = SplitDelimited(LcaseList, ",");
//                for (Ptr = 0; Ptr <= Information.UBound(Items); Ptr++)
//                {
//                    if (Items(Ptr) == LcaseItem)
//                    {
//                        functionReturnValue = Ptr + 1;
//                        break; // TODO: might not be correct. Was : Exit For
//                    }
//                }
//            }
//            return functionReturnValue;
//            //
//        }
//        //
//        //========================================================================================================
//        //
//        // Finds all tags matching the input, and concatinates them into the output
//        // does NOT account for nested tags, use for body, script, style
//        //
//        // ReturnAll - if true, it returns all the occurances, back-to-back
//        //
//        //========================================================================================================
//        //
//        public static string GetTagInnerHTML(string PageSource, string Tag, bool ReturnAll)
//        {
//            string functionReturnValue = null;
//            // ERROR: Not supported in C#: OnErrorStatement

//            //
//            int TagStart = 0;
//            int TagEnd = 0;
//            int LoopCnt = 0;
//            string WB = null;
//            int Pos = 0;
//            int PosEnd = 0;
//            int CommentPos = 0;
//            int ScriptPos = 0;
//            //
//            functionReturnValue = "";
//            Pos = 1;
//            while ((Pos > 0) & (LoopCnt < 100))
//            {
//                TagStart = vbInstr(Pos, PageSource, "<" + Tag, Constants.vbTextCompare);
//                if (TagStart == 0)
//                {
//                    Pos = 0;
//                }
//                else {
//                    //
//                    // tag found, skip any comments that start between current position and the tag
//                    //
//                    CommentPos = vbInstr(Pos, PageSource, "<!--");
//                    if ((CommentPos != 0) & (CommentPos < TagStart))
//                    {
//                        //
//                        // skip comment and start again
//                        //
//                        Pos = vbInstr(CommentPos, PageSource, "-->");
//                    }
//                    else {
//                        ScriptPos = vbInstr(Pos, PageSource, "<script");
//                        if ((ScriptPos != 0) & (ScriptPos < TagStart))
//                        {
//                            //
//                            // skip comment and start again
//                            //
//                            Pos = vbInstr(ScriptPos, PageSource, "</script");
//                        }
//                        else {
//                            //
//                            // Get the tags innerHTML
//                            //
//                            TagStart = vbInstr(TagStart, PageSource, ">", Constants.vbTextCompare);
//                            Pos = TagStart;
//                            if (TagStart != 0)
//                            {
//                                TagStart = TagStart + 1;
//                                TagEnd = vbInstr(TagStart, PageSource, "</" + Tag, Constants.vbTextCompare);
//                                if (TagEnd != 0)
//                                {
//                                    functionReturnValue += Strings.Mid(PageSource, TagStart, TagEnd - TagStart);
//                                }
//                            }
//                        }
//                    }
//                    LoopCnt = LoopCnt + 1;
//                    if (ReturnAll)
//                    {
//                        TagStart = vbInstr(TagEnd, PageSource, "<" + Tag, Constants.vbTextCompare);
//                    }
//                    else {
//                        TagStart = 0;
//                    }
//                }
//            }
//            return functionReturnValue;
//            ErrorTrap:
//            return functionReturnValue;
//            //
//            //
//        }
//        //
//        //
//        //
//        public static int EncodeInteger(object Expression)
//        {
//            int functionReturnValue = 0;
//            //
//            functionReturnValue = 0;
//            if (vbIsNumeric(Expression))
//            {
//                functionReturnValue = Convert.ToInt32(Expression);
//            }
//            else if (Expression is bool)
//            {
//                if ((bool)Expression)
//                {
//                    functionReturnValue = 1;
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //
//        //
//        public static double EncodeNumber(object Expression)
//        {
//            double functionReturnValue = 0;
//            functionReturnValue = 0;
//            if (vbIsNumeric(Expression))
//            {
//                functionReturnValue = Convert.ToDouble(Expression);
//            }
//            else if (Expression is bool)
//            {
//                if ((bool)Expression)
//                {
//                    functionReturnValue = 1;
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //====================================================================================================
//        //
//        public static string EncodeText(object Expression)
//        {
//            string functionReturnValue = null;
//            try
//            {
//                functionReturnValue = "";
//                if (!(Expression is DBNull))
//                {
//                    if ((Expression != null))
//                    {
//                        functionReturnValue = Convert.ToString(Expression);
//                    }
//                }
//            }
//            catch
//            {
//                functionReturnValue = "";
//            }
//            return functionReturnValue;
//        }
//        //
//        //====================================================================================================
//        //
//        public static bool EncodeBoolean(object Expression)
//        {
//            bool functionReturnValue = false;
//            functionReturnValue = false;
//            if (Expression is bool)
//            {
//                functionReturnValue = (bool)Expression;
//            }
//            else if (vbIsNumeric(Expression))
//            {
//                functionReturnValue = (Convert.ToString(Expression) != "0");
//            }
//            else if (Expression is string)
//            {
//                switch (Expression.ToString.ToLower.Trim)
//                {
//                    case "on":
//                    case "yes":
//                    case "true":
//                        functionReturnValue = true;
//                        break;
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //====================================================================================================
//        //
//        public static System.DateTime EncodeDate(object Expression)
//        {
//            System.DateTime functionReturnValue = default(System.DateTime);
//            functionReturnValue = System.DateTime.MinValue;
//            if (Information.IsDate(Expression))
//            {
//                functionReturnValue = Convert.ToDateTime(Expression);
//                //If EncodeDate < #1/1/1990# Then
//                //    EncodeDate = Date.MinValue
//                //End If
//            }
//            return functionReturnValue;
//        }
//        //
//        //========================================================================
//        //   Gets the next line from a string, and removes the line
//        //========================================================================
//        //
//        public static string getLine(ref string Body)
//        {
//            string returnFirstLine = Body;
//            try
//            {
//                int EOL = 0;
//                int NextCR = 0;
//                int NextLF = 0;
//                int BOL = 0;
//                //
//                NextCR = vbInstr(1, Body, Constants.vbCr);
//                NextLF = vbInstr(1, Body, Constants.vbLf);

//                if (NextCR != 0 | NextLF != 0)
//                {
//                    if (NextCR != 0)
//                    {
//                        if (NextLF != 0)
//                        {
//                            if (NextCR < NextLF)
//                            {
//                                EOL = NextCR - 1;
//                                if (NextLF == NextCR + 1)
//                                {
//                                    BOL = NextLF + 1;
//                                }
//                                else {
//                                    BOL = NextCR + 1;
//                                }

//                            }
//                            else {
//                                EOL = NextLF - 1;
//                                BOL = NextLF + 1;
//                            }
//                        }
//                        else {
//                            EOL = NextCR - 1;
//                            BOL = NextCR + 1;
//                        }
//                    }
//                    else {
//                        EOL = NextLF - 1;
//                        BOL = NextLF + 1;
//                    }
//                    returnFirstLine = Strings.Mid(Body, 1, EOL);
//                    Body = Strings.Mid(Body, BOL);
//                }
//                else {
//                    returnFirstLine = Body;
//                    Body = "";
//                }
//            }
//            catch (Exception ex)
//            {
//                //
//                //
//                //
//            }
//            return returnFirstLine;
//        }
//        //
//        //
//        //
//        public static string runProcess(cpCoreClass cpCore, string Cmd, string Arguments = "", bool WaitForReturn = false)
//        {
//            string returnResult = "";
//            Process p = new Process();
//            //
//            cpCore.log_appendLog("ccCommonModule.runProcess, cmd=[" + Cmd + "], Arguments=[" + Arguments + "], WaitForReturn=[" + WaitForReturn + "]");
//            //
//            p.StartInfo.FileName = Cmd;
//            p.StartInfo.Arguments = Arguments;
//            p.StartInfo.UseShellExecute = false;
//            p.StartInfo.CreateNoWindow = true;
//            p.StartInfo.ErrorDialog = false;
//            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
//            if (WaitForReturn)
//            {
//                p.StartInfo.RedirectStandardOutput = true;
//            }
//            p.Start();
//            if (WaitForReturn)
//            {
//                p.WaitForExit(1000 * 60 * 5);
//                returnResult = p.StandardOutput.ReadToEnd();
//            }

//            p.Dispose();
//            //
//            return returnResult;
//        }
//        //'
//        //Public Sub runProcess(cp.core,cmd As String, Optional ignore As Object = "", Optional waitforreturn As Boolean = False)
//        //    Call runProcess(cp.core,cmd, waitforreturn)
//        //    'Dim ShellObj As Object
//        //    'ShellObj = CreateObject("WScript.Shell")
//        //    'If Not (ShellObj Is Nothing) Then
//        //    '    Call ShellObj.Run(Cmd, 0, WaitForReturn)
//        //    'End If
//        //    'ShellObj = Nothing
//        //End Sub
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //   use only internally
//        //
//        //   encode an argument to be used in a name=value& (N-V-A) string
//        //
//        //   an argument can be any one of these is this format:
//        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
//        //
//        //   to create an nva string
//        //       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
//        //
//        //   to decode an nva string
//        //       split on ampersand then on equal, and decodeNvaArgument() each part
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //
//        public static string encodeNvaArgument(string Arg)
//        {
//            string a = null;
//            a = Arg;
//            if (!string.IsNullOrEmpty(a))
//            {
//                a = vbReplace(a, Constants.vbCrLf, "#0013#");
//                a = vbReplace(a, Constants.vbLf, "#0013#");
//                a = vbReplace(a, Constants.vbCr, "#0013#");
//                a = vbReplace(a, "&", "#0038#");
//                a = vbReplace(a, "=", "#0061#");
//                a = vbReplace(a, ",", "#0044#");
//                a = vbReplace(a, "\"", "#0034#");
//                a = vbReplace(a, "'", "#0039#");
//                a = vbReplace(a, "|", "#0124#");
//                a = vbReplace(a, "[", "#0091#");
//                a = vbReplace(a, "]", "#0093#");
//                a = vbReplace(a, ":", "#0058#");
//            }
//            return a;
//        }
//        //
//        //------------------------------------------------------------------------------------------------------------
//        //   use only internally
//        //       decode an argument removed from a name=value& string
//        //       see encodeNvaArgument for details on how to use this
//        //------------------------------------------------------------------------------------------------------------
//        //
//        public static string decodeNvaArgument(string EncodedArg)
//        {
//            string a = null;
//            //
//            a = EncodedArg;
//            a = vbReplace(a, "#0058#", ":");
//            a = vbReplace(a, "#0093#", "]");
//            a = vbReplace(a, "#0091#", "[");
//            a = vbReplace(a, "#0124#", "|");
//            a = vbReplace(a, "#0039#", "'");
//            a = vbReplace(a, "#0034#", "\"");
//            a = vbReplace(a, "#0044#", ",");
//            a = vbReplace(a, "#0061#", "=");
//            a = vbReplace(a, "#0038#", "&");
//            a = vbReplace(a, "#0013#", Constants.vbCrLf);
//            return a;
//        }
//        //
//        //
//        public static string LogFileCopyPrep(string Source)
//        {
//            string Copy = null;
//            Copy = Source;
//            Copy = vbReplace(Copy, Constants.vbCrLf, " ");
//            Copy = vbReplace(Copy, Constants.vbLf, " ");
//            Copy = vbReplace(Copy, Constants.vbCr, " ");
//            return Copy;
//        }
//        //        '
//        //        '========================================================================
//        //        '   encodeSQLText
//        //        '========================================================================
//        //        '
//        //        Public Function app.EncodeSQLText(ByVal expression As Object) As String
//        //            Dim returnString As String = ""
//        //            If expression Is Nothing Then
//        //                returnString = "null"
//        //            Else
//        //                returnString = EncodeText(expression)
//        //                If returnString = "" Then
//        //                    returnString = "null"
//        //                Else
//        //                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
//        //                End If
//        //            End If
//        //            Return returnString
//        //        End Function
//        //        '
//        //        '========================================================================
//        //        '   encodeSQLLongText
//        //        '========================================================================
//        //        '
//        //        Public Function app.EncodeSQLText(ByVal expression As Object) As String
//        //            Dim returnString As String = ""
//        //            If expression Is Nothing Then
//        //                returnString = "null"
//        //            Else
//        //                returnString = EncodeText(expression)
//        //                If returnString = "" Then
//        //                    returnString = "null"
//        //                Else
//        //                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
//        //                End If
//        //            End If
//        //            Return returnString
//        //        End Function
//        //        '
//        //        '========================================================================
//        //        '   encodeSQLDate
//        //        '       encode a date variable to go in an sql expression
//        //        '========================================================================
//        //        '
//        //        Public Function app.EncodeSQLDate(ByVal expression As Object) As String
//        //            Dim returnString As String = ""
//        //            Dim expressionDate As Date = Date.MinValue
//        //            If expression Is Nothing Then
//        //                returnString = "null"
//        //            ElseIf Not IsDate(expression) Then
//        //                returnString = "null"
//        //            Else
//        //                If IsDBNull(expression) Then
//        //                    returnString = "null"
//        //                Else
//        //                    expressionDate = EncodeDate(expression)
//        //                    If (expressionDate = Date.MinValue) Then
//        //                        returnString = "null"
//        //                    Else
//        //                        returnString = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
//        //                    End If
//        //                End If
//        //            End If
//        //            Return returnString
//        //        End Function
//        //        '
//        //        '========================================================================
//        //        '   encodeSQLNumber
//        //        '       encode a number variable to go in an sql expression
//        //        '========================================================================
//        //        '
//        //        Public Function app.EncodeSQLNumber(ByVal expression As Object) As String
//        //            Dim returnString As String = ""
//        //            Dim expressionNumber As Double = 0
//        //            If expression Is Nothing Then
//        //                returnString = "null"
//        //            ElseIf VarType(expression) = vbBoolean Then
//        //                If expression Then
//        //                    returnString = SQLTrue
//        //                Else
//        //                    returnString = SQLFalse
//        //                End If
//        //            ElseIf Not vbIsNumeric(expression) Then
//        //                returnString = "null"
//        //            Else
//        //                returnString = expression.ToString
//        //            End If
//        //            Return returnString
//        //            Exit Function
//        //            '
//        //            ' ----- Error Trap
//        //            '
//        //ErrorTrap:
//        //        End Function
//        //        '
//        //        '========================================================================
//        //        '   encodeSQLBoolean
//        //        '       encode a boolean variable to go in an sql expression
//        //        '========================================================================
//        //        '
//        //        Public Function app.EncodeSQLBoolean(ByVal ExpressionVariant As Object) As String
//        //            '
//        //            'Dim src As String
//        //            '
//        //            app.EncodeSQLBoolean = SQLFalse
//        //            If EncodeBoolean(ExpressionVariant) Then
//        //                app.EncodeSQLBoolean = SQLTrue
//        //            End If
//        //        End Function
//        //
//        //=================================================================================
//        //   Renamed to catch all the cases that used it in addons
//        //
//        //   Do not use this routine in Addons to get the addon option string value
//        //   to get the value in an option string, use cmc.csv_getAddonOption("name")
//        //
//        // Get the value of a name in a string of name value pairs parsed with vrlf and =
//        //   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
//        //   new format is "name1=value1 crlf name2=value2 crlf ..."
//        //   There can be no extra spaces between the delimiter, the name and the "="
//        //=================================================================================
//        //
//        public static string getSimpleNameValue(string Name, string ArgumentString, string DefaultValue, string Delimiter)
//        {
//            string functionReturnValue = null;
//            //
//            string WorkingString = null;
//            string iDefaultValue = null;
//            int NameLength = 0;
//            int ValueStart = 0;
//            int ValueEnd = 0;
//            bool IsQuoted = false;
//            //
//            // determine delimiter
//            //
//            if (string.IsNullOrEmpty(Delimiter))
//            {
//                //
//                // If not explicit
//                //
//                if (vbInstr(1, ArgumentString, Constants.vbCrLf) != 0)
//                {
//                    //
//                    // crlf can only be here if it is the delimiter
//                    //
//                    Delimiter = Constants.vbCrLf;
//                }
//                else {
//                    //
//                    // either only one option, or it is the legacy '&' delimit
//                    //
//                    Delimiter = "&";
//                }
//            }
//            iDefaultValue = DefaultValue;
//            WorkingString = ArgumentString;
//            functionReturnValue = iDefaultValue;
//            if (!string.IsNullOrEmpty(WorkingString))
//            {
//                WorkingString = Delimiter + WorkingString + Delimiter;
//                ValueStart = vbInstr(1, WorkingString, Delimiter + Name + "=", Constants.vbTextCompare);
//                if (ValueStart != 0)
//                {
//                    NameLength = Strings.Len(Name);
//                    ValueStart = ValueStart + Strings.Len(Delimiter) + NameLength + 1;
//                    if (Strings.Mid(WorkingString, ValueStart, 1) == "\"")
//                    {
//                        IsQuoted = true;
//                        ValueStart = ValueStart + 1;
//                    }
//                    if (IsQuoted)
//                    {
//                        ValueEnd = vbInstr(ValueStart, WorkingString, "\"" + Delimiter);
//                    }
//                    else {
//                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter);
//                    }
//                    if (ValueEnd == 0)
//                    {
//                        functionReturnValue = Strings.Mid(WorkingString, ValueStart);
//                    }
//                    else {
//                        functionReturnValue = Strings.Mid(WorkingString, ValueStart, ValueEnd - ValueStart);
//                    }
//                }
//            }
//            return functionReturnValue;
//            ErrorTrap:
//            return functionReturnValue;
//            //

//            //
//            // ----- ErrorTrap
//            //
//        }
//        //==========================================================================================================================
//        //   To convert from site license to server licenses, we still need the URLEncoder in the site license
//        //   This routine generates a site license that is just the URL encoder.
//        //==========================================================================================================================
//        //
//        public static string GetURLEncoder()
//        {
//            VBMath.Randomize();
//            return Convert.ToString(Conversion.Int(1 + (VBMath.Rnd() * 8))) + Convert.ToString(Conversion.Int(1 + (VBMath.Rnd() * 8))) + Convert.ToString(Conversion.Int(1000000000 + (VBMath.Rnd() * 899999999)));
//        }
//        //
//        //
//        //
//        public static string getIpAddressList()
//        {
//            string ipAddressList = "";
//            IPHostEntry host = default(IPHostEntry);
//            //
//            host = Dns.GetHostEntry(Dns.GetHostName());
//            foreach (IPAddress ip in host.AddressList)
//            {
//                if ((ip.AddressFamily == Sockets.AddressFamily.InterNetwork))
//                {
//                    ipAddressList += "," + ip.ToString;
//                }
//            }
//            if (!string.IsNullOrEmpty(ipAddressList))
//            {
//                ipAddressList = ipAddressList.Substring(1);
//            }
//            return ipAddressList;
//        }
//        //'
//        //'
//        //'
//        //Public Function GetAddonRootPath() As String
//        //    Dim testPath As String
//        //    '
//        //    GetAddonRootPath = "addons"
//        //    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
//        //        '
//        //        ' debugging - change program path to dummy path so addon builds all copy to
//        //        '
//        //        testPath = Environ$("programfiles(x86)")
//        //        If testPath = "" Then
//        //            testPath = Environ$("programfiles")
//        //        End If
//        //        GetAddonRootPath = testPath & "\kma\contensive\addons"
//        //    End If
//        //End Function
//        //
//        //
//        //
//        public static bool IsNull(object source)
//        {
//            return (source == null);
//        }
//        //'
//        //'
//        //'
//        //Public Function isNothing(ByVal source As Object) As Boolean
//        //    Return IsNothing(source)
//        //    'Dim returnIsEmpty As Boolean = True
//        //    'Try
//        //    '    If Not IsNothing(source) Then

//        //    '    End If
//        //    'Catch ex As Exception
//        //    '    '
//        //    'End Try
//        //    'Return returnIsEmpty
//        //End Function
//        //
//        //
//        //
//        public static bool isMissing(object source)
//        {
//            return false;
//        }
//        //
//        // convert date to number of seconds since 1/1/1990
//        //
//        public static int dateToSeconds(System.DateTime sourceDate)
//        {
//            int returnSeconds = 0;
//            object oldDate = new System.DateTime(1900, 1, 1);
//            if (sourceDate.CompareTo(oldDate) > 0)
//            {
//                returnSeconds = Convert.ToInt32(sourceDate.Subtract(oldDate).TotalSeconds);
//            }
//            return returnSeconds;
//        }
//        public const maxLongValue = 2147483647;
//        //
//        // Error Definitions
//        //
//        public const int ERR_UNKNOWN = Constants.vbObjectError + 101;
//        public const int ERR_FIELD_DOES_NOT_EXIST = Constants.vbObjectError + 102;
//        public const int ERR_FILESIZE_NOT_ALLOWED = Constants.vbObjectError + 103;
//        public const int ERR_FOLDER_DOES_NOT_EXIST = Constants.vbObjectError + 104;
//        public const int ERR_FILE_ALREADY_EXISTS = Constants.vbObjectError + 105;
//        public const int ERR_FILE_TYPE_NOT_ALLOWED = Constants.vbObjectError + 106;
//        //
//        // page content cache
//        //
//        public const pageManager_cache_pageContent_cacheName = "cache_pageContent";
//        public const pageManager_cache_pageContent_fieldList = "ID,Active,ParentID,Name,Headline,MenuHeadline,DateArchive,DateExpires,PubDate,ChildListSortMethodID,ContentControlID,TemplateID" + ",BlockContent,BlockPage,Link,RegistrationGroupID,BlockSourceID,CustomBlockMessage,JSOnLoad,JSHead,JSEndBody,Viewings,ContactMemberID" + ",AllowHitNotification,TriggerSendSystemEmailID,TriggerConditionID,TriggerConditionGroupID,TriggerAddGroupID,TriggerRemoveGroupID,AllowMetaContentNoFollow" + ",ParentListName,copyFilename,BriefFilename,AllowChildListDisplay,SortOrder,DateAdded,ModifiedDate,ChildPagesFound,AllowInMenus,AllowInChildLists" + ",JSFilename,ChildListInstanceOptions,IsSecure,AllowBrief,allowReturnLinkDisplay,AllowPrinterVersion,allowEmailPage,allowSeeAlso,allowMoreInfo" + ",allowFeedback,allowLastModifiedFooter,modifiedBy,DateReviewed,ReviewedBy,allowReviewedFooter,allowMessageFooter,contentpadding";
//        public const PCC_ID = 0;
//        public const PCC_Active = 1;
//        public const PCC_ParentID = 2;
//        public const PCC_Name = 3;
//        public const PCC_Headline = 4;
//        public const PCC_MenuHeadline = 5;
//        public const PCC_DateArchive = 6;
//        public const PCC_DateExpires = 7;
//        public const PCC_PubDate = 8;
//        public const PCC_ChildListSortMethodID = 9;
//        public const PCC_ContentControlID = 10;
//        public const PCC_TemplateID = 11;
//        public const PCC_BlockContent = 12;
//        public const PCC_BlockPage = 13;
//        public const PCC_Link = 14;
//        public const PCC_RegistrationGroupID = 15;
//        public const PCC_BlockSourceID = 16;
//        public const PCC_CustomBlockMessageFilename = 17;
//        public const PCC_JSOnLoad = 18;
//        public const PCC_JSHead = 19;
//        public const PCC_JSEndBody = 20;
//        public const PCC_Viewings = 21;
//        public const PCC_ContactMemberID = 22;
//        public const PCC_AllowHitNotification = 23;
//        public const PCC_TriggerSendSystemEmailID = 24;
//        public const PCC_TriggerConditionID = 25;
//        public const PCC_TriggerConditionGroupID = 26;
//        public const PCC_TriggerAddGroupID = 27;
//        public const PCC_TriggerRemoveGroupID = 28;
//        public const PCC_AllowMetaContentNoFollow = 29;
//        public const PCC_ParentListName = 30;
//        public const PCC_CopyFilename = 31;
//        public const PCC_BriefFilename = 32;
//        public const PCC_AllowChildListDisplay = 33;
//        public const PCC_SortOrder = 34;
//        public const PCC_DateAdded = 35;
//        public const PCC_ModifiedDate = 36;
//        public const PCC_ChildPagesFound = 37;
//        public const PCC_AllowInMenus = 38;
//        public const PCC_AllowInChildLists = 39;
//        public const PCC_JSFilename = 40;
//        public const PCC_ChildListInstanceOptions = 41;
//        public const PCC_IsSecure = 42;
//        public const PCC_AllowBrief = 43;
//        public const PCC_allowReturnLinkDisplay = 44;
//        public const pcc_allowPrinterVersion = 45;
//        public const pcc_allowEmailPage = 46;
//        public const pcc_allowSeeAlso = 47;
//        public const pcc_allowMoreInfo = 48;
//        public const pcc_allowFeedback = 49;
//        public const pcc_allowLastModifiedFooter = 50;
//        public const PCC_ModifiedBy = 51;
//        public const PCC_DateReviewed = 52;
//        public const PCC_ReviewedBy = 53;
//        public const PCC_allowReviewedFooter = 54;
//        public const PCC_allowMessageFooter = 55;
//        public const PCC_ContentPadding = 56;
//        public const PCC_ColCnt = 57;
//        //
//        // addonIncludeRules cache
//        //
//        public const cache_sharedStylesAddonRules_cacheName = "cache_sharedStylesAddonRules";
//        public const cache_sharedStylesAddonRules_fieldList = "addonId,styleId";
//        public const sharedStylesAddonRulesCache_addonId = 0;
//        public const sharedStylesAddonRulesCache_styleId = 1;
//        public const sharedStylesAddonRulesCacheColCnt = 2;
//        //
//        // linkalias cache
//        //
//        public const cache_linkAlias_fieldList = "ID,Name,Link,PageID,QueryStringSuffix";
//        public const linkAliasCache_id = 0;
//        public const linkAliasCache_name = 1;
//        public const linkAliasCache_link = 2;
//        public const linkAliasCache_pageId = 3;
//        public const linkAliasCache_queryStringSuffix = 4;
//        public const linkAliasCacheColCnt = 5;
//        //
//        //   addonCache
//        //
//        //Public Const cache_addon_cacheName = "cache_addon"
//        //Public Const cache_addon_fieldList = "id,active,name,ccguid,collectionid,Copy,ccguid,Link,ObjectProgramID,DotNetClass,ArgumentList,CopyText,IsInline,BlockDefaultStyles,StylesFilename,CustomStylesFilename,formxml,RemoteAssetLink,AsAjax,InFrame,ScriptingEntryPoint,ScriptingLanguageID,ScriptingCode,BlockEditTools,ScriptingTimeout,inlineScript,help,helplink,JavaScriptOnLoad,JavaScriptBodyEnd,PageTitle,MetaDescription,MetaKeywordList,OtherHeadTags,JSFilename,remoteMethod,onBodyStart,onBodyEnd,OnPageStartEvent,OnPageEndEvent,robotsTxt"
//        //Public Const addonCache_Id = 0
//        //Public Const addonCache_active = 1
//        //Public Const addonCache_name = 2
//        //Public Const addonCache_guid = 3
//        //Public Const addonCache_collectionid = 4
//        //Public Const addonCache_Copy = 5
//        //Public Const addonCache_ccguid = 6
//        //Public Const addonCache_Link = 7
//        //Public Const addonCache_ObjectProgramID = 8
//        //Public Const addonCache_DotNetClass = 9
//        //Public Const addonCache_ArgumentList = 10
//        //Public Const addonCache_CopyText = 11
//        //Public Const addonCache_IsInline = 12
//        //Public Const addonCache_BlockDefaultStyles = 13
//        //Public Const addonCache_StylesFilename = 14
//        //Public Const addonCache_CustomStylesFilename = 15
//        //Public Const addonCache_formxml = 16
//        //Public Const addonCache_RemoteAssetLink = 17
//        //Public Const addonCache_AsAjax = 18
//        //Public Const addonCache_InFrame = 19
//        //Public Const addonCache_ScriptingEntryPoint = 20
//        //Public Const addonCache_ScriptingLanguageID = 21
//        //Public Const addonCache_ScriptingCode = 22
//        //Public Const addonCache_BlockEditTools = 23
//        //Public Const addonCache_ScriptingTimeout = 24
//        //Public Const addonCache_inlineScript = 25
//        //Public Const addonCache_help = 26
//        //Public Const addonCache_helpLink = 27
//        //Public Const addonCache_JavaScriptOnLoad = 28
//        //Public Const addonCache_JavaScriptBodyEnd = 29
//        //Public Const addonCache_PageTitle = 30
//        //Public Const addonCache_MetaDescription = 31
//        //Public Const addonCache_MetaKeywordList = 32
//        //Public Const addonCache_OtherHeadTags = 33
//        //Public Const addonCache_JSFilename = 34
//        //Public Const addonCache_remoteMethod = 35
//        //Public Const addoncache_OnBodyStart = 36
//        //Public Const addoncache_OnBodyEnd = 37
//        //Public Const addoncache_OnPageStart = 38
//        //Public Const addoncache_OnPageEnd = 39
//        //Public Const addoncache_robotsTxt = 40
//        //Public Const addonCacheColCnt = 41
//        //
//        // addonIncludeRules cache
//        //
//        public const cache_addonIncludeRules_cacheName = "cache_addonIncludeRules";
//        public const cache_addonIncludeRules_fieldList = "addonId,includedAddonId";
//        public const addonIncludeRulesCache_addonId = 0;
//        public const addonIncludeRulesCache_includedAddonId = 1;
//        public const addonIncludeRulesCacheColCnt = 2;
//        //
//        // addonIncludeRules cache
//        //
//        public const cache_LibraryFiles_cacheName = "cache_LibraryFiles";
//        public const cache_LibraryFiles_fieldList = "id,ccguid,clicks,filename,width,height,AltText,altsizelist";
//        public const LibraryFilesCache_Id = 0;
//        public const LibraryFilesCache_ccGuid = 1;
//        public const LibraryFilesCache_clicks = 2;
//        public const LibraryFilesCache_filename = 3;
//        public const LibraryFilesCache_width = 4;
//        public const LibraryFilesCache_height = 5;
//        public const LibraryFilesCache_alttext = 6;
//        public const LibraryFilesCache_altsizelist = 7;
//        public const LibraryFilesCacheColCnt = 8;
//        //
//        // link forward cache
//        //
//        public const cache_linkForward_cacheName = "cache_linkForward";
//        //
//        // ==============================================================================
//        // true if there is a previous instance of this application running
//        // ==============================================================================
//        //
//        public static bool PrevInstance()
//        {
//            if (Information.UBound(Diagnostics.Process.GetProcessesByName(Diagnostics.Process.GetCurrentProcess.ProcessName)) > 0)
//            {
//                return true;
//            }
//            else {
//                return false;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Encode a date to minvalue, if date is < minVAlue,m set it to minvalue, if date < 1/1/1990 (the beginning of time), it returns date.minvalue
//        /// </summary>
//        /// <param name="sourceDate"></param>
//        /// <returns></returns>
//        public static System.DateTime encodeDateMinValue(System.DateTime sourceDate)
//        {
//            if (sourceDate <= 1 / 1 / 1000 12:00:00 AM) {
//                return System.DateTime.MinValue;
//            }
//            return sourceDate;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Return true if a date is the mindate, else return false 
//        /// </summary>
//        /// <param name="sourceDate"></param>
//        /// <returns></returns>
//        public static bool isMinDate(System.DateTime sourceDate)
//        {
//            return encodeDateMinValue(sourceDate) == System.DateTime.MinValue;
//        }

//        //
//        //========================================================================
//        // ----- Create a filename for the Virtual Directory
//        //   Do not allow spaces.
//        //   If the content supports authoring, the filename returned will be for the
//        //   current authoring record.
//        //========================================================================
//        //
//        public static string csv_GetVirtualFilenameByTable(string TableName, string FieldName, int RecordID, string OriginalFilename, int fieldType)
//        {
//            string functionReturnValue = null;
//            //
//            string RecordIDString = null;
//            string iTableName = null;
//            string iFieldName = null;
//            int iRecordID = 0;
//            string MethodName = null;
//            string iOriginalFilename = null;
//            //
//            MethodName = "csv_GetVirtualFilenameByTable";
//            //
//            iTableName = TableName;
//            iTableName = vbReplace(iTableName, " ", "_");
//            iTableName = vbReplace(iTableName, ".", "_");
//            //
//            iFieldName = FieldName;
//            iFieldName = vbReplace(FieldName, " ", "_");
//            iFieldName = vbReplace(iFieldName, ".", "_");
//            //
//            iOriginalFilename = OriginalFilename;
//            iOriginalFilename = vbReplace(iOriginalFilename, " ", "_");
//            iOriginalFilename = vbReplace(iOriginalFilename, ".", "_");
//            //
//            RecordIDString = Convert.ToString(RecordID);
//            if (RecordID == 0)
//            {
//                RecordIDString = Convert.ToString(getRandomLong());
//                RecordIDString = new string('0', 12 - Strings.Len(RecordIDString)) + RecordIDString;
//            }
//            else {
//                RecordIDString = new string('0', 12 - Strings.Len(RecordIDString)) + RecordIDString;
//            }
//            //
//            if (!string.IsNullOrEmpty(OriginalFilename))
//            {
//                functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + "/" + OriginalFilename;
//            }
//            else {
//                switch (fieldType)
//                {
//                    case FieldTypeIdFileCSS:
//                        functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + ".css";
//                        break;
//                    case FieldTypeIdFileXML:
//                        functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + ".xml";
//                        break;
//                    case FieldTypeIdFileJavascript:
//                        functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + ".js";
//                        break;
//                    case FieldTypeIdFileHTMLPrivate:
//                        functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + ".html";
//                        break;
//                    default:
//                        functionReturnValue = iTableName + "/" + iFieldName + "/" + RecordIDString + ".txt";
//                        break;
//                }
//            }
//            return functionReturnValue;
//        }
//        //
//        //=================================================================================
//        //   Get a Random Long Value
//        //=================================================================================
//        //
//        public static int getRandomLong()
//        {
//            //
//            int RandomBase = 0;
//            int RandomLimit = 0;
//            string MethodName = null;
//            //
//            MethodName = "getRandomLong";
//            //
//            RandomBase = Threading.Thread.CurrentThread.ManagedThreadId;
//            RandomBase = EncodeInteger(RandomBase + ((Math.Pow(2, 30)) - 1));
//            RandomLimit = EncodeInteger((Math.Pow(2, 31)) - RandomBase - 1);
//            VBMath.Randomize();
//            return EncodeInteger(RandomBase + (VBMath.Rnd() * RandomLimit));
//        }
//        //
//        //====================================================================================================
//        // the root folder where all app data is stored.
//        //   In local mode, this folder is the backup target
//        //   -- path means no trailing slash
//        //====================================================================================================
//        //
//        public static string getProgramDataFolder()
//        {
//            //Return "c:\inetpub"
//            return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\clib";
//        }
//        //
//        //====================================================================================================
//        // the the c:\program files x86) path 
//        //   -- path means no trailing slash
//        //====================================================================================================
//        //
//        public static string getProgramFilesPath()
//        {
//            string returnPath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
//            int ptr = 0;
//            //
//            ptr = vbInstr(1, returnPath, "\\github\\", Constants.vbTextCompare);
//            if (ptr != 0)
//            {
//                // for ...\github\contensive4?\bin"
//                ptr = vbInstr(ptr + 8, returnPath, "\\");
//                returnPath = Strings.Left(returnPath, ptr) + "bin\\";
//            }

//            return returnPath;
//        }
//        //
//        //====================================================================================================
//        // the the \cclib path 
//        //   -- path means no trailing slash
//        //====================================================================================================
//        //
//        public static string getccLibPath()
//        {
//            return getProgramFilesPath() + "ccLib\\";
//        }
//        //
//        //====================================================================================================
//        // the the name of the current executable
//        //====================================================================================================
//        //
//        public static string getAppExeName()
//        {
//            return System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath);
//        }
//        //
//        //====================================================================================================
//        // convert a dtaTable to a comma delimited list of column 0
//        //====================================================================================================
//        //
//        public static string convertDataTableColumntoItemList(DataTable dt)
//        {
//            string returnString = "";
//            foreach (DataRow dr in dt.Rows)
//            {
//                returnString += "," + dr.Item(0).ToString;
//            }
//            if (returnString.Length > 0)
//            {
//                returnString = returnString.Substring(1);
//            }
//            return returnString;
//        }
//        //
//        //====================================================================================================
//        // convert a dtaTable to a simple array - quick way to adapt old code
//        //====================================================================================================
//        //
//        public static string[,] convertDataTabletoArray(DataTable dt)
//        {
//            //Public Function convertDataTabletoArray(dt As DataTable, ByRef returnSuccess As Boolean) As String(,)
//            int columnCnt = 0;
//            int rowCnt = 0;
//            string[,] rows = {

//            };
//            int cPtr = 0;
//            int rPtr = 0;
//            //
//            // 20150717 check for no columns
//            if (((dt.Rows.Count > 0) & (dt.Columns.Count > 0)))
//            {
//                columnCnt = dt.Columns.Count;
//                rowCnt = dt.Rows.Count;
//                // 20150717 change from rows(columnCnt,rowCnt) because other routines appear to use this count
//                rows = new string[columnCnt, rowCnt];
//                rPtr = 0;
//                foreach (DataRow dr in dt.Rows)
//                {
//                    cPtr = 0;
//                    foreach (DataColumn cell in dt.Columns)
//                    {
//                        rows(cPtr, rPtr) = EncodeText(dr(cell));
//                        cPtr += 1;
//                    }
//                    rPtr += 1;
//                }
//            }
//            return rows;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// returns true or false if a string is located within another string. Similar to indexof but returns true/false 
//        /// </summary>
//        /// <param name="start"></param>
//        /// <param name="haystack"></param>
//        /// <param name="needle"></param>
//        /// <param name="ignore"></param>
//        /// <returns></returns>
//        public static bool isInStr(int start, string haystack, string needle, CompareMethod ignore = CompareMethod.Text)
//        {
//            return (Strings.InStr(start, haystack, needle, Constants.vbTextCompare) >= 0);
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Convert a route to the anticipated format (leading /, lowercase, no trailing slash)
//        /// </summary>
//        /// <param name="route"></param>
//        /// <returns></returns>
//        public static string normalizeRoute(string route)
//        {
//            string normalizedRoute = route.ToLower();
//            try
//            {
//                if (string.IsNullOrEmpty(normalizedRoute))
//                {
//                    normalizedRoute = "/";
//                }
//                else {
//                    if ((normalizedRoute.Substring(0, 1) != "/"))
//                    {
//                        normalizedRoute = "/" + normalizedRoute;
//                    }
//                    if ((normalizedRoute.Substring(normalizedRoute.Length - 1, 1) == "/"))
//                    {
//                        normalizedRoute = normalizedRoute.Substring(0, normalizedRoute.Length - 1);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("Unexpected exception in normalizeRoute(route=[" + route + "])", ex);
//            }
//            return normalizedRoute;
//        }
//        //
//        //========================================================================
//        //   converts a virtual file into a filename
//        //       - in local mode, the cdnFiles can be mapped to a virtual folder in appRoot
//        //           -- see appConfig.cdnFilesVirtualFolder
//        //       convert all / to \
//        //       if it includes "://", it is a root file
//        //       if it starts with "/", it is already root relative
//        //       else (if it start with a file or a path), add the publicFileContentPathPrefix
//        //========================================================================
//        //
//        public static string convertCdnUrlToCdnPathFilename(string cdnUrl)
//        {
//            //
//            // this routine was originally written to handle modes that were not adopted (content file absolute and relative URLs)
//            // leave it here as a simple slash converter in case other conversions are needed later
//            //
//            return vbReplace(cdnUrl, "/", "\\");
//        }
//        //
//        //==============================================================================
//        public static bool isGuid(object ignore, string Source)
//        {
//            bool returnValue = false;
//            try
//            {
//                if ((Strings.Len(Source) == 38) & (Strings.Left(Source, 1) == "{") & (Strings.Right(Source, 1) == "}"))
//                {
//                    //
//                    // Good to go
//                    //
//                    returnValue = true;
//                }
//                else if ((Strings.Len(Source) == 36) & (Strings.InStr(1, Source, " ") == 0))
//                {
//                    //
//                    // might be valid with the brackets, add them
//                    //
//                    returnValue = true;
//                    //source = "{" & source & "}"
//                }
//                else if ((Strings.Len(Source) == 32))
//                {
//                    //
//                    // might be valid with the brackets and the dashes, add them
//                    //
//                    returnValue = true;
//                    //source = "{" & Mid(source, 1, 8) & "-" & Mid(source, 9, 4) & "-" & Mid(source, 13, 4) & "-" & Mid(source, 17, 4) & "-" & Mid(source, 21) & "}"
//                }
//                else {
//                    //
//                    // not valid
//                    //
//                    returnValue = false;
//                    //        source = ""
//                }
//            }
//            catch (Exception ex)
//            {
//                throw new ApplicationException("Exception in isGuid", ex);
//            }
//            return returnValue;
//        }
//        //
//        //====================================================================================================
//        //
//        public static int vbInstr(string string1, string string2, CompareMethod compare)
//        {
//            return vbInstr(1, string1, string2, compare);
//        }
//        //
//        public static int vbInstr(string string1, string string2)
//        {
//            return vbInstr(1, string1, string2, CompareMethod.Binary);
//        }
//        //
//        public static int vbInstr(int start, string string1, string string2)
//        {
//            return vbInstr(start, string1, string2, CompareMethod.Binary);
//        }
//        //
//        public static int vbInstr(int start, string string1, string string2, CompareMethod compare)
//        {
//            if ((string.IsNullOrEmpty(string1)))
//            {
//                return 0;
//            }
//            else {
//                if ((start < 1))
//                {
//                    throw new ArgumentException("Instr() start must be > 0.");
//                }
//                else {
//                    if ((compare == CompareMethod.Binary))
//                    {
//                        return string1.IndexOf(string2, start - 1, StringComparison.CurrentCulture) + 1;
//                    }
//                    else {
//                        return string1.IndexOf(string2, start - 1, StringComparison.CurrentCultureIgnoreCase) + 1;
//                    }
//                }
//            }
//        }
//        //
//        //====================================================================================================
//        //
//        public static string vbReplace(string expression, string find, string replacement)
//        {
//            return vbReplace(expression, find, replacement, 1, 9999, CompareMethod.Binary);
//        }
//        //
//        public static string vbReplace(string expression, string find, string replacement, int startIgnore, int countIgnore, CompareMethod compare)
//        {
//            if (string.IsNullOrEmpty(expression))
//            {
//                return expression;
//            }
//            else if (string.IsNullOrEmpty(find))
//            {
//                return expression;
//            }
//            else {
//                if (compare == CompareMethod.Binary)
//                {
//                    return expression.Replace(find, replacement);
//                }
//                else if (string.IsNullOrEmpty(replacement))
//                {
//                    return Regex.Replace(expression, find, "", RegexOptions.IgnoreCase);
//                }
//                else {
//                    return Regex.Replace(expression, find, replacement, RegexOptions.IgnoreCase);
//                }
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic UCase
//        /// </summary>
//        /// <param name="source"></param>
//        /// <returns></returns>
//        public static string vbUCase(string source)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.ToUpper;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic LCase
//        /// </summary>
//        /// <param name="source"></param>
//        /// <returns></returns>
//        public static string vbLCase(string source)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.ToLower;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// visual basic Left()
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public static string vbLeft(string source, int length)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.Substring(length);
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic Right()
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public static string vbRight(string source, int length)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.Substring(source.Length - length);
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic Len()
//        /// </summary>
//        /// <param name="source"></param>
//        /// <returns></returns>
//        public static int vbLen(string source)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return 0;
//            }
//            else {
//                return source.Length;
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic Mid()
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="startIndex"></param>
//        /// <returns></returns>
//        public static string vbMid(string source, int startIndex)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.Substring(startIndex);
//            }
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// Visual Basic Mid()
//        /// </summary>
//        /// <param name="source"></param>
//        /// <param name="startIndex"></param>
//        /// <param name="length"></param>
//        /// <returns></returns>
//        public static string vbMid(string source, int startIndex, int length)
//        {
//            if ((string.IsNullOrEmpty(source)))
//            {
//                return "";
//            }
//            else {
//                return source.Substring(startIndex, length);
//            }
//        }
//        //
//        public static bool vbIsNumeric(object Expression)
//        {
//            if ((Expression is DateTime))
//            {
//                return false;
//            }
//            else if ((Expression == null))
//            {
//                return false;
//            }
//            else if ((Expression is int) | (Expression is Int16) | (Expression is Int32) | (Expression is Int64) | (Expression is decimal) | (Expression is float) | (Expression is double) | (Expression is bool))
//            {
//                return true;
//            }
//            else if ((Expression is string))
//            {
//                double output = 0;
//                return double.TryParse((string)Expression, output);
//            }
//            else {
//                return false;
//            }
//        }
//    }
//    //
//    public class coreCommonTests
//    {
//        //
//        [Fact()]
//        public void normalizeRoute_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual(coreCommonModule.normalizeRoute("TEST"), "/test");
//            Assert.AreEqual(coreCommonModule.normalizeRoute("test"), "/test");
//            Assert.AreEqual(coreCommonModule.normalizeRoute("/test/"), "/test");
//            Assert.AreEqual(coreCommonModule.normalizeRoute("test/"), "/test");
//        }
//        //
//        [Fact()]
//        public void encodeBoolean_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual(coreCommonModule.EncodeBoolean(true), true);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean(0), false);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean(1), true);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean("on"), true);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean("off"), false);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean("true"), true);
//            Assert.AreEqual(coreCommonModule.EncodeBoolean("false"), false);
//        }
//        //
//        [Fact()]
//        public void encodeText_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual(coreCommonModule.EncodeText(1), "1");
//        }
//        //
//        [Fact()]
//        public void sample_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual(true, true);
//        }
//        //
//        [Fact()]
//        public void dateToSeconds_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual(coreCommonModule.dateToSeconds(new System.DateTime(1900, 1, 1)), 0);
//            Assert.AreEqual(coreCommonModule.dateToSeconds(new System.DateTime(1900, 1, 2)), 86400);
//        }
//        //
//        [Fact()]
//        public void ModifyQueryString_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual("", coreCommonModule.ModifyQueryString("", "a", "1", false));
//            Assert.AreEqual("a=1", coreCommonModule.ModifyQueryString("", "a", "1", true));
//            Assert.AreEqual("a=1", coreCommonModule.ModifyQueryString("a=0", "a", "1", false));
//            Assert.AreEqual("a=1", coreCommonModule.ModifyQueryString("a=0", "a", "1", true));
//            Assert.AreEqual("a=1&b=2", coreCommonModule.ModifyQueryString("a=1", "b", "2", true));
//        }
//        //
//        [Fact()]
//        public void ModifyLinkQuery_unit()
//        {
//            // arrange
//            // act
//            // assert
//            Assert.AreEqual("index.html", coreCommonModule.modifyLinkQuery("index.html", "a", "1", false));
//            Assert.AreEqual("index.html?a=1", coreCommonModule.modifyLinkQuery("index.html", "a", "1", true));
//            Assert.AreEqual("index.html?a=1", coreCommonModule.modifyLinkQuery("index.html?a=0", "a", "1", false));
//            Assert.AreEqual("index.html?a=1", coreCommonModule.modifyLinkQuery("index.html?a=0", "a", "1", true));
//            Assert.AreEqual("index.html?a=1&b=2", coreCommonModule.modifyLinkQuery("index.html?a=1", "b", "2", true));
//        }
//        //
//        [Fact()]
//        public void vbInstr_test()
//        {
//            //vbInstr(1, Link, "?")
//            Assert.AreEqual(Strings.InStr("abcdefgabcdefgabcdefgabcdefg", "d"), coreCommonModule.vbInstr("abcdefgabcdefgabcdefgabcdefg", "d"));
//            Assert.AreEqual(Strings.InStr("abcdefgabcdefgabcdefgabcdefg", "E"), coreCommonModule.vbInstr("abcdefgabcdefgabcdefgabcdefg", "E"));
//            Assert.AreEqual(Strings.InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E"), coreCommonModule.vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E"));
//            Assert.AreEqual(Strings.InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Binary), coreCommonModule.vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Binary));
//            Assert.AreEqual(Strings.InStr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Text), coreCommonModule.vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "E", CompareMethod.Text));
//            Assert.AreEqual(Strings.InStr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Binary), coreCommonModule.vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Binary));
//            Assert.AreEqual(Strings.InStr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Text), coreCommonModule.vbInstr(10, "abcdefgabcdefgabcdefgabcdefg", "c", CompareMethod.Text));
//            string haystack = "abcdefgabcdefgabcdefgabcdefg";
//            string needle = "c";
//            Assert.AreEqual(Strings.InStr(1, "?", "?"), coreCommonModule.vbInstr(1, "?", "?"));
//            for (int ptr = 1; ptr <= haystack.Length; ptr++)
//            {
//                Assert.AreEqual(Strings.InStr(ptr, haystack, needle, CompareMethod.Binary), coreCommonModule.vbInstr(ptr, haystack, needle, CompareMethod.Binary));
//            }
//        }
//        //
//        [Fact()]
//        public void vbIsNumeric_test()
//        {
//            Assert.AreEqual(Information.IsNumeric(0), coreCommonModule.vbIsNumeric(0));
//            Assert.AreEqual(Information.IsNumeric(new System.DateTime(2000, 1, 1)), coreCommonModule.vbIsNumeric(new System.DateTime(2000, 1, 1)));
//            Assert.AreEqual(Information.IsNumeric(1234), coreCommonModule.vbIsNumeric(1234));
//            Assert.AreEqual(Information.IsNumeric(12.34), coreCommonModule.vbIsNumeric(12.34));
//            Assert.AreEqual(Information.IsNumeric("abcd"), coreCommonModule.vbIsNumeric("abcd"));
//            Assert.AreEqual(Information.IsNumeric("1234"), coreCommonModule.vbIsNumeric("1234"));
//            Assert.AreEqual(Information.IsNumeric("12.34"), coreCommonModule.vbIsNumeric("12.34"));
//            Assert.AreEqual(Information.IsNumeric(null), coreCommonModule.vbIsNumeric(null));
//        }
//        //
//        [Fact()]
//        public void vbReplace_test()
//        {
//            string actual = null;
//            string expected = null;
//            int start = 1;
//            int count = 9999;
//            //
//            expected = Strings.Replace("abcdefg", "cd", "12345");
//            actual = coreCommonModule.vbReplace("abcdefg", "cd", "12345");
//            Assert.AreEqual(expected, actual);
//            //
//            expected = Strings.Replace("abcdefg", "cD", "12345");
//            actual = coreCommonModule.vbReplace("abcdefg", "cD", "12345");
//            Assert.AreEqual(expected, actual);
//            //
//            expected = Strings.Replace("abcdefg", "cd", "12345", start, count, CompareMethod.Binary);
//            actual = coreCommonModule.vbReplace("abcdefg", "cd", "12345", start, count, CompareMethod.Binary);
//            Assert.AreEqual(expected, actual);
//            //
//            expected = Strings.Replace("abcdefg", "cD", "12345", start, count, CompareMethod.Binary);
//            actual = coreCommonModule.vbReplace("abcdefg", "cD", "12345", start, count, CompareMethod.Binary);
//            Assert.AreEqual(expected, actual);
//            //
//            expected = Strings.Replace("abcdefg", "cd", "12345", start, count, CompareMethod.Text);
//            actual = coreCommonModule.vbReplace("abcdefg", "cd", "12345", start, count, CompareMethod.Text);
//            Assert.AreEqual(expected, actual);
//            //
//            expected = Strings.Replace("abcdefg", "cD", "12345", start, count, CompareMethod.Text);
//            actual = coreCommonModule.vbReplace("abcdefg", "cD", "12345", start, count, CompareMethod.Text);
//            Assert.AreEqual(expected, actual);
//        }
//        //
//        [Fact()]
//        public void vbUCase_test()
//        {
//            Assert.AreEqual(Strings.UCase("AbCdEfG"), coreCommonModule.vbUCase("AbCdEfG"));
//            Assert.AreEqual(Strings.UCase("ABCDEFG"), coreCommonModule.vbUCase("ABCDEFG"));
//            Assert.AreEqual(Strings.UCase("abcdefg"), coreCommonModule.vbUCase("abcdefg"));
//        }
//        //
//        [Fact()]
//        public void vbLCase_test()
//        {
//            Assert.AreEqual(Strings.LCase("AbCdEfG"), coreCommonModule.vbLCase("AbCdEfG"));
//            Assert.AreEqual(Strings.LCase("ABCDEFG"), coreCommonModule.vbLCase("ABCDEFG"));
//            Assert.AreEqual(Strings.LCase("abcdefg"), coreCommonModule.vbLCase("abcdefg"));
//        }
//        //
//        [Fact()]
//        public void vbLeft_test()
//        {
//            Assert.AreEqual(Strings.LCase("AbCdEfG"), coreCommonModule.vbLCase("AbCdEfG"));
//        }

//    }
//}

////=======================================================
////Service provided by Telerik (www.telerik.com)
////Conversion powered by NRefactory.
////Twitter: @telerik
////Facebook: facebook.com/telerik
////=======================================================
