using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Contensive.Core
{
	//
	// comVisible to be activeScript compatible
	//
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPCSClass.ClassId, CPCSClass.InterfaceId, CPCSClass.EventsId)]
	public class CPCSClass : CPCSBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "63745D9C-795E-4C01-BD6D-4BA35FC4A843";
		public const string InterfaceId = "3F1E7D2E-D697-47A8-A0D3-B625A906BF6A";
		public const string EventsId = "04B8E338-ABB7-44FE-A8DF-2681A36DCA46";
#endregion
		//
		private Contensive.Core.coreClass cpCore;
		private int cs;
		private int OpeningMemberID;
		private CPClass cp;
		protected bool disposed = false;
		//
		// Constructor - Initialize the Main and Csv objects
		//
		public CPCSClass(ref CPClass cpParent) : base()
		{
			cp = cpParent;
			cpCore = cp.core;
			cs = -1;
			OpeningMemberID = cpCore.doc.authContext.user.id;
		}
		//
		// dispose
		//
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					//
					// call .dispose for managed objects
					//
					try
					{
						if (cs != -1 && true)
						{
							cpCore.db.csClose(cs);
						}
						//If Not (False) Then
						//    Call cmc.asv.csv_CloseCS(CSPointer)
						//End If
					}
					catch (Exception ex)
					{
					}
					cpCore = null;
					cp = null;
				}
				//
				// Add code here to release the unmanaged resource.
				//
				//csv = Nothing
			}
			this.disposed = true;
		}
		//
		// Insert, called only from Processor41.CSInsert after initializing 
		//
		public override bool Insert(string ContentName)
		{
			bool success = false;
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				cs = cpCore.db.csInsertRecord(ContentName, OpeningMemberID);
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.insert")
				throw;
			}
			return success;
		}
		//
		//
		//
		public override bool OpenRecord(string ContentName, int recordId, string SelectFieldList = "", bool ActiveOnly = true)
		{
			bool success = false;
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				cs = cpCore.db.csOpen(ContentName, "id=" + recordId,, ActiveOnly,,,, SelectFieldList, 1, 1);
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenRecord")
				throw;
			}
			return success;
		}
		//
		//
		//
		public override bool Open(string ContentName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, string SelectFieldList = "", int pageSize = 10, int PageNumber = 1)
		{
			//Public Overrides Function Open(ByVal ContentName As String, Optional ByVal SQLCriteria As String = "", Optional ByVal SortFieldList As String = "", Optional ByVal ActiveOnly As Boolean = True, Optional ByVal SelectFieldList As String = "", Optional ByVal ignore As Integer = 10, Optional ByVal PageNumber As Integer = 1, Optional pageSize As Integer = 0) As Boolean
			bool success = false;
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				if ((pageSize == 0) || (pageSize == 10))
				{

					// -- (hack) fix for interface issue that has default value 0. later add new method and deprecate
					// -- pagesize=10, pageNumber=1 -- old code returns all records, new code only returns the first 10 records -- this in effect makes it not compatiblie
					// -- if I change new cpBase to default pagesize=9999, the change is breaking and old code does not run
					// -- when I changed new cpbase to pagesize default 0, and compiled code against it, it would not run on c41 because pagesize=0 is passed
					pageSize = 9999;
				}
				cs = cpCore.db.csOpen(ContentName, SQLCriteria, SortFieldList, ActiveOnly,,,, SelectFieldList, pageSize, PageNumber);
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.Open") : Throw
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override bool OpenGroupUsers(List<string> GroupList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1)
		{
			bool success = false;
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				cs = cpCore.db.csOpenGroupUsers(GroupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
				success = OK();
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenGroupUsers")
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override bool OpenGroupUsers(string GroupName, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1)
		{
			bool success = false;
			//
			try
			{
				List<string> groupList = new List<string>();
				groupList.Add(GroupName);
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				cs = cpCore.db.csOpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
				success = OK();
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenGroupUsers")
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override bool OpenGroupListUsers(string GroupCommaList, string SQLCriteria = "", string SortFieldList = "", bool ActiveOnly = true, int PageSize = 10, int PageNumber = 1)
		{
			List<string> groupList = new List<string>();
			//
			groupList.AddRange(GroupCommaList.Split(','));
			return OpenGroupUsers(groupList, SQLCriteria, SortFieldList, ActiveOnly, PageSize, PageNumber);
		}
		//
		//====================================================================================================
		//
		public override bool OpenSQL(string sql)
		{
			bool success = false;
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				cs = cpCore.db.csOpenSql_rev("default", sql);
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenSQL")
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override bool OpenSQL(string sql, string DataSourcename, int PageSize = 10, int PageNumber = 1)
		{
			bool success = false;
			//Dim swap As String
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				if (((string.IsNullOrEmpty(sql)) || (sql.ToLower() == "default")) & (!string.IsNullOrEmpty(DataSourcename)) & (DataSourcename.ToLower() != "default"))
				{
					//
					// support legacy calls were the arguments were was backwards (datasourcename is sql and vise-versa)
					//
					cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
				}
				else
				{
					cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
				}
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenSQL")
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override bool OpenSQL2(string sql, string DataSourcename = "default", int PageSize = 10, int PageNumber = 1)
		{
			bool success = false;
			//Dim swap As String
			//
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
				}
				if (((string.IsNullOrEmpty(sql)) || (sql.ToLower() == "default")) & (!string.IsNullOrEmpty(DataSourcename)) & (DataSourcename.ToLower() != "default"))
				{
					//
					// support legacy calls were the arguments were was backwards (datasourcename is sql and vise-versa)
					//
					cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
				}
				else
				{
					cs = cpCore.db.csOpenSql(sql, DataSourcename, PageSize, PageNumber);
				}
				success = cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.OpenSQL")
				throw;
			}
			return success;
		}
		//
		//====================================================================================================
		//
		public override void Close()
		{
			try
			{
				if (cs != -1)
				{
					cpCore.db.csClose(cs);
					cs = -1;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.Close")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override object GetFormInput(string ContentName, string FieldName, string Height = "", string Width = "", string HtmlId = "")
		{
			if (true)
			{
				return cpCore.html.html_GetFormInputCS(cs, ContentName, FieldName, Height, Width, HtmlId);
			}
			else
			{
				return "";
			}
		}
		//
		//====================================================================================================
		//
		public override void Delete()
		{
			try
			{
				cpCore.db.csDeleteRecord(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.Delete")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override bool FieldOK(string FieldName)
		{
			bool result = false;
			//
			try
			{
				result = cpCore.db.cs_isFieldSupported(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.FieldOK")
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		//
		public override void GoFirst()
		{
			try
			{
				cpCore.db.cs_goFirst(cs, false);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.Delete")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override string GetAddLink(string PresetNameValueList = "", bool AllowPaste = false)
		{
			object result = null;
			//
			try
			{
				result = cpCore.html.main_cs_getRecordAddLink(cs, PresetNameValueList, AllowPaste);
				if (result == null)
				{
					result = string.Empty;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.GetAddLink")
				throw;
				result = string.Empty;
			}
			return Convert.ToString(result);
		}
		//
		//====================================================================================================
		//
		public override bool GetBoolean(string FieldName)
		{
			bool result = false;
			//
			try
			{
				result = cpCore.db.csGetBoolean(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.GetBoolean")
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		//
		public override DateTime GetDate(string FieldName)
		{
			try
			{
				return cpCore.db.csGetDate(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return DateTime.MinValue;
		}
		//
		//====================================================================================================
		//
		public override string GetEditLink(bool allowCut = false)
		{
			try
			{
				return cpCore.db.csGetRecordEditLink(cs, allowCut);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		public override string GetFilename(string fieldName, string OriginalFilename = "", string ContentName = "")
		{
			try
			{
				return cpCore.db.csGetFilename(cs, fieldName, OriginalFilename, ContentName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		public override int GetInteger(string FieldName)
		{
			try
			{
				return cpCore.db.csGetInteger(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return Convert.ToInt32(string.Empty);
		}
		//
		//====================================================================================================
		//
		public override double GetNumber(string FieldName)
		{
			try
			{
				return cpCore.db.csGetNumber(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return Convert.ToDouble(string.Empty);
		}
		//
		//====================================================================================================
		//
		public override int GetRowCount()
		{
			try
			{
				return cpCore.db.csGetRowCount(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return Convert.ToInt32(string.Empty);
		}
		//
		//====================================================================================================
		//
		public override string GetSQL()
		{
			try
			{
				return cpCore.db.csGetSource(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		public override string GetText(string FieldName)
		{
			try
			{
				return cpCore.db.csGet(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		public override string GetHtml(string FieldName)
		{
			try
			{
				return cpCore.db.csGet(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		[Obsolete("Use getText for copy. getFilename for filename", false)]
		public override string GetTextFile(string FieldName)
		{
			try
			{
				return cpCore.db.csGetText(cs, FieldName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		//
		public override void GoNext()
		{
			try
			{
				cpCore.db.csGoNext(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override bool NextOK()
		{
			try
			{
				cpCore.db.csGoNext(cs);
				return cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return false;
		}
		//
		//====================================================================================================
		//
		public override bool OK()
		{
			try
			{
				return cpCore.db.csOk(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return false;
		}
		//
		//====================================================================================================
		//
		public override void Save()
		{
			try
			{
				cpCore.db.csSave2(cs);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override void SetField(string FieldName, object FieldValue)
		{
			try
			{
				cpCore.db.csSet(cs, FieldName, Convert.ToDateTime(FieldValue));
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.SetField")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override void SetField(string FieldName, string FieldValue)
		{
			try
			{
				cpCore.db.csSet(cs, FieldName, FieldValue);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.SetField")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override void SetFile(string FieldName, string Copy, string ContentName)
		{
			try
			{
				cpCore.db.csSetTextFile(cs, FieldName, Copy, ContentName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.SetFile")
				throw;
			}
		}
		//
		//====================================================================================================
		//
		public override void SetFormInput(string FieldName, string RequestName = "")
		{
			bool success = false;
			try
			{
				csController.cs_setFormInput(cpCore, cs, FieldName, RequestName);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex); // "Unexpected error in cs.SetFormInput")
				throw;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// Return the value in the field
		/// </summary>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public override string GetValue(string fieldName)
		{
			return cpCore.db.cs_getValue(cs, fieldName);
		}

#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPCSClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}

}

