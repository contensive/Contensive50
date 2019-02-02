
using Contensive.BaseClasses;
using System;
using System.Collections.Generic;
using System.Data;
using static Contensive.Processor.Constants;

namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class EditorController : IDisposable {
        //
        //====================================================================================================
        /// <summary>
        /// Process the active editor form
        /// </summary>
        /// <param name="core"></param>
        public static void processActiveEditor(CoreController core) {
            //
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
                    ContentCopy = ActiveContentController.processWysiwygResponseForSave(core, ContentCopy);
                    //
                    // ----- save the content
                    //
                    ContentName = MetaController.getContentNameByID(core, ContentID);
                    if (!string.IsNullOrEmpty(ContentName)) {
                        using (var csData = new CsModel(core)) {
                            csData.open(ContentName, "ID=" + DbController.encodeSQLNumber(RecordID), "", false);
                            if (csData.ok()) {
                                csData.set(FieldName, ContentCopy);
                            }
                            csData.close();
                        }
                    }
                    break;
            }
        }
        //
        //====================================================================================================
        //
        public static string getActiveEditor(CoreController core, string ContentName, int RecordID, string FieldName, string FormElements = "") {
            //
            int ContentID = 0;
            string Copy = null;
            string Stream = "";
            string ButtonPanel = null;
            string EditorPanel = null;
            string PanelCopy = null;
            string intContentName = null;
            int intRecordId = 0;
            string strFieldName = null;
            //
            intContentName = GenericController.encodeText(ContentName);
            intRecordId = GenericController.encodeInteger(RecordID);
            strFieldName = GenericController.encodeText(FieldName);
            //
            EditorPanel = "";
            ContentID = Models.Domain.MetaModel.getContentId(core, intContentName);
            if ((ContentID < 1) || (intRecordId < 1) || (string.IsNullOrEmpty(strFieldName))) {
                PanelCopy = SpanClassAdminNormal + "The information you have selected can not be accessed.</span>";
                EditorPanel = EditorPanel + core.html.getPanel(PanelCopy);
            } else {
                intContentName = MetaController.getContentNameByID(core, ContentID);
                if (!string.IsNullOrEmpty(intContentName)) {
                    using (var csData = new CsModel(core)) {
                        csData.open(intContentName, "ID=" + intRecordId);
                        if (!csData.ok()) {
                            PanelCopy = SpanClassAdminNormal + "The information you have selected can not be accessed.</span>";
                            EditorPanel = EditorPanel + core.html.getPanel(PanelCopy);
                        } else {
                            Copy = csData.getText(strFieldName);
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
                        csData.close();
                    }
                }
            }
            Stream = Stream + core.html.getPanelHeader("Contensive Active Content Editor");
            Stream = Stream + core.html.getPanel(EditorPanel);
            Stream = HtmlController.form(core, Stream);
            return Stream;
        }
        //
        //====================================================================================================
        //
        public static Dictionary<int, int> getFieldTypeDefaultEditorAddonIdDictionary(CoreController core) {
            var result = new Dictionary<int, int>();
            try {
                //
                // load default editors into editors() - these are the editors used when there is no editorPreference
                //   editors(fieldtypeid) = addonid
                string SQL = ""
                    + " select"
                    + " t.id as contentfieldtypeid"
                    + " ,t.editorAddonId"
                    + " from ccFieldTypes t"
                    + " left join ccaggregatefunctions a on a.id=t.editorAddonId"
                    + " where (t.active<>0)and(a.active<>0) order by t.id";
                DataTable RS = core.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    int fieldTypeID = GenericController.encodeInteger(dr["contentfieldtypeid"]);
                    if ( !result.ContainsKey(fieldTypeID)) {
                        result.Add(fieldTypeID, GenericController.encodeInteger(dr["editorAddonId"]));
                    }
                }
                //
                // -- set any editors not specifically requested in fieldtype
                SQL = "select contentfieldtypeid, max(addonId) as editorAddonId from ccAddonContentFieldTypeRules group by contentfieldtypeid";
                RS = core.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    int fieldTypeID = GenericController.encodeInteger(dr["contentfieldtypeid"]);
                    if (!result.ContainsKey(fieldTypeID)) {
                        result.Add(fieldTypeID, GenericController.encodeInteger(dr["editorAddonId"]));
                    }
                }
                return result;
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                return null;
            }
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