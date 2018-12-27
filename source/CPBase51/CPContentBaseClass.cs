
using System;

namespace Contensive.BaseClasses {
    public abstract class CPContentBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
        /// </summary>
        /// <param name="CopyName"></param>
        /// <param name="DefaultContent"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetCopy(string CopyName, string DefaultContent);
        //
        public abstract string GetCopy(string CopyName);
        //
        //====================================================================================================
        /// <summary>
        /// Get the string from the 'Copy Content' record based on it's name. If the record does not exist it is created with the default value provided.
        /// </summary>
        /// <param name="CopyName"></param>
        /// <param name="DefaultContent"></param>
        /// <param name="personalizationPeopleId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetCopy(string CopyName, string DefaultContent, int personalizationPeopleId);
        //
        //====================================================================================================
        /// <summary>
        /// Set a string in a 'Copy Content' record. The record will be created or modified.
        /// </summary>
        /// <param name="CopyName"></param>
        /// <param name="Content"></param>
        /// <remarks></remarks>
        public abstract void SetCopy(string CopyName, string Content);
        //
        //====================================================================================================
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
        //
        //====================================================================================================
        /// <summary>
        /// Returns an SQL compatible where-clause which includes all the contentcontentid values allowed for this content name.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetContentControlCriteria(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns a named field property. Valid values for PropertyName are the field names of the 'Content Fields' content definition, also found as the columns in the ccfields table.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="PropertyName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetFieldProperty(string ContentName, string FieldName, string PropertyName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the content id given its name
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract int GetID(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the datasource name of the content given.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetDataSource(string ContentName);
        //
        //====================================================================================================
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
        //
        //====================================================================================================
        /// <summary>
        /// Returns the primary link alias for the record id and querystringsuffix. If no link alias exists, it defaultvalue is returned.
        /// </summary>
        /// <param name="PageID"></param>
        /// <param name="QueryStringSuffix">In the case where an add-on is on the page, there may be many unique documents possible from the one pageid. Each possible variation is determined by values in the querystring added by the cpcore.addon. These name=value pairs in Querystring format are used to identify additional link aliases.</param>
        /// <param name="DefaultLink">If no link alias is found, this value is returned.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetLinkAliasByPageID(int PageID, string QueryStringSuffix, string DefaultLink);
        //
        //====================================================================================================
        /// <summary>
        /// Return the appropriate link for a page.
        /// </summary>
        /// <param name="PageID"></param>
        /// <param name="QueryStringSuffix">If a link alias exists, this is used to lookup the correct alias. See GetLinkAliasByPageID for details. In other cases, this is added to the querystring.</param>
        /// <param name="AllowLinkAlias"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetPageLink(int PageID, string QueryStringSuffix, bool AllowLinkAlias);
        public abstract string GetPageLink(int PageID, string QueryStringSuffix);
        public abstract string GetPageLink(int PageID);
        //
        //====================================================================================================
        /// <summary>
        /// Return a record's ID given it's name. If duplicates exist, the first one ordered by ID is returned.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract int GetRecordID(string ContentName, string RecordName);
        //
        //====================================================================================================
        /// <summary>
        /// Return a records name given it's ID.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetRecordName(string ContentName, int RecordID);
        //
        //====================================================================================================
        /// <summary>
        /// Get the table used for a content definition.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetTable(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Used to test if a field exists in a content definition
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool IsField(string ContentName, string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns true if the record is currently being edited.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="RecordID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool IsLocked(string ContentName, string RecordID);
        //
        //====================================================================================================
        /// <summary>
        /// Returns true if the childcontentid is a child of the parentcontentid
        /// </summary>
        /// <param name="ChildContentID"></param>
        /// <param name="ParentContentID"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool IsChildContent(string ChildContentID, string ParentContentID);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the html layout field of a layout record
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string getLayout(string layoutName);
        //
        //====================================================================================================
        /// <summary>
        /// Inserts a record and returns the Id for the record
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
		public abstract int AddRecord(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Insert a record and set its name. REturn the id of the record.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="recordName"></param>
        /// <returns></returns>
		public abstract int AddRecord(string ContentName, string recordName);
        //
        //====================================================================================================
        /// <summary>
        /// Create a new content with sqlTablename and default fields on the default datasource. After the call, modify the content with the database model - Models.Db.ContentModel.create( cp, id )
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="sqlTableName"></param>
        /// <param name="dataSource"></param>
        /// <returns></returns>
		public abstract int AddContent(string ContentName, string sqlTableName, string dataSource);
        //
        //====================================================================================================
        /// <summary>
        /// Create a new content with sqlTablename and default fields. After the call, modify the content with the database model - Models.Db.ContentModel.create( cp, id )
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="sqlTableName"></param>
        /// <returns></returns>
        public abstract int AddContent(string ContentName, string sqlTableName);
        //
        //====================================================================================================
        /// <summary>
        /// Create a new content with default fields. sqlTablename created from contentName. After the call, modify the content with the database model - Models.Db.ContentModel.create( cp, id )
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        public abstract int AddContent(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Create a new field in an existing content, return the fieldid
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="FieldType"></param>
        /// <returns></returns>
        public abstract int AddContentField(string ContentName, string FieldName, int FieldType);
        //
        //====================================================================================================
        /// <summary>
        /// Delete a content from the system, sqlTable is left intact. Use db.DeleteTable to drop the table
        /// </summary>
        /// <param name="ContentName"></param>
        /// <remarks></remarks>
        public abstract void DeleteContent(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Delete records based from a table based on a content name and SQL criteria.
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="SQLCriteria"></param>
        /// <remarks></remarks>
        public abstract void Delete(string ContentName, string SQLCriteria);
        //
        //====================================================================================================
        /// <summary>
        /// Returns a linked icon to the admin list page for the content
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetListLink(string ContentName);
        //
        //====================================================================================================
        // deprecated
        //
        [Obsolete("Deprecated, template link is not supported", true)]
        public abstract string GetTemplateLink(int TemplateID);
        //
        [Obsolete("workflow editing is deprecated", true)]
        public abstract bool IsWorkflow(string ContentName);
        //
        [Obsolete("workflow editing is deprecated", true)]
        public abstract void PublishEdit(string ContentName, int RecordID);
        //
        [Obsolete("workflow editing is deprecated", true)]
        public abstract void SubmitEdit(string ContentName, int RecordID);
        //
        [Obsolete("workflow editing is deprecated", true)]
        public abstract void AbortEdit(string ContentName, int RecordId);
        //
        [Obsolete("workflow editing is deprecated", true)]
        public abstract void ApproveEdit(string ContentName, int RecordId);
        //
        [Obsolete("Please use AddRecord(ContentName as String)", true)]
        public abstract int AddRecord(object ContentName);
        //
        [Obsolete("Use models to access record fields)", true)]
        public abstract string GetProperty(string ContentName, string PropertyName);
    }

}

