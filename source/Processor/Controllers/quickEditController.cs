
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
using Contensive.BaseClasses;
using System.Linq;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class QuickEditController : IDisposable {
        //
        //=============================================================================
        /// <summary>
        /// editor for page content inline editing
        /// </summary>
        /// <param name="core"></param>
        /// <param name="rootPageId"></param>
        /// <param name="OrderByClause"></param>
        /// <param name="AllowPageList"></param>
        /// <param name="AllowReturnLink"></param>
        /// <param name="ArchivePages"></param>
        /// <param name="contactMemberID"></param>
        /// <param name="childListSortMethodId"></param>
        /// <param name="main_AllowChildListComposite"></param>
        /// <param name="ArchivePage"></param>
        /// <returns></returns>
        internal static string getQuickEditing( CoreController core, int rootPageId, string OrderByClause, bool AllowPageList, bool AllowReturnLink, bool ArchivePages, int contactMemberID, int childListSortMethodId, bool main_AllowChildListComposite, bool ArchivePage) {
            string result = "";
            try {
                core.html.addStyleLink("/quickEditor/styles.css", "Quick Editor");
                //
                // -- First Active Record - Output Quick Editor form
                Models.Domain.ContentMetadataModel cdef = Models.Domain.ContentMetadataModel.createByUniqueName(core, PageContentModel.contentName);
                var pageContentTable = Models.Db.TableModel.create(core, cdef.id);
                var editLock = WorkflowController.getEditLock(core, pageContentTable.id, core.doc.pageController.page.id);
                WorkflowController.recordWorkflowStatusClass authoringStatus = WorkflowController.getWorkflowStatus(core, PageContentModel.contentName, core.doc.pageController.page.id);
                PermissionController.UserContentPermissions userContentPermissions = PermissionController.getUserContentPermissions(core, cdef);
                bool AllowMarkReviewed = DbModel.containsField<PageContentModel>("DateReviewed");
                string OptionsPanelAuthoringStatus = core.session.getAuthoringStatusMessage(core, false, editLock.isEditLocked, editLock.editLockByMemberName, editLock.editLockExpiresDate, authoringStatus.isWorkflowApproved, authoringStatus.workflowApprovedMemberName, authoringStatus.isWorkflowSubmitted, authoringStatus.workflowSubmittedMemberName, authoringStatus.isWorkflowDeleted, authoringStatus.isWorkflowInserted, authoringStatus.isWorkflowModified, authoringStatus.workflowModifiedByMemberName);
                //
                // Set Editing Authoring Control
                //
                WorkflowController.setEditLock( core, pageContentTable.id, core.doc.pageController.page.id);
                //
                // SubPanel: Authoring Status
                //
                string ButtonList = "";
                ButtonList = ButtonList + "," + ButtonCancel;
                if (userContentPermissions.allowSave) {
                    ButtonList = ButtonList + "," + ButtonSave + "," + ButtonOK;
                }
                if (userContentPermissions.allowDelete && (core.doc.pageController.pageToRootList.Count==1)) {
                    //
                    // -- allow delete and not root page
                    ButtonList = ButtonList + "," + ButtonDelete;
                }
                if (userContentPermissions.allowAdd) {
                    ButtonList = ButtonList + "," + ButtonAddChildPage;
                }
                int page_ParentID = 0;
                if ((page_ParentID != 0) && userContentPermissions.allowAdd) {
                    ButtonList = ButtonList + "," + ButtonAddSiblingPage;
                }
                if (AllowMarkReviewed) {
                    ButtonList = ButtonList + "," + ButtonMarkReviewed;
                }
                if (!string.IsNullOrEmpty(ButtonList)) {
                    ButtonList = ButtonList.Substring(1);
                    ButtonList = core.html.getPanelButtons(ButtonList, "Button");
                }
                //If OptionsPanelAuthoringStatus <> "" Then
                //    result = result & "" _
                //        & cr & "<tr>" _
                //        & cr2 & "<td colspan=2 class=""qeRow""><div class=""qeHeadCon"">" & OptionsPanelAuthoringStatus & "</div></td>" _
                //        & cr & "</tr>"
                //End If
                if (!core.doc.userErrorList.Count.Equals(0)) {
                    result += ""
                        + "\r<tr>"
                        + cr2 + "<td colspan=2 class=\"qeRow\"><div class=\"qeHeadCon\">" + ErrorController.getUserError(core) + "</div></td>"
                        + "\r</tr>";
                }
                if (!userContentPermissions.allowSave) {
                    result += ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(core, PageContentModel.contentName, OrderByClause, AllowPageList, true, rootPageId, !userContentPermissions.allowSave, AllowReturnLink, PageContentModel.contentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                } else {
                    result += ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(core, PageContentModel.contentName, OrderByClause, AllowPageList, true, rootPageId, !userContentPermissions.allowSave, AllowReturnLink, PageContentModel.contentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                }
                result += "\r<tr>"
                    + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:10px;\">Name</td>"
                    + cr2 + "<td class=\"qeRow qeRight\">" + HtmlController.inputText(core, "name", core.doc.pageController.page.name, 1, 0, "", false, !userContentPermissions.allowSave) + "</td>"
                    + "\r</tr>"
                    + "";
                string PageList = null;
                //
                // ----- Parent pages
                //
                if (core.doc.pageController.pageToRootList.Count == 1) {
                    PageList = "&nbsp;(there are no parent pages)";
                } else {
                    PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">Current Page</li></ul>";
                    foreach (PageContentModel testPage in Enumerable.Reverse(core.doc.pageController.pageToRootList)) {
                        string Link = testPage.name;
                        if (string.IsNullOrEmpty(Link)) {
                            Link = "no name #" + GenericController.encodeText(testPage.id);
                        }
                        Link = "<a href=\"" + testPage.pageLink + "\">" + Link + "</a>";
                        PageList = "<ul class=\"qeListUL\"><li class=\"qeListLI\">" + Link + PageList + "</li></ul>";
                    }
                }
                result += ""
                + "\r<tr>"
                + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:26px;\">Parent Pages</td>"
                + cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
                + "\r</tr>";
                //
                // ----- Child pages
                //
                AddonModel addon = AddonModel.create(core, addonGuidChildList);
                CPUtilsBaseClass.addonExecuteContext executeContext = new CPUtilsBaseClass.addonExecuteContext {
                    addonType = CPUtilsBaseClass.addonContext.ContextPage,
                    hostRecord = new CPUtilsBaseClass.addonExecuteHostRecordContext() {
                        contentName = PageContentModel.contentName,
                        fieldName = "",
                        recordId = core.doc.pageController.page.id
                    },
                    argumentKeyValuePairs = GenericController.convertQSNVAArgumentstoDocPropertiesList(core, core.doc.pageController.page.childListInstanceOptions),
                    instanceGuid = PageChildListInstanceID,
                    errorContextMessage = "calling child page addon in quick editing editor"
                };
                PageList = core.addon.execute(addon, executeContext);
                //PageList = core.addon.execute_legacy2(core.siteProperties.childListAddonID, "", page.ChildListInstanceOptions, CPUtilsBaseClass.addonContext.ContextPage, pageContentModel.contentName, page.id, "", PageChildListInstanceID, False, -1, "", AddonStatusOK, Nothing)
                if (GenericController.vbInstr(1, PageList, "<ul", 1) == 0) {
                    PageList = "(there are no child pages)";
                }
                result += "\r<tr>"
                    + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:36px;\">Child Pages</td>"
                    + cr2 + "<td class=\"qeRow qeRight\"><div class=\"qeListCon\">" + PageList + "</div></td>"
                    + "\r</tr>";
                result = ""
                    + "\r<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                    + GenericController.nop(result) + "\r</table>";
                result = ""
                    + ButtonList + result + ButtonList;
                result = core.html.getPanel(result);
                //
                // Form Wrapper
                //
                result += ""
                    + HtmlController.inputHidden("Type", FormTypePageAuthoring)
                    + HtmlController.inputHidden("ID", core.doc.pageController.page.id)
                    + HtmlController.inputHidden("ContentName", PageContentModel.contentName);
                result = HtmlController.formMultipart(core, result, core.webServer.requestQueryString, "", "ccForm");
                result = "<div class=\"ccCon\">" + result + "</div>";
            } catch (Exception ex) {
                LogController.handleError(core, ex);
            }
            return result;
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        internal static string getQuickEditingBody(CoreController core, string ContentName, string OrderByClause, bool AllowChildList, bool Authoring, int rootPageId, bool readOnlyField, bool AllowReturnLink, string RootPageContentName, bool ArchivePage, int contactMemberID) {
            string pageCopy = core.doc.pageController.page.copyfilename.content;
            //
            // ----- Page Copy
            //
            int FieldRows = core.userProperty.getInteger(ContentName + ".copyFilename.PixelHeight", 500);
            if (FieldRows < 50) {
                FieldRows = 50;
                core.userProperty.setProperty(ContentName + ".copyFilename.PixelHeight", FieldRows);
            }
            //
            // At this point we do now know the the template so we can not main_Get the stylelist.
            // Put in main_fpo_QuickEditing to be replaced after template known
            //
           core.doc.quickEditCopy = pageCopy;
            return html_quickEdit_fpo;
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
        ~QuickEditController() {
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