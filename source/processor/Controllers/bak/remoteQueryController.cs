

using System.Text.RegularExpressions;
// 

namespace Controllers {
    
    // 
    // ====================================================================================================
    // '' <summary>
    // '' static class controller
    // '' </summary>
    public class remoteQueryController : IDisposable {
        
        // 
        // 
        // 
        // 
        public static string main_GetRemoteQueryKey(coreClass cpCore, string SQL, string DataSourceName, void =, void , int maxRows, void =, void 1000) {
            // 
            // Warning!!! Optional parameters not supported
            // Warning!!! Optional parameters not supported
            int CS;
            string RemoteKey = "";
            int DataSourceID;
            // 
            if ((maxRows == 0)) {
                maxRows = 1000;
            }
            
            CS = cpCore.db.csInsertRecord("Remote Queries");
            if (cpCore.db.csOk(CS)) {
                RemoteKey = Guid.NewGuid.ToString();
                DataSourceID = cpCore.db.getRecordID("Data Sources", DataSourceName);
                cpCore.db.csSet(CS, "remotekey", RemoteKey);
                cpCore.db.csSet(CS, "datasourceid", DataSourceID);
                cpCore.db.csSet(CS, "sqlquery", SQL);
                cpCore.db.csSet(CS, "maxRows", maxRows);
                cpCore.db.csSet(CS, "dateexpires", cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.AddDays(1)));
                cpCore.db.csSet(CS, "QueryTypeID", QueryTypeSQL);
                cpCore.db.csSet(CS, "VisitId", cpCore.doc.authContext.visit.id);
            }
            
            cpCore.db.csClose(CS);
            // 
            return RemoteKey;
        }
        
        // 
        // 
        // 
        public static string main_FormatRemoteQueryOutput(coreClass cpCore, GoogleDataType gd, RemoteFormatEnum RemoteFormat) {
            // 
            stringBuilderLegacyController s;
            string ColDelim;
            string RowDelim;
            int ColPtr;
            int RowPtr;
            // 
            //  Select output format
            // 
            s = new stringBuilderLegacyController();
            switch (RemoteFormat) {
                case RemoteFormatEnum.RemoteFormatJsonNameValue:
                    // 
                    // 
                    // 
                    s.Add("{");
                    if (!gd.IsEmpty) {
                        ColDelim = "";
                        for (ColPtr = 0; (ColPtr <= UBound(gd.col)); ColPtr++) {
                            s.Add((ColDelim 
                                            + (gd.col(ColPtr).Id + (":\'" 
                                            + (gd.row(0).Cell(ColPtr).v + "\'")))));
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
                        for (ColPtr = 0; (ColPtr <= UBound(gd.col)); ColPtr++) {
                            s.Add((ColDelim 
                                            + (gd.col(ColPtr).Id + ":[")));
                            ColDelim = ",";
                            RowDelim = "";
                            for (RowPtr = 0; (RowPtr <= UBound(gd.row)); RowPtr++) {
                                // With...
                                s.Add((RowDelim + ("\'" 
                                                + (gd.row(RowPtr).Cell(ColPtr).v + "\'"))));
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
                        for (ColPtr = 0; (ColPtr <= UBound(gd.col)); ColPtr++) {
                            // With...
                            s.Add((ColDelim + ("{id: \'" 
                                            + (genericController.EncodeJavascript(gd.col(ColPtr).Id) + ("\', label: \'" 
                                            + (genericController.EncodeJavascript(gd.col(ColPtr).Label) + ("\', type: \'" 
                                            + (genericController.EncodeJavascript(gd.col(ColPtr).Type) + "\'}"))))))));
                            ColDelim = ",";
                        }
                        
                        s.Add("],rows:[");
                        RowDelim = "";
                        for (RowPtr = 0; (RowPtr <= UBound(gd.row)); RowPtr++) {
                            s.Add((RowDelim + "{c:["));
                            RowDelim = ",";
                            ColDelim = "";
                            for (ColPtr = 0; (ColPtr <= UBound(gd.col)); ColPtr++) {
                                // With...
                                s.Add((ColDelim + ("{v: \'" 
                                                + (genericController.EncodeJavascript(gd.row(RowPtr).Cell(ColPtr).v) + "\'}"))));
                                ColDelim = ",";
                            }
                            
                            s.Add("]}");
                        }
                        
                        s.Add("]");
                    }
                    
                    s.Add("}");
                    break;
            }
            main_FormatRemoteQueryOutput = s.Text;
            // 
        }
        
        // 
        //  this class must implement System.IDisposable
        //  never throw an exception in dispose
        //  Do not change or add Overridable to these methods.
        //  Put cleanup code in Dispose(ByVal disposing As Boolean).
        // ====================================================================================================
        // 
        protected bool disposed = false;
        
        public void Dispose() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        // 
        protected override void Finalize() {
            //  do not add code here. Use the Dispose(disposing) overload
            this.Dispose(false);
            base.Finalize();
        }
        
        // 
        // ====================================================================================================
        // '' <summary>
        // '' dispose.
        // '' </summary>
        // '' <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (!this.disposed) {
                this.disposed = true;
                if (disposing) {
                    // If (cacheClient IsNot Nothing) Then
                    //     cacheClient.Dispose()
                    // End If
                }
                
                // 
                //  cleanup non-managed objects
                // 
            }
            
        }
    }
}