﻿using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Models;
using Models.Context;
using Models.Entity;
using Contensive.Core.Controllers;

namespace Contensive.Core.Models.Complex
{

	//
	//
	// ----- Table Schema caching to speed up update
	//
	public class tableSchemaModel
	{
		public string TableName {get; set;}
		public bool Dirty {get; set;}
		public List<string> columns {get; set;}
		// list of all indexes, with the field it covers
		public List<string> indexes {get; set;}
		//
		//=================================================================================
		// Returns a pointer into the cdefCache.tableSchema() array for the table that matches
		//   returns -1 if the table is not found
		//=================================================================================
		//
		public static Models.Complex.tableSchemaModel getTableSchema(coreClass cpcore, string TableName, string DataSourceName)
		{
			Models.Complex.tableSchemaModel tableSchema = null;
			try
			{
				DataTable dt = null;
				bool isInCache = false;
				bool isInDb = false;
				//Dim readFromDb As Boolean
				string lowerTablename = null;
				bool buildCache = false;
				//
				if ((!string.IsNullOrEmpty(DataSourceName)) & (DataSourceName != "-1") & (DataSourceName.ToLower() != "default"))
				{
					throw new NotImplementedException("alternate datasources not supported yet");
				}
				else
				{
					if (!string.IsNullOrEmpty(TableName))
					{
						lowerTablename = TableName.ToLower();
						if ((cpcore.doc.tableSchemaDictionary) == null)
						{
							cpcore.doc.tableSchemaDictionary = new Dictionary<string, Models.Complex.tableSchemaModel>();
						}
						else
						{
							isInCache = cpcore.doc.tableSchemaDictionary.TryGetValue(lowerTablename, tableSchema);
						}
						buildCache = !isInCache;
						if (isInCache)
						{
							buildCache = tableSchema.Dirty;
						}
						if (buildCache)
						{
							//
							// cache needs to be built
							//
							dt = cpcore.db.getTableSchemaData(TableName);
							if (dt.Rows.Count <= 0)
							{
								tableSchema = null;
							}
							else
							{
								isInDb = true;
								tableSchema = new Models.Complex.tableSchemaModel();
								tableSchema.columns = new List<string>();
								tableSchema.indexes = new List<string>();
								tableSchema.TableName = lowerTablename;
								//
								// load columns
								//
								dt = cpcore.db.getColumnSchemaData(TableName);
								if (dt.Rows.Count > 0)
								{
									foreach (DataRow row in dt.Rows)
									{
										tableSchema.columns.Add(genericController.encodeText(row("COLUMN_NAME")).ToLower());
									}
								}
								//
								// Load the index schema
								//
								dt = cpcore.db.getIndexSchemaData(TableName);
								if (dt.Rows.Count > 0)
								{
									foreach (DataRow row in dt.Rows)
									{
										tableSchema.indexes.Add(genericController.encodeText(row("INDEX_NAME")).ToLower());
									}
								}
							}
							if (!isInDb && isInCache)
							{
								cpcore.doc.tableSchemaDictionary.Remove(lowerTablename);
							}
							else if (isInDb && (!isInCache))
							{
								cpcore.doc.tableSchemaDictionary.Add(lowerTablename, tableSchema);
							}
							else if (isInDb && isInCache)
							{
								cpcore.doc.tableSchemaDictionary(lowerTablename) = tableSchema;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
			return tableSchema;
		}
		//
		//====================================================================================================
		public static void tableSchemaListClear(coreClass cpcore)
		{
			cpcore.doc.tableSchemaDictionary.Clear();
		}
	}

}