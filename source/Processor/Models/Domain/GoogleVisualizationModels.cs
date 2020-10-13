
using System.Collections.Generic;
//
namespace Contensive.Processor.Models.Domain {
    //
    // Google Data Object construction in GetRemoteQuery
    //
    //
    public class ColsType {
        public string Type;
        public string Id;
        public string Label;
        public string Pattern;
    }
    //
    //
    public class CellType {
        public string v;
        public string f;
        public string p;
    }
    //
    //
    public class RowsType {
        public List<CellType> Cell;
    }
    //
    //
    public class GoogleDataType {
        public bool IsEmpty;
        public List<ColsType> col;
        public List<RowsType> row;
    }
    //
    //
    public enum GoogleVisualizationStatusEnum {
        OK = 1,
        warning = 2,
        ErrorStatus = 3
    }
    //
    //
    public class GoogleVisualizationType {
        public string version;
        public string reqid;
        public GoogleVisualizationStatusEnum status;
        public string[] warnings;
        public string[] errors;
        public string sig;
        public GoogleDataType table;
    }

}