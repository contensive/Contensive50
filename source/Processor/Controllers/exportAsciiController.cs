
using System;
using static Contensive.Processor.Constants;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class ExportAsciiController : IDisposable {
        //
        //====================================================================================================
        //
        public static string exportAscii_GetAsciiExport(CoreController core, string ContentName, int PageSize = 1000, int PageNumber = 1) {
            string result = "";
            try {
                string Delimiter = null;
                string Copy = "";
                string TableName = null;
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
                TableName = DbController.getDbObjectTableName(MetaController.getContentTablename(core, iContentName));
                switch (GenericController.vbUCase(TableName)) {
                    case "CCMEMBERS":
                        //
                        // ----- People and member content export
                        //
                        if (!core.session.isAuthenticatedAdmin(core)) {
                            sb.Append("Warning: You must be a site administrator to export this information.");
                        } else {
                            using (var csXfer = new CsModel(core)) {
                                csXfer.open(iContentName, "", "ID", false, 0, "", PageSize, PageNumber);
                                //
                                // ----- print out the field names
                                //
                                if (csXfer.ok()) {
                                    sb.Append("\"EID\"");
                                    Delimiter = ",";
                                    FieldNameVariant = csXfer.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        FieldName = GenericController.encodeText(FieldNameVariant);
                                        UcaseFieldName = GenericController.vbUCase(FieldName);
                                        if ((UcaseFieldName != "USERNAME") && (UcaseFieldName != "PASSWORD")) {
                                            sb.Append(Delimiter + "\"" + FieldName + "\"");
                                        }
                                        FieldNameVariant = csXfer.getNextFieldName();
                                        ///DoEvents
                                    }
                                    sb.Append("\r\n");
                                }
                                //
                                // ----- print out the values
                                //
                                while (csXfer.ok()) {
                                    if (!(csXfer.getBoolean("Developer"))) {
                                        Copy = SecurityController.encodeToken(core, csXfer.getInteger("ID"), core.doc.profileStartTime);
                                        sb.Append("\"" + Copy + "\"");
                                        Delimiter = ",";
                                        FieldNameVariant = csXfer.getFirstFieldName();
                                        while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                            FieldName = GenericController.encodeText(FieldNameVariant);
                                            UcaseFieldName = GenericController.vbUCase(FieldName);
                                            if ((UcaseFieldName != "USERNAME") && (UcaseFieldName != "PASSWORD")) {
                                                Copy = csXfer.getText(FieldName);
                                                if (!string.IsNullOrEmpty(Copy)) {
                                                    Copy = GenericController.vbReplace(Copy, "\"", "'");
                                                    Copy = GenericController.vbReplace(Copy, "\r\n", " ");
                                                    Copy = GenericController.vbReplace(Copy, "\r", " ");
                                                    Copy = GenericController.vbReplace(Copy, "\n", " ");
                                                }
                                                sb.Append(Delimiter + "\"" + Copy + "\"");
                                            }
                                            FieldNameVariant = csXfer.getNextFieldName();
                                        }
                                        sb.Append("\r\n");
                                    }
                                    csXfer.goNext();
                                }
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
                            using (var csXfer = new CsModel(core)) {
                                csXfer.open(iContentName, "", "ID", false, 0, "", PageSize, PageNumber);
                                //
                                // ----- print out the field names
                                if (csXfer.ok()) {
                                    Delimiter = "";
                                    FieldNameVariant = csXfer.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        core.appRootFiles.appendFile(TestFilename, Delimiter + "\"" + FieldNameVariant + "\"");
                                        Delimiter = ",";
                                        FieldNameVariant = csXfer.getNextFieldName();
                                    }
                                    core.appRootFiles.appendFile(TestFilename, "\r\n");
                                }
                                //
                                // ----- print out the values
                                while (csXfer.ok()) {
                                    Delimiter = "";
                                    FieldNameVariant = csXfer.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        switch (csXfer.getFieldTypeId(GenericController.encodeText(FieldNameVariant))) {
                                            case _fieldTypeIdFileText:
                                            case _fieldTypeIdFileCSS:
                                            case _fieldTypeIdFileXML:
                                            case _fieldTypeIdFileJavascript:
                                            case _fieldTypeIdFileHTML:
                                                Copy = csXfer.getTextEncoded(GenericController.encodeText(FieldNameVariant));
                                                break;
                                            case _fieldTypeIdLookup:
                                                Copy = csXfer.getText(GenericController.encodeText(FieldNameVariant));
                                                break;
                                            case _fieldTypeIdRedirect:
                                            case _fieldTypeIdManyToMany:
                                                break;
                                            default:
                                                Copy = csXfer.getText(GenericController.encodeText(FieldNameVariant));
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
                                        FieldNameVariant = csXfer.getNextFieldName();
                                        ///DoEvents
                                    }
                                    core.appRootFiles.appendFile(TestFilename, "\r\n");
                                    csXfer.goNext();
                                }
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
        //
        //====================================================================================================
        //
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