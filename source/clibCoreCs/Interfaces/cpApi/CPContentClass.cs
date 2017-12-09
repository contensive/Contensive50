
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
using Contensive.BaseClasses;
//
namespace Contensive.Core {
    public class CPContentClass : CPContentBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "D8D3D8F9-8459-46F7-B8AC-01B4DFAA4DB2";
        public const string InterfaceId = "9B321DE5-D154-4EB1-B533-DBA2E5F2B5D2";
        public const string EventsId = "6E068297-E09E-42C8-97B6-02DE591009DD";
        #endregion
        //
        private CPClass cp { get; set; }
        private Contensive.Core.coreClass cpCore { get; set; }
        protected bool disposed { get; set; } = false;
        //
        //====================================================================================================
        //
        public CPContentClass(CPClass cpParent) : base() {
            cp = cpParent;
            cpCore = cp.core;
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string CopyName, string DefaultContent = "") {
            return cpCore.html.html_GetContentCopy(CopyName, DefaultContent, cpCore.doc.authContext.user.id, true, cpCore.doc.authContext.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override string GetCopy(string CopyName, string DefaultContent, int personalizationPeopleId) {
            return cpCore.html.html_GetContentCopy(CopyName, DefaultContent, personalizationPeopleId, true, cpCore.doc.authContext.isAuthenticated);
        }
        //
        //====================================================================================================
        //
        public override void SetCopy(string CopyName, string Content) {
            cpCore.db.SetContentCopy(CopyName, Content);
        }
        //
        //====================================================================================================
        //
        public override string GetAddLink(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing) {
            return cpCore.html.main_GetRecordAddLink2(ContentName, PresetNameValueList, AllowPaste, IsEditing);
        }
        //
        //====================================================================================================
        //
        public override string GetContentControlCriteria(string ContentName) {
            return Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override string GetFieldProperty(string ContentName, string FieldName, string PropertyName) {
            return Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, FieldName, PropertyName);
        }
        //
        //====================================================================================================
        //
        public override int GetID(string ContentName) {
            return Models.Complex.cdefModel.getContentId(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override string GetProperty(string ContentName, string PropertyName) {
            return Models.Complex.cdefModel.GetContentProperty(cp.core, ContentName, PropertyName);
        }
        //
        //====================================================================================================
        //
        public override string GetDataSource(string ContentName) {
            return Models.Complex.cdefModel.getContentDataSource(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        public override string GetEditLink(string ContentName, string RecordID, bool AllowCut, string RecordName, bool IsEditing) {
            if (true) {
                return cpCore.html.main_GetRecordEditLink2(ContentName, genericController.EncodeInteger(RecordID), AllowCut, RecordName, IsEditing);
            } else {
                return "";
            }
        }
        //
        //====================================================================================================
        //
        public override string GetLinkAliasByPageID(int PageID, string QueryStringSuffix, string DefaultLink) {
            return docController.getLinkAlias(cpCore, PageID, QueryStringSuffix, DefaultLink);
        }
        //
        //====================================================================================================
        //
        public override string GetPageLink(int PageID, string QueryStringSuffix = "", bool AllowLinkAlias = true) {
            return pageContentController.getPageLink(cpCore, PageID, QueryStringSuffix, AllowLinkAlias, false);
        }
        //
        //====================================================================================================
        //
        public override int GetRecordID(string ContentName, string RecordName) {
            return cpCore.db.getRecordID(ContentName, RecordName);
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns the matching record name if a match is found, otherwise blank. Does NOT validate the record.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        public override string GetRecordName(string ContentName, int RecordID) {
            return cpCore.db.getRecordName(ContentName, RecordID);
        }
        //
        //====================================================================================================
        //
        public override string GetTable(string ContentName) {
            return Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
        }
        //
        //====================================================================================================
        //
        [Obsolete("Deprecated, template link is not supported", true)]
        public override string GetTemplateLink(int TemplateID) {
            return ""; // cpCore.doc.getTemplateLink(TemplateID)
        }
        //
        //====================================================================================================
        //
        public override bool IsField(string ContentName, string FieldName) {
            return Models.Complex.cdefModel.isContentFieldSupported(cpCore, ContentName, FieldName);
        }
        //
        //====================================================================================================
        //
        public override bool IsLocked(string ContentName, string RecordID) {
            return cpCore.workflow.isRecordLocked(ContentName, genericController.EncodeInteger(RecordID), 0);
        }
        //
        //====================================================================================================
        //
        public override bool IsChildContent(string ChildContentID, string ParentContentID) {
            return Models.Complex.cdefModel.isWithinContent(cp.core, genericController.EncodeInteger(ChildContentID), genericController.EncodeInteger(ParentContentID));
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override bool IsWorkflow(string ContentName) {
            //
            // -- workflow no longer supported (but may come back)
            return false;
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void PublishEdit(string ContentName, int RecordID) {
            // Call cpCore.workflow.publishEdit(ContentName, RecordID, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void SubmitEdit(string ContentName, int RecordID) {
            //Call cpCore.workflow.submitEdit2(ContentName, RecordID, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void AbortEdit(string ContentName, int RecordId) {
            // Call cpCore.workflow.abortEdit2(ContentName, RecordId, 0)
        }
        //
        //====================================================================================================
        //
        [Obsolete("workflow editing is deprecated", false)]
        public override void ApproveEdit(string ContentName, int RecordId) {
            //Call cpCore.workflow.approveEdit(ContentName, RecordId, 0)
        }
        //
        //====================================================================================================
        //
        public override string getLayout(string layoutName) {
            string result = "";
            try {
                csController cs = new csController(cpCore);
                cs.open("layouts", "name=" + cp.Db.EncodeSQLText(layoutName), "id", false, "layout");
                if (cs.OK()) {
                    result = cs.getText("layout");
                }
                cs.Close();
            } catch (Exception ex) {
                cpCore.handleException(ex); // "Unexpected error in getLayout")
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        public override int AddRecord(string ContentName, string recordName) {
            int recordId = 0;
            try {
                csController cs = new csController(cpCore);
                if (cs.Insert(ContentName)) {
                    cs.setField("name", recordName);
                    recordId = cs.getInteger("id");
                }
                cs.Close();
            } catch (Exception ex) {
                cpCore.handleException(ex); // "Unexpected error in AddRecord")
                throw;
            }
            return recordId;
        }
        //
        //====================================================================================================
        //
        [Obsolete("Please use AddRecord( ContentName as string) ", true)]
        public override int AddRecord(object ContentName) {
            return AddRecord(cp.Utils.EncodeText(ContentName));
        }
        public override int AddRecord(string ContentName) {
            int recordId = 0;
            try {
                csController cs = new csController(cpCore);
                if (cs.Insert(ContentName)) {
                    recordId = cs.getInteger("id");
                }
                cs.Close();
            } catch (Exception ex) {
                cpCore.handleException(ex); // "Unexpected error in AddRecord")
                throw;
            }
            return recordId;
        }
        //
        //====================================================================================================
        //
        public override void Delete(string ContentName, string SQLCriteria) {
            cpCore.db.deleteContentRecords(ContentName, SQLCriteria);
        }
        //
        //====================================================================================================
        //
        public override void DeleteContent(string ContentName) {
            contentModel.delete(cpCore, Models.Complex.cdefModel.getContentId(cpCore, ContentName));
        }
        //
        //====================================================================================================
        //
        public override int AddContentField(string ContentName, string FieldName, int FieldType) {
            Models.Complex.CDefFieldModel field = new Models.Complex.CDefFieldModel();
            field.active = true;
            field.adminOnly = false;
            field.authorable = true;
            field.blockAccess = false;
            field.caption = FieldName;
            field.contentId = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
            field.developerOnly = false;
            field.editSortPriority = 9999;
            field.editTabName = "";
            field.fieldTypeId = FieldType;
            field.htmlContent = false;
            field.indexColumn = 0;
            field.indexSortDirection = 0;
            field.indexSortOrder = 0;
            field.indexWidth = "";
            field.installedByCollectionGuid = "";
            field.isBaseField = false;
            field.lookupContentID = 0;
            //field.lookupContentName = ""
            field.lookupList = "";
            field.manyToManyContentID = 0;
            field.set_ManyToManyContentName(cpCore, "");
            field.manyToManyRuleContentID = 0;
            field.set_ManyToManyRuleContentName(cpCore, "");
            field.ManyToManyRulePrimaryField = "";
            field.ManyToManyRuleSecondaryField = "";
            field.MemberSelectGroupID = 0;
            field.set_MemberSelectGroupName(cpCore, "");
            field.nameLc = FieldName.ToLower();
            field.Password = false;
            field.ReadOnly = false;
            field.RedirectContentID = 0;
            field.set_RedirectContentName(cpCore, "");
            field.RedirectID = "";
            field.RedirectPath = "";
            field.Required = false;
            field.Scramble = false;
            field.TextBuffered = false;
            field.UniqueName = false;
            return Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field);
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName) {
            return AddContent(ContentName, ContentName.Replace(' '.ToString(), "").Replace(' '.ToString(), ""), "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName, string sqlTableName) {
            return AddContent(ContentName, sqlTableName, "default");
        }
        //
        //====================================================================================================
        //
        public override int AddContent(string ContentName, string sqlTableName, string dataSourceName) {
            var tmpList = new List<string> { };
            dataSourceModel dataSource = dataSourceModel.createByName(cpCore, dataSourceName, ref tmpList);
            return Models.Complex.cdefModel.addContent(cpCore, true, dataSource, sqlTableName, ContentName, false, false, true, false, "", "sort order", "name", false, false, false, false, false, false, "", 0, 0, 0, "", false, "", true);
        }
        //
        //====================================================================================================
        //
        public override string GetListLink(string ContentName) {
            string returnHtml = "";
            try {
                string adminUrl = cp.Site.GetText("adminUrl") + "?cid=" + cp.Content.GetID(ContentName);
                string encodedCaption = "List Records in " + cp.Utils.EncodeHTML(ContentName);

                returnHtml = returnHtml + "<a"
                    + " class=\"ccRecordEditLink\" "
                    + " TabIndex=-1"
                    + " href=\"" + adminUrl + "\""
                    + "><img"
                    + " src=\"/ccLib/images/IconContentEdit.gif\""
                    + " border=\"0\""
                    + " alt=\"" + encodedCaption + "\""
                    + " title=\"" + encodedCaption + "\""
                    + " align=\"absmiddle\""
                    + "></a>";
            } catch (Exception ex) {
                cpCore.handleException(ex); // "Unexpected error in GetListLink")
                throw;
            }
            return returnHtml;
        }
        //
        //====================================================================================================
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cpCore = null;
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //====================================================================================================
        //
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPContentClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}
