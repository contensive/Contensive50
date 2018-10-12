
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
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class EditorController : IDisposable {
        //
        //
        //========================================================================
        // ----- Process the active editor form
        //========================================================================
        //
        public static void processActiveEditor(CoreController core) {
            //
            int CS = 0;
            string Button = null;
            int ContentID = 0;
            string ContentName = null;
            int RecordID = 0;
            string FieldName = null;
            string ContentCopy = null;
            //
            Button = core.docProperties.getText("Button");
            switch (Button) {
                case ButtonCancel:
                    //
                    // ----- Do nothing, the form will reload with the previous contents
                    //
                    break;
                case ButtonSave:
                    //
                    // ----- read the form fields
                    //
                    ContentID = core.docProperties.getInteger("cid");
                    RecordID = core.docProperties.getInteger("id");
                    FieldName = core.docProperties.getText("fn");
                    ContentCopy = core.docProperties.getText("ContentCopy");
                    //
                    // ----- convert editor active edit icons
                    //
                    ContentCopy = ActiveContentController.processWysiwygResponseForSave(core,ContentCopy);
                    //
                    // ----- save the content
                    //
                    ContentName = CdefController.getContentNameByID(core, ContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        CS = core.db.csOpen(ContentName, "ID=" + core.db.encodeSQLNumber(RecordID), "", false);
                        if (core.db.csOk(CS)) {
                            core.db.csSet(CS, FieldName, ContentCopy);
                        }
                        core.db.csClose(ref CS);
                    }
                    break;
            }
        }
        //
        //========================================================================
        // Print the active editor form
        //========================================================================
        //
        public static string main_GetActiveEditor(CoreController core, string ContentName, int RecordID, string FieldName, string FormElements = "") {
            //
            int ContentID = 0;
            int CSPointer = 0;
            string Copy = null;
            string Stream = "";
            string ButtonPanel = null;
            string EditorPanel = null;
            string PanelCopy = null;
            //
            string intContentName = null;
            int intRecordId = 0;
            string strFieldName = null;
            //
            intContentName = GenericController.encodeText(ContentName);
            intRecordId = GenericController.encodeInteger(RecordID);
            strFieldName = GenericController.encodeText(FieldName);
            //
            EditorPanel = "";
            ContentID = CdefController.getContentId(core, intContentName);
            if ((ContentID < 1) || (intRecordId < 1) || (string.IsNullOrEmpty(strFieldName))) {
                PanelCopy = SpanClassAdminNormal + "The information you have selected can not be accessed.</span>";
                EditorPanel = EditorPanel + core.html.getPanel(PanelCopy);
            } else {
                intContentName = CdefController.getContentNameByID(core, ContentID);
                if (!string.IsNullOrEmpty(intContentName)) {
                    CSPointer = core.db.csOpen(intContentName, "ID=" + intRecordId);
                    if (!core.db.csOk(CSPointer)) {
                        PanelCopy = SpanClassAdminNormal + "The information you have selected can not be accessed.</span>";
                        EditorPanel = EditorPanel + core.html.getPanel(PanelCopy);
                    } else {
                        Copy = core.db.csGet(CSPointer, strFieldName);
                        EditorPanel = EditorPanel + HtmlController.inputHidden("Type", FormTypeActiveEditor);
                        EditorPanel = EditorPanel + HtmlController.inputHidden("cid", ContentID);
                        EditorPanel = EditorPanel + HtmlController.inputHidden("ID", intRecordId);
                        EditorPanel = EditorPanel + HtmlController.inputHidden("fn", strFieldName);
                        EditorPanel = EditorPanel + GenericController.encodeText(FormElements);
                        EditorPanel = EditorPanel + core.html.getFormInputHTML("ContentCopy", Copy, "3", "45", false, true);
                        //EditorPanel = EditorPanel & main_GetFormInputActiveContent( "ContentCopy", Copy, 3, 45)
                        ButtonPanel = core.html.getPanelButtons(ButtonCancel + "," + ButtonSave, "button");
                        EditorPanel = EditorPanel + ButtonPanel;
                    }
                    core.db.csClose(ref CSPointer);
                }
            }
            Stream = Stream + core.html.getPanelHeader("Contensive Active Content Editor");
            Stream = Stream + core.html.getPanel(EditorPanel);
            Stream = HtmlController.form(core, Stream );
            return Stream;
        }
        //
        //========================================================================
        // main_Get FieldEditorList
        //
        //   FieldEditorList is a comma delmited list of addonids, one for each fieldtype
        //   to use it, split the list on comma and use the fieldtype as index
        //========================================================================
        //
        public static string getFieldTypeDefaultEditorAddonIdList(CoreController core) {
            string result = "";
            try {
                string[] editorAddonIds = null;
                DataTable RS = null;
                int fieldTypeID = 0;
                //
                // load default editors into editors() - these are the editors used when there is no editorPreference
                //   editors(fieldtypeid) = addonid
                editorAddonIds = new string[FieldTypeIdMax + 1];
                string SQL = ""
                    + " select"
                    + " t.id as contentfieldtypeid"
                    + " ,t.editorAddonId"
                    + " from ccFieldTypes t"
                    + " left join ccaggregatefunctions a on a.id=t.editorAddonId"
                    + " where (t.active<>0)and(a.active<>0) order by t.id";
                RS = core.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    fieldTypeID = GenericController.encodeInteger(dr["contentfieldtypeid"]);
                    if (fieldTypeID <= FieldTypeIdMax) {
                        editorAddonIds[fieldTypeID] = GenericController.encodeText(dr["editorAddonId"]);
                    }
                }
                //
                // -- set any editors not specifically requested in fieldtype
                SQL = "select contentfieldtypeid, max(addonId) as editorAddonId from ccAddonContentFieldTypeRules group by contentfieldtypeid";
                RS = core.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    fieldTypeID = GenericController.encodeInteger(dr["contentfieldtypeid"]);
                    if (fieldTypeID <= FieldTypeIdMax) {
                        if (string.IsNullOrEmpty(editorAddonIds[fieldTypeID])) {
                            editorAddonIds[fieldTypeID] = GenericController.encodeText(dr["editorAddonId"]);
                        }

                    }
                }
                result = string.Join(",", editorAddonIds);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
            }
            return result;
        }
        //
        //====================================================================================================
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
        ~EditorController() {
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