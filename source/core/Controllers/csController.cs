
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using Contensive.BaseClasses;
//

namespace Contensive.Core {
    //
    //==========================================================================================
    /// <summary>
    /// A recordset-like construct
    /// </summary>
    public class csController : IDisposable {
        //
        private Contensive.Core.coreClass cpCore;
        private int csPtr;
        private int openingMemberID;
        protected bool disposed = false;
        //
        // Constructor - Initialize the Main and Csv objects
        //
        public csController(coreClass cpCore) {
            this.cpCore = cpCore;
            openingMemberID = cpCore.doc.sessionContext.user.id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // -- call .dispose for managed objects
                    if (csPtr > -1) {
                        cpCore.db.csClose(ref csPtr);
                    }
                }
                //
                // -- Add code here to release the unmanaged resource.
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        /// <summary>
        /// insert
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public bool insert(string ContentName) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csInsertRecord(ContentName, openingMemberID);
                success = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openRecord(string ContentName, int recordId, string SelectFieldList = "", bool ActiveOnly = true) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpen(ContentName, "id=" + recordId, "", ActiveOnly, 0, false, false, SelectFieldList, 1, 1);
                success = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int pageSize = 0, int PageNumber = 1) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly,0,false,false,SelectFieldList, pageSize, PageNumber);
                success = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openGroupUsers(List<string> GroupList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpenGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = ok();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openGroupUsers(string GroupName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                List<string> groupList = new List<string>();
                groupList.Add(GroupName);
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = ok();
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openGroupListUsers(string GroupCommaList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            bool result = false;
            try {
                List<string> groupList = new List<string>();
                groupList.AddRange(GroupCommaList.Split(','));
                result = openGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public bool openSQL(string sql) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpenSql_rev("default", sql);
                success = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openSQL(string sql, string DataSourcename, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                }
                csPtr = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                success = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public void Close() {
            try {
                if (csPtr != -1) {
                    cpCore.db.csClose(ref csPtr);
                    csPtr = -1;
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public string getFormInput(string ContentName, string FieldName, int HeightLines = 1, int WidthRows = 40, string HtmlId = "") {
            return cpCore.html.inputCs(csPtr, ContentName, FieldName, HeightLines, WidthRows, HtmlId);
        }
        //
        //====================================================================================================
        public void delete() {
            cpCore.db.csDeleteRecord(csPtr);
        }
        //
        //====================================================================================================
        public bool fieldOK(string FieldName) {
            return cpCore.db.cs_isFieldSupported(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public void goFirst() {
            cpCore.db.cs_goFirst(csPtr, false);
        }
        //
        //====================================================================================================
        public string getAddLink(string PresetNameValueList = "", bool AllowPaste = false) {
            string result = "";
            try {
                result = cpCore.html.cs_getRecordAddLink(csPtr, PresetNameValueList, AllowPaste);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public bool getBoolean(string FieldName) {
            return cpCore.db.csGetBoolean(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string FieldName) {
            return cpCore.db.csGetDate(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public string getEditLink(bool AllowCut = false) {
            string result = "";
            try {
                result = cpCore.db.csGetRecordEditLink(csPtr, AllowCut);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field is backed by a filename, use this method to read the filename
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="OriginalFilename"></param>
        /// <param name="ContentName"></param>
        /// <param name="fieldTypeId"></param>
        /// <returns></returns>
        public string getFieldFilename(string FieldName, string OriginalFilename = "", string ContentName = "", int fieldTypeId = 0) {
            string result = "";
            try {
                result = cpCore.db.csGetFieldFilename(csPtr, FieldName, OriginalFilename, ContentName, fieldTypeId);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public int getInteger(string FieldName) {
            return cpCore.db.csGetInteger(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string FieldName) {
            return cpCore.db.csGetNumber(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public int getRowCount() {
            return cpCore.db.csGetRowCount(csPtr);
        }
        //
        //====================================================================================================
        public string getSql() {
            string result = "";
            try {
                result = cpCore.db.csGetSource(csPtr);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the text value stored in the field. For Lookup fields, this method returns the name of the foreign key record.
        /// For textFile fields, this method returns the filename.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        public string getText(string FieldName) {
            string result = "";
            try {
                result = cpCore.db.csGet(csPtr, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        // Needs to be implemented (refactor to check the field type. if not fieldTypeHtml, encodeHtml)
        public string getHtml(string FieldName) {
            return getText(FieldName);
        }
        //
        //====================================================================================================
        // <summary>
        // returns the text stored in a textfile type field instead of the filename.
        // </summary>
        // <param name="FieldName"></param>
        // <returns></returns>
        //Public Function getTextFile(ByVal FieldName As String) As String
        //    Dim result As String = ""
        //    Try
        //        result = cpCore.db.cs_getTextFile(csPtr, FieldName)
        //        If result Is Nothing Then
        //            result = ""
        //        End If
        //    Catch ex As Exception
        //        Call cpCore.handleException(ex); : Throw
        //    End Try
        //    Return result
        //End Function
        //
        //====================================================================================================
        public void goNext() {
            cpCore.db.csGoNext(csPtr);
        }
        //
        //====================================================================================================
        public bool nextOK() {
            bool result = false;
            try {
                cpCore.db.csGoNext(csPtr);
                result = cpCore.db.csOk(csPtr);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public bool ok() {
            return cpCore.db.csOk(csPtr);
        }
        //
        //====================================================================================================
        public void save() {
            cpCore.db.csSave2(csPtr);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            cpCore.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFieldFilename( string fieldName, string filename ) {
            cpCore.db.csSetFieldFilename(csPtr, fieldName, filename);
        }
        //
        //========================================================================
        //   main_cs_get Field, translate all fields to their best text equivalent, and encode for display
        //========================================================================
        //
        public static string getTextEncoded(coreClass cpcore, int CSPointer, string FieldName) {
            string ContentName = "";
            int RecordID = 0;
            if (cpcore.db.cs_isFieldSupported(CSPointer, "id") & cpcore.db.cs_isFieldSupported(CSPointer, "contentcontrolId")) {
                RecordID = cpcore.db.csGetInteger(CSPointer, "id");
                ContentName = Models.Complex.cdefModel.getContentNameByID(cpcore, cpcore.db.csGetInteger(CSPointer, "contentcontrolId"));
            }
            string source = cpcore.db.csGet(CSPointer, FieldName);
            return activeContentController.convertActiveContentToHtmlForWebRender(cpcore, source, ContentName, RecordID, cpcore.doc.sessionContext.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        //========================================================================
        //
        public string getValue(string FieldName) {
            string result = "";
            try {
                result = cpCore.db.cs_getValue(csPtr, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }

        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~csController() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}

