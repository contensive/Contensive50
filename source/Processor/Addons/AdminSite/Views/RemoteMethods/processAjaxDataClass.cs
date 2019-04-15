
using System;
using Contensive.Processor;
using Contensive.Processor.Controllers;
using Contensive.Processor.Exceptions;

namespace Contensive.Addons.AdminSite {
    //
    public class ProcessAjaxDataClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// getFieldEditorPreference remote method
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cp) {
            string result = "";
            try {
                CoreController core = ((CPClass)cp).core;

                result = processAjaxData(core);

            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
        //
        //=================================================================================================
        /// <summary>
        /// Run and return results from a remotequery call from cj.ajax.data(handler,key,args,pagesize,pagenumber)
        /// This routine builds an xml object inside a <result></result> node. 
        /// Right now, the response is in JSON format, and conforms to the google data visualization spec 0.5
        /// </summary>
        /// <returns></returns>
        public static string processAjaxData(CoreController core) {
            string result = "";
            try {
                LogController.handleError(core, new GenericException("executeRoute_ProcessAjaxData deprecated"));
                //string RemoteKey = core.docProperties.getText("key");
                //string EncodedArgs = core.docProperties.getText("args");
                //int PageSize = core.docProperties.getInteger("pagesize");
                //int PageNumber = core.docProperties.getInteger("pagenumber");
                //RemoteFormatEnum RemoteFormat = null;
                //switch (genericController.vbLCase(core.docProperties.getText("responseformat"))) {
                //    case "jsonnamevalue":
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameValue;
                //        break;
                //    case "jsonnamearray":
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonNameArray;
                //        break;
                //    default: //jsontable
                //        RemoteFormat = RemoteFormatEnum.RemoteFormatJsonTable;
                //        break;
                //}
                ////
                //return "";
                ////
                ////
                //// Handle common work
                ////
                //if (PageNumber == 0) {
                //    PageNumber = 1;
                //}
                //if (PageSize == 0) {
                //    PageSize = 100;
                //}
                //int maxRows = 0;
                //if (maxRows != 0 && PageSize > maxRows) {
                //    PageSize = maxRows;
                //}
                ////
                //string[] ArgName = { };
                //string[] ArgValue = { };
                //if (!string.IsNullOrEmpty(EncodedArgs)) {
                //    string Args = EncodedArgs;
                //    string[] ArgArray = Args.Split('&');
                //    int ArgCnt = ArgArray.GetUpperBound(0) + 1;
                //    ArgName = new string[ArgCnt + 1];
                //    ArgValue = new string[ArgCnt + 1];
                //    for (var Ptr = 0; Ptr < ArgCnt; Ptr++) {
                //        int Pos = genericController.vbInstr(1, ArgArray[Ptr], "=");
                //        if (Pos > 0) {
                //            ArgName[Ptr] = genericController.DecodeResponseVariable(ArgArray[Ptr].Left( Pos - 1));
                //            ArgValue[Ptr] = genericController.DecodeResponseVariable(ArgArray[Ptr].Substring(Pos));
                //        }
                //    }
                //}
                ////
                //// main_Get values out of the remote query record
                ////
                //GoogleVisualizationType gv = new GoogleVisualizationType();
                //gv.status = GoogleVisualizationStatusEnum.OK;
                ////
                //if (gv.status == GoogleVisualizationStatusEnum.OK) {
                //    string SetPairString = "";
                //    int QueryType = 0;
                //    string ContentName = "";
                //    string Criteria = "";
                //    string SortFieldList = "";
                //    bool AllowInactiveRecords2 = false;
                //    string SelectFieldList = "";
                //    int CS = db.csOpen("Remote Queries", "((VisitId=" + core.doc.authContext.visit.id + ")and(remotekey=" + DbController.encodeSQLText(RemoteKey) + "))");
                //    if (db.csOk()) {
                //        //
                //        // Use user definied query
                //        //
                //        string SQLQuery = db.csGetText("sqlquery");
                //        //DataSource = dataSourceModel.create(Me, db.cs_getInteger(CS, "datasourceid"), New List(Of String))
                //        maxRows = db.csGetInteger("maxrows");
                //        QueryType = db.csGetInteger("QueryTypeID");
                //        ContentName = db.csGet("ContentID");
                //        Criteria = db.csGetText("Criteria");
                //        SortFieldList = db.csGetText("SortFieldList");
                //        AllowInactiveRecords2 = db.csGetBoolean("AllowInactiveRecords");
                //        SelectFieldList = db.csGetText("SelectFieldList");
                //    } else {
                //        //
                //        // Try Hardcoded queries
                //        //
                //        switch (genericController.vbLCase(RemoteKey)) {
                //            case "ccfieldhelpupdate":
                //                //
                //                // developers editing field help
                //                //
                //                if (!core.doc.authContext.user.Developer) {
                //                    gv.status = GoogleVisualizationStatusEnum.ErrorStatus;
                //                    int Ptr = 0;
                //                    if (gv.errors.GetType().IsArray) {
                //                        Ptr = gv.errors.GetUpperBound(0) + 1;
                //                    }
                //                    Array.Resize(ref gv.errors, Ptr);
                //                    gv.errors[Ptr] = "permission error";
                //                } else {
                //                    QueryType = QueryTypeUpdateContent;
                //                    ContentName = "Content Field Help";
                //                    Criteria = "";
                //                    AllowInactiveRecords2 = false;
                //                }
                //                //Case Else
                //                //    '
                //                //    ' query not found
                //                //    '
                //                //    gv.status = GoogleVisualizationStatusEnum.ErrorStatus
                //                //    If IsArray(gv.errors) Then
                //                //        Ptr = 0
                //                //    Else
                //                //        Ptr = UBound(gv.errors) + 1
                //                //    End If
                //                //    ReDim gv.errors[Ptr]
                //                //    gv.errors[Ptr] = "query not found"
                //                break;
                //        }
                //    }
                //    db.csClose();
                //    //
                //    if (gv.status == GoogleVisualizationStatusEnum.OK) {
                //        switch (QueryType) {
                //            case QueryTypeUpdateContent:
                //                //
                //                // Contensive Content Update, args are field=value updates
                //                // !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                //                //
                //                //
                //                // Go though args and main_Get Set and Criteria
                //                //
                //                SetPairString = "";
                //                Criteria = "";
                //                for (var Ptr = 0; Ptr < ArgName.Length; Ptr++) {
                //                    if (genericController.vbLCase(ArgName[Ptr]) == "setpairs") {
                //                        SetPairString = ArgValue[Ptr];
                //                    } else if (genericController.vbLCase(ArgName[Ptr]) == "criteria") {
                //                        Criteria = ArgValue[Ptr];
                //                    }
                //                }
                //                //
                //                // Open the content and cycle through each setPair
                //                //
                //                CS = db.csOpen(ContentName, Criteria, SortFieldList, AllowInactiveRecords2, 0, false, false, SelectFieldList);
                //                if (db.csOk()) {
                //                    //
                //                    // update by looping through the args and setting name=values
                //                    //
                //                    string[] SetPairs = SetPairString.Split('&');
                //                    for (var Ptr = 0; Ptr <= SetPairs.GetUpperBound(0); Ptr++) {
                //                        if (!string.IsNullOrEmpty(SetPairs[Ptr])) {
                //                            int Pos = genericController.vbInstr(1, SetPairs[Ptr], "=");
                //                            if (Pos > 0) {
                //                                string FieldValue = genericController.DecodeResponseVariable(SetPairs[Ptr].Substring(Pos));
                //                                string FieldName = genericController.DecodeResponseVariable(SetPairs[Ptr].Left( Pos - 1));
                //                                if (!Models.Complex.cdefModel.isContentFieldSupported(core, ContentName, FieldName)) {
                //                                    string errorMessage = "result, QueryTypeUpdateContent, key [" + RemoteKey + "], bad field [" + FieldName + "] skipped";
                //                                    throw (new GenericException(errorMessage));
                //                                } else {
                //                                    db.csSet(FieldName, FieldValue);
                //                                }
                //                            }
                //                        }
                //                    }
                //                }
                //                db.csClose();
                //                //Case QueryTypeInsertContent
                //                //    '
                //                //    ' !!!! only allow inbound hits with a referrer from this site - later use the aggregate access table
                //                //    '
                //                //    '
                //                //    ' Contensive Content Insert, args are field=value
                //                //    '
                //                //    'CS = main_InsertCSContent(ContentName)
                //                break;
                //            default:
                //                break;
                //        }
                //        //
                //        // output
                //        //
                //        GoogleDataType gd = new GoogleDataType();
                //        gd.IsEmpty = true;
                //        //
                //        string Copy = remoteQueryController.main_FormatRemoteQueryOutput(core, gd, RemoteFormat);
                //        Copy = htmlController.encodeHTML(Copy);
                //        result = "<data>" + Copy + "</data>";
                //    }
                //}
            } catch (Exception ex) {
                throw (ex);
            }
            return result;
        }
    }
}
