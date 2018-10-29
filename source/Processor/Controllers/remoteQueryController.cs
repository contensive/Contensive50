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
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.Constants;
//
namespace Contensive.Processor.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// static class controller
    /// </summary>
    public class RemoteQueryController : IDisposable {
        //
        //
        //
        //
        public static string main_GetRemoteQueryKey(CoreController core, string SQL, string dataSourceName = "default", int maxRows = 1000) {
            //
            int CS = 0;
            string RemoteKey = "";
            int DataSourceID = 0;
            //
            if (maxRows == 0) {
                maxRows = 1000;
            }
            CS = core.db.csInsertRecord("Remote Queries");
            if (core.db.csOk(CS)) {
                RemoteKey = GenericController.getGUIDString();
                DataSourceID = core.db.getRecordID("Data Sources", dataSourceName);
                core.db.csSet(CS, "remotekey", RemoteKey);
                core.db.csSet(CS, "datasourceid", DataSourceID);
                core.db.csSet(CS, "sqlquery", SQL);
                core.db.csSet(CS, "maxRows", maxRows);
                core.db.csSet(CS, "dateexpires", core.db.encodeSQLDate(core.doc.profileStartTime.AddDays(1)));
                core.db.csSet(CS, "QueryTypeID", QueryTypeSQL);
                core.db.csSet(CS, "VisitId", core.session.visit.id);
            }
            core.db.csClose(ref CS);
            //
            return RemoteKey;
        }
        //
        //
        //
        public static string main_FormatRemoteQueryOutput(CoreController core, GoogleDataType gd, RemoteFormatEnum RemoteFormat) {
            //
            StringBuilderLegacyController s = null;
            string ColDelim = null;
            string RowDelim = null;
            int ColPtr = 0;
            int RowPtr = 0;
            //
            // Select output format
            //
            s = new StringBuilderLegacyController();
            switch (RemoteFormat) {
                case RemoteFormatEnum.RemoteFormatJsonNameValue:
                    //
                    //
                    //
                    s.Add("{");
                    if (!gd.IsEmpty) {
                        ColDelim = "";
                        for (ColPtr = 0; ColPtr <= gd.col.Count; ColPtr++) {
                            s.Add(ColDelim + gd.col[ColPtr].Id + ":'" + gd.row[0].Cell[ColPtr].v + "'");
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
                    if (!gd.IsEmpty) {
                        ColDelim = "";
                        for (ColPtr = 0; ColPtr <= gd.col.Count; ColPtr++) {
                            s.Add(ColDelim + gd.col[ColPtr].Id + ":[");
                            ColDelim = ",";
                            RowDelim = "";
                            for (RowPtr = 0; RowPtr <= gd.row.Count ; RowPtr++) {
                                var tempVar = gd.row[RowPtr].Cell[ColPtr];
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
                    if (!gd.IsEmpty) {
                        s.Add("cols: [");
                        ColDelim = "";
                        for (ColPtr = 0; ColPtr <= gd.col.Count; ColPtr++) {
                            var tempVar2 = gd.col[ColPtr];
                            s.Add(ColDelim + "{id: '" + GenericController.EncodeJavascriptStringSingleQuote(tempVar2.Id) + "', label: '" + GenericController.EncodeJavascriptStringSingleQuote(tempVar2.Label) + "', type: '" + GenericController.EncodeJavascriptStringSingleQuote(tempVar2.Type) + "'}");
                            ColDelim = ",";
                        }
                        s.Add("],rows:[");
                        RowDelim = "";
                        for (RowPtr = 0; RowPtr <= gd.row.Count; RowPtr++) {
                            s.Add(RowDelim + "{c:[");
                            RowDelim = ",";
                            ColDelim = "";
                            for (ColPtr = 0; ColPtr <= gd.col.Count; ColPtr++) {
                                var tempVar3 = gd.row[RowPtr].Cell[ColPtr];
                                s.Add(ColDelim + "{v: '" + GenericController.EncodeJavascriptStringSingleQuote(tempVar3.v) + "'}");
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
        public void Dispose() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        //
        ~RemoteQueryController() {
            // do not add code here. Use the Dispose(disposing) overload
            Dispose(false);
            
            
        }
        //
        //====================================================================================================
        /// <summary>
        /// dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
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