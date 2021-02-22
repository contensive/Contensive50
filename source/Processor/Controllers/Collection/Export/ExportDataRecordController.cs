using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Processor.Models.Domain;
using System.Text;

namespace Contensive.Processor.Controllers {
    /// <summary>
    /// Control export of data records
    /// </summary>
    public static class ExportDataRecordController {
        // 
        // ====================================================================================================
        /// <summary>
        /// Create XML string for the Data node of the collection file from DataRecordList (list from collection record data-records).
        /// </summary>
        /// <param name="cp"></param>
        /// <param name="dataRecordObjList">crlf delimited list of records to export. Each line is either the name of hte content, or content-record-guid</param>
        /// <param name="tempPathFileList"></param>
        /// <param name="tempExportPath"></param>
        /// <returns></returns>
        public static string getNodeList(CPBaseClass cp, List<CollectionDataRecordModel> dataRecordObjList, List<string> tempPathFileList, string tempExportPath) {
            try {
                var result = new StringBuilder();
                if (true) {
                    var RecordNodes = new StringBuilder();
                    foreach (CollectionDataRecordModel dataRecord in dataRecordObjList) {
                        //
                        // -- export one line from the data-record
                        string FieldNodes = "";
                        if (true) {
                            if (true) {
                                int DataContentId = cp.Content.GetID(dataRecord.contentName);
                                if (DataContentId <= 0)
                                    RecordNodes.Append ( ""
                                        + System.Environment.NewLine + "\t"
                                                + "<!-- data missing, content not found during export, "
                                                + "content=\"" + dataRecord.contentName + "\" "
                                                + "guid=\"" + dataRecord.recordGuid + "\" "
                                                + "name=\"" + dataRecord.recordName + "\" -->");
                                else {
                                    string Criteria;
                                    if (!string.IsNullOrEmpty(dataRecord.recordGuid)) {
                                        // 
                                        // guid {726ED098-5A9E-49A9-8840-767A74F41D01} format
                                        Criteria = "ccguid=" + cp.Db.EncodeSQLText(dataRecord.recordGuid);
                                    } else if (string.IsNullOrEmpty(dataRecord.recordName)) {
                                        // 
                                        // name and guid empty, export all records
                                        Criteria = "";
                                    } else if ((Strings.Len(dataRecord.recordName) == 36) & (Strings.Mid(dataRecord.recordName, 9, 1) == "-")) {
                                        // 
                                        // use name as guid 726ED098-5A9E-49A9-8840-767A74F41D01 format
                                        Criteria = "ccguid=" + cp.Db.EncodeSQLText(dataRecord.recordName);
                                    } else if ((Strings.Len(dataRecord.recordName) == 32) & (Strings.InStr(1, dataRecord.recordName, " ") == 0)) {
                                        // 
                                        // use name as guid 726ED0985A9E49A98840767A74F41D01 format
                                        Criteria = "ccguid=" + cp.Db.EncodeSQLText(dataRecord.recordName);
                                    } else {
                                        // 
                                        // use name as-is
                                        Criteria = "name=" + cp.Db.EncodeSQLText(dataRecord.recordName);
                                    }
                                    using (CPCSBaseClass CSData = cp.CSNew()) {
                                        if (!CSData.Open(dataRecord.contentName, Criteria, "id"))
                                            RecordNodes.Append( ""
                                                + System.Environment.NewLine + "\t"
                                                + "<!-- data missing, record not found during export, "
                                                + "content=\"" + dataRecord.contentName + "\" "
                                                + "guid=\"" + dataRecord.recordGuid + "\" "
                                                + "name=\"" + dataRecord.recordName + "\" -->");
                                        else {
                                            // 
                                            // determine all valid fields
                                            // 
                                            int fieldCnt = 0;
                                            //string Sql = "select * from ccFields where contentid=" + DataContentId;
                                            string fieldLookupListValue = "";
                                            string[] fieldNames = Array.Empty<string>();
                                            int[] fieldTypes = Array.Empty<int>();
                                            string[] fieldLookupContent = Array.Empty<string>();
                                            string[] fieldLookupList = Array.Empty<string>();
                                            string FieldLookupContentName;
                                            int FieldTypeNumber;
                                            string FieldName;
                                            using (CPCSBaseClass csFields = cp.CSNew()) {
                                                if (csFields.Open("content fields", "contentid=" + DataContentId)) {
                                                    do {
                                                        FieldName = csFields.GetText("name");
                                                        if (FieldName != "") {
                                                            int FieldLookupContentID = 0;
                                                            FieldLookupContentName = "";
                                                            FieldTypeNumber = csFields.GetInteger("type");
                                                            switch (Strings.LCase(FieldName)) {
                                                                case "ccguid":
                                                                case "name":
                                                                case "id":
                                                                case "dateadded":
                                                                case "createdby":
                                                                case "modifiedby":
                                                                case "modifieddate":
                                                                case "createkey":
                                                                case "contentcontrolid":
                                                                case "editsourceid":
                                                                case "editarchive":
                                                                case "editblank":
                                                                case "contentcategoryid": {
                                                                        break;
                                                                    }

                                                                default: {
                                                                        if (FieldTypeNumber == (int)CPContentBaseClass.FieldTypeIdEnum.Lookup) {
                                                                            FieldLookupContentID = csFields.GetInteger("Lookupcontentid");
                                                                            fieldLookupListValue = csFields.GetText("LookupList");
                                                                            if (FieldLookupContentID != 0)
                                                                                FieldLookupContentName = cp.Content.GetRecordName("content", FieldLookupContentID);
                                                                        }
                                                                        //CPContentBaseClass.FieldTypeIdEnum.File
                                                                        switch (FieldTypeNumber) {
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Integer:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Text:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.LongText:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Boolean:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Date:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.File:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Lookup:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Currency:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileText:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileImage:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Float:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileCSS:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileXML:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileJavascript:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.Link:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.ResourceLink:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.HTML:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileHTML:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.HTMLCode:
                                                                            case (int)CPContentBaseClass.FieldTypeIdEnum.FileHTMLCode: {
                                                                                    var oldFieldNames = fieldNames;
                                                                                    fieldNames = new string[fieldCnt + 1];
                                                                                    // 
                                                                                    // this is a keeper
                                                                                    // 
                                                                                    if (oldFieldNames != null)
                                                                                        Array.Copy(oldFieldNames, fieldNames, Math.Min(fieldCnt + 1, oldFieldNames.Length));
                                                                                    var oldFieldTypes = fieldTypes;
                                                                                    fieldTypes = new int[fieldCnt + 1];
                                                                                    if (oldFieldTypes != null)
                                                                                        Array.Copy(oldFieldTypes, fieldTypes, Math.Min(fieldCnt + 1, oldFieldTypes.Length));
                                                                                    var oldFieldLookupContent = fieldLookupContent;
                                                                                    fieldLookupContent = new string[fieldCnt + 1];
                                                                                    if (oldFieldLookupContent != null)
                                                                                        Array.Copy(oldFieldLookupContent, fieldLookupContent, Math.Min(fieldCnt + 1, oldFieldLookupContent.Length));
                                                                                    var oldFieldLookupList = fieldLookupList;
                                                                                    fieldLookupList = new string[fieldCnt + 1];
                                                                                    if (oldFieldLookupList != null)
                                                                                        Array.Copy(oldFieldLookupList, fieldLookupList, Math.Min(fieldCnt + 1, oldFieldLookupList.Length));
                                                                                    // fieldLookupContent
                                                                                    fieldNames[fieldCnt] = FieldName;
                                                                                    fieldTypes[fieldCnt] = FieldTypeNumber;
                                                                                    fieldLookupContent[fieldCnt] = FieldLookupContentName;
                                                                                    fieldLookupList[fieldCnt] = fieldLookupListValue;
                                                                                    fieldCnt = fieldCnt + 1;
                                                                                    break;
                                                                                }
                                                                        }

                                                                        break;
                                                                    }
                                                            }
                                                        }

                                                        csFields.GoNext();
                                                    }
                                                    while (csFields.OK());
                                                }
                                                csFields.Close();
                                            }
                                            // 
                                            // output records
                                            // 
                                            dataRecord.recordGuid = "";
                                            while (CSData.OK()) {
                                                FieldNodes = "";
                                                dataRecord.recordName = CSData.GetText("name");
                                                if (true) {
                                                    dataRecord.recordGuid = CSData.GetText("ccguid");
                                                    if (dataRecord.recordGuid == "") {
                                                        dataRecord.recordGuid = cp.Utils.CreateGuid();
                                                        CSData.SetField("ccGuid", dataRecord.recordGuid);
                                                    }
                                                }
                                                int fieldPtr;
                                                for (fieldPtr = 0; fieldPtr <= fieldCnt - 1; fieldPtr++) {
                                                    FieldName = fieldNames[fieldPtr];
                                                    FieldTypeNumber = cp.Utils.EncodeInteger(fieldTypes[fieldPtr]);
                                                    // Dim ContentID As Integer
                                                    string FieldValue;
                                                    switch (FieldTypeNumber) {
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.File:
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.FileImage: {
                                                                // 
                                                                // files -- copy pathFilename to tmp folder and save pathFilename to fieldValue
                                                                FieldValue = CSData.GetText(FieldName).ToString();
                                                                if ((!string.IsNullOrWhiteSpace(FieldValue))) {
                                                                    string pathFilename = FieldValue;
                                                                    cp.CdnFiles.Copy(pathFilename, tempExportPath + pathFilename, cp.TempFiles);
                                                                    if (!tempPathFileList.Contains(tempExportPath + pathFilename)) {
                                                                        tempPathFileList.Add(tempExportPath + pathFilename);
                                                                        string path = FileController.getPath(pathFilename);
                                                                        string filename = FileController.getFilename(pathFilename);
                                                                        result.Append(  System.Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(filename) + "\" type=\"content\" path=\"" + System.Net.WebUtility.HtmlEncode(path) + "\" />");
                                                                    }
                                                                }

                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Boolean: {
                                                                // 
                                                                // true/false
                                                                // 
                                                                FieldValue = CSData.GetBoolean(FieldName).ToString();
                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.FileCSS:
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.FileJavascript:
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.FileText:
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.FileXML: {
                                                                // 
                                                                // text files
                                                                // 
                                                                FieldValue = CSData.GetText(FieldName);
                                                                FieldValue = ExportController.encodeCData(FieldValue);
                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Integer: {
                                                                // 
                                                                // integer
                                                                // 
                                                                FieldValue = CSData.GetInteger(FieldName).ToString();
                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Currency:
                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Float: {
                                                                // 
                                                                // numbers
                                                                // 
                                                                FieldValue = CSData.GetNumber(FieldName).ToString();
                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Date: {
                                                                // 
                                                                // date
                                                                // 
                                                                FieldValue = CSData.GetDate(FieldName).ToString();
                                                                break;
                                                            }

                                                        case (int)CPContentBaseClass.FieldTypeIdEnum.Lookup: {
                                                                // 
                                                                // lookup
                                                                // 
                                                                FieldValue = "";
                                                                int FieldValueInteger = CSData.GetInteger(FieldName);
                                                                if ((FieldValueInteger != 0)) {
                                                                    FieldLookupContentName = fieldLookupContent[fieldPtr];
                                                                    fieldLookupListValue = fieldLookupList[fieldPtr];
                                                                    if ((FieldLookupContentName != "")) {
                                                                        // 
                                                                        // content lookup
                                                                        // 
                                                                        if (cp.Content.IsField(FieldLookupContentName, "ccguid")) {
                                                                            using (CPCSBaseClass CSlookup = cp.CSNew()) {
                                                                                CSlookup.OpenRecord(FieldLookupContentName, FieldValueInteger);
                                                                                if (CSlookup.OK()) {
                                                                                    FieldValue = CSlookup.GetText("ccguid");
                                                                                    if (FieldValue == "") {
                                                                                        FieldValue = cp.Utils.CreateGuid();
                                                                                        CSlookup.SetField("ccGuid", FieldValue);
                                                                                    }
                                                                                }
                                                                                CSlookup.Close();
                                                                            }
                                                                        }
                                                                    } else if (fieldLookupListValue != "")
                                                                        // 
                                                                        // list lookup, ok to save integer
                                                                        // 
                                                                        FieldValue = FieldValueInteger.ToString();
                                                                }

                                                                break;
                                                            }

                                                        default: {
                                                                // 
                                                                // text types
                                                                // 
                                                                FieldValue = CSData.GetText(FieldName);
                                                                FieldValue = ExportController.encodeCData(FieldValue);
                                                                break;
                                                            }
                                                    }
                                                    FieldNodes = FieldNodes + System.Environment.NewLine + "\t" + "<field name=\"" + System.Net.WebUtility.HtmlEncode(FieldName) + "\">" + FieldValue + "</field>";
                                                }
                                                RecordNodes.Append( ""
                                                        + System.Environment.NewLine + "\t" + "<record content=\"" + System.Net.WebUtility.HtmlEncode(dataRecord.contentName) + "\" guid=\"" + dataRecord.recordGuid + "\" name=\"" + System.Net.WebUtility.HtmlEncode(dataRecord.recordName) + "\">"
                                                        + ExportController.tabIndent(cp, FieldNodes)
                                                        + System.Environment.NewLine + "\t" + "</record>");
                                                CSData.GoNext();
                                            }
                                        }
                                        CSData.Close();
                                    }
                                }
                            }
                        }
                    }
                    if (RecordNodes.Length > 0 )
                        result.Append( ""
                            + System.Environment.NewLine + "\t" + "<data>"
                            + ExportController.tabIndent(cp, RecordNodes.ToString())
                            + System.Environment.NewLine + "\t" + "</data>");
                }
                return result.ToString();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
    }
}
