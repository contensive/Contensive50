
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class exportAsciiController : IDisposable {
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
        public static string exportAscii_GetAsciiExport(coreClass cpCore, string ContentName, int PageSize = 1000, int PageNumber = 1) {
            string result = "";
            try {
                string Delimiter = null;
                string Copy = string.Empty;
                string TableName = null;
                int CSPointer = 0;
                string FieldNameVariant = null;
                string FieldName = null;
                string UcaseFieldName = null;
                string iContentName = null;
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                string TestFilename;
                //
                TestFilename = "AsciiExport" + genericController.GetRandomInteger() + ".txt";
                //
                iContentName = genericController.encodeText(ContentName);
                if (PageSize == 0) {
                    PageSize = 1000;
                }
                if (PageNumber == 0) {
                    PageNumber = 1;
                }
                //
                // ----- Check for special case iContentNames
                //
                cpCore.webServer.setResponseContentType("text/plain");
                cpCore.html.enableOutputBuffer(false);
                TableName = genericController.GetDbObjectTableName(Models.Complex.cdefModel.getContentTablename(cpCore, iContentName));
                switch (genericController.vbUCase(TableName)) {
                    case "CCMEMBERS":
                        //
                        // ----- People and member content export
                        //
                        if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                            sb.Append("Warning: You must be a site administrator to export this information.");
                        } else {
                            CSPointer = cpCore.db.csOpen(iContentName, "", "ID", false, 0, false, false, "", PageSize, PageNumber);
                            //
                            // ----- print out the field names
                            //
                            if (cpCore.db.csOk(CSPointer)) {
                                sb.Append("\"EID\"");
                                Delimiter = ",";
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    FieldName = genericController.encodeText(FieldNameVariant);
                                    UcaseFieldName = genericController.vbUCase(FieldName);
                                    if ((UcaseFieldName != "USERNAME") & (UcaseFieldName != "PASSWORD")) {
                                        sb.Append(Delimiter + "\"" + FieldName + "\"");
                                    }
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                sb.Append(Environment.NewLine);
                            }
                            //
                            // ----- print out the values
                            //
                            while (cpCore.db.csOk(CSPointer)) {
                                if (!(cpCore.db.csGetBoolean(CSPointer, "Developer"))) {
                                    Copy = cpCore.security.encodeToken((cpCore.db.csGetInteger(CSPointer, "ID")), cpCore.doc.profileStartTime);
                                    sb.Append("\"" + Copy + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer);
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        FieldName = genericController.encodeText(FieldNameVariant);
                                        UcaseFieldName = genericController.vbUCase(FieldName);
                                        if ((UcaseFieldName != "USERNAME") & (UcaseFieldName != "PASSWORD")) {
                                            Copy = cpCore.db.csGet(CSPointer, FieldName);
                                            if (!string.IsNullOrEmpty(Copy)) {
                                                Copy = genericController.vbReplace(Copy, "\"", "'");
                                                Copy = genericController.vbReplace(Copy, Environment.NewLine, " ");
                                                Copy = genericController.vbReplace(Copy, "\r", " ");
                                                Copy = genericController.vbReplace(Copy, "\n", " ");
                                            }
                                            sb.Append(Delimiter + "\"" + Copy + "\"");
                                        }
                                        FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer);
                                        ///DoEvents
                                    }
                                    sb.Append(Environment.NewLine);
                                }
                                cpCore.db.csGoNext(CSPointer);
                                ///DoEvents
                            }
                        }
                        // End Case
                        break;
                    default:
                        //
                        // ----- All other content
                        //
                        if (!cpCore.doc.authContext.isAuthenticatedContentManager(cpCore, iContentName)) {
                            sb.Append("Error: You must be a content manager to export this data.");
                        } else {
                            CSPointer = cpCore.db.csOpen(iContentName, "", "ID", false, 0, false, false, "", PageSize, PageNumber);
                            //
                            // ----- print out the field names
                            //
                            if (cpCore.db.csOk(CSPointer)) {
                                Delimiter = "";
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    cpCore.appRootFiles.appendFile(TestFilename, Delimiter + "\"" + FieldNameVariant + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                cpCore.appRootFiles.appendFile(TestFilename, Environment.NewLine);
                            }
                            //
                            // ----- print out the values
                            //
                            while (cpCore.db.csOk(CSPointer)) {
                                Delimiter = "";
                                FieldNameVariant = cpCore.db.cs_getFirstFieldName(CSPointer);
                                while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                    switch (cpCore.db.cs_getFieldTypeId(CSPointer, genericController.encodeText(FieldNameVariant))) {
                                        case FieldTypeIdFileText:
                                        case FieldTypeIdFileCSS:
                                        case FieldTypeIdFileXML:
                                        case FieldTypeIdFileJavascript:
                                        case FieldTypeIdFileHTML:
                                            Copy = csController.getTextEncoded(cpCore, CSPointer, genericController.encodeText(FieldNameVariant));
                                            break;
                                        case FieldTypeIdLookup:
                                            Copy = cpCore.db.csGetLookup(CSPointer, genericController.encodeText(FieldNameVariant));
                                            break;
                                        case FieldTypeIdRedirect:
                                        case FieldTypeIdManyToMany:
                                            break;
                                        default:
                                            Copy = cpCore.db.csGetText(CSPointer, genericController.encodeText(FieldNameVariant));
                                            break;
                                    }
                                    if (!string.IsNullOrEmpty(Copy)) {
                                        Copy = genericController.vbReplace(Copy, "\"", "'");
                                        Copy = genericController.vbReplace(Copy, Environment.NewLine, " ");
                                        Copy = genericController.vbReplace(Copy, "\r", " ");
                                        Copy = genericController.vbReplace(Copy, "\n", " ");
                                    }
                                    cpCore.appRootFiles.appendFile(TestFilename, Delimiter + "\"" + Copy + "\"");
                                    Delimiter = ",";
                                    FieldNameVariant = cpCore.db.cs_getNextFieldName(CSPointer);
                                    ///DoEvents
                                }
                                cpCore.appRootFiles.appendFile(TestFilename, Environment.NewLine);
                                cpCore.db.csGoNext(CSPointer);
                                ///DoEvents
                            }
                        }
                        break;
                }
                result = cpCore.appRootFiles.readFile(TestFilename);
                cpCore.appRootFiles.deleteFile(TestFilename);
            } catch (Exception ex) {
                cpCore.handleException(ex);
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
        ~exportAsciiController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
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