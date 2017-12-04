

using System.Text.RegularExpressions;
// 

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class editorController : IDisposable {
        
        // 
        // 
        // ========================================================================
        //  ----- Process the active editor form
        // ========================================================================
        // 
        public static void processActiveEditor(coreClass cpCore) {
            // 
            int CS;
            string Button;
            int ContentID;
            string ContentName;
            int RecordID;
            string FieldName;
            string ContentCopy;
            // 
            Button = cpCore.docProperties.getText("Button");
            switch (Button) {
                case ButtonCancel:
                    // 
                    //  ----- Do nothing, the form will reload with the previous contents
                    // 
                    break;
                case ButtonSave:
                    // 
                    //  ----- read the form fields
                    // 
                    ContentID = cpCore.docProperties.getInteger("cid");
                    RecordID = cpCore.docProperties.getInteger("id");
                    FieldName = cpCore.docProperties.getText("fn");
                    ContentCopy = cpCore.docProperties.getText("ContentCopy");
                    // 
                    //  ----- convert editor active edit icons
                    // 
                    ContentCopy = cpCore.html.convertEditorResponseToActiveContent(ContentCopy);
                    // 
                    //  ----- save the content
                    // 
                    ContentName = models.complex.cdefmodel.getContentNameByID(cpcore, ContentID);
                    if ((ContentName != "")) {
                        CS = cpCore.db.csOpen(ContentName, ("ID=" + cpCore.db.encodeSQLNumber(RecordID)), ,, false);
                        if (cpCore.db.csOk(CS)) {
                            cpCore.db.csSet(CS, FieldName, ContentCopy);
                        }
                        
                        cpCore.db.csClose(CS);
                    }
                    
                    break;
            }
        }
        
        // 
        // ========================================================================
        //  Print the active editor form
        // ========================================================================
        // 
        public static string main_GetActiveEditor(coreClass cpcore, string ContentName, int RecordID, string FieldName, string FormElements, void =, void ) {
            // 
            // Warning!!! Optional parameters not supported
            int ContentID;
            int CSPointer;
            string Copy;
            string Stream = "";
            string ButtonPanel;
            string EditorPanel;
            string PanelCopy;
            // 
            string intContentName;
            int intRecordId;
            string strFieldName;
            // 
            intContentName = genericController.encodeText(ContentName);
            intRecordId = genericController.EncodeInteger(RecordID);
            strFieldName = genericController.encodeText(FieldName);
            // 
            EditorPanel = "";
            ContentID = models.complex.cdefmodel.getcontentid(cpcore, intContentName);
            if (((ContentID < 1) 
                        || ((intRecordId < 1) 
                        || (strFieldName == "")))) {
                PanelCopy = (SpanClassAdminNormal + "The information you have selected can not be accessed.</span>");
                EditorPanel = (EditorPanel + cpcore.html.main_GetPanel(PanelCopy));
            }
            else {
                intContentName = models.complex.cdefmodel.getContentNameByID(cpcore, ContentID);
                if ((intContentName != "")) {
                    CSPointer = cpcore.db.csOpen(intContentName, ("ID=" + intRecordId));
                    if (!cpcore.db.csOk(CSPointer)) {
                        PanelCopy = (SpanClassAdminNormal + "The information you have selected can not be accessed.</span>");
                        EditorPanel = (EditorPanel + cpcore.html.main_GetPanel(PanelCopy));
                    }
                    else {
                        Copy = cpcore.db.csGet(CSPointer, strFieldName);
                        EditorPanel = (EditorPanel + cpcore.html.html_GetFormInputHidden("Type", FormTypeActiveEditor));
                        EditorPanel = (EditorPanel + cpcore.html.html_GetFormInputHidden("cid", ContentID));
                        EditorPanel = (EditorPanel + cpcore.html.html_GetFormInputHidden("ID", intRecordId));
                        EditorPanel = (EditorPanel + cpcore.html.html_GetFormInputHidden("fn", strFieldName));
                        EditorPanel = (EditorPanel + genericController.encodeText(FormElements));
                        EditorPanel = (EditorPanel + cpcore.html.getFormInputHTML("ContentCopy", Copy, "3", "45", false, true));
                        // EditorPanel = EditorPanel & main_GetFormInputActiveContent( "ContentCopy", Copy, 3, 45)
                        ButtonPanel = cpcore.html.main_GetPanelButtons((ButtonCancel + ("," + ButtonSave)), "button");
                        EditorPanel = (EditorPanel + ButtonPanel);
                    }
                    
                    cpcore.db.csClose(CSPointer);
                }
                
            }
            
            Stream = (Stream + cpcore.html.main_GetPanelHeader("Contensive Active Content Editor"));
            Stream = (Stream + cpcore.html.main_GetPanel(EditorPanel));
            Stream = (cpcore.html.html_GetFormStart() 
                        + (Stream + cpcore.html.html_GetFormEnd()));
            return Stream;
        }
        
        // 
        // ========================================================================
        //  main_Get FieldEditorList
        // 
        //    FieldEditorList is a comma delmited list of addonids, one for each fieldtype
        //    to use it, split the list on comma and use the fieldtype as index
        // ========================================================================
        // 
        public static string getFieldTypeDefaultEditorAddonIdList(coreClass cpCore) {
            string result = "";
            try {
                string[] editorAddonIds;
                DataTable RS;
                int fieldTypeID;
                // 
                //  load default editors into editors() - these are the editors used when there is no editorPreference
                //    editors(fieldtypeid) = addonid
                object editorAddonIds;
                string SQL = ("" + (" select" + (" t.id as contentfieldtypeid" + (" ,t.editorAddonId" + (" from ccFieldTypes t" + (" left join ccaggregatefunctions a on a.id=t.editorAddonId" + " where (t.active<>0)and(a.active<>0) order by t.id"))))));
                RS = cpCore.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    fieldTypeID = genericController.EncodeInteger(dr["contentfieldtypeid"]);
                    if ((fieldTypeID <= FieldTypeIdMax)) {
                        editorAddonIds[fieldTypeID] = genericController.encodeText(dr["editorAddonId"]);
                    }
                    
                }
                
                // 
                //  -- set any editors not specifically requested in fieldtype
                SQL = "select contentfieldtypeid, max(addonId) as editorAddonId from ccAddonContentFieldTypeRules group by c" +
                "ontentfieldtypeid";
                RS = cpCore.db.executeQuery(SQL);
                foreach (DataRow dr in RS.Rows) {
                    fieldTypeID = genericController.EncodeInteger(dr["contentfieldtypeid"]);
                    if ((fieldTypeID <= FieldTypeIdMax)) {
                        if ((editorAddonIds[fieldTypeID] == "")) {
                            editorAddonIds[fieldTypeID] = genericController.encodeText(dr["editorAddonId"]);
                        }
                        
                    }
                    
                }
                
                result = Join(editorAddonIds, ",");
            }
            catch (Exception ex) {
                cpCore.handleException(ex);
            }
            
            return result;
        }
        
        // 
        //  this class must implement System.IDisposable
        //  never throw an exception in dispose
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        // ====================================================================================================
        // 
        protected bool disposed = false;
        
        public void Dispose() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        // 
        protected override void Finalize() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(false);
            base.Finalize();
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' dispose.
        // '' </summary>
        // '' <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    // If (cacheClient IsNot Nothing) Then
                    //     cacheClient.Dispose()
                    // End If
                }
                
                // 
                //  cleanup non-managed objects
                // 
            }
            
        }
    }
}