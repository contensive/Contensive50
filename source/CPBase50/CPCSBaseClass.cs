
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    /// <summary>
    /// CP.CS - The secondary interface to execute queries on a sql database. Use dbModels when possible (does not use cdef metadata). To run queries, use executeQuery, executeNonQuery and executeNonQueryAsync.
    /// </summary>
    /// <remarks></remarks>
    public abstract class CPCSBaseClass {
        // todo cs.openRecord -- very important as it will use the cdef model + dbmodels so it will be cached - cs.open() cannot be cached.
        // todo in collection file, in cdef for each text field, include a maxtextlength that will be used throughout to prevent db truncation
        //
        //====================================================================================================
        /// <summary>
        /// Inserts a new content row
        /// </summary>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool Insert(string ContentName);
        //
        //====================================================================================================
        /// <summary>
        /// Opens a record set with the record specified by the recordId
        /// </summary>
        /// <param name="contentName"></param>
        /// <param name="recordId"></param>
        /// <param name="SelectFieldList"></param>
        /// <param name="activeOnly"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool OpenRecord(string contentName, int recordId, string SelectFieldList = "", bool activeOnly = true);
        //
        //====================================================================================================
        /// <summary>
        /// Opens a record set with the records specified by the sqlCriteria
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="SQLCriteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="SelectFieldList"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool Open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int PageSize = 10, int PageNumber = 1);
        //
        //====================================================================================================
        /// <summary>
        /// Opens a record set with user records that are in a Group
        /// </summary>
        /// <param name="GroupName"></param>
        /// <param name="SQLCriteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        //
        //====================================================================================================
        public abstract bool OpenGroupUsers(string GroupName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1);
        /// <summary>
        /// Opens a record set with user records that are in a Group
        /// </summary>
        /// <param name="GroupList"></param>
        /// <param name="SQLCriteria"></param>
        /// <param name="SortFieldList"></param>
        /// <param name="ActiveOnly"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        //
        //====================================================================================================
        public abstract bool OpenGroupUsers(List<string> GroupList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1);
        /// <summary>
        /// Opens a record set based on an sql statement
        /// </summary>
        /// <param name="SQL"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool OpenSQL(string SQL);
        //
        //====================================================================================================
        /// <summary>
        /// Opens a record set based on an sql statement
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="DataSourcename"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool OpenSQL(string SQL, string DataSourcename, int PageSize = 10, int PageNumber = 1);
        //
        //====================================================================================================
        /// <summary>
        /// Opens a record set based on an sql statement, (polymorphism is not supported by active scripting)
        /// </summary>
        /// <param name="SQL"></param>
        /// <param name="DataSourcename"></param>
        /// <param name="PageSize"></param>
        /// <param name="PageNumber"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool OpenSQL2(string SQL, string DataSourcename = "default", int PageSize = 10, int PageNumber = 1);
        //
        //====================================================================================================
        /// <summary>
        ///  Closes an open record set
        /// </summary>
        /// <remarks></remarks>
        public abstract void Close();
        //
        //====================================================================================================
        /// <summary>
        /// Returns a form input element based on a content field definition
        /// </summary>
        /// <param name="ContentName"></param>
        /// <param name="FieldName"></param>
        /// <param name="Height"></param>
        /// <param name="Width"></param>
        /// <param name="HtmlId"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract object GetFormInput(string ContentName, string FieldName, string Height = "", string Width = "", string HtmlId = "");
        //
        //====================================================================================================
        /// <summary>
        /// Deletes the current row
        /// </summary>
        /// <remarks></remarks>
        public abstract void Delete();
        //
        //====================================================================================================
        /// <summary>
        /// Returns true if the given field is valid for this record set
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool FieldOK(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Move to the first record in the current record set
        /// </summary>
        /// <remarks></remarks>
        public abstract void GoFirst();
        //
        //====================================================================================================
        /// <summary>
        /// Returns an icon linked to the add function in the admin site for this content
        /// </summary>
        /// <param name="PresetNameValueList"></param>
        /// <param name="AllowPaste"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetAddLink(string PresetNameValueList = "", bool AllowPaste = false);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the field value cast as a boolean
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool GetBoolean(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the field value cast as a date
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract DateTime GetDate(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns an icon linked to the edit function in the admin site for this content
        /// </summary>
        /// <param name="AllowCut"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetEditLink(bool AllowCut = false);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the filename for the field, if a filename is related to the field type. Use this call to create the appropriate filename when a new file is added. The filename with the appropriate path is created or returned. This file and path is relative to the site's content file path and does not include a leading slash. To use this file in a URL, prefix with cp.site.filepath.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="OriginalFilename"></param>
        /// <param name="ContentName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetFilename(string FieldName, string OriginalFilename = "", string ContentName = "");
        //
        //====================================================================================================
        /// <summary>
        /// Returns the field value cast as an integer
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract int GetInteger(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the field value cast as a number (double)
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract double GetNumber(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the number of rows in the result.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract int GetRowCount();
        //
        //====================================================================================================
        /// <summary>
        /// returns the query used to generate the results
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetSQL();
        //
        //====================================================================================================
        /// <summary>
        /// Returns the result and converts it to a text type. For field types that store text in files, the text is returned instead of the filename. These include textfile, cssfile, javascriptfile. For file types that do not contain text, the filename is returned. These include filetype and imagefiletype.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetText(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Returns the result of getText() after verifying it's content is valid for use in Html content. If the field is a fieldTypeHtml the content is returned without conversion. If the field is any other type, the content is HtmlEncoded first (> converted to &gt;, etc)
        /// </summary>
        /// <param name="FieldName"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract string GetHtml(string FieldName);
        //
        //====================================================================================================
        /// <summary>
        /// Move to the next record in a result set.
        /// </summary>
        /// <remarks></remarks>
        public abstract void GoNext();
        //
        //====================================================================================================
        /// <summary>
        /// Move to the next record in a result set and return true if the row is valid.
        /// </summary>
        /// <remarks></remarks>
        public abstract bool NextOK();
        //
        //====================================================================================================
        /// <summary>
        /// Returns true if there is valid data in the current row of the result set.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        public abstract bool OK();
        //
        //====================================================================================================
        /// <summary>
        /// Forces a save of any changes made to the current row. A save occurs automatically when the content set is closed or when it moves to another row.
        /// </summary>
        /// <remarks></remarks>
        public abstract void Save();
        //
        //====================================================================================================
        /// <summary>
        /// Sets a value in a field of the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        /// <remarks></remarks>
        public abstract void SetField(string FieldName, object FieldValue);
        //
        //====================================================================================================
        /// <summary>
        /// Sets a value in a field of the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public abstract void SetField(string FieldName, string FieldValue);
        //
        //====================================================================================================
        /// <summary>
        /// Sets a value in a field of the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public abstract void SetField(string FieldName, int FieldValue);
        //
        //====================================================================================================
        /// <summary>
        /// Sets a value in a field of the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public abstract void SetField(string FieldName, bool FieldValue);
        //
        //====================================================================================================
        /// <summary>
        /// Sets a value in a field of the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="FieldValue"></param>
        public abstract void SetField(string FieldName, DateTime FieldValue);
        //
        //====================================================================================================
        /// <summary>
        /// Processes a value from the incoming request to a field in the current row.
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="RequestName"></param>
        /// <remarks></remarks>
        public abstract void SetFormInput(string FieldName, string RequestName = "");
        //
        //====================================================================================================
        /// <summary>
        /// Return the value directly from the field, without the conversions associated with GetText().
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public abstract string GetValue(string fieldName);
        //
        //====================================================================================================
        // Deprecated
        //
        [Obsolete("Use OpenGroupUsers instead. The GroupCommaList is a comma delimited list of groups and cannot handle group names that include a comma.", false)]
        public abstract bool OpenGroupListUsers(string GroupCommaList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1);
        //
        [Obsolete("Use SetField for all field types that store data in files (textfile, cssfile, etc)")]
        public abstract void SetFile(string FieldName, string Copy, string ContentName);
        //
        [Obsolete("Use getText to get copy, getFilename to get file.", false)]
        public abstract string GetTextFile(string FieldName);

    }

}

