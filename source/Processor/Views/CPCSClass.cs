﻿
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;

namespace Contensive.Processor {
    /// <summary>
    /// The implementation of the CPCSBaseClass interface. Base interface is exposed to addons, and this implementation is passed during run-time
    /// </summary>
    public class CPCSClass : CPCSBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "63745D9C-795E-4C01-BD6D-4BA35FC4A843";
        public const string InterfaceId = "3F1E7D2E-D697-47A8-A0D3-B625A906BF6A";
        public const string EventsId = "04B8E338-ABB7-44FE-A8DF-2681A36DCA46";
        #endregion
        /// <summary>
        /// dependancies
        /// </summary>
        private CPClass cp;
        /// <summary>
        /// index into metadata array
        /// </summary>
        private int legacy_cs;
        /// <summary>
        /// user who opened the dataset
        /// </summary>
        private int legacy_openingMemberID;
        //
        private CsModel cs;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="cp"></param>
        public CPCSClass(CPBaseClass cp) {
            this.cp = (CPClass)cp;
            cs = new CsModel(this.cp.core);
            //csKeyLegacy = -1;
            //legacy_openingMemberID = this.cp.core.session.user.id;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Insert a record, leaving the dataset open in this object. Call cs.close() to close the data
        /// </summary>
        /// <param name="contentName"></param>
        /// <returns></returns>
        public override bool Insert(string contentName) {
            try {
                if (legacy_cs != -1) { cp.core.db.csClose(ref legacy_cs); }
                legacy_cs = cp.core.db.csInsertRecord(contentName, legacy_openingMemberID);
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Open a ContentSet object with a single record
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <param name="selectFieldList"></param>
        /// <param name="activeOnly"></param>
        /// <returns></returns>
        public override bool OpenRecord(string contentName, int recordId, string selectFieldList, bool activeOnly) {
            try {
                if (legacy_cs != -1) { cp.core.db.csClose(ref legacy_cs); }
                legacy_cs = cp.core.db.csOpen(contentName, "id=" + recordId, "", activeOnly, 0, false, false, selectFieldList, 1, 1);
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex );
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OpenRecord(string contentName, int recordId) 
            => OpenRecord(contentName, recordId, "", true);
        //
        //====================================================================================================
        //
        public override bool OpenRecord(string contentName, int recordId, string SelectFieldList) 
            => OpenRecord(contentName, recordId, SelectFieldList, true);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria, string sortFieldList, bool activeOnly, string selectFieldList, int pageSize, int pageNumber) {
            try {
                if (legacy_cs != -1) { cp.core.db.csClose(ref legacy_cs); }
                if ((pageSize == 0) || (pageSize == 10)) {
                    // -- (hack) fix for interface issue that has default value 0. later add new method and deprecate
                    // -- pagesize=10, pageNumber=1 -- old code returns all records, new code only returns the first 10 records -- this in effect makes it not compatiblie
                    // -- if I change new cpBase to default pagesize=9999, the change is breaking and old code does not run
                    // -- when I changed new cpbase to pagesize default 0, and compiled code against it, it would not run on c41 because pagesize=0 is passed
                    pageSize = 9999;
                }
                legacy_cs = cp.core.db.csOpen(contentName, sqlCriteria, sortFieldList, activeOnly, 0, false, false, selectFieldList, pageSize, pageNumber);
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria, string sortFieldList, bool activeOnly, string selectFieldList, int pageSize)
            => Open(contentName, sqlCriteria, sortFieldList, activeOnly, selectFieldList, pageSize, 1);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria, string sortFieldList, bool activeOnly, string selectFieldList)
            => Open(contentName, sqlCriteria, sortFieldList, activeOnly, selectFieldList, 10, 1);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria, string sortFieldList, bool activeOnly)
            => Open(contentName, sqlCriteria, sortFieldList, activeOnly, "", 10, 1);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria, string sortFieldList) 
            => Open(contentName, sqlCriteria, sortFieldList, true, "", 10, 1);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName, string sqlCriteria) 
            => Open(contentName, sqlCriteria, "", true, "", 10, 1);
        //
        //====================================================================================================
        //
        public override bool Open(string contentName) 
            => Open(contentName, "", "", true, "", 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize, int pageNumber) {
            try {
                if (legacy_cs != -1) { cp.core.db.csClose(ref legacy_cs); }
                legacy_cs = cp.core.db.csOpenGroupUsers(groupList, sqlCriteria, sortFieldList, activeOnly, pageSize, pageNumber);
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core, ex );
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize)
            => OpenGroupUsers(groupList, sqlCriteria, sortFieldList, activeOnly, pageSize, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList, bool activeOnly)
            => OpenGroupUsers(groupList, sqlCriteria, sortFieldList, activeOnly, 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList, string sqlCriteria, string sortFieldList)
            => OpenGroupUsers(groupList, sqlCriteria, sortFieldList, true, 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList, string sqlCriteria)
            => OpenGroupUsers(groupList, sqlCriteria, "", true, 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> groupList)
            => OpenGroupUsers(groupList, "", "", true, 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize, int pageNumber)
            => OpenGroupUsers(new List<string>() { groupName }, sqlCriteria, sortFieldList, activeOnly, pageSize, pageNumber);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName, string sqlCriteria, string sortFieldList, bool activeOnly, int pageSize)
            => OpenGroupUsers(new List<string>() { groupName }, sqlCriteria, sortFieldList, activeOnly, pageSize);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName, string sqlCriteria, string sortFieldList, bool activeOnly)
            => OpenGroupUsers(new List<string>() { groupName }, sqlCriteria, sortFieldList, activeOnly);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName, string sqlCriteria, string sortFieldList)
            => OpenGroupUsers(new List<string>() { groupName }, sqlCriteria, sortFieldList);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName, string sqlCriteria)
            => OpenGroupUsers(new List<string>() { groupName }, sqlCriteria);
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string groupName)
            => OpenGroupUsers(new List<string>() { groupName });
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql, string dataSourcename, int pageSize, int pageNumber) {
            try {
                if (legacy_cs != -1) { cp.core.db.csClose(ref legacy_cs); }
                if (((string.IsNullOrEmpty(sql)) || (sql.ToLowerInvariant() == "default")) && (!string.IsNullOrEmpty(dataSourcename)) && (dataSourcename.ToLowerInvariant() != "default")) {
                    //
                    // -- arguments reversed from legacy api mistake, datasource has the query, sql has the datasource
                    LogController.logWarn(cp.core, "Call to cs with arguments reversed, datasource [" + dataSourcename + "], sql [" + sql + "]");
                    legacy_cs = cp.core.db.csOpenSql(dataSourcename, sql, pageSize, pageNumber);
                } else {
                    legacy_cs = cp.core.db.csOpenSql(sql, dataSourcename, pageSize, pageNumber);
                }
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql, string dataSourcename, int pageSize)
            => OpenSQL(sql, dataSourcename, pageSize, 1);
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql, string dataSourcename)
            => OpenSQL(sql, dataSourcename, 10, 1);
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql)
            => OpenSQL(sql, "", 10, 1);
        //
        //====================================================================================================
        //
        public override void Close() {
            try {
                if (legacy_cs != -1) {
                    cp.core.db.csClose(ref legacy_cs);
                    legacy_cs = -1;
                }
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override object GetFormInput(string contentName, string fieldName, int height, int width, string htmlId) {
            return cp.core.html.inputCs(legacy_cs, contentName, fieldName, height, width, htmlId);
        }
        public override object GetFormInput(string contentName, string fieldName, int height, int width) {
            return cp.core.html.inputCs(legacy_cs, contentName, fieldName, height, width);
        }
        public override object GetFormInput(string contentName, string fieldName, int height) {
            return cp.core.html.inputCs(legacy_cs, contentName, fieldName, height);
        }
        public override object GetFormInput(string contentName, string fieldName) {
            return cp.core.html.inputCs(legacy_cs, contentName, fieldName);
        }
        //
        //====================================================================================================
        //
        public override void Delete() {
            try {
                cp.core.db.csDeleteRecord(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool FieldOK(string fieldName) {
            try {
                return cp.core.db.csIsFieldSupported(legacy_cs, fieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void GoFirst() {
            try {
                cp.core.db.csGoFirst(legacy_cs, false);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetAddLink(string presetNameValueList, bool allowPaste) {
            try {
                return DbController.csGetRecordAddLink(cp.core, legacy_cs, presetNameValueList, allowPaste);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        public override string GetAddLink(string presetNameValueList)
            => GetAddLink(presetNameValueList, false);
        //
        public override string GetAddLink()
            => GetAddLink("", false);
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string FieldName) {
            try {
                return cp.core.db.csGetBoolean(legacy_cs, FieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override DateTime GetDate(string FieldName) {
            try {
                return cp.core.db.csGetDate(legacy_cs, FieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetEditLink(bool allowCut) {
            try {
                return cp.core.db.csGetRecordEditLink(legacy_cs, allowCut);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        public override string GetEditLink() => GetEditLink(false);
        //
        //====================================================================================================
        //
        public override string GetFilename(string fieldName, string OriginalFilename, string ContentName) {
            try {
                return cp.core.db.csGetFieldFilename(legacy_cs, fieldName, OriginalFilename, ContentName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        public override string GetFilename(string fieldName, string originalFilename)
            => GetFilename(fieldName, originalFilename);

        public override string GetFilename(string fieldName)
            => GetFilename(fieldName, "");
        //
        //====================================================================================================
        //
        public override int GetInteger(string FieldName) {
            try {
                return cp.core.db.csGetInteger(legacy_cs, FieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string FieldName) {
            try {
                return cp.core.db.csGetNumber(legacy_cs, FieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override int GetRowCount() {
            try {
                return cp.core.db.csGetRowCount(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetSQL() {
            try {
                return cp.core.db.csGetSql(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetText(string fieldName) {
            try {
                return cp.core.db.csGet(legacy_cs, fieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetHtml(string fieldName) {
            try {
                return cp.core.db.csGet(legacy_cs, fieldName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void GoNext() {
            try {
                cp.core.db.csGoNext(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool NextOK() {
            try {
                cp.core.db.csGoNext(legacy_cs);
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OK() {
            try {
                return cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Save() {
            try {
                cp.core.db.csSave(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetField(string fieldName, object fieldValue) {
            try {
                if (fieldValue is string fieldString) {
                    cp.core.db.csSet(legacy_cs, fieldName, fieldString);
                    return;
                }
                int? fieldInt = fieldValue as int?;
                if (fieldInt != null) {
                    cp.core.db.csSet(legacy_cs, fieldName, fieldInt.GetValueOrDefault());
                    return;
                }
                bool? fieldBool = fieldValue as bool?;
                if (fieldBool != null) {
                    cp.core.db.csSet(legacy_cs, fieldName, fieldBool.GetValueOrDefault());
                    return;
                }
                DateTime? fieldDate = fieldValue as DateTime?;
                if (fieldDate != null) {
                    cp.core.db.csSet(legacy_cs, fieldName, fieldDate.GetValueOrDefault());
                    return;
                }
                cp.core.db.csSet(legacy_cs, fieldName, fieldValue.ToString());
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
        }
        //
        public override void SetField(string FieldName, int FieldValue) {
            cp.core.db.csSet(legacy_cs, FieldName, FieldValue);
        }
        //
        public override void SetField(string FieldName, bool FieldValue) {
            cp.core.db.csSet(legacy_cs, FieldName, FieldValue);
        }
        //
        public override void SetField(string FieldName, DateTime FieldValue) {
            cp.core.db.csSet(legacy_cs, FieldName, FieldValue);
        }
        //
        public override void SetField(string FieldName, String FieldValue) {
            cp.core.db.csSet(legacy_cs, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        //
        public override void SetFormInput(string fieldName, string requestName) {
            try {
                DbController.csSetFormInput(cp.core, legacy_cs, fieldName, requestName);
            } catch (Exception ex) {
                LogController.handleError( cp.core,ex);
                throw;
            }
        }
        public override void SetFormInput(string fieldName)
            => SetFormInput(fieldName, fieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Return the value in the field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public override string GetValue(string fieldName) {
            return cp.core.db.csGetValue(legacy_cs, fieldName);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Use OpenGroupUsers()", true)]
        public override bool OpenGroupListUsers(string GroupCommaList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            List<string> groupList = new List<string>();
            groupList.AddRange(GroupCommaList.Split(','));
            return OpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
        }
        //
        [Obsolete("Use GetFormInput(string,string,int,int,string)", true)]
        public override object GetFormInput(string contentName, string fieldName, string height, string width, string htmlId)
            => GetFormInput(contentName, fieldName, GenericController.encodeInteger(height), GenericController.encodeInteger(width), htmlId);
        //
        [Obsolete("Use GetFormInput(string,string,int,int)", true)]
        public override object GetFormInput(string contentName, string fieldName, string height, string width)
            => GetFormInput(contentName, fieldName, GenericController.encodeInteger(height), GenericController.encodeInteger(width));
        //
        //
        [Obsolete("Use GetFormInput(string,string,int)", true)]
        public override object GetFormInput(string contentName, string fieldName, string height)
            => GetFormInput(contentName, fieldName, GenericController.encodeInteger(height));
        //
        [Obsolete("Use SetField for all field types that store data in files (textfile, cssfile, etc)")]
        public override void SetFile(string FieldName, string Copy, string ContentName) {
            try {
                cp.core.db.csSetTextFile(legacy_cs, FieldName, Copy, ContentName);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
        }
        //
        [Obsolete("Use OpenSql()", true)]
        public override bool OpenSQL2(string sql, string DataSourcename = "default", int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (legacy_cs != -1) {
                    cp.core.db.csClose(ref legacy_cs);
                }
                if (((string.IsNullOrEmpty(sql)) || (sql.ToLowerInvariant() == "default")) && (!string.IsNullOrEmpty(DataSourcename)) && (DataSourcename.ToLowerInvariant() != "default")) {
                    legacy_cs = cp.core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                } else {
                    legacy_cs = cp.core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                }
                success = cp.core.db.csOk(legacy_cs);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
            return success;
        }
        //
        [Obsolete("Use getText for copy. getFilename for filename", true)]
        public override string GetTextFile(string FieldName) {
            try {
                return cp.core.db.csGetText(legacy_cs, FieldName);
            } catch (Exception ex) {
                LogController.handleError(cp.core, ex);
                throw;
            }
        }
        //
        //====================================================================================================
        // Disposable
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public override void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    try {
                        if (legacy_cs > -1) { cp.core.db.csClose(ref legacy_cs); }
                    } catch( Exception ) {
                        //
                    }
                }
            }
            this.disposed = true;
        }
        ~CPCSClass() {
            Dispose(false);
        }
        protected bool disposed = false;
        #endregion
    }

}

