
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
using static Contensive.Processor.Controllers.genericController;
using static Contensive.Processor.constants;
using Contensive.BaseClasses;
//
namespace Contensive.Processor {
    //
    // comVisible to be activeScript compatible
    //
    //[ComVisible(true), Microsoft.VisualBasic.ComClass(CPCSClass.ClassId, CPCSClass.InterfaceId, CPCSClass.EventsId)]
    public class CPCSClass : CPCSBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "63745D9C-795E-4C01-BD6D-4BA35FC4A843";
        public const string InterfaceId = "3F1E7D2E-D697-47A8-A0D3-B625A906BF6A";
        public const string EventsId = "04B8E338-ABB7-44FE-A8DF-2681A36DCA46";
        #endregion
        //
        private Contensive.Processor.Controllers.CoreController core;
        private int cs;
        private int OpeningMemberID;
        private CPClass cp;
        protected bool disposed = false;
        //
        // Constructor - Initialize the Main and Csv objects
        //
        public CPCSClass(CPClass cpParent) : base() {
            cp = cpParent;
            core = cp.core;
            cs = -1;
            OpeningMemberID = core.session.user.id;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    try {
                        if (cs != -1 && true) {
                            core.db.csClose(ref cs);
                        }
                    } catch (Exception) {
                    }
                    core = null;
                    cp = null;
                }
            }
            this.disposed = true;
        }
        //
        // Insert, called only from Processor41.CSInsert after initializing 
        //
        public override bool Insert(string ContentName) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                cs = core.db.csInsertRecord(ContentName, OpeningMemberID);
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //
        //
        public override bool OpenRecord(string ContentName, int recordId, string SelectFieldList = "", bool ActiveOnly = true) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                cs = core.db.csOpen(ContentName, "id=" + recordId, "", ActiveOnly, 0, false, false, SelectFieldList, 1, 1);
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //
        //
        public override bool Open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int pageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                if ((pageSize == 0) || (pageSize == 10)) {
                    // -- (hack) fix for interface issue that has default value 0. later add new method and deprecate
                    // -- pagesize=10, pageNumber=1 -- old code returns all records, new code only returns the first 10 records -- this in effect makes it not compatiblie
                    // -- if I change new cpBase to default pagesize=9999, the change is breaking and old code does not run
                    // -- when I changed new cpbase to pagesize default 0, and compiled code against it, it would not run on c41 because pagesize=0 is passed
                    pageSize = 9999;
                }
                cs = core.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly, 0, false, false, SelectFieldList, pageSize, PageNumber);
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex); // "Unexpected error in cs.Open") : Throw
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(List<string> GroupList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                cs = core.db.csOpenGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = OK();
            } catch (Exception ex) {
                logController.handleError( core,ex); // "Unexpected error in cs.OpenGroupUsers")
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public override bool OpenGroupUsers(string GroupName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                List<string> groupList = new List<string>();
                groupList.Add(GroupName);
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                cs = core.db.csOpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
                success = OK();
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        [Obsolete()]
        public override bool OpenGroupListUsers(string GroupCommaList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1) {
            List<string> groupList = new List<string>();
            groupList.AddRange(GroupCommaList.Split(','));
            return OpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
        }
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                cs = core.db.csOpenSql(sql,"Default");
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public override bool OpenSQL(string sql, string DataSourcename, int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                if (((string.IsNullOrEmpty(sql)) || (sql.ToLower() == "default")) & (!string.IsNullOrEmpty(DataSourcename)) & (DataSourcename.ToLower() != "default")) {
                    cs = core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                } else {
                    cs = core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                }
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public override bool OpenSQL2(string sql, string DataSourcename = "default", int PageSize = 10, int PageNumber = 1) {
            bool success = false;
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                }
                if (((string.IsNullOrEmpty(sql)) || (sql.ToLower() == "default")) & (!string.IsNullOrEmpty(DataSourcename)) & (DataSourcename.ToLower() != "default")) {
                    cs = core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                } else {
                    cs = core.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
                }
                success = core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public override void Close() {
            try {
                if (cs != -1) {
                    core.db.csClose(ref cs);
                    cs = -1;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override object GetFormInput(string ContentName, string FieldName, string Height = "", string Width = "", string HtmlId = "") {
            return core.html.inputCs(cs, ContentName, FieldName, genericController.encodeInteger(Height), genericController.encodeInteger(Width), HtmlId);
        }
        //
        //====================================================================================================
        //
        public override void Delete() {
            try {
                core.db.csDeleteRecord(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool FieldOK(string FieldName) {
            try {
                return core.db.csIsFieldSupported(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void GoFirst() {
            try {
                core.db.csGoFirst(cs, false);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetAddLink(string PresetNameValueList = "", bool AllowPaste = false) {
            try {
                return DbController.csGetRecordAddLink(core, cs, PresetNameValueList, AllowPaste);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool GetBoolean(string FieldName) {
            try {
                return core.db.csGetBoolean(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex); // "Unexpected error in cs.GetBoolean")
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override DateTime GetDate(string FieldName) {
            try {
                return core.db.csGetDate(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetEditLink(bool allowCut = false) {
            try {
                return core.db.csGetRecordEditLink(cs, allowCut);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetFilename(string fieldName, string OriginalFilename = "", string ContentName = "") {
            try {
                return core.db.csGetFieldFilename(cs, fieldName, OriginalFilename, ContentName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override int GetInteger(string FieldName) {
            try {
                return core.db.csGetInteger(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override double GetNumber(string FieldName) {
            try {
                return core.db.csGetNumber(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override int GetRowCount() {
            try {
                return core.db.csGetRowCount(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetSQL() {
            try {
                return core.db.csGetSource(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetText(string FieldName) {
            try {
                return core.db.csGet(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override string GetHtml(string FieldName) {
            try {
                return core.db.csGet(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        [Obsolete("Use getText for copy. getFilename for filename", false)]
        public override string GetTextFile(string FieldName) {
            try {
                return core.db.csGetText(cs, FieldName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void GoNext() {
            try {
                core.db.csGoNext(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool NextOK() {
            try {
                core.db.csGoNext(cs);
                return core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override bool OK() {
            try {
                return core.db.csOk(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void Save() {
            try {
                core.db.csSave(cs);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetField(string FieldName, object FieldValue) {
            try {
                string fieldString = FieldValue as string;
                if (fieldString != null) {
                    core.db.csSet(cs, FieldName, fieldString);
                } else {
                    int? fieldInt = FieldValue as int?;
                    if (fieldInt != null) {
                        core.db.csSet(cs, FieldName, fieldInt.GetValueOrDefault());
                    } else {
                        bool? fieldBool = FieldValue as bool?;
                        if (fieldBool != null) {
                            core.db.csSet(cs, FieldName, fieldBool.GetValueOrDefault());
                        } else {
                            DateTime? fieldDate = FieldValue as DateTime?;
                            if (fieldDate != null) {
                                core.db.csSet(cs, FieldName, fieldDate.GetValueOrDefault());
                            } else {
                                core.db.csSet(cs, FieldName, FieldValue.ToString());
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetField(string FieldName, string FieldValue) {
            try {
                core.db.csSet(cs, FieldName, FieldValue);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetFile(string FieldName, string Copy, string ContentName) {
            try {
                core.db.csSetTextFile(cs, FieldName, Copy, ContentName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public override void SetFormInput(string FieldName, string RequestName = "") {
            try {
                DbController.csSetFormInput(core, cs, FieldName, RequestName);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the value in the field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public override string GetValue(string fieldName) {
            return core.db.csGetValue(cs, fieldName);
        }

        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPCSClass() {
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }

}

