﻿//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;
using Newtonsoft.Json;

namespace Contensive.Core.Models.Entity
{
	public class visitModel
	{
		//
		//-- const
		public const string primaryContentName = "visits";
		private const string primaryContentTableName = "ccvisits";
		private const string primaryContentDataSource = "default"; //<----- set to datasource if not default
		//
		// -- instance properties
		public int id;
		public bool Active;
		public bool Bot;
		public string Browser;
		public string ccGuid;

		public int ContentControlID;
		public bool CookieSupport;
		public int CreatedBy;
		public int CreateKey;
		public DateTime DateAdded;



		public bool ExcludeFromAnalytics;
		public string HTTP_FROM;
		public string HTTP_REFERER;
		public string HTTP_VIA;
		public DateTime LastVisitTime;
		public int LoginAttempts;
		public int MemberID;
		public bool MemberNew;
		public bool Mobile;
		public int ModifiedBy;
		public DateTime ModifiedDate;
		public string Name;
		public int PageVisits;
		public string RefererPathPage;
		public string REMOTE_ADDR;
		public string RemoteName;
		public string SortOrder;
		public int StartDateValue;
		public DateTime StartTime;
		public DateTime StopTime;
		public int TimeToLastHit;
		public bool VerboseReporting;
		public bool VisitAuthenticated;
		public int VisitorID;
		public bool VisitorNew;
		//
		// -- publics not exposed to the UI (test/internal data)
		//<JsonIgnore> Public createKey As Integer
		//
		//====================================================================================================
		/// <summary>
		/// Create an empty object. needed for deserialization
		/// </summary>
		public visitModel()
		{
			//
		}
		//
		//====================================================================================================
		/// <summary>
		/// add a new recod to the db and open it. Starting a new model with this method will use the default
		/// values in Contensive metadata (active, contentcontrolid, etc)
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="cacheNameList"></param>
		/// <returns></returns>
		public static visitModel add(coreClass cpCore, ref List<string> cacheNameList)
		{
			visitModel result = null;
			try
			{
				result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, 0), cacheNameList);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordId">The id of the record to be read into the new object</param>
		/// <param name="cacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
		public static visitModel create(coreClass cpCore, int recordId, ref List<string> cacheNameList)
		{
			visitModel result = null;
			try
			{
				if (recordId > 0)
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
					result = cpCore.cache.getObject<visitModel>(cacheName);
					if (result == null)
					{
						result = loadObject(cpCore, "id=" + recordId.ToString(), ref cacheNameList);
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// open an existing object
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordGuid"></param>
		public static visitModel create(coreClass cpCore, string recordGuid, ref List<string> cacheNameList)
		{
			visitModel result = null;
			try
			{
				if (!string.IsNullOrEmpty(recordGuid))
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
					result = cpCore.cache.getObject<visitModel>(cacheName);
					if (result == null)
					{
						result = loadObject(cpCore, "ccGuid=" + cpCore.db.encodeSQLText(recordGuid), ref cacheNameList);
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// open an existing object
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="sqlCriteria"></param>
		private static visitModel loadObject(coreClass cpCore, string sqlCriteria, ref List<string> cacheNameList, string sortOrder = "id")
		{
			visitModel result = null;
			try
			{
				csController cs = new csController(cpCore);
				if (cs.open(primaryContentName, sqlCriteria, sortOrder))
				{
					result = new visitModel();
					//
					// -- populate result model
					result.id = cs.getInteger("ID");
					result.Active = cs.getBoolean("Active");
					result.Bot = cs.getBoolean("Bot");
					result.Browser = cs.getText("Browser");
					result.ccGuid = cs.getText("ccGuid");
					//
					result.ContentControlID = cs.getInteger("ContentControlID");
					result.CookieSupport = cs.getBoolean("CookieSupport");
					result.CreatedBy = cs.getInteger("CreatedBy");
					result.CreateKey = cs.getInteger("CreateKey");
					result.DateAdded = cs.getDate("DateAdded");
					//
					//
					//
					result.ExcludeFromAnalytics = cs.getBoolean("ExcludeFromAnalytics");
					result.HTTP_FROM = cs.getText("HTTP_FROM");
					result.HTTP_REFERER = cs.getText("HTTP_REFERER");
					result.HTTP_VIA = cs.getText("HTTP_VIA");
					result.LastVisitTime = cs.getDate("LastVisitTime");
					result.LoginAttempts = cs.getInteger("LoginAttempts");
					result.MemberID = cs.getInteger("MemberID");
					result.MemberNew = cs.getBoolean("MemberNew");
					result.Mobile = cs.getBoolean("Mobile");
					result.ModifiedBy = cs.getInteger("ModifiedBy");
					result.ModifiedDate = cs.getDate("ModifiedDate");
					result.Name = cs.getText("Name");
					result.PageVisits = cs.getInteger("PageVisits");
					result.RefererPathPage = cs.getText("RefererPathPage");
					result.REMOTE_ADDR = cs.getText("REMOTE_ADDR");
					result.RemoteName = cs.getText("RemoteName");
					result.SortOrder = cs.getText("SortOrder");
					result.StartDateValue = cs.getInteger("StartDateValue");
					result.StartTime = cs.getDate("StartTime");
					result.StopTime = cs.getDate("StopTime");
					result.TimeToLastHit = cs.getInteger("TimeToLastHit");
					result.VerboseReporting = cs.getBoolean("VerboseReporting");
					result.VisitAuthenticated = cs.getBoolean("VisitAuthenticated");
					result.VisitorID = cs.getInteger("VisitorID");
					result.VisitorNew = cs.getBoolean("VisitorNew");
					if (string.IsNullOrEmpty(result.ccGuid))
					{
						result.ccGuid = Controllers.genericController.getGUID();
					}
					if (result != null)
					{
						//
						// -- set primary and secondary caches
						// -- add all cachenames to the injected cachenamelist
						string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.id.ToString());
						cacheNameList.Add(cacheName0);
						cpCore.cache.setContent(cacheName0, result);
						//
						string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
						cacheNameList.Add(cacheName1);
						cpCore.cache.setPointer(cacheName1, cacheName0);
					}
				}
				cs.Close();
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// save the instance properties to a record with matching id. If id is not provided, a new record is created.
		/// </summary>
		/// <param name="cpCore"></param>
		/// <returns></returns>
		public int saveObject(coreClass cpCore)
		{
			try
			{
				csController cs = new csController(cpCore);
				if (id > 0)
				{
					if (!cs.open(primaryContentName, "id=" + id))
					{
						id = 0;
						cs.Close();
						throw new ApplicationException("Unable to open record in content [" + primaryContentName + "], with id [" + id + "]");
					}
				}
				else
				{
					if (!cs.Insert(primaryContentName))
					{
						cs.Close();
						id = 0;
						throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
					}
				}
				if (cs.ok())
				{
					id = cs.getInteger("id");
					if (string.IsNullOrEmpty(ccGuid))
					{
						ccGuid = Controllers.genericController.getGUID();
					}
					if (string.IsNullOrEmpty(Name))
					{
						Name = "Visit " + id.ToString();
					}
					cs.setField("Active", Active.ToString());
					cs.SetField("Bot", Bot.ToString());
					cs.SetField("Browser", Browser);
					cs.SetField("ccGuid", ccGuid);
					//
					cs.SetField("ContentControlID", ContentControlID.ToString());
					cs.SetField("CookieSupport", CookieSupport.ToString());
					cs.SetField("CreatedBy", CreatedBy.ToString());
					cs.SetField("CreateKey", CreateKey.ToString());
					cs.SetField("DateAdded", DateAdded.ToString());
					//
					//
					//
					cs.SetField("ExcludeFromAnalytics", ExcludeFromAnalytics.ToString());
					cs.SetField("HTTP_FROM", HTTP_FROM);
					cs.SetField("HTTP_REFERER", HTTP_REFERER);
					cs.SetField("HTTP_VIA", HTTP_VIA);
					cs.SetField("LastVisitTime", LastVisitTime.ToString());
					cs.SetField("LoginAttempts", LoginAttempts.ToString());
					cs.SetField("MemberID", MemberID.ToString());
					cs.SetField("MemberNew", MemberNew.ToString());
					cs.SetField("Mobile", Mobile.ToString());
					cs.SetField("ModifiedBy", ModifiedBy.ToString());
					cs.SetField("ModifiedDate", ModifiedDate.ToString());
					cs.SetField("Name", Name);
					cs.SetField("PageVisits", PageVisits.ToString());
					cs.SetField("RefererPathPage", RefererPathPage);
					cs.SetField("REMOTE_ADDR", REMOTE_ADDR);
					cs.SetField("RemoteName", RemoteName);
					cs.SetField("SortOrder", SortOrder);
					cs.SetField("StartDateValue", StartDateValue.ToString());
					cs.SetField("StartTime", StartTime.ToString());
					cs.SetField("StopTime", StopTime.ToString());
					cs.SetField("TimeToLastHit", TimeToLastHit.ToString());
					cs.SetField("VerboseReporting", VerboseReporting.ToString());
					cs.SetField("VisitAuthenticated", VisitAuthenticated.ToString());
					cs.SetField("VisitorID", VisitorID.ToString());
					cs.SetField("VisitorNew", VisitorNew.ToString());
				}
				cs.Close();
				//
				// -- object is here, but the cache was invalidated, setting
				cpCore.cache.setContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.id.ToString()), this);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return id;
		}
		//
		//====================================================================================================
		/// <summary>
		/// delete an existing database record
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordId"></param>
		public static void delete(coreClass cpCore, int recordId)
		{
			try
			{
				if (recordId > 0)
				{
					cpCore.db.deleteContentRecords(primaryContentName, "id=" + recordId.ToString());
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// delete an existing database record
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordId"></param>
		public static void delete(coreClass cpCore, string guid)
		{
			try
			{
				if (!string.IsNullOrEmpty(guid))
				{
					cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" + cpCore.db.encodeSQLText(guid) + ")");
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
		}
		//
		//====================================================================================================
		/// <summary>
		/// get a list of objects from this model
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="someCriteria"></param>
		/// <returns></returns>
		public static List<visitModel> getObjectList(coreClass cpCore, int someCriteria)
		{
			List<visitModel> result = new List<visitModel>();
			try
			{
				csController cs = new csController(cpCore);
				List<string> ignoreCacheNames = new List<string>();
				if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id"))
				{
					visitModel instance = null;
					do
					{
						instance = visitModel.create(cpCore, cs.getInteger("id"), ignoreCacheNames);
						if (instance != null)
						{
							result.Add(instance);
						}
						cs.goNext();
					} while (cs.ok());
				}
				cs.Close();
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// invalidate the primary key (which depends on all secondary keys)
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="recordId"></param>
		public static void invalidateIdCache(coreClass cpCore, int recordId)
		{
			cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
			//
			// -- always clear the cache with the content name
			//?? '?? cpCore.cache.invalidateObject(primaryContentName)
		}
		//
		//====================================================================================================
		/// <summary>
		/// invalidate a secondary key (ccGuid field).
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="guid"></param>
		public static void invalidateGuidCache(coreClass cpCore, string guid)
		{
			cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", guid));
			//
			// -- always clear the cache with the content name
			//?? cpCore.cache.invalidateObject(primaryContentName)
		}
		//'
		//'====================================================================================================
		//''' <summary>
		//''' produce a standard format cachename for this model
		//''' </summary>
		//''' <param name="fieldName"></param>
		//''' <param name="fieldValue"></param>
		//''' <returns></returns>
		//Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,fieldName As String, fieldValue As String) As String
		//    Return (primaryContentTableName & "." & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
		//End Function
		//
		//====================================================================================================
		/// <summary>
		/// return a visit object for the visitor's last visit before the provided id
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="visitId"></param>
		/// <param name="visitorId"></param>
		/// <returns></returns>
		public static visitModel getLastVisitByVisitor(coreClass cpCore, int visitId, int visitorId)
		{
			object tempVar = new List<string>();
			visitModel result = loadObject(cpCore, "(id<>" + visitId + ")and(VisitorID=" + visitorId + ")", ref tempVar, "id desc");
			return result;
		}
	}
}