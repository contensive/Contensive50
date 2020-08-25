//========================================================================



//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPHtmlBaseClass
	{
		public enum EditorContentScope
		{
			Page = 1,
			Email = 2,
			PageTemplate = 3
		}
		public enum EditorUserScope
		{
			Developer = 1,
			Administrator = 2,
			ContentManager = 3,
			PublicUser = 4,
			CurrentUser = 5
		}
		public abstract string div(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string p(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string li(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "");
		public abstract string ul(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "");
		public abstract string ol(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "");
		public abstract string CheckBox(string HtmlName, bool HtmlValue = false, string HtmlClass = "", string HtmlId = "");
		public abstract string CheckList(string HtmlName, string PrimaryContentName, int PrimaryRecordId, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName, string SecondaryContentSelectSQLCriteria = "", string CaptionFieldName = "", bool IsReadOnly = false, string HtmlClass = "", string HtmlId = "");
		public abstract string Form(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "", string ActionQueryString = "", string Method = "post");
		public abstract string h1(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string h2(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string h3(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string h4(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string h5(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = ""); 
		public abstract string h6(string InnerHtml, string HtmlName = "", string HtmlClass = "", string HtmlId = "");
        public abstract string RadioBox(string HtmlName, string HtmlValue, string CurrentValue, string HtmlClass = "", string HtmlId = "");
        public abstract string RadioBox(string HtmlName, int HtmlValue, int CurrentValue, string HtmlClass = "", string HtmlId = "");
        public abstract string SelectContent(string HtmlName, string HtmlValue, string ContentName, string SQLCriteria = "", string NoneCaption = "", string HtmlClass = "", string HtmlId = "");
        public abstract string SelectList(string HtmlName, string HtmlValue, string OptionList, string NoneCaption = "", string HtmlClass = "", string HtmlId = "");
		public abstract void ProcessCheckList(string HtmlName, string PrimaryContentName, string PrimaryRecordID, string SecondaryContentName, string RulesContentName, string RulesPrimaryFieldname, string RulesSecondaryFieldName);
	    [Obsolete("Instead, use cp.cdeFiles.saveUpload() or similar fileSystem object.")] public abstract void ProcessInputFile(string HtmlName, string VirtualFilePath = "");
		public abstract string Hidden(string HtmlName, string HtmlValue, string HtmlClass = "", string HtmlId = "");
		public abstract string InputDate(string HtmlName, string HtmlValue = "", string Width = "", string HtmlClass = "", string HtmlId = "");
		public abstract string InputFile(string HtmlName, string HtmlClass = "", string HtmlId = "");
		public abstract string InputText(string HtmlName, string HtmlValue = "", string Height = "", string Width = "", bool IsPassword = false, string HtmlClass = "", string HtmlId = "");
		public abstract string InputTextExpandable(string HtmlName, string HtmlValue = "", int Rows = 0, string StyleWidth = "", bool IsPassword = false, string HtmlClass = "", string HtmlId = "");
		public abstract string SelectUser(string HtmlName, int HtmlValue, int GroupId, string NoneCaption = "", string HtmlClass = "", string HtmlId = "");
		public abstract string Indent(string SourceHtml, int TabCnt = 1);
		public abstract string InputWysiwyg(string HtmlName, string HtmlValue = "", BaseClasses.CPHtmlBaseClass.EditorUserScope UserScope = BaseClasses.CPHtmlBaseClass.EditorUserScope.CurrentUser, BaseClasses.CPHtmlBaseClass.EditorContentScope ContentScope = BaseClasses.CPHtmlBaseClass.EditorContentScope.Page, string Height = "", string Width = "", string HtmlClass = "", string HtmlId = "");
		public abstract void AddEvent(string HtmlId, string DOMEvent, string JavaScript);
		public abstract string Button(string HtmlName, string HtmlValue = "", string HtmlClass = "", string HtmlId = "");
		public abstract string adminHint(string innerHtml);
	}

}

