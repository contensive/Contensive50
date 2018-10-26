
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
using static Contensive.Processor.constants;
using Contensive.Processor.Models.Domain;
using Contensive.Addons.Tools;
using static Contensive.Processor.AdminUIController;
//
namespace Contensive.Addons.AdminSite {
    public class IndexConfigClass {
        public bool Loaded;
        public int ContentID;
        public int PageNumber;
        public int RecordsPerPage;
        public int RecordTop;
        public Dictionary<string, IndexConfigFindWordClass> FindWords = new Dictionary<string, IndexConfigFindWordClass>();
        public bool ActiveOnly;
        public bool LastEditedByMe;
        public bool LastEditedToday;
        public bool LastEditedPast7Days;
        public bool LastEditedPast30Days;
        public bool Open;
        public Dictionary<string, IndexConfigSortClass> Sorts = new Dictionary<string, IndexConfigSortClass>();
        public int GroupListCnt;
        public string[] GroupList;
        public List<IndexConfigColumnClass> columns = new List<IndexConfigColumnClass>() { };
        public int SubCDefID;
        //
        //=================================================================================
        //   Load the index configig
        //       if it is empty, setup defaults
        //=================================================================================
        //
        public static IndexConfigClass get(CoreController core, AdminDataModel adminData) {
            IndexConfigClass returnIndexConfig = new IndexConfigClass();
            try {
                // refactor this out
                CDefModel content = adminData.adminContent;
                //
                // Setup defaults
                returnIndexConfig.ContentID = adminData.adminContent.id;
                returnIndexConfig.ActiveOnly = false;
                returnIndexConfig.LastEditedByMe = false;
                returnIndexConfig.LastEditedToday = false;
                returnIndexConfig.LastEditedPast7Days = false;
                returnIndexConfig.LastEditedPast30Days = false;
                returnIndexConfig.Loaded = true;
                returnIndexConfig.Open = false;
                returnIndexConfig.PageNumber = 1;
                returnIndexConfig.RecordsPerPage = Constants.RecordsPerPageDefault;
                returnIndexConfig.RecordTop = 0;
                //
                // Setup Member Properties
                //
                string ConfigList = core.userProperty.getText(AdminDataModel.IndexConfigPrefix + encodeText(adminData.adminContent.id), "");
                if (!string.IsNullOrEmpty(ConfigList)) {
                    //
                    // load values
                    //
                    ConfigList = ConfigList + "\r\n";
                    string[] ConfigListLines = GenericController.splitNewLine(ConfigList);
                    int Ptr = 0;
                    while (Ptr < ConfigListLines.GetUpperBound(0)) {
                        //
                        // check next line
                        //
                        string ConfigListLine = GenericController.vbLCase(ConfigListLines[Ptr]);
                        if (!string.IsNullOrEmpty(ConfigListLine)) {
                            switch (ConfigListLine) {
                                case "columns":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        string Line = ConfigListLines[Ptr];
                                        string[] LineSplit = Line.Split('\t');
                                        if (LineSplit.GetUpperBound(0) > 0) {
                                            string fieldName = LineSplit[0].Trim().ToLowerInvariant();
                                            if (!string.IsNullOrWhiteSpace(fieldName)) {
                                                if (adminData.adminContent.fields.ContainsKey(fieldName)) {
                                                    returnIndexConfig.columns.Add(new IndexConfigColumnClass() {
                                                        Name = fieldName,
                                                        Width = GenericController.encodeInteger(LineSplit[1]),
                                                        SortDirection = 0,
                                                        SortPriority = 0
                                                    });
                                                }
                                            }
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "sorts":
                                    Ptr = Ptr + 1;
                                    int orderPtr = 0;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        string[] LineSplit = ConfigListLines[Ptr].Split('\t');
                                        if (LineSplit.GetUpperBound(0) == 1) {
                                            string fieldName = LineSplit[0].Trim().ToLowerInvariant();
                                            if (!string.IsNullOrWhiteSpace(fieldName)) {
                                                returnIndexConfig.Sorts.Add(fieldName, new IndexConfigSortClass {
                                                    fieldName = fieldName,
                                                    direction = ((LineSplit[1] == "1") ? 1 : 2),
                                                    order = ++orderPtr
                                                });
                                                //returnIndexConfig.Sorts.Add(fieldName, new indexConfigSortClass {
                                                //    fieldName = fieldName,
                                                //    direction = (genericController.encodeBoolean(LineSplit[1]) ? 1 : 2),
                                                //    order = ++orderPtr
                                                //});
                                            }
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                            }
                        }
                        Ptr = Ptr + 1;
                    }
                    if (returnIndexConfig.RecordsPerPage <= 0) {
                        returnIndexConfig.RecordsPerPage = Constants.RecordsPerPageDefault;
                    }
                    //.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                }
                //
                // Setup Visit Properties
                //
                ConfigList = core.visitProperty.getText(AdminDataModel.IndexConfigPrefix + encodeText(adminData.adminContent.id), "");
                if (!string.IsNullOrEmpty(ConfigList)) {
                    //
                    // load values
                    //
                    ConfigList = ConfigList + "\r\n";
                    string[] ConfigListLines = GenericController.splitNewLine(ConfigList);
                    int Ptr = 0;
                    while (Ptr < ConfigListLines.GetUpperBound(0)) {
                        //
                        // check next line
                        //
                        string ConfigListLine = GenericController.vbLCase(ConfigListLines[Ptr]);
                        if (!string.IsNullOrEmpty(ConfigListLine)) {
                            switch (ConfigListLine) {
                                case "findwordlist":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        //ReDim Preserve .FindWords(.FindWords.Count)
                                        string Line = ConfigListLines[Ptr];
                                        string[] LineSplit = Line.Split('\t');
                                        if (LineSplit.GetUpperBound(0) > 1) {
                                            returnIndexConfig.FindWords.Add(LineSplit[0], new IndexConfigFindWordClass {
                                                Name = LineSplit[0],
                                                Value = LineSplit[1],
                                                MatchOption = (FindWordMatchEnum)GenericController.encodeInteger(LineSplit[2])
                                            });
                                        }
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "grouplist":
                                    Ptr = Ptr + 1;
                                    while (!string.IsNullOrEmpty(ConfigListLines[Ptr])) {
                                        Array.Resize(ref returnIndexConfig.GroupList, returnIndexConfig.GroupListCnt + 1);
                                        returnIndexConfig.GroupList[returnIndexConfig.GroupListCnt] = ConfigListLines[Ptr];
                                        returnIndexConfig.GroupListCnt = returnIndexConfig.GroupListCnt + 1;
                                        Ptr = Ptr + 1;
                                    }
                                    break;
                                case "cdeflist":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.SubCDefID = GenericController.encodeInteger(ConfigListLines[Ptr]);
                                    break;
                                case "indexfiltercategoryid":
                                    // -- remove deprecated value
                                    Ptr = Ptr + 1;
                                    int ignore = GenericController.encodeInteger(ConfigListLines[Ptr]);
                                    break;
                                case "indexfilteractiveonly":
                                    returnIndexConfig.ActiveOnly = true;
                                    break;
                                case "indexfilterlasteditedbyme":
                                    returnIndexConfig.LastEditedByMe = true;
                                    break;
                                case "indexfilterlasteditedtoday":
                                    returnIndexConfig.LastEditedToday = true;
                                    break;
                                case "indexfilterlasteditedpast7days":
                                    returnIndexConfig.LastEditedPast7Days = true;
                                    break;
                                case "indexfilterlasteditedpast30days":
                                    returnIndexConfig.LastEditedPast30Days = true;
                                    break;
                                case "indexfilteropen":
                                    returnIndexConfig.Open = true;
                                    break;
                                case "recordsperpage":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.RecordsPerPage = GenericController.encodeInteger(ConfigListLines[Ptr]);
                                    if (returnIndexConfig.RecordsPerPage <= 0) {
                                        returnIndexConfig.RecordsPerPage = 50;
                                    }
                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) * returnIndexConfig.RecordsPerPage);
                                    break;
                                case "pagenumber":
                                    Ptr = Ptr + 1;
                                    returnIndexConfig.PageNumber = GenericController.encodeInteger(ConfigListLines[Ptr]);
                                    if (returnIndexConfig.PageNumber <= 0) {
                                        returnIndexConfig.PageNumber = 1;
                                    }
                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) * returnIndexConfig.RecordsPerPage);
                                    break;
                            }
                        }
                        Ptr = Ptr + 1;
                    }
                    if (returnIndexConfig.RecordsPerPage <= 0) {
                        returnIndexConfig.RecordsPerPage = Constants.RecordsPerPageDefault;
                    }
                    //.PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                }
                //
                // Setup defaults if not loaded
                //
                if ((returnIndexConfig.columns.Count == 0) && (adminData.adminContent.adminColumns.Count > 0)) {
                    foreach (var keyValuePair in adminData.adminContent.adminColumns) {
                        returnIndexConfig.columns.Add(new IndexConfigColumnClass {
                            Name = keyValuePair.Value.Name,
                            Width = keyValuePair.Value.Width
                        });
                    }
                }
                //
                // Set field pointers for columns and sorts
                //
                // dont knwo what this does
                //For Each keyValuePair As KeyValuePair(Of String, appServices_metaDataClass.CDefFieldClass) In adminContext.content.fields
                //    Dim field As appServices_metaDataClass.CDefFieldClass = keyValuePair.Value
                //    If .Columns.Count > 0 Then
                //        For Ptr = 0 To .Columns.Count - 1
                //            With .Columns[Ptr]
                //                If genericController.vbLCase(.Name) = field.Name.ToLowerInvariant() Then
                //                    .FieldId = SrcPtr
                //                    Exit For
                //                End If
                //            End With
                //        Next
                //    End If
                //    '
                //    If .SortCnt > 0 Then
                //        For Ptr = 0 To .SortCnt - 1
                //            With .Sorts[Ptr]
                //                If genericController.vbLCase(.FieldName) = field.Name Then
                //                    .FieldPtr = SrcPtr
                //                    Exit For
                //                End If
                //            End With
                //        Next
                //    End If
                //Next
                //        '
                //        ' set Column Field Ptr for later
                //        '
                //        If .columns.count > 0 Then
                //            For Ptr = 0 To .columns.count - 1
                //                With .Columns[Ptr]
                //                    For SrcPtr = 0 To adminContext.content.fields.count - 1
                //                        If .Name = adminContext.content.fields[SrcPtr].Name Then
                //                            .FieldPointer = SrcPtr
                //                            Exit For
                //                        End If
                //                    Next
                //                End With
                //            Next
                //        End If
                //        '
                //        ' set Sort Field Ptr for later
                //        '
                //        If .SortCnt > 0 Then
                //            For Ptr = 0 To .SortCnt - 1
                //                With .Sorts[Ptr]
                //                    For SrcPtr = 0 To adminContext.content.fields.count - 1
                //                        If genericController.vbLCase(.FieldName) = genericController.vbLCase(adminContext.content.fields[SrcPtr].Name) Then
                //                            .FieldPtr = SrcPtr
                //                            Exit For
                //                        End If
                //                    Next
                //                End With
                //            Next
                //        End If
            } catch (Exception ex) {
                LogController.handleError(core, ex);
                throw;
            }
            return returnIndexConfig;
        }
        //

        //
        //
        public class IndexConfigSortClass {
            //Dim FieldPtr As Integer
            public string fieldName;
            public int direction; // 1=forward, 2=reverse, 0=ignore/remove this sort
            public int order; // 1...n, if multiple sorts, the order of the sort
        }
        //
        public class IndexConfigFindWordClass {
            public string Name;
            public string Value;
            public int Type;
            public FindWordMatchEnum MatchOption;
        }
        //
        public class IndexConfigColumnClass {
            public string Name;
            //Public FieldId As Integer
            public int Width;
            public int SortPriority;
            public int SortDirection;
        }
    }
}
