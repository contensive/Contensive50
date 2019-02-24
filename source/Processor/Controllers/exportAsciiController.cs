
using System;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;

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
        public static string exportAscii_GetAsciiExport(CoreController core, string ContentName, int PageSize, int PageNumber) {
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
                TableName = DbController.getDbObjectTableName(MetadataController.getContentTablename(core, iContentName));
                switch (GenericController.vbUCase(TableName)) {
                    case "CCMEMBERS":
                        //
                        // ----- People and member content export
                        //
                        if (!core.session.isAuthenticatedAdmin(core)) {
                            sb.Append("Warning: You must be a site administrator to export this information.");
                        } else {
                            using (var csData = new CsModel(core)) {
                                csData.open(iContentName, "", "ID", false, 0, "", PageSize, PageNumber);
                                //
                                // ----- print out the field names
                                //
                                if (csData.ok()) {
                                    sb.Append("\"EID\"");
                                    Delimiter = ",";
                                    FieldNameVariant = csData.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        FieldName = GenericController.encodeText(FieldNameVariant);
                                        UcaseFieldName = GenericController.vbUCase(FieldName);
                                        if ((UcaseFieldName != "USERNAME") && (UcaseFieldName != "PASSWORD")) {
                                            sb.Append(Delimiter + "\"" + FieldName + "\"");
                                        }
                                        FieldNameVariant = csData.getNextFieldName();
                                        ///DoEvents
                                    }
                                    sb.Append("\r\n");
                                }
                                //
                                // ----- print out the values
                                //
                                while (csData.ok()) {
                                    if (!(csData.getBoolean("Developer"))) {
                                        Copy = SecurityController.encodeToken(core, csData.getInteger("ID"), core.doc.profileStartTime);
                                        sb.Append("\"" + Copy + "\"");
                                        Delimiter = ",";
                                        FieldNameVariant = csData.getFirstFieldName();
                                        while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                            FieldName = GenericController.encodeText(FieldNameVariant);
                                            UcaseFieldName = GenericController.vbUCase(FieldName);
                                            if ((UcaseFieldName != "USERNAME") && (UcaseFieldName != "PASSWORD")) {
                                                Copy = csData.getText(FieldName);
                                                if (!string.IsNullOrEmpty(Copy)) {
                                                    Copy = GenericController.vbReplace(Copy, "\"", "'");
                                                    Copy = GenericController.vbReplace(Copy, "\r\n", " ");
                                                    Copy = GenericController.vbReplace(Copy, "\r", " ");
                                                    Copy = GenericController.vbReplace(Copy, "\n", " ");
                                                }
                                                sb.Append(Delimiter + "\"" + Copy + "\"");
                                            }
                                            FieldNameVariant = csData.getNextFieldName();
                                        }
                                        sb.Append("\r\n");
                                    }
                                    csData.goNext();
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
                            using (var csData = new CsModel(core)) {
                                csData.open(iContentName, "", "ID", false, 0, "", PageSize, PageNumber);
                                //
                                // ----- print out the field names
                                if (csData.ok()) {
                                    Delimiter = "";
                                    FieldNameVariant = csData.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        core.wwwFiles.appendFile(TestFilename, Delimiter + "\"" + FieldNameVariant + "\"");
                                        Delimiter = ",";
                                        FieldNameVariant = csData.getNextFieldName();
                                    }
                                    core.wwwFiles.appendFile(TestFilename, "\r\n");
                                }
                                //
                                // ----- print out the values
                                while (csData.ok()) {
                                    Delimiter = "";
                                    FieldNameVariant = csData.getFirstFieldName();
                                    while (!string.IsNullOrEmpty(FieldNameVariant)) {
                                        switch (csData.getFieldTypeId(GenericController.encodeText(FieldNameVariant))) {
                                            case CPContentBaseClass.fileTypeIdEnum.FileText:
                                            case CPContentBaseClass.fileTypeIdEnum.FileCSS:
                                            case CPContentBaseClass.fileTypeIdEnum.FileXML:
                                            case CPContentBaseClass.fileTypeIdEnum.FileJavascript:
                                            case CPContentBaseClass.fileTypeIdEnum.FileHTML:
                                                Copy = csData.getTextEncoded(GenericController.encodeText(FieldNameVariant));
                                                break;
                                            case CPContentBaseClass.fileTypeIdEnum.Lookup:
                                                Copy = csData.getText(GenericController.encodeText(FieldNameVariant));
                                                break;
                                            case CPContentBaseClass.fileTypeIdEnum.Redirect:
                                            case CPContentBaseClass.fileTypeIdEnum.ManyToMany:
                                                break;
                                            default:
                                                Copy = csData.getText(GenericController.encodeText(FieldNameVariant));
                                                break;
                                        }
                                        if (!string.IsNullOrEmpty(Copy)) {
                                            Copy = GenericController.vbReplace(Copy, "\"", "'");
                                            Copy = GenericController.vbReplace(Copy, "\r\n", " ");
                                            Copy = GenericController.vbReplace(Copy, "\r", " ");
                                            Copy = GenericController.vbReplace(Copy, "\n", " ");
                                        }
                                        core.wwwFiles.appendFile(TestFilename, Delimiter + "\"" + Copy + "\"");
                                        Delimiter = ",";
                                        FieldNameVariant = csData.getNextFieldName();
                                        ///DoEvents
                                    }
                                    core.wwwFiles.appendFile(TestFilename, "\r\n");
                                    csData.goNext();
                                }
                            }
                        }
                        break;
                }
                result = core.wwwFiles.readFileText(TestFilename);
                core.wwwFiles.deleteFile(TestFilename);
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