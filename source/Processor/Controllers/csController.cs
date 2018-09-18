
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
    public class csController : IDisposable {
        //
        private Contensive.Processor.Controllers.CoreController core;
        private int csPtr;
        private int openingMemberID;
        protected bool disposed = false;
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public csController(CoreController core) {
            this.core = core;
            openingMemberID = core.session.user.id;
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
                        core.db.csClose(ref csPtr);
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
                    core.db.csClose(ref csPtr);
                }
                csPtr = core.db.csInsertRecord(ContentName, openingMemberID);
                success = core.db.csOk(csPtr);
            } catch (Exception ex) {
                logController.handleError( core,ex);
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
                    core.db.csClose(ref csPtr);
                }
                csPtr = core.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly,0,false,false,SelectFieldList, pageSize, PageNumber);
                success = core.db.csOk(csPtr);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public bool openSQL(string sql) {
            bool success = false;
            try {
                if (csPtr != -1) {
                    core.db.csClose(ref csPtr);
                }
                csPtr = core.db.csOpenSql(sql,"Default");
                success = core.db.csOk(csPtr);
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        public void close() {
            try {
                if (csPtr != -1) {
                    core.db.csClose(ref csPtr);
                    csPtr = -1;
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        public bool getBoolean(string FieldName) {
            return core.db.csGetBoolean(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public DateTime getDate(string FieldName) {
            return core.db.csGetDate(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public int getInteger(string FieldName) {
            return core.db.csGetInteger(csPtr, FieldName);
        }
        //
        //====================================================================================================
        public double getNumber(string FieldName) {
            return core.db.csGetNumber(csPtr, FieldName);
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
                result = core.db.csGet(csPtr, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        public void goNext() {
            core.db.csGoNext(csPtr);
        }
        //
        //====================================================================================================
        public bool ok() {
            return core.db.csOk(csPtr);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, DateTime FieldValue) {
            core.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, bool FieldValue) {
            core.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, string FieldValue) {
            core.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field.
        /// </summary>
        public void setField(string FieldName, double FieldValue) {
            core.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// set the value for the field. If the field is backed by a file, the value will be saved to the file
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public void setField(string FieldName, int FieldValue) {
            core.db.csSet(csPtr, FieldName, FieldValue);
        }
        //
        //====================================================================================================
        /// <summary>
        /// if the field uses an underlying filename, use this method to set that filename. The content for the field will switch to that contained by the new file
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="filename"></param>
        public void setFieldFilename( string fieldName, string filename ) {
            core.db.csSetFieldFilename(csPtr, fieldName, filename);
        }
        //
        //========================================================================
        //
        public static string getTextEncoded(CoreController core, int CSPointer, string FieldName) {
            string ContentName = "";
            int RecordID = 0;
            if (core.db.csIsFieldSupported(CSPointer, "id") & core.db.csIsFieldSupported(CSPointer, "contentcontrolId")) {
                RecordID = core.db.csGetInteger(CSPointer, "id");
                ContentName = Models.Domain.CDefModel.getContentNameByID(core, core.db.csGetInteger(CSPointer, "contentcontrolId"));
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
                result = core.db.csGetValue(csPtr, FieldName);
                if (result == null) {
                    result = "";
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
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

