
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPContentBaseClass
	{
		/// <summary>
		/// Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
		/// </summary>
		/// <param name="CopyName"></param>
		/// <param name="DefaultContent"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetCopy(string CopyName, string DefaultContent = "");
		/// <summary>
		/// Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
		/// </summary>
		/// <param name="CopyName"></param>
		/// <param name="DefaultContent"></param>
		/// <param name="personalizationPeopleId"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetCopy(string CopyName, string DefaultContent, int personalizationPeopleId);
		/// <summary>
		/// Set a string in a 'Copy Content' record. The record will be created or modified.
		/// </summary>
		/// <param name="CopyName"></param>
		/// <param name="Content"></param>
		/// <remarks></remarks>
		public abstract void SetCopy(string CopyName, string Content);
		/// <summary>
		/// Get an icon linked to the administration site which adds a new record to the content.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="PresetNameValueList">A comma delimited list of name=value pairs. Each name is a field name and the value is used to prepopulate the new record.</param>
		/// <param name="AllowPaste">If true and the content supports cut-paste from the public site, the returned string will include a cut icon.</param>
		/// <param name="IsEditing">If false, this call returns nothing. Set it true if IsEdiing( contentname ) is true.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetAddLink(string ContentName, string PresetNameValueList, bool AllowPaste, bool IsEditing);
		/// <summary>
		/// Returns an SQL compatible where-clause which includes all the contentcontentid values allowed for this content name.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetContentControlCriteria(string ContentName); 
		/// <summary>
		/// Returns a named field property. Valid values for PropertyName are the field names of the 'Content Fields' content definition, also found as the columns in the ccfields table.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="FieldName"></param>
		/// <param name="PropertyName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetFieldProperty(string ContentName, string FieldName, string PropertyName);
		/// <summary>
		/// Returns the content id given its name
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int GetID(string ContentName); 
		/// <summary>
		/// Returns a named content property. Valid values for PropertyName are the field names of the 'Content' content definition, also found as the columns in the ccfields table.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="PropertyName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetProperty(string ContentName, string PropertyName); 
		/// <summary>
		/// Returns the datasource name of the content given.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetDataSource(string ContentName); 
		/// <summary>
		/// Get an icon linked to the administration site which edits the referenced record. The record is identified by its ID. The recordname is only used for the display caption.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="RecordID"></param>
		/// <param name="AllowCut">If true and the content allows cut and paste, and cut icon will be included in the return string.</param>
		/// <param name="RecordName">Used as a caption for the label</param>
		/// <param name="IsEditing">If false, this call returns nothing. Set it true if IsEdiing( contentname ) is true.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetEditLink(string ContentName, string RecordID, bool AllowCut, string RecordName, bool IsEditing); 
		/// <summary>
		/// Returns the primary link alias for the record id and querystringsuffix. If no link alias exists, it defaultvalue is returned.
		/// </summary>
		/// <param name="PageID"></param>
		/// <param name="QueryStringSuffix">In the case where an add-on is on the page, there may be many unique documents possible from the one pageid. Each possible variation is determined by values in the querystring added by the cpcore.addon. These name=value pairs in Querystring format are used to identify additional link aliases.</param>
		/// <param name="DefaultLink">If no link alias is found, this value is returned.</param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetLinkAliasByPageID(int PageID, string QueryStringSuffix, string DefaultLink);
		/// <summary>
		/// Return the appropriate link for a page.
		/// </summary>
		/// <param name="PageID"></param>
		/// <param name="QueryStringSuffix">If a link alias exists, this is used to lookup the correct alias. See GetLinkAliasByPageID for details. In other cases, this is added to the querystring.</param>
		/// <param name="AllowLinkAlias"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetPageLink(int PageID, string QueryStringSuffix = "", bool AllowLinkAlias = true); 
		/// <summary>
		/// Return a record's ID given it's name. If duplicates exist, the first one ordered by ID is returned.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="RecordName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract int GetRecordID(string ContentName, string RecordName);
		/// <summary>
		/// Return a records name given it's ID.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="RecordID"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetRecordName(string ContentName, int RecordID);
		/// <summary>
		/// Get the table used for a content definition.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetTable(string ContentName);
        /// <summary>
        /// If a template uses a fixed URL, the returns the link associted with a template. Otherwise it returns a blank string.
        /// </summary>
        /// <param name="TemplateID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("Deprecated, template link is not supported", true)]
        public abstract string GetTemplateLink(int TemplateID);
		/// <summary>
		/// Used to test if a field exists in a content definition
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="FieldName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool IsField(string ContentName, string FieldName);
		/// <summary>
		/// Returns true if the record is currently being edited.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="RecordID"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool IsLocked(string ContentName, string RecordID); 
		/// <summary>
		/// Returns true if the childcontentid is a child of the parentcontentid
		/// </summary>
		/// <param name="ChildContentID"></param>
		/// <param name="ParentContentID"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract bool IsChildContent(string ChildContentID, string ParentContentID);
        /// <summary>
        /// Returns true if the content is currently using workflow editing.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        [Obsolete("workflow editing is deprecated", false)]
        public abstract bool IsWorkflow(string ContentName);
        /// <summary>
        /// If Workflow editing, the record is published.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <remarks></remarks>
        [Obsolete("workflow editing is deprecated", false)]
        public abstract void PublishEdit(string ContentName, int RecordID); 
        /// <summary>
        /// If workflow editing, the record is submitted for pushlishing
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <remarks></remarks>
        [Obsolete("workflow editing is deprecated", false)]
         public abstract void SubmitEdit(string ContentName, int RecordID);
        /// <summary>
        /// If workflow editing, edits to the record are aborted and the edit record is returned to the condition of hte live record. This condition is used in the Workflow publishing tool.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordId"></param>
        /// <remarks></remarks>
        [Obsolete("workflow editing is deprecated", false)]
        public abstract void AbortEdit(string ContentName, int RecordId);
		/// <summary>
		/// If workflow editing, the record is marked as approved for publishing. This condition is used in the Workflow publishing tool.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="RecordId"></param>
		/// <remarks></remarks>
		[Obsolete("workflow editing is deprecated", false)] public abstract void ApproveEdit(string ContentName, int RecordId); //Implements BaseClasses.CPContentBaseClass.ApproveEdit
		/// <summary>
		/// Returns the html layout field of a layout record
		/// </summary>
		/// <param name="layoutName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string getLayout(string layoutName);
		/// <summary>
		/// Inserts a record and returns the Id for the record
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		[Obsolete("Please use AddRecord(ContentName as String)", true)]
		public abstract int AddRecord(object ContentName);
		public abstract int AddRecord(string ContentName);
		public abstract int AddRecord(string ContentName, string recordName);
		/// <summary>
		/// Delete records based from a table based on a content name and SQL criteria.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <remarks></remarks>
		///
		public abstract int AddContent(string ContentName);
		public abstract int AddContent(string ContentName, string sqlTableName);
		public abstract int AddContent(string ContentName, string sqlTableName, string dataSource);
		/// <summary>
		/// Create a new field in an existing content, return the fieldid
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="FieldName"></param>
		/// <param name="FieldType"></param>
		/// <returns></returns>
		public abstract int AddContentField(string ContentName, string FieldName, int FieldType);
		/// <summary>
		/// Delete a content from the system, sqlTable is left intact. Use db.DeleteTable to drop the table
		/// </summary>
		/// <param name="ContentName"></param>
		/// <remarks></remarks>
		public abstract void DeleteContent(string ContentName);
		/// <summary>
		/// Delete records based from a table based on a content name and SQL criteria.
		/// </summary>
		/// <param name="ContentName"></param>
		/// <param name="SQLCriteria"></param>
		/// <remarks></remarks>
		public abstract void Delete(string ContentName, string SQLCriteria);
		/// <summary>
		/// Returns a linked icon to the admin list page for the content
		/// </summary>
		/// <param name="ContentName"></param>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string GetListLink(string ContentName);
	}

}

