//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System;
using System.Collections.Generic;
using System.Text;
using Contensive.BaseClasses;
using System.Reflection;
using Contensive.Core.Controllers;
using Newtonsoft.Json;

namespace Contensive.Core.Models.Entity
{
	//
	//====================================================================================================
	// entity model pattern
	//   factory pattern load because if a record is not found, must rturn nothing
	//   new() - empty constructor to allow deserialization
	//   saveObject() - saves instance properties (nonstatic method)
	//   create() - loads instance properties and returns a model 
	//   delete() - deletes the record that matches the argument
	//   getObjectList() - a pattern for creating model lists.
	//   invalidateFIELDNAMEcache() - method to invalide the model cache. One per cache
	//
	//	1) set the primary content name in const cnPrimaryContent. avoid constants Like cnAddons used outside model
	//	2) find-And-replace "_blankModel" with the name for this model
	//	3) when adding model fields, add in three places: the Public Property, the saveObject(), the loadObject()
	//	4) when adding create() methods to support other fields/combinations of fields, 
	//       - add a secondary cache For that new create method argument in loadObjec()
	//       - add it to the injected cachename list in loadObject()
	//       - add an invalidate
	//
	// Model Caching
	//   caching applies to model objects only, not lists of models (for now)
	//       - this is because of the challenge of invalidating the list object when individual records are added or deleted
	//
	//   a model should have 1 primary cache object which stores the data and can have other secondary cacheObjects which do not hold data
	//    the cacheName of the 'primary' cacheObject for models and db records (cacheNamePrefix + ".id." + #id)
	//    'secondary' cacheName is (cacheNamePrefix + . + fieldName + . + #)
	//
	//   cacheobjects can be used to hold data (primary cacheobjects), or to hold only metadata (secondary cacheobjects)
	//       - primary cacheobjects are like 'personModel.id.99' that holds the model for id=99
	//           - it is primary because the .primaryobject is null
	//           - invalidationData. This cacheobject is invalid after this datetime
	//           - dependentobjectlist() - this object is invalid if any of those objects are invalid
	//       - secondary cachobjects are like 'person.ccguid.12345678'. It does not hold data, just a reference to the primary cacheobject
	//
	//   cacheNames spaces are replaced with underscores, so "addon collections" should be addon_collections
	//
	//   cacheNames that match content names are treated as caches of "any" record in the content, so invalidating "people" can be used to invalidate
	//       any non-specific cache in the people table, by including "people" as a dependant cachename. the "people" cachename should not clear
	//       specific people caches, like people.id.99, but can be used to clear lists of records like "staff_list_group"
	//       - this can be used as a fallback strategy to cache record lists: a remote method list can be cached with a dependancy on "add-ons".
	//       - models should always clear this content name cache entry on all cache clears
	//
	//   when a model is created, the code first attempts to read the model's cacheobject. if it fails, it builds it and saves the cache object and tags
	//       - when building the model, is writes object to the primary cacheobject, and writes all the secondaries to be used
	//       - when building the model, if a database record is opened, a dependantObject Tag is created for the tablename+'id'+id
	//       - when building the model, if another model is added, that model returns its cachenames in the cacheNameList to be added as dependentObjects
	//
	//
	public class oldBaseModel
	{
		//
		//-- const
		public const string primaryContentName = ""; //<------ set content name
		public const string primaryContentTableName = ""; //<------ set to tablename for the primary content (used for cache names)
		public const string primaryContentDataSource = "default"; //<----- set to datasource if not default
		//
		// -- instance properties
		public int id {get; set;}
		public string name {get; set;}
		public string ccguid {get; set;}
		public bool Active {get; set;}
		public int ContentControlID {get; set;}
		public int CreatedBy {get; set;}
		public int CreateKey {get; set;}
		public DateTime DateAdded {get; set;}
		public int ModifiedBy {get; set;}
		public DateTime ModifiedDate {get; set;}
		public string SortOrder {get; set;}
		//
		//Public foreignKey1Id As Integer ' <-- DELETE - sample field for create/delete patterns
		//Public foreignKey2Id As Integer ' <-- DELETE - sample field for create/delete patterns
		//
		// -- publics not exposed to the UI (test/internal data)
		//<JsonIgnore> Public createKey As Integer
		//
		//====================================================================================================
		/// <summary>
		/// Create an empty object. needed for deserialization
		/// </summary>
		public oldBaseModel()
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
		public static oldBaseModel add(coreClass cpCore, ref List<string> callersCacheNameList)
		{
			oldBaseModel result = null;
			try
			{
				result = create(cpCore, cpCore.db.insertContentRecordGetID(primaryContentName, cpCore.doc.authContext.user.id), ref callersCacheNameList);
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
		/// return a new model with the data selected.
		/// </summary>
		/// <param name="cpCore"></param>
		/// <param name="recordId"></param>
		/// <returns></returns>
		public static oldBaseModel create(coreClass cpCore, int recordId)
		{
			object tempVar = new List<string>();
			return create(cpCore, recordId, ref tempVar);
		}
		//
		//====================================================================================================
		/// <summary>
		/// return a new model with the data selected. All cacheNames related to the object will be added to the cacheNameList.
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordId">The id of the record to be read into the new object</param>
		/// <param name="callersCacheNameList">Any cachenames effected by this record will be added to this list. If the method consumer creates a cache object, add these cachenames to its dependent cachename list.</param>
		public static oldBaseModel create(coreClass cpCore, int recordId, ref List<string> callersCacheNameList)
		{
			oldBaseModel result = null;
			try
			{
				if (recordId > 0)
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, recordId);
					result = cpCore.cache.getObject<oldBaseModel>(cacheName);
					if (result == null)
					{
						using (csController cs = new csController(cpCore))
						{
							throw new ApplicationException("This cannot work - to get the derived class's primaryContentName, I need the 'me', but me not available because this is static.");
							//If cs.open(Me.GetType().GetProperties(BindingFlags.Instance Or BindingFlags.Public      instance.primaryContentName, "(id=" & recordId.ToString() & ")") Then
							//    result = loadRecord(cpCore, cs, callersCacheNameList)
							//End If
						}
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
		public static oldBaseModel create(coreClass cpCore, string recordGuid, ref List<string> callersCacheNameList)
		{
			oldBaseModel result = null;
			try
			{
				if (!string.IsNullOrEmpty(recordGuid))
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", recordGuid);
					result = cpCore.cache.getObject<oldBaseModel>(cacheName);
					if (result == null)
					{
						using (csController cs = new csController(cpCore))
						{
							if (cs.open(primaryContentName, "(ccGuid=" + cpCore.db.encodeSQLText(recordGuid) + ")"))
							{
								result = loadRecord(cpCore, cs, ref callersCacheNameList);
							}
						}
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
		//'
		//'====================================================================================================
		//''' <summary>
		//''' template for open an existing object with multiple keys (like a rule)
		//''' </summary>
		//''' <param name="cp"></param>
		//''' <param name="foreignKey1Id"></param>
		//Public Shared Function create(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer, ByRef callersCacheNameList As List(Of String)) As baseModel
		//    Dim result As baseModel = Nothing
		//    Try
		//        If ((foreignKey1Id > 0) And (foreignKey2Id > 0)) Then
		//            result = cpCore.cache.getObject(Of baseModel)(Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", foreignKey1Id.ToString(), "foreignKey2", foreignKey2Id.ToString()))
		//            If (result Is Nothing) Then
		//                Using cs As New csController(cpCore)
		//                    If cs.open(primaryContentName, "(foreignKey1=" & foreignKey1Id.ToString() & ")and(foreignKey1=" & foreignKey1Id.ToString() & ")") Then
		//                        result = loadRecord(cpCore, cs, callersCacheNameList)
		//                    End If
		//                End Using
		//            End If
		//        End If
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex) : Throw
		//        Throw
		//    End Try
		//    Return result
		//End Function
		//
		//====================================================================================================
		/// <summary>
		/// open an existing object
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="recordName"></param>
		public static oldBaseModel createByName(coreClass cpCore, string recordName, ref List<string> callersCacheNameList)
		{
			oldBaseModel result = null;
			try
			{
				if (!string.IsNullOrEmpty(recordName))
				{
					string cacheName = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "name", recordName);
					result = cpCore.cache.getObject<oldBaseModel>(cacheName);
					if (result == null)
					{
						using (csController cs = new csController(cpCore))
						{
							if (cs.open(primaryContentName, "(name=" + cpCore.db.encodeSQLText(recordName) + ")", "id"))
							{
								result = loadRecord(cpCore, cs, ref callersCacheNameList);
							}
						}
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
		private static oldBaseModel loadRecord(coreClass cpCore, csController cs, ref List<string> callersCacheNameList)
		{
			oldBaseModel instance = null;
			try
			{
				if (cs.ok())
				{
					instance = new oldBaseModel();
					foreach (PropertyInfo resultProperty in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
					{
						switch (resultProperty.Name.ToLower())
						{
							case "specialcasefield":
							break;
							default:
								switch (resultProperty.PropertyType.Name)
								{
									case "Int32":
										resultProperty.SetValue(instance, cs.getInteger(resultProperty.Name), null);
										break;
									case "Boolean":
										resultProperty.SetValue(instance, cs.getBoolean(resultProperty.Name), null);
										break;
									case "DateTime":
										resultProperty.SetValue(instance, cs.getDate(resultProperty.Name), null);
										break;
									case "Double":
										resultProperty.SetValue(instance, cs.getNumber(resultProperty.Name), null);
										break;
									default:
										resultProperty.SetValue(instance, cs.getText(resultProperty.Name), null);
										break;
								}
								break;
						}
					}
					if (instance != null)
					{
						//
						// -- set primary cache to the object created
						// -- set secondary caches to the primary cache
						// -- add all cachenames to the injected cachenamelist
						string cacheName0 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "id", instance.id.ToString());
						callersCacheNameList.Add(cacheName0);
						cpCore.cache.setContent(cacheName0, instance);
						//
						string cacheName1 = Controllers.cacheController.getCacheKey_Entity(primaryContentTableName, "ccguid", instance.ccguid);
						callersCacheNameList.Add(cacheName1);
						cpCore.cache.setPointer(cacheName1, cacheName0);
						//
						//Dim cacheName2 As String = Controllers.cacheController.getDbRecordCacheName(primaryContentTableName, "foreignKey1", result.foreignKey1Id.ToString(), "foreignKey2", result.foreignKey2Id.ToString())
						//callersCacheNameList.Add(cacheName2)
						//cpCore.cache.setSecondaryObject(cacheName2, cacheName0)
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw;
			}
			return instance;
		}
		//
		//====================================================================================================
		/// <summary>
		/// save the instance properties to a record with matching id. If id is not provided, a new record is created.
		/// </summary>
		/// <param name="cpCore"></param>
		/// <returns></returns>
		public int save(coreClass cpCore)
		{
			try
			{
				csController cs = new csController(cpCore);
				if (id > 0)
				{
					if (!cs.open(primaryContentName, "id=" + id))
					{
						string message = "Unable to open record in content [" + primaryContentName + "], with id [" + id + "]";
						cs.Close();
						id = 0;
						throw new ApplicationException(message);
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
//INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
				int value = 0;
//INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
				bool value = false;
//INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
				DateTime value = default(DateTime);
//INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
				double value = 0;
				foreach (PropertyInfo resultProperty in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
				{
					switch (resultProperty.Name.ToLower())
					{
						case "id":
							id = cs.getInteger("id");
							break;
						case "ccguid":
							if (string.IsNullOrEmpty(ccguid))
							{
								ccguid = Controllers.genericController.getGUID();
							}
							string value = resultProperty.GetValue(this, null).ToString();
							cs.setField(resultProperty.Name, value);
							break;
						default:
							switch (resultProperty.PropertyType.Name)
							{
								case "Int32":
//									Dim value As Integer
									int.TryParse(resultProperty.GetValue(this, null).ToString(), out value);
									cs.setField(resultProperty.Name, value);
									break;
								case "Boolean":
//									Dim value As Boolean
									bool.TryParse(resultProperty.GetValue(this, null).ToString(), out value);
									cs.setField(resultProperty.Name, value);
									break;
								case "DateTime":
//									Dim value As Date
									DateTime.TryParse(resultProperty.GetValue(this, null).ToString(), out value);
									cs.setField(resultProperty.Name, value);
									break;
								case "Double":
//									Dim value As Double
									double.TryParse(resultProperty.GetValue(this, null).ToString(), out value);
									cs.setField(resultProperty.Name, value);
									break;
								default:
									string value = resultProperty.GetValue(this, null).ToString();
									cs.setField(resultProperty.Name, value);
									break;
							}
							break;
					}
				}
				//
				// -- invalidate objects
				// -- no, the primary is invalidated by the cs.save()
				//cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"id", id.ToString))
				// -- no, the secondary points to the pirmary, which is invalidated. Dont waste resources invalidating
				//cpCore.cache.invalidateObject(controllers.cacheController.getModelCacheName(primaryContentTablename,"ccguid", ccguid))
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
					oldBaseModel instance = create(cpCore, ccguid, ref tempVar);
					if (instance != null)
					{
						invalidatePrimaryCache(cpCore, instance.id);
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
		//'
		//'====================================================================================================
		//''' <summary>
		//''' pattern to delete an existing object based on multiple criteria (like a rule record)
		//''' </summary>
		//''' <param name="cp"></param>
		//''' <param name="foreignKey1Id"></param>
		//''' <param name="foreignKey2Id"></param>
		//Public Shared Sub delete(cpCore As coreClass, foreignKey1Id As Integer, foreignKey2Id As Integer)
		//    Try
		//        If (foreignKey2Id > 0) And (foreignKey1Id > 0) Then
		//            Dim instance As baseModel = create(cpCore, foreignKey1Id, foreignKey2Id, New List(Of String))
		//            If (instance IsNot Nothing) Then
		//                invalidatePrimaryCache(cpCore, instance.id)
		//                cpCore.db.deleteTableRecord(primaryContentTableName, instance.id, primaryContentDataSource)
		//            End If
		//        End If
		//    Catch ex As Exception
		//        cpCore.handleExceptionAndContinue(ex) : Throw
		//        Throw
		//    End Try
		//End Sub
		//
		//====================================================================================================
		/// <summary>
		/// pattern get a list of objects from this model
		/// </summary>
		/// <param name="cp"></param>
		/// <param name="sqlCriteria"></param>
		/// <returns></returns>
		public static List<oldBaseModel> createList(coreClass cpCore, string sqlCriteria, List<string> callersCacheNameList)
		{
			List<oldBaseModel> result = new List<oldBaseModel>();
			try
			{
				csController cs = new csController(cpCore);
				List<string> ignoreCacheNames = new List<string>();
				if (cs.open(primaryContentName, sqlCriteria, "id"))
				{
					oldBaseModel instance = null;
					do
					{
						instance = loadRecord(cpCore, cs, ref callersCacheNameList);
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
			return oldBaseModel.create(cpcore, recordId, ref tempVar).name;
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
			return oldBaseModel.create(cpcore, ccGuid, ref tempVar).name;
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
			return oldBaseModel.create(cpcore, ccGuid, ref tempVar).id;
		}
		//
		//====================================================================================================
		//
		public static oldBaseModel createDefault(coreClass cpcore)
		{
			oldBaseModel instance = new oldBaseModel();
			try
			{
				Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpcore, primaryContentName);
				if (CDef == null)
				{
					throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
				}
				else if (CDef.Id <= 0)
				{
					throw new ApplicationException("content [" + primaryContentName + "] could Not be found.");
				}
				else
				{
					foreach (PropertyInfo resultProperty in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
					{
						switch (resultProperty.Name.ToLower())
						{
							case "id":
								instance.id = 0;
								break;
							default:
								switch (resultProperty.PropertyType.Name)
								{
									case "Int32":
										resultProperty.SetValue(instance, genericController.EncodeInteger(CDef.fields(resultProperty.Name).defaultValue), null);
										break;
									case "Boolean":
										resultProperty.SetValue(instance, genericController.EncodeBoolean(CDef.fields(resultProperty.Name).defaultValue), null);
										break;
									case "DateTime":
										resultProperty.SetValue(instance, genericController.EncodeDate(CDef.fields(resultProperty.Name).defaultValue), null);
										break;
									case "Double":
										resultProperty.SetValue(instance, genericController.EncodeNumber(CDef.fields(resultProperty.Name).defaultValue), null);
										break;
									default:
										resultProperty.SetValue(instance, CDef.fields(resultProperty.Name).defaultValue, null);
										break;
								}
								break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return instance;
		}
	}
}
