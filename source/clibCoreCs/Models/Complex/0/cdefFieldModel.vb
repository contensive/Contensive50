
Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Complex
    '
    ' ---------------------------------------------------------------------------------------------------
    ' ----- CDefFieldClass
    '       class not structure because it has to marshall to vb6
    ' ---------------------------------------------------------------------------------------------------
    '
    <Serializable>
    Public Class CDefFieldModel
        Implements ICloneable
        Implements IComparable
        Public Property nameLc As String                      ' The name of the field
        Public Property id As Integer                          ' the ID in the ccContentFields Table that this came from
        Public Property active As Boolean                   ' if the field is available in the admin area
        Public Property fieldTypeId As Integer                   ' The type of data the field holds
        Public Property caption As String                   ' The caption for displaying the field
        Public Property [ReadOnly] As Boolean            ' was ReadOnly -- If true, this field can not be written back to the database
        Public Property NotEditable As Boolean              ' if true, you can only edit new records
        Public Property Required As Boolean                 ' if true, this field must be entered
        Public Property defaultValue As String         ' default value on a new record
        Public Property HelpMessage As String               ' explaination of this field
        Public Property UniqueName As Boolean               '
        Public Property TextBuffered As Boolean             ' if true, the input is run through RemoveControlCharacters()
        Public Property Password As Boolean                 ' for text boxes, sets the password attribute
        Public Property RedirectID As String                ' If TYPEREDIRECT, this is the field that must match ID of this record
        Public Property RedirectPath As String              ' New Field, If TYPEREDIRECT, this is the path to the next page (if blank, current page is used)
        Public Property indexColumn As Integer                 ' the column desired in the admin index form
        Public Property indexWidth As String                ' either number or percentage, blank if not included 
        Public Property indexSortOrder As Integer            ' alpha sort on index page
        Public Property indexSortDirection As Integer          ' 1 sorts forward, -1 backward
        Public Property adminOnly As Boolean                ' This field is only available to administrators
        Public Property developerOnly As Boolean            ' This field is only available to administrators
        Public Property blockAccess As Boolean              ' ***** Field Reused to keep binary compatiblity - "IsBaseField" - if true this is a CDefBase field
        Public Property htmlContent As Boolean              ' if true, the HTML editor (active edit) can be used
        Public Property authorable As Boolean               ' true if it can be seen in the admin form
        Public Property inherited As Boolean                ' if true, this field takes its values from a parent, see ContentID
        Public Property contentId As Integer                   ' This is the ID of the Content Def that defines these properties
        Public Property editSortPriority As Integer            ' The Admin Edit Sort Order
        Public Property ManyToManyRulePrimaryField As String     ' Rule Field Name for Primary Table
        Public Property ManyToManyRuleSecondaryField As String   ' Rule Field Name for Secondary Table
        Public Property RSSTitleField As Boolean             ' When creating RSS fields from this content, this is the title
        Public Property RSSDescriptionField As Boolean       ' When creating RSS fields from this content, this is the description
        Public Property editTabName As String                   ' Editing group - used for the tabs
        Public Property Scramble As Boolean                 ' save the field scrambled in the Db
        Public Property lookupList As String                ' If TYPELOOKUP, and LookupContentID is null, this is a comma separated list of choices
        Public Property dataChanged As Boolean
        Public Property isBaseField As Boolean
        Public Property isModifiedSinceInstalled As Boolean
        Public Property installedByCollectionGuid As String
        Public Property HelpDefault As String
        Public Property HelpCustom As String
        Public Property HelpChanged As Boolean
        '
        ' fields stored differently in xml collection files
        '   name is loaded from xml collection files 
        '   id is created during the cacheLoad process when loading from Db (and used in metaData)
        '
        Public Property lookupContentID As Integer             ' If TYPELOOKUP, (for Db controled sites) this is the content ID of the source table
        Public Property RedirectContentID As Integer           ' If TYPEREDIRECT, this is new contentID
        Public Property manyToManyContentID As Integer         ' Content containing Secondary Records
        Public Property manyToManyRuleContentID As Integer     ' Content with rules between Primary and Secondary
        Public Property MemberSelectGroupID As Integer
        '
        '====================================================================================================
        '
        Public Property RedirectContentName(cpCore As coreClass) As String
            Get
                If (_RedirectContentName Is Nothing) Then
                    If RedirectContentID > 0 Then
                        _RedirectContentName = ""
                        Dim dt As DataTable = cpCore.db.executeQuery("select name from cccontent where id=" & RedirectContentID.ToString())
                        If dt.Rows.Count > 0 Then
                            _RedirectContentName = genericController.encodeText(dt.Rows(0).Item(0))
                        End If
                    End If
                End If
                Return _RedirectContentName
            End Get
            Set(value As String)
                _RedirectContentName = value
            End Set
        End Property
        Private _RedirectContentName As String = Nothing
        '
        '====================================================================================================
        '
        Public Property MemberSelectGroupName(cpCore As coreClass) As String
            Get
                If (_MemberSelectGroupName Is Nothing) Then
                    If MemberSelectGroupID > 0 Then
                        _MemberSelectGroupName = ""
                        Dim dt As DataTable = cpCore.db.executeQuery("select name from cccontent where id=" & MemberSelectGroupID.ToString())
                        If dt.Rows.Count > 0 Then
                            _MemberSelectGroupName = genericController.encodeText(dt.Rows(0).Item(0))
                        End If
                    End If
                End If
                Return _MemberSelectGroupName
            End Get
            Set(value As String)
                _MemberSelectGroupName = value
            End Set
        End Property
        Private _MemberSelectGroupName As String = Nothing
        '
        '====================================================================================================
        '
        Public Property ManyToManyContentName(cpCore As coreClass) As String
            Get
                If (_ManyToManyRuleContentName Is Nothing) Then
                    If manyToManyContentID > 0 Then
                        _ManyToManyRuleContentName = ""
                        Dim dt As DataTable = cpCore.db.executeQuery("select name from cccontent where id=" & manyToManyContentID.ToString())
                        If dt.Rows.Count > 0 Then
                            _ManyToManyContentName = genericController.encodeText(dt.Rows(0).Item(0))
                        End If
                    End If
                End If
                Return _ManyToManyContentName
            End Get
            Set(value As String)
                _ManyToManyContentName = value
            End Set
        End Property
        Private _ManyToManyContentName As String = Nothing
        '
        '====================================================================================================
        '
        Public Property ManyToManyRuleContentName(cpCore As coreClass) As String
            Get
                If (_ManyToManyRuleContentName Is Nothing) Then
                    If manyToManyRuleContentID > 0 Then
                        _ManyToManyRuleContentName = ""
                        Dim dt As DataTable = cpCore.db.executeQuery("select name from cccontent where id=" & manyToManyRuleContentID.ToString())
                        If dt.Rows.Count > 0 Then
                            _ManyToManyRuleContentName = genericController.encodeText(dt.Rows(0).Item(0))
                        End If
                    End If
                End If
                Return _ManyToManyRuleContentName
            End Get
            Set(value As String)
                _ManyToManyRuleContentName = value
            End Set
        End Property
        Private _ManyToManyRuleContentName As String = Nothing
        '
        '====================================================================================================
        '
        Public Property lookupContentName(cpCore As coreClass) As String
            Get
                If (_lookupContentName Is Nothing) Then
                    If lookupContentID > 0 Then
                        _lookupContentName = ""
                        Dim dt As DataTable = cpCore.db.executeQuery("select name from cccontent where id=" & lookupContentID.ToString())
                        If dt.Rows.Count > 0 Then
                            _lookupContentName = genericController.encodeText(dt.Rows(0).Item(0))
                        End If
                    End If
                End If
                Return _lookupContentName
            End Get
            Set(value As String)
                _lookupContentName = value
            End Set
        End Property
        Private _lookupContentName As String = Nothing
        '
        '====================================================================================================
        '
        Public Function Clone() As Object Implements ICloneable.Clone
            Return Me.MemberwiseClone
        End Function
        '
        '====================================================================================================
        '
        Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
            Dim c As Models.Complex.CDefFieldModel = CType(obj, Models.Complex.CDefFieldModel)
            Return String.Compare(Me.nameLc.ToLower, c.nameLc.ToLower)
        End Function
    End Class
End Namespace
