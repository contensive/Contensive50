
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons.AdminSite
    Partial Public Class getAdminSiteClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '
        '========================================================================
        '   Save Whats New values if present
        '
        '   does NOT check AuthoringLocked -- you must check before calling
        '========================================================================
        '
        Private Sub SaveContentTracking(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "SaveContentTracking")
            '
            Dim ContentID As Integer
            Dim CSPointer As Integer
            Dim CSRules As Integer
            Dim CSContentWatch As Integer
            Dim CSContentWatchList As Integer
            Dim ContentWatchID As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            Dim MethodName As String
            '
            MethodName = "Admin.SaveContentTracking()"
            '
            '
            If adminContent.AllowContentTracking And (Not editRecord.Read_Only) Then
                '
                ' ----- Set default content watch link label
                '
                If (ContentWatchListIDCount > 0) And (ContentWatchLinkLabel = "") Then
                    If editRecord.menuHeadline <> "" Then
                        ContentWatchLinkLabel = editRecord.menuHeadline
                    ElseIf editRecord.nameLc <> "" Then
                        ContentWatchLinkLabel = editRecord.nameLc
                    Else
                        ContentWatchLinkLabel = "Click Here"
                    End If
                End If
                ' ----- update/create the content watch record for this content record
                '
                ContentID = editRecord.contentControlId
                CSContentWatch = cpCore.db.csOpen("Content Watch", "(ContentID=" & cpCore.db.encodeSQLNumber(ContentID) & ")And(RecordID=" & cpCore.db.encodeSQLNumber(editRecord.id) & ")")
                If Not cpCore.db.csOk(CSContentWatch) Then
                    Call cpCore.db.csClose(CSContentWatch)
                    CSContentWatch = cpCore.db.csInsertRecord("Content Watch")
                    Call cpCore.db.csSet(CSContentWatch, "contentid", ContentID)
                    Call cpCore.db.csSet(CSContentWatch, "recordid", editRecord.id)
                    Call cpCore.db.csSet(CSContentWatch, "ContentRecordKey", ContentID & "." & editRecord.id)
                    Call cpCore.db.csSet(CSContentWatch, "clicks", 0)
                End If
                If Not cpCore.db.csOk(CSContentWatch) Then
                    Call handleLegacyClassError(MethodName, "SaveContentTracking, can Not create New record")
                Else
                    ContentWatchID = cpCore.db.csGetInteger(CSContentWatch, "ID")
                    Call cpCore.db.csSet(CSContentWatch, "LinkLabel", ContentWatchLinkLabel)
                    Call cpCore.db.csSet(CSContentWatch, "WhatsNewDateExpires", ContentWatchExpires)
                    Call cpCore.db.csSet(CSContentWatch, "Link", ContentWatchLink)
                    '
                    ' ----- delete all rules for this ContentWatch record
                    '
                    'Call cpCore.app.DeleteContentRecords("Content Watch List Rules", "(ContentWatchID=" & ContentWatchID & ")")
                    CSPointer = cpCore.db.csOpen("Content Watch List Rules", "(ContentWatchID=" & ContentWatchID & ")")
                    Do While cpCore.db.csOk(CSPointer)
                        Call cpCore.db.csDeleteRecord(CSPointer)
                        Call cpCore.db.csGoNext(CSPointer)
                    Loop
                    Call cpCore.db.csClose(CSPointer)
                    '
                    ' ----- Update ContentWatchListRules for all entries in ContentWatchListID( ContentWatchListIDCount )
                    '
                    Dim ListPointer As Integer
                    If ContentWatchListIDCount > 0 Then
                        For ListPointer = 0 To ContentWatchListIDCount - 1
                            CSRules = cpCore.db.csInsertRecord("Content Watch List Rules")
                            If cpCore.db.csOk(CSRules) Then
                                Call cpCore.db.csSet(CSRules, "ContentWatchID", ContentWatchID)
                                Call cpCore.db.csSet(CSRules, "ContentWatchListID", ContentWatchListID(ListPointer))
                            End If
                            Call cpCore.db.csClose(CSRules)
                        Next
                    End If
                End If
                Call cpCore.db.csClose(CSContentWatch)
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("SaveContentTracking")
            '
        End Sub
        '
        '========================================================================
        '   Read in Whats New values if present
        '========================================================================
        '
        Private Sub LoadContentTrackingResponse(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadContentTrackingResponse")
            '
            Dim CSContentWatchList As Integer
            Dim RecordID As Integer
            '
            ContentWatchListIDCount = 0
            If (cpCore.docProperties.getText("WhatsNewResponse") <> "") And (adminContent.AllowContentTracking) Then
                '
                ' ----- set single fields
                '
                ContentWatchLinkLabel = cpCore.docProperties.getText("ContentWatchLinkLabel")
                ContentWatchExpires = cpCore.docProperties.getDate("ContentWatchExpires")
                '
                ' ----- Update ContentWatchListRules for all checked boxes
                '
                CSContentWatchList = cpCore.db.csOpen("Content Watch Lists")
                Do While cpCore.db.csOk(CSContentWatchList)
                    RecordID = (cpCore.db.csGetInteger(CSContentWatchList, "ID"))
                    If cpCore.docProperties.getBoolean("ContentWatchList." & RecordID) Then
                        If ContentWatchListIDCount >= ContentWatchListIDSize Then
                            ContentWatchListIDSize = ContentWatchListIDSize + 50
                            ReDim Preserve ContentWatchListID(ContentWatchListIDSize)
                        End If
                        ContentWatchListID(ContentWatchListIDCount) = RecordID
                        ContentWatchListIDCount = ContentWatchListIDCount + 1
                    End If
                    Call cpCore.db.csGoNext(CSContentWatchList)
                Loop
                Call cpCore.db.csClose(CSContentWatchList)
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadContentTrackingResponse")
            '
        End Sub
        '        '
        '        '========================================================================
        '        '   Load and Save
        '        '       From both response and database
        '        '
        '        '   This needs to be updated:
        '        '       - Divide into LoadCalendarEvents / SaveCalendarEvents
        '        '       - Put LoadCalendarEvents in LoadResponse/LoadDatabase
        '        '       - Put SaveCalendarEvents with SaveEditRecord
        '        '       - this is so a usererror will preserve form responses
        '        '       - Dont delete all and recreate / just update records
        '        '========================================================================
        '        '
        '        Private Sub LoadAndSaveMetaContent()
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadAndSaveMetaContent")
        '            '
        '            Dim CS As Integer
        '            Dim MetaContentID As Integer
        '            Dim MetaKeywordList As String
        '            '
        '            MetaContentID = cpCore.docProperties.getInteger("MetaContent.MetaContentID")
        '            If (MetaContentID <> 0) Then
        '                '
        '                ' ----- Load from Response
        '                '
        '                CS = cpCore.db.csOpenRecord("Meta Content", MetaContentID)
        '                If cpCore.db.cs_ok(CS) Then
        '                    Call cpCore.db.cs_set(CS, "Name", cpCore.docProperties.getText("MetaContent.PageTitle"))
        '                    Call cpCore.db.cs_set(CS, "MetaDescription", cpCore.docProperties.getText("MetaContent.MetaDescription"))
        '                    If True Then ' 3.3.930" Then
        '                        Call cpCore.db.cs_set(CS, "OtherHeadTags", cpCore.docProperties.getText("MetaContent.OtherHeadTags"))
        '                        MetaKeywordList = cpCore.docProperties.getText("MetaContent.MetaKeywordList")
        '                        MetaKeywordList = genericController.vbReplace(MetaKeywordList, ",", vbCrLf)
        '                        Do While genericController.vbInstr(1, MetaKeywordList, vbCrLf & " ") <> 0
        '                            MetaKeywordList = genericController.vbReplace(MetaKeywordList, vbCrLf & " ", vbCrLf)
        '                        Loop
        '                        Do While genericController.vbInstr(1, MetaKeywordList, " " & vbCrLf) <> 0
        '                            MetaKeywordList = genericController.vbReplace(MetaKeywordList, " " & vbCrLf, vbCrLf)
        '                        Loop
        '                        Do While genericController.vbInstr(1, MetaKeywordList, vbCrLf & vbCrLf) <> 0
        '                            MetaKeywordList = genericController.vbReplace(MetaKeywordList, vbCrLf & vbCrLf, vbCrLf)
        '                        Loop
        '                        Do While (MetaKeywordList <> "") And (Right(MetaKeywordList, 2) = vbCrLf)
        '                            MetaKeywordList = Left(MetaKeywordList, Len(MetaKeywordList) - 2)
        '                        Loop
        '                        Call cpCore.db.cs_set(CS, "MetaKeywordList", MetaKeywordList)
        '                    ElseIf cpCore.db.cs_isFieldSupported(CS, "OtherHeadTags") Then
        '                        Call cpCore.db.cs_set(CS, "OtherHeadTags", cpCore.docProperties.getText("MetaContent.OtherHeadTags"))
        '                    End If
        '                    Call cpCore.html.main_ProcessCheckList("MetaContent.KeywordList", "Meta Content", genericController.encodeText(MetaContentID), "Meta Keywords", "Meta Keyword Rules", "MetaContentID", "MetaKeywordID")
        '                End If
        '                Call cpCore.db.cs_Close(CS)
        '                '
        '                ' Clear any bakes involving this content
        '                '
        '                Call cpCore.cache.invalidateContent("Meta Content")
        '                Call cpCore.cache.invalidateContent("Meta Keyword Rules")
        '            End If
        '            '
        '            Exit Sub
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Call handleLegacyClassError3("LoadAndSaveMetaContent")
        '            '
        '        End Sub
        '
        '========================================================================
        '   Save Link Alias field if it supported, and is non-authoring
        '   if it is authoring, it will be saved by the userfield routines
        '   if not, it appears in the LinkAlias tab, and must be saved here
        '========================================================================
        '
        Private Sub SaveLinkAlias(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            Try
                '
                ' --use field ptr to test if the field is supported yet
                If cpCore.siteProperties.allowLinkAlias Then
                    Dim isDupError As Boolean = False
                    Dim linkAlias As String = cpCore.docProperties.getText("linkalias")
                    Dim OverRideDuplicate As Boolean = cpCore.docProperties.getBoolean("OverRideDuplicate")
                    Dim DupCausesWarning As Boolean = False
                    If linkAlias = "" Then
                        '
                        ' Link Alias is blank, use the record name
                        '
                        linkAlias = editRecord.nameLc
                        DupCausesWarning = True
                    End If
                    If (linkAlias <> "") Then
                        If OverRideDuplicate Then
                            Call cpCore.db.executeQuery("update " & adminContent.ContentTableName & " set linkalias=null where ( linkalias=" & cpCore.db.encodeSQLText(linkAlias) & ") and (id<>" & editRecord.id & ")")
                        Else
                            Dim CS As Integer = cpCore.db.csOpen(adminContent.Name, "( linkalias=" & cpCore.db.encodeSQLText(linkAlias) & ")and(id<>" & editRecord.id & ")")
                            If cpCore.db.csOk(CS) Then
                                isDupError = True
                                Call errorController.error_AddUserError(cpCore, "The Link Alias you entered can not be used because another record uses this value [" & linkAlias & "]. Enter a different Link Alias, or check the Override Duplicates checkbox in the Link Alias tab.")
                            End If
                            Call cpCore.db.csClose(CS)
                        End If
                        If Not isDupError Then
                            DupCausesWarning = True
                            Dim CS As Integer = cpCore.db.cs_open2(adminContent.Name, editRecord.id, True, True)
                            If cpCore.db.csOk(CS) Then
                                Call cpCore.db.csSet(CS, "linkalias", linkAlias)
                            End If
                            Call cpCore.db.csClose(CS)
                            '
                            ' Update the Link Aliases
                            '
                            Call docController.addLinkAlias(cpCore, linkAlias, editRecord.id, "", OverRideDuplicate, DupCausesWarning)
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        '   Read in Whats New values if present
        '   Field values must be loaded
        '========================================================================
        '
        Private Sub LoadContentTrackingDataBase(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "LoadContentTrackingDataBase")
            '
            Dim ContentID As Integer
            Dim CSPointer As Integer
            ' converted array to dictionary - Dim FieldPointer As Integer
            '
            ' ----- check if admin record is present
            '
            If (editRecord.id <> 0) And (adminContent.AllowContentTracking) Then
                '
                ' ----- Open the content watch record for this content record
                '
                ContentID = editRecord.contentControlId
                CSPointer = cpCore.db.csOpen("Content Watch", "(ContentID=" & cpCore.db.encodeSQLNumber(ContentID) & ")AND(RecordID=" & cpCore.db.encodeSQLNumber(editRecord.id) & ")")
                If cpCore.db.csOk(CSPointer) Then
                    ContentWatchLoaded = True
                    ContentWatchRecordID = (cpCore.db.csGetInteger(CSPointer, "ID"))
                    ContentWatchLink = (cpCore.db.csGet(CSPointer, "Link"))
                    ContentWatchClicks = (cpCore.db.csGetInteger(CSPointer, "Clicks"))
                    ContentWatchLinkLabel = (cpCore.db.csGet(CSPointer, "LinkLabel"))
                    ContentWatchExpires = (cpCore.db.csGetDate(CSPointer, "WhatsNewDateExpires"))
                    Call cpCore.db.csClose(CSPointer)
                End If
            End If
            '
            Exit Sub
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("LoadContentTrackingDataBase")
            '
        End Sub
        '
        '========================================================================
        '
        Private Sub SaveEditRecord(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass)
            Try
                Dim SaveCCIDValue As Integer = 0
                Dim ActivityLogOrganizationID As Integer = -1
                If (cpCore.doc.debug_iUserError <> "") Then
                    '
                    ' -- If There is an error, block the save
                    AdminAction = AdminActionNop
                ElseIf Not cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, adminContent.Name) Then
                    '
                    ' -- must be content manager
                ElseIf editRecord.Read_Only Then
                    '
                    ' -- read only block
                Else
                    '
                    ' -- Record will be saved, create a new one if this is an add
                    Dim NewRecord As Boolean = False
                    Dim RecordChanged As Boolean = False
                    Dim CSEditRecord As Integer = -1
                    If editRecord.id = 0 Then
                        NewRecord = True
                        RecordChanged = True
                        CSEditRecord = cpCore.db.csInsertRecord(adminContent.Name)
                    Else
                        NewRecord = False
                        CSEditRecord = cpCore.db.cs_open2(adminContent.Name, editRecord.id, True, True)
                    End If
                    If Not cpCore.db.csOk(CSEditRecord) Then
                        '
                        ' ----- Error: new record could not be created
                        '
                        If NewRecord Then
                            '
                            ' Could not insert record
                            '
                            cpCore.handleException(New ApplicationException("A new record could not be inserted for content [" & adminContent.Name & "]. Verify the Database table and field DateAdded, CreateKey, and ID."))
                        Else
                            '
                            ' Could not locate record you requested
                            '
                            cpCore.handleException(New ApplicationException("The record you requested (ID=" & editRecord.id & ") could not be found for content [" & adminContent.Name & "]"))
                        End If
                    Else
                        '
                        ' ----- Get the ID of the current record
                        '
                        editRecord.id = cpCore.db.csGetInteger(CSEditRecord, "ID")
                        '
                        ' ----- Create the update sql
                        '
                        Dim FieldChanged As Boolean = False
                        For Each keyValuePair In adminContent.fields
                            Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                            With field
                                Dim editRecordField As editRecordFieldClass = editRecord.fieldsLc(.nameLc)
                                Dim fieldValueObject As Object = editRecordField.value
                                Dim FieldValueText As String = genericController.encodeText(fieldValueObject)
                                Dim FieldName As String = .nameLc
                                Dim UcaseFieldName As String = genericController.vbUCase(FieldName)
                                '
                                ' ----- Handle special case fields
                                '
                                Select Case UcaseFieldName
                                    Case "NAME"
                                        '
                                        editRecord.nameLc = genericController.encodeText(fieldValueObject)
                                    Case "CCGUID"
                                        Dim saveValue As String = genericController.encodeText(fieldValueObject)
                                        If cpCore.db.csGetText(CSEditRecord, FieldName) <> saveValue Then
                                            FieldChanged = True
                                            RecordChanged = True
                                            Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                        End If
                                        'RecordChanged = True
                                        'Call cpCore.app.SetCS(CSEditRecord, FieldName, FieldValueVariant)
                                    Case "CONTENTCONTROLID"
                                        '
                                        ' run this after the save, so it will be blocked if the save fails
                                        ' block the change from this save
                                        ' Update the content control ID here, for all the children, and all the edit and archive records of both
                                        '
                                        Dim saveValue As Integer = genericController.EncodeInteger(fieldValueObject)
                                        If editRecord.contentControlId <> saveValue Then
                                            SaveCCIDValue = saveValue
                                            RecordChanged = True
                                        End If
                                    Case "ACTIVE"
                                        Dim saveValue As Boolean = genericController.EncodeBoolean(fieldValueObject)
                                        If cpCore.db.csGetBoolean(CSEditRecord, FieldName) <> saveValue Then
                                            FieldChanged = True
                                            RecordChanged = True
                                            Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                        End If
                                    Case "DATEEXPIRES"
                                        '
                                        ' ----- make sure content watch expires before content expires
                                        '
                                        If (Not genericController.IsNull(fieldValueObject)) Then
                                            If IsDate(fieldValueObject) Then
                                                Dim saveValue As Date = genericController.EncodeDate(fieldValueObject)
                                                If ContentWatchExpires <= Date.MinValue Then
                                                    ContentWatchExpires = saveValue
                                                ElseIf ContentWatchExpires > saveValue Then
                                                    ContentWatchExpires = saveValue
                                                End If
                                            End If
                                        End If
                                        '
                                    Case "DATEARCHIVE"
                                        '
                                        ' ----- make sure content watch expires before content archives
                                        '
                                        If (Not genericController.IsNull(fieldValueObject)) Then
                                            If IsDate(fieldValueObject) Then
                                                Dim saveValue As Date = genericController.EncodeDate(fieldValueObject)
                                                If (ContentWatchExpires) <= Date.MinValue Then
                                                    ContentWatchExpires = saveValue
                                                ElseIf ContentWatchExpires > saveValue Then
                                                    ContentWatchExpires = saveValue
                                                End If
                                            End If
                                        End If
                                End Select
                                '
                                ' ----- Put the field in the SQL to be saved
                                '
                                If IsVisibleUserField(.adminOnly, .developerOnly, .active, .authorable, .nameLc, adminContent.ContentTableName) And (NewRecord Or (Not .ReadOnly)) And (NewRecord Or (Not .NotEditable)) Then
                                    '
                                    ' ----- save the value by field type
                                    '
                                    Select Case .fieldTypeId
                                        Case FieldTypeIdAutoIdIncrement, FieldTypeIdRedirect
                                            '
                                            ' do nothing with these
                                            '
                                        Case FieldTypeIdFile, FieldTypeIdFileImage
                                            '
                                            ' filenames, upload to cdnFiles
                                            '
                                            If cpCore.docProperties.getBoolean(FieldName & ".DeleteFlag") Then
                                                RecordChanged = True
                                                FieldChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, "")
                                            End If
                                            FieldValueText = genericController.encodeText(fieldValueObject)
                                            If FieldValueText <> "" Then
                                                Dim Filename As String = encodeFilename(FieldValueText)
                                                Dim unixPathFilename As String = cpCore.db.csGetFilename(CSEditRecord, FieldName, Filename, adminContent.Name)
                                                Dim dosPathFilename As String = genericController.convertToDosSlash(unixPathFilename)
                                                Dim dosPath As String = genericController.getPath(dosPathFilename)
                                                cpCore.cdnFiles.upload(FieldName, dosPath, Filename)
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, unixPathFilename)
                                                RecordChanged = True
                                                FieldChanged = True
                                            End If
                                        Case FieldTypeIdBoolean
                                            '
                                            ' boolean
                                            '
                                            Dim saveValue As Boolean = genericController.EncodeBoolean(fieldValueObject)
                                            If cpCore.db.csGetBoolean(CSEditRecord, FieldName) <> saveValue Then
                                                RecordChanged = True
                                                FieldChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                            End If
                                        Case FieldTypeIdCurrency, FieldTypeIdFloat
                                            '
                                            ' Floating pointer numbers
                                            '
                                            Dim saveValue As Double = genericController.EncodeNumber(fieldValueObject)
                                            If cpCore.db.csGetNumber(CSEditRecord, FieldName) <> saveValue Then
                                                RecordChanged = True
                                                FieldChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                            End If
                                        Case FieldTypeIdDate
                                            '
                                            ' Date
                                            '
                                            Dim saveValue As Date = genericController.EncodeDate(fieldValueObject)
                                            If cpCore.db.csGetDate(CSEditRecord, FieldName) <> saveValue Then
                                                FieldChanged = True
                                                RecordChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                            End If
                                        Case FieldTypeIdInteger, FieldTypeIdLookup
                                            '
                                            ' Integers
                                            '
                                            Dim saveValue As Integer = genericController.EncodeInteger(fieldValueObject)
                                            If saveValue <> cpCore.db.csGetInteger(CSEditRecord, FieldName) Then
                                                FieldChanged = True
                                                RecordChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                            End If
                                        Case FieldTypeIdLongText, FieldTypeIdText, FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdHTML, FieldTypeIdFileHTML
                                            '
                                            ' Text
                                            '
                                            Dim saveValue As String = genericController.encodeText(fieldValueObject)
                                            If cpCore.db.csGet(CSEditRecord, FieldName) <> saveValue Then
                                                FieldChanged = True
                                                RecordChanged = True
                                                Call cpCore.db.csSet(CSEditRecord, FieldName, saveValue)
                                            End If
                                        Case FieldTypeIdManyToMany
                                            '
                                            ' Many to Many checklist
                                            '
                                            'MTMContent0 = models.complex.cdefmodel.getContentNameByID(cpcore,.contentId)
                                            'MTMContent1 = models.complex.cdefmodel.getContentNameByID(cpcore,.manyToManyContentID)
                                            'MTMRuleContent = models.complex.cdefmodel.getContentNameByID(cpcore,.manyToManyRuleContentID)
                                            'MTMRuleField0 = .ManyToManyRulePrimaryField
                                            'MTMRuleField1 = .ManyToManyRuleSecondaryField
                                            Call cpCore.html.main_ProcessCheckList("ManyToMany" & .id _
                                                , Models.Complex.cdefModel.getContentNameByID(cpCore, .contentId) _
                                                , CStr(editRecord.id) _
                                                , Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyContentID) _
                                                , Models.Complex.cdefModel.getContentNameByID(cpCore, .manyToManyRuleContentID) _
                                                , .ManyToManyRulePrimaryField _
                                                , .ManyToManyRuleSecondaryField)
                                        Case Else
                                            '
                                            ' Unknown other types
                                            '

                                            Dim saveValue As String = genericController.encodeText(fieldValueObject)
                                            FieldChanged = True
                                            RecordChanged = True
                                            Call cpCore.db.csSet(CSEditRecord, UcaseFieldName, saveValue)
                                            'sql &=  "," & .Name & "=" & cpCore.app.EncodeSQL(FieldValueVariant, .FieldType)
                                    End Select
                                End If
                                '
                                ' -- put any changes back in array for the next page to display
                                editRecordField.value = fieldValueObject
                                '
                                ' -- Log Activity for changes to people and organizattions
                                If FieldChanged Then
                                    Select Case genericController.vbLCase(adminContent.ContentTableName)
                                        Case "ccmembers"
                                            '
                                            If ActivityLogOrganizationID < 0 Then
                                                Dim person As personModel = personModel.create(cpCore, editRecord.id)
                                                If (person IsNot Nothing) Then
                                                    ActivityLogOrganizationID = person.OrganizationID
                                                End If
                                            End If
                                            logController.logActivity2(cpCore, "modifying field " & FieldName, editRecord.id, ActivityLogOrganizationID)
                                        Case "organizations"
                                            '
                                            Call logController.logActivity2(cpCore, "modifying field " & FieldName, 0, editRecord.id)
                                        Case "cclibraryfiles"
                                            '
                                            If cpCore.docProperties.getText("filename") <> "" Then
                                                Call cpCore.db.csSet(CSEditRecord, "altsizelist", "")
                                            End If
                                    End Select
                                End If
                            End With
                        Next
                        '
                        Call cpCore.db.csClose(CSEditRecord)
                        If RecordChanged Then
                            '
                            ' -- clear cache
                            Dim tableName As String = ""
                            If editRecord.contentControlId = 0 Then
                                tableName = Models.Complex.cdefModel.getContentTablename(cpCore, adminContent.Name)
                            Else
                                tableName = Models.Complex.cdefModel.getContentTablename(cpCore, editRecord.contentControlId_Name)
                            End If
                            Select Case tableName.ToLower()
                                Case linkAliasModel.contentTableName.ToLower()
                                    '
                                    linkAliasModel.invalidateCache(cpCore, editRecord.id)
                                    'Models.Complex.routeDictionaryModel.invalidateCache(cpCore)
                                Case addonModel.contentTableName.ToLower()
                                    '
                                    addonModel.invalidateCache(cpCore, editRecord.id)
                                    'Models.Complex.routeDictionaryModel.invalidateCache(cpCore)
                                Case Else
                                    linkAliasModel.invalidateCache(cpCore, editRecord.id)
                            End Select

                        End If
                        '
                        ' ----- Clear/Set PageNotFound
                        '
                        If editRecord.SetPageNotFoundPageID Then
                            Call cpCore.siteProperties.setProperty("PageNotFoundPageID", genericController.encodeText(editRecord.id))
                        End If
                        '
                        ' ----- Clear/Set LandingPageID
                        '
                        If editRecord.SetLandingPageID Then
                            Call cpCore.siteProperties.setProperty("LandingPageID", genericController.encodeText(editRecord.id))
                        End If
                        '
                        ' ----- clear/set authoring controls
                        '
                        Call cpCore.workflow.ClearEditLock(adminContent.Name, editRecord.id)
                        '
                        ' ----- if admin content is changed, reload the admincontent data in case this is a save, and not an OK
                        '
                        If RecordChanged And SaveCCIDValue <> 0 Then
                            Call Models.Complex.cdefModel.setContentControlId(cpCore, editRecord.contentControlId, editRecord.id, SaveCCIDValue)
                            editRecord.contentControlId_Name = Models.Complex.cdefModel.getContentNameByID(cpCore, SaveCCIDValue)
                            adminContent = Models.Complex.cdefModel.getCdef(cpCore, editRecord.contentControlId_Name)
                            adminContent.Id = adminContent.Id
                            adminContent.Name = adminContent.Name
                            ' false = cpCore.siteProperties.allowWorkflowAuthoring And adminContent.AllowWorkflowAuthoring
                        End If
                    End If
                    editRecord.Saved = True
                End If
            Catch ex As Exception
                cpCore.handleException(ex)
            End Try
        End Sub
        '
        '========================================================================
        ' Get Just the tablename from a sql statement
        '   This is to be compatible with the old way of setting up FieldTypeLookup
        '========================================================================
        '
        Private Function GetJustTableName(ByVal SQL As String) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetJustTableName")
            '
            GetJustTableName = Trim(UCase(SQL))
            Do While (GetJustTableName <> "") And (InStr(GetJustTableName, " ") <> 0)
                GetJustTableName = Mid(GetJustTableName, genericController.vbInstr(GetJustTableName, " ") + 1)
            Loop
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetJustTableName")
            '
        End Function
        '
        '========================================================================
        ' Test Content Access -- return based on Admin/Developer/MemberRules
        '   if developer, let all through
        '   if admin, block if table is developeronly
        '   if member, run blocking query (which also traps adminonly and developer only)
        '       if blockin query has a null RecordID, this member gets everything
        '       if not null recordid in blocking query, use RecordIDs in result for Where clause on this lookup
        '========================================================================
        '
        Private Function userHasContentAccess(ByVal ContentID As Integer) As Boolean
            Dim returnHas As Boolean = False
            Try
                Dim ContentName As String
                '
                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID)
                If ContentName <> "" Then
                    returnHas = cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, ContentName)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHas
        End Function
        '
        '========================================================================
        '   Display a field in the admin index form
        '========================================================================
        '
        Private Function GetForm_Index_GetCell(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, fieldName As String, ByVal CS As Integer, ByVal IsLookupFieldValid As Boolean, ByVal IsEmailContent As Boolean) As String
            Dim return_formIndexCell As String = ""
            Try
                Dim FieldText As String
                Dim Filename As String
                Dim Copy As String
                Dim Stream As New stringBuilderLegacyController
                Dim lookups() As String
                Dim LookupPtr As Integer
                Dim Pos As Integer
                Dim lookupTableCnt As Integer = 0
                '
                With adminContent.fields(fieldName.ToLower())
                    lookupTableCnt = .id ' workaround for universally creating the left join tablename for each field
                    Select Case .fieldTypeId
                        'Case FieldTypeImage
                        '    Stream.Add( Mid(cpCore.app.cs_get(CS, .Name), 7 + Len(.Name) + Len(AdminContent.ContentTableName)))
                        Case FieldTypeIdFile, FieldTypeIdFileImage
                            Filename = cpCore.db.csGet(CS, .nameLc)
                            Filename = genericController.vbReplace(Filename, "\", "/")
                            Pos = InStrRev(Filename, "/")
                            If Pos <> 0 Then
                                Filename = Mid(Filename, Pos + 1)
                            End If
                            Stream.Add(Filename)
                        Case FieldTypeIdLookup
                            If IsLookupFieldValid Then
                                Stream.Add(cpCore.db.csGet(CS, "LookupTable" & lookupTableCnt & "Name"))
                                lookupTableCnt += 1
                            ElseIf .lookupList <> "" Then
                                lookups = Split(.lookupList, ",")
                                LookupPtr = cpCore.db.csGetInteger(CS, .nameLc) - 1
                                If LookupPtr <= UBound(lookups) Then
                                    If LookupPtr < 0 Then
                                        'Stream.Add( "-1")
                                    Else
                                        Stream.Add(lookups(LookupPtr))
                                    End If
                                Else
                                    'Stream.Add( "-2")
                                End If

                            Else
                                'Stream.Add( "-3")
                                Stream.Add(" ")
                            End If
                        Case FieldTypeIdMemberSelect
                            If IsLookupFieldValid Then
                                Stream.Add(cpCore.db.csGet(CS, "LookupTable" & lookupTableCnt & "Name"))
                                lookupTableCnt += 1
                            Else
                                Stream.Add(cpCore.db.csGet(CS, .nameLc))
                            End If
                        Case FieldTypeIdBoolean
                            If cpCore.db.csGetBoolean(CS, .nameLc) Then
                                Stream.Add("yes")
                            Else
                                Stream.Add("no")
                            End If
                        Case FieldTypeIdCurrency
                            Stream.Add(FormatCurrency(cpCore.db.csGetNumber(CS, .nameLc)))
                        Case FieldTypeIdLongText, FieldTypeIdHTML
                            FieldText = cpCore.db.csGet(CS, .nameLc)
                            If Len(FieldText) > 50 Then
                                FieldText = Mid(FieldText, 1, 50) & "[more]"
                            End If
                            Stream.Add(FieldText)
                        Case FieldTypeIdFileText, FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript, FieldTypeIdFileHTML
                            ' rw( "n/a" )
                            Filename = cpCore.db.csGet(CS, .nameLc)
                            If Filename <> "" Then
                                Copy = cpCore.cdnFiles.readFile(Filename)
                                ' 20171103 - dont see why this is being converted, not html
                                'Copy = cpCore.html.convertActiveContent_internal(Copy, cpCore.doc.authContext.user.id, "", 0, 0, True, False, False, False, True, False, "", "", IsEmailContent, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextAdmin, cpCore.doc.authContext.isAuthenticated, Nothing, cpCore.doc.authContext.isEditingAnything())
                                Stream.Add(Copy)
                            End If
                        Case FieldTypeIdRedirect, FieldTypeIdManyToMany
                            Stream.Add("n/a")
                        Case Else
                            If .Password Then
                                Stream.Add("****")
                            Else
                                Stream.Add(cpCore.db.csGet(CS, .nameLc))
                            End If
                    End Select
                End With
                '
                return_formIndexCell = genericController.encodeHTML(Stream.Text)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return return_formIndexCell

        End Function
        '
        '========================================================================
        ' Get the Normal Edit Button Bar String
        '
        '   used on Normal Edit and others
        '========================================================================
        '
        Private Function GetForm_Edit_ButtonBar(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass, ByVal AllowDelete As Boolean, ByVal allowSave As Boolean, ByVal AllowAdd As Boolean, Optional ByVal AllowRefresh As Boolean = False) As String
            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_Edit_ButtonBar")
            '
            Dim Adminui As New adminUIController(cpCore)
            Dim IncludeCDefReload As Boolean
            Dim IsPageContent As Boolean
            Dim HasChildRecords As Boolean
            Dim CS As Integer
            Dim AllowMarkReviewed As Boolean
            '
            IsPageContent = Models.Complex.cdefModel.isWithinContent(cpCore, adminContent.Id, Models.Complex.cdefModel.getContentId(cpCore, "Page Content"))
            If Models.Complex.cdefModel.isContentFieldSupported(cpCore, adminContent.Name, "parentid") Then
                CS = cpCore.db.csOpen(adminContent.Name, "parentid=" & editRecord.id, , , , , , "ID")
                HasChildRecords = cpCore.db.csOk(CS)
                Call cpCore.db.csClose(CS)
            End If
            IncludeCDefReload = (LCase(adminContent.ContentTableName) = "cccontent") Or (LCase(adminContent.ContentTableName) = "ccdatasources") Or (LCase(adminContent.ContentTableName) = "cctables") Or (LCase(adminContent.ContentTableName) = "ccfields")
            AllowMarkReviewed = cpCore.db.isSQLTableField("default", adminContent.ContentTableName, "DateReviewed")
            '
            GetForm_Edit_ButtonBar = Adminui.GetEditButtonBar2(MenuDepth, AllowDelete And editRecord.AllowDelete, editRecord.AllowCancel, (allowSave And editRecord.AllowSave), (SpellCheckSupported And (Not SpellCheckRequest)), editRecord.AllowPublish, editRecord.AllowAbort, editRecord.AllowSubmit, editRecord.AllowApprove, (AllowAdd And adminContent.AllowAdd And editRecord.AllowInsert), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed, AllowRefresh, (allowSave And editRecord.AllowSave And (editRecord.id <> 0)))
            'GetForm_Edit_ButtonBar = AdminUI.GetEditButtonBar2( MenuDepth, iAllowDelete And EditRecord.AllowDelete, EditRecord.AllowCancel, (iAllowSave And EditRecord.AllowSave), (SpellCheckSupported And (Not SpellCheckRequest)), EditRecord.AllowPublish, EditRecord.AllowAbort, EditRecord.AllowSubmit, EditRecord.AllowApprove, (AdminContent.allowAdd And EditRecord.AllowInsert), IncludeCDefReload, HasChildRecords, IsPageContent, AllowMarkReviewed)
            Exit Function
            '
            ' ----- Error Trap
            '
ErrorTrap:
            Call handleLegacyClassError3("GetForm_Edit_ButtonBar")
            '
        End Function
        '
        '========================================================================
        ' ----- Print the edit form
        '
        '   Prints the correct form based on the current AdminContent.contenttablename
        '   AdminContent.type is not longer used
        '========================================================================
        '
        Private Function GetForm_Edit(adminContent As Models.Complex.cdefModel, editRecord As editRecordClass) As String
            Dim returnHtml As String = ""
            Try
                Dim ContentType As csv_contentTypeEnum
                Dim editorAddonListJSON As String
                Dim Active As Boolean
                Dim Name As String
                Dim WFMessage As String = ""
                Dim IgnoreString As String = ""
                Dim styleList As String
                Dim styleOptionList As String = ""
                Dim fieldEditorList As String
                Dim fieldTypeDefaultEditors As String()
                Dim fieldEditorPreferencesList As String
                Dim dt As DataTable
                Dim Cells As String(,)
                Dim fieldId As Integer
                Dim TableID As Integer
                Dim LastSendTestDate As Date
                Dim AllowEmailSendWithoutTest As Boolean
                Dim fieldEditorOptions As New Dictionary(Of String, Integer)
                Dim Ptr As Integer
                Dim fieldEditorOptionCnt As Integer
                Dim SQL As String
                Dim IsTemplateTable As Boolean
                Dim TemplateIDForStyles As Integer
                Dim emailIdForStyles As Integer
                ' Dim RootPageSectionID As Integer
                Dim AllowajaxTabs As Boolean
                Dim XMLTools As New xmlController(cpCore)
                Dim IsPageContentTable As Boolean
                Dim IsSectionTable As Boolean
                Dim IsEmailTable As Boolean
                Dim IsLandingPageTemp As Boolean
                Dim Pos As Integer
                Dim IsLandingPageParent As Boolean
                Dim IsLandingSection As Boolean
                Dim CreatedCopy As String
                Dim ModifiedCopy As String
                Dim CS As Integer
                Dim EditReferer As String
                Dim CustomDescription As String
                Dim EditSectionButtonBar As String
                Dim EmailSent As Boolean
                Dim EmailSubmitted As Boolean
                Dim Stream As New stringBuilderLegacyController
                Dim SystemEmailCID As Integer
                Dim ConditionalEmailCID As Integer
                Dim HeaderDescription As String
                Dim Adminui As New adminUIController(cpCore)
                Dim IsLandingPage As Boolean
                Dim IsRootPage As Boolean
                Dim CreatedBy As String
                Dim allowCMEdit As Boolean
                Dim allowCMAdd As Boolean
                Dim allowCMDelete As Boolean
                Dim AllowAdd As Boolean
                Dim AllowDelete As Boolean
                Dim allowSave As Boolean
                '
                CustomDescription = ""
                AllowajaxTabs = (cpCore.siteProperties.getBoolean("AllowAjaxEditTabBeta", False))
                '
                If ((cpCore.doc.debug_iUserError <> "") And editRecord.Loaded) Then
                    '
                    ' block load if there was a user error and it is already loaded (assume error was from response )
                    '
                ElseIf (adminContent.Id <= 0) Then
                    '
                    ' Invalid Content
                    '
                    Call errorController.error_AddUserError(cpCore, "There was a problem identifying the content you requested. Please return to the previous form and verify your selection.")
                    Return ""
                ElseIf editRecord.Loaded And Not editRecord.Saved Then
                    '
                    '   File types need to be reloaded from the Db, because...
                    '       LoadDb - sets them to the path-page
                    '       LoadResponse - sets the blank if no change, filename if there is an upload
                    '       SaveEditRecord - if blank, no change. If a filename it saves the uploaded file
                    '       GetForm_Edit - expects the Db value to be in EditRecordValueVariants (path-page)
                    '
                    ' xx This was added to bypass the load for the editrefresh case (reload the response so the editor preference can change)
                    ' xx  I do not know why the following section says "reload even if it is loaded", but lets try this
                    '
                    For Each keyValuePair In adminContent.fields
                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
                        Select Case field.fieldTypeId
                            Case FieldTypeIdFile, FieldTypeIdFileImage
                                editRecord.fieldsLc(field.nameLc).value = editRecord.fieldsLc(field.nameLc).dbValue
                        End Select
                    Next
                    'For Ptr = 0 To adminContent.fields.Count - 1
                    '    fieldType = arrayOfFields(Ptr).fieldType
                    '    Select Case fieldType
                    '        Case FieldTypeFile, FieldTypeImage
                    '            EditRecordValuesObject(Ptr) = EditRecordDbValues(Ptr)
                    '    End Select
                    'Next
                Else
                    '
                    ' otherwise, load the record, even if it was loaded during a previous form process
                    '
                    Call LoadEditRecord(adminContent, editRecord, True)
                    If (editRecord.contentControlId = 0) Then
                        If (cpCore.doc.debug_iUserError <> "") Then
                            '
                            ' known user error, just return
                            '
                        Else
                            '
                            ' unknown error, set userError and return
                            '
                            errorController.error_AddUserError(cpCore, "There was an unknown error in your request for data. Please let the site administrator know.")
                        End If
                        Return ""
                    End If
                End If
                '
                ' Test if this editors has access to this record
                '
                If Not userHasContentAccess(editRecord.contentControlId) Then
                    Call errorController.error_AddUserError(cpCore, "Your account on this system does not have access rights to edit this content.")
                    Return ""
                End If
                If False Then
                    '
                    ' Test for 100Mb available in Content Files drive
                    '
                    If cpCore.appRootFiles.getDriveFreeSpace() < 1.0E+8! Then
                        Call errorController.error_AddUserError(cpCore, "The server drive holding data for this site may not have enough free space to complete this edit operation. If you attempt to save, your data may be lost. Please contact the site administrator.")
                    End If
                    If cpCore.privateFiles.getDriveFreeSpace() < 1.0E+8! Then
                        Call errorController.error_AddUserError(cpCore, "The server drive holding data for this site may not have enough free space to complete this edit operation. If you attempt to save, your data may be lost. Please contact the site administrator.")
                    End If
                End If
                '
                ' Setup Edit Referer
                '
                EditReferer = cpCore.docProperties.getText(RequestNameEditReferer)
                If EditReferer = "" Then
                    EditReferer = cpCore.webServer.requestReferer
                    If EditReferer <> "" Then
                        '
                        ' special case - if you are coming from the advanced search, go back to the list page
                        '
                        EditReferer = genericController.vbReplace(EditReferer, "&af=39", "")
                        '
                        ' if referer includes AdminWarningMsg (admin hint message), remove it -- this edit may fix the problem
                        '
                        Pos = genericController.vbInstr(1, EditReferer, "AdminWarningMsg=", vbTextCompare)
                        If Pos <> 0 Then
                            EditReferer = Left(EditReferer, Pos - 2)
                        End If
                    End If
                End If
                Call cpCore.doc.addRefreshQueryString(RequestNameEditReferer, EditReferer)
                '
                ' Print common form elements
                '
                Call Stream.Add(GetForm_EditFormStart(AdminFormEdit))
                '
                IsLandingPageParent = False
                'LandingPageID = 0
                IsLandingPage = False
                IsRootPage = False
                TemplateIDForStyles = 0
                IsTemplateTable = (LCase(adminContent.ContentTableName) = "cctemplates")
                IsPageContentTable = (LCase(adminContent.ContentTableName) = "ccpagecontent")
                IsSectionTable = (LCase(adminContent.ContentTableName) = "ccsections")
                IsEmailTable = (LCase(adminContent.ContentTableName) = "ccemail")
                '
                If IsEmailTable Then
                    '
                    ' ----- special case - email
                    '
                    emailIdForStyles = editRecord.id
                End If
                '
                If IsPageContentTable Then
                    '
                    ' ----- special case - page content
                    '
                    If (editRecord.id <> 0) Then
                        '
                        ' landing page case
                        '
                        '$$$$$ problem -- could be landing page based on domain, not property
                        'LandingPageID = (cpCore.siteProperties.getinteger("LandingPageID", 0))
                        If cpCore.siteProperties.landingPageID = 0 Then
                            '
                            ' The call generated a user error because the landingpageid could not be determined
                            ' block it
                            '
                            'Call cpCore.main_GetUserError
                        Else
                            IsLandingPage = (editRecord.id = cpCore.siteProperties.landingPageID)
                            'If IsLandingPage Then
                            '    If genericController.EncodeInteger(cpCore.main_GetSiteProperty2("LandingPageID", "", True)) <> LandingPageID Then
                            '        IsLandingPageTemp = True
                            '    End If
                            'End If
                            IsRootPage = IsPageContentTable And (editRecord.parentID = 0)
                            'If IsRootPage Then
                            '    '$$$$$ cache
                            '    CS = cpCore.db.cs_open("Site Sections", "RootPageID=" & editRecord.id, , , , , , "ID")
                            '    IsRootPage = cpCore.db.cs_ok(CS)
                            '    If IsRootPage Then
                            '        RootPageSectionID = cpCore.db.cs_getInteger(CS, "ID")
                            '    End If
                            '    Call cpCore.db.cs_Close(CS)
                            'End If
                        End If
                    End If
                End If
                '
                If (Not IsLandingPage) And (IsPageContentTable Or IsSectionTable) Then
                    ''
                    '' ----- special case, Is this page a LandingPageParent (Parent of the landing page), or is this section the landing page section
                    ''
                    'TestPageID = cpCore.siteProperties.landingPageID
                    'Do While LoopPtr < 20 And (TestPageID <> 0)
                    '    IsLandingPageParent = IsPageContentTable And (editRecord.id = TestPageID)
                    '    IsLandingSection = IsSectionTable And (EditRecordRootPageID = TestPageID)
                    '    If IsLandingPageParent Or IsLandingSection Then
                    '        Exit Do
                    '    End If
                    '    PCCPtr = cpCore.pages.cache_pageContent_getPtr(TestPageID, False, False)
                    '    If PCCPtr >= 0 Then
                    '        TestPageID = genericController.EncodeInteger(PCC(PCC_ParentID, PCCPtr))
                    '    End If
                    '    LoopPtr = LoopPtr + 1
                    'Loop
                End If
                '
                ' ----- special case messages
                '
                If IsLandingSection Then
                    CustomDescription = "<div>This is the default Landing Section for this website. This section is displayed when no specific page is requested. It should not be deleted, renamed, marked inactive, blocked or hidden.</div>"
                ElseIf IsLandingPageTemp Then
                    CustomDescription = "<div>This page is being used as the default Landing Page for this website, although it has not been set. This may be because a landing page has not been created, or it has been deleted. To make this page the permantent landing page, check the appropriate box in the control tab.</div>"
                ElseIf IsLandingPage Then
                    CustomDescription = "<div>This is the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>"
                ElseIf IsLandingPageParent Then
                    CustomDescription = "<div>This page is a parent of the default Landing Page for this website. It should not be deleted. You can not mark this record inactive, or use the Publish Date, Expire Date or Archive Date features.</div>"
                ElseIf IsRootPage Then
                    CustomDescription = "<div>This page is a Root Page. A Root Page is the primary page of a section. If you delete or inactivate this page, the section will create a new blank page in its place.</div>"
                End If
                '
                ' ----- Determine TemplateIDForStyles
                '
                If IsTemplateTable Then
                    TemplateIDForStyles = editRecord.id
                ElseIf IsPageContentTable Then
                    'Call cpCore.pages.getPageArgs(editRecord.id, false, False, ignoreInteger, TemplateIDForStyles, ignoreInteger, IgnoreString, IgnoreBoolean, ignoreInteger, IgnoreBoolean, "")
                End If
                '
                ' ----- create page headers
                '
                If editRecord.id = 0 Then
                    HeaderDescription = "<div>New Record</div>"
                Else
                    HeaderDescription = "" _
                    & "<table border=0 cellpadding=0 cellspacing=0 style=""width:90%"">"
                    If CustomDescription <> "" Then
                        HeaderDescription = HeaderDescription & "<tr><td colspan=2>" & CustomDescription & "<div>&nbsp;</div></td></tr>"
                    End If
                    HeaderDescription = HeaderDescription _
                    & "<tr><td width=""50%"">" _
                    & "Name: " & editRecord.nameLc _
                    & "<br>Record ID: " & editRecord.id _
                    & "</td><td width=""50%"">"
                    '
                    CreatedCopy = ""
                    Dim editRecordDateAdded As Date
                    editRecordDateAdded = genericController.EncodeDate(editRecord.fieldsLc("dateadded").value)
                    If editRecord.dateAdded <> Date.MinValue Then
                        CreatedCopy = CreatedCopy & " " & editRecordDateAdded.ToString()  ' editRecord.dateAdded
                    End If
                    '
                    CreatedBy = "the system"
                    If editRecord.createByMemberId <> 0 Then
                        CS = cpCore.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" & editRecord.createByMemberId)
                        'CS = cpCore.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.AddedByMemberID)
                        If cpCore.db.csOk(CS) Then
                            Name = cpCore.db.csGetText(CS, "name")
                            Active = cpCore.db.csGetBoolean(CS, "active")
                            If Not Active And (Name <> "") Then
                                CreatedBy = "Inactive user " & Name
                            ElseIf Not Active Then
                                CreatedBy = "Inactive user #" & editRecord.createByMemberId
                            ElseIf Name = "" Then
                                CreatedBy = "Unnamed user #" & editRecord.createByMemberId
                            Else
                                CreatedBy = Name
                            End If
                        Else
                            CreatedBy = "deleted user #" & editRecord.createByMemberId
                        End If
                        Call cpCore.db.csClose(CS)
                    End If
                    If CreatedBy <> "" Then
                        CreatedCopy = CreatedCopy & " by " & CreatedBy
                    Else
                    End If
                    HeaderDescription = HeaderDescription & "Created:" & CreatedCopy
                    '
                    ModifiedCopy = ""
                    If editRecord.modifiedDate = Date.MinValue Then
                        ModifiedCopy = CreatedCopy
                    Else
                        ModifiedCopy = ModifiedCopy & " " & editRecord.modifiedDate
                        CreatedBy = "the system"
                        If editRecord.modifiedByMemberID <> 0 Then
                            CS = cpCore.db.csOpenSql_rev("default", "select Name,Active from ccMembers where id=" & editRecord.modifiedByMemberID)
                            'CS = cpCore.app.openCsSql_rev("default", "select Name,Active from ccmembers where id=" & EditRecord.ModifiedByMemberID)
                            If cpCore.db.csOk(CS) Then
                                Name = cpCore.db.csGetText(CS, "name")
                                Active = cpCore.db.csGetBoolean(CS, "active")
                                If Not Active And (Name <> "") Then
                                    CreatedBy = "Inactive user " & Name
                                ElseIf Not Active Then
                                    CreatedBy = "Inactive user #" & editRecord.modifiedByMemberID
                                ElseIf Name = "" Then
                                    CreatedBy = "Unnamed user #" & editRecord.modifiedByMemberID
                                Else
                                    CreatedBy = Name
                                End If
                            Else
                                CreatedBy = "deleted user #" & editRecord.modifiedByMemberID
                            End If
                            Call cpCore.db.csClose(CS)
                        End If
                        If CreatedBy <> "" Then
                            ModifiedCopy = ModifiedCopy & " by " & CreatedBy
                        Else
                        End If
                    End If
                    If False Then
                        If editRecord.IsInserted Then
                            HeaderDescription = HeaderDescription & "<BR >Last Published: not published"
                        Else
                            HeaderDescription = HeaderDescription & "<BR >Last Published:" & ModifiedCopy
                        End If
                    Else
                        HeaderDescription = HeaderDescription & "<BR >Last Modified:" & ModifiedCopy
                    End If
                    '
                    ' Add Edit Locking to right panel
                    '
                    If editRecord.EditLock Then
                        HeaderDescription = HeaderDescription & "<BR ><b>Record is locked by " & editRecord.EditLockMemberName & " until " & editRecord.EditLockExpires & "</b>"
                    End If
                    '
                    HeaderDescription = HeaderDescription & "</td></tr>"
                    ''
                    'If Not False Then
                    '    HeaderDescription = HeaderDescription & "<tr><td colspan=2>Authoring Mode: Immediate</td></tr>"
                    'Else
                    '    HeaderDescription = HeaderDescription & "<tr><td style=""vertical-align:top;"">Authoring Mode: Workflow</td>"
                    '    If editRecord.EditLock Then
                    '        WFMessage = WFMessage & "<div>Locked: Currently being edited by " & editRecord.EditLockMemberName & "</div>"
                    '    End If
                    '    If editRecord.LockModifiedDate <> Date.MinValue Then
                    '        WFMessage = WFMessage & "<div>Modified: " & editRecord.LockModifiedDate & " by " & editRecord.LockModifiedName & " and has not been published</div>"
                    '    End If
                    '    If editRecord.SubmitLock Then
                    '        WFMessage = WFMessage & "<div>Submitted for Publishing: " & editRecord.SubmittedDate & " by " & editRecord.SubmittedName & "</div>"
                    '    End If
                    '    If editRecord.ApproveLock Then
                    '        WFMessage = WFMessage & "<div>Approved for Publishing: " & editRecord.SubmittedDate & " by " & editRecord.SubmittedName & "</div>"
                    '    End If
                    '    If WFMessage <> "" Then
                    '        HeaderDescription = HeaderDescription & "<td>" & WFMessage & "</td></tr>"
                    '    Else
                    '        HeaderDescription = HeaderDescription & "<td>&nbsp;</td></tr>"
                    '    End If
                    'End If
                    ''
                    HeaderDescription = HeaderDescription & "</table>"
                End If
                '
                ' ----- determine access details
                '
                Call cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowCMEdit, allowCMAdd, allowCMDelete)
                AllowAdd = adminContent.AllowAdd And allowCMAdd
                AllowDelete = adminContent.AllowDelete And allowCMDelete And (editRecord.id <> 0)
                allowSave = allowCMEdit
                '
                ' ----- custom fieldEditors
                '
                '
                '   Editor Preference
                '   any addon can be an editor for a fieldtype with a checkbox in the addon
                '   the editor in any field can be over-ridden by just a single member with a popup next to the editor
                '       that popup (fancybox) sets the hidden fieldEditorPreference to fieldid:addonid and submits the form
                '       the edit form does a refresh action after setting the members property "editorPreferencesForContent:99"
                '   if no editor preference, the default editor is used from a drop-down selection in fieldtypes
                '   if nothing in field types, Contensive handles it internally
                '
                Call Stream.Add(cr & "<input type=""hidden"" name=""fieldEditorPreference"" id=""fieldEditorPreference"" value="""">")
                '
                fieldEditorList = editorController.getFieldTypeDefaultEditorAddonIdList(cpCore)
                fieldTypeDefaultEditors = Split(fieldEditorList, ",")
                '
                ' load user's editor preferences to fieldEditorPreferences() - this is the editor this user has picked when there are >1
                '   fieldId:addonId,fieldId:addonId,etc
                '   with custom FancyBox form in edit window with button "set editor preference"
                '   this button causes a 'refresh' action, reloads fields with stream without save
                '
                fieldEditorPreferencesList = cpCore.userProperty.getText("editorPreferencesForContent:" & adminContent.Id, "")
                '
                ' add the addon editors assigned to each field
                ' !!!!! this should be added to metaData load
                '
                Dim Cnt As Integer
                SQL = "select" _
                    & " f.id,f.editorAddonID" _
                    & " from ccfields f" _
                    & " where" _
                    & " f.ContentID=" & adminContent.Id _
                    & " and f.editorAddonId is not null"
                dt = cpCore.db.executeQuery(SQL)

                Cells = cpCore.db.convertDataTabletoArray(dt)
                Cnt = Cells.GetLength(1)
                'If CBool(Cells.GetLength(0)) Then
                '    Cnt = 0
                'Else
                '    Cnt = UBound(Cells, 2) + 1
                'End If
                For Ptr = 0 To Cnt - 1
                    fieldId = genericController.EncodeInteger(Cells(0, Ptr))
                    If fieldId > 0 Then
                        fieldEditorPreferencesList = fieldEditorPreferencesList & "," & fieldId & ":" & Cells(1, Ptr)
                    End If
                Next
                '
                ' load fieldEditorOptions - these are all the editors available for each field
                '
                fieldEditorOptionCnt = 0
                SQL = "select r.contentFieldTypeId,a.Id" _
                    & " from ccAddonContentFieldTypeRules r" _
                    & " left join ccaggregatefunctions a on a.id=r.addonid" _
                    & " where (r.active<>0)and(a.active<>0)and(a.id is not null) order by r.contentFieldTypeID"
                dt = cpCore.db.executeQuery(SQL)
                Cells = cpCore.db.convertDataTabletoArray(dt)
                fieldEditorOptionCnt = UBound(Cells, 2) + 1
                For Ptr = 0 To fieldEditorOptionCnt - 1
                    fieldId = genericController.EncodeInteger(Cells(0, Ptr))
                    If (fieldId > 0) And (Not fieldEditorOptions.ContainsKey(fieldId.ToString)) Then
                        fieldEditorOptions.Add(fieldId.ToString, genericController.EncodeInteger(Cells(1, Ptr)))
                    End If
                Next
                '
                ' ----- determine contentType for editor
                '
                If genericController.vbLCase(adminContent.Name) = "email templates" Then
                    ContentType = csv_contentTypeEnum.contentTypeEmailTemplate
                ElseIf genericController.vbLCase(adminContent.ContentTableName) = "cctemplates" Then
                    ContentType = csv_contentTypeEnum.contentTypeWebTemplate
                ElseIf genericController.vbLCase(adminContent.ContentTableName) = "ccemail" Then
                    ContentType = csv_contentTypeEnum.contentTypeEmail
                Else
                    ContentType = csv_contentTypeEnum.contentTypeWeb
                End If
                '
                ' ----- editor strings needed - needs to be on-demand
                '
                editorAddonListJSON = cpCore.html.main_GetEditorAddonListJSON(ContentType)
                styleList = "" ' cpCore.html.main_GetStyleSheet2(ContentType, TemplateIDForStyles, emailIdForStyles)
                '
                ' ----- Create edit page
                '
                Select Case genericController.vbUCase(adminContent.ContentTableName)
                    Case genericController.vbUCase("ccMembers")
                        If Not (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) Then
                            '
                            ' Must be admin
                            '
                            Call Stream.Add(GetForm_Error(
                            "This edit form requires Member Administration access." _
                            , ""
                            ))
                        Else
                            EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePerson)
                            Call Stream.Add(EditSectionButtonBar)
                            Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                            Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                            Call Stream.Add(GetForm_Edit_AddTab("Groups", GetForm_Edit_MemberGroups(adminContent, editRecord), allowAdminTabs))
                            'Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                            'Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_MemberReports(adminContent, editRecord), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                            If allowAdminTabs Then
                                Call Stream.Add(cpCore.html.menu_GetComboTabs())
                            End If
                            Call Stream.Add(EditSectionButtonBar)
                        End If
                    Case "CCEMAIL"
                        '
                        ' ----- Email table
                        '
                        SystemEmailCID = Models.Complex.cdefModel.getContentId(cpCore, "System Email")
                        ConditionalEmailCID = Models.Complex.cdefModel.getContentId(cpCore, "Conditional Email")
                        LastSendTestDate = Date.MinValue
                        If True Then ' 3.4.201" Then
                            AllowEmailSendWithoutTest = (cpCore.siteProperties.getBoolean("AllowEmailSendWithoutTest", False))
                            If editRecord.fieldsLc.ContainsKey("lastsendtestdate") Then
                                LastSendTestDate = genericController.EncodeDate(editRecord.fieldsLc("lastsendtestdate").value)
                            End If
                        End If
                        If Not (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) Then
                            '
                            ' Must be admin
                            '
                            Call Stream.Add(GetForm_Error(
                            "This edit form requires Member Administration access." _
                            , "This edit form requires Member Administration access."
                            ))
                        ElseIf Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, SystemEmailCID) Then
                            '
                            ' System Email
                            '
                            EmailSubmitted = False
                            If editRecord.id <> 0 Then
                                If editRecord.fieldsLc.ContainsKey("testmemberid") Then
                                    editRecord.fieldsLc.Item("testmemberid").value = cpCore.doc.authContext.user.id
                                End If
                            End If
                            EditSectionButtonBar = ""
                            If MenuDepth > 0 Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                            Else
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCancel, , , "Return processSubmit(this)")
                            End If
                            If (AllowDelete) And (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonDeleteEmail, , , "If(!DeleteCheck())Return False;")
                            End If
                            If (Not EmailSubmitted) And (Not EmailSent) Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSave, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonOK, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSendTest, , , "Return processSubmit(this)")
                            ElseIf AllowAdd Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCreateDuplicate, , , "Return processSubmit(this)")
                            End If
                            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4)
                            '
                            Call Stream.Add(EditSectionButtonBar)
                            Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                            Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                            Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only And (Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, editRecord.Read_Only And (Not cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore))), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                            If allowAdminTabs Then
                                Call Stream.Add(cpCore.html.menu_GetComboTabs())
                                'Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                            End If
                            Call Stream.Add(EditSectionButtonBar)
                        ElseIf Models.Complex.cdefModel.isWithinContent(cpCore, editRecord.contentControlId, ConditionalEmailCID) Then
                            '
                            ' Conditional Email
                            '
                            EmailSubmitted = False
                            If editRecord.id <> 0 Then
                                If editRecord.fieldsLc.ContainsKey("testmemberid") Then
                                    editRecord.fieldsLc.Item("testmemberid").value = cpCore.doc.authContext.user.id
                                End If
                                If editRecord.fieldsLc.ContainsKey("submitted") Then
                                    EmailSubmitted = genericController.EncodeBoolean(editRecord.fieldsLc.Item("submitted").value)
                                End If
                            End If
                            EditSectionButtonBar = ""
                            If MenuDepth > 0 Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                            Else
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCancel, , , "Return processSubmit(this)")
                            End If
                            If AllowDelete Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonDeleteEmail, , , "If(!DeleteCheck())Return False;")
                            End If
                            If (Not EmailSubmitted) Then
                                '
                                ' Not Submitted
                                '
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSave, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonOK, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton2(ButtonActivate, , , "Return processSubmit(this)", (LastSendTestDate = Date.MinValue) And (Not AllowEmailSendWithoutTest))
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSendTest, , , "Return processSubmit(this)")
                            Else
                                '
                                ' Submitted
                                '
                                If AllowAdd Then
                                    EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCreateDuplicate, , , "Return processSubmit(this)")
                                End If
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonDeactivate, , , "Return processSubmit(this)")
                            End If
                            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4)
                            '
                            Call Stream.Add(EditSectionButtonBar)
                            Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                            Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only Or EmailSubmitted, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                            Call Stream.Add(GetForm_Edit_AddTab("Condition&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only Or EmailSubmitted), allowAdminTabs))
                            'Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(editrecord.read_only Or EmailSubmitted), AllowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                            If allowAdminTabs Then
                                Call Stream.Add(cpCore.html.menu_GetComboTabs())
                                'Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                            End If
                            Call Stream.Add(EditSectionButtonBar)
                        Else
                            '
                            ' Group Email
                            '
                            EmailSubmitted = False
                            EmailSent = False
                            If editRecord.id <> 0 Then
                                If editRecord.fieldsLc.ContainsKey("testmemberid") Then
                                    editRecord.fieldsLc.Item("testmemberid").value = cpCore.doc.authContext.user.id
                                End If
                                If editRecord.fieldsLc.ContainsKey("submitted") Then
                                    EmailSubmitted = genericController.EncodeBoolean(editRecord.fieldsLc.Item("submitted").value)
                                End If
                                If editRecord.fieldsLc.ContainsKey("sent") Then
                                    EmailSent = genericController.EncodeBoolean(editRecord.fieldsLc.Item("sent").value)
                                End If
                            End If
                            EditSectionButtonBar = ""
                            If MenuDepth > 0 Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonClose, , , "window.close();")
                            Else
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCancel, , , "Return processSubmit(this)")
                            End If
                            If (editRecord.id <> 0) Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonDeleteEmail, , , "If(!DeleteCheck())Return False;")
                            End If
                            If (Not EmailSubmitted) And (Not EmailSent) Then
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSave, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonOK, , , "Return processSubmit(this)")
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton2(ButtonSend, , , "Return processSubmit(this)", (LastSendTestDate = Date.MinValue) And (Not AllowEmailSendWithoutTest))
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonSendTest, , , "Return processSubmit(this)")
                            Else
                                '
                                ' Submitted
                                '
                                EditSectionButtonBar = EditSectionButtonBar & cpCore.html.html_GetFormButton(ButtonCreateDuplicate, , , "Return processSubmit(this)")
                            End If
                            EditSectionButtonBar = cpCore.html.main_GetPanel(EditSectionButtonBar, "ccPanel", "ccPanelHilite", "ccPanelShadow", "100%", 4)
                            '
                            Call Stream.Add(EditSectionButtonBar)
                            Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                            Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only Or EmailSubmitted Or EmailSent, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                            Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Groups", GetForm_Edit_EmailRules(adminContent, editRecord, editRecord.Read_Only Or EmailSubmitted Or EmailSent), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Send&nbsp;To&nbsp;Topics", GetForm_Edit_EmailTopics(adminContent, editRecord, editRecord.Read_Only Or EmailSubmitted Or EmailSent), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Bounce&nbsp;Control", GetForm_Edit_EmailBounceStatus(), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                            If allowAdminTabs Then
                                Call Stream.Add(cpCore.html.menu_GetComboTabs())
                                'Call Stream.Add("<div Class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                            End If
                            Call Stream.Add(EditSectionButtonBar)
                        End If
                    Case "CCCONTENT"
                        If Not (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) Then
                            '
                            ' Must be admin
                            '
                            Call Stream.Add(GetForm_Error(
                            "This edit form requires Member Administration access." _
                            , "This edit form requires Member Administration access."
                            ))
                        Else
                            EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                            Call Stream.Add(EditSectionButtonBar)
                            Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                            Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                            Call Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_GroupRules(adminContent, editRecord), allowAdminTabs))
                            Call Stream.Add(GetForm_Edit_AddTab("Control&nbsp;Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                            If allowAdminTabs Then
                                Call Stream.Add(cpCore.html.menu_GetComboTabs())
                                'Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                            End If
                            Call Stream.Add(EditSectionButtonBar)
                        End If
                    '
                    Case "CCPAGECONTENT"
                        '
                        ' Page Content
                        '
                        TableID = cpCore.db.getRecordID("Tables", "ccPageContent")
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, (Not IsLandingPage) And (Not IsLandingPageParent) And AllowDelete, allowSave, AllowAdd, True)
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeletePage)
                        Call Stream.Add(EditSectionButtonBar)
                        Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                        Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, IsLandingPage Or IsLandingPageParent, IsRootPage, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                        'Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Link Aliases", GetForm_Edit_LinkAliases(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("RSS/Podcasts", GetForm_Edit_RSSFeeds(EditRecord.ContentName, EditRecord.ContentID, EditRecord.ID, cpCore.main_GetPageLink(EditRecord.ID)), AllowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                        If allowAdminTabs Then
                            Call Stream.Add(cpCore.html.menu_GetComboTabs())
                        End If
                        Call Stream.Add(EditSectionButtonBar)
                    'Case "CCSECTIONS"
                    '    '
                    '    ' Site Sections
                    '    '
                    '    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, (Not IsLandingSection) And AllowDelete, allowSave, AllowAdd)
                    '    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    '    Call Stream.Add(EditSectionButtonBar)
                    '    Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    '    Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, IsLandingSection, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    '    Call Stream.Add(GetForm_Edit_AddTab("Select Menus", GetForm_Edit_SectionDynamicMenuRules(adminContent, editRecord), allowAdminTabs))
                    '    Call Stream.Add(GetForm_Edit_AddTab("Section Blocking", GetForm_Edit_SectionBlockRules(adminContent, editRecord), allowAdminTabs))
                    '    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                    '    If allowAdminTabs Then
                    '        Call Stream.Add(cpCore.htmlDoc.menu_GetComboTabs())
                    '        'Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                    '    End If
                    '    Call Stream.Add(EditSectionButtonBar)
                    'Case "CCDYNAMICMENUS"
                    '    '
                    '    ' Edit Dynamic Sections
                    '    '
                    '    EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                    '    EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    '    Call Stream.Add(EditSectionButtonBar)
                    '    Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    '    Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                    '    Call Stream.Add(GetForm_Edit_AddTab("Select Sections", GetForm_Edit_DynamicMenuSectionRules(adminContent, editRecord), allowAdminTabs))
                    '    Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                    '    If allowAdminTabs Then
                    '        Call Stream.Add(cpCore.htmlDoc.menu_GetComboTabs())
                    '        'Call Stream.Add("<div class=""ccPanelBackground"">" & cpCore.main_GetComboTabs() & "</div>")
                    '    End If
                    '    Call Stream.Add(EditSectionButtonBar)
                    Case "CCLIBRARYFOLDERS"
                        '
                        ' Library Folders
                        '
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                        Call Stream.Add(EditSectionButtonBar)
                        Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                        Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                        Call Stream.Add(GetForm_Edit_AddTab("Authoring Access", GetForm_Edit_LibraryFolderRules(adminContent, editRecord), allowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                        If allowAdminTabs Then
                            Call Stream.Add(cpCore.html.menu_GetComboTabs())
                        End If
                        Call Stream.Add(EditSectionButtonBar)
                    Case genericController.vbUCase("ccGroups")
                        'Case "CCGROUPS"
                        '
                        ' Groups
                        '
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                        Call Stream.Add(EditSectionButtonBar)
                        Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                        Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                        Call Stream.Add(GetForm_Edit_AddTab("Authoring Permissions", GetForm_Edit_ContentGroupRules(adminContent, editRecord), allowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("Topics", GetForm_Edit_TopicRules, AllowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs))
                        'Call Stream.Add(GetForm_Edit_AddTab("Calendar", GetForm_Edit_CalendarEvents, AllowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                        If allowAdminTabs Then
                            Call Stream.Add(cpCore.html.menu_GetComboTabs())
                        End If
                        Call Stream.Add(EditSectionButtonBar)
                    '
                    ' This is the beginnings of a good idea. use a selector string to create the value input. The problem is
                    ' both the selector and value appear on the same page. if you screw up the selector, you can not save it
                    ' again without also saving the 'bad' value it creates.
                    '
                    ' For now, skip this and put the higher-level interface in control pages (an add-on type).
                    '
                    '        Case "CCSETUP"
                    '            '
                    '            '   Site Properties
                    '            '
                    '            EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord,)
                    '            EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                    '            Call Stream.Add(EditSectionButtonBar)
                    '            Call Stream.Add(Adminui.GetTitleBar( GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                    '            Call Stream.Add(GetForm_Edit_UserFieldTabs(adminContent, editRecord,FormID, editrecord.read_only, False, False, ContentType, AllowAjaxTabs))
                    '            Call Stream.Add(GetForm_Edit_AddTab("Site Property", GetForm_Edit_SiteProperties(FormID), AllowAdminTabs))
                    '            Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editrecord), AllowAdminTabs))
                    '            If AllowAdminTabs Then
                    '                Call Stream.Add(cpCore.main_GetComboTabs())
                    '            End If
                    '            Call Stream.Add(EditSectionButtonBar)
                    Case "CCLAYOUTS"
                        '
                        ' LAYOUTS
                        '
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                        Call Stream.Add(EditSectionButtonBar)
                        Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                        Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                        Call Stream.Add(GetForm_Edit_AddTab("Reports", GetForm_Edit_LayoutReports(adminContent, editRecord), allowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                        If allowAdminTabs Then
                            Call Stream.Add(cpCore.html.menu_GetComboTabs())
                        End If
                        Call Stream.Add(EditSectionButtonBar)
                    Case Else
                        '
                        ' All other tables (User definined)
                        '
                        EditSectionButtonBar = GetForm_Edit_ButtonBar(adminContent, editRecord, AllowDelete, allowSave, AllowAdd)
                        EditSectionButtonBar = genericController.vbReplace(EditSectionButtonBar, ButtonDelete, ButtonDeleteRecord)
                        Call Stream.Add(EditSectionButtonBar)
                        Call Stream.Add(Adminui.GetTitleBar(GetForm_EditTitle(adminContent, editRecord), HeaderDescription))
                        Call Stream.Add(GetForm_Edit_Tabs(adminContent, editRecord, editRecord.Read_Only, False, False, ContentType, AllowajaxTabs, TemplateIDForStyles, fieldTypeDefaultEditors, fieldEditorPreferencesList, styleList, styleOptionList, emailIdForStyles, IsTemplateTable, editorAddonListJSON))
                        'Call Stream.Add(GetForm_Edit_AddTab("Meta Content", GetForm_Edit_MetaContent(adminContent, editRecord, editRecord.Read_Only), allowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Content Watch", GetForm_Edit_ContentTracking(adminContent, editRecord), allowAdminTabs))
                        Call Stream.Add(GetForm_Edit_AddTab("Control Info", GetForm_Edit_Control(adminContent, editRecord), allowAdminTabs))
                        If allowAdminTabs Then
                            Call Stream.Add(cpCore.html.menu_GetComboTabs())
                        End If
                        Call Stream.Add(EditSectionButtonBar)
                End Select
                Call Stream.Add("</form>")
                returnHtml = Stream.Text
                If editRecord.id = 0 Then
                    Call cpCore.html.addTitle("Add " & adminContent.Name)
                ElseIf editRecord.nameLc = "" Then
                    Call cpCore.html.addTitle("Edit #" & editRecord.id & " in " & editRecord.contentControlId_Name)
                Else
                    Call cpCore.html.addTitle("Edit " & editRecord.nameLc & " in " & editRecord.contentControlId_Name)
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        ''
        ''========================================================================
        '' Print the DHTMLEdit form
        ''========================================================================
        ''
        'Private Function GetForm_EditHTML() As String
        '    On Error GoTo ErrorTrap: 'Dim th as integer: th = profileLogAdminMethodEnter("AdminClass.GetForm_EditHTML")
        '    '
        '    Dim WhereCount as integer
        '    Dim FastString As fastStringClass
        '    '
        '    FastString = New FastStringClass
        '    FastString.Add( cpCore.main_GetFormInputHidden(RequestNameAdminDepth, MenuDepth))
        '    FastString.Add( cpCore.main_GetFormInputHidden(RequestNameTitleExtension, TitleExtension))
        '    If WherePairCount > 0 Then
        '        For WhereCount = 0 To WherePairCount - 1
        '            FastString.Add( cpCore.main_GetFormInputHidden("wl" & WhereCount, WherePair(0, WhereCount)))
        '            FastString.Add( cpCore.main_GetFormInputHidden("wr" & WhereCount, WherePair(1, WhereCount)))
        '            Next
        '        End If
        '    GetForm_EditHTML = cpCore.main_GetActiveEditor(AdminContent.Name, EditRecord.ID, InputFieldName, FastString.Text)
        '    FastString = Nothing
        '    '
        '    '''Dim th as integer: Exit Function
        '    '
        '    ' ----- Error Trap
        '    '
        'ErrorTrap:
        '    FastString = Nothing
        '    Call HandleClassTrapErrorBubble("PrintDHTMLEditForm")
        '    '
        'End Function
        '        '
        '        '========================================================================
        '        ' Print the DHTMLEdit form
        '        '========================================================================
        '        '
        '        Private Function GetForm_StaticPublishControl() As String
        '            On Error GoTo ErrorTrap ''Dim th as integer : th = profileLogAdminMethodEnter( "GetForm_StaticPublishControl")
        '            '
        '            Dim WhereCount As Integer
        '            Dim Content As New stringBuilderLegacyController
        '            Dim EDGPublishNow As Boolean
        '            Dim Activity As String
        '            Dim TargetDomain As String
        '            Dim EDGCreateSnapShot As Boolean
        '            Dim EDGPublishToProduction As Boolean
        '            Dim CSPointer As Integer
        '            Dim Copy As String
        '            Dim StagingServer As String
        '            Dim PagesFound As Integer
        '            Dim PagesComplete As Integer
        '            Dim SQL As String
        '            Dim Button As String
        '            Dim EDGAuthUsername As String
        '            Dim EDGAuthPassword As String
        '            Dim QueryString As String
        '            Dim Adminui As New adminUIController(cpCore)
        '            Dim Description As String
        '            Dim ButtonList As String = ""
        '            '
        '            Button = cpCore.docProperties.getText(RequestNameButton)
        '            If Button = ButtonCancel Then
        '                '
        '                '
        '                '
        '                Return cpCore.webServer.redirect("/" & cpCore.serverConfig.appConfig.adminRoute, "StaticPublishControl, Cancel Button Pressed")
        '            ElseIf Not cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) Then
        '                '
        '                '
        '                '
        '                ButtonList = ButtonCancel
        '                Content.Add(Adminui.GetFormBodyAdminOnly())
        '            Else
        '                Content.Add(Adminui.EditTableOpen)
        '                '
        '                ' Set defaults
        '                '
        '                EDGCreateSnapShot = (cpCore.siteProperties.getBoolean("EDGCreateSnapShot", True))
        '                EDGPublishToProduction = (cpCore.siteProperties.getBoolean("EDGPublishToProduction", True))
        '                EDGPublishNow = (cpCore.siteProperties.getBoolean("EDGPublishNow"))
        '                EDGAuthUsername = cpCore.siteProperties.getText("EDGAuthUsername", "")
        '                EDGAuthPassword = cpCore.siteProperties.getText("EDGAuthPassword", "")
        '                '
        '                ' Process Requests
        '                '
        '                Select Case Button
        '                    Case ButtonBegin
        '                        '
        '                        ' Save form values
        '                        '
        '                        EDGAuthUsername = cpCore.docProperties.getText("EDGAuthUsername")
        '                        Call cpCore.siteProperties.setProperty("EDGAuthUsername", EDGAuthUsername)
        '                        '
        '                        EDGAuthPassword = cpCore.docProperties.getText("EDGAuthPassword")
        '                        Call cpCore.siteProperties.setProperty("EDGAuthPassword", EDGAuthPassword)
        '                        '
        '                        EDGCreateSnapShot = cpCore.docProperties.getBoolean("EDGCreateSnapShot")
        '                        Call cpCore.siteProperties.setProperty("EDGCreateSnapShot", genericController.encodeText(EDGCreateSnapShot))
        '                        '
        '                        EDGPublishToProduction = cpCore.docProperties.getBoolean("EDGPublishToProduction")
        '                        Call cpCore.siteProperties.setProperty("EDGPublishToProduction", genericController.encodeText(EDGPublishToProduction))
        '                        '
        '                        ' Begin Publish
        '                        '
        '                        EDGPublishNow = (EDGCreateSnapShot Or EDGPublishToProduction)
        '                        Call cpCore.siteProperties.setProperty("EDGPublishNow", genericController.encodeText(EDGPublishNow))
        '                    Case ButtonAbort
        '                        '
        '                        ' Abort Publish
        '                        '
        '                        EDGPublishNow = False
        '                        Call cpCore.siteProperties.setProperty("EDGPublishNow", genericController.encodeText(EDGPublishNow))
        '                    Case ButtonRefresh
        '                        '
        '                        ' Refresh (no action)
        '                        '
        '                End Select
        '                '
        '                ' ----- Status
        '                '
        '                If EDGPublishNow Then
        '                    Copy = "Started"
        '                Else
        '                    Copy = "Stopped"
        '                End If
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Status", "", False, False, ""))
        '                '
        '                ' ----- activity
        '                '
        '                Copy = genericController.encodeText(cpCore.siteProperties.getText("EDGPublishStatus", "Waiting"))
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Activity", "", False, False, ""))
        '                '
        '                ' ----- Pages Found
        '                '
        '                Copy = "n/a"
        '                SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs;"
        '                CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
        '                If cpCore.db.csOk(CSPointer) Then
        '                    Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
        '                End If
        '                Call cpCore.db.csClose(CSPointer)
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Links Found", "", False, False, ""))
        '                '
        '                ' ----- Pages Complete
        '                '
        '                Copy = "n/a"
        '                SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs where (UpToDate=1);"
        '                CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
        '                If cpCore.db.csOk(CSPointer) Then
        '                    Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
        '                End If
        '                Call cpCore.db.csClose(CSPointer)
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Pages Complete", "", False, False, ""))
        '                '
        '                ' ----- Bad Links
        '                '
        '                Copy = "n/a"
        '                QueryString = genericController.ModifyQueryString(cpCore.doc.refreshQueryString, RequestNameAdminForm, AdminFormReports, True)
        '                QueryString = genericController.ModifyQueryString(QueryString, RequestNameReportForm, ReportFormEDGDocErrors, True)
        '                SQL = "SELECT Count(ccEDGPublishDocs.ID) AS PagesFound FROM ccEDGPublishDocs where (UpToDate=1) And (LinkAlias Is Not null) And ((HTTPResponse Is null) Or ((Not (HTTPResponse Like '% 200 %'))and (not (HTTPResponse like '% 302 %'))));"
        '                CSPointer = cpCore.db.csOpenSql_rev("Default", SQL)
        '                If cpCore.db.csOk(CSPointer) Then
        '                    Copy = genericController.encodeText(cpCore.db.csGetInteger(CSPointer, "PagesFound"))
        '                End If
        '                Call cpCore.db.csClose(CSPointer)
        '                Call Content.Add(Adminui.GetEditRow("<a href=""/" & genericController.encodeHTML(cpCore.serverConfig.appConfig.adminRoute & "?" & QueryString) & """ target=""_blank"">" & SpanClassAdminNormal & Copy & "</a>", "Bad Links", "", False, False, ""))
        '                '
        '                ' ----- Options
        '                '

        '                'Content.Add( "<tr><td align=""right"">" & SpanClassAdminSmall & "Options</span></td>")
        '                If EDGPublishNow Then
        '                    '
        '                    ' Publishing
        '                    '
        '                    Call Content.Add(Adminui.GetEditRow(genericController.main_GetYesNo(EDGCreateSnapShot), "Create Staging Snap-Shot", "", False, False, ""))
        '                    Call Content.Add(Adminui.GetEditRow(genericController.main_GetYesNo(EDGPublishToProduction), "Publish Snap-Shot to Production", "", False, False, ""))
        '                Else
        '                    '
        '                    ' Ready
        '                    '
        '                    Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputCheckBox2("EDGCreateSnapShot", EDGCreateSnapShot), "Create Staging Snap-Shot", "", False, False, ""))
        '                    Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputCheckBox2("EDGPublishToProduction", EDGPublishToProduction), "Publish Snap-Shot to Production", "", False, False, ""))
        '                End If
        '                '
        '                ' Username
        '                '
        '                Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputText2("EDGAuthUsername", EDGAuthUsername), "Username", "", False, False, ""))
        '                '
        '                ' Password
        '                '
        '                Call Content.Add(Adminui.GetEditRow(cpCore.html.html_GetFormInputText2("EDGAuthPassword", EDGAuthPassword), "Password", "", False, False, ""))
        '                '
        '                ' Seed Documents
        '                '
        '                Copy = ""
        '                CSPointer = cpCore.db.csOpen("EDG Publish Seeds")
        '                Do While cpCore.db.csOk(CSPointer)
        '                    If Copy <> "" Then
        '                        Copy = Copy & "<br>"
        '                    End If
        '                    Copy = Copy & cpCore.db.csGetRecordEditLink(CSPointer) & cpCore.db.csGet(CSPointer, "Name")
        '                    cpCore.db.csGoNext(CSPointer)
        '                Loop
        '                Call cpCore.db.csClose(CSPointer)
        '                Copy = Copy & "<br>" & cpCore.html.main_cs_getRecordAddLink(CSPointer)
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Seed URLs", "", False, False, ""))
        '                '
        '                ' Production Servers
        '                '
        '                Copy = ""
        '                CSPointer = cpCore.db.csOpen("EDG Publish Servers")
        '                Do While cpCore.db.csOk(CSPointer)
        '                    If Copy <> "" Then
        '                        Copy = Copy & "<br>"
        '                    End If
        '                    Copy = Copy & cpCore.db.csGetRecordEditLink(CSPointer) & cpCore.db.csGet(CSPointer, "Name")
        '                    cpCore.db.csGoNext(CSPointer)
        '                Loop
        '                Call cpCore.db.csClose(CSPointer)
        '                'If cpCore.visitProperty_AllowEditing Then
        '                '    If Copy <> "" Then
        '                '        'Copy = Copy & "<br>"
        '                '        End If
        '                Copy = Copy & "<br>" & cpCore.html.main_cs_getRecordAddLink(CSPointer)
        '                '    End If
        '                Call Content.Add(Adminui.GetEditRow(Copy, "Production Servers", "", False, False, ""))
        '                '
        '                ' Buttons
        '                '
        '                If Not EDGPublishNow Then
        '                    ButtonList = ButtonBegin
        '                Else
        '                    ButtonList = ButtonAbort & "," & ButtonRefresh
        '                End If
        '                '
        '                Content.Add(Adminui.EditTableClose)
        '                Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEDGControl))
        '            End If
        '            '
        '            Description = "Static Publishing lets you create a completely static version of your website on remote servers. Some dynamic features such as personalization will not work on a static site."
        '            GetForm_StaticPublishControl = Adminui.GetBody("Static Publish Control", ButtonList, "", True, True, Description, "", 0, Content.Text)
        '            Content = Nothing
        '            '
        '            Exit Function
        '            '
        '            ' ----- Error Trap
        '            '
        'ErrorTrap:
        '            Content = Nothing
        '            Call handleLegacyClassError3("PrintDHTMLEditForm")
        '            '
        '        End Function
        '
    End Class
End Namespace
