﻿
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Complex {
    //
    // ----- Table Schema caching to speed up update
    //
    public class TableSchemaModel {
        public class ColumnSchemaModel {
            public string COLUMN_NAME;
            public string DATA_TYPE;
            public int DATETIME_PRECISION;
            public int CHARACTER_MAXIMUM_LENGTH;
        }
        public class IndexSchemaModel {
            public string index_name;
            public string index_keys;
            public List<string> indexKeyList;
        }
        public string TableName { get; set; }
        public bool Dirty { get; set; }
        public List<ColumnSchemaModel> columns { get; set; }
        // list of all indexes, with the field it covers
        public List<IndexSchemaModel> indexes { get; set; }
        //
        //=================================================================================
        // Returns a pointer into the cdefCache.tableSchema() array for the table that matches
        //   returns -1 if the table is not found
        //=================================================================================
        //
        public static Models.Complex.TableSchemaModel getTableSchema(coreController core, string TableName, string DataSourceName) {
            Models.Complex.TableSchemaModel tableSchema = null;
            try {
                DataTable dt = null;
                bool isInCache = false;
                bool isInDb = false;
                //Dim readFromDb As Boolean
                string lowerTablename = null;
                bool buildCache = false;
                //
                if ((!string.IsNullOrEmpty(DataSourceName)) & (DataSourceName != "-1") & (DataSourceName.ToLower() != "default")) {
                    throw new NotImplementedException("alternate datasources not supported yet");
                } else {
                    if (!string.IsNullOrEmpty(TableName)) {
                        lowerTablename = TableName.ToLower();
                        if ((core.doc.tableSchemaDictionary) == null) {
                            core.doc.tableSchemaDictionary = new Dictionary<string, Models.Complex.TableSchemaModel>();
                        } else {
                            isInCache = core.doc.tableSchemaDictionary.TryGetValue(lowerTablename, out tableSchema);
                        }
                        buildCache = !isInCache;
                        if (isInCache) {
                            buildCache = tableSchema.Dirty;
                        }
                        if (buildCache) {
                            //
                            // cache needs to be built
                            //
                            dt = core.db.getTableSchemaData(TableName);
                            if (dt.Rows.Count <= 0) {
                                tableSchema = null;
                            } else {
                                isInDb = true;
                                tableSchema = new Models.Complex.TableSchemaModel();
                                tableSchema.columns = new List<ColumnSchemaModel>();
                                tableSchema.indexes = new List<IndexSchemaModel>();
                                tableSchema.TableName = lowerTablename;
                                //
                                // load columns
                                //
                                dt = core.db.getColumnSchemaData(TableName);
                                if (dt.Rows.Count > 0) {
                                    foreach (DataRow row in dt.Rows) {
                                        tableSchema.columns.Add( new ColumnSchemaModel() {
                                            COLUMN_NAME = genericController.encodeText(row["COLUMN_NAME"]).ToLower(),
                                            DATA_TYPE = genericController.encodeText(row["DATA_TYPE"]).ToLower(),
                                            CHARACTER_MAXIMUM_LENGTH = genericController.encodeInteger( row["CHARACTER_MAXIMUM_LENGTH"] ),
                                            DATETIME_PRECISION= genericController.encodeInteger(row["DATETIME_PRECISION"])
                                        });
                                    }
                                }
                                //
                                // Load the index schema
                                //
                                dt = core.db.getIndexSchemaData(TableName);
                                if (dt.Rows.Count > 0) {
                                    foreach (DataRow row in dt.Rows) {
                                        string index_keys = genericController.encodeText(row["index_keys"]).ToLower();
                                        tableSchema.indexes.Add(new IndexSchemaModel() {
                                             index_name = genericController.encodeText(row["INDEX_NAME"]).ToLower(),
                                             index_keys = index_keys,
                                             indexKeyList = index_keys.Split(',').Select(s => s.Trim()).ToList()
                                        });
                                    }
                                }
                            }
                            if (!isInDb && isInCache) {
                                core.doc.tableSchemaDictionary.Remove(lowerTablename);
                            } else if (isInDb && (!isInCache)) {
                                core.doc.tableSchemaDictionary.Add(lowerTablename, tableSchema);
                            } else if (isInDb && isInCache) {
                                core.doc.tableSchemaDictionary[lowerTablename] = tableSchema;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                logController.handleError( core,ex);
                throw;
            }
            return tableSchema;
        }
        //
        //====================================================================================================
        public static void tableSchemaListClear(coreController core) {
            core.doc.tableSchemaDictionary.Clear();
        }
    }

}
