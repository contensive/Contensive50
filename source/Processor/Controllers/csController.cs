
using System;
using System.Collections.Generic;
using Contensive.Processor.Controllers;
using Contensive.BaseClasses;
//
namespace Contensive.Processor {
    //
    //==========================================================================================
    /// <summary>
    /// The prefered database access methods. A recordset-like construct. Create an instance and use it to read/write to the Db.
    /// </summary>
    public class CsController : IDisposable {
        //
        private CoreController core;
        private int csKey;
        private int openingMemberID;
        protected bool disposed = false;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public CsController(CoreController core) {
            this.core = core;
            //
            // -- capture userId at the time data opened
            openingMemberID = core.session.user.id;
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
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csInsertRecord(ContentName, openingMemberID);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int pageSize = 0, int PageNumber = 1) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly, 0, false, false, SelectFieldList, pageSize, PageNumber);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        // need a new cs method here... openForUpdate( optional id )
        //  creates a cs with no read data and an empty write buffer
        //  read buffer get() is blocked, but you can setField()
        //  cs.save() writes values, if id=0 it does insert, else just update
        //
        public bool openForUpdate(string ContentName, int recordId ) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpenForUpdate(ContentName, recordId);
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openSQL(string sql) {
            bool success = false;
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey);
                }
                csKey = core.db.csOpenSql(sql,"Default");
                success = core.db.csOk(csKey);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public void close(bool asyncSave = false) {
            try {
                if (csKey != -1) {
                    core.db.csClose(ref csKey, asyncSave);
                    csKey = -1;
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public bool getBoolean(string fieldName) {
            return core.db.csGetBoolean(csKey, fieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string fieldName) {
            return core.db.csGetDate(csKey, fieldName);
        }
        //
        //====================================================================================================
        public int getInteger(string fieldName) {
            return core.db.csGetInteger(csKey, fieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string fieldName) {
            return core.db.csGetNumber(csKey, fieldName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the text value stored in the field. For Lookup fields, this method returns the name of the foreign key record.
        /// For textFile fields, this method returns the filename.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public string getText(string fieldName) {
            string result = "";
            try {
                result = core.db.csGet(csKey, fieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public void goNext() {
            core.db.csGoNext(csKey);
        }
        //
        //====================================================================================================
        public bool ok() {
            return core.db.csOk(csKey);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            core.db.csSet(csKey, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFieldFilename( string fieldName, string filename ) {
            core.db.csSetFieldFilename(csKey, fieldName, filename);
        }
        //
        //========================================================================
        //
        public static string getTextEncoded(CoreController core, int CSPointer, string FieldName) {
            string ContentName = "";
            int RecordID = 0;
            if (core.db.csIsFieldSupported(CSPointer, "id") & core.db.csIsFieldSupported(CSPointer, "contentcontrolId")) {
                RecordID = core.db.csGetInteger(CSPointer, "id");
                ContentName = CdefController.getContentNameByID(core, core.db.csGetInteger(CSPointer, "contentcontrolId"));
            }
            string source = core.db.csGet(CSPointer, FieldName);
            return ActiveContentController.renderHtmlForWeb(core, source, ContentName, RecordID, core.session.user.id, "", 0, CPUtilsBaseClass.addonContext.ContextPage);
        }
        //
        //========================================================================
        //
        public string getValue(string FieldName) {
            string result = "";
            try {
                result = core.db.csGetValue(csKey, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
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
        ~CsController() {
            Dispose(false);
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
                    if (csKey > -1) {
                        core.db.csClose(ref csKey);
                    }
                }
                //
                // -- Add code here to release the unmanaged resource.
            }
            this.disposed = true;
        }
        #endregion
    }

}

