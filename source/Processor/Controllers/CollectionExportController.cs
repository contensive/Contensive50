
//using System;
//using System.Collections.Generic;
//using System.Globalization;
//using Contensive.BaseClasses;
//using Contensive.Models.Db;
//using static Contensive.BaseClasses.CPContentBaseClass;

//namespace Contensive.Processor.Controllers {
//    public class CollectionExportClass {
//        //
//        //====================================================================================================
//        /// <summary>
//        /// create the colleciton zip file and return the pathFilename in the Cdn
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="CollectionID"></param>
//        /// <returns></returns>
//        private static string createCollectionZip_returnCdnPathFilename(CPClass cp, int CollectionID) {
//            string cdnExportZip_Filename = "";
//            try {
//                CPCSBaseClass CS = cp.CSNew();
//                CS.OpenRecord("Add-on Collections", CollectionID);
//                if (!CS.OK()) {
//                    cp.UserError.Add("The collection you selected could not be found");
//                } else {
//                    string collectionXml = "<?xml version=\"1.0\" encoding=\"windows-1252\"?>";
//                    //
//                    string CollectionName = CS.GetText("name");
//                    string CollectionGuid = CS.GetText("ccGuid");
//                    if (string.IsNullOrEmpty(CollectionGuid)) {
//                        CollectionGuid = cp.Utils.CreateGuid();
//                        CS.SetField("ccGuid", CollectionGuid);
//                    }
//                    string onInstallAddonGuid = "";
//                    if (CS.FieldOK("onInstallAddonId")) {
//                        int onInstallAddonId = CS.GetInteger("onInstallAddonId");
//                        if (onInstallAddonId > 0) {
//                            AddonModel addon = DbBaseModel.create<AddonModel>(cp, onInstallAddonId);
//                            onInstallAddonGuid = addon.ccguid;
//                        }
//                    }
//                    collectionXml += Environment.NewLine + "<Collection";
//                    collectionXml += " name=\"" + CollectionName + "\"";
//                    collectionXml += " guid=\"" + CollectionGuid + "\"";
//                    collectionXml += " system=\"" + kmaGetYesNo(cp, CS.GetBoolean("system")) + "\"";
//                    collectionXml += " updatable=\"" + kmaGetYesNo(cp, CS.GetBoolean("updatable")) + "\"";
//                    collectionXml += " blockNavigatorNode=\"" + kmaGetYesNo(cp, CS.GetBoolean("blockNavigatorNode")) + "\"";
//                    collectionXml += " onInstallAddonGuid=\"" + onInstallAddonGuid + "\"";
//                    collectionXml += ">";
//                    //
//                    // Archive Filenames
//                    //   copy all files to be included into the cdnExportFilesPath folder
//                    //   build the tmp zip file
//                    //   copy it to the cdnZip file
//                    //
//                    string tempExportPath = "CollectionExport" + Guid.NewGuid().ToString() + "\\";
//                    string tempExportXml_Filename = encodeFilename(cp, CollectionName + ".xml");
//                    string tempExportZip_Filename = encodeFilename(cp, CollectionName + ".zip");
//                    cdnExportZip_Filename = encodeFilename(cp, CollectionName + ".zip");
//                    //
//                    // Build executable file list Resource Node so executables can be added to addons for Version40compatibility
//                    //   but save it for the end, executableFileList
//                    //
//                    string AddonPath = "addons\\";
//                    string FileList = CS.GetText("execFileList");
//                    string Path = null;
//                    string Filename = null;
//                    string PathFilename = null;
//                    int Ptr = 0;
//                    string[] Files = null;
//                    int ResourceCnt = 0;
//                    int Pos = 0;
//                    List<string> tempPathFileList = new List<string>();
//                    string CollectionPath = "";
//                    string ExecFileListNode = "";
//                    if (!string.IsNullOrEmpty(FileList)) {
//                        DateTime LastChangeDate = default(DateTime);
//                        //
//                        // There are executable files to include in the collection
//                        //   If installed, source path is collectionpath, if not installed, collectionpath will be empty
//                        //   and file will be sourced right from addon path
//                        //
//                        GetLocalCollectionArgs(cp, CollectionGuid, ref CollectionPath, ref LastChangeDate);
//                        if (!string.IsNullOrEmpty(CollectionPath)) {
//                            CollectionPath = CollectionPath + "\\";
//                        }
//                        Files = Microsoft.VisualBasic.Strings.Split(FileList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        for (Ptr = 0; Ptr <= Files.GetUpperBound(0); Ptr++) {
//                            PathFilename = Files[Ptr];
//                            if (!string.IsNullOrEmpty(PathFilename)) {
//                                PathFilename = PathFilename.Replace("\\", "/");
//                                Path = "";
//                                Filename = PathFilename;
//                                Pos = PathFilename.LastIndexOf("/") + 1;
//                                if (Pos > 0) {
//                                    Filename = PathFilename.Substring(Pos);
//                                    Path = PathFilename.Substring(0, Pos - 1);
//                                }
//                                string ManualFilename = "";
//                                if (Filename.ToLower(CultureInfo.InvariantCulture) != ManualFilename.ToLower(CultureInfo.InvariantCulture)) {
//                                    cp.PrivateFiles.Copy(AddonPath + CollectionPath + Filename, tempExportPath + Filename, cp.TempFiles);
//                                    if (!tempPathFileList.Contains(tempExportPath + Filename)) {
//                                        tempPathFileList.Add(tempExportPath + Filename);
//                                        ExecFileListNode = ExecFileListNode + Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"executable\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />";
//                                    }
//                                }
//                                ResourceCnt = ResourceCnt + 1;
//                            }
//                        }
//                    }
//                    //
//                    // helpLink
//                    //
//                    if (CS.FieldOK("HelpLink")) {
//                        collectionXml = collectionXml + Environment.NewLine + "\t" + "<HelpLink>" + System.Net.WebUtility.HtmlEncode(CS.GetText("HelpLink")) + "</HelpLink>";
//                    }
//                    //
//                    // Help
//                    //
//                    collectionXml = collectionXml + Environment.NewLine + "\t" + "<Help>" + System.Net.WebUtility.HtmlEncode(CS.GetText("Help")) + "</Help>";
//                    CPCSBaseClass CS2 = cp.CSNew();
//                    //
//                    // Addons
//                    //
//                    CS2.Open("Add-ons", "collectionid=" + CollectionID, "name", true, "id");
//                    string IncludeModuleGuidList = "";
//                    string IncludeSharedStyleGuidList = "";
//                    while (CS2.OK()) {
//                        collectionXml = collectionXml + GetAddonNode(cp, CS2.GetInteger("id"), ref IncludeModuleGuidList, ref IncludeSharedStyleGuidList);
//                        CS2.GoNext();
//                    }
//                    //
//                    // Data Records
//                    //
//                    string DataRecordList = CS.GetText("DataRecordList");
//                    if (!string.IsNullOrEmpty(DataRecordList)) {
//                        string[] DataRecords = Microsoft.VisualBasic.Strings.Split(DataRecordList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        string RecordNodes = "";
//                        for (Ptr = 0; Ptr <= DataRecords.GetUpperBound(0); Ptr++) {
//                            string FieldNodes = "";
//                            string DataRecordName = "";
//                            string DataRecordGuid = "";
//                            string DataRecord = DataRecords[Ptr];
//                            if (!string.IsNullOrEmpty(DataRecord)) {
//                                string[] DataSplit = DataRecord.Split(',');
//                                if (DataSplit.GetUpperBound(0) >= 0) {
//                                    string DataContentName = DataSplit[0].Trim(' ');
//                                    int DataContentId = cp.Content.GetID(DataContentName);
//                                    if (DataContentId <= 0) {
//                                        RecordNodes = ""
//                                            + RecordNodes + Environment.NewLine + "\t" + "<!-- data missing, content not found during export, content=\"" + DataContentName + "\" guid=\"" + DataRecordGuid + "\" name=\"" + DataRecordName + "\" -->";
//                                    } else {
//                                        bool supportsGuid = cp.Content.IsField(DataContentName, "ccguid");
//                                        string Criteria = null;
//                                        if (DataSplit.GetUpperBound(0) == 0) {
//                                            Criteria = "";
//                                        } else {
//                                            string TestString = DataSplit[1].Trim(' ');
//                                            if (string.IsNullOrEmpty(TestString)) {
//                                                //
//                                                // blank is a select all
//                                                //
//                                                Criteria = "";
//                                                DataRecordName = "";
//                                                DataRecordGuid = "";
//                                            } else if (!supportsGuid) {
//                                                //
//                                                // if no guid, this is name
//                                                //
//                                                DataRecordName = TestString;
//                                                DataRecordGuid = "";
//                                                Criteria = "name=" + cp.Db.EncodeSQLText(DataRecordName);
//                                            } else if ((TestString.Length == 38) && (TestString.Substring(0, 1) == "{") && (TestString.Substring(TestString.Length - 1) == "}")) {
//                                                //
//                                                // guid {726ED098-5A9E-49A9-8840-767A74F41D01} format
//                                                //
//                                                DataRecordGuid = TestString;
//                                                DataRecordName = "";
//                                                Criteria = "ccguid=" + cp.Db.EncodeSQLText(DataRecordGuid);
//                                            } else if ((TestString.Length == 36) && (TestString.Substring(8, 1) == "-")) {
//                                                //
//                                                // guid 726ED098-5A9E-49A9-8840-767A74F41D01 format
//                                                //
//                                                DataRecordGuid = TestString;
//                                                DataRecordName = "";
//                                                Criteria = "ccguid=" + cp.Db.EncodeSQLText(DataRecordGuid);
//                                            } else if ((TestString.Length == 32) && (TestString.IndexOf(" ") + 1 == 0)) {
//                                                //
//                                                // guid 726ED0985A9E49A98840767A74F41D01 format
//                                                //
//                                                DataRecordGuid = TestString;
//                                                DataRecordName = "";
//                                                Criteria = "ccguid=" + cp.Db.EncodeSQLText(DataRecordGuid);
//                                            } else {
//                                                //
//                                                // use name
//                                                //
//                                                DataRecordName = TestString;
//                                                DataRecordGuid = "";
//                                                Criteria = "name=" + cp.Db.EncodeSQLText(DataRecordName);
//                                            }
//                                        }
//                                        CPCSBaseClass CSData = cp.CSNew();
//                                        if (!CSData.Open(DataContentName, Criteria, "id")) {
//                                            RecordNodes = ""
//                                                + RecordNodes + Environment.NewLine + "\t" + "<!-- data missing, record not found during export, content=\"" + DataContentName + "\" guid=\"" + DataRecordGuid + "\" name=\"" + DataRecordName + "\" -->";
//                                        } else {
//                                            //
//                                            // determine all valid fields
//                                            //
//                                            int fieldCnt = 0;
//                                            string Sql = "select * from ccFields where contentid=" + DataContentId;
//                                            CPCSBaseClass csFields = cp.CSNew();
//                                            string fieldLookupListValue = "";
//                                            string[] fieldNames = { };
//                                            int[] fieldTypes = { };
//                                            string[] fieldLookupContent = { };
//                                            string[] fieldLookupList = { };
//                                            string FieldLookupContentName = null;
//                                            int FieldTypeNumber = 0;
//                                            string FieldName = null;
//                                            if (csFields.Open("content fields", "contentid=" + DataContentId)) {
//                                                do {
//                                                    FieldName = csFields.GetText("name");
//                                                    if (!string.IsNullOrEmpty(FieldName)) {
//                                                        int FieldLookupContentID = 0;
//                                                        FieldLookupContentName = "";
//                                                        FieldTypeNumber = csFields.GetInteger("type");
//                                                        switch (FieldName.ToLower(CultureInfo.InvariantCulture)) {
//                                                            case "ccguid":
//                                                            case "name":
//                                                            case "id":
//                                                            case "dateadded":
//                                                            case "createdby":
//                                                            case "modifiedby":
//                                                            case "modifieddate":
//                                                            case "createkey":
//                                                            case "contentcontrolid":
//                                                            case "editsourceid":
//                                                            case "editarchive":
//                                                            case "editblank":
//                                                            case "contentcategoryid":
//                                                            break;
//                                                            default:
//                                                            if (FieldTypeNumber == 7) {
//                                                                FieldLookupContentID = csFields.GetInteger("Lookupcontentid");
//                                                                fieldLookupListValue = csFields.GetText("LookupList");
//                                                                if (FieldLookupContentID != 0) {
//                                                                    FieldLookupContentName = cp.Content.GetRecordName("content", FieldLookupContentID);
//                                                                }
//                                                            }
//                                                            switch (FieldTypeNumber) {
//                                                                case (int)FieldTypeIdEnum.Lookup:
//                                                                case (int)FieldTypeIdEnum.Boolean:
//                                                                case (int)FieldTypeIdEnum.FileCSS:
//                                                                case (int)FieldTypeIdEnum.FileJavascript:
//                                                                case (int)FieldTypeIdEnum.FileText:
//                                                                case (int)FieldTypeIdEnum.FileXML:
//                                                                case (int)FieldTypeIdEnum.Currency:
//                                                                case (int)FieldTypeIdEnum.Float:
//                                                                case (int)FieldTypeIdEnum.Integer:
//                                                                case (int)FieldTypeIdEnum.Date:
//                                                                case (int)FieldTypeIdEnum.Link:
//                                                                case (int)FieldTypeIdEnum.LongText:
//                                                                case (int)FieldTypeIdEnum.ResourceLink:
//                                                                case (int)FieldTypeIdEnum.Text:
//                                                                case (int)FieldTypeIdEnum.HTML:
//                                                                case (int)FieldTypeIdEnum.FileHTML:
//                                                                //
//                                                                // this is a keeper
//                                                                //
//                                                                Array.Resize(ref fieldNames, fieldCnt + 1);
//                                                                Array.Resize(ref fieldTypes, fieldCnt + 1);
//                                                                Array.Resize(ref fieldLookupContent, fieldCnt + 1);
//                                                                Array.Resize(ref fieldLookupList, fieldCnt + 1);
//                                                                fieldNames[fieldCnt] = FieldName;
//                                                                fieldTypes[fieldCnt] = FieldTypeNumber;
//                                                                fieldLookupContent[fieldCnt] = FieldLookupContentName;
//                                                                fieldLookupList[fieldCnt] = fieldLookupListValue;
//                                                                fieldCnt = fieldCnt + 1;
//                                                                break;
//                                                            }
//                                                            break;
//                                                        }
//                                                    }
//                                                    csFields.GoNext();
//                                                }
//                                                while (csFields.OK());
//                                            }
//                                            csFields.Close();
//                                            //
//                                            // output records
//                                            //
//                                            DataRecordGuid = "";
//                                            while (CSData.OK()) {
//                                                FieldNodes = "";
//                                                DataRecordName = CSData.GetText("name");
//                                                if (supportsGuid) {
//                                                    DataRecordGuid = CSData.GetText("ccguid");
//                                                    if (string.IsNullOrEmpty(DataRecordGuid)) {
//                                                        DataRecordGuid = cp.Utils.CreateGuid();
//                                                        CSData.SetField("ccGuid", DataRecordGuid);
//                                                    }
//                                                }
//                                                int fieldPtr = 0;
//                                                for (fieldPtr = 0; fieldPtr < fieldCnt; fieldPtr++) {
//                                                    FieldName = fieldNames[fieldPtr];
//                                                    FieldTypeNumber = cp.Utils.EncodeInteger(fieldTypes[fieldPtr]);
//                                                    string FieldValue = null;
//                                                    switch (FieldTypeNumber) {
//                                                        case (int)FieldTypeIdEnum.Boolean:
//                                                        //
//                                                        // true/false
//                                                        //
//                                                        FieldValue = CSData.GetBoolean(FieldName).ToString();
//                                                        break;
//                                                        case (int)FieldTypeIdEnum.FileCSS:
//                                                        case (int)FieldTypeIdEnum.FileJavascript:
//                                                        case (int)FieldTypeIdEnum.FileText:
//                                                        case (int)FieldTypeIdEnum.FileXML:
//                                                        //
//                                                        // text files
//                                                        //
//                                                        FieldValue = CSData.GetText(FieldName);
//                                                        FieldValue = EncodeCData(cp, FieldValue);
//                                                        break;
//                                                        case (int)FieldTypeIdEnum.Integer:
//                                                        //
//                                                        // integer
//                                                        //
//                                                        FieldValue = CSData.GetInteger(FieldName).ToString();
//                                                        break;
//                                                        case (int)FieldTypeIdEnum.Currency:
//                                                        case (int)FieldTypeIdEnum.Float:
//                                                        //
//                                                        // numbers
//                                                        //
//                                                        FieldValue = CSData.GetNumber(FieldName).ToString();
//                                                        break;
//                                                        case (int)FieldTypeIdEnum.Date:
//                                                        //
//                                                        // date
//                                                        //
//                                                        FieldValue = CSData.GetDate(FieldName).ToString();
//                                                        break;
//                                                        case (int)FieldTypeIdEnum.Lookup:
//                                                        //
//                                                        // lookup
//                                                        //
//                                                        FieldValue = "";
//                                                        int FieldValueInteger = CSData.GetInteger(FieldName);
//                                                        if (FieldValueInteger != 0) {
//                                                            FieldLookupContentName = fieldLookupContent[fieldPtr];
//                                                            fieldLookupListValue = fieldLookupList[fieldPtr];
//                                                            if (!string.IsNullOrEmpty(FieldLookupContentName)) {
//                                                                //
//                                                                // content lookup
//                                                                //
//                                                                if (cp.Content.IsField(FieldLookupContentName, "ccguid")) {
//                                                                    CPCSBaseClass CSlookup = cp.CSNew();
//                                                                    CSlookup.OpenRecord(FieldLookupContentName, FieldValueInteger);
//                                                                    if (CSlookup.OK()) {
//                                                                        FieldValue = CSlookup.GetText("ccguid");
//                                                                        if (string.IsNullOrEmpty(FieldValue)) {
//                                                                            FieldValue = cp.Utils.CreateGuid();
//                                                                            CSlookup.SetField("ccGuid", FieldValue);
//                                                                        }
//                                                                    }
//                                                                    CSlookup.Close();
//                                                                }
//                                                            } else if (!string.IsNullOrEmpty(fieldLookupListValue)) {
//                                                                //
//                                                                // list lookup, ok to save integer
//                                                                //
//                                                                FieldValue = FieldValueInteger.ToString();
//                                                            }
//                                                        }
//                                                        break;
//                                                        default:
//                                                        //
//                                                        // text types
//                                                        //
//                                                        FieldValue = CSData.GetText(FieldName);
//                                                        FieldValue = EncodeCData(cp, FieldValue);
//                                                        break;
//                                                    }
//                                                    FieldNodes = FieldNodes + Environment.NewLine + "\t" + "<field name=\"" + System.Net.WebUtility.HtmlEncode(FieldName) + "\">" + FieldValue + "</field>";
//                                                }
//                                                RecordNodes = ""
//                                                    + RecordNodes + Environment.NewLine + "\t" + "<record content=\"" + System.Net.WebUtility.HtmlEncode(DataContentName) + "\" guid=\"" + DataRecordGuid + "\" name=\"" + System.Net.WebUtility.HtmlEncode(DataRecordName) + "\">"
//                                                    + tabIndent(cp, FieldNodes) + Environment.NewLine + "\t" + "</record>";
//                                                CSData.GoNext();
//                                            }
//                                        }
//                                        CSData.Close();
//                                    }
//                                }
//                            }
//                        }
//                        if (!string.IsNullOrEmpty(RecordNodes)) {
//                            collectionXml = ""
//                                + collectionXml + Environment.NewLine + "\t" + "<data>"
//                                + tabIndent(cp, RecordNodes) + Environment.NewLine + "\t" + "</data>";
//                        }
//                    }
//                    string Node = null;
//                    //
//                    // CDef
//                    //
//                    foreach (ContentModel content in ContentModel.createListFromCollection(cp, CollectionID)) {
//                        if (string.IsNullOrEmpty(content.ccguid)) {
//                            content.ccguid = cp.Utils.CreateGuid();
//                            content.save(cp);
//                        }
//                        Node = CollectionExportCDefController.getCollectionCdef(cp.core, content.name, false);
//                        //
//                        // remove the <collection> top node
//                        //
//                        Pos = Node.IndexOf("<cdef", System.StringComparison.OrdinalIgnoreCase) + 1;
//                        if (Pos > 0) {
//                            Node = Node.Substring(Pos - 1);
//                            Pos = Node.IndexOf("</cdef>", System.StringComparison.OrdinalIgnoreCase) + 1;
//                            if (Pos > 0) {
//                                Node = Node.Substring(0, Pos + 6);
//                                collectionXml = collectionXml + Environment.NewLine + "\t" + Node;
//                            }
//                        }
//                    }
//                    //
//                    // Scripting Modules
//                    //
//                    if (!string.IsNullOrEmpty(IncludeModuleGuidList)) {
//                        string[] Modules = Microsoft.VisualBasic.Strings.Split(IncludeModuleGuidList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        for (Ptr = 0; Ptr <= Modules.GetUpperBound(0); Ptr++) {
//                            string ModuleGuid = Modules[Ptr];
//                            if (!string.IsNullOrEmpty(ModuleGuid)) {
//                                CS2.Open("Scripting Modules", "ccguid=" + cp.Db.EncodeSQLText(ModuleGuid));
//                                if (CS2.OK()) {
//                                    string Code = Convert.ToString(CS2.GetText("code")).Trim(' ');
//                                    Code = EncodeCData(cp, Code);
//                                    collectionXml = collectionXml + Environment.NewLine + "\t" + "<ScriptingModule Name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\" guid=\"" + ModuleGuid + "\">" + Code + "</ScriptingModule>";
//                                }
//                                CS2.Close();
//                            }
//                        }
//                    }
//                    //
//                    // shared styles
//                    //
//                    string[] recordGuids = null;
//                    string recordGuid = null;
//                    if (!string.IsNullOrEmpty(IncludeSharedStyleGuidList)) {
//                        recordGuids = Microsoft.VisualBasic.Strings.Split(IncludeSharedStyleGuidList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        for (Ptr = 0; Ptr <= recordGuids.GetUpperBound(0); Ptr++) {
//                            recordGuid = recordGuids[Ptr];
//                            if (!string.IsNullOrEmpty(recordGuid)) {
//                                CS2.Open("Shared Styles", "ccguid=" + cp.Db.EncodeSQLText(recordGuid));
//                                if (CS2.OK()) {
//                                    collectionXml = collectionXml + Environment.NewLine + "\t" + "<SharedStyle"
//                                        + " Name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\""
//                                        + " guid=\"" + recordGuid + "\""
//                                        + " alwaysInclude=\"" + CS2.GetBoolean("alwaysInclude") + "\""
//                                        + " prefix=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("prefix")) + "\""
//                                        + " suffix=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("suffix")) + "\""
//                                        + " sortOrder=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("sortOrder")) + "\""
//                                        + ">"
//                                        + EncodeCData(cp, Convert.ToString(CS2.GetText("styleFilename")).Trim(' ')) + "</SharedStyle>";
//                                }
//                                CS2.Close();
//                            }
//                        }
//                    }
//                    //
//                    // Import Collections
//                    //
//                    Node = "";
//                    CPCSBaseClass CS3 = cp.CSNew();
//                    if (CS3.Open("Add-on Collection Parent Rules", "parentid=" + CollectionID)) {
//                        do {
//                            CS2.OpenRecord("Add-on Collections", CS3.GetInteger("childid"));
//                            if (CS2.OK()) {
//                                string Guid = CS2.GetText("ccGuid");
//                                if (string.IsNullOrEmpty(Guid)) {
//                                    Guid = cp.Utils.CreateGuid();
//                                    CS2.SetField("ccGuid", Guid);
//                                }
//                                Node = Node + Environment.NewLine + "\t" + "<ImportCollection name=\"" + System.Net.WebUtility.HtmlEncode(CS2.GetText("name")) + "\">" + Guid + "</ImportCollection>";
//                            }
//                            CS2.Close();
//                            CS3.GoNext();
//                        }
//                        while (CS3.OK());
//                    }
//                    CS3.Close();
//                    collectionXml = collectionXml + Node;
//                    //
//                    // wwwFileList
//                    //
//                    ResourceCnt = 0;
//                    FileList = CS.GetText("wwwFileList");
//                    if (!string.IsNullOrEmpty(FileList)) {
//                        Files = Microsoft.VisualBasic.Strings.Split(FileList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        for (Ptr = 0; Ptr <= Files.GetUpperBound(0); Ptr++) {
//                            PathFilename = Files[Ptr];
//                            if (!string.IsNullOrEmpty(PathFilename)) {
//                                PathFilename = PathFilename.Replace("\\", "/");
//                                Path = "";
//                                Filename = PathFilename;
//                                Pos = PathFilename.LastIndexOf("/") + 1;
//                                if (Pos > 0) {
//                                    Filename = PathFilename.Substring(Pos);
//                                    Path = PathFilename.Substring(0, Pos - 1);
//                                }
//                                if (Filename.ToLower(CultureInfo.InvariantCulture) == "collection.hlp") {
//                                    //
//                                    // legacy file, remove it
//                                    //
//                                } else {
//                                    PathFilename = PathFilename.Replace("/", "\\");
//                                    if (tempPathFileList.Contains(tempExportPath + Filename)) {
//                                        cp.UserError.Add("There was an error exporting this collection because there were multiple files with the same filename [" + Filename + "]");
//                                    } else {
//                                        cp.WwwFiles.Copy(PathFilename, tempExportPath + Filename, cp.TempFiles);
//                                        tempPathFileList.Add(tempExportPath + Filename);
//                                        collectionXml = collectionXml + Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"www\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />";
//                                    }
//                                    ResourceCnt = ResourceCnt + 1;
//                                }
//                            }
//                        }
//                    }
//                    //
//                    // ContentFileList
//                    //
//                    FileList = CS.GetText("ContentFileList");
//                    if (!string.IsNullOrEmpty(FileList)) {
//                        Files = Microsoft.VisualBasic.Strings.Split(FileList, Environment.NewLine, -1, Microsoft.VisualBasic.CompareMethod.Binary);
//                        for (Ptr = 0; Ptr <= Files.GetUpperBound(0); Ptr++) {
//                            PathFilename = Files[Ptr];
//                            if (!string.IsNullOrEmpty(PathFilename)) {
//                                PathFilename = PathFilename.Replace("\\", "/");
//                                Path = "";
//                                Filename = PathFilename;
//                                Pos = PathFilename.LastIndexOf("/") + 1;
//                                if (Pos > 0) {
//                                    Filename = PathFilename.Substring(Pos);
//                                    Path = PathFilename.Substring(0, Pos - 1);
//                                }
//                                if (tempPathFileList.Contains(tempExportPath + Filename)) {
//                                    cp.UserError.Add("There was an error exporting this collection because there were multiple files with the same filename [" + Filename + "]");
//                                } else {
//                                    cp.CdnFiles.Copy(PathFilename, tempExportPath + Filename, cp.TempFiles);
//                                    tempPathFileList.Add(tempExportPath + Filename);
//                                    collectionXml = collectionXml + Environment.NewLine + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"content\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />";
//                                }
//                                ResourceCnt = ResourceCnt + 1;
//                            }
//                        }
//                    }
//                    //
//                    // ExecFileListNode
//                    //
//                    collectionXml = collectionXml + ExecFileListNode;
//                    //
//                    // Other XML
//                    //
//                    string OtherXML = CS.GetText("otherxml");
//                    if (!string.IsNullOrEmpty(OtherXML.Trim(' '))) {
//                        collectionXml = collectionXml + Environment.NewLine + OtherXML;
//                    }
//                    collectionXml = collectionXml + Environment.NewLine + "</Collection>";
//                    CS.Close();
//                    //
//                    // Save the installation file and add it to the archive
//                    //
//                    cp.TempFiles.Save(tempExportPath + tempExportXml_Filename, collectionXml);
//                    if (!tempPathFileList.Contains(tempExportPath + tempExportXml_Filename)) {
//                        tempPathFileList.Add(tempExportPath + tempExportXml_Filename);
//                    }
//                    //
//                    // -- zip up the folder to make the collection zip file in temp filesystem
//                    zipTempCdnFile(cp, tempExportPath + tempExportZip_Filename, tempPathFileList);
//                    //
//                    // -- copy the collection zip file to the cdn filesystem as the download link
//                    cp.TempFiles.Copy(tempExportPath + tempExportZip_Filename, cdnExportZip_Filename, cp.CdnFiles);
//                    //
//                    // -- delete the temp folder
//                    cp.TempFiles.DeleteFolder(tempExportPath);
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "GetCollection");
//            }
//            return cdnExportZip_Filename;
//        }
//        //
//        //====================================================================================================

//        private static string GetAddonNode(CPBaseClass cp, int addonid, ref string Return_IncludeModuleGuidList, ref string Return_IncludeSharedStyleGuidList) {
//            string result = "";
//            try {
//                //
//                int styleId = 0;
//                string fieldType = null;
//                int fieldTypeId = 0;
//                int TriggerContentId = 0;
//                string StylesTest = null;
//                bool BlockEditTools = false;
//                string NavType = null;
//                string Styles = null;
//                int IncludedAddonId = 0;
//                int ScriptingModuleId = 0;
//                string Guid = null;
//                string addonName = null;
//                bool processRunOnce = false;
//                CPCSBaseClass CS = cp.CSNew();
//                CPCSBaseClass CS2 = cp.CSNew();
//                CPCSBaseClass CS3 = cp.CSNew();
//                //
//                if (CS.OpenRecord("Add-ons", addonid)) {
//                    addonName = CS.GetText("name");
//                    processRunOnce = CS.GetBoolean("ProcessRunOnce");
//                    if ((addonName.ToLower(CultureInfo.InvariantCulture) == "oninstall") || (addonName.ToLower(CultureInfo.InvariantCulture) == "_oninstall")) {
//                        processRunOnce = true;
//                    }
//                    result += GetNodeText(cp, "Copy", CS.GetText("Copy"));
//                    result += GetNodeText(cp, "CopyText", CS.GetText("CopyText"));
//                    {
//                        //
//                        // DLL - ActiveX DLL node is being deprecated. This should be in the collection resource section
//                        //
//                        result += GetNodeText(cp, "ActiveXProgramID", CS.GetText("objectprogramid"));
//                        result += GetNodeText(cp, "DotNetClass", CS.GetText("DotNetClass"));
//                    }
//                    {
//                        //
//                        // Features
//                        //
//                        result += GetNodeText(cp, "ArgumentList", CS.GetText("ArgumentList"));
//                        result += GetNodeBoolean(cp, "AsAjax", CS.GetBoolean("AsAjax"));
//                        result += GetNodeBoolean(cp, "Filter", CS.GetBoolean("Filter"));
//                        result += GetNodeText(cp, "Help", CS.GetText("Help"));
//                        result += GetNodeText(cp, "HelpLink", CS.GetText("HelpLink"));
//                        result += Environment.NewLine + "\t" + "<Icon Link=\"" + CS.GetText("iconfilename") + "\" width=\"" + CS.GetInteger("iconWidth") + "\" height=\"" + CS.GetInteger("iconHeight") + "\" sprites=\"" + CS.GetInteger("iconSprites") + "\" />";
//                        result += GetNodeBoolean(cp, "InIframe", CS.GetBoolean("InFrame"));
//                        BlockEditTools = false;
//                        if (CS.FieldOK("BlockEditTools")) {
//                            BlockEditTools = CS.GetBoolean("BlockEditTools");
//                        }
//                        result += GetNodeBoolean(cp, "BlockEditTools", BlockEditTools);
//                    }
//                    {
//                        //
//                        // Form XML
//                        //
//                        result += GetNodeText(cp, "FormXML", CS.GetText("FormXML"));
//                        CS2.Open("Add-on Include Rules", "addonid=" + addonid);
//                        while (CS2.OK()) {
//                            IncludedAddonId = CS2.GetInteger("IncludedAddonID");
//                            CS3.Open("Add-ons", "ID=" + IncludedAddonId);
//                            if (CS3.OK()) {
//                                Guid = CS3.GetText("ccGuid");
//                                if (string.IsNullOrEmpty(Guid)) {
//                                    Guid = cp.Utils.CreateGuid();
//                                    CS3.SetField("ccGuid", Guid);
//                                }
//                                result += Environment.NewLine + "\t" + "<IncludeAddon name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
//                            }
//                            CS3.Close();
//                            CS2.GoNext();
//                        }
//                        CS2.Close();
//                    }
//                    //
//                    result += GetNodeBoolean(cp, "IsInline", CS.GetBoolean("IsInline"));
//                    {
//                        //
//                        // -- javascript (xmlnode may not match Db filename)
//                        result += GetNodeText(cp, "JavascriptInHead", CS.GetText("JSFilename"));
//                        if (cp.Version.CompareTo("4.2") > 0) {
//                            result += GetNodeBoolean(cp, "javascriptForceHead", CS.GetBoolean("javascriptForceHead"));
//                            result += GetNodeText(cp, "JSHeadScriptSrc", CS.GetText("JSHeadScriptSrc"));
//                        } else {
//                            result += GetNodeBoolean(cp, "javascriptForceHead", false);
//                            result += GetNodeText(cp, "JSHeadScriptSrc", "");
//                        }
//                    }
//                    {
//                        //
//                        // -- javascript deprecated
//                        result += GetNodeText(cp, "JSBodyScriptSrc", CS.GetText("JSBodyScriptSrc"), true);
//                        result += GetNodeText(cp, "JavascriptBodyEnd", CS.GetText("JavascriptBodyEnd"), true);
//                        result += GetNodeText(cp, "JavascriptOnLoad", CS.GetText("JavascriptOnLoad"), true);
//                    }
//                    {
//                        //
//                        // -- Placements
//                        result += GetNodeBoolean(cp, "Content", CS.GetBoolean("Content"));
//                        result += GetNodeBoolean(cp, "Template", CS.GetBoolean("Template"));
//                        result += GetNodeBoolean(cp, "Email", CS.GetBoolean("Email"));
//                        result += GetNodeBoolean(cp, "Admin", CS.GetBoolean("Admin"));
//                        result += GetNodeBoolean(cp, "OnPageEndEvent", CS.GetBoolean("OnPageEndEvent"));
//                        result += GetNodeBoolean(cp, "OnPageStartEvent", CS.GetBoolean("OnPageStartEvent"));
//                        result += GetNodeBoolean(cp, "OnBodyStart", CS.GetBoolean("OnBodyStart"));
//                        result += GetNodeBoolean(cp, "OnBodyEnd", CS.GetBoolean("OnBodyEnd"));
//                        result += GetNodeBoolean(cp, "RemoteMethod", CS.GetBoolean("RemoteMethod"));
//                        result += GetNodeBoolean(cp, "Diagnostic", ((cp.Version.CompareTo("5.01.00007101") >= 0) ? CS.GetBoolean("Diagnostic") : false));
//                        result += (cp.Version.CompareTo("5.01.00007101") >= 0) ? GetNodeBoolean(cp, "Diagnostic", CS.GetBoolean("Diagnostic")) : "";
//                    }
//                    //
//                    // -- Process
//                    result += GetNodeBoolean(cp, "ProcessRunOnce", processRunOnce);
//                    result += GetNodeInteger(cp, "ProcessInterval", CS.GetInteger("ProcessInterval"));
//                    //
//                    // Meta
//                    //
//                    result += GetNodeText(cp, "MetaDescription", CS.GetText("MetaDescription"));
//                    result += GetNodeText(cp, "OtherHeadTags", CS.GetText("OtherHeadTags"));
//                    result += GetNodeText(cp, "PageTitle", CS.GetText("PageTitle"));
//                    result += GetNodeText(cp, "RemoteAssetLink", CS.GetText("RemoteAssetLink"));
//                    //
//                    // Styles
//                    Styles = "";
//                    if (!CS.GetBoolean("BlockDefaultStyles")) {
//                        Styles = Convert.ToString(CS.GetText("StylesFilename")).Trim(' ');
//                    }
//                    StylesTest = Convert.ToString(CS.GetText("CustomStylesFilename")).Trim(' ');
//                    if (!string.IsNullOrEmpty(StylesTest)) {
//                        if (!string.IsNullOrEmpty(Styles)) {
//                            Styles = Styles + Environment.NewLine + StylesTest;
//                        } else {
//                            Styles = StylesTest;
//                        }
//                    }
//                    result += GetNodeText(cp, "Styles", Styles);
//                    result += GetNodeText(cp, "styleslinkhref", CS.GetText("styleslinkhref"));
//                    {
//                        //
//                        // Scripting
//                        //
//                        string NodeInnerText = Convert.ToString(CS.GetText("ScriptingCode")).Trim(' ');
//                        if (!string.IsNullOrEmpty(NodeInnerText)) {
//                            NodeInnerText = Environment.NewLine + "\t" + "\t" + "<Code>" + EncodeCData(cp, NodeInnerText) + "</Code>";
//                        }
//                        CS2.Open("Add-on Scripting Module Rules", "addonid=" + addonid);
//                        while (CS2.OK()) {
//                            ScriptingModuleId = CS2.GetInteger("ScriptingModuleID");
//                            CS3.Open("Scripting Modules", "ID=" + ScriptingModuleId);
//                            if (CS3.OK()) {
//                                Guid = CS3.GetText("ccGuid");
//                                if (string.IsNullOrEmpty(Guid)) {
//                                    Guid = cp.Utils.CreateGuid();
//                                    CS3.SetField("ccGuid", Guid);
//                                }
//                                Return_IncludeModuleGuidList = Return_IncludeModuleGuidList + Environment.NewLine + Guid;
//                                NodeInnerText = NodeInnerText + Environment.NewLine + "\t" + "\t" + "<IncludeModule name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
//                            }
//                            CS3.Close();
//                            CS2.GoNext();
//                        }
//                        CS2.Close();
//                        if (string.IsNullOrEmpty(NodeInnerText)) {
//                            result += Environment.NewLine + "\t" + "<Scripting Language=\"" + CS.GetText("ScriptingLanguageID") + "\" EntryPoint=\"" + CS.GetText("ScriptingEntryPoint") + "\" Timeout=\"" + CS.GetText("ScriptingTimeout") + "\"/>";
//                        } else {
//                            result += Environment.NewLine + "\t" + "<Scripting Language=\"" + CS.GetText("ScriptingLanguageID") + "\" EntryPoint=\"" + CS.GetText("ScriptingEntryPoint") + "\" Timeout=\"" + CS.GetText("ScriptingTimeout") + "\">" + NodeInnerText + Environment.NewLine + "\t" + "</Scripting>";
//                        }

//                    }
//                    //
//                    // Shared Styles
//                    //
//                    CS2.Open("Shared Styles Add-on Rules", "addonid=" + addonid);
//                    while (CS2.OK()) {
//                        styleId = CS2.GetInteger("styleId");
//                        CS3.Open("shared styles", "ID=" + styleId);
//                        if (CS3.OK()) {
//                            Guid = CS3.GetText("ccGuid");
//                            if (string.IsNullOrEmpty(Guid)) {
//                                Guid = cp.Utils.CreateGuid();
//                                CS3.SetField("ccGuid", Guid);
//                            }
//                            Return_IncludeSharedStyleGuidList = Return_IncludeSharedStyleGuidList + Environment.NewLine + Guid;
//                            result += Environment.NewLine + "\t" + "<IncludeSharedStyle name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
//                        }
//                        CS3.Close();
//                        CS2.GoNext();
//                    }
//                    CS2.Close();
//                    {
//                        //
//                        // Process Triggers
//                        //
//                        string NodeInnerText = "";
//                        CS2.Open("Add-on Content Trigger Rules", "addonid=" + addonid);
//                        while (CS2.OK()) {
//                            TriggerContentId = CS2.GetInteger("ContentID");
//                            CS3.Open("content", "ID=" + TriggerContentId);
//                            if (CS3.OK()) {
//                                Guid = CS3.GetText("ccGuid");
//                                if (string.IsNullOrEmpty(Guid)) {
//                                    Guid = cp.Utils.CreateGuid();
//                                    CS3.SetField("ccGuid", Guid);
//                                }
//                                NodeInnerText = NodeInnerText + Environment.NewLine + "\t" + "\t" + "<ContentChange name=\"" + System.Net.WebUtility.HtmlEncode(CS3.GetText("name")) + "\" guid=\"" + Guid + "\"/>";
//                            }
//                            CS3.Close();
//                            CS2.GoNext();
//                        }
//                        CS2.Close();
//                        if (!string.IsNullOrEmpty(NodeInnerText)) {
//                            result += Environment.NewLine + "\t" + "<ProcessTriggers>" + NodeInnerText + Environment.NewLine + "\t" + "</ProcessTriggers>";
//                        }

//                    }
//                    {
//                        //
//                        // Editors
//                        //
//                        if (cp.Content.IsField("Add-on Content Field Type Rules", "id")) {
//                            string NodeInnerText = "";
//                            CS2.Open("Add-on Content Field Type Rules", "addonid=" + addonid);
//                            while (CS2.OK()) {
//                                fieldTypeId = CS2.GetInteger("contentFieldTypeID");
//                                fieldType = cp.Content.GetRecordName("Content Field Types", fieldTypeId);
//                                if (!string.IsNullOrEmpty(fieldType)) {
//                                    NodeInnerText = NodeInnerText + Environment.NewLine + "\t" + "\t" + "<type>" + fieldType + "</type>";
//                                }
//                                CS2.GoNext();
//                            }
//                            CS2.Close();
//                            if (!string.IsNullOrEmpty(NodeInnerText)) {
//                                result += Environment.NewLine + "\t" + "<Editors>" + NodeInnerText + Environment.NewLine + "\t" + "</Editors>";
//                            }
//                        }

//                    }
//                    {
//                        //
//                        // Category
//                        //
//                        string NodeInnerText = CS.GetText("addonCategoryId");
//                        if (!string.IsNullOrEmpty(NodeInnerText)) {
//                            result += Environment.NewLine + "\t" + "<Category>" + NodeInnerText + Environment.NewLine + "\t" + "</Category>";
//                        }

//                    }
//                    //
//                    // Guid
//                    //
//                    Guid = CS.GetText("ccGuid");
//                    if (string.IsNullOrEmpty(Guid)) {
//                        Guid = cp.Utils.CreateGuid();
//                        CS.SetField("ccGuid", Guid);
//                    }
//                    //
//                    // Navigator Type
//                    //
//                    NavType = CS.GetText("NavTypeID");
//                    if (string.IsNullOrEmpty(NavType)) {
//                        NavType = "Add-on";
//                    }
//                    //
//                    // create xml
//                    //
//                    result = ""
//                    + Environment.NewLine + "\t" + "<Addon name=\"" + System.Net.WebUtility.HtmlEncode(addonName) + "\" guid=\"" + Guid + "\" type=\"" + NavType + "\">"
//                    + tabIndent(cp, result) + Environment.NewLine + "\t" + "</Addon>";
//                }
//                CS.Close();
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "GetAddonNode");
//            }
//            return result;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// create a simple text node with a name and content
//        /// </summary>
//        /// <param name="NodeName"></param>
//        /// <param name="NodeContent"></param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        private static string GetNodeText(CPBaseClass cp, string NodeName, string NodeContent, bool deprecated = false) {
//            string tempGetNodeText = null;
//            tempGetNodeText = "";
//            try {
//                string prefix = "";
//                if (deprecated) {
//                    prefix = "<!-- deprecated -->";
//                }
//                tempGetNodeText = "";
//                if (string.IsNullOrEmpty(NodeContent)) {
//                    tempGetNodeText = tempGetNodeText + Environment.NewLine + "\t" + prefix + "<" + NodeName + "></" + NodeName + ">";
//                } else {
//                    tempGetNodeText = tempGetNodeText + Environment.NewLine + "\t" + prefix + "<" + NodeName + ">" + EncodeCData(cp, NodeContent) + "</" + NodeName + ">";
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "getNodeText");
//            }
//            return tempGetNodeText;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// create a simple boolean node with a name and content
//        /// </summary>
//        /// <param name="NodeName"></param>
//        /// <param name="NodeContent"></param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        private static string GetNodeBoolean(CPBaseClass cp, string NodeName, bool NodeContent) {
//            string tempGetNodeBoolean = null;
//            tempGetNodeBoolean = "";
//            try {
//                tempGetNodeBoolean = Environment.NewLine + "\t" + "<" + NodeName + ">" + kmaGetYesNo(cp, NodeContent) + "</" + NodeName + ">";
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "GetNodeBoolean");
//            }
//            return tempGetNodeBoolean;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// create a simple integer node with a name and content
//        /// </summary>
//        /// <param name="NodeName"></param>
//        /// <param name="NodeContent"></param>
//        /// <returns></returns>
//        /// <remarks></remarks>
//        private static string GetNodeInteger(CPBaseClass cp, string NodeName, int NodeContent) {
//            string tempGetNodeInteger = null;
//            tempGetNodeInteger = "";
//            try {
//                tempGetNodeInteger = Environment.NewLine + "\t" + "<" + NodeName + ">" + NodeContent.ToString() + "</" + NodeName + ">";
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "GetNodeInteger");
//            }
//            return tempGetNodeInteger;
//        }
//        //
//        //====================================================================================================
//        public static string replaceMany(CPBaseClass cp, string Source, string[] ArrayOfSource, string[] ArrayOfReplacement) {
//            string tempreplaceMany = null;
//            tempreplaceMany = "";
//            try {
//                int Count = ArrayOfSource.GetUpperBound(0) + 1;
//                tempreplaceMany = Source;
//                for (var Pointer = 0; Pointer < Count; Pointer++) {
//                    tempreplaceMany = tempreplaceMany.Replace(ArrayOfSource[Pointer], ArrayOfReplacement[Pointer]);
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "replaceMany");
//            }
//            return tempreplaceMany;
//        }
//        //
//        //====================================================================================================
//        public static string encodeFilename(CPBaseClass cp, string Filename) {
//            string tempencodeFilename = null;
//            tempencodeFilename = "";
//            try {
//                string[] Source = null;
//                string[] Replacement = null;
//                //
//                Source = new[] { "\"", "*", "/", ":", "<", ">", "?", "\\", "|" };
//                Replacement = new[] { "_", "_", "_", "_", "_", "_", "_", "_", "_" };
//                //
//                tempencodeFilename = replaceMany(cp, Filename, Source, Replacement);
//                if (tempencodeFilename.Length > 254) {
//                    tempencodeFilename = tempencodeFilename.Substring(0, 254);
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "encodeFilename");
//            }
//            return tempencodeFilename;
//        }
//        //
//        //====================================================================================================
//        /// <summary>
//        /// load the addon\collections.xml file
//        /// </summary>
//        /// <param name="cp"></param>
//        /// <param name="CollectionGuid"></param>
//        /// <param name="return_CollectionPath"></param>
//        /// <param name="return_LastChangedate"></param>
//        internal static void GetLocalCollectionArgs(CPBaseClass cp, string CollectionGuid, ref string return_CollectionPath, ref DateTime return_LastChangedate) {
//            try {
//                const string CollectionListRootNode = "collectionlist";
//                return_CollectionPath = "";
//                return_LastChangedate = DateTime.MinValue;
//                System.Xml.XmlDocument Doc = new System.Xml.XmlDocument();
//                Doc.LoadXml(cp.PrivateFiles.Read("addons\\Collections.xml"));
//                if (Doc.DocumentElement.Name.ToLower(CultureInfo.InvariantCulture) != CollectionListRootNode.ToLower(CultureInfo.InvariantCulture)) {
//                } else {
//                    if (Doc.DocumentElement.Name.ToLower(CultureInfo.InvariantCulture) != "collectionlist") {
//                    } else {
//                        foreach (System.Xml.XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
//                            string LocalName = "no name found";
//                            string LocalPath = string.Empty;
//                            LocalPath = "";
//                            string LocalGuid = "";
//                            string CollectionPath = "";
//                            DateTime LastChangeDate = default(DateTime);
//                            switch (LocalListNode.Name.ToLower(CultureInfo.InvariantCulture)) {
//                                case "collection":
//                                LocalGuid = "";
//                                foreach (System.Xml.XmlNode CollectionNode in LocalListNode.ChildNodes) {
//                                    switch (CollectionNode.Name.ToLower(CultureInfo.InvariantCulture)) {
//                                        case "name":
//                                        //
//                                        LocalName = CollectionNode.InnerText.ToLower(CultureInfo.InvariantCulture);
//                                        break;
//                                        case "guid":
//                                        //
//                                        LocalGuid = CollectionNode.InnerText.ToLower(CultureInfo.InvariantCulture);
//                                        break;
//                                        case "path":
//                                        //
//                                        CollectionPath = CollectionNode.InnerText.ToLower(CultureInfo.InvariantCulture);
//                                        break;
//                                        case "lastchangedate":
//                                        LastChangeDate = cp.Utils.EncodeDate(CollectionNode.InnerText);
//                                        break;
//                                    }
//                                }
//                                break;
//                            }
//                            if (CollectionGuid.ToLower(CultureInfo.InvariantCulture) == LocalGuid) {
//                                return_CollectionPath = CollectionPath;
//                                return_LastChangedate = LastChangeDate;
//                                break;
//                            }
//                        }
//                    }
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "GetLocalCollectionArgs");
//            }
//        }
//        //
//        //====================================================================================================
//        internal static string EncodeCData(CPBaseClass cp, string Source) {
//            string tempEncodeCData = null;
//            tempEncodeCData = "";
//            try {
//                tempEncodeCData = Source;
//                if (!string.IsNullOrEmpty(tempEncodeCData)) {
//                    tempEncodeCData = "<![CDATA[" + tempEncodeCData.Replace("]]>", "]]]]><![CDATA[>") + "]]>";
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "EncodeCData");
//            }
//            return tempEncodeCData;
//        }
//        //
//        //====================================================================================================
//        public static string kmaGetYesNo(CPBaseClass cp, bool Key) {
//            string tempkmaGetYesNo = null;
//            if (Key) {
//                tempkmaGetYesNo = "Yes";
//            } else {
//                tempkmaGetYesNo = "No";
//            }
//            return tempkmaGetYesNo;
//        }
//        //
//        //=======================================================================================
//        /// <summary>
//        /// zip
//        /// </summary>
//        /// <param name="PathFilename"></param>
//        /// <remarks></remarks>
//        public static void unzipFile(CPBaseClass cp, string PathFilename) {
//            try {
//                //
//                ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
//                string fileFilter = null;

//                fastZip.ExtractZip(PathFilename, getPath(cp, PathFilename), fileFilter);
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "UnzipFile");
//            }
//        }
//        //
//        //=======================================================================================
//        /// <summary>
//        /// unzip
//        /// </summary>
//        /// <param name="zipTempPathFilename"></param>
//        /// <param name="addTempPathFilename"></param>
//        /// <remarks></remarks>
//        public static void zipTempCdnFile(CPBaseClass cp, string zipTempPathFilename, List<string> addTempPathFilename) {
//            try {
//                ICSharpCode.SharpZipLib.Zip.ZipFile z = null;
//                if (cp.TempFiles.FileExists(zipTempPathFilename)) {
//                    //
//                    // update existing zip with list of files
//                    z = new ICSharpCode.SharpZipLib.Zip.ZipFile(cp.TempFiles.PhysicalFilePath + zipTempPathFilename);
//                } else {
//                    //
//                    // create new zip
//                    z = ICSharpCode.SharpZipLib.Zip.ZipFile.Create(cp.TempFiles.PhysicalFilePath + zipTempPathFilename);
//                }
//                z.BeginUpdate();
//                foreach (var pathFilename in addTempPathFilename) {
//                    z.Add(cp.TempFiles.PhysicalFilePath + pathFilename, System.IO.Path.GetFileName(pathFilename));
//                }
//                z.CommitUpdate();
//                z.Close();
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "zipFile");
//            }
//        }
//        //
//        //=======================================================================================
//        private static string getPath(CPBaseClass cp, string pathFilename) {
//            string tempgetPath = null;
//            tempgetPath = "";
//            try {
//                int Position;
//                //
//                Position = pathFilename.LastIndexOf("\\") + 1;
//                if (Position != 0) {
//                    tempgetPath = pathFilename.Substring(0, Position);
//                }
//            } catch (Exception ex) {
//                cp.Site.ErrorReport(ex, "getPath");
//            }
//            return tempgetPath;
//        }
//        //
//        //=======================================================================================
//        public static string getFilename(CPBaseClass cp, string PathFilename) {
//            string tempGetFilename = null;
//            int Position = 0;
//            //
//            tempGetFilename = PathFilename;
//            Position = tempGetFilename.LastIndexOf("/") + 1;
//            if (Position != 0) {
//                tempGetFilename = tempGetFilename.Substring(Position);
//            }
//            return tempGetFilename;
//        }
//        //
//        //=======================================================================================
//        //
//        //   Indent every line by 1 tab
//        //
//        public static string tabIndent(CPBaseClass cp, string Source) {
//            string temptabIndent = null;
//            int posStart = 0;
//            int posEnd = 0;
//            string pre = null;
//            string post = null;
//            string target = null;
//            //
//            posStart = Source.IndexOf("<![CDATA[", System.StringComparison.OrdinalIgnoreCase) + 1;
//            if (posStart == 0) {
//                //
//                // no cdata
//                //
//                posStart = Source.IndexOf("<textarea", System.StringComparison.OrdinalIgnoreCase) + 1;
//                if (posStart == 0) {
//                    //
//                    // no textarea
//                    //
//                    temptabIndent = Source.Replace(Environment.NewLine + "\t", Environment.NewLine + "\t" + "\t");
//                } else {
//                    //
//                    // text area found, isolate it and indent before and after
//                    //
//                    posEnd = Source.IndexOf("</textarea>", posStart - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
//                    pre = Source.Substring(0, posStart - 1);
//                    if (posEnd == 0) {
//                        target = Source.Substring(posStart - 1);
//                        post = "";
//                    } else {
//                        target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("</textarea>")).Length);
//                        post = Source.Substring((posEnd + ((string)("</textarea>")).Length) - 1);
//                    }
//                    temptabIndent = tabIndent(cp, pre) + target + tabIndent(cp, post);
//                }
//            } else {
//                //
//                // cdata found, isolate it and indent before and after
//                //
//                posEnd = Source.IndexOf("]]>", posStart - 1, System.StringComparison.OrdinalIgnoreCase) + 1;
//                pre = Source.Substring(0, posStart - 1);
//                if (posEnd == 0) {
//                    target = Source.Substring(posStart - 1);
//                    post = "";
//                } else {
//                    target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("]]>")).Length);
//                    post = Source.Substring(posEnd + 2);
//                }
//                temptabIndent = tabIndent(cp, pre) + target + tabIndent(cp, post);
//            }
//            return temptabIndent;
//        }

//    }
//}
