
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
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
                Models.Domain.CDefModel CDef = Models.Domain.CDefModel.getCdef(core, PageContentModel.contentName);
                bool IsEditLocked = core.workflow.GetEditLockStatus(PageContentModel.contentName, core.doc.pageController.page.id);
                string editLockMemberName = "";
                DateTime editLockDateExpires = default(DateTime);
                if (IsEditLocked) {
                    editLockMemberName = core.workflow.GetEditLockMemberName(PageContentModel.contentName, core.doc.pageController.page.id);
                    editLockDateExpires = GenericController.encodeDate(core.workflow.GetEditLockMemberName(PageContentModel.contentName, core.doc.pageController.page.id));
                }
                bool IsModified = false;
                bool IsSubmitted = false;
                bool IsApproved = false;
                string SubmittedMemberName = "";
                string ApprovedMemberName = "";
                string ModifiedMemberName = "";
                bool IsDeleted = false;
                bool IsInserted = false;
                bool IsRootPage = false;
                DateTime SubmittedDate = default(DateTime);
                DateTime ApprovedDate = default(DateTime);
                DateTime ModifiedDate = default(DateTime);
                core.doc.getAuthoringStatus(PageContentModel.contentName, core.doc.pageController.page.id, ref IsSubmitted, ref IsApproved, ref SubmittedMemberName, ref ApprovedMemberName, ref IsInserted, ref IsDeleted, ref IsModified, ref ModifiedMemberName, ref ModifiedDate, ref SubmittedDate, ref ApprovedDate);
                bool tempVar = false;
                bool tempVar2 = false;
                bool tempVar3 = false;
                bool tempVar4 = false;
                bool AllowInsert = false;
                bool AllowCancel = false;
                bool allowSave = false;
                bool AllowDelete = false;
                bool readOnlyField = false;
                core.doc.getAuthoringPermissions(PageContentModel.contentName, core.doc.pageController.page.id, ref AllowInsert, ref AllowCancel, ref allowSave, ref AllowDelete, ref tempVar, ref tempVar2, ref tempVar3, ref tempVar4, ref readOnlyField);
                bool AllowMarkReviewed = Models.Domain.CDefModel.isContentFieldSupported(core, PageContentModel.contentName, "DateReviewed");
                string OptionsPanelAuthoringStatus = core.session.getAuthoringStatusMessage(core, false, IsEditLocked, editLockMemberName, editLockDateExpires, IsApproved, ApprovedMemberName, IsSubmitted, SubmittedMemberName, IsDeleted, IsInserted, IsModified, ModifiedMemberName);
                //
                // Set Editing Authoring Control
                //
                core.workflow.SetEditLock(PageContentModel.contentName, core.doc.pageController.page.id);
                //
                // SubPanel: Authoring Status
                //
                string ButtonList = "";
                if (AllowCancel) {
                    ButtonList = ButtonList + "," + ButtonCancel;
                }
                if (allowSave) {
                    ButtonList = ButtonList + "," + ButtonSave + "," + ButtonOK;
                }
                if (AllowDelete && !IsRootPage) {
                    ButtonList = ButtonList + "," + ButtonDelete;
                }
                if (AllowInsert) {
                    ButtonList = ButtonList + "," + ButtonAddChildPage;
                }
                int page_ParentID = 0;
                if ((page_ParentID != 0) && AllowInsert) {
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
                if (core.doc.debug_iUserError != "") {
                    result += ""
                        + "\r<tr>"
                        + cr2 + "<td colspan=2 class=\"qeRow\"><div class=\"qeHeadCon\">" + ErrorController.getUserError(core) + "</div></td>"
                        + "\r</tr>";
                }
                if (readOnlyField) {
                    result += ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(core, PageContentModel.contentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, PageContentModel.contentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                } else {
                    result += ""
                    + "\r<tr>"
                    + cr2 + "<td colspan=\"2\" class=\"qeRow\">" + getQuickEditingBody(core, PageContentModel.contentName, OrderByClause, AllowPageList, true, rootPageId, readOnlyField, AllowReturnLink, PageContentModel.contentName, ArchivePages, contactMemberID) + "</td>"
                    + "\r</tr>";
                }
                result += "\r<tr>"
                    + cr2 + "<td class=\"qeRow qeLeft\" style=\"padding-top:10px;\">Name</td>"
                    + cr2 + "<td class=\"qeRow qeRight\">" + HtmlController.inputText(core, "name", core.doc.pageController.page.name, 1, 0, "", false, readOnlyField) + "</td>"
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
                        Link = "<a href=\"" + testPage.PageLink + "\">" + Link + "</a>";
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
                    instanceArguments = GenericController.convertAddonArgumentstoDocPropertiesList(core, core.doc.pageController.page.ChildListInstanceOptions),
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
            string pageCopy = core.doc.pageController.page.Copyfilename.content;
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