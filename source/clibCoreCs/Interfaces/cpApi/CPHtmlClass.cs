
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
namespace Contensive.Core {
    public class CPHtmlClass : BaseClasses.CPHtmlBaseClass, IDisposable {
        //
        #region COM GUIDs
        public const string ClassId = "637E3815-0DA6-4672-84E9-A319D85F2101";
        public const string InterfaceId = "24267471-9CE4-44F9-B4BD-8E9CE357D6E6";
        public const string EventsId = "4021B791-0F55-4841-90AE-64C7FAFB9756";
        #endregion
        //
        private CPClass cp;
        private Contensive.Core.coreClass cpCore;
        protected bool disposed = false;
        //
        // Constructor
        //
        public CPHtmlClass(CPClass cpParent) : base() {
            cp = cpParent;
            cpCore = cp.core;
        }
        //
        // dispose
        //
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                appendDebugLog(".dispose, dereference cp, main, csv");
                if (disposing) {
                    //
                    // call .dispose for managed objects
                    //
                    cp = null;
                }
                //
                // Add code here to release the unmanaged resource.
                //
            }
            this.disposed = true;
        }
        //
        //
        //
        private string BlockBase(string TagName, string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") {
            string s = "";
            //
            if (!string.IsNullOrEmpty(HtmlName)) {
                s += " name=\"" + HtmlName + "\"";
            }
            if (!string.IsNullOrEmpty(HtmlClass)) {
                s += " class=\"" + HtmlClass + "\"";
            }
            if (!string.IsNullOrEmpty(HtmlId)) {
                s += " id=\"" + HtmlId + "\"";
            }
            return "<" + TagName.Trim() + s + ">" + InnerHtml + "</" + TagName.Trim() + ">";
        }
        //
        //
        //
        public override string div(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.div
        {
            return BlockBase("div", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string p(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.p
        {
            return BlockBase("p", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string li(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.li
        {
            return BlockBase("li", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string ul(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.ul
        {
            return BlockBase("ul", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string ol(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.ol
        {
            return BlockBase("ol", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string CheckBox(string HtmlName, bool HtmlValue = false, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.CheckBox
        {
            return cpCore.html.html_GetFormInputCheckBox(HtmlName, HtmlValue.ToString(), HtmlId);
        }
        //
        //
        //
        public override string CheckList(string HtmlName, string PrimaryContentName, int PrimaryRecordId, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectSQLCriteria = "", string CaptionFieldName = "", bool IsReadOnly = false, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.CheckList
        {
            return cpCore.html.getCheckList2(HtmlName, PrimaryContentName, PrimaryRecordId, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName, SecondaryContentSelectSQLCriteria, CaptionFieldName, IsReadOnly);
        }
        //
        //
        //
        public override string Form(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "", string ActionQueryString = "", string Method = "post") //Inherits BaseClasses.CPHtmlBaseClass.Form
        {
            string FormStart = "";
            try {
                if (Method.ToLower() == "get") {
                    if (InnerHtml.IndexOf("type=\"file", 0, 1, StringComparison.OrdinalIgnoreCase) >= 0) {
                        cpCore.handleException(new ApplicationException("cp.html.form called with method=get can not contain an upload file (input type=file)"));
                    }
                    if (string.IsNullOrEmpty(ActionQueryString)) {
                        FormStart = cpCore.html.html_GetFormStart("", HtmlName, HtmlId, Method);
                    } else {
                        FormStart = cpCore.html.html_GetFormStart(ActionQueryString, HtmlName, HtmlId, Method);
                    }

                } else {
                    if (string.IsNullOrEmpty(ActionQueryString)) {
                        FormStart = cpCore.html.html_GetUploadFormStart();
                    } else {
                        FormStart = cpCore.html.html_GetUploadFormStart(ActionQueryString);
                    }
                    if (!string.IsNullOrEmpty(HtmlName)) {
                        FormStart = FormStart.Replace(">", " name=\"" + HtmlName + "\">");
                    }
                    if (!string.IsNullOrEmpty(HtmlClass)) {
                        FormStart = FormStart.Replace(">", " class=\"" + HtmlClass + "\">");
                    }
                    if (!string.IsNullOrEmpty(HtmlId)) {
                        FormStart = FormStart.Replace(">", " id=\"" + HtmlId + "\">");
                    }
                }
            } catch (Exception ex) {

            }
            return ""
                    + FormStart + InnerHtml + "</form>";
        }
        //
        //
        //
        public override string h1(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h1
        {
            return BlockBase("h1", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string h2(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h2
        {
            return BlockBase("h2", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string h3(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h3
        {
            return BlockBase("h3", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string h4(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h4
        {
            return BlockBase("h4", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string h5(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h5
        {
            return BlockBase("h5", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }
        //
        //
        //
        public override string h6(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.h6
        {
            return BlockBase("h6", InnerHtml, HtmlName, HtmlClass, HtmlId);
        }

        public override string RadioBox(string HtmlName, string HtmlValue, string CurrentValue, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.RadioBox
        {
            if (true) {
                return cpCore.html.html_GetFormInputRadioBox(HtmlName, HtmlValue, CurrentValue, HtmlId);
            } else {
                return "";
            }
        }
        //
        //
        //
        public override string SelectContent(string HtmlName, string HtmlValue, string ContentName, string SQLCriteria = "", string NoneCaption = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.SelectContent
        {
            string tempSelectContent = null;
            if (true) {
                tempSelectContent = cpCore.html.main_GetFormInputSelect(HtmlName, genericController.EncodeInteger(HtmlValue), ContentName, SQLCriteria, NoneCaption);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    tempSelectContent = tempSelectContent.Replace("<select ", "<select class=\"" + HtmlClass + "\" ");
                }
                if (!string.IsNullOrEmpty(HtmlId)) {
                    tempSelectContent = tempSelectContent.Replace("<select ", "<select id=\"" + HtmlId + "\" ");
                }
            } else {
                tempSelectContent = "";
            }
            return tempSelectContent;
        }

        public override string SelectList(string HtmlName, string HtmlValue, string OptionList, string NoneCaption = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.SelectList
        {
            string tempSelectList = null;
            if (true) {
                tempSelectList = cpCore.html.getInputSelectList(HtmlName, HtmlValue, OptionList, NoneCaption, HtmlId);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    tempSelectList = tempSelectList.Replace("<select ", "<select class=\"" + HtmlClass + "\" ");
                }
                return tempSelectList;
            } else {
                return "";
            }
            return tempSelectList;
        }
        //
        //
        //
        public override void ProcessCheckList(string HtmlName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName) //Inherits BaseClasses.CPHtmlBaseClass.ProcessCheckList
        {
            if (true) {
                cpCore.html.main_ProcessCheckList(HtmlName, PrimaryContentName, PrimaryRecordID, SecondaryContentName, RulesContentName, RulesPrimaryFieldname, RulesSecondaryFieldName);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// process an html file element to cdnFiles to the optional path. If the path is ommitted, the path "upload"
        /// </summary>
        /// <param name="HtmlName"></param>
        /// <param name="VirtualFilePath"></param>
        [Obsolete("Instead, use cp.cdeFiles.saveUpload() or similar fileSystem object.")]
        public override void ProcessInputFile(string HtmlName, string VirtualFilePath = "") //Inherits BaseClasses.CPHtmlBaseClass.ProcessInputFile
        {
            string ignoreFilename = "";
            cpCore.cdnFiles.upload(HtmlName, VirtualFilePath, ref ignoreFilename);
        }
        //'
        //'====================================================================================================
        //''' <summary>
        //''' process an html file element to a specified file system (cp.files.cdnFiles for example). The return path is returned.
        //''' </summary>
        //''' <param name="HtmlName"></param>
        //''' <param name="fileSystem"></param>
        //''' <param name="returnPathFilename"></param>
        //''' <returns></returns>
        //Public Overrides Function ProcessInputFile(HtmlName As String, fileSystem As CPFileSystemBaseClass, ByRef returnPathFilename As String) As Boolean
        //    Throw New NotImplementedException()
        //End Function
        //'
        //'====================================================================================================
        //''' <summary>
        //''' process an html file element to a specified file system and a specified path. The filename uploaded to that path is returned
        //''' </summary>
        //''' <param name="HtmlName"></param>
        //''' <param name="fileSystem"></param>
        //''' <param name="uploadFilePath"></param>
        //''' <param name="returnFilename"></param>
        //''' <returns></returns>
        //Public Overrides Function ProcessInputFile(HtmlName As String, fileSystem As CPFileSystemBaseClass, uploadFilePath As String, ByRef returnFilename As String) As Boolean
        //    Return cpCore.web_processFormInputFile(HtmlName, fileSystem, uploadFilePath, returnFilename)
        //End Function
        //
        //
        //
        public override string Hidden(string HtmlName, string HtmlValue, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.Hidden
        {
            if (true) {
                return cpCore.html.html_GetFormInputHidden(HtmlName, HtmlValue, HtmlId);
            } else {
                return "";
            }
        }
        //
        //
        //
        public override string InputDate(string HtmlName, string HtmlValue = "", string Width = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.InputDate
        {
            string returnValue = "";
            if (true) {
                returnValue = cpCore.html.html_GetFormInputDate(HtmlName, HtmlValue, Width, HtmlId);
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    returnValue = returnValue.Replace(">", " class=\"" + HtmlClass + "\">");
                }
            }
            return returnValue;
        }
        //
        //
        //
        public override string InputFile(string HtmlName, string HtmlClass = "", string HtmlId = "") {
            string returnValue = "";
            if (true) {
                returnValue = cpCore.html.html_GetFormInputFile2(HtmlName, HtmlId, HtmlClass);
            }
            return returnValue;
        }
        //
        //
        //
        public override string InputText(string HtmlName, string HtmlValue = "", string Height = "", string Width = "", bool IsPassword = false, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.InputText
        {
            string returnValue = "";
            if (true) {
                returnValue = cpCore.html.html_GetFormInputText2(HtmlName, HtmlValue, genericController.EncodeInteger(Height), genericController.EncodeInteger(Width), HtmlId, IsPassword, false, HtmlClass);
                returnValue = returnValue.Replace(" SIZE=\"60\"", "");
            }
            return returnValue;
        }
        //'
        //'
        //'
        //Public Overrides Function InputField(ByVal ContentName As String, ByVal FieldName As String, Optional ByVal HtmlName As String = "", Optional ByVal HtmlValue As String = "", Optional ByVal HtmlClass As String = "", Optional ByVal HtmlId As String = "", Optional ByVal HtmlStyle As String = "", Optional ByVal ManyToManySourceRecordID As Integer = 0) As String
        //    If True Then
        //        Return cmc.main_GetFormInputField(ContentName, FieldName, HtmlName, HtmlValue, HtmlClass, HtmlId, HtmlClass, ManyToManySourceRecordID)
        //    Else
        //        Return ""
        //    End If
        //End Function

        //
        //
        public override string InputTextExpandable(string HtmlName, string HtmlValue = "", int Rows = 0, string StyleWidth = "", bool IsPassword = false, string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.InputTextExpandable
        {
            if (true) {
                return cpCore.html.html_GetFormInputTextExpandable(HtmlName, HtmlValue, Rows, StyleWidth, HtmlId, IsPassword);
            } else {
                return "";
            }
        }
        //
        //
        //
        public override string SelectUser(string HtmlName, int HtmlValue, int GroupId, string NoneCaption = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.SelectUser
        {
            if (true) {
                return cpCore.html.getInputMemberSelect(HtmlName, HtmlValue, GroupId, NoneCaption, HtmlId);
            } else {
                return "";
            }
        }
        //
        //
        //
        public override string Indent(string SourceHtml, int TabCnt = 1) //Inherits BaseClasses.CPHtmlBaseClass.Indent
        {
            string tempIndent = null;
            //
            //   Indent every line by 1 tab
            //
            int posStart = 0;
            int posEnd = 0;
            string pre = null;
            string post = null;
            string target = null;
            //
            posStart = genericController.vbInstr(1, SourceHtml, "<![CDATA[", Microsoft.VisualBasic.CompareMethod.Text);
            if (posStart == 0) {
                //
                // no cdata
                //
                posStart = genericController.vbInstr(1, SourceHtml, "<textarea", Microsoft.VisualBasic.CompareMethod.Text);
                if (posStart == 0) {
                    //
                    // no textarea
                    //
                    if (TabCnt > 0 && TabCnt < 99) {
                        tempIndent = SourceHtml.Replace("\r\n", "\r\n" + new string(Convert.ToChar("\t"), TabCnt));
                    } else {
                        tempIndent = SourceHtml.Replace("\r\n", "\r\n\t");
                    }
                    //Indent = genericController.vbReplace(SourceHtml, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
                } else {
                    //
                    // text area found, isolate it and indent before and after
                    //
                    posEnd = genericController.vbInstr(posStart, SourceHtml, "</textarea>", Microsoft.VisualBasic.CompareMethod.Text);
                    pre = SourceHtml.Left( posStart - 1);
                    if (posEnd == 0) {
                        target = SourceHtml.Substring(posStart - 1);
                        post = "";
                    } else {
                        target = SourceHtml.Substring(posStart - 1, posEnd - posStart + ((string)("</textarea>")).Length);
                        post = SourceHtml.Substring((posEnd + ((string)("</textarea>")).Length) - 1);
                    }
                    tempIndent = Indent(pre, TabCnt) + target + Indent(post, TabCnt);
                }
            } else {
                //
                // cdata found, isolate it and indent before and after
                //
                posEnd = genericController.vbInstr(posStart, SourceHtml, "]]>", Microsoft.VisualBasic.CompareMethod.Text);
                pre = SourceHtml.Left( posStart - 1);
                if (posEnd == 0) {
                    target = SourceHtml.Substring(posStart - 1);
                    post = "";
                } else {
                    target = SourceHtml.Substring(posStart - 1, posEnd - posStart + ((string)("]]>")).Length);
                    post = SourceHtml.Substring(posEnd + 2);
                }
                tempIndent = Indent(pre, TabCnt) + target + Indent(post, TabCnt);
            }
            //If TabCnt > 0 And TabCnt < 99 Then
            //Return SourceHtml.Replace(vbCrLf, vbCrLf & New String(vbTab, TabCnt))
            //Else
            //Return SourceHtml.Replace(vbCrLf, vbCrLf & vbTab)
            //End If
            return tempIndent;
        }

        public override string InputWysiwyg(string HtmlName, string HtmlValue = "", BaseClasses.CPHtmlBaseClass.EditorUserScope UserScope = BaseClasses.CPHtmlBaseClass.EditorUserScope.CurrentUser, BaseClasses.CPHtmlBaseClass.EditorContentScope ContentScope = BaseClasses.CPHtmlBaseClass.EditorContentScope.Page, string Height = "", string Width = "", string HtmlClass = "", string HtmlId = "") //Inherits BaseClasses.CPHtmlBaseClass.InputWysiwyg
        {
            if (true) {
                return cpCore.html.getFormInputHTML(HtmlName, HtmlValue, Height, Width);
            } else {
                return "";
            }
        }
        //
        //
        //
        public override void AddEvent(string HtmlId, string DOMEvent, string JavaScript) {
            if (true) {
                cpCore.html.html_AddEvent(HtmlId, DOMEvent, JavaScript);
            }
        }
        //
        //
        //
        public override string Button(string HtmlName, string HtmlValue = "", string HtmlClass = "", string HtmlId = "") {
            string tempButton = null;
            if (true) {
                tempButton = cpCore.html.html_GetFormButton(HtmlValue, HtmlName, HtmlId, "");
                if (!string.IsNullOrEmpty(HtmlClass)) {
                    tempButton = tempButton.Replace(">", " class=\"" + HtmlClass + "\">");
                }
            } else {
                return "";
            }
            return tempButton;
        }
        //
        //
        //
        public override string adminHint(string innerHtml) {
            string returnString = innerHtml;
            try {
                if (cpCore.doc.authContext.isEditingAnything() | cpCore.doc.authContext.user.Admin) {
                    returnString = ""
                        + "<div class=\"ccHintWrapper\">"
                            + "<div  class=\"ccHintWrapperContent\">"
                            + "<b>Administrator</b>"
                            + "<BR>"
                            + "<BR>" + genericController.encodeText(innerHtml) + "</div>"
                        + "</div>";
                }
            } catch (Exception ex) {
                cpCore.handleException(ex, "Unexpected error in cp.html.adminHint()");
            }
            return returnString;
        }
        //
        //
        //
        private void appendDebugLog(string copy) {
            //My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.html, " & copy & vbCrLf, True)
            // 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        }
        //
        // testpoint
        //
        private void tp(string msg) {
            //Call appendDebugLog(msg)
        }
        #region  IDisposable Support 
        // Do not change or add Overridable to these methods.
        // Put cleanup code in Dispose(ByVal disposing As Boolean).
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~CPHtmlClass() {
            Dispose(false);
            //INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
            //base.Finalize();
        }
        #endregion
    }
}