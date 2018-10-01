
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
using static Contensive.Processor.constants;
//
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class ExportAsciiController : IDisposable {
        //
        // ----- constants
        //
        //Private Const invalidationDaysDefault As Double = 365
        //
        // ----- objects constructed that must be disposed
        //
        //Private cacheClient As Enyim.Caching.MemcachedClient
        //
        // ----- private instance storage
        //
        //Private remoteCacheDisabled As Boolean
        //
        //====================================================================================================
        //
        public static string exportAscii_GetAsciiExport(CoreController core, string ContentName, int PageSize = 1000, int PageNumber = 1) {
            string result = "";
            try {
                string Delimiter = null;
                string Copy = "";
                string TableName = null;
                int CSPointer = 0;
                string FieldNameVariant = null;
                string FieldName = null;
                string UcaseFieldName = null;
                string iContentName = null;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string TestFilename;
                //
                TestFilename = "AsciiExport" + GenericController.GetRandomInteger(core) + ".txt";
                //
                iContentName = GenericController.encodeText(ContentName);
                if (PageSize == 0) {
                    PageSize = 1000;
                }
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                //
                // ----- Check for special case iContentNames
                //
                core.webServer.setResponseContentType("text/plain");
                core.html.enableOutputBuffer(false);
                TableName = DbController.getDbObjectTableName(Models.Domain.CDefModel.getContentTablename(core, iContentName));
                switch (GenericController.vbUCase(TableName)) {
                    case "CCMEMBERS":
                        //
                        // ----- People and member content export
                        //
                        if (!core.session.isAuthenticatedAdmin(core)) {
                            sb.Append("Warning: You must be a site administrator to export this information.");
                        } else {
                            CSPointer = core.db.csOpen(iContentName, "", "ID", false, 0, false, false, "", PageSize, PageNumber);
                            //
                            // ----- print out the field names
                            //
                            if (core.db.csOk(CSPointer)) {
                                sb.Append("\"EID\"");
                                Delimiter = ",";
                                FieldNameVariant = core.db.csGetFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    FieldName = GenericController.encodeText(FieldNameVariant);
                                    UcaseFieldName = GenericController.vbUCase(FieldName);
                                    if ((UcaseFieldName != "USERNAME") & (UcaseFieldName != "PASSWORD")) {
                                        sb.Append(Delimiter + "\"" + FieldName + "\"");
                                    }
                                    FieldNameVariant = core.db.csGetNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                sb.Append("\r\n");
                            }
                            //
                            // ----- print out the values
                            //
                            while (core.db.csOk(CSPointer)) {
                                if (!(core.db.csGetBoolean(CSPointer, "Developer"))) {
                                    Copy = SecurityController.encodeToken(core, core.db.csGetInteger(CSPointer, "ID"), core.doc.profileStartTime);
                                    sb.Append("\"" + Copy + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = core.db.csGetFirstFieldName(CSPointer);
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        FieldName = GenericController.encodeText(FieldNameVariant);
                                        UcaseFieldName = GenericController.vbUCase(FieldName);
                                        if ((UcaseFieldName != "USERNAME") & (UcaseFieldName != "PASSWORD")) {
                                            Copy = core.db.csGet(CSPointer, FieldName);
                                            if (!string.IsNullOrEmpty(Copy)) {
                                                Copy = GenericController.vbReplace(Copy, "\"", "'");
                                                Copy = GenericController.vbReplace(Copy, "\r\n", " ");
                                                Copy = GenericController.vbReplace(Copy, "\r", " ");
                                                Copy = GenericController.vbReplace(Copy, "\n", " ");
                                            }
                                            sb.Append(Delimiter + "\"" + Copy + "\"");
                                        }
                                        FieldNameVariant = core.db.csGetNextFieldName(CSPointer);
                                        ///DoEvents
                                    }
                                    sb.Append("\r\n");
                                }
                                core.db.csGoNext(CSPointer);
                                ///DoEvents
                            }
                        }
                        // End Case
                        break;
                    default:
                        //
                        // ----- All other content
                        //
                        if (!core.session.isAuthenticatedContentManager(core, iContentName)) {
                            sb.Append("Error: You must be a content manager to export this data.");
                        } else {
                            CSPointer = core.db.csOpen(iContentName, "", "ID", false, 0, false, false, "", PageSize, PageNumber);
                            //
                            // ----- print out the field names
                            //
                            if (core.db.csOk(CSPointer)) {
                                Delimiter = "";
                                FieldNameVariant = core.db.csGetFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    core.appRootFiles.appendFile(TestFilename, Delimiter + "\"" + FieldNameVariant + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = core.db.csGetNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                core.appRootFiles.appendFile(TestFilename, "\r\n");
                            }
                            //
                            // ----- print out the values
                            //
                            while (core.db.csOk(CSPointer)) {
                                Delimiter = "";
                                FieldNameVariant = core.db.csGetFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    switch (core.db.csGetFieldTypeId(CSPointer, GenericController.encodeText(FieldNameVariant))) {
                                        case FieldTypeIdFileText:
                                        case FieldTypeIdFileCSS:
                                        case FieldTypeIdFileXML:
                                        case FieldTypeIdFileJavascript:
                                        case FieldTypeIdFileHTML:
                                            Copy = CsController.getTextEncoded(core, CSPointer, GenericController.encodeText(FieldNameVariant));
                                            break;
                                        case FieldTypeIdLookup:
                                            Copy = core.db.csGetLookup(CSPointer, GenericController.encodeText(FieldNameVariant));
                                            break;
                                        case FieldTypeIdRedirect:
                                        case FieldTypeIdManyToMany:
                                            break;
                                        default:
                                            Copy = core.db.csGetText(CSPointer, GenericController.encodeText(FieldNameVariant));
                                            break;
                                    }
                                    if (!string.IsNullOrEmpty(Copy)) {
                                        Copy = GenericController.vbReplace(Copy, "\"", "'");
                                        Copy = GenericController.vbReplace(Copy, "\r\n", " ");
                                        Copy = GenericController.vbReplace(Copy, "\r", " ");
                                        Copy = GenericController.vbReplace(Copy, "\n", " ");
                                    }
                                    core.appRootFiles.appendFile(TestFilename, Delimiter + "\"" + Copy + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = core.db.csGetNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                core.appRootFiles.appendFile(TestFilename, "\r\n");
                                core.db.csGoNext(CSPointer);
                                ///DoEvents
                            }
                        }
                        break;
                }
                result = core.appRootFiles.readFileText(TestFilename);
                core.appRootFiles.deleteFile(TestFilename);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }


        #region  IDisposable Support 
        //
        // this class must implement System.IDisposable
        // never throw an exception in dispose
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        //====================================================================================================
        //
        protected bool disposed = false;
        //
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~ExportAsciiController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //todo  NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    //If (cacheClient IsNot Nothing) Then
                    //    cacheClient.Dispose()
                    //End If
                }
                //
                // cleanup non-managed objects
                //
            }
        }
        #endregion
    }
    //
}