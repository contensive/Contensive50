

using System.Runtime.InteropServices;
using Controllers;
using Contensive.BaseClasses;

namespace Contensive.Core {
    
    public class csController : IDisposable {
        
        // 
        private Contensive.Core.coreClass cpCore;
        
        private int csPtr;
        
        private int OpeningMemberID;
        
        protected bool disposed = false;
        
        public csController(ref coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' dispose
        // '' </summary>
        // '' <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    // 
                    //  -- call .dispose for managed objects
                    if ((csPtr > -1)) {
                        cpCore.db.csClose(csPtr);
                    }
                    
                }
                
                // 
                //  -- Add code here to release the unmanaged resource.
            }
            
            this.disposed = true;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' insert
        // '' </summary>
        // '' <param name="ContentName"></param>
        // '' <returns></returns>
        public bool Insert(string ContentName) {
            bool success = false;
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csInsertRecord(ContentName, OpeningMemberID);
                success = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool OpenRecord(string ContentName, int recordId, string SelectFieldList, void =, void , bool ActiveOnly, void =, void True) {
            bool success = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpen(ContentName, ("id=" + recordId), ,, ActiveOnly, ,, ,, SelectFieldList, 1, 1);
                success = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool open(
                    string ContentName, 
                    string SQLCriteria, 
                    void =, 
                    void , 
                    string SortFieldList, 
                    void =, 
                    void , 
                    bool ActiveOnly, 
                    void =, 
                    void True, 
                    string SelectFieldList, 
                    void =, 
                    void , 
                    int pageSize, 
                    void =, 
                    void 0, 
                    int PageNumber, 
                    void =, 
                    void 1) {
            bool success = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly, ,, ,, SelectFieldList, pageSize, PageNumber);
                success = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool openGroupUsers(
                    List<string> GroupList, 
                    string SQLCriteria, 
                    void =, 
                    void , 
                    string SortFieldList, 
                    void =, 
                    void , 
                    bool ActiveOnly, 
                    void =, 
                    void True, 
                    int PageSize, 
                    void =, 
                    void 10, 
                    int PageNumber, 
                    void =, 
                    void 1) {
            bool success = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpenGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = this.ok();
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool openGroupUsers(
                    string GroupName, 
                    string SQLCriteria, 
                    void =, 
                    void , 
                    string SortFieldList, 
                    void =, 
                    void , 
                    bool ActiveOnly, 
                    void =, 
                    void True, 
                    int PageSize, 
                    void =, 
                    void 10, 
                    int PageNumber, 
                    void =, 
                    void 1) {
            bool success = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                List<string> groupList = new List<string>();
                groupList.Add(GroupName);
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = this.ok();
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool openGroupListUsers(
                    string GroupCommaList, 
                    string SQLCriteria, 
                    void =, 
                    void , 
                    string SortFieldList, 
                    void =, 
                    void , 
                    bool ActiveOnly, 
                    void =, 
                    void True, 
                    int PageSize, 
                    void =, 
                    void 10, 
                    int PageNumber, 
                    void =, 
                    void 1) {
            bool result = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                List<string> groupList = new List<string>();
                groupList.AddRange(",".Split(c));
                result = this.openGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        public bool openSQL(string sql) {
            bool success = false;
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpenSql_rev("default", sql);
                success = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public bool openSQL(string sql, string DataSourcename, int PageSize, void =, void 10, int PageNumber, void =, void 1) {
            bool success = false;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                }
                
                csPtr = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                success = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return success;
        }
        
        // 
        // ====================================================================================================
        public void Close() {
            try {
                if ((csPtr != -1)) {
                    cpCore.db.csClose(csPtr);
                    csPtr = -1;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
        }
        
        // 
        // ====================================================================================================
        public string getFormInput(string ContentName, string FieldName, int HeightLines, void =, void 1, int WidthRows, void =, void 40, string HtmlId, void =, void ) {
            return cpCore.html.html_GetFormInputCS(csPtr, ContentName, FieldName, HeightLines, WidthRows, HtmlId);
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // ====================================================================================================
        public void delete() {
            cpCore.db.csDeleteRecord(csPtr);
        }
        
        // 
        // ====================================================================================================
        public bool fieldOK(string FieldName) {
            return cpCore.db.cs_isFieldSupported(csPtr, FieldName);
        }
        
        // 
        // ====================================================================================================
        public void goFirst() {
            cpCore.db.cs_goFirst(csPtr, false);
        }
        
        // 
        // ====================================================================================================
        public string getAddLink(string PresetNameValueList, void =, void , bool AllowPaste, void =, void False) {
            string result = "";
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                result = cpCore.html.main_cs_getRecordAddLink(csPtr, PresetNameValueList, AllowPaste);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        public bool getBoolean(string FieldName) {
            return cpCore.db.csGetBoolean(csPtr, FieldName);
        }
        
        // 
        // ====================================================================================================
        public DateTime getDate(string FieldName) {
            return cpCore.db.csGetDate(csPtr, FieldName);
        }
        
        // 
        // ====================================================================================================
        public string getEditLink(bool AllowCut, void =, void False) {
            string result = String.Empty;
            // Warning!!! Optional parameters not supported
            try {
                result = cpCore.db.csGetRecordEditLink(csPtr, AllowCut);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        public string getFilename(string FieldName, string OriginalFilename, void =, void , string ContentName, void =, void , int fieldTypeId, void =, void 0) {
            string result = String.Empty;
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            try {
                result = cpCore.db.csGetFilename(csPtr, FieldName, OriginalFilename, ContentName, fieldTypeId);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        public int getInteger(string FieldName) {
            return cpCore.db.csGetInteger(csPtr, FieldName);
        }
        
        // 
        // ====================================================================================================
        public double getNumber(string FieldName) {
            return cpCore.db.csGetNumber(csPtr, FieldName);
        }
        
        // 
        // ====================================================================================================
        public int getRowCount() {
            return cpCore.db.csGetRowCount(csPtr);
        }
        
        // 
        // ====================================================================================================
        public string getSql() {
            string result = String.Empty;
            try {
                result = cpCore.db.csGetSource(csPtr);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' returns the text value stored in the field. For Lookup fields, this method returns the name of the foreign key record.
        // '' For textFile fields, this method returns the filename.
        // '' </summary>
        // '' <param name="FieldName"></param>
        // '' <returns></returns>
        public string getText(string FieldName) {
            string result = String.Empty;
            try {
                result = cpCore.db.csGet(csPtr, FieldName);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        //  Needs to be implemented (refactor to check the field type. if not fieldTypeHtml, encodeHtml)
        public string getHtml(string FieldName) {
            return this.getText(FieldName);
        }
        
        // '
        // '====================================================================================================
        // ''' <summary>
        // ''' returns the text stored in a textfile type field instead of the filename.
        // ''' </summary>
        // ''' <param name="FieldName"></param>
        // ''' <returns></returns>
        // Public Function getTextFile(ByVal FieldName As String) As String
        //     Dim result As String = String.Empty
        //     Try
        //         result = cpCore.db.cs_getTextFile(csPtr, FieldName)
        //         If result Is Nothing Then
        //             result = String.Empty
        //         End If
        //     Catch ex As Exception
        //         Call cpCore.handleException(ex) : Throw
        //     End Try
        //     Return result
        // End Function
        // 
        // ====================================================================================================
        public void goNext() {
            cpCore.db.csGoNext(csPtr);
        }
        
        // 
        // ====================================================================================================
        public bool nextOK() {
            bool result = false;
            try {
                cpCore.db.csGoNext(csPtr);
                result = cpCore.db.csOk(csPtr);
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        // 
        // ====================================================================================================
        public bool ok() {
            return cpCore.db.csOk(csPtr);
        }
        
        // 
        // ====================================================================================================
        public void save() {
            cpCore.db.csSave2(csPtr);
        }
        
        // 
        // ====================================================================================================
        // 
        public void setField(string FieldName, DateTime FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        
        public void setField(string FieldName, bool FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        
        public void setField(string FieldName, string FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        
        public void setField(string FieldName, double FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        
        public void setField(string FieldName, int FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        
        // 
        // ====================================================================================================
        public void setFile(string FieldName, string Copy, string ContentName) {
            cpCore.db.csSetTextFile(csPtr, FieldName, Copy, ContentName);
        }
        
        // 
        // ====================================================================================================
        public void SetFormInput(string FieldName, string RequestName, void =, void ) {
            csController.cs_setFormInput(cpCore, csPtr, FieldName, RequestName);
            // Warning!!! Optional parameters not supported
        }
        
        // 
        // 
        // 
        public static void cs_setFormInput(coreClass cpcore, int CSPointer, string FieldName, string RequestName, void =, void ) {
            string LocalRequestName;
            // Warning!!! Optional parameters not supported
            string Filename;
            string Path;
            // 
            // If Not (true) Then Exit Sub
            // 
            if (!cpcore.db.csOk(CSPointer)) {
                throw new ApplicationException("ContentSetPointer is invalid, empty, or end-of-file");
            }
            else if ((FieldName.Trim() == "")) {
                throw new ApplicationException("FieldName is invalid or blank");
            }
            else {
                LocalRequestName = RequestName;
                if ((LocalRequestName == "")) {
                    LocalRequestName = FieldName;
                }
                
                switch (cpcore.db.cs_getFieldTypeId(CSPointer, FieldName)) {
                    case FieldTypeIdBoolean:
                        // 
                        //  Boolean
                        // 
                        cpcore.db.csSet(CSPointer, FieldName, cpcore.docProperties.getBoolean(LocalRequestName));
                        break;
                    case FieldTypeIdCurrency:
                    case FieldTypeIdFloat:
                    case FieldTypeIdInteger:
                    case FieldTypeIdLookup:
                    case FieldTypeIdManyToMany:
                        // 
                        //  Numbers
                        // 
                        cpcore.db.csSet(CSPointer, FieldName, cpcore.docProperties.getNumber(LocalRequestName));
                        break;
                    case FieldTypeIdDate:
                        // 
                        //  Date
                        // 
                        cpcore.db.csSet(CSPointer, FieldName, cpcore.docProperties.getDate(LocalRequestName));
                        break;
                    case FieldTypeIdFile:
                    case FieldTypeIdFileImage:
                        // 
                        // 
                        // 
                        Filename = cpcore.docProperties.getText(LocalRequestName);
                        if ((Filename != "")) {
                            Path = cpcore.db.csGetFilename(CSPointer, FieldName, Filename, ,, cpcore.db.cs_getFieldTypeId(CSPointer, FieldName));
                            cpcore.db.csSet(CSPointer, FieldName, Path);
                            Path = genericController.vbReplace(Path, "\\", "/");
                            Path = genericController.vbReplace(Path, ("/" + Filename), "");
                            cpcore.appRootFiles.upload(LocalRequestName, Path, Filename);
                        }
                        
                        break;
                    default:
                        cpcore.db.csSet(CSPointer, FieldName, cpcore.docProperties.getText(LocalRequestName));
                        break;
                }
            }
            
        }
        
        // 
        // ========================================================================
        //    main_cs_get Field, translate all fields to their best text equivalent, and encode for display
        // ========================================================================
        // 
        public static string getTextEncoded(coreClass cpcore, int CSPointer, string FieldName) {
            string ContentName = String.Empty;
            int RecordID = 0;
            if ((cpcore.db.cs_isFieldSupported(CSPointer, "id") && cpcore.db.cs_isFieldSupported(CSPointer, "contentcontrolId"))) {
                RecordID = cpcore.db.csGetInteger(CSPointer, "id");
                ContentName = models.complex.cdefmodel.getContentNameByID(cpcore, cpcore.db.csGetInteger(CSPointer, "contentcontrolId"));
            }
            
            string source = cpcore.db.csGet(CSPointer, FieldName);
            return cpcore.html.convertActiveContentToHtmlForWebRender(source, ContentName, RecordID, cpCore.doc.authContext.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
            // Return cpcore.html.convertActiveContent_internal(source, cpCore.doc.authContext.user.id, ContentName, RecordID, 0, False, False, True, True, False, True, "", "http://" & cpcore.webServer.requestDomain, False, 0, "", CPUtilsBaseClass.addonContext.ContextPage, cpCore.doc.authContext.isAuthenticated, Nothing, cpCore.doc.authContext.isEditingAnything())
        }
        
        // 
        // ========================================================================
        // 
        public string getValue(string FieldName) {
            string result = String.Empty;
            try {
                result = cpCore.db.cs_getValue(csPtr, FieldName);
                if ((result == null)) {
                    result = String.Empty;
                }
                
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            
            return result;
        }
        
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected override void Finalize() {
            this.Dispose(false);
            base.Finalize();
        }
    }
}