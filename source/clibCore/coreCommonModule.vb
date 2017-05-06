
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
    End Module

End Namespace
