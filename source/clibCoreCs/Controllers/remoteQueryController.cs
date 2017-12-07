
using System;
using System.Text.RegularExpressions;
using static Contensive.Core.constants;

namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// static class controller
	/// </summary>
	public class remoteQueryController : IDisposable
	{
		//
		//
		//
		//
		public static string main_GetRemoteQueryKey(coreClass cpCore, string SQL, string DataSourceName = "", int maxRows = 1000)
		{
			//
			int CS = 0;
			string RemoteKey = "";
			int DataSourceID = 0;
			//
			if (maxRows == 0)
			{
				maxRows = 1000;
			}
			CS = cpCore.db.csInsertRecord("Remote Queries");
			if (cpCore.db.csOk(CS))
			{
				RemoteKey = Guid.NewGuid().ToString();
				DataSourceID = cpCore.db.getRecordID("Data Sources", DataSourceName);
				cpCore.db.csSet(CS, "remotekey", RemoteKey);
				cpCore.db.csSet(CS, "datasourceid", DataSourceID);
				cpCore.db.csSet(CS, "sqlquery", SQL);
				cpCore.db.csSet(CS, "maxRows", maxRows);
				cpCore.db.csSet(CS, "dateexpires", cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.AddDays(1)));
				cpCore.db.csSet(CS, "QueryTypeID", QueryTypeSQL);
				cpCore.db.csSet(CS, "VisitId", cpCore.doc.authContext.visit.id);
			}
			cpCore.db.csClose(ref CS);
			//
			return RemoteKey;
		}
		//
		//
		//
		public static string main_FormatRemoteQueryOutput(coreClass cpCore, GoogleDataType gd, RemoteFormatEnum RemoteFormat)
		{
			//
			stringBuilderLegacyController s = null;
			string ColDelim = null;
			string RowDelim = null;
			int ColPtr = 0;
			int RowPtr = 0;
			//
			// Select output format
			//
			s = new stringBuilderLegacyController();
			switch (RemoteFormat)
			{
				case RemoteFormatEnum.RemoteFormatJsonNameValue:
					//
					//
					//
					s.Add("{");
					if (!gd.IsEmpty)
					{
						ColDelim = "";
						for (ColPtr = 0; ColPtr <= gd.col.GetUpperBound(0); ColPtr++)
						{
							s.Add(ColDelim + gd.col(ColPtr).Id + ":'" + gd.row(0).Cell(ColPtr).v + "'");
							ColDelim = ",";
						}
					}
					s.Add("}");
					break;
				case RemoteFormatEnum.RemoteFormatJsonNameArray:
					//
					//
					//
					s.Add("{");
					if (!gd.IsEmpty)
					{
						ColDelim = "";
						for (ColPtr = 0; ColPtr <= gd.col.GetUpperBound(0); ColPtr++)
						{
							s.Add(ColDelim + gd.col(ColPtr).Id + ":[");
							ColDelim = ",";
							RowDelim = "";
							for (RowPtr = 0; RowPtr <= gd.row.GetUpperBound(0); RowPtr++)
							{
								var tempVar = gd.row(RowPtr).Cell(ColPtr);
								s.Add(RowDelim + "'" + tempVar.v + "'");
								RowDelim = ",";
							}
							s.Add("]");
						}
					}
					s.Add("}");
					break;
				case RemoteFormatEnum.RemoteFormatJsonTable:
					//
					//
					//
					s.Add("{");
					if (!gd.IsEmpty)
					{
						s.Add("cols: [");
						ColDelim = "";
						for (ColPtr = 0; ColPtr <= gd.col.GetUpperBound(0); ColPtr++)
						{
							var tempVar2 = gd.col(ColPtr);
							s.Add(ColDelim + "{id: '" + genericController.EncodeJavascript(tempVar2.Id) + "', label: '" + genericController.EncodeJavascript(tempVar2.Label) + "', type: '" + genericController.EncodeJavascript(tempVar2.Type) + "'}");
							ColDelim = ",";
						}
						s.Add("],rows:[");
						RowDelim = "";
						for (RowPtr = 0; RowPtr <= gd.row.GetUpperBound(0); RowPtr++)
						{
							s.Add(RowDelim + "{c:[");
							RowDelim = ",";
							ColDelim = "";
							for (ColPtr = 0; ColPtr <= gd.col.GetUpperBound(0); ColPtr++)
							{
								var tempVar3 = gd.row(RowPtr).Cell(ColPtr);
								s.Add(ColDelim + "{v: '" + genericController.EncodeJavascript(tempVar3.v) + "'}");
								ColDelim = ",";
							}
							s.Add("]}");
						}
						s.Add("]");
					}
					s.Add("}");
					break;
			}
			return s.Text;
			//
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
		public void Dispose()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		//
		~remoteQueryController()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
		//
		//====================================================================================================
		/// <summary>
		/// dispose.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				this.disposed = true;
				if (disposing)
				{
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