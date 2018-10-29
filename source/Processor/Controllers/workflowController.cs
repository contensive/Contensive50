
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Controllers {
    public class WorkflowController : IDisposable {
        //
        //------------------------------------------------------------------------------------------------------------------------
        // objects passed in that are not disposed
        //------------------------------------------------------------------------------------------------------------------------
        //
        private CoreController core;
        //
        //------------------------------------------------------------------------------------------------------------------------
        // objects created within class to dispose in dispose
        //   typically used as app.meaData.method()
        //------------------------------------------------------------------------------------------------------------------------
        //
        //------------------------------------------------------------------------------------------------------------------------
        // internal storage
        //   these objects is deserialized during constructor
        //   appConfig has static setup values like file system endpoints and Db connection string
        //   appProperties has dynamic values and is serialized and saved when changed
        //   include properties saved in appConfig file, settings not editable online.
        //------------------------------------------------------------------------------------------------------------------------
        //
        public const bool csv_AllowAutocsv_ClearContentTimeStamp = true;
        //
        private struct EditLockType {
            public string Key;
            public int MemberID;
            public DateTime DateExpires;
        }
        private EditLockType[] EditLockArray;
        private int EditLockCount;
        //
        //-----------------------------------------------------------------------
        // ----- Edit Lock info
        //-----------------------------------------------------------------------
        //
        private string main_EditLockContentRecordKey_Local = "";
        private bool main_EditLockStatus_Local = false;
        private int main_EditLockMemberID_Local = 0;
        private string main_EditLockMemberName_Local = "";
        private DateTime main_EditLockDateExpires_Local = DateTime.MinValue;
        //
        //==========================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="cluster"></param>
        /// <param name="appName"></param>
        /// <remarks></remarks>
        public WorkflowController(CoreController core) : base() {
            try {
                //
                this.core = core;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
                throw (ex);
            }
        }
        //
        //==========================================================================================
        //
        //
        //========================================================================
        //   Returns true if the record is locked to the current member
        //========================================================================
        //
        public bool GetEditLockStatus(string ContentName, int RecordID) {
            try {
                //
                //If Not (true) Then Exit Function
                //
                int ReturnMemberID = 0;
                DateTime ReturnDateExpires = default(DateTime);
                //
                main_EditLockContentRecordKey_Local = (ContentName + GenericController.encodeText(RecordID));
                main_EditLockDateExpires_Local = DateTime.MinValue;
                main_EditLockMemberID_Local = 0;
                main_EditLockMemberName_Local = "";
                main_EditLockStatus_Local = false;
                //
                main_EditLockStatus_Local = getEditLock(GenericController.encodeText(ContentName), GenericController.encodeInteger(RecordID), ref ReturnMemberID, ref ReturnDateExpires);
                if (main_EditLockStatus_Local && (ReturnMemberID != core.session.user.id)) {
                    main_EditLockStatus_Local = true;
                    main_EditLockDateExpires_Local = ReturnDateExpires;
                    main_EditLockMemberID_Local = ReturnMemberID;
                    main_EditLockMemberName_Local = "";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return main_EditLockStatus_Local;
        }
        //
        //========================================================================
        //   Edit Lock Handling
        //========================================================================
        //
        public string GetEditLockMemberName(string ContentName, int RecordID) {
            string tempGetEditLockMemberName = null;
            try {
                int CS = 0;
                //
                if (main_EditLockContentRecordKey_Local != (ContentName + GenericController.encodeText(RecordID))) {
                    GetEditLockStatus(ContentName, RecordID);
                }
                if (main_EditLockStatus_Local) {
                    if (string.IsNullOrEmpty(main_EditLockMemberName_Local)) {
                        if (main_EditLockMemberID_Local != 0) {
                            CS = core.db.csOpen2("people", main_EditLockMemberID_Local);
                            if (core.db.csOk(CS)) {
                                main_EditLockMemberName_Local = core.db.csGetText(CS, "name");
                            }
                            core.db.csClose(ref CS);
                        }
                        if (string.IsNullOrEmpty(main_EditLockMemberName_Local)) {
                            main_EditLockMemberName_Local = "unknown";
                        }
                    }
                    tempGetEditLockMemberName = main_EditLockMemberName_Local;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return main_EditLockMemberName_Local;
        }
        //
        //========================================================================
        //   Edit Lock Handling
        //========================================================================
        //
        public DateTime GetEditLockDateExpires(string ContentName, int RecordID) {
            DateTime returnDate = DateTime.MinValue;
            try {
                if (main_EditLockContentRecordKey_Local != (ContentName + GenericController.encodeText(RecordID))) {
                    GetEditLockStatus(ContentName, RecordID);
                }
                if (main_EditLockStatus_Local) {
                    returnDate = main_EditLockDateExpires_Local;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnDate;
        }
        //
        //========================================================================
        //   Sets the edit lock for this record
        //========================================================================
        //
        public void SetEditLock(string ContentName, int RecordID) {
            setEditLock(GenericController.encodeText(ContentName), GenericController.encodeInteger(RecordID), core.session.user.id);
        }
        //
        //========================================================================
        //   Clears the edit lock for this record
        //========================================================================
        //
        public void ClearEditLock(string ContentName, int RecordID) {
            clearEditLock(GenericController.encodeText(ContentName), GenericController.encodeInteger(RecordID), core.session.user.id);
        }
        //
        //========================================================================
        //   Aborts any edits for this record
        //========================================================================
        //
        //Public Sub publishEdit(ByVal ContentName As String, ByVal RecordID As Integer)
        //    Call publishEdit(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID), core.doc.authContext.user.id)
        //End Sub
        //
        //========================================================================
        //   Approves any edits for this record
        //========================================================================
        //
        //Public Sub approveEdit(ByVal ContentName As String, ByVal RecordID As Integer)
        //    Call approveEdit(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID), core.doc.authContext.user.id)
        //End Sub
        //
        //========================================================================
        //   Submits any edits for this record
        //========================================================================
        //
        //Public Sub main_SubmitEdit(ByVal ContentName As String, ByVal RecordID As Integer)
        //    Call submitEdit2(genericController.encodeText(ContentName), genericController.EncodeInteger(RecordID), core.doc.authContext.user.id)
        //End Sub
        //
        //=========================================================================================
        //   Determine if this Content Definition is run on a table that supports
        //   Workflow authoring. Only the ContentTable is checked, it is assumed that the
        //   AuthoringTable is a copy of the ContentTable
        //=========================================================================================
        //
        //Public Function isWorkflowAuthoringCompatible(ByVal ContentName As String) As Boolean
        //    isWorkflowAuthoringCompatible = genericController.EncodeBoolean(core.metaData.GetContentProperty(genericController.encodeText(ContentName), "ALLOWWORKFLOWAUTHORING"))
        //End Function
        //
        //==========================================================================================
        //==========================================================================================
        //
        //
        //=================================================================================
        //
        //=================================================================================
        //
        //Public Sub publishEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
        //    Try
        //        '
        //        Dim RSLive As DataTable
        //        Dim LiveRecordID As Integer
        //        Dim LiveDataSourceName As String
        //        Dim LiveTableName As String
        //        Dim LiveSQLValue As String
        //        Dim LiveRecordBlank As Boolean
        //        '
        //        Dim BlankSQLValue As String
        //        '
        //        Dim RSEdit As DataTable
        //        Dim EditDataSourceName As String
        //        Dim EditTableName As String
        //        Dim EditRecordID As Integer
        //        Dim EditSQLValue As String
        //        Dim EditFilename As String
        //        Dim EditRecordCID As Integer
        //        Dim EditRecordBlank As Boolean
        //        '
        //        Dim NewEditRecordID As Integer
        //        Dim NewEditFilename As String
        //        '
        //        Dim ContentID As Integer
        //        '
        //        Dim FieldNameArray() As String
        //        Dim ArchiveSqlFieldList As New sqlFieldListClass
        //        Dim NewEditSqlFieldList As New sqlFieldListClass
        //        Dim PublishFieldNameArray() As String
        //        Dim PublishSqlFieldList As New sqlFieldListClass
        //        Dim BlankSqlFieldList As New sqlFieldListClass
        //        '
        //        Dim FieldPointer As Integer
        //        Dim FieldCount As Integer
        //        Dim FieldName As String
        //        Dim fieldTypeId As Integer
        //        Dim SQL As String
        //        Dim MethodName As String
        //        Dim FieldArraySize As Integer
        //        Dim PublishingDelete As Boolean
        //        Dim PublishingInactive As Boolean
        //        Dim CDef As Models.Complex.cdefModel
        //        Dim FieldList As String
        //        '
        //        MethodName = "csv_PublishEdit"
        //        '
        //        CDef = Models.Complex.cdefModel.getcdef(core,ContentName)
        //        If CDef.Id > 0 Then
        //            If false And core.siteProperties.allowWorkflowAuthoring Then
        //                With CDef
        //                    FieldList = .SelectCommaList
        //                    LiveDataSourceName = .ContentDataSourceName
        //                    LiveTableName = .ContentTableName
        //                    EditDataSourceName = .AuthoringDataSourceName
        //                    EditTableName = .AuthoringTableName
        //                    FieldCount = .fields.Count
        //                    ContentID = .Id
        //                    ContentName = .Name
        //                End With
        //                FieldArraySize = FieldCount + 6
        //                ReDim FieldNameArray(FieldArraySize)
        //                'ReDim ArchiveSqlFieldList(FieldArraySize)
        //                'ReDim NewEditSqlFieldList(FieldArraySize)
        //                'ReDim BlankSqlFieldList(FieldArraySize)
        //                'ReDim PublishSqlFieldList(FieldArraySize)
        //                ReDim PublishFieldNameArray(FieldArraySize)
        //                LiveRecordID = RecordID
        //                '
        //                ' ----- Open the live record
        //                '
        //                RSLive = core.db.executeSql("SELECT " & FieldList & " FROM " & LiveTableName & " WHERE ID=" & core.db.encodeSQLNumber(LiveRecordID) & ";", LiveDataSourceName)
        //                'RSLive = appservices.core.db.executeSql(LiveDataSourceName, "SELECT " & FieldList & " FROM " & LiveTableName & " WHERE ID=" & encodeSQLNumber(LiveRecordID) & ";")
        //                If RSLive.Rows.Count <= 0 Then
        //                    '
        //                    Throw (New ApplicationException("During record publishing, there was an error opening the live record, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
        //                Else
        //                    If True Then
        //                        LiveRecordID = genericController.EncodeInteger(core.db.getDataRowColumnName(RSLive.Rows[0], "ID"))
        //                        LiveRecordBlank = genericController.EncodeBoolean(core.db.getDataRowColumnName(RSLive.Rows[0], "EditBlank"))
        //                        '
        //                        ' ----- Open the edit record
        //                        '
        //                        RSEdit = core.db.executeSql("SELECT " & FieldList & " FROM " & EditTableName & " WHERE (EditSourceID=" & core.db.encodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;", EditDataSourceName)
        //                        'RSEdit = appservices.core.db.executeSql(EditDataSourceName, "SELECT " & FieldList & " FROM " & EditTableName & " WHERE (EditSourceID=" & encodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;")
        //                        If RSEdit.Rows.Count <= 0 Then
        //                            '
        //                            Throw (New ApplicationException("During record publishing, there was an error opening the edit record [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
        //                        Else
        //                            If True Then
        //                                EditRecordID = genericController.EncodeInteger(core.db.getDataRowColumnName(RSEdit.Rows[0], "ID"))
        //                                EditRecordCID = genericController.EncodeInteger(core.db.getDataRowColumnName(RSEdit.Rows[0], "ContentControlID"))
        //                                EditRecordBlank = genericController.EncodeBoolean(core.db.getDataRowColumnName(RSEdit.Rows[0], "EditBlank"))
        //                                PublishingDelete = EditRecordBlank
        //                                PublishingInactive = Not genericController.EncodeBoolean(core.db.getDataRowColumnName(RSEdit.Rows[0], "active"))
        //                                '
        //                                ' ----- Create new Edit record
        //                                '
        //                                If Not PublishingDelete Then
        //                                    NewEditRecordID = core.db.insertTableRecordGetId(EditDataSourceName, EditTableName, SystemMemberID)
        //                                    If NewEditRecordID < 1 Then
        //                                        '
        //                                        Throw (New ApplicationException("During record publishing, a new edit record could not be create, table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
        //                                    End If
        //                                End If
        //                                If True Then
        //                                    '
        //                                    ' ----- create update arrays
        //                                    '
        //                                    FieldPointer = 0
        //                                    For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In CDef.fields
        //                                        Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
        //                                        With field
        //                                            FieldName = .nameLc
        //                                            fieldTypeId = .fieldTypeId
        //                                            Select Case fieldTypeId
        //                                                Case FieldTypeIdManyToMany, FieldTypeIdRedirect
        //                                                    '
        //                                                    ' These content fields have no Db Field
        //                                                    '
        //                                                Case Else
        //                                                    '
        //                                                    ' Process These field types
        //                                                    '
        //                                                    Select Case genericController.vbUCase(FieldName)
        //                                                        Case "ID", "CONTENTCONTROLID"  
        //                                                        '
        //                                                        ' ----- control fields that should not be in any dataset
        //                                                        '
        //                                                        Case "MODIFIEDDATE", "MODIFIEDBY"
        //                                                            '
        //                                                            ' ----- non-content fields that need to be in all datasets
        //                                                            '       add manually later, in case they are not in ContentDefinition
        //                                                            '
        //                                                        Case Else
        //                                                            '
        //                                                            ' ----- Content related field
        //                                                            '
        //                                                            LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                                            EditSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName), fieldTypeId)
        //                                                            BlankSQLValue = core.db.EncodeSQL(Nothing, fieldTypeId)
        //                                                            FieldNameArray(FieldPointer) = FieldName
        //                                                            '
        //                                                            ' ----- New Edit Record value
        //                                                            '
        //                                                            If Not PublishingDelete Then
        //                                                                Select Case fieldTypeId
        //                                                                    Case FieldTypeIdFileCSS, FieldTypeIdFileXML, FieldTypeIdFileJavascript
        //                                                                        '
        //                                                                        ' ----- cdn files - create copy of File for neweditrecord
        //                                                                        '
        //                                                                        EditFilename = genericController.encodeText(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName))
        //                                                                        If EditFilename = "" Then
        //                                                                            NewEditSqlFieldList.add(FieldName, core.db.encodeSQLText(""))
        //                                                                        Else
        //                                                                            NewEditFilename = fileController.getVirtualRecordPathFilename(EditTableName, FieldName, NewEditRecordID, fieldTypeId)
        //                                                                            'NewEditFilename = csv_GetVirtualFilename(ContentName, FieldName, NewEditRecordID)
        //                                                                            Call core.cdnFiles.copyFile(EditFilename, NewEditFilename)
        //                                                                            NewEditSqlFieldList.add(FieldName, core.db.encodeSQLText(NewEditFilename))
        //                                                                        End If
        //                                                                    Case FieldTypeIdFileText, FieldTypeIdFileHTML
        //                                                                        '
        //                                                                        ' ----- private files - create copy of File for neweditrecord
        //                                                                        '
        //                                                                        EditFilename = genericController.encodeText(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName))
        //                                                                        If EditFilename = "" Then
        //                                                                            NewEditSqlFieldList.add(FieldName, core.db.encodeSQLText(""))
        //                                                                        Else
        //                                                                            NewEditFilename = fileController.getVirtualRecordPathFilename(EditTableName, FieldName, NewEditRecordID, fieldTypeId)
        //                                                                            'NewEditFilename = csv_GetVirtualFilename(ContentName, FieldName, NewEditRecordID)
        //                                                                            Call core.cdnFiles.copyFile(EditFilename, NewEditFilename)
        //                                                                            NewEditSqlFieldList.add(FieldName, core.db.encodeSQLText(NewEditFilename))
        //                                                                        End If
        //                                                                    Case Else
        //                                                                        '
        //                                                                        ' ----- put edit value in new edit record
        //                                                                        '
        //                                                                        NewEditSqlFieldList.add(FieldName, EditSQLValue)
        //                                                                End Select
        //                                                            End If
        //                                                            '
        //                                                            ' ----- set archive value
        //                                                            '
        //                                                            ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
        //                                                            '
        //                                                            ' ----- set live record value (and name)
        //                                                            '
        //                                                            If PublishingDelete Then
        //                                                                '
        //                                                                ' ----- Record delete - fill the live record with null
        //                                                                '
        //                                                                PublishFieldNameArray(FieldPointer) = FieldName
        //                                                                PublishSqlFieldList.add(FieldName, BlankSQLValue)
        //                                                            Else
        //                                                                PublishFieldNameArray(FieldPointer) = FieldName
        //                                                                PublishSqlFieldList.add(FieldName, EditSQLValue)
        //                                                            End If
        //                                                    End Select
        //                                            End Select
        //                                        End With
        //                                        FieldPointer += 1
        //                                    Next
        //                                    '
        //                                    ' ----- create non-content control field entries
        //                                    '
        //                                    FieldName = "MODIFIEDDATE"
        //                                    fieldTypeId = FieldTypeIdDate
        //                                    FieldNameArray(FieldPointer) = FieldName
        //                                    LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                    EditSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName), fieldTypeId)
        //                                    ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
        //                                    NewEditSqlFieldList.add(FieldName, EditSQLValue)
        //                                    PublishFieldNameArray(FieldPointer) = FieldName
        //                                    PublishSqlFieldList.add(FieldName, EditSQLValue)
        //                                    FieldPointer = FieldPointer + 1
        //                                    '
        //                                    FieldName = "MODIFIEDBY"
        //                                    fieldTypeId = FieldTypeIdLookup
        //                                    FieldNameArray(FieldPointer) = FieldName
        //                                    LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                    EditSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName), fieldTypeId)
        //                                    ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
        //                                    NewEditSqlFieldList.add(FieldName, EditSQLValue)
        //                                    PublishFieldNameArray(FieldPointer) = FieldName
        //                                    PublishSqlFieldList.add(FieldName, EditSQLValue)
        //                                    FieldPointer = FieldPointer + 1
        //                                    '
        //                                    FieldName = "CONTENTCONTROLID"
        //                                    fieldTypeId = FieldTypeIdLookup
        //                                    FieldNameArray(FieldPointer) = FieldName
        //                                    LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                    EditSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName), fieldTypeId)
        //                                    ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
        //                                    NewEditSqlFieldList.add(FieldName, EditSQLValue)
        //                                    PublishFieldNameArray(FieldPointer) = FieldName
        //                                    If PublishingDelete Then
        //                                        PublishSqlFieldList.add(FieldName, core.db.encodeSQLNumber(0))
        //                                    Else
        //                                        PublishSqlFieldList.add(FieldName, EditSQLValue)
        //                                    End If
        //                                    FieldPointer = FieldPointer + 1
        //                                    '
        //                                    FieldName = "EDITBLANK"
        //                                    fieldTypeId = FieldTypeIdBoolean
        //                                    FieldNameArray(FieldPointer) = FieldName
        //                                    LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                    EditSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSEdit.Rows[0], FieldName), fieldTypeId)
        //                                    ArchiveSqlFieldList.add(FieldName, LiveSQLValue)
        //                                    NewEditSqlFieldList.add(FieldName, EditSQLValue)
        //                                    PublishFieldNameArray(FieldPointer) = FieldName
        //                                    If PublishingDelete Then
        //                                        PublishSqlFieldList.add(FieldName, SQLFalse)
        //                                    Else
        //                                        PublishSqlFieldList.add(FieldName, EditSQLValue)
        //                                    End If

        //                                    '
        //                                    ' ----- copy edit record to live record
        //                                    '
        //                                    Call core.db.updateTableRecord(LiveDataSourceName, LiveTableName, "ID=" & LiveRecordID, PublishSqlFieldList)
        //                                    '
        //                                    ' ----- copy live record to archive record and the edit to the new edit
        //                                    '
        //                                    Call core.db.updateTableRecord(EditDataSourceName, EditTableName, "ID=" & EditRecordID, ArchiveSqlFieldList)
        //                                    If Not PublishingDelete Then
        //                                        Call core.db.updateTableRecord(EditDataSourceName, EditTableName, "ID=" & NewEditRecordID, NewEditSqlFieldList)
        //                                    End If
        //                                    '
        //                                    ' ----- Content Watch effects
        //                                    '
        //                                    If PublishingDelete Then
        //                                        '
        //                                        ' Record was deleted, delete contentwatch records also
        //                                        '
        //                                        Call core.db.deleteContentRules(ContentID, RecordID)
        //                                    End If
        //                                    '
        //                                    ' ----- mark the SpiderDocs record not up-to-date
        //                                    '
        //                                    If (LCase(EditTableName) = "ccpagecontent") And (LiveRecordID <> 0) Then
        //                                        If core.db.isSQLTableField("default", "ccSpiderDocs", "PageID") Then
        //                                            SQL = "UPDATE ccspiderdocs SET UpToDate = 0 WHERE PageID=" & LiveRecordID
        //                                            Call core.db.executeSql(SQL)
        //                                        End If
        //                                    End If
        //                                    '
        //                                    ' ----- Clear Time Stamp because a record changed
        //                                    '
        //                                    If csv_AllowAutocsv_ClearContentTimeStamp Then
        //                                        Call core.cache.invalidateObject(ContentName)
        //                                    End If
        //                                End If
        //                            End If
        //                            'RSEdit.Close()
        //                        End If
        //                        'RSEdit = Nothing
        //                    End If
        //                    'RSLive.Close()
        //                End If
        //                RSLive.Dispose()
        //                '
        //                ' ----- Clear all Authoring Controls
        //                '
        //                Call clearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsEditing, MemberID)
        //                Call clearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsModified, MemberID)
        //                Call clearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsApproved, MemberID)
        //                Call clearAuthoringControl(ContentName, LiveRecordID, AuthoringControlsSubmitted, MemberID)
        //                '
        //                '
        //                '
        //                If PublishingDelete Or PublishingInactive Then
        //                    Call core.db.deleteContentRules(ContentID, LiveRecordID)
        //                End If
        //            End If
        //        End If
        //    Catch ex As Exception
        //        logController.handleException( core,ex); : Throw
        //    End Try
        //End Sub
        //
        //=================================================================================
        //
        //=================================================================================
        //
        //Public Sub abortEdit2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
        //    Try
        //        '
        //        Dim RSLive As DataTable
        //        Dim LiveRecordID As Integer
        //        Dim LiveDataSourceName As String
        //        Dim LiveTableName As String
        //        Dim LiveSQLValue As String
        //        Dim LiveFilename As String
        //        '
        //        Dim RSEdit As DataTable
        //        Dim EditDataSourceName As String
        //        Dim EditTableName As String
        //        Dim EditRecordID As Integer
        //        Dim EditFilename As String
        //        Dim ContentID As Integer
        //        Dim FieldPointer As Integer
        //        Dim FieldCount As Integer
        //        Dim FieldName As String
        //        Dim fieldTypeId As Integer
        //        Dim CDef As Models.Complex.cdefModel
        //        Dim sqlFieldList As New sqlFieldListClass
        //        '
        //        CDef = Models.Complex.cdefModel.getcdef(core,ContentName)
        //        If CDef.Id > 0 Then
        //            If false And core.siteProperties.allowWorkflowAuthoring Then
        //                With CDef
        //                    LiveDataSourceName = .ContentDataSourceName
        //                    LiveTableName = .ContentTableName
        //                    EditDataSourceName = .AuthoringDataSourceName
        //                    EditTableName = .AuthoringTableName
        //                    FieldCount = .fields.Count
        //                    ContentID = .Id
        //                    ContentName = .Name
        //                End With
        //                'ReDim sqlFieldList(FieldCount + 2)
        //                'ReDim PublishFieldNameArray(FieldCount + 2)
        //                LiveRecordID = RecordID
        //                ' LiveRecordID = appservices.csv_cs_getField(CSPointer, "ID")
        //                '
        //                ' Open the live record
        //                '
        //                RSLive = core.db.executeSql("SELECT * FROM " & LiveTableName & " WHERE ID=" & core.db.encodeSQLNumber(LiveRecordID) & ";", LiveDataSourceName)
        //                If (RSLive Is Nothing) Then
        //                    '
        //                    Throw (New ApplicationException("During record publishing, there was an error opening the live record, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
        //                Else
        //                    If RSLive.Rows.Count <= 0 Then
        //                        '
        //                        Throw (New ApplicationException("During record publishing, the live record could not be found, [ID=" & LiveRecordID & "] in table [" & LiveTableName & "] on datasource [" & LiveDataSourceName & " ]"))
        //                    Else
        //                        LiveRecordID = genericController.EncodeInteger(core.db.getDataRowColumnName(RSLive.Rows[0], "ID"))
        //                        '
        //                        ' Open the edit record
        //                        '
        //                        RSEdit = core.db.executeSql("SELECT * FROM " & EditTableName & " WHERE (EditSourceID=" & core.db.encodeSQLNumber(LiveRecordID) & ")and(EditArchive=0) Order By ID DESC;", EditDataSourceName)
        //                        If (RSEdit Is Nothing) Then
        //                            '
        //                            Throw (New ApplicationException("During record publishing, there was an error opening the edit record [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
        //                        Else
        //                            If RSEdit.Rows.Count <= 0 Then
        //                                '
        //                                Throw (New ApplicationException("During record publishing, the edit record could not be found, [EditSourceID=" & LiveRecordID & "] in table [" & EditTableName & "] on datasource [" & EditDataSourceName & " ]"))
        //                            Else
        //                                EditRecordID = genericController.EncodeInteger(core.db.getDataRowColumnName(RSEdit.Rows[0], "ID"))
        //                                '
        //                                ' create update arrays
        //                                '
        //                                FieldPointer = 0
        //                                For Each keyValuePair As KeyValuePair(Of String, Models.Complex.CDefFieldModel) In CDef.fields
        //                                    Dim field As Models.Complex.CDefFieldModel = keyValuePair.Value
        //                                    With field
        //                                        FieldName = .nameLc
        //                                        If core.db.isSQLTableField(EditDataSourceName, EditTableName, FieldName) Then
        //                                            fieldTypeId = .fieldTypeId
        //                                            LiveSQLValue = core.db.EncodeSQL(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName), fieldTypeId)
        //                                            Select Case genericController.vbUCase(FieldName)
        //                                                Case "ID"
        //                                                    '
        //                                                    '   block from dataset
        //                                                    '
        //                                                Case Else
        //                                                    '
        //                                                    ' allow only authorable fields
        //                                                    '
        //                                                    If (fieldTypeId = FieldTypeIdFileCSS) Or (fieldTypeId = FieldTypeIdFileJavascript) Or (fieldTypeId = FieldTypeIdFileXML) Then
        //                                                        '
        //                                                        '   cdnfiles - create copy of Live TextFile for Edit record
        //                                                        '
        //                                                        LiveFilename = genericController.encodeText(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName))
        //                                                        If LiveFilename <> "" Then
        //                                                            EditFilename = fileController.getVirtualRecordPathFilename(EditTableName, FieldName, EditRecordID, fieldTypeId)
        //                                                            Call core.cdnFiles.copyFile(LiveFilename, EditFilename)
        //                                                            LiveSQLValue = core.db.encodeSQLText(EditFilename)
        //                                                        End If
        //                                                    End If
        //                                                    If (fieldTypeId = FieldTypeIdFileText) Or (fieldTypeId = FieldTypeIdFileHTML) Then
        //                                                        '
        //                                                        '   pivatefiles - create copy of Live TextFile for Edit record
        //                                                        '
        //                                                        LiveFilename = genericController.encodeText(core.db.getDataRowColumnName(RSLive.Rows[0], FieldName))
        //                                                        If LiveFilename <> "" Then
        //                                                            EditFilename = fileController.getVirtualRecordPathFilename(EditTableName, FieldName, EditRecordID, fieldTypeId)
        //                                                            Call core.cdnFiles.copyFile(LiveFilename, EditFilename)
        //                                                            LiveSQLValue = core.db.encodeSQLText(EditFilename)
        //                                                        End If
        //                                                    End If
        //                                                    '
        //                                                    sqlFieldList.add(FieldName, LiveSQLValue)
        //                                            End Select
        //                                        End If
        //                                    End With
        //                                    FieldPointer += 1
        //                                Next
        //                            End If
        //                            Call RSEdit.Dispose()
        //                        End If
        //                        RSEdit = Nothing
        //                    End If
        //                    RSLive.Dispose()
        //                End If
        //                RSLive = Nothing
        //                '
        //                ' ----- copy live record to editrecord
        //                '
        //                Call core.db.updateTableRecord(EditDataSourceName, EditTableName, "ID=" & EditRecordID, sqlFieldList)
        //                '
        //                ' ----- Clear all authoring controls
        //                '
        //                Call clearAuthoringControl(ContentName, RecordID, AuthoringControlsModified, MemberID)
        //                Call clearAuthoringControl(ContentName, RecordID, AuthoringControlsSubmitted, MemberID)
        //                Call clearAuthoringControl(ContentName, RecordID, AuthoringControlsApproved, MemberID)
        //            End If
        //        End If
        //    Catch ex As Exception
        //        logController.handleException( core,ex); : Throw
        //    End Try
        //End Sub
        //
        //=================================================================================
        //
        //=================================================================================
        //
        //Public Sub approveEdit(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
        //    Try
        //        '
        //        Dim CDef As Models.Complex.cdefModel
        //        '
        //        CDef = Models.Complex.cdefModel.getcdef(core,ContentName)
        //        If CDef.Id > 0 Then
        //            If false And core.siteProperties.allowWorkflowAuthoring Then
        //                Call setAuthoringControl(ContentName, RecordID, AuthoringControlsApproved, MemberID)
        //            End If
        //        End If
        //    Catch ex As Exception
        //        logController.handleException( core,ex); : Throw
        //    End Try
        //End Sub
        //
        //=================================================================================
        //
        //=================================================================================
        //
        //Public Sub submitEdit2(ByVal ContentName As String, ByVal RecordID As Integer, ByVal MemberID As Integer)
        //    Try
        //        '
        //        Dim CDef As Models.Complex.cdefModel
        //        '
        //        CDef = Models.Complex.cdefModel.getcdef(core,ContentName)
        //        If CDef.Id > 0 Then
        //            If false And core.siteProperties.allowWorkflowAuthoring Then
        //                Call setAuthoringControl(ContentName, RecordID, AuthoringControlsSubmitted, MemberID)
        //            End If
        //        End If
        //    Catch ex As Exception
        //        logController.handleException( core,ex); : Throw
        //    End Try
        //End Sub
        //
        //=====================================================================================================
        //   returns true if another user has the record locked
        //=====================================================================================================
        //
        public bool isRecordLocked(string ContentName, int RecordID, int MemberID) {
            bool result = false;
            try {
                string Criteria = null;
                int CS = 0;
                //
                Criteria = getAuthoringControlCriteria(ContentName, RecordID) + "and(CreatedBy<>" + core.db.encodeSQLNumber(MemberID) + ")";
                CS = core.db.csOpen("Authoring Controls", Criteria,"", true, MemberID);
                result = core.db.csOk(CS);
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //=====================================================================================================
        //   returns true if another user has the record locked
        //=====================================================================================================
        //
        private string getAuthoringControlCriteria(string ContentName, int RecordID) {
            string result = "";
            try {
                int ContentCnt = 0;
                int CS = 0;
                string Criteria = "";
                int TableID;
                //
                TableID = core.db.getContentTableID(ContentName);
                //
                // Authoring Control records are referenced by ContentID
                //
                ContentCnt = 0;
                CS = core.db.csOpen("Content", "(contenttableid=" + TableID + ")");
                while (core.db.csOk(CS)) {
                    Criteria = Criteria + "," + core.db.csGetInteger(CS, "ID");
                    ContentCnt = ContentCnt + 1;
                    core.db.csGoNext(CS);
                }
                core.db.csClose(ref CS);
                if (ContentCnt < 1) {
                    //
                    // No references to this table
                    //
                    //throw (new ApplicationException("TableID [" + TableID + "] could not be found in any ccContent.ContentTableID"));
                    result = "(1=0)";
                } else if (ContentCnt == 1) {
                    //
                    // One content record
                    //
                    result = "(ContentID=" + core.db.encodeSQLNumber(encodeInteger(Criteria.Substring(1))) + ")And(RecordID=" + core.db.encodeSQLNumber(RecordID) + ")And((DateExpires>" + core.db.encodeSQLDate(DateTime.Now) + ")Or(DateExpires Is null))";
                } else {
                    //
                    // Multiple content records
                    //
                    result = "(ContentID In (" + Criteria.Substring(1) + "))And(RecordID=" + core.db.encodeSQLNumber(RecordID) + ")And((DateExpires>" + core.db.encodeSQLDate(DateTime.Now) + ")Or(DateExpires Is null))";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //=====================================================================================================
        //   Clear the Approved Authoring Control
        //=====================================================================================================
        //
        public void clearAuthoringControl(string ContentName, int RecordID, int AuthoringControl, int MemberID) {
            try {
                string Criteria;
                //
                Criteria = getAuthoringControlCriteria(ContentName, RecordID) + "And(ControlType=" + AuthoringControl + ")";
                switch (AuthoringControl) {
                    case AuthoringControlsEditing:
                        core.db.deleteContentRecords("Authoring Controls", Criteria + "And(CreatedBy=" + core.db.encodeSQLNumber(MemberID) + ")", MemberID);
                        break;
                    case AuthoringControlsSubmitted:
                    case AuthoringControlsApproved:
                    case AuthoringControlsModified:
                        core.db.deleteContentRecords("Authoring Controls", Criteria, MemberID);
                        break;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=====================================================================================================
        //   the Approved Authoring Control
        //=====================================================================================================
        //
        public void setRecordLocking(string ContentName, int RecordID, int AuthoringControl, int MemberID) {
            try {
                int ContentID = 0;
                string AuthoringCriteria = null;
                int CSNewLock = 0;
                int CSCurrentLock = 0;
                string sqlCriteria = null;
                double EditLockTimeoutDays = 0;
                double EditLockTimeoutMinutes = 0;
                Models.Domain.CDefModel CDef = null;
                //
                CDef = Models.Domain.CDefModel.create(core, ContentName);
                ContentID = CDef.id;
                if (ContentID != 0) {
                    AuthoringCriteria = getAuthoringControlCriteria(ContentName, RecordID);
                    switch (AuthoringControl) {
                        case AuthoringControlsEditing:
                            EditLockTimeoutMinutes = encodeNumber(core.siteProperties.getText("EditLockTimeout", "5"));
                            if (EditLockTimeoutMinutes != 0) {
                                sqlCriteria = AuthoringCriteria + "And(ControlType=" + AuthoringControlsEditing + ")";
                                EditLockTimeoutDays = (EditLockTimeoutMinutes / 60 / 24);
                                //
                                // Delete expired locks
                                core.db.deleteContentRecords("Authoring Controls", sqlCriteria + "And(DATEEXPIRES<" + core.db.encodeSQLDate(DateTime.Now) + ")");
                                //
                                // Select any lock left, only the newest counts
                                CSCurrentLock = core.db.csOpen("Authoring Controls", sqlCriteria, "ID DESC", false, MemberID, false, false);
                                if (!core.db.csOk(CSCurrentLock)) {
                                    //
                                    // No lock, create one
                                    CSNewLock = core.db.csInsertRecord("Authoring Controls", MemberID);
                                    if (core.db.csOk(CSNewLock)) {
                                        core.db.csSet(CSNewLock, "RecordID", RecordID);
                                        core.db.csSet(CSNewLock, "DateExpires", (DateTime.Now.AddDays(EditLockTimeoutDays)));
                                        core.db.csSet(CSNewLock, "ControlType", AuthoringControlsEditing);
                                        core.db.csSet(CSNewLock, "ContentRecordKey", GenericController.encodeText(ContentID + "." + RecordID));
                                        core.db.csSet(CSNewLock, "ContentID", ContentID);
                                    }
                                    core.db.csClose(ref CSNewLock);
                                } else {
                                    if (core.db.csGetInteger(CSCurrentLock, "CreatedBy") == MemberID) {
                                        //
                                        // Record Locked by Member, update DateExpire
                                        //
                                        core.db.csSet(CSCurrentLock, "DateExpires", (DateTime.Now.AddDays(EditLockTimeoutDays)));
                                    }
                                }
                                core.db.csClose(ref CSCurrentLock);
                            }
                            break;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=====================================================================================================
        //   the Approved Authoring Control
        //=====================================================================================================
        //
        public void getAuthoringStatus(string ContentName, int RecordID, ref bool IsSubmitted, ref bool IsApproved, ref string SubmittedName, ref string ApprovedName, ref bool IsInserted, ref bool IsDeleted, ref bool IsModified, ref string ModifiedName, ref DateTime ModifiedDate, ref DateTime SubmittedDate, ref DateTime ApprovedDate) {
            try {
                int ContentID = 0;
                Models.Domain.CDefModel CDef = null;
                //
                IsModified = false;
                ModifiedName = "";
                ModifiedDate = DateTime.MinValue;
                IsSubmitted = false;
                SubmittedName = "";
                SubmittedDate = DateTime.MinValue;
                IsApproved = false;
                ApprovedName = "";
                ApprovedDate = DateTime.MinValue;
                IsInserted = false;
                IsDeleted = false;
                if (RecordID > 0) {
                    //
                    // Get Workflow Locks
                    //
                    CDef = Models.Domain.CDefModel.create(core, ContentName);
                    ContentID = CDef.id;
                    if (ContentID > 0) {
                        //If false And core.siteProperties.allowWorkflowAuthoring Then
                        //    '
                        //    ' Check Authoring Controls record for Locks
                        //    '
                        //    'TableID = csv_GetContentTableID(ContentName)
                        //    Criteria = getAuthoringControlCriteria(ContentName, RecordID)
                        //    CSLocks = core.db.cs_open("Authoring Controls", Criteria, "DateAdded Desc", , , , , "DateAdded,ControlType,CreatedBy,ID,DateExpires")
                        //    Do While core.db.cs_ok(CSLocks)
                        //        ControlType = core.db.cs_getInteger(CSLocks, "ControlType")
                        //        Select Case ControlType
                        //            Case AuthoringControlsModified
                        //                If Not IsModified Then
                        //                    ModifiedDate = core.db.cs_getDate(CSLocks, "DateAdded")
                        //                    ModifiedName = core.db.cs_get(CSLocks, "CreatedBy")
                        //                    IsModified = True
                        //                End If
                        //            Case AuthoringControlsSubmitted
                        //                If Not IsSubmitted Then
                        //                    SubmittedDate = core.db.cs_getDate(CSLocks, "DateAdded")
                        //                    SubmittedName = core.db.cs_get(CSLocks, "CreatedBy")
                        //                    IsSubmitted = True
                        //                End If
                        //            Case AuthoringControlsApproved
                        //                If Not IsApproved Then
                        //                    ApprovedDate = core.db.cs_getDate(CSLocks, "DateAdded")
                        //                    ApprovedName = core.db.cs_get(CSLocks, "CreatedBy")
                        //                    IsApproved = True
                        //                End If
                        //        End Select
                        //        core.db.cs_goNext(CSLocks)
                        //    Loop
                        //    Call core.db.cs_Close(CSLocks)
                        //    '
                        //    ContentTableName = CDef.ContentTableName
                        //    AuthoringTableName = CDef.AuthoringTableName
                        //    DataSourceName = CDef.ContentDataSourceName
                        //    SQL = "Select ContentTableName.ID As LiveRecordID, AuthoringTableName.ID As EditRecordID, AuthoringTableName.EditBlank As IsDeleted, ContentTableName.EditBlank As IsInserted, AuthoringTableName.ModifiedDate As EditRecordModifiedDate, ContentTableName.ModifiedDate As LiveRecordModifiedDate" _
                        //    & " FROM " & AuthoringTableName & " As AuthoringTableName RIGHT JOIN " & ContentTableName & " As ContentTableName On AuthoringTableName.EditSourceID = ContentTableName.ID" _
                        //    & " Where (((ContentTableName.ID) = " & RecordID & "))" _
                        //    & " ORDER BY AuthoringTableName.ID DESC;"
                        //    rs = core.db.executeSql(SQL, DataSourceName)
                        //    If isDataTableOk(rs) Then
                        //        IsInserted = genericController.EncodeBoolean(core.db.getDataRowColumnName(rs.Rows[0], "IsInserted"))
                        //        IsDeleted = genericController.EncodeBoolean(core.db.getDataRowColumnName(rs.Rows[0], "IsDeleted"))
                        //        'IsModified = (getDataRowColumnName(RS.rows(0), "LiveRecordModifiedDate") <> getDataRowColumnName(RS.rows(0), "EditRecordModifiedDate"))
                        //    End If
                        //    Call closeDataTable(rs)
                        //End If
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=================================================================================
        //   Depricate this
        //   Instead, use csv_IsRecordLocked2, which uses the Db
        //=================================================================================
        //
        public void setEditLock(string ContentName, int RecordID, int MemberID) {
            try {
                setEditLock2(ContentName, RecordID, MemberID, false);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=================================================================================
        //   Depricate this
        //   Instead, use csv_IsRecordLocked, which uses the Db
        //=================================================================================
        //
        public void clearEditLock(string ContentName, int RecordID, int MemberID) {
            try {
                setEditLock2(ContentName, RecordID, MemberID, true);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=================================================================================
        //   Depricate this
        //   Instead, use csv_IsRecordLocked, which uses the Db
        //=================================================================================
        //
        public bool getEditLock(string ContentName, int RecordID, ref int ReturnMemberID, ref DateTime ReturnDateExpires) {
            return getEditLock2(ContentName, RecordID, ref ReturnMemberID, ref ReturnDateExpires);
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        public void setEditLock2(string ContentName, int RecordID, int MemberID, bool ClearLock = false) {
            try {
                int SourcePointer = 0;
                double EditLockTimeoutMinutes = 0;
                bool LockFound = false;
                DateTime EditLockDateExpires = default(DateTime);
                string SourceKey = null;
                DateTime SourceDateExpires = default(DateTime);
                int DestinationPointer = 0;
                string StringBuffer = null;
                string EditLockKey2 = null;
                //
                if ((!string.IsNullOrEmpty(ContentName)) && (RecordID != 0)) {
                    EditLockKey2 = GenericController.vbUCase(ContentName + "," + RecordID.ToString());
                    StringBuffer = core.siteProperties.getText("EditLockTimeout", "5");
                    EditLockTimeoutMinutes = encodeNumber(StringBuffer);
                    EditLockDateExpires = DateTime.Now.AddMinutes(EditLockTimeoutMinutes);
                    if (EditLockCount > 0) {
                        for (SourcePointer = 0; SourcePointer < EditLockCount; SourcePointer++) {
                            SourceKey = EditLockArray[SourcePointer].Key;
                            SourceDateExpires = EditLockArray[SourcePointer].DateExpires;
                            if (SourceKey == EditLockKey2) {
                                //
                                // This edit lock was found
                                //
                                LockFound = true;
                                if (EditLockArray[SourcePointer].MemberID != MemberID) {
                                    //
                                    // This member did not create the lock, he can not change it either
                                    //
                                    if (SourcePointer != DestinationPointer) {
                                        EditLockArray[DestinationPointer].Key = SourceKey;
                                        EditLockArray[DestinationPointer].MemberID = MemberID;
                                        EditLockArray[DestinationPointer].DateExpires = SourceDateExpires;
                                    }
                                    DestinationPointer = DestinationPointer + 1;
                                } else if (!ClearLock) {
                                    //
                                    // This lock created by this member, he can change it
                                    //
                                    EditLockArray[DestinationPointer].Key = SourceKey;
                                    EditLockArray[DestinationPointer].MemberID = MemberID;
                                    EditLockArray[DestinationPointer].DateExpires = EditLockDateExpires;
                                    DestinationPointer = DestinationPointer + 1;
                                }
                            } else if (SourceDateExpires >= EditLockDateExpires) {
                                //
                                // Lock not expired, move it if needed
                                //
                                if (SourcePointer != DestinationPointer) {
                                    EditLockArray[DestinationPointer].Key = SourceKey;
                                    EditLockArray[DestinationPointer].MemberID = MemberID;
                                    EditLockArray[DestinationPointer].DateExpires = SourceDateExpires;
                                }
                                DestinationPointer = DestinationPointer + 1;
                            }
                            ///DoEvents()
                        }
                    }
                    if ((!LockFound) && (!ClearLock)) {
                        //
                        // Lock not found, add it
                        //
                        if (EditLockCount > 0) {
                            if (DestinationPointer > EditLockArray.GetUpperBound(0)) {
                                Array.Resize(ref EditLockArray, DestinationPointer + 11);
                            }
                        } else {
                            Array.Resize(ref EditLockArray, 11);
                        }
                        EditLockArray[DestinationPointer].Key = EditLockKey2;
                        EditLockArray[DestinationPointer].MemberID = MemberID;
                        EditLockArray[DestinationPointer].DateExpires = EditLockDateExpires;
                        DestinationPointer = DestinationPointer + 1;
                    }
                    EditLockCount = DestinationPointer;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //=================================================================================
        //   Returns true if this content/record is locked
        //       if true, the ReturnMemberID is the member that locked it
        //           and ReturnDteExpires is the date when it will be released
        //=================================================================================
        //
        public bool getEditLock2(string ContentName, int RecordID, ref int ReturnMemberID, ref DateTime ReturnDateExpires) {
            bool EditLock2 = false;
            try {
                int SourcePointer = 0;
                string EditLockKey2 = null;
                DateTime DateNow = default(DateTime);
                //
                if ((!string.IsNullOrEmpty(ContentName)) && (RecordID != 0) && (EditLockCount > 0)) {
                    EditLockKey2 = GenericController.vbUCase(ContentName + "," + RecordID.ToString());
                    DateNow = DateTime.Now;
                    for (SourcePointer = 0; SourcePointer < EditLockCount; SourcePointer++) {
                        if (EditLockArray[SourcePointer].Key == EditLockKey2) {
                            ReturnMemberID = EditLockArray[SourcePointer].MemberID;
                            ReturnDateExpires = EditLockArray[SourcePointer].DateExpires;
                            if (ReturnDateExpires > DateTime.Now) {
                                EditLock2 = true;
                            }
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return EditLock2;
        }

        //
        //==========================================================================================
        #region  IDisposable Support 
        protected bool disposed = false;
        //
        //==========================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        /// <remarks></remarks>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // ----- call .dispose for managed objects
                    //
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~WorkflowController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}

