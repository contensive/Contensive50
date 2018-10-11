
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Models.Domain {
    /// <summary>
    /// Table Schema caching to speed up update
    /// </summary>
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
        public string tableName { get; set; }
        public bool dirty { get; set; }
        public List<ColumnSchemaModel> columns { get; set; }
        // list of all indexes, with the field it covers
        public List<IndexSchemaModel> indexes { get; set; }
        //
        //=================================================================================
        //
        public static TableSchemaModel getTableSchema(CoreController core, string TableName, string DataSourceName) {
            TableSchemaModel tableSchema = null;
            try {
                if ((!string.IsNullOrEmpty(DataSourceName)) & (DataSourceName != "-1") & (DataSourceName.ToLower() != "default")) {
                    throw new NotImplementedException("alternate datasources not supported yet");
                } else {
                    if (!string.IsNullOrEmpty(TableName)) {
                        string lowerTablename = TableName.ToLower();
                        bool isInCache = false;
                        if ((core.doc.tableSchemaDictionary) == null) {
                            core.doc.tableSchemaDictionary = new Dictionary<string, Models.Domain.TableSchemaModel>();
                        } else {
                            isInCache = core.doc.tableSchemaDictionary.TryGetValue(lowerTablename, out tableSchema);
                        }
                        bool buildCache = !isInCache;
                        if (isInCache) {
                            buildCache = tableSchema.dirty;
                        }
                        if (buildCache) {
                            //
                            // cache needs to be built
                            //
                            DataTable dt = core.db.getTableSchemaData(TableName);
                            bool isInDb = false;
                            if (dt.Rows.Count <= 0) {
                                tableSchema = null;
                            } else {
                                isInDb = true;
                                tableSchema = new Models.Domain.TableSchemaModel();
                                tableSchema.columns = new List<ColumnSchemaModel>();
                                tableSchema.indexes = new List<IndexSchemaModel>();
                                tableSchema.tableName = lowerTablename;
                                //
                                // load columns
                                //
                                dt = core.db.getColumnSchemaData(TableName);
                                if (dt.Rows.Count > 0) {
                                    foreach (DataRow row in dt.Rows) {
                                        tableSchema.columns.Add(new ColumnSchemaModel() {
                                            COLUMN_NAME = GenericController.encodeText(row["COLUMN_NAME"]).ToLower(),
                                            DATA_TYPE = GenericController.encodeText(row["DATA_TYPE"]).ToLower(),
                                            CHARACTER_MAXIMUM_LENGTH = GenericController.encodeInteger(row["CHARACTER_MAXIMUM_LENGTH"]),
                                            DATETIME_PRECISION = GenericController.encodeInteger(row["DATETIME_PRECISION"])
                                        });
                                    }
                                }
                                //
                                // Load the index schema
                                //
                                dt = core.db.getIndexSchemaData(TableName);
                                if (dt.Rows.Count > 0) {
                                    foreach (DataRow row in dt.Rows) {
                                        string index_keys = GenericController.encodeText(row["index_keys"]).ToLower();
                                        tableSchema.indexes.Add(new IndexSchemaModel() {
                                            index_name = GenericController.encodeText(row["INDEX_NAME"]).ToLower(),
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
                LogController.handleError( core,ex);
                throw;
            }
            return tableSchema;
        }
        //
        //====================================================================================================
        public static void tableSchemaListClear(CoreController core) {
            core.doc.tableSchemaDictionary.Clear();
        }
    }

}
