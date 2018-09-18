
Option Explicit On
Option Strict On

Imports System.Xml
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports Contensive.Core.Models
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' install addon collections
    ''' </summary>
    Public Class addonInstallClass
        '
        '==========================================================================================================================
        '   Overlay a Src CDef on to the current one (Dst)
        '       Any Src CDEf entries found in Src are added to Dst.
        '       if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
        '
        '       isBaseContent
        '           if dst not found, it is created to match src
        '           if dst found, it is updated only if isBase matches
        '               content attributes updated if .isBaseContent matches
        '               field attributes updated if .isBaseField matches
        '==========================================================================================================================
        '
        Private Shared Function installCollection_AddMiniCollectionSrcToDst(cpCore As coreClass, ByRef dstCollection As miniCollectionModel, ByVal srcCollection As miniCollectionModel, ByVal SrcIsUserCDef As Boolean) As Boolean
            Dim returnOk As Boolean = True
            Try
                Dim HelpSrc As String
                Dim HelpCustomChanged As Boolean
                Dim HelpDefaultChanged As Boolean
                Dim HelpChanged As Boolean
                Dim Copy As String
                Dim n As String
                Dim srcCollectionCdefField As Models.Complex.CDefFieldModel
                Dim dstCollectionCdef As Models.Complex.cdefModel
                Dim dstCollectionCdefField As Models.Complex.CDefFieldModel
                Dim IsMatch As Boolean
                Dim DstKey As String
                Dim SrcKey As String
                Dim DataBuildVersion As String
                Dim SrcIsNavigator As Boolean
                Dim DstIsNavigator As Boolean
                Dim SrcContentName As String
                Dim DstName As String
                Dim SrcFieldName As String
                Dim okToUpdateDstFromSrc As Boolean
                Dim srcCollectionCdef As Models.Complex.cdefModel
                Dim DebugSrcFound As Boolean
                Dim DebugDstFound As Boolean
                '
                ' If the Src is the BaseCollection, the Dst must be the Application Collectio
                '   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                '
                If srcCollection.isBaseCollection Then
                    For Each dstKeyValuePair In dstCollection.CDef
                        Dim dstWorkingCdef As Models.Complex.cdefModel = dstKeyValuePair.Value
                        Dim contentName As String
                        contentName = dstWorkingCdef.Name
                        If dstCollection.CDef(contentName.ToLower).IsBaseContent Then
                            '
                            ' this application collection Cdef is marked base, verify it is in the base collection
                            '
                            If Not srcCollection.CDef.ContainsKey(contentName.ToLower) Then
                                '
                                ' cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                '
                                With dstCollection.CDef(contentName.ToLower)
                                    .IsBaseContent = False
                                    .dataChanged = True
                                    For Each dstFieldKeyValuePair In .fields
                                        Dim field As Models.Complex.CDefFieldModel = dstFieldKeyValuePair.Value
                                        If field.isBaseField Then
                                            field.isBaseField = False
                                            'field.Changed = True
                                        End If
                                    Next
                                End With
                            End If
                        End If
                    Next
                End If
                '
                '
                ' -------------------------------------------------------------------------------------------------
                ' Go through all CollectionSrc and find the CollectionDst match
                '   if it is an exact match, do nothing
                '   if the cdef does not match, set cdefext(ptr).CDefChanged true
                '   if any field does not match, set cdefext...field...CDefChanged
                '   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                ' -------------------------------------------------------------------------------------------------
                '
                Call logController.appendInstallLog(cpCore, "Application: " & cpCore.serverConfig.appConfig.name & ", UpgradeCDef_AddSrcToDst")
                '
                For Each srcKeyValuePair In srcCollection.CDef
                    srcCollectionCdef = srcKeyValuePair.Value

                    SrcContentName = srcCollectionCdef.Name
                    'If genericController.vbLCase(SrcContentName) = "site sections" Then
                    '    SrcContentName = SrcContentName
                    'End If
                    DebugSrcFound = False
                    If genericController.vbInstr(1, SrcContentName, cnNavigatorEntries, vbTextCompare) <> 0 Then
                        DebugSrcFound = True
                    End If
                    '
                    ' Search for this cdef in the Dst
                    '
                    okToUpdateDstFromSrc = False
                    If Not dstCollection.CDef.ContainsKey(SrcContentName.ToLower) Then
                        '
                        ' add src to dst
                        '
                        dstCollection.CDef.Add(SrcContentName.ToLower, New Models.Complex.cdefModel)
                        okToUpdateDstFromSrc = True
                    Else
                        dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower)
                        DstName = SrcContentName
                        '
                        ' found a match between Src and Dst
                        '
                        If dstCollectionCdef.IsBaseContent = srcCollectionCdef.IsBaseContent Then
                            '
                            ' Allow changes to user cdef only from user cdef, changes to base only from base
                            '
                            With dstCollectionCdef
                                n = "ActiveOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.ActiveOnly <> srcCollectionCdef.ActiveOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AdminOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AdminOnly <> srcCollectionCdef.AdminOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "DeveloperOnly"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.DeveloperOnly <> srcCollectionCdef.DeveloperOnly)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowAdd"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowAdd <> srcCollectionCdef.AllowAdd)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowCalendarEvents"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowCalendarEvents <> srcCollectionCdef.AllowCalendarEvents)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowContentTracking"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowContentTracking <> srcCollectionCdef.AllowContentTracking)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowDelete"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowDelete <> srcCollectionCdef.AllowDelete)
                                '
                                'If Not okToUpdateDstFromSrc Then n = "AllowMetaContent"
                                'okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowMetaContent <> srcCollectionCdef.AllowMetaContent)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowTopicRules"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowTopicRules <> srcCollectionCdef.AllowTopicRules)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ContentDataSourceName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentDataSourceName, srcCollectionCdef.ContentDataSourceName)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ContentTableName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentTableName, srcCollectionCdef.ContentTableName)
                                '
                                If DebugDstFound Then
                                    DebugDstFound = DebugDstFound
                                End If
                                If Not okToUpdateDstFromSrc Then n = "DefaultSortMethod"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .DefaultSortMethod, srcCollectionCdef.DefaultSortMethod)
                                '
                                If Not okToUpdateDstFromSrc Then n = "DropDownFieldList"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .DropDownFieldList, srcCollectionCdef.DropDownFieldList)
                                '
                                If Not okToUpdateDstFromSrc Then n = "EditorGroupName"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .EditorGroupName, srcCollectionCdef.EditorGroupName)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IgnoreContentControl"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IgnoreContentControl <> srcCollectionCdef.IgnoreContentControl)
                                If okToUpdateDstFromSrc Then
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                End If
                                '
                                If Not okToUpdateDstFromSrc Then n = "Active"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Active <> srcCollectionCdef.Active)
                                '
                                If Not okToUpdateDstFromSrc Then n = "AllowContentChildTool"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowContentChildTool <> srcCollectionCdef.AllowContentChildTool)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ParentId"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.parentID <> srcCollectionCdef.parentID)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconLink"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .IconLink, srcCollectionCdef.IconLink)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconHeight"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconHeight <> srcCollectionCdef.IconHeight)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconWidth"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconWidth <> srcCollectionCdef.IconWidth)
                                '
                                If Not okToUpdateDstFromSrc Then n = "IconSprites"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IconSprites <> srcCollectionCdef.IconSprites)
                                '
                                If Not okToUpdateDstFromSrc Then n = "installedByCollectionGuid"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .installedByCollectionGuid, srcCollectionCdef.installedByCollectionGuid)
                                '
                                If Not okToUpdateDstFromSrc Then n = "ccGuid"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .guid, srcCollectionCdef.guid)
                                '
                                ' IsBaseContent
                                '   if Dst IsBase, and Src is not, this change will be blocked following the changes anyway
                                '   if Src IsBase, and Dst is not, Dst should be changed, and IsBaseContent can be treated like any other field
                                '
                                If Not okToUpdateDstFromSrc Then n = "IsBaseContent"
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.IsBaseContent <> srcCollectionCdef.IsBaseContent)
                                If okToUpdateDstFromSrc Then
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                End If
                            End With
                            If okToUpdateDstFromSrc Then
                                If dstCollectionCdef.IsBaseContent And Not srcCollectionCdef.IsBaseContent Then
                                    '
                                    ' Dst is a base CDef, Src is not. This update is not allowed. Log it and skip the Add
                                    '
                                    Copy = "An attempt was made to update a Base Content Definition [" & DstName & "] from a collection that is not the Base Collection. This is not allowed."
                                    Call logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, " & Copy)
                                    Throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, Copy, "dll", "builderClass", "UpgradeCDef_AddSrcToDst", 0, "", "", False, True, "")
                                    okToUpdateDstFromSrc = False
                                Else
                                    '
                                    ' Just log the change for tracking
                                    '
                                    Call logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, (Logging only) While merging two collections (probably application and an upgrade), one or more attributes for a content definition or field were different, first change was CDef=" & SrcContentName & ", field=" & n)
                                End If
                            End If
                        End If
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.CDef(SrcContentName.ToLower)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .Active = srcCollectionCdef.Active
                            .ActiveOnly = srcCollectionCdef.ActiveOnly
                            .AdminOnly = srcCollectionCdef.AdminOnly
                            '.adminColumns = srcCollectionCdef.adminColumns
                            .AliasID = srcCollectionCdef.AliasID
                            .AliasName = srcCollectionCdef.AliasName
                            .AllowAdd = srcCollectionCdef.AllowAdd
                            .AllowCalendarEvents = srcCollectionCdef.AllowCalendarEvents
                            .AllowContentChildTool = srcCollectionCdef.AllowContentChildTool
                            .AllowContentTracking = srcCollectionCdef.AllowContentTracking
                            .AllowDelete = srcCollectionCdef.AllowDelete
                            .AllowTopicRules = srcCollectionCdef.AllowTopicRules
                            .guid = srcCollectionCdef.guid
                            .dataChanged = True
                            .ContentControlCriteria = srcCollectionCdef.ContentControlCriteria
                            .ContentDataSourceName = srcCollectionCdef.ContentDataSourceName
                            .ContentTableName = srcCollectionCdef.ContentTableName
                            .dataSourceId = srcCollectionCdef.dataSourceId
                            .DefaultSortMethod = srcCollectionCdef.DefaultSortMethod
                            .DeveloperOnly = srcCollectionCdef.DeveloperOnly
                            .DropDownFieldList = srcCollectionCdef.DropDownFieldList
                            .EditorGroupName = srcCollectionCdef.EditorGroupName
                            '.fields
                            .IconHeight = srcCollectionCdef.IconHeight
                            .IconLink = srcCollectionCdef.IconLink
                            .IconSprites = srcCollectionCdef.IconSprites
                            .IconWidth = srcCollectionCdef.IconWidth
                            '.Id
                            .IgnoreContentControl = srcCollectionCdef.IgnoreContentControl
                            .includesAFieldChange = True
                            .installedByCollectionGuid = srcCollectionCdef.installedByCollectionGuid
                            .IsBaseContent = srcCollectionCdef.IsBaseContent
                            .IsModifiedSinceInstalled = srcCollectionCdef.IsModifiedSinceInstalled
                            .Name = srcCollectionCdef.Name
                            .parentID = srcCollectionCdef.parentID
                            .parentName = srcCollectionCdef.parentName
                            .SelectCommaList = srcCollectionCdef.SelectCommaList
                            '.selectList
                            '.TimeStamp
                            .WhereClause = srcCollectionCdef.WhereClause
                        End With
                    End If
                    '
                    ' Now check each of the field records for an addition, or a change
                    ' DstPtr is still set to the Dst CDef
                    '
                    'Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef(SrcPtr).fields.count=" & CollectionSrc.CDef(SrcPtr).fields.count)
                    With srcCollectionCdef
                        For Each srcFieldKeyValuePair In .fields
                            srcCollectionCdefField = srcFieldKeyValuePair.Value
                            SrcFieldName = srcCollectionCdefField.nameLc
                            okToUpdateDstFromSrc = False
                            If Not dstCollection.CDef.ContainsKey(SrcContentName.ToLower) Then
                                '
                                ' should have been the collection
                                '
                                Throw (New ApplicationException("ERROR - cannot update destination content because it was not found after being added."))
                            Else
                                dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower)
                                If dstCollectionCdef.fields.ContainsKey(SrcFieldName.ToLower) Then
                                    '
                                    ' Src field was found in Dst fields
                                    '

                                    dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower)
                                    okToUpdateDstFromSrc = False
                                    If dstCollectionCdefField.isBaseField = srcCollectionCdefField.isBaseField Then
                                        With srcCollectionCdefField
                                            n = "Active"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.active <> dstCollectionCdefField.active)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "AdminOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.adminOnly <> dstCollectionCdefField.adminOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Authorable"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.authorable <> dstCollectionCdefField.authorable)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Caption"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .caption, dstCollectionCdefField.caption)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.contentId <> dstCollectionCdefField.contentId)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "DeveloperOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.developerOnly <> dstCollectionCdefField.developerOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "EditSortPriority"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.editSortPriority <> dstCollectionCdefField.editSortPriority)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "EditTab"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .editTabName, dstCollectionCdefField.editTabName)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "FieldType"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.fieldTypeId <> dstCollectionCdefField.fieldTypeId)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "HTMLContent"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.htmlContent <> dstCollectionCdefField.htmlContent)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexColumn"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.indexColumn <> dstCollectionCdefField.indexColumn)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexSortDirection"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.indexSortDirection <> dstCollectionCdefField.indexSortDirection)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexSortOrder"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (EncodeInteger(.indexSortOrder) <> genericController.EncodeInteger(dstCollectionCdefField.indexSortOrder))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IndexWidth"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .indexWidth, dstCollectionCdefField.indexWidth)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.lookupContentID <> dstCollectionCdefField.lookupContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupList"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .lookupList, dstCollectionCdefField.lookupList)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.manyToManyContentID <> dstCollectionCdefField.manyToManyContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.manyToManyRuleContentID <> dstCollectionCdefField.manyToManyRuleContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRulePrimaryField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRulePrimaryField, dstCollectionCdefField.ManyToManyRulePrimaryField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleSecondaryField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRuleSecondaryField, dstCollectionCdefField.ManyToManyRuleSecondaryField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "MemberSelectGroupID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.MemberSelectGroupID <> dstCollectionCdefField.MemberSelectGroupID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "NotEditable"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.NotEditable <> dstCollectionCdefField.NotEditable)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Password"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Password <> dstCollectionCdefField.Password)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ReadOnly"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.ReadOnly <> dstCollectionCdefField.ReadOnly)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectContentID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RedirectContentID <> dstCollectionCdefField.RedirectContentID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectID"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectID, dstCollectionCdefField.RedirectID)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectPath"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectPath, dstCollectionCdefField.RedirectPath)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Required"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Required <> dstCollectionCdefField.Required)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RSSDescriptionField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RSSDescriptionField <> dstCollectionCdefField.RSSDescriptionField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RSSTitleField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.RSSTitleField <> dstCollectionCdefField.RSSTitleField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "Scramble"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Scramble <> dstCollectionCdefField.Scramble)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "TextBuffered"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.TextBuffered <> dstCollectionCdefField.TextBuffered)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "DefaultValue"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (genericController.encodeText(.defaultValue) <> genericController.encodeText(dstCollectionCdefField.defaultValue))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "UniqueName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.UniqueName <> dstCollectionCdefField.UniqueName)
                                            If okToUpdateDstFromSrc Then
                                                okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                            End If
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "IsBaseField"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.isBaseField <> dstCollectionCdefField.isBaseField)
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "LookupContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .lookupContentName(cpCore), dstCollectionCdefField.lookupContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyContentName(cpCore), dstCollectionCdefField.ManyToManyContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "ManyToManyRuleContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ManyToManyRuleContentName(cpCore), dstCollectionCdefField.ManyToManyRuleContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "RedirectContentName"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .RedirectContentName(cpCore), dstCollectionCdefField.RedirectContentName(cpCore))
                                            '
                                            If Not okToUpdateDstFromSrc Then n = "installedByCollectionid"
                                            okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .installedByCollectionGuid, dstCollectionCdefField.installedByCollectionGuid)
                                            '
                                            If okToUpdateDstFromSrc Then
                                                okToUpdateDstFromSrc = okToUpdateDstFromSrc
                                            End If
                                        End With
                                    End If
                                    '
                                    ' Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
                                    '
                                    HelpSrc = srcCollectionCdefField.HelpCustom
                                    HelpCustomChanged = Not TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpCustom)
                                    '
                                    HelpSrc = srcCollectionCdefField.HelpDefault
                                    HelpDefaultChanged = Not TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpDefault)
                                    '
                                    HelpChanged = HelpDefaultChanged Or HelpCustomChanged
                                Else
                                    '
                                    ' field was not found in dst, add it and populate
                                    '
                                    dstCollectionCdef.fields.Add(SrcFieldName.ToLower, New Models.Complex.CDefFieldModel)
                                    dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower)
                                    okToUpdateDstFromSrc = True
                                    HelpChanged = True
                                End If
                                '
                                ' If okToUpdateDstFromSrc, update the Dst record with the Src record
                                '
                                If okToUpdateDstFromSrc Then
                                    '
                                    ' Update Fields
                                    '
                                    With dstCollectionCdefField
                                        .active = srcCollectionCdefField.active
                                        .adminOnly = srcCollectionCdefField.adminOnly
                                        .authorable = srcCollectionCdefField.authorable
                                        .caption = srcCollectionCdefField.caption
                                        .contentId = srcCollectionCdefField.contentId
                                        .defaultValue = srcCollectionCdefField.defaultValue
                                        .developerOnly = srcCollectionCdefField.developerOnly
                                        .editSortPriority = srcCollectionCdefField.editSortPriority
                                        .editTabName = srcCollectionCdefField.editTabName
                                        .fieldTypeId = srcCollectionCdefField.fieldTypeId
                                        .htmlContent = srcCollectionCdefField.htmlContent
                                        .indexColumn = srcCollectionCdefField.indexColumn
                                        .indexSortDirection = srcCollectionCdefField.indexSortDirection
                                        .indexSortOrder = srcCollectionCdefField.indexSortOrder
                                        .indexWidth = srcCollectionCdefField.indexWidth
                                        .lookupContentID = srcCollectionCdefField.lookupContentID
                                        .lookupList = srcCollectionCdefField.lookupList
                                        .manyToManyContentID = srcCollectionCdefField.manyToManyContentID
                                        .manyToManyRuleContentID = srcCollectionCdefField.manyToManyRuleContentID
                                        .ManyToManyRulePrimaryField = srcCollectionCdefField.ManyToManyRulePrimaryField
                                        .ManyToManyRuleSecondaryField = srcCollectionCdefField.ManyToManyRuleSecondaryField
                                        .MemberSelectGroupID = srcCollectionCdefField.MemberSelectGroupID
                                        .nameLc = srcCollectionCdefField.nameLc
                                        .NotEditable = srcCollectionCdefField.NotEditable
                                        .Password = srcCollectionCdefField.Password
                                        .ReadOnly = srcCollectionCdefField.ReadOnly
                                        .RedirectContentID = srcCollectionCdefField.RedirectContentID
                                        .RedirectID = srcCollectionCdefField.RedirectID
                                        .RedirectPath = srcCollectionCdefField.RedirectPath
                                        .Required = srcCollectionCdefField.Required
                                        .RSSDescriptionField = srcCollectionCdefField.RSSDescriptionField
                                        .RSSTitleField = srcCollectionCdefField.RSSTitleField
                                        .Scramble = srcCollectionCdefField.Scramble
                                        .TextBuffered = srcCollectionCdefField.TextBuffered
                                        .UniqueName = srcCollectionCdefField.UniqueName
                                        .isBaseField = srcCollectionCdefField.isBaseField
                                        .lookupContentName(cpCore) = srcCollectionCdefField.lookupContentName(cpCore)
                                        .ManyToManyContentName(cpCore) = srcCollectionCdefField.ManyToManyContentName(cpCore)
                                        .ManyToManyRuleContentName(cpCore) = srcCollectionCdefField.ManyToManyRuleContentName(cpCore)
                                        .RedirectContentName(cpCore) = srcCollectionCdefField.RedirectContentName(cpCore)
                                        .installedByCollectionGuid = srcCollectionCdefField.installedByCollectionGuid
                                        .dataChanged = True
                                        If HelpChanged Then
                                            .HelpCustom = srcCollectionCdefField.HelpCustom
                                            .HelpDefault = srcCollectionCdefField.HelpDefault
                                            .HelpChanged = True
                                        End If
                                    End With
                                    dstCollectionCdef.includesAFieldChange = True
                                End If
                                '
                            End If
                        Next
                    End With
                Next
                '
                ' -------------------------------------------------------------------------------------------------
                ' Check SQL Indexes
                ' -------------------------------------------------------------------------------------------------
                '
                Dim dstSqlIndexPtr As Integer
                Dim SrcsSqlIndexPtr As Integer
                For SrcsSqlIndexPtr = 0 To srcCollection.SQLIndexCnt - 1
                    SrcContentName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName & "-" & srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName & "-" & srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName
                    okToUpdateDstFromSrc = False
                    '
                    ' Search for this name in the Dst
                    '
                    For dstSqlIndexPtr = 0 To dstCollection.SQLIndexCnt - 1
                        DstName = dstCollection.SQLIndexes(dstSqlIndexPtr).DataSourceName & "-" & dstCollection.SQLIndexes(dstSqlIndexPtr).TableName & "-" & dstCollection.SQLIndexes(dstSqlIndexPtr).IndexName
                        If TextMatch(cpCore, DstName, SrcContentName) Then
                            '
                            ' found a match between Src and Dst
                            '
                            With dstCollection.SQLIndexes(dstSqlIndexPtr)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .FieldNameList, srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList)
                            End With
                            Exit For
                        End If
                    Next
                    If dstSqlIndexPtr = dstCollection.SQLIndexCnt Then
                        '
                        ' CDef was not found, add it
                        '
                        ReDim Preserve dstCollection.SQLIndexes(dstCollection.SQLIndexCnt)
                        dstCollection.SQLIndexCnt = dstSqlIndexPtr + 1
                        okToUpdateDstFromSrc = True
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.SQLIndexes(dstSqlIndexPtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .DataSourceName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName
                            .FieldNameList = srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList
                            .IndexName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName
                            .TableName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName
                        End With
                    End If
                Next
                '
                '-------------------------------------------------------------------------------------------------
                ' Check menus
                '-------------------------------------------------------------------------------------------------
                '
                Dim DstMenuPtr As Integer
                Dim SrcNameSpace As String
                Dim SrcParentName As String
                DataBuildVersion = cpCore.siteProperties.dataBuildVersion
                For SrcMenuPtr = 0 To srcCollection.MenuCnt - 1
                    DstMenuPtr = 0
                    SrcContentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Name)
                    SrcParentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).ParentName)
                    SrcNameSpace = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).menuNameSpace)
                    SrcIsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator
                    If SrcIsNavigator Then
                        If (SrcContentName = "manage add-ons") Then
                            SrcContentName = SrcContentName
                        End If
                    End If
                    okToUpdateDstFromSrc = False
                    '
                    SrcKey = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Key)
                    '
                    ' Search for match using guid
                    '
                    IsMatch = False
                    For DstMenuPtr = 0 To dstCollection.MenuCnt - 1
                        DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name)
                        If DstName = SrcContentName Then
                            DstName = DstName
                            DstIsNavigator = dstCollection.Menus(DstMenuPtr).IsNavigator
                            DstKey = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Key)
                            If genericController.vbLCase(DstName) = "settings" Then
                                DstName = DstName
                            End If
                            IsMatch = (DstKey = SrcKey) And (SrcIsNavigator = DstIsNavigator)
                            If IsMatch Then
                                Exit For
                            End If
                        End If
                    Next
                    If Not IsMatch Then
                        '
                        ' no match found on guid, try name and ( either namespace or parentname )
                        '
                        For DstMenuPtr = 0 To dstCollection.MenuCnt - 1
                            DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name)
                            If genericController.vbLCase(DstName) = "settings" Then
                                DstName = DstName
                            End If
                            If ((SrcContentName = DstName) And (SrcIsNavigator = DstIsNavigator)) Then
                                If SrcIsNavigator Then
                                    '
                                    ' Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    '
                                    IsMatch = (SrcNameSpace = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).menuNameSpace)) And (dstCollection.Menus(DstMenuPtr).Guid = "")
                                Else
                                    '
                                    ' AdminMenu - check parentname
                                    '
                                    IsMatch = (SrcParentName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).ParentName))
                                End If
                                If IsMatch Then
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                    If Not IsMatch Then
                        'If DstPtr = CollectionDst.MenuCnt Then
                        '
                        ' menu was not found, add it
                        '
                        ReDim Preserve dstCollection.Menus(dstCollection.MenuCnt)
                        dstCollection.MenuCnt = dstCollection.MenuCnt + 1
                        okToUpdateDstFromSrc = True
                        'End If
                    Else
                        'If IsMatch Then
                        '
                        ' found a match between Src and Dst
                        '
                        If SrcIsUserCDef Or SrcIsNavigator Then
                            '
                            ' Special case -- Navigators update from all upgrade sources so Base migrates changes
                            ' test for cdef attribute changes
                            '
                            With dstCollection.Menus(DstMenuPtr)
                                'With dstCollection.Menus(dstCollection.MenuCnt)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.Active <> srcCollection.Menus(SrcMenuPtr).Active)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AdminOnly <> srcCollection.Menus(SrcMenuPtr).AdminOnly)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .ContentName, srcCollection.Menus(SrcMenuPtr).ContentName)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.DeveloperOnly <> srcCollection.Menus(SrcMenuPtr).DeveloperOnly)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .LinkPage, srcCollection.Menus(SrcMenuPtr).LinkPage)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Name, srcCollection.Menus(SrcMenuPtr).Name)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.NewWindow <> srcCollection.Menus(SrcMenuPtr).NewWindow)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .SortOrder, srcCollection.Menus(SrcMenuPtr).SortOrder)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .AddonName, srcCollection.Menus(SrcMenuPtr).AddonName)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .NavIconType, srcCollection.Menus(SrcMenuPtr).NavIconType)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .NavIconTitle, srcCollection.Menus(SrcMenuPtr).NavIconTitle)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .menuNameSpace, srcCollection.Menus(SrcMenuPtr).menuNameSpace)
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Guid, srcCollection.Menus(SrcMenuPtr).Guid)
                                'okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ParentName, CollectionSrc.Menus(SrcPtr).ParentName)
                            End With
                        End If
                        'Exit For
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.Menus(DstMenuPtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .Guid = srcCollection.Menus(SrcMenuPtr).Guid
                            .Name = srcCollection.Menus(SrcMenuPtr).Name
                            .IsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator
                            .Active = srcCollection.Menus(SrcMenuPtr).Active
                            .AdminOnly = srcCollection.Menus(SrcMenuPtr).AdminOnly
                            .ContentName = srcCollection.Menus(SrcMenuPtr).ContentName
                            .DeveloperOnly = srcCollection.Menus(SrcMenuPtr).DeveloperOnly
                            .LinkPage = srcCollection.Menus(SrcMenuPtr).LinkPage
                            .NewWindow = srcCollection.Menus(SrcMenuPtr).NewWindow
                            .ParentName = srcCollection.Menus(SrcMenuPtr).ParentName
                            .menuNameSpace = srcCollection.Menus(SrcMenuPtr).menuNameSpace
                            .SortOrder = srcCollection.Menus(SrcMenuPtr).SortOrder
                            .AddonName = srcCollection.Menus(SrcMenuPtr).AddonName
                            .NavIconType = srcCollection.Menus(SrcMenuPtr).NavIconType
                            .NavIconTitle = srcCollection.Menus(SrcMenuPtr).NavIconTitle
                        End With
                    End If
                Next
                ''
                ''-------------------------------------------------------------------------------------------------
                '' Check addons -- yes, this should be done.
                ''-------------------------------------------------------------------------------------------------
                ''
                'If False Then
                '    '
                '    ' remove this for now -- later add ImportCollections to track the collections (not addons)
                '    '
                '    '
                '    '
                '    For SrcPtr = 0 To srcCollection.AddOnCnt - 1
                '        SrcContentName = genericController.vbLCase(srcCollection.AddOns(SrcPtr).Name)
                '        okToUpdateDstFromSrc = False
                '        '
                '        ' Search for this name in the Dst
                '        '
                '        For DstPtr = 0 To dstCollection.AddOnCnt - 1
                '            DstName = genericController.vbLCase(dstCollection.AddOns(DstPtr).Name)
                '            If DstName = SrcContentName Then
                '                '
                '                ' found a match between Src and Dst
                '                '
                '                If SrcIsUserCDef Then
                '                    '
                '                    ' test for cdef attribute changes
                '                    '
                '                    With dstCollection.AddOns(DstPtr)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ArgumentList, srcCollection.AddOns(SrcPtr).ArgumentList)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Copy, srcCollection.AddOns(SrcPtr).Copy)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Link, srcCollection.AddOns(SrcPtr).Link)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Name, srcCollection.AddOns(SrcPtr).Name)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ObjectProgramID, srcCollection.AddOns(SrcPtr).ObjectProgramID)
                '                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.SortOrder, srcCollection.AddOns(SrcPtr).SortOrder)
                '                    End With
                '                End If
                '                Exit For
                '            End If
                '        Next
                '        If DstPtr = dstCollection.AddOnCnt Then
                '            '
                '            ' CDef was not found, add it
                '            '
                '            ReDim Preserve dstCollection.AddOns(dstCollection.AddOnCnt)
                '            dstCollection.AddOnCnt = DstPtr + 1
                '            okToUpdateDstFromSrc = True
                '        End If
                '        If okToUpdateDstFromSrc Then
                '            With dstCollection.AddOns(DstPtr)
                '                '
                '                ' It okToUpdateDstFromSrc, update the Dst with the Src
                '                '
                '                .CDefChanged = True
                '                .ArgumentList = srcCollection.AddOns(SrcPtr).ArgumentList
                '                .Copy = srcCollection.AddOns(SrcPtr).Copy
                '                .Link = srcCollection.AddOns(SrcPtr).Link
                '                .Name = srcCollection.AddOns(SrcPtr).Name
                '                .ObjectProgramID = srcCollection.AddOns(SrcPtr).ObjectProgramID
                '                .SortOrder = srcCollection.AddOns(SrcPtr).SortOrder
                '            End With
                '        End If
                '    Next
                'End If
                '
                '-------------------------------------------------------------------------------------------------
                ' Check styles
                '-------------------------------------------------------------------------------------------------
                '
                Dim srcStylePtr As Integer
                Dim dstStylePtr As Integer
                For srcStylePtr = 0 To srcCollection.StyleCnt - 1
                    SrcContentName = genericController.vbLCase(srcCollection.Styles(srcStylePtr).Name)
                    okToUpdateDstFromSrc = False
                    '
                    ' Search for this name in the Dst
                    '
                    For dstStylePtr = 0 To dstCollection.StyleCnt - 1
                        DstName = genericController.vbLCase(dstCollection.Styles(dstStylePtr).Name)
                        If DstName = SrcContentName Then
                            '
                            ' found a match between Src and Dst
                            '
                            If SrcIsUserCDef Then
                                '
                                ' test for cdef attribute changes
                                '
                                With dstCollection.Styles(dstStylePtr)
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpCore, .Copy, srcCollection.Styles(srcStylePtr).Copy)
                                End With
                            End If
                            Exit For
                        End If
                    Next
                    If dstStylePtr = dstCollection.StyleCnt Then
                        '
                        ' CDef was not found, add it
                        '
                        ReDim Preserve dstCollection.Styles(dstCollection.StyleCnt)
                        dstCollection.StyleCnt = dstStylePtr + 1
                        okToUpdateDstFromSrc = True
                    End If
                    If okToUpdateDstFromSrc Then
                        With dstCollection.Styles(dstStylePtr)
                            '
                            ' It okToUpdateDstFromSrc, update the Dst with the Src
                            '
                            .dataChanged = True
                            .Copy = srcCollection.Styles(srcStylePtr).Copy
                            .Name = srcCollection.Styles(srcStylePtr).Name
                        End With
                    End If
                Next
                '
                '-------------------------------------------------------------------------------------------------
                ' Add Collections
                '-------------------------------------------------------------------------------------------------
                '
                Dim dstPtr As Integer = 0
                For SrcPtr = 0 To srcCollection.ImportCnt - 1
                    dstPtr = dstCollection.ImportCnt
                    ReDim Preserve dstCollection.collectionImports(dstPtr)
                    dstCollection.collectionImports(dstPtr) = srcCollection.collectionImports(SrcPtr)
                    dstCollection.ImportCnt = dstPtr + 1
                Next
                '
                '-------------------------------------------------------------------------------------------------
                ' Page Templates
                '-------------------------------------------------------------------------------------------------
                '
                '
                '-------------------------------------------------------------------------------------------------
                ' Site Sections
                '-------------------------------------------------------------------------------------------------
                '
                '
                '-------------------------------------------------------------------------------------------------
                ' Dynamic Menus
                '-------------------------------------------------------------------------------------------------
                '
                '
                '-------------------------------------------------------------------------------------------------
                ' Page Content
                '-------------------------------------------------------------------------------------------------
                '
                '
                '-------------------------------------------------------------------------------------------------
                ' Copy Content
                '-------------------------------------------------------------------------------------------------
                '
                '
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnOk
        End Function
        ''
        ''===========================================================================
        ''   Error handler
        ''===========================================================================
        ''
        'Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ResumeNext As Boolean = False)
        '    '
        '    'Call App.LogEvent("addonInstallClass.HandleClassTrapError called from " & MethodName)
        '    '
        '   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError3(ApplicationName, "unknown", "dll", "AddonInstallClass", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
        '    '
        'End Sub
        '
        '
        '
        Private Shared Function installCollection_GetApplicationMiniCollection(cpCore As coreClass, isNewBuild As Boolean) As miniCollectionModel
            Dim returnColl As New miniCollectionModel
            Try
                '
                Dim ExportFilename As String
                Dim ExportPathPage As String
                Dim CollectionData As String
                '
                If Not isNewBuild Then
                    '
                    ' if this is not an empty database, get the application collection, else return empty
                    '
                    ExportFilename = "cdef_export_" & CStr(genericController.GetRandomInteger()) & ".xml"
                    ExportPathPage = "tmp\" & ExportFilename
                    Call exportApplicationCDefXml(cpCore, ExportPathPage, True)
                    CollectionData = cpCore.privateFiles.readFile(ExportPathPage)
                    Call cpCore.privateFiles.deleteFile(ExportPathPage)
                    returnColl = installCollection_LoadXmlToMiniCollection(cpCore, CollectionData, False, False, isNewBuild, New miniCollectionModel)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnColl
        End Function
        '
        '========================================================================
        ' ----- Get an XML nodes attribute based on its name
        '========================================================================
        '
        Public Shared Function GetXMLAttribute(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As String
            Dim returnAttr As String = ""
            Try
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
                            returnAttr = NodeAttribute.Value
                            Found = True
                            Exit For
                        End If
                    Next
                    If Not Found Then
                        returnAttr = DefaultIfNotFound
                    End If
                Else
                    returnAttr = ResultNode.Value
                    Found = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnAttr
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeNumber(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As String) As Double
            GetXMLAttributeNumber = EncodeNumber(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeBoolean(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Boolean) As Boolean
            GetXMLAttributeBoolean = genericController.EncodeBoolean(GetXMLAttribute(cpCore, Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '========================================================================
        '
        '========================================================================
        '
        Private Shared Function GetXMLAttributeInteger(cpCore As coreClass, ByVal Found As Boolean, ByVal Node As XmlNode, ByVal Name As String, ByVal DefaultIfNotFound As Integer) As Integer
            GetXMLAttributeInteger = genericController.EncodeInteger(GetXMLAttribute(cpCore, Found, Node, Name, CStr(DefaultIfNotFound)))
        End Function
        '
        '==================================================================================================================
        '
        '==================================================================================================================
        '
        Private Shared Function TextMatch(cpCore As coreClass, ByVal Source1 As String, ByVal Source2 As String) As Boolean
            TextMatch = (LCase(Source1) = genericController.vbLCase(Source2))
        End Function
        '
        '
        '
        Private Shared Function GetMenuNameSpace(cpCore As coreClass, ByVal Collection As miniCollectionModel, ByVal MenuPtr As Integer, ByVal IsNavigator As Boolean, ByVal UsedIDList As String) As String
            Dim returnAttr As String = ""
            Try
                Dim ParentName As String
                Dim Ptr As Integer
                Dim Prefix As String
                Dim LCaseParentName As String

                '
                With Collection
                    ParentName = .Menus(MenuPtr).ParentName
                    If ParentName <> "" Then
                        LCaseParentName = genericController.vbLCase(ParentName)
                        For Ptr = 0 To .MenuCnt - 1
                            If genericController.vbInstr(1, "," & UsedIDList & ",", "," & CStr(Ptr) & ",") = 0 Then
                                If LCaseParentName = genericController.vbLCase(.Menus(Ptr).Name) And (IsNavigator = .Menus(Ptr).IsNavigator) Then
                                    Prefix = GetMenuNameSpace(cpCore, Collection, Ptr, IsNavigator, UsedIDList & "," & MenuPtr)
                                    If Prefix = "" Then
                                        returnAttr = ParentName
                                    Else
                                        returnAttr = Prefix & "." & ParentName
                                    End If
                                    Exit For
                                End If
                            End If
                        Next
                    End If
                End With
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnAttr
        End Function
        '
        '=============================================================================
        '   Create an entry in the Sort Methods Table
        '=============================================================================
        '
        Private Shared Sub VerifySortMethod(cpCore As coreClass, ByVal Name As String, ByVal OrderByCriteria As String)
            Try
                '
                Dim dt As DataTable
                Dim sqlList As New sqlFieldListClass
                '
                Call sqlList.add("name", cpCore.db.encodeSQLText(Name))
                Call sqlList.add("CreatedBy", "0")
                Call sqlList.add("OrderByClause", cpCore.db.encodeSQLText(OrderByCriteria))
                Call sqlList.add("active", SQLTrue)
                Call sqlList.add("ContentControlID", Models.Complex.cdefModel.getContentId(cpCore, "Sort Methods").ToString())
                '
                dt = cpCore.db.openTable("Default", "ccSortMethods", "Name=" & cpCore.db.encodeSQLText(Name), "ID", "ID", 1)
                If dt.Rows.Count > 0 Then
                    '
                    ' update sort method
                    '
                    Call cpCore.db.updateTableRecord("Default", "ccSortMethods", "ID=" & genericController.EncodeInteger(dt.Rows(0).Item("ID")).ToString, sqlList)
                Else
                    '
                    ' Create the new sort method
                    '
                    Call cpCore.db.insertTableRecord("Default", "ccSortMethods", sqlList)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=========================================================================================
        '
        '=========================================================================================
        '
        Public Shared Sub VerifySortMethods(cpCore As coreClass)
            Try
                '
                Call logController.appendInstallLog(cpCore, "Verify Sort Records")
                '
                Call VerifySortMethod(cpCore, "By Name", "Name")
                Call VerifySortMethod(cpCore, "By Alpha Sort Order Field", "SortOrder")
                Call VerifySortMethod(cpCore, "By Date", "DateAdded")
                Call VerifySortMethod(cpCore, "By Date Reverse", "DateAdded Desc")
                Call VerifySortMethod(cpCore, "By Alpha Sort Order Then Oldest First", "SortOrder,ID")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '   Get a ContentID from the ContentName using just the tables
        '=============================================================================
        '
        Private Shared Sub VerifyContentFieldTypes(cpCore As coreClass)
            Try
                '
                Dim RowsFound As Integer
                Dim CID As Integer
                Dim TableBad As Boolean
                Dim RowsNeeded As Integer
                '
                ' ----- make sure there are enough records
                '
                TableBad = False
                RowsFound = 0
                Using rs As DataTable = cpCore.db.executeQuery("Select ID from ccFieldTypes order by id")
                    If (Not isDataTableOk(rs)) Then
                        '
                        ' problem
                        '
                        TableBad = True
                    Else
                        '
                        ' Verify the records that are there
                        '
                        RowsFound = 0
                        For Each dr As DataRow In rs.Rows
                            RowsFound = RowsFound + 1
                            If RowsFound <> genericController.EncodeInteger(dr.Item("ID")) Then
                                '
                                ' Bad Table
                                '
                                TableBad = True
                                Exit For
                            End If
                        Next
                    End If

                End Using
                '
                ' ----- Replace table if needed
                '
                If TableBad Then
                    Call cpCore.db.deleteTable("Default", "ccFieldTypes")
                    Call cpCore.db.createSQLTable("Default", "ccFieldTypes")
                    RowsFound = 0
                End If
                '
                ' ----- Add the number of rows needed
                '
                RowsNeeded = FieldTypeIdMax - RowsFound
                If RowsNeeded > 0 Then
                    CID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Types")
                    If CID <= 0 Then
                        '
                        ' Problem
                        '
                        cpCore.handleException(New ApplicationException("Content Field Types content definition was not found"))
                    Else
                        Do While RowsNeeded > 0
                            Call cpCore.db.executeQuery("Insert into ccFieldTypes (active,contentcontrolid)values(1," & CID & ")")
                            RowsNeeded = RowsNeeded - 1
                        Loop
                    End If
                End If
                '
                ' ----- Update the Names of each row
                '
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Integer' where ID=1;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Text' where ID=2;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='LongText' where ID=3;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Boolean' where ID=4;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Date' where ID=5;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='File' where ID=6;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Lookup' where ID=7;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Redirect' where ID=8;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Currency' where ID=9;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='TextFile' where ID=10;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Image' where ID=11;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Float' where ID=12;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='AutoIncrement' where ID=13;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='ManyToMany' where ID=14;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Member Select' where ID=15;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='CSS File' where ID=16;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='XML File' where ID=17;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Javascript File' where ID=18;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Link' where ID=19;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Resource Link' where ID=20;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML' where ID=21;")
                Call cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML File' where ID=22;")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateScript(cpCore As coreClass, ByVal Name As String, ByVal Link As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateScript"
                '
                ContentName = "Aggregate Function Scripts"
                CSEntry = cpCore.db.csOpen(ContentName, "(name=" & cpCore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Link,ObjectProgramID,ArgumentList,SortOrder")
                '
                ' If no current entry, create one
                '
                If Not cpCore.db.csOk(CSEntry) Then
                    cpCore.db.csClose(CSEntry)
                    CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpCore.db.csOk(CSEntry) Then
                        Call cpCore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpCore.db.csOk(CSEntry) Then
                    Call cpCore.db.csSet(CSEntry, "Link", Link)
                    Call cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                    Call cpCore.db.csSet(CSEntry, "SortOrder", SortOrder)
                End If
                Call cpCore.db.csClose(CSEntry)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateReplacement(cpcore As coreClass, ByVal Name As String, ByVal Copy As String, ByVal SortOrder As String)
            Call csv_VerifyAggregateReplacement2(cpcore, Name, Copy, "", SortOrder)
        End Sub
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Shared Sub csv_VerifyAggregateReplacement2(cpCore As coreClass, ByVal Name As String, ByVal Copy As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateReplacement2"
                '
                ContentName = "Aggregate Function Replacements"
                CSEntry = cpCore.db.csOpen(ContentName, "(name=" & cpCore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Copy,SortOrder,ArgumentList")
                '
                ' If no current entry, create one
                '
                If Not cpCore.db.csOk(CSEntry) Then
                    cpCore.db.csClose(CSEntry)
                    CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpCore.db.csOk(CSEntry) Then
                        Call cpCore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpCore.db.csOk(CSEntry) Then
                    Call cpCore.db.csSet(CSEntry, "Copy", Copy)
                    Call cpCore.db.csSet(CSEntry, "SortOrder", SortOrder)
                    Call cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                End If
                Call cpCore.db.csClose(CSEntry)
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub        '
        '
        '=============================================================================
        '
        '=============================================================================
        '
        Public Sub csv_VerifyAggregateObject(cpcore As coreClass, ByVal Name As String, ByVal ObjectProgramID As String, ByVal ArgumentList As String, ByVal SortOrder As String)
            Try
                '
                Dim CSEntry As Integer
                Dim ContentName As String
                Dim MethodName As String
                '
                MethodName = "csv_VerifyAggregateObject"
                '
                ' Locate current entry
                '
                ContentName = "Aggregate Function Objects"
                CSEntry = cpcore.db.csOpen(ContentName, "(name=" & cpcore.db.encodeSQLText(Name) & ")", , False, , , , "Name,Link,ObjectProgramID,ArgumentList,SortOrder")
                '
                ' If no current entry, create one
                '
                If Not cpcore.db.csOk(CSEntry) Then
                    cpcore.db.csClose(CSEntry)
                    CSEntry = cpcore.db.csInsertRecord(ContentName, SystemMemberID)
                    If cpcore.db.csOk(CSEntry) Then
                        Call cpcore.db.csSet(CSEntry, "name", Name)
                    End If
                End If
                If cpcore.db.csOk(CSEntry) Then
                    Call cpcore.db.csSet(CSEntry, "ObjectProgramID", ObjectProgramID)
                    Call cpcore.db.csSet(CSEntry, "ArgumentList", ArgumentList)
                    Call cpcore.db.csSet(CSEntry, "SortOrder", SortOrder)
                End If
                Call cpcore.db.csClose(CSEntry)
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        '
        '========================================================================
        '
        Public Shared Sub exportApplicationCDefXml(cpCore As coreClass, ByVal privateFilesPathFilename As String, ByVal IncludeBaseFields As Boolean)
            Try
                Dim XML As xmlController
                Dim Content As String
                '
                XML = New xmlController(cpCore)
                Content = XML.GetXMLContentDefinition3("", IncludeBaseFields)
                Call cpCore.privateFiles.saveFile(privateFilesPathFilename, Content)
                XML = Nothing
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
    End Class
End Namespace
