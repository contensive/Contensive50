
using Contensive.BaseModels;
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    public abstract class CPAdminUIBaseClass {
        //
        //==========================================================================================
        /// <summary>
        /// Create a new instance of a Tool Form. Tool Forms are simple forms with key elements like buttons and header with a simple body
        /// </summary>
        /// <returns></returns>
        public abstract AdminUI.ToolFormBaseClass NewToolForm();
        //
        //==========================================================================================
        /// <summary>
        /// Create an html row that includes a caption, editor and optional help content
        /// </summary>
        /// <returns></returns>
        public abstract string GetEditRow(string caption, string editor);
        public abstract string GetEditRow(string caption, string editor, string help);
        public abstract string GetEditRow(string caption, string editor, string help, string htmlId);
        public abstract string GetEditRow(string caption, string editor, string help, string htmlId, bool required);
        public abstract string GetEditRow(string caption, string editor, string help, string htmlId, bool required, bool blockBottomRule);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a boolean field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId, bool readOnly);
        public abstract string GetBooleanEditor(string htmlName, bool htmlValue, string htmlId);
        public abstract string GetBooleanEditor(string htmlName, bool htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a boolean field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetCurrencyEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetCurrencyEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly);
        public abstract string GetCurrencyEditor(string htmlName, double? htmlValue, string htmlId);
        public abstract string GetCurrencyEditor(string htmlName, double? htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a datetime field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId, bool readOnly);
        public abstract string GetDateTimeEditor(string htmlName, DateTime? htmlValue, string htmlId);
        public abstract string GetDateTimeEditor(string htmlName, DateTime? htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a datetime field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetFileEditor(string htmlName, string currentPathFilename, string htmlId, bool readOnly, bool required);
        public abstract string GetFileEditor(string htmlName, string currentPathFilename, string htmlId, bool readOnly);
        public abstract string GetFileEditor(string htmlName, string currentPathFilename, string htmlId);
        public abstract string GetFileEditor(string htmlName, string currentPathFilename);
        public abstract string GetFileEditor(string htmlName);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a htmlcode field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId, bool readOnly);
        public abstract string GetHtmlCodeEditor(string htmlName, string htmlValue, string htmlId);
        public abstract string GetHtmlCodeEditor(string htmlName, string htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a html field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetHtmlEditor(string htmlName, string htmlValue, string htmlId, bool readOnly);
        public abstract string GetHtmlEditor(string htmlName, string htmlValue, string htmlId);
        public abstract string GetHtmlEditor(string htmlName, string htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a html field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetImageEditor(string htmlName, string currentPathFilename, string htmlId, bool readOnly, bool required);
        public abstract string GetImageEditor(string htmlName, string currentPathFilename, string htmlId, bool readOnly);
        public abstract string GetImageEditor(string htmlName, string currentPathFilename, string htmlId);
        public abstract string GetImageEditor(string htmlName, string currentPathFilename);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for an integer field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly);
        public abstract string GetIntegerEditor(string htmlName, int? htmlValue, string htmlId);
        public abstract string GetIntegerEditor(string htmlName, int? htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for an integer field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetLinkEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetLinkEditor(string htmlName, int? htmlValue, string htmlId, bool readOnly);
        public abstract string GetLinkEditor(string htmlName, int? htmlValue, string htmlId);
        public abstract string GetLinkEditor(string htmlName, int? htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for all text field types
        /// </summary>
        /// <returns></returns>
        public abstract string GetLongTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetLongTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly);
        public abstract string GetLongTextEditor(string htmlName, string htmlValue, string htmlId);
        public abstract string GetLongTextEditor(string htmlName, string htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a lookup content field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId, int lookupRecordId, string htmlId, bool readOnly, bool required, string sqlFilter);
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId, int lookupRecordId, string htmlId, bool readOnly, bool required);
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId, int lookupRecordId, string htmlId, bool readOnly);
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId, int lookupRecordId, string htmlId);
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId, int lookupRecordId);
        public abstract string GetLookupContentEditor(string htmlName, int lookupContentId);
        //
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName, int lookupRecordId, string htmlId, bool readOnly, bool required, string sqlFilter);
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName, int lookupRecordId, string htmlId, bool readOnly, bool required);
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName, int lookupRecordId, string htmlId, bool readOnly);
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName, int lookupRecordId, string htmlId);
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName, int lookupRecordId);
        public abstract string GetLookupContentEditor(string htmlName, string lookupContentName);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a lookup list content field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, int lookupListIndex, string htmlId, bool readOnly, bool required);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, int lookupListIndex, string htmlId, bool readOnly);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, int lookupListIndex, string htmlId);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, int lookupListIndex);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList);
        //
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, string lookupListName, string htmlId, bool readOnly, bool required);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, string lookupListName, string htmlId, bool readOnly);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, string lookupListName, string htmlId);
        public abstract string GetLookupListEditor(string htmlName, List<string> lookupList, string lookupListName);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a lookup list content field type
        /// </summary>
        /// <returns></returns>
        //public abstract string GetManyToManyCheckList(string htmlName, int lookupListIndex, List<string> lookupList, string htmlId, bool readOnly, bool required);
        //
        //public abstract string GetManyToManySelect(string htmlName, int lookupListIndex, List<string> lookupList, string htmlId, bool readOnly, bool required);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a member select content field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId, bool readOnly, bool required);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId, bool readOnly);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid, string htmlId);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, string groupGuid);
        //
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId, bool readOnly, bool required);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId, bool readOnly);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId, string htmlId);
        public abstract string GetMemberSelectEditor(string htmlName, int lookupPersonId, int groupId);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for a number field type
        /// </summary>
        /// <returns></returns>
        public abstract string GetNumberEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetNumberEditor(string htmlName, double? htmlValue, string htmlId, bool readOnly);
        public abstract string GetNumberEditor(string htmlName, double? htmlValue, string htmlId);
        public abstract string GetNumberEditor(string htmlName, double? htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for all text field types
        /// </summary>
        /// <returns></returns>
        public abstract string GetPasswordEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetPasswordEditor(string htmlName, string htmlValue, string htmlId, bool readOnly);
        public abstract string GetPasswordEditor(string htmlName, string htmlValue, string htmlId);
        public abstract string GetPasswordEditor(string htmlName, string htmlValue);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for all text field types
        /// </summary>
        /// <returns></returns>
        public abstract string GetSelectorStringEditor(string htmlName, string htmlValue, string selectorString, string htmlId, bool readOnly, bool required);
        public abstract string GetSelectorStringEditor(string htmlName, string htmlValue, string selectorString, string htmlId, bool readOnly);
        public abstract string GetSelectorStringEditor(string htmlName, string htmlValue, string selectorString, string htmlId);
        public abstract string GetSelectorStringEditor(string htmlName, string htmlValue, string selectorString);
        //
        //==========================================================================================
        /// <summary>
        /// Create an input for all text field types
        /// </summary>
        /// <returns></returns>
        public abstract string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly, bool required);
        public abstract string GetTextEditor(string htmlName, string htmlValue, string htmlId, bool readOnly);
        public abstract string GetTextEditor(string htmlName, string htmlValue, string htmlId);
        public abstract string GetTextEditor(string htmlName, string htmlValue);
        //
    }
}

