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
	public class dataSourceModel
	{
		//
		//-- const
		public const string primaryContentName = "data sources";
		private const string primaryContentTableName = "ccDataSources";
		private const string primaryContentDataSource = "default";
		//
		public enum dataSourceTypeEnum
		{
			sqlServerOdbc = 1,
			sqlServerNative = 2,
			mySqlNative = 3
		}
		//
		// -- instance properties
		public int ID;
		public bool Active;
		public string ccGuid;
		public string ConnString;
		public string endPoint;
		public string username;
		public string password;
		public int type;

		public int ContentControlID;
		public int CreatedBy;
		public int CreateKey;
		public DateTime DateAdded;



		public int ModifiedBy;
		public DateTime ModifiedDate;
		public string Name;
		public string SortOrder;
		//
		//====================================================================================================
		/// <summary>
		/// Create an empty object. needed for deserialization
		/// </summary>
		public dataSourceModel()
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
		/// <param name="callersCacheNameList"></param>
		/// <returns></returns>
		public static dataSourceModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, cpCore.doc.authContext.user.id), callersCacheNameList);
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
		/// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
		public static dataSourceModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				if (recordId <= 0)
				{
					result = getDefaultDatasource(cpCore);
				}
				else
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
					result = cpCore.cache.getObject<dataSourceModel>(cacheName);
					if (result == null)
					{
						result = loadObject(cpCore, "id=" + recordId.ToString(), ref callersCacheNameList);
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
		public static dataSourceModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				if (string.IsNullOrEmpty(recordGuid))
				{
					result = getDefaultDatasource(cpCore);
				}
				else
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
					result = cpCore.cache.getObject<dataSourceModel>(cacheName);
					if (result == null)
					{
						result = loadObject(cpCore, "ccGuid=" + cpCore.db.encodeSQLText(recordGuid), ref callersCacheNameList);
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
		/// template for open an existing object with multiple keys (like a rule)
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="foreignKey1Id"></param>
		public static dataSourceModel create(coreClass cpCore, int foreignKey1Id, int foreignKey2Id, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				if ((foreignKey1Id > 0) && (foreignKey2Id > 0))
				{
					result = cpCore.cache.getObject<dataSourceModel>(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "foreignKey1", foreignKey1Id, "foreignKey2", foreignKey2Id));
					if (result == null)
					{
						result = loadObject(cpCore, "(foreignKey1=" + foreignKey1Id.ToString() + ")and(foreignKey1=" + foreignKey1Id.ToString() + ")", ref callersCacheNameList);
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
		private static dataSourceModel loadObject(coreClass cpCore, string sqlCriteria, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				csController cs = new csController(cpCore);
				if (cs.open(primaryContentName, sqlCriteria))
				{
					result = new dataSourceModel();
					//
					// -- populate result model
					result.ID = cs.getInteger("ID");
					result.Active = cs.getBoolean("Active");
					result.ccGuid = cs.getText("ccGuid");
					result.ConnString = cs.getText("ConnString");
					result.endPoint = cs.getText("endpoint");
					result.username = cs.getText("username");
					result.password = cs.getText("password");
					result.type = cs.getInteger("dbTypeId");
					//'
					result.ContentControlID = cs.getInteger("ContentControlID");
					result.CreatedBy = cs.getInteger("CreatedBy");
					result.CreateKey = cs.getInteger("CreateKey");
					result.DateAdded = cs.getDate("DateAdded");
					//'
					//'
					//'
					result.ModifiedBy = cs.getInteger("ModifiedBy");
					result.ModifiedDate = cs.getDate("ModifiedDate");
					result.Name = normalizeDataSourceName(cs.getText("Name"));
					result.SortOrder = cs.getText("SortOrder");
					if (result != null)
					{
						//
						// -- set primary cache to the object created
						// -- set secondary caches to the primary cache
						// -- add all cachenames to the injected cachenamelist
						string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", result.ID.ToString());
						callersCacheNameList.Add(cacheName0);
						cpCore.cache.setContent(cacheName0, result);
						//
						string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", result.ccGuid);
						callersCacheNameList.Add(cacheName1);
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
				if (ID > 0)
				{
					if (!cs.open(primaryContentName, "id=" + ID))
					{
						ID = 0;
						cs.Close();
						throw new ApplicationException("Unable to open record in content [" + primaryContentName + "], with id [" + ID + "]");
					}
				}
				else
				{
					if (!cs.Insert(primaryContentName))
					{
						cs.Close();
						ID = 0;
						throw new ApplicationException("Unable to insert record in content [" + primaryContentName + "]");
					}
				}
				if (cs.ok())
				{
					ID = cs.getInteger("id");
					if (string.IsNullOrEmpty(ccGuid))
					{
						ccGuid = Controllers.genericController.getGUID();
					}
					cs.setField("Active", Active.ToString());
					cs.setField("ccGuid", ccGuid);
					cs.setField("ConnString", ConnString);
					cs.setField("endPoint", endPoint);
					cs.setField("username", username);
					cs.setField("password", password);
					cs.setField("dbTypeId", type);
					//'
					cs.setField("ContentControlID", ContentControlID.ToString());
					cs.setField("CreatedBy", CreatedBy.ToString());
					cs.setField("CreateKey", CreateKey.ToString());
					cs.setField("DateAdded", DateAdded.ToString());
					//'
					//'
					//'
					cs.setField("ModifiedBy", ModifiedBy.ToString());
					cs.setField("ModifiedDate", ModifiedDate.ToString());
					cs.setField("Name", normalizeDataSourceName(Name));
					cs.setField("SortOrder", SortOrder);
				}
				cs.Close();
				//
				// -- invalidate objects
				// -- no, the primary is invalidated by the cs.save()
				//cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
				// -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
				//cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
				//
				// -- object is here, but the cache was invalidated, setting
				cpCore.cache.setContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", this.ID.ToString()), this);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return ID;
		}
		//
		//====================================================================================================
		/// <summary>
		/// delete an existing database record by id
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
					cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
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
		/// delete an existing database record by guid
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="ccguid"></param>
		public static void delete(coreClass cpCore, string ccguid)
		{
			try
			{
				if (!string.IsNullOrEmpty(ccguid))
				{
					object tempVar = new List<string>();
					dataSourceModel instance = create(cpCore, ccguid, ref tempVar);
					if (instance != null)
					{
						invalidatePrimaryCache(cpCore, instance.ID);
						cpCore.db.deleteContentRecords(primaryContentName, "(ccguid=" + cpCore.db.encodeSQLText(ccguid) + ")");
					}
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
		/// pattern to delete an existing object based on multiple criteria (like a rule record)
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="foreignKey1Id"></param>
		/// <param name="foreignKey2Id"></param>
		public static void delete(coreClass cpCore, int foreignKey1Id, int foreignKey2Id)
		{
			try
			{
				if ((foreignKey2Id > 0) && (foreignKey1Id > 0))
				{
					object tempVar = new List<string>();
					dataSourceModel instance = create(cpCore, foreignKey1Id, foreignKey2Id, ref tempVar);
					if (instance != null)
					{
						invalidatePrimaryCache(cpCore, instance.ID);
						cpCore.db.deleteTableRecord(primaryContentTableName, instance.ID, primaryContentDataSource);
					}
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
		/// pattern get a list of objects from this model
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="someCriteria"></param>
		/// <returns></returns>
		public static List<dataSourceModel> getObjectList(coreClass cpCore, int someCriteria, List<string> callersCacheNameList)
		{
			List<dataSourceModel> result = new List<dataSourceModel>();
			try
			{
				csController cs = new csController(cpCore);
				List<string> ignoreCacheNames = new List<string>();
				if (cs.open(primaryContentName, "(someCriteria=" + someCriteria + ")", "name", true, "id"))
				{
					dataSourceModel instance = null;
					do
					{
						instance = dataSourceModel.create(cpCore, cs.getInteger("id"), callersCacheNameList);
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
		public static void invalidatePrimaryCache(coreClass cpCore, int recordId)
		{
			cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId));
			//
			// -- the zero record cache means any record was updated. Can be used to invalidate arbitraty lists of records in the table
			cpCore.cache.invalidateContent(Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", "0"));
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
		//    Return (primaryContentTableName & "-" & fieldName & "." & fieldValue).ToLower().Replace(" ", "_")
		//End Function
		//'
		//Private Shared Function Controllers.cacheController.getModelCacheName( primaryContentTableName,field1Name As String, field1Value As String, field2Name As String, field2Value As String) As String
		//    Return (primaryContentTableName & "-" & field1Name & "." & field1Value & "-" & field2Name & "." & field2Value).ToLower().Replace(" ", "_")
		//End Function
		//
		//====================================================================================================
		/// <summary>
		/// get the name of the record by it's id
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordId"></param>record
		/// <returns></returns>
		public static string getRecordName(coreClass cpcore, int recordId)
		{
			object tempVar = new List<string>();
			return normalizeDataSourceName(dataSourceModel.create(cpcore, recordId, ref tempVar).Name);
		}
		//
		//====================================================================================================
		/// <summary>
		/// get the name of the record by it's guid 
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="ccGuid"></param>record
		/// <returns></returns>
		public static string getRecordName(coreClass cpcore, string ccGuid)
		{
			object tempVar = new List<string>();
			return normalizeDataSourceName(dataSourceModel.create(cpcore, ccGuid, ref tempVar).Name);
		}
		//
		//====================================================================================================
		/// <summary>
		/// get the id of the record by it's guid 
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="ccGuid"></param>record
		/// <returns></returns>
		public static int getRecordId(coreClass cpcore, string ccGuid)
		{
			object tempVar = new List<string>();
			return dataSourceModel.create(cpcore, ccGuid, ref tempVar).ID;
		}
		//
		//====================================================================================================
		/// <summary>
		/// convert a datasource name into the key value used by the datasourcedictionary cache
		/// </summary>
		/// <param name="DataSourceName"></param>
		/// <returns></returns>
		public static string normalizeDataSourceName(string DataSourceName)
		{
			if (!string.IsNullOrEmpty(DataSourceName))
			{
				return DataSourceName.Trim().ToLower();
			}
			return string.Empty;
		}
		//
		//====================================================================================================
		public static Dictionary<string, dataSourceModel> getNameDict(coreClass cpcore)
		{
			Dictionary<string, dataSourceModel> result = new Dictionary<string, dataSourceModel>();
			try
			{
				csController cs = new csController(cpcore);
				List<string> ignoreCacheNames = new List<string>();
				if (cs.open(primaryContentName, "", "id", true, "id"))
				{
					do
					{
						dataSourceModel instance = create(cpcore, cs.getInteger("id"), new List<string>());
						if (instance != null)
						{
							result.Add(instance.Name.ToLower(), instance);
						}
					} while (true);
				}
				if (!result.ContainsKey("default"))
				{
					result.Add("default", getDefaultDatasource(cpcore));
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
			return result;
		}
		//
		//====================================================================================================
		/// <summary>
		/// open an existing object from its name
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordGuid"></param>
		public static dataSourceModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			dataSourceModel result = null;
			try
			{
				if (string.IsNullOrEmpty(recordName.Trim()) || (recordName.Trim().ToLower() == "default"))
				{
					result = getDefaultDatasource(cpCore);
				}
				else
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordName);
					result = cpCore.cache.getObject<dataSourceModel>(cacheName);
					if (result == null)
					{
						result = loadObject(cpCore, "name=" + cpCore.db.encodeSQLText(recordName), ref callersCacheNameList);
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
		/// return the default datasource
		/// </summary>
		/// <param name="cp"></param>
		public static dataSourceModel getDefaultDatasource(coreClass cpCore)
		{
			dataSourceModel result = null;
			try
			{
				result = new dataSourceModel();
				result.Active = true;
				result.ccGuid = "";
				result.ConnString = "";
				result.ContentControlID = 0;
				result.CreatedBy = 0;
				result.CreateKey = 0;
				result.DateAdded = DateTime.MinValue;
				result.type = (int)dataSourceTypeEnum.sqlServerNative;
				result.endPoint = cpCore.serverConfig.defaultDataSourceAddress;
				result.Name = "default";
				result.password = cpCore.serverConfig.password;
				result.username = cpCore.serverConfig.username;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return result;
		}
		//'
		//'====================================================================================================
		//Public Shared Function getHtmlInputSelect(cpCore As coreClass, HtmlName As String, selectedId As Integer, callerCacheList As List(Of String)) As String
		//    Dim result As String = ""
		//    Try
		//        result = cpCore.htmlDoc.main_GetFormInputSelect2(HtmlName, selectedId, primaryContentName, "", "default", "", Nothing)
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex)
		//    End Try
		//    Return result
		//End Function
	}
}
