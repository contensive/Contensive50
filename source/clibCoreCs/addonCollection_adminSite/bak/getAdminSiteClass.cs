

using Controllers;

using System.Xml;
using Contensive.Core;
using Models.Entity;
// 

namespace Contensive.Addons.AdminSite {
    
    public class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass {
        
        // 
        // =================================================================================
        //    Load the index configig
        //        if it is empty, setup defaults
        // =================================================================================
        // 
        private indexConfigClass LoadIndexConfig(Models.Complex.cdefModel adminContent) {
            indexConfigClass returnIndexConfig = new indexConfigClass();
            try {
                // 
                string ConfigListLine;
                string Line;
                int Ptr;
                string ConfigList;
                string[] ConfigListLines;
                string[] LineSplit;
                // 
                // With...
                // 
                //  Setup defaults
                // 
                RecordsPerPageDefault.RecordTop = 0;
                1.RecordsPerPage = 0;
                false.PageNumber = 0;
                true.Open = 0;
                false.Loaded = 0;
                false.LastEditedPast30Days = 0;
                false.LastEditedPast7Days = 0;
                false.LastEditedToday = 0;
                false.LastEditedByMe = 0;
                adminContent.Id.ActiveOnly = 0;
                returnIndexConfig.ContentID = 0;
                // 
                //  Setup Member Properties
                // 
                ConfigList = cpCore.userProperty.getText((IndexConfigPrefix + adminContent.Id.ToString()), "");
                if ((ConfigList != "")) {
                    // 
                    //  load values
                    // 
                    ConfigList = (ConfigList + "\r\n");
                    ConfigListLines = genericController.SplitCRLF(ConfigList);
                    Ptr = 0;
                    while ((Ptr < UBound(ConfigListLines))) {
                        // 
                        //  check next line
                        // 
                        ConfigListLine = genericController.vbLCase(ConfigListLines[Ptr]);
                        if ((ConfigListLine != "")) {
                            switch (ConfigListLine) {
                                case "columns":
                                    Ptr = (Ptr + 1);
                                    while ((ConfigListLines[Ptr] != "")) {
                                        Line = ConfigListLines[Ptr];
                                        LineSplit = Line.Split('\t', 2);
                                        if ((UBound(LineSplit) > 0)) {
                                            indexConfigColumnClass column = new indexConfigColumnClass();
                                            column.Name = LineSplit[0].Trim();
                                            column.Width = genericController.EncodeInteger(LineSplit[1]).Columns.Add(column.Name.ToLower(), column);
                                        }
                                        
                                        Ptr = (Ptr + 1);
                                    }
                                    
                                    break;
                                case "sorts":
                                    Ptr = (Ptr + 1);
                                    while ((ConfigListLines[Ptr] != "")) {
                                        Line = ConfigListLines[Ptr];
                                        LineSplit = Line.Split('\t', 2);
                                        if ((UBound(LineSplit) == 1)) {
                                            returnIndexConfig.Sorts.Add;
                                            LineSplit[0].ToLower;
                                            new indexConfigSortClass();
                                            // With...
                                            fieldName = LineSplit[0];
                                            genericController.EncodeBoolean(LineSplit[1]);
                                            1;
                                            2;
                                            Ptr = (Ptr + 1);
                                        }
                                        
                                        if ((Ptr 
                                                    == (Ptr + 1))) {
                                            if ((returnIndexConfig.RecordsPerPage <= 0)) {
                                                returnIndexConfig.RecordsPerPage = RecordsPerPageDefault;
                                            }
                                            
                                            // .PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                                        }
                                        
                                        // 
                                        //  Setup Visit Properties
                                        // 
                                        ConfigList = cpCore.visitProperty.getText((IndexConfigPrefix + adminContent.Id.ToString()), "");
                                        if ((ConfigList != "")) {
                                            // 
                                            //  load values
                                            // 
                                            ConfigList = (ConfigList + "\r\n");
                                            ConfigListLines = genericController.SplitCRLF(ConfigList);
                                            Ptr = 0;
                                            while ((Ptr < UBound(ConfigListLines))) {
                                                // 
                                                //  check next line
                                                // 
                                                ConfigListLine = genericController.vbLCase(ConfigListLines[Ptr]);
                                                if ((ConfigListLine != "")) {
                                                    switch (ConfigListLine) {
                                                        case "findwordlist":
                                                            Ptr = (Ptr + 1);
                                                            while ((ConfigListLines[Ptr] != "")) {
                                                                Line = ConfigListLines[Ptr];
                                                                LineSplit = Line.Split('\t');
                                                                if ((UBound(LineSplit) > 1)) {
                                                                    indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                                                    findWord.Name = LineSplit[0];
                                                                    findWord.Value = LineSplit[1];
                                                                    genericController.EncodeInteger(LineSplit[2]);
                                                                    FindWordMatchEnum;
                                                                    returnIndexConfig.FindWords.Add;
                                                                    findWord.Name;
                                                                    findWord;
                                                                }
                                                                
                                                                Ptr = (Ptr + 1);
                                                            }
                                                            
                                                            break;
                                                        case "grouplist":
                                                            Ptr = (Ptr + 1);
                                                            while ((ConfigListLines[Ptr] != "")) {
                                                                object Preserve.GroupList;
                                                                returnIndexConfig.GroupList;
                                                                ConfigListLines[Ptr].GroupListCnt = (returnIndexConfig.GroupListCnt + 1);
                                                                returnIndexConfig.GroupListCnt = (returnIndexConfig.GroupListCnt + 1);
                                                                Ptr = (Ptr + 1);
                                                            }
                                                            
                                                            break;
                                                        case "cdeflist":
                                                            (Ptr + 1.SubCDefID) = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                            Ptr = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                            break;
                                                        case "indexfiltercategoryid":
                                                            Ptr = (Ptr + 1);
                                                            int ignore = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                            break;
                                                    }
                                                    "indexfilterlasteditedbyme".LastEditedByMe = true;
                                                    "indexfilterlasteditedtoday".LastEditedToday = true;
                                                    "indexfilterlasteditedpast7days".LastEditedPast7Days = true;
                                                    "indexfilterlasteditedpast30days".LastEditedPast30Days = true;
                                                    "indexfilteropen".Open = true;
                                                    "recordsperpage";
                                                    (Ptr + 1.RecordsPerPage) = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                    Ptr = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                    if ((returnIndexConfig.RecordsPerPage <= 0)) {
                                                        returnIndexConfig.RecordsPerPage = 50;
                                                    }
                                                    
                                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) 
                                                                * returnIndexConfig.RecordsPerPage);
                                                    "pagenumber";
                                                    (Ptr + 1.PageNumber) = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                    Ptr = genericController.EncodeInteger(ConfigListLines[Ptr]);
                                                    if ((returnIndexConfig.PageNumber <= 0)) {
                                                        returnIndexConfig.PageNumber = 1;
                                                    }
                                                    
                                                    returnIndexConfig.RecordTop = ((returnIndexConfig.PageNumber - 1) 
                                                                * returnIndexConfig.RecordsPerPage);
                                                }
                                                
                                                if ((Ptr 
                                                            == (Ptr + 1))) {
                                                    if ((returnIndexConfig.RecordsPerPage <= 0)) {
                                                        returnIndexConfig.RecordsPerPage = RecordsPerPageDefault;
                                                    }
                                                    
                                                    // .PageNumber = 1 + Int(.RecordTop / .RecordsPerPage)
                                                }
                                                
                                                // 
                                                //  Setup defaults if not loaded
                                                // 
                                                if (((returnIndexConfig.Columns.Count == 0) 
                                                            && (adminContent.adminColumns.Count > 0))) {
                                                    // .Columns.Count = adminContent.adminColumns.Count
                                                    // ReDim .Columns(.Columns.Count - 1)
                                                    // Ptr = 0
                                                    foreach (keyValuepair in adminContent.adminColumns) {
                                                        Models.Complex.cdefModel.CDefAdminColumnClass cdefAdminColumn = keyValuepair.Value;
                                                        indexConfigColumnClass column = new indexConfigColumnClass();
                                                        column.Name = cdefAdminColumn.Name;
                                                        column.Width = cdefAdminColumn.Width;
                                                        returnIndexConfig.Columns.Add(column.Name.ToLower(), column);
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                // With...
                                                ((Exception)(ex));
                                                cpCore.handleException(ex);
                                                throw;
                                                return returnIndexConfig;
                                                // 
                                                // ========================================================================================
                                                //    Process request input on the IndexConfig
                                                // ========================================================================================
                                                // 
                                                SetIndexSQL_ProcessIndexConfigRequests(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)), ref ((indexConfigClass)(IndexConfig)));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 'Dim th as integer : th = profileLogAdminMethodEnter("ProcessIndexConfigRequests")
                                                // 
                                                int TestInteger;
                                                int MatchOption;
                                                int FindWordPtr;
                                                int FormFieldCnt;
                                                int FormFieldPtr;
                                                indexConfigFindWordClass[] ContentFields;
                                                string NumericOption;
                                                int fieldType;
                                                string FieldValue;
                                                string FieldName;
                                                int CS;
                                                string Criteria;
                                                string VarText;
                                                string FindName;
                                                string FindValue;
                                                int Ptr;
                                                int ColumnCnt;
                                                int ColumnPtr;
                                                string Button;
                                                // 'Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                                                // 
                                                // arrayOfFields = adminContent.fields
                                                // With...
                                                if (!IndexConfig.Loaded) {
                                                    IndexConfig = this.LoadIndexConfig(adminContent);
                                                }
                                                
                                                // 
                                                //  ----- Page number
                                                // 
                                                VarText = cpCore.docProperties.getText("rt");
                                                if ((VarText != "")) {
                                                    IndexConfig.RecordTop = genericController.EncodeInteger(VarText);
                                                }
                                                
                                                // 
                                                VarText = cpCore.docProperties.getText("RS");
                                                if ((VarText != "")) {
                                                    IndexConfig.RecordsPerPage = genericController.EncodeInteger(VarText);
                                                }
                                                
                                                if ((IndexConfig.RecordsPerPage <= 0)) {
                                                    IndexConfig.RecordsPerPage = RecordsPerPageDefault;
                                                }
                                                
                                                IndexConfig.PageNumber = int.Parse((1 + Int((IndexConfig.RecordTop / IndexConfig.RecordsPerPage))));
                                                TestInteger = cpCore.docProperties.getInteger("indexGoToPage");
                                                if ((TestInteger > 0)) {
                                                    TestInteger.RecordTop = ((IndexConfig.PageNumber - 1) 
                                                                * IndexConfig.RecordsPerPage);
                                                    IndexConfig.PageNumber = ((IndexConfig.PageNumber - 1) 
                                                                * IndexConfig.RecordsPerPage);
                                                }
                                                else {
                                                    // 
                                                    //  ----- Read filter changes and First/Next/Previous from form
                                                    // 
                                                    Button = cpCore.docProperties.getText(RequestNameButton);
                                                    if ((Button != "")) {
                                                        switch (AdminButton) {
                                                            case ButtonFirst:
                                                                // 
                                                                //  Force to first page
                                                                // 
                                                                1.RecordTop = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                IndexConfig.PageNumber = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                break;
                                                            case ButtonNext:
                                                                // 
                                                                //  Go to next page
                                                                // 
                                                                (IndexConfig.PageNumber + 1.RecordTop) = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                IndexConfig.PageNumber = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                break;
                                                            case ButtonPrevious:
                                                                // 
                                                                //  Go to previous page
                                                                // 
                                                                IndexConfig.PageNumber = (IndexConfig.PageNumber - 1);
                                                                if ((IndexConfig.PageNumber <= 0)) {
                                                                    IndexConfig.PageNumber = 1;
                                                                }
                                                                
                                                                IndexConfig.RecordTop = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                break;
                                                            case ButtonFind:
                                                                // 
                                                                //  Find (change search criteria and go to first page)
                                                                // 
                                                                1.RecordTop = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                IndexConfig.PageNumber = ((IndexConfig.PageNumber - 1) 
                                                                            * IndexConfig.RecordsPerPage);
                                                                ColumnCnt = cpCore.docProperties.getInteger("ColumnCnt");
                                                                if ((ColumnCnt > 0)) {
                                                                    for (ColumnPtr = 0; (ColumnPtr 
                                                                                <= (ColumnCnt - 1)); ColumnPtr++) {
                                                                        FindName = cpCore.docProperties.getText(("FindName" + ColumnPtr)).ToLower;
                                                                        if (!string.IsNullOrEmpty(FindName)) {
                                                                            if (adminContent.fields.ContainsKey(FindName.ToLower)) {
                                                                                FindValue = cpCore.docProperties.getText(("FindValue" + ColumnPtr)).Trim();
                                                                                if (string.IsNullOrEmpty(FindValue)) {
                                                                                    // 
                                                                                    //  -- find blank, if name in list, remove it
                                                                                    if (IndexConfig.FindWords.ContainsKey[FindName]) {
                                                                                        IndexConfig.FindWords.Remove;
                                                                                        FindName;
                                                                                    }
                                                                                    
                                                                                }
                                                                                else {
                                                                                    // 
                                                                                    //  -- nonblank find, store it
                                                                                    if (IndexConfig.FindWords.ContainsKey[FindName]) {
                                                                                        IndexConfig.FindWords.Item;
                                                                                        FindName.Value = FindValue;
                                                                                    }
                                                                                    else {
                                                                                        Models.Complex.CDefFieldModel field = adminContent.fields(FindName.ToLower);
                                                                                        indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                                                                        findWord.Name = FindName;
                                                                                        findWord.Value = FindValue;
                                                                                        switch (field.fieldTypeId) {
                                                                                            case FieldTypeIdAutoIdIncrement:
                                                                                            case FieldTypeIdCurrency:
                                                                                            case FieldTypeIdFloat:
                                                                                            case FieldTypeIdInteger:
                                                                                            case FieldTypeIdLookup:
                                                                                            case FieldTypeIdMemberSelect:
                                                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                                                break;
                                                                                            case FieldTypeIdDate:
                                                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                                                break;
                                                                                            case FieldTypeIdBoolean:
                                                                                                findWord.MatchOption = FindWordMatchEnum.MatchEquals;
                                                                                                break;
                                                                                            default:
                                                                                                findWord.MatchOption = FindWordMatchEnum.matchincludes;
                                                                                                break;
                                                                                        }
                                                                                        IndexConfig.FindWords.Add;
                                                                                        FindName;
                                                                                        findWord;
                                                                                    }
                                                                                    
                                                                                }
                                                                                
                                                                            }
                                                                            
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                }
                                                                
                                                                break;
                                                        }
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                //  Process Filter form
                                                // 
                                                if (cpCore.docProperties.getBoolean("IndexFilterRemoveAll")) {
                                                    // 
                                                    //  Remove all filters
                                                    // 
                                                    (// TODO: Warning!!!! NULL EXPRESSION DETECTED...
                                                     + // TODO: Warning!!!! NULL EXPRESSION DETECTED...
                                                    );
                                                    false.LastEditedPast7Days = false;
                                                    false.LastEditedToday = false;
                                                    false.LastEditedByMe = false;
                                                    0.ActiveOnly = false;
                                                    0.SubCDefID = false;
                                                    GroupListCnt = false;
                                                }
                                                else {
                                                    int VarInteger;
                                                    // 
                                                    //  Add CDef
                                                    // 
                                                    VarInteger = cpCore.docProperties.getInteger("IndexFilterAddCDef");
                                                    if ((VarInteger != 0)) {
                                                        VarInteger.PageNumber = 1;
                                                        IndexConfig.SubCDefID = 1;
                                                        //                 If .SubCDefCnt > 0 Then
                                                        //                     For Ptr = 0 To .SubCDefCnt - 1
                                                        //                         If VarInteger = .SubCDefs(Ptr) Then
                                                        //                             Exit For
                                                        //                         End If
                                                        //                     Next
                                                        //                 End If
                                                        //                 If Ptr = .SubCDefCnt Then
                                                        //                     ReDim Preserve .SubCDefs(.SubCDefCnt)
                                                        //                     .SubCDefs(.SubCDefCnt) = VarInteger
                                                        //                     .SubCDefCnt = .SubCDefCnt + 1
                                                        //                     .PageNumber = 1
                                                        //                 End If
                                                    }
                                                    
                                                    // 
                                                    //  Remove CDef
                                                    // 
                                                    VarInteger = cpCore.docProperties.getInteger("IndexFilterRemoveCDef");
                                                    if ((VarInteger != 0)) {
                                                        0.PageNumber = 1;
                                                        IndexConfig.SubCDefID = 1;
                                                        //                 If .SubCDefCnt > 0 Then
                                                        //                     For Ptr = 0 To .SubCDefCnt - 1
                                                        //                         If .SubCDefs(Ptr) = VarInteger Then
                                                        //                             .SubCDefs(Ptr) = 0
                                                        //                             .PageNumber = 1
                                                        //                             Exit For
                                                        //                         End If
                                                        //                     Next
                                                        //                 End If
                                                    }
                                                    
                                                    // 
                                                    //  Add Groups
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterAddGroup").ToLower();
                                                    if ((VarText != "")) {
                                                        if ((IndexConfig.GroupListCnt > 0)) {
                                                            for (Ptr = 0; (Ptr 
                                                                        <= (IndexConfig.GroupListCnt - 1)); Ptr++) {
                                                                if ((VarText == IndexConfig.GroupList)) {
                                                                    Ptr;
                                                                    break;
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((Ptr == IndexConfig.GroupListCnt)) {
                                                            object Preserve.GroupList;
                                                            IndexConfig.GroupList;
                                                            (IndexConfig.GroupListCnt + 1.PageNumber) = 1;
                                                            VarText.GroupListCnt = 1;
                                                            IndexConfig.GroupListCnt = 1;
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Remove Groups
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterRemoveGroup").ToLower();
                                                    if ((VarText != "")) {
                                                        if ((IndexConfig.GroupListCnt > 0)) {
                                                            for (Ptr = 0; (Ptr 
                                                                        <= (IndexConfig.GroupListCnt - 1)); Ptr++) {
                                                                if (IndexConfig.GroupList) {
                                                                    Ptr = VarText;
                                                                    IndexConfig.GroupList;
                                                                    "".PageNumber = 1;
                                                                    Ptr = 1;
                                                                    break;
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Remove FindWords
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterRemoveFind").ToLower();
                                                    if ((VarText != "")) {
                                                        if (IndexConfig.FindWords.ContainsKey) {
                                                            VarText;
                                                            IndexConfig.FindWords.Remove;
                                                            VarText;
                                                        }
                                                        
                                                        // If .FindWords.Count > 0 Then
                                                        //     For Ptr = 0 To .FindWords.Count - 1
                                                        //         If .FindWords(Ptr).Name = VarText Then
                                                        //             .FindWords(Ptr).MatchOption = FindWordMatchEnum.MatchIgnore
                                                        //             .FindWords(Ptr).Value = ""
                                                        //             .PageNumber = 1
                                                        //             Exit For
                                                        //         End If
                                                        //     Next
                                                        // End If
                                                    }
                                                    
                                                    // 
                                                    //  Read ActiveOnly
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterActiveOnly");
                                                    if ((VarText != "")) {
                                                        genericController.EncodeBoolean(VarText).PageNumber = 1;
                                                        IndexConfig.ActiveOnly = 1;
                                                    }
                                                    
                                                    // 
                                                    //  Read LastEditedByMe
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterLastEditedByMe");
                                                    if ((VarText != "")) {
                                                        genericController.EncodeBoolean(VarText).PageNumber = 1;
                                                        IndexConfig.LastEditedByMe = 1;
                                                    }
                                                    
                                                    // 
                                                    //  Last Edited Past 30 Days
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterLastEditedPast30Days");
                                                    if ((VarText != "")) {
                                                        false.PageNumber = 1;
                                                        false.LastEditedToday = 1;
                                                        genericController.EncodeBoolean(VarText).LastEditedPast7Days = 1;
                                                        IndexConfig.LastEditedPast30Days = 1;
                                                    }
                                                    else {
                                                        // 
                                                        //  Past 7 Days
                                                        // 
                                                        VarText = cpCore.docProperties.getText("IndexFilterLastEditedPast7Days");
                                                        if ((VarText != "")) {
                                                            false.PageNumber = 1;
                                                            genericController.EncodeBoolean(VarText).LastEditedToday = 1;
                                                            false.LastEditedPast7Days = 1;
                                                            IndexConfig.LastEditedPast30Days = 1;
                                                        }
                                                        else {
                                                            // 
                                                            //  Read LastEditedToday
                                                            // 
                                                            VarText = cpCore.docProperties.getText("IndexFilterLastEditedToday");
                                                            if ((VarText != "")) {
                                                                genericController.EncodeBoolean(VarText).PageNumber = 1;
                                                                false.LastEditedToday = 1;
                                                                false.LastEditedPast7Days = 1;
                                                                IndexConfig.LastEditedPast30Days = 1;
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Read IndexFilterOpen
                                                    // 
                                                    VarText = cpCore.docProperties.getText("IndexFilterOpen");
                                                    if ((VarText != "")) {
                                                        genericController.EncodeBoolean(VarText).PageNumber = 1;
                                                        IndexConfig.Open = 1;
                                                    }
                                                    
                                                    // 
                                                    //  SortField
                                                    // 
                                                    VarText = cpCore.docProperties.getText("SetSortField").ToLower();
                                                    if ((VarText != "")) {
                                                        if (IndexConfig.Sorts.ContainsKey) {
                                                            VarText;
                                                            IndexConfig.Sorts.Remove;
                                                            VarText;
                                                        }
                                                        
                                                        int sortDirection = cpCore.docProperties.getInteger("SetSortDirection");
                                                        if ((sortDirection > 0)) {
                                                            IndexConfig.Sorts.Add;
                                                            VarText;
                                                            new indexConfigSortClass();
                                                            // With...
                                                            fieldName = VarText;
                                                            direction = sortDirection;
                                                        }
                                                        
                                                        // 
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                return;
                                            ErrorTrap:
                                                handleLegacyClassError3("ProcessIndexConfigRequests");
                                                // 
                                                // =================================================================================
                                                // 
                                                // =================================================================================
                                                // 
                                                SetIndexSQL_SaveIndexConfig(((indexConfigClass)(IndexConfig)));
                                                // 
                                                string FilterText;
                                                string SubList;
                                                int Ptr;
                                                // 
                                                //  ----- Save filter state to the visit property
                                                // 
                                                // With...
                                                // 
                                                //  -----------------------------------------------------------------------------------------------
                                                //    Visit Properties (non-persistant)
                                                //  -----------------------------------------------------------------------------------------------
                                                // 
                                                FilterText = "";
                                                SubList = "";
                                                foreach (kvp in IndexConfig.FindWords) {
                                                    indexConfigFindWordClass findWord = kvp.Value;
                                                    if (((findWord.Name != "") 
                                                                && (findWord.MatchOption != FindWordMatchEnum.MatchIgnore))) {
                                                        SubList = (SubList + ("\r\n" 
                                                                    + (findWord.Name + ('\t' 
                                                                    + (findWord.Value + ('\t' + findWord.MatchOption))))));
                                                    }
                                                    
                                                }
                                                
                                                if ((SubList != "")) {
                                                    FilterText = (FilterText + ("\r\n" + ("FindWordList" 
                                                                + (SubList + "\r\n"))));
                                                }
                                                
                                                // 
                                                //  CDef List
                                                // 
                                                if ((IndexConfig.SubCDefID > 0)) {
                                                    FilterText = (FilterText + ("\r\n" + ("CDefList" + ("\r\n" 
                                                                + (IndexConfig.SubCDefID + "\r\n")))));
                                                }
                                                
                                                //         SubList = ""
                                                //         If .SubCDefCnt > 0 Then
                                                //             For Ptr = 0 To .SubCDefCnt - 1
                                                //                 If .SubCDefs(Ptr) <> 0 Then
                                                //                     SubList = SubList & vbCrLf & .SubCDefs(Ptr)
                                                //                 End If
                                                //             Next
                                                //         End If
                                                //         If SubList <> "" Then
                                                //             FilterText = FilterText & vbCrLf & "CDefList" & SubList & vbCrLf
                                                //         End If
                                                // 
                                                //  Group List
                                                // 
                                                SubList = "";
                                                if ((IndexConfig.GroupListCnt > 0)) {
                                                    for (Ptr = 0; (Ptr 
                                                                <= (IndexConfig.GroupListCnt - 1)); Ptr++) {
                                                        if (IndexConfig.GroupList) {
                                                            (Ptr != "");
                                                            SubList = (SubList + ("\r\n" + IndexConfig.GroupList));
                                                            Ptr;
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                                if ((SubList != "")) {
                                                    FilterText = (FilterText + ("\r\n" + ("GroupList" 
                                                                + (SubList + "\r\n"))));
                                                }
                                                
                                                // 
                                                //  PageNumber and Records Per Page
                                                // 
                                                FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + ("pagenumber" + ("\r\n" + IndexConfig.PageNumber))))));
                                                FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + ("recordsperpage" + ("\r\n" + IndexConfig.RecordsPerPage))))));
                                                // 
                                                //  misc filters
                                                // 
                                                if (IndexConfig.ActiveOnly) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterActiveOnly"))));
                                                }
                                                
                                                if (IndexConfig.LastEditedByMe) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterLastEditedByMe"))));
                                                }
                                                
                                                if (IndexConfig.LastEditedToday) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterLastEditedToday"))));
                                                }
                                                
                                                if (IndexConfig.LastEditedPast7Days) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterLastEditedPast7Days"))));
                                                }
                                                
                                                if (IndexConfig.LastEditedPast30Days) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterLastEditedPast30Days"))));
                                                }
                                                
                                                if (IndexConfig.Open) {
                                                    FilterText = (FilterText + ("\r\n" + ("" + ("\r\n" + "IndexFilterOpen"))));
                                                }
                                                
                                                // 
                                                cpCore.visitProperty.setProperty((IndexConfigPrefix + IndexConfig.ContentID.ToString()), FilterText);
                                                // 
                                                //  -----------------------------------------------------------------------------------------------
                                                //    Member Properties (persistant)
                                                //  -----------------------------------------------------------------------------------------------
                                                // 
                                                FilterText = "";
                                                SubList = "";
                                                foreach (kvp in IndexConfig.Columns) {
                                                    indexConfigColumnClass column = kvp.Value;
                                                    if ((column.Name != "")) {
                                                        SubList = (SubList + ("\r\n" 
                                                                    + (column.Name + ('\t' + column.Width))));
                                                    }
                                                    
                                                }
                                                
                                                if ((SubList != "")) {
                                                    FilterText = (FilterText + ("\r\n" + ("Columns" 
                                                                + (SubList + "\r\n"))));
                                                }
                                                
                                                // 
                                                //  Sorts
                                                // 
                                                SubList = "";
                                                foreach (kvp in IndexConfig.Sorts) {
                                                    indexConfigSortClass sort = kvp.Value;
                                                    if ((sort.fieldName != "")) {
                                                        SubList = (SubList + ("\r\n" 
                                                                    + (sort.fieldName + ('\t' + sort.direction))));
                                                    }
                                                    
                                                }
                                                
                                                if ((SubList != "")) {
                                                    FilterText = (FilterText + ("\r\n" + ("Sorts" 
                                                                + (SubList + "\r\n"))));
                                                }
                                                
                                                cpCore.userProperty.setProperty((IndexConfigPrefix + IndexConfig.ContentID.ToString()), FilterText);
                                                // 
                                                // 
                                                // 
                                                // 
                                                ((string)(GetFormInputWithFocus2(((string)(ElementName)), Optional, CurrentValueAsString=, Optional, HeightAsInteger=-1, Optional, WidthAsInteger=-1, Optional, ElementIDAsString=, Optional, OnFocusJavascriptAsString=, Optional, HtmlClassAsString=)));
                                                GetFormInputWithFocus2 = cpCore.html.html_GetFormInputText2(ElementName, CurrentValue, Height, Width, ElementID);
                                                if ((OnFocusJavascript != "")) {
                                                    GetFormInputWithFocus2 = genericController.vbReplace(GetFormInputWithFocus2, ">", (" onFocus=\"" 
                                                                    + (OnFocusJavascript + "\">")));
                                                }
                                                
                                                if ((HtmlClass != "")) {
                                                    GetFormInputWithFocus2 = genericController.vbReplace(GetFormInputWithFocus2, ">", (" class=\"" 
                                                                    + (HtmlClass + "\">")));
                                                }
                                                
                                                // 
                                                // 
                                                // 
                                                ((string)(GetFormInputWithFocus(((string)(ElementName)), ((string)(CurrentValue)), ((int)(Height)), ((int)(Width)), ((string)(ElementID)), ((string)(OnFocus)))));
                                                GetFormInputWithFocus = GetFormInputWithFocus2(ElementName, CurrentValue, Height, Width, ElementID, OnFocus);
                                                // 
                                                // 
                                                // 
                                                ((string)(GetFormInputDateWithFocus2(((string)(ElementName)), Optional, CurrentValueAsString=, Optional, WidthAsString=, Optional, ElementIDAsString=, Optional, OnFocusJavascriptAsString=, Optional, HtmlClassAsString=)));
                                                GetFormInputDateWithFocus2 = cpCore.html.html_GetFormInputDate(ElementName, CurrentValue, Width, ElementID);
                                                if ((OnFocusJavascript != "")) {
                                                    GetFormInputDateWithFocus2 = genericController.vbReplace(GetFormInputDateWithFocus2, ">", (" onFocus=\"" 
                                                                    + (OnFocusJavascript + "\">")));
                                                }
                                                
                                                if ((HtmlClass != "")) {
                                                    GetFormInputDateWithFocus2 = genericController.vbReplace(GetFormInputDateWithFocus2, ">", (" class=\"" 
                                                                    + (HtmlClass + "\">")));
                                                }
                                                
                                                // 
                                                // 
                                                // 
                                                ((string)(GetFormInputDateWithFocus(((string)(ElementName)), ((string)(CurrentValue)), ((string)(Width)), ((string)(ElementID)), ((string)(OnFocus)))));
                                                GetFormInputDateWithFocus = GetFormInputDateWithFocus2(ElementName, CurrentValue, Width, ElementID, OnFocus);
                                                // 
                                                // =================================================================================
                                                // 
                                                // =================================================================================
                                                // 
                                                ((string)(GetForm_Index_AdvancedSearch(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                                string returnForm = "";
                                                try {
                                                    // 
                                                    string SearchValue;
                                                    FindWordMatchEnum MatchOption;
                                                    int FormFieldPtr;
                                                    int FormFieldCnt;
                                                    Models.Complex.cdefModel CDef;
                                                    string FieldName;
                                                    stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                                                    int FieldPtr;
                                                    bool RowEven;
                                                    string Button;
                                                    string RQS;
                                                    string[] FieldNames;
                                                    string[] FieldCaption;
                                                    int[] fieldId;
                                                    int[] fieldTypeId;
                                                    string[] FieldValue;
                                                    int[] FieldMatchOptions;
                                                    int FieldMatchOption;
                                                    string[] FieldLookupContentName;
                                                    string[] FieldLookupList;
                                                    int ContentID;
                                                    int FieldCnt;
                                                    int FieldSize;
                                                    int RowPointer;
                                                    adminUIController Adminui = new adminUIController(cpCore);
                                                    string LeftButtons = "";
                                                    string ButtonBar;
                                                    string Title;
                                                    string TitleBar;
                                                    string Content;
                                                    string TitleDescription;
                                                    indexConfigClass IndexConfig;
                                                    // 
                                                    if (!false) {
                                                        // 
                                                        //  Process last form
                                                        // 
                                                        Button = cpCore.docProperties.getText("button");
                                                        if ((Button != "")) {
                                                            switch (Button) {
                                                                case ButtonSearch:
                                                                    IndexConfig = this.LoadIndexConfig(adminContent);
                                                                    // With...
                                                                    FormFieldCnt = cpCore.docProperties.getInteger("fieldcnt");
                                                                    if ((FormFieldCnt > 0)) {
                                                                        for (FormFieldPtr = 0; (FormFieldPtr 
                                                                                    <= (FormFieldCnt - 1)); FormFieldPtr++) {
                                                                            FieldName = genericController.vbLCase(cpCore.docProperties.getText(("fieldname" + FormFieldPtr)));
                                                                            cpCore.docProperties.getInteger(("FieldMatch" + FormFieldPtr));
                                                                            FindWordMatchEnum;
                                                                            switch (MatchOption) {
                                                                                case FindWordMatchEnum.MatchEquals:
                                                                                case FindWordMatchEnum.MatchGreaterThan:
                                                                                case FindWordMatchEnum.matchincludes:
                                                                                case FindWordMatchEnum.MatchLessThan:
                                                                                    SearchValue = cpCore.docProperties.getText(("FieldValue" + FormFieldPtr));
                                                                                    break;
                                                                                default:
                                                                                    SearchValue = "";
                                                                                    break;
                                                                            }
                                                                            if (!IndexConfig.FindWords.ContainsKey) {
                                                                                FieldName;
                                                                                // 
                                                                                //  fieldname not found, save if not FindWordMatchEnum.MatchIgnore
                                                                                // 
                                                                                if ((MatchOption != FindWordMatchEnum.MatchIgnore)) {
                                                                                    indexConfigFindWordClass findWord = new indexConfigFindWordClass();
                                                                                    findWord.Name = FieldName;
                                                                                    findWord.MatchOption = MatchOption;
                                                                                    findWord.Value = SearchValue.FindWords.Add(FieldName, findWord);
                                                                                }
                                                                                
                                                                            }
                                                                            else {
                                                                                // 
                                                                                //  fieldname was found
                                                                                // 
                                                                                IndexConfig.FindWords.Item;
                                                                                MatchOption.FindWords.Item[FieldName].Value = SearchValue;
                                                                                FieldName.MatchOption = SearchValue;
                                                                            }
                                                                            
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                    SetIndexSQL_SaveIndexConfig(IndexConfig);
                                                                    return String.Empty;
                                                                    break;
                                                                case ButtonCancel:
                                                                    return String.Empty;
                                                                    break;
                                                            }
                                                        }
                                                        
                                                        IndexConfig = this.LoadIndexConfig(adminContent);
                                                        Button = "CriteriaSelect";
                                                        RQS = cpCore.doc.refreshQueryString;
                                                        // 
                                                        //  ----- ButtonBar
                                                        // 
                                                        if ((MenuDepth > 0)) {
                                                            cpCore.html.html_GetFormButton(ButtonClose, ,, "window.close();");
                                                        }
                                                        else {
                                                            cpCore.html.html_GetFormButton(ButtonCancel);
                                                            // LeftButtons &= cpCore.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
                                                        }
                                                        
                                                        cpCore.html.html_GetFormButton(ButtonSearch);
                                                        // LeftButtons &= cpCore.main_GetFormButton(ButtonSearch, , , "return processSubmit(this)")
                                                        ButtonBar = Adminui.GetButtonBar(LeftButtons, "");
                                                        // 
                                                        //  ----- TitleBar
                                                        // 
                                                        Title = adminContent.Name;
                                                        Title = (Title + " Advanced Search");
                                                        Title = ("<strong>" 
                                                                    + (Title + "</strong>"));
                                                        Title = (SpanClassAdminNormal 
                                                                    + (Title + "</span>"));
                                                        TitleDescription = "<div>Enter criteria for each field to identify and select your results. The results of a search will " +
                                                        "have to have all of the criteria you enter.</div>";
                                                        TitleBar = Adminui.GetTitleBar(Title, TitleDescription);
                                                        // 
                                                        //  ----- List out all fields
                                                        // 
                                                        CDef = Models.Complex.cdefModel.getCdef(cpCore, adminContent.Name);
                                                        FieldSize = 100;
                                                        object Preserve;
                                                        FieldNames[FieldSize];
                                                        object Preserve;
                                                        FieldCaption[FieldSize];
                                                        object Preserve;
                                                        fieldId[FieldSize];
                                                        object Preserve;
                                                        fieldTypeId[FieldSize];
                                                        object Preserve;
                                                        FieldValue[FieldSize];
                                                        object Preserve;
                                                        FieldMatchOptions[FieldSize];
                                                        object Preserve;
                                                        FieldLookupContentName[FieldSize];
                                                        object Preserve;
                                                        FieldLookupList[FieldSize];
                                                        foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                            Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                            if ((FieldPtr >= FieldSize)) {
                                                                FieldSize = (FieldSize + 100);
                                                                object Preserve;
                                                                FieldNames[FieldSize];
                                                                object Preserve;
                                                                FieldCaption[FieldSize];
                                                                object Preserve;
                                                                fieldId[FieldSize];
                                                                object Preserve;
                                                                fieldTypeId[FieldSize];
                                                                object Preserve;
                                                                FieldValue[FieldSize];
                                                                object Preserve;
                                                                FieldMatchOptions[FieldSize];
                                                                object Preserve;
                                                                FieldLookupContentName[FieldSize];
                                                                object Preserve;
                                                                FieldLookupList[FieldSize];
                                                            }
                                                            
                                                            // With...
                                                            FieldName = genericController.vbLCase(field.nameLc);
                                                            FieldNames[FieldPtr] = FieldName;
                                                            FieldCaption[FieldPtr] = field.caption;
                                                            fieldId[FieldPtr] = field.id;
                                                            fieldTypeId[FieldPtr] = field.fieldTypeId;
                                                            if ((fieldTypeId[FieldPtr] == FieldTypeIdLookup)) {
                                                                ContentID = field.lookupContentID;
                                                                if ((ContentID > 0)) {
                                                                    FieldLookupContentName[FieldPtr] = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
                                                                }
                                                                
                                                                FieldLookupList[FieldPtr] = field.lookupList;
                                                            }
                                                            
                                                            // 
                                                            //  set prepoplate value from indexconfig
                                                            // 
                                                            // With...
                                                            if (IndexConfig.FindWords.ContainsKey) {
                                                                FieldName;
                                                                FieldValue[FieldPtr] = IndexConfig.FindWords;
                                                                FieldName.Value;
                                                                FieldMatchOptions[FieldPtr] = IndexConfig.FindWords;
                                                                FieldName.MatchOption;
                                                            }
                                                            
                                                            FieldPtr++;
                                                        }
                                                        
                                                        FieldCnt = FieldPtr;
                                                        // 
                                                        //  Add headers to stream
                                                        // 
                                                        returnForm = (returnForm + "<table border=0 width=100% cellspacing=0 cellpadding=4>");
                                                        RowPointer = 0;
                                                        for (FieldPtr = 0; (FieldPtr 
                                                                    <= (FieldCnt - 1)); FieldPtr++) {
                                                            returnForm = (returnForm + cpCore.html.html_GetFormInputHidden(("fieldname" + FieldPtr), FieldNames[FieldPtr]));
                                                            RowEven = ((RowPointer % 2) 
                                                                        == 0);
                                                            FieldMatchOption = FieldMatchOptions[FieldPtr];
                                                            switch (fieldTypeId[FieldPtr]) {
                                                                case FieldTypeIdDate:
                                                                    // 
                                                                    //  Date
                                                                    // 
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") + ("empty</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") + ("not&nbsp;empty</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEquals).ToString, FieldMatchOption.ToString, "") + ("=</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchGreaterThan).ToString, FieldMatchOption.ToString, "") + ("&gt;</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchLessThan).ToString, FieldMatchOption.ToString, "") + ("&lt;</div>" + ("<div style=\"display:block;float:left;width:300px;\">" 
                                                                                + (GetFormInputDateWithFocus2(("fieldvalue" + FieldPtr), FieldValue[FieldPtr], "5", "", "", "ccAdvSearchText") + ("</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))))))))))))))));
                                                                    break;
                                                                case FieldTypeIdCurrency:
                                                                case FieldTypeIdFloat:
                                                                case FieldTypeIdInteger:
                                                                case FieldTypeIdFloat:
                                                                case FieldTypeIdAutoIdIncrement:
                                                                    // 
                                                                    //  Numeric
                                                                    // 
                                                                    //  changed FindWordMatchEnum.MatchEquals to MatchInclude to be compatible with Find Search
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") + ("empty</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") + ("not&nbsp;empty</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, ("n" + FieldPtr)) + ("=</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchGreaterThan).ToString, FieldMatchOption.ToString, "") + ("&gt;</div>" + ("<div style=\"display:block;float:left;width:50px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchLessThan).ToString, FieldMatchOption.ToString, "") + ("&lt;</div>" + ("<div style=\"display:block;float:left;width:300px;\">" 
                                                                                + (GetFormInputWithFocus2(("fieldvalue" + FieldPtr), FieldValue[FieldPtr], 1, 5, "", ("var e=getElementById(\'n" 
                                                                                    + (FieldPtr + "\');e.checked=1;")), "ccAdvSearchText") + ("</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))))))))))))))));
                                                                    RowPointer = (RowPointer + 1);
                                                                    break;
                                                                case FieldTypeIdFile:
                                                                case FieldTypeIdFileImage:
                                                                    // 
                                                                    //  File
                                                                    // 
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") + ("empty</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") + ("not&nbsp;empty</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))));
                                                                    RowPointer = (RowPointer + 1);
                                                                    break;
                                                                case FieldTypeIdBoolean:
                                                                    // 
                                                                    //  Boolean
                                                                    // 
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchTrue).ToString, FieldMatchOption.ToString, "") + ("true</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchFalse).ToString, FieldMatchOption.ToString, "") + ("false</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))));
                                                                    break;
                                                                case FieldTypeIdText:
                                                                case FieldTypeIdLongText:
                                                                case FieldTypeIdHTML:
                                                                case FieldTypeIdFileHTML:
                                                                case FieldTypeIdFileCSS:
                                                                case FieldTypeIdFileJavascript:
                                                                case FieldTypeIdFileXML:
                                                                    // 
                                                                    //  Text
                                                                    // 
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") + ("empty</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") + ("not&nbsp;empty</div>" + ("<div style=\"display:block;float:left;width:150px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, ("t" + FieldPtr)) + ("includes</div>" + ("<div style=\"display:block;float:left;width:300px;\">" 
                                                                                + (GetFormInputWithFocus2(("fieldvalue" + FieldPtr), FieldValue[FieldPtr], 1, 5, "", ("var e=getElementById(\'t" 
                                                                                    + (FieldPtr + "\');e.checked=1;")), "ccAdvSearchText") + ("</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))))))))));
                                                                    RowPointer = (RowPointer + 1);
                                                                    break;
                                                                case FieldTypeIdLookup:
                                                                case FieldTypeIdMemberSelect:
                                                                    // 
                                                                    //  Lookup
                                                                    // 
                                                                    returnForm = (returnForm + ("<tr>" + ("<td class=\"ccAdminEditCaption\">" 
                                                                                + (FieldCaption[FieldPtr] + ("</td>" + ("<td class=\"ccAdminEditField\">" + ("<div style=\"display:block;float:left;width:800px;\">" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchIgnore).ToString, FieldMatchOption.ToString, "") + ("ignore</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchEmpty).ToString, FieldMatchOption.ToString, "") + ("empty</div>" + ("<div style=\"display:block;float:left;width:100px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.MatchNotEmpty).ToString, FieldMatchOption.ToString, "") + ("not&nbsp;empty</div>" + ("<div style=\"display:block;float:left;width:150px;\">" 
                                                                                + (cpCore.html.html_GetFormInputRadioBox(("FieldMatch" + FieldPtr), int.Parse(FindWordMatchEnum.matchincludes).ToString, FieldMatchOption.ToString, ("t" + FieldPtr)) + ("includes</div>" + ("<div style=\"display:block;float:left;width:300px;\">" 
                                                                                + (GetFormInputWithFocus2(("fieldvalue" + FieldPtr), FieldValue[FieldPtr], 1, 5, "", ("var e=getElementById(\'t" 
                                                                                    + (FieldPtr + "\');e.checked=1;")), "ccAdvSearchText") + ("</div>" + ("</div>" + ("</td>" + "</tr>"))))))))))))))))))))))));
                                                                    RowPointer = (RowPointer + 1);
                                                                    break;
                                                            }
                                                        }
                                                        
                                                        returnForm = (returnForm + genericController.StartTableRow());
                                                        returnForm = (returnForm 
                                                                    + (genericController.StartTableCell("120", 1, RowEven, "right") + "<img src=/ccLib/images/spacer.gif width=120 height=1></td>"));
                                                        returnForm = (returnForm 
                                                                    + (genericController.StartTableCell("99%", 1, RowEven, "left") + "<img src=/ccLib/images/spacer.gif width=1 height=1></td>"));
                                                        returnForm = (returnForm + kmaEndTableRow);
                                                        returnForm = (returnForm + "</table>");
                                                        Content = returnForm;
                                                        // 
                                                        //  Assemble LiveWindowTable
                                                        // 
                                                        //         Stream.Add( OpenLiveWindowTable)
                                                        Stream.Add(("\r\n" + cpCore.html.html_GetFormStart()));
                                                        Stream.Add(ButtonBar);
                                                        Stream.Add(TitleBar);
                                                        Stream.Add(Content);
                                                        Stream.Add(ButtonBar);
                                                        Stream.Add(("<input type=hidden name=fieldcnt VALUE=" 
                                                                        + (FieldCnt + ">")));
                                                        // Stream.Add( "<input type=hidden name=af VALUE=" & AdminFormIndex & ">")
                                                        Stream.Add(("<input type=hidden name=" 
                                                                        + (RequestNameAdminSubForm + (" VALUE=" 
                                                                        + (AdminFormIndex_SubFormAdvancedSearch + ">")))));
                                                        Stream.Add("</form>");
                                                        //         Stream.Add( CloseLiveWindowTable)
                                                        // 
                                                        returnForm = Stream.Text;
                                                        cpCore.html.addTitle((adminContent.Name + " Advanced Search"));
                                                    }
                                                    
                                                }
                                                catch (Exception ex) {
                                                    cpCore.handleException(ex);
                                                    throw;
                                                }
                                                
                                                return returnForm;
                                                // 
                                                // =============================================================================
                                                //    Export the Admin List form results
                                                // =============================================================================
                                                // 
                                                ((string)(GetForm_Index_Export(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 
                                                bool AllowContentAccess;
                                                string ButtonList = "";
                                                string ExportName;
                                                adminUIController Adminui = new adminUIController(cpCore);
                                                string Description;
                                                string Content = "";
                                                int ExportType;
                                                string Button;
                                                int RecordLimit;
                                                int recordCnt;
                                                // Dim DataSourceName As String
                                                // Dim DataSourceType As Integer
                                                string sqlFieldList = "";
                                                string SQLFrom = "";
                                                string SQLWhere = "";
                                                string SQLOrderBy = "";
                                                bool IsLimitedToSubContent;
                                                string ContentAccessLimitMessage = "";
                                                Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>();
                                                Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
                                                indexConfigClass IndexConfig;
                                                string SQL;
                                                int CS;
                                                // Dim RecordTop As Integer
                                                // Dim RecordsPerPage As Integer
                                                bool IsRecordLimitSet;
                                                string RecordLimitText;
                                                bool allowContentEdit;
                                                bool allowContentAdd;
                                                bool allowContentDelete;
                                                Models.Entity.dataSourceModel datasource = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, new List<string>());
                                                // 
                                                //  ----- Process Input
                                                // 
                                                Button = cpCore.docProperties.getText("Button");
                                                if ((Button == ButtonCancelAll)) {
                                                    // 
                                                    //  Cancel out to the main page
                                                    // 
                                                    return cpCore.webServer.redirect("?", "CancelAll button pressed on Index Export");
                                                }
                                                else if ((Button != ButtonCancel)) {
                                                    // 
                                                    //  get content access rights
                                                    // 
                                                    cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowContentEdit, allowContentAdd, allowContentDelete);
                                                    if (!allowContentEdit) {
                                                        // If Not cpCore.doc.authContext.user.main_IsContentManager2(AdminContent.Name) Then
                                                        // 
                                                        //  You must be a content manager of this content to use this tool
                                                        // 
                                                        Content = ("" + ("<p>You must be a content manager of " 
                                                                    + (adminContent.Name + (" to use this tool. Hit Cancel to return to main admin page.</p>" 
                                                                    + (cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "")))));
                                                        ButtonList = ButtonCancelAll;
                                                    }
                                                    else {
                                                        IsRecordLimitSet = false;
                                                        if ((Button == "")) {
                                                            // 
                                                            //  Set Defaults
                                                            // 
                                                            ExportName = "";
                                                            ExportType = 1;
                                                            RecordLimit = 0;
                                                            RecordLimitText = "";
                                                        }
                                                        else {
                                                            ExportName = cpCore.docProperties.getText("ExportName");
                                                            ExportType = cpCore.docProperties.getInteger("ExportType");
                                                            RecordLimitText = cpCore.docProperties.getText("RecordLimit");
                                                            if ((RecordLimitText != "")) {
                                                                IsRecordLimitSet = true;
                                                                RecordLimit = genericController.EncodeInteger(RecordLimitText);
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((ExportName == "")) {
                                                            ExportName = (adminContent.Name + (" export for " + cpCore.doc.authContext.user.name));
                                                        }
                                                        
                                                        // 
                                                        //  Get the SQL parts
                                                        // 
                                                        // DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
                                                        // DataSourceType = cpCore.db.getDataSourceType(DataSourceName)
                                                        IndexConfig = this.LoadIndexConfig(adminContent);
                                                        // RecordTop = IndexConfig.RecordTop
                                                        // RecordsPerPage = IndexConfig.RecordsPerPage
                                                        SetIndexSQL(adminContent, editRecord, IndexConfig, AllowContentAccess, sqlFieldList, SQLFrom, SQLWhere, SQLOrderBy, IsLimitedToSubContent, ContentAccessLimitMessage, FieldUsedInColumns, IsLookupFieldValid);
                                                        if (!AllowContentAccess) {
                                                            // 
                                                            //  This should be caught with check earlier, but since I added this, and I never make mistakes, I will leave this in case there is a mistake in the earlier code
                                                            // 
                                                            errorController.error_AddUserError(cpCore, ("Your account does not have access to any records in \'" 
                                                                            + (adminContent.Name + "\'.")));
                                                        }
                                                        else {
                                                            // 
                                                            //  Get the total record count
                                                            // 
                                                            SQL = ("select count(" 
                                                                        + (adminContent.ContentTableName + (".ID) as cnt from " 
                                                                        + (SQLFrom + (" where " + SQLWhere)))));
                                                            CS = cpCore.db.csOpenSql_rev(datasource.Name, SQL);
                                                            if (cpCore.db.csOk(CS)) {
                                                                recordCnt = cpCore.db.csGetInteger(CS, "cnt");
                                                            }
                                                            
                                                            cpCore.db.csClose(CS);
                                                            // 
                                                            //  Build the SQL
                                                            // 
                                                            SQL = "select";
                                                            if ((IsRecordLimitSet 
                                                                        && (datasource.type != DataSourceTypeODBCMySQL))) {
                                                                (" Top " + RecordLimit);
                                                            }
                                                            
                                                            (" " 
                                                                        + (adminContent.ContentTableName + (".* From " 
                                                                        + (SQLFrom + (" WHERE " + SQLWhere)))));
                                                            if ((SQLOrderBy != "")) {
                                                                (" Order By" + SQLOrderBy);
                                                            }
                                                            
                                                            if ((IsRecordLimitSet 
                                                                        && (datasource.type == DataSourceTypeODBCMySQL))) {
                                                                (" Limit " + RecordLimit);
                                                            }
                                                            
                                                            // 
                                                            //  Assumble the SQL
                                                            // 
                                                            if ((recordCnt == 0)) {
                                                                // 
                                                                //  There are no records to request
                                                                // 
                                                                Content = ("" + ("<p>This selection has no records.. Hit Cancel to return to the " 
                                                                            + (adminContent.Name + (" list page.</p>" 
                                                                            + (cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "")))));
                                                                ButtonList = ButtonCancel;
                                                            }
                                                            else if ((Button == ButtonRequestDownload)) {
                                                                // 
                                                                //  Request the download
                                                                // 
                                                                switch (ExportType) {
                                                                    case 1:
                                                                        taskSchedulerController.main_RequestTask(cpCore, "BuildCSV", SQL, ExportName, ("Export-" 
                                                                                        + (genericController.GetRandomInteger.ToString() + ".csv")));
                                                                        break;
                                                                    default:
                                                                        taskSchedulerController.main_RequestTask(cpCore, "BuildXML", SQL, ExportName, ("Export-" 
                                                                                        + (genericController.GetRandomInteger.ToString() + ".xml")));
                                                                        break;
                                                                }
                                                                // 
                                                                Content = ("" + ("<p>Your export has been requested and will be available shortly in the <a href=\"?" 
                                                                            + (RequestNameAdminForm + ("=" 
                                                                            + (AdminFormDownloads + ("\">Download Manager</a>. Hit Cancel to return to the " 
                                                                            + (adminContent.Name + (" list page.</p>" 
                                                                            + (cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "")))))))));
                                                                ButtonList = ButtonCancel;
                                                            }
                                                            else {
                                                                // 
                                                                //  no button or refresh button, Ask are you sure
                                                                // 
                                                                Content = (Content 
                                                                            + (cr + ("<tr>" 
                                                                            + (cr2 + ("<td class=\"exportTblCaption\">Export Name</td>" 
                                                                            + (cr2 + ("<td class=\"exportTblInput\">" 
                                                                            + (cpCore.html.html_GetFormInputText2("ExportName", ExportName) + ("</td>" 
                                                                            + (cr + "</tr>"))))))))));
                                                                Content = (Content 
                                                                            + (cr + ("<tr>" 
                                                                            + (cr2 + ("<td class=\"exportTblCaption\">Export Format</td>" 
                                                                            + (cr2 + ("<td class=\"exportTblInput\">" 
                                                                            + (cpCore.html.getInputSelectList2("ExportType", ExportType, "Comma Delimited,XML", "", "") + ("</td>" 
                                                                            + (cr + "</tr>"))))))))));
                                                                Content = (Content 
                                                                            + (cr + ("<tr>" 
                                                                            + (cr2 + ("<td class=\"exportTblCaption\">Records Found</td>" 
                                                                            + (cr2 + ("<td class=\"exportTblInput\">" 
                                                                            + (cpCore.html.html_GetFormInputText2("RecordCnt", recordCnt.ToString(), ,, ,, true) + ("</td>" 
                                                                            + (cr + "</tr>"))))))))));
                                                                Content = (Content 
                                                                            + (cr + ("<tr>" 
                                                                            + (cr2 + ("<td class=\"exportTblCaption\">Record Limit</td>" 
                                                                            + (cr2 + ("<td class=\"exportTblInput\">" 
                                                                            + (cpCore.html.html_GetFormInputText2("RecordLimit", RecordLimitText) + ("</td>" 
                                                                            + (cr + "</tr>"))))))))));
                                                                if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                                                                    Content = (Content 
                                                                                + (cr + ("<tr>" 
                                                                                + (cr2 + ("<td class=\"exportTblCaption\">Results SQL</td>" 
                                                                                + (cr2 + ("<td class=\"exportTblInput\"><div style=\"border:1px dashed #ccc;background-color:#fff;padding:10px;\">" 
                                                                                + (SQL + ("</div></td>" 
                                                                                + (cr + ("</tr>" + "")))))))))));
                                                                }
                                                                
                                                                // 
                                                                Content = ("" 
                                                                            + (cr + ("<table>" 
                                                                            + (genericController.htmlIndent(Content) 
                                                                            + (cr + ("</table>" + ""))))));
                                                                Content = ("" 
                                                                            + (cr + ("<style>" 
                                                                            + (cr2 + (".exportTblCaption {width:100px;}" 
                                                                            + (cr2 + (".exportTblInput {}" 
                                                                            + (cr + ("</style>" 
                                                                            + (Content 
                                                                            + (cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormExport) + "")))))))))));
                                                                ButtonList = (ButtonCancel + ("," + ButtonRequestDownload));
                                                                if (cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)) {
                                                                    ButtonList = (ButtonList + ("," + ButtonRefresh));
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    Description = @"<p>This tool creates an export of the current admin list page results. If you would like to download the current results, select a format and press OK. Your search results will be submitted for export. Your download will be ready shortly in the download manager. To exit without requesting an output, hit Cancel.</p>";
                                                    GetForm_Index_Export = ("" + Adminui.GetBody((adminContent.Name + " Export"), ButtonList, "", false, false, Description, "", 10, Content));
                                                }
                                                
                                                // 
                                                // TODO: Exit Function: Warning!!! Need to return the value
                                                return;
                                            ErrorTrap:
                                                handleLegacyClassError3("GetForm_Index_Export");
                                                // 
                                                // =============================================================================
                                                //    Print the Configure Index Form
                                                // =============================================================================
                                                // 
                                                ((string)(GetForm_Index_SetColumns(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)))));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 
                                                string Button;
                                                int Ptr;
                                                string Description;
                                                bool NeedToReloadCDef;
                                                string Title;
                                                string TitleBar;
                                                string Content;
                                                string ButtonBar;
                                                adminUIController Adminui = new adminUIController(cpCore);
                                                string SQL;
                                                string MenuHeader;
                                                int ColumnPtr;
                                                int ColumnWidth;
                                                int FieldPtr;
                                                string FieldName;
                                                int FieldToAdd;
                                                string AStart;
                                                int CS;
                                                bool SetSort;
                                                int MenuEntryID;
                                                int MenuHeaderID;
                                                int MenuDirection;
                                                int SourceID;
                                                int PreviousID;
                                                int SetID;
                                                int NextSetID;
                                                bool SwapWithPrevious;
                                                int HitID;
                                                string HitTable;
                                                int SortPriorityLowest;
                                                string TempColumn;
                                                string Tempfield;
                                                string TempWidth;
                                                int TempSortPriority;
                                                int TempSortDirection;
                                                int CSPointer;
                                                int RecordID;
                                                int ContentID;
                                                Models.Complex.cdefModel CDef;
                                                // Dim AdminColumn As appServices_metaDataClass.CDefAdminColumnType
                                                int[] RowFieldID;
                                                int[] RowFieldWidth;
                                                string[] RowFieldCaption;
                                                // Dim RowFieldCount as integer
                                                int[] NonRowFieldID;
                                                string[] NonRowFieldCaption;
                                                int NonRowFieldCount;
                                                string ContentName;
                                                // 
                                                DataTable dt;
                                                int IndexWidth;
                                                int CS1;
                                                int CS2;
                                                int FieldPtr1;
                                                int FieldPtr2;
                                                int NewRowFieldWidth;
                                                int TargetFieldID;
                                                string TargetFieldName;
                                                // 
                                                int ColumnWidthTotal;
                                                // 
                                                int ColumnPointer;
                                                int CDefFieldCount;
                                                int fieldId;
                                                int FieldWidth;
                                                bool AllowContentAutoLoad;
                                                int TargetFieldPtr;
                                                bool MoveNextColumn;
                                                string FieldNameToAdd;
                                                int FieldIDToAdd;
                                                int CSSource;
                                                int CSTarget;
                                                int SourceContentID;
                                                string SourceName;
                                                bool NeedToReloadConfig;
                                                int InheritedFieldCount;
                                                string Caption;
                                                // Dim ContentNameValues() As NameValuePrivateType
                                                int ContentCount;
                                                int ContentSize;
                                                stringBuilderLegacyController Stream = new stringBuilderLegacyController();
                                                string ButtonList;
                                                string FormPanel;
                                                int ColumnWidthIncrease;
                                                int ColumnWidthBalance;
                                                int ToolsAction;
                                                indexConfigClass IndexConfig;
                                                // 'Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                                                int FieldPointerTemp;
                                                string NameTemp;
                                                int WidthTemp;
                                                // 
                                                const object RequestNameAddField = "addfield";
                                                const object RequestNameAddFieldID = "addfieldID";
                                                Button = cpCore.docProperties.getText(RequestNameButton);
                                                if ((Button == ButtonOK)) {
                                                    // TODO: Exit Function: Warning!!! Need to return the value
                                                    return;
                                                }
                                                
                                                // 
                                                // --------------------------------------------------------------------------------
                                                //    Load Request
                                                // --------------------------------------------------------------------------------
                                                // 
                                                ContentID = adminContent.Id;
                                                ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID);
                                                if ((Button == ButtonReset)) {
                                                    cpCore.userProperty.setProperty((IndexConfigPrefix + ContentID.ToString()), "");
                                                }
                                                
                                                IndexConfig = this.LoadIndexConfig(adminContent);
                                                Title = (adminContent.Name + " Columns");
                                                Description = ("Use the icons to add, remove and modify your personal column prefernces for this content (" 
                                                            + (ContentName + "). Hit OK when complete. Hit Reset to restore your column preferences for this content to the site\'s " +
                                                            "default column preferences."));
                                                ToolsAction = cpCore.docProperties.getInteger("dta");
                                                TargetFieldID = cpCore.docProperties.getInteger("fi");
                                                TargetFieldName = cpCore.docProperties.getText("FieldName");
                                                ColumnPointer = cpCore.docProperties.getInteger("dtcn");
                                                FieldNameToAdd = genericController.vbUCase(cpCore.docProperties.getText(RequestNameAddField));
                                                FieldIDToAdd = cpCore.docProperties.getInteger(RequestNameAddFieldID);
                                                // ButtonList = ButtonCancel & "," & ButtonSelect
                                                NeedToReloadConfig = cpCore.docProperties.getBoolean("NeedToReloadConfig");
                                                // 
                                                // --------------------------------------------------------------------------------
                                                //  Process actions
                                                // --------------------------------------------------------------------------------
                                                // 
                                                if ((ContentID != 0)) {
                                                    CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                                                    if ((ToolsAction != 0)) {
                                                        // 
                                                        //  Block contentautoload, then force a load at the end
                                                        // 
                                                        AllowContentAutoLoad = cpCore.siteProperties.getBoolean("AllowContentAutoLoad", true);
                                                        cpCore.siteProperties.setProperty("AllowContentAutoLoad", false);
                                                        // 
                                                        //  Make sure the FieldNameToAdd is not-inherited, if not, create new field
                                                        // 
                                                        if ((FieldIDToAdd != 0)) {
                                                            foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                                if ((field.id == FieldIDToAdd)) {
                                                                    // If CDef.fields(FieldPtr).Name = FieldNameToAdd Then
                                                                    if (field.inherited) {
                                                                        SourceContentID = field.contentId;
                                                                        SourceName = field.nameLc;
                                                                        CSSource = cpCore.db.csOpen("Content Fields", ("(ContentID=" 
                                                                                        + (SourceContentID + (")and(Name=" 
                                                                                        + (cpCore.db.encodeSQLText(SourceName) + ")")))));
                                                                        if (cpCore.db.csOk(CSSource)) {
                                                                            CSTarget = cpCore.db.csInsertRecord("Content Fields");
                                                                            if (cpCore.db.csOk(CSTarget)) {
                                                                                cpCore.db.csCopyRecord(CSSource, CSTarget);
                                                                                cpCore.db.csSet(CSTarget, "ContentID", ContentID);
                                                                                NeedToReloadCDef = true;
                                                                            }
                                                                            
                                                                            cpCore.db.csClose(CSTarget);
                                                                        }
                                                                        
                                                                        cpCore.db.csClose(CSSource);
                                                                    }
                                                                    
                                                                    break;
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                        // 
                                                        //  Make sure all fields are not-inherited, if not, create new fields
                                                        // 
                                                        foreach (kvp in IndexConfig.Columns) {
                                                            indexConfigColumnClass column = kvp.Value;
                                                            Models.Complex.CDefFieldModel field = adminContent.fields(column.Name.ToLower());
                                                            if (field.inherited) {
                                                                SourceContentID = field.contentId;
                                                                SourceName = field.nameLc;
                                                                CSSource = cpCore.db.csOpen("Content Fields", ("(ContentID=" 
                                                                                + (SourceContentID + (")and(Name=" 
                                                                                + (cpCore.db.encodeSQLText(SourceName) + ")")))));
                                                                if (cpCore.db.csOk(CSSource)) {
                                                                    CSTarget = cpCore.db.csInsertRecord("Content Fields");
                                                                    if (cpCore.db.csOk(CSTarget)) {
                                                                        cpCore.db.csCopyRecord(CSSource, CSTarget);
                                                                        cpCore.db.csSet(CSTarget, "ContentID", ContentID);
                                                                        NeedToReloadCDef = true;
                                                                    }
                                                                    
                                                                    cpCore.db.csClose(CSTarget);
                                                                }
                                                                
                                                                cpCore.db.csClose(CSSource);
                                                            }
                                                            
                                                        }
                                                        
                                                        // 
                                                        //  get current values for Processing
                                                        // 
                                                        foreach (kvp in IndexConfig.Columns) {
                                                            indexConfigColumnClass column = kvp.Value;
                                                            ColumnWidthTotal = (ColumnWidthTotal + column.Width);
                                                        }
                                                        
                                                        // 
                                                        //  ----- Perform any actions first
                                                        // 
                                                        switch (ToolsAction) {
                                                            case ToolsActionAddField:
                                                                // 
                                                                //  Add a field to the index form
                                                                // 
                                                                if ((FieldIDToAdd != 0)) {
                                                                    indexConfigColumnClass column;
                                                                    foreach (kvp in IndexConfig.Columns) {
                                                                        column = kvp.Value;
                                                                        column.Width = int.Parse(((column.Width * 80) 
                                                                                        / ColumnWidthTotal));
                                                                    }
                                                                    
                                                                    column = new indexConfigColumnClass();
                                                                    CSPointer = cpCore.db.csOpenRecord("Content Fields", FieldIDToAdd, false, false);
                                                                    if (cpCore.db.csOk(CSPointer)) {
                                                                        column.Name = cpCore.db.csGet(CSPointer, "name");
                                                                        column.Width = 20;
                                                                    }
                                                                    
                                                                    cpCore.db.csClose(CSPointer);
                                                                    IndexConfig.Columns.Add(column.Name.ToLower(), column);
                                                                    NeedToReloadConfig = true;
                                                                }
                                                                
                                                                // 
                                                                break;
                                                            case ToolsActionRemoveField:
                                                                // 
                                                                //  Remove a field to the index form
                                                                // 
                                                                indexConfigColumnClass column;
                                                                if (IndexConfig.Columns.ContainsKey(TargetFieldName.ToLower())) {
                                                                    column = IndexConfig.Columns[TargetFieldName.ToLower()];
                                                                    ColumnWidthTotal = (ColumnWidthTotal + column.Width);
                                                                    IndexConfig.Columns.Remove(TargetFieldName.ToLower());
                                                                    // 
                                                                    //  Normalize the widths of the remaining columns
                                                                    // 
                                                                    foreach (kvp in IndexConfig.Columns) {
                                                                        column = kvp.Value;
                                                                        column.Width = int.Parse(((1000 * column.Width) 
                                                                                        / ColumnWidthTotal));
                                                                    }
                                                                    
                                                                    NeedToReloadConfig = true;
                                                                }
                                                                
                                                                break;
                                                            case ToolsActionMoveFieldLeft:
                                                                break;
                                                            case ToolsActionMoveFieldRight:
                                                                break;
                                                            case ToolsActionExpand:
                                                                // 
                                                                //  Expand column
                                                                // 
                                                                //  end case
                                                                break;
                                                            case ToolsActionContract:
                                                                break;
                                                        }
                                                        // 
                                                        //  Reload CDef if it changed
                                                        // 
                                                        if (NeedToReloadCDef) {
                                                            cpCore.doc.clearMetaData();
                                                            cpCore.cache.invalidateAll();
                                                            CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                                                        }
                                                        
                                                        // 
                                                        //  save indexconfig
                                                        // 
                                                        if (NeedToReloadConfig) {
                                                            SetIndexSQL_SaveIndexConfig(IndexConfig);
                                                            IndexConfig = this.LoadIndexConfig(adminContent);
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    // --------------------------------------------------------------------------------
                                                    //    Display the form
                                                    // --------------------------------------------------------------------------------
                                                    // 
                                                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                                                    Stream.Add("<td width=\"5%\">&nbsp;</td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>10%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>20%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>30%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>40%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>50%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>60%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>70%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>80%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>90%</nobr></td>");
                                                    Stream.Add("<td width=\"9%\" align=\"center\" class=\"ccAdminSmall\"><nobr>100%</nobr></td>");
                                                    Stream.Add("<td width=\"4%\" align=\"center\">&nbsp;</td>");
                                                    Stream.Add("</tr></table>");
                                                    // 
                                                    Stream.Add("<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"99%\"><tr>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("<td width=\"9%\"><nobr><img src=\"/ccLib/images/black.gif\" width=\"1\" height=\"10\" ><img alt=\"space\" src=\"" +
                                                        "/ccLib/images/spacer.gif\" width=\"100%\" height=\"10\" ></nobr></td>");
                                                    Stream.Add("</tr></table>");
                                                    // 
                                                    //  print the column headers
                                                    // 
                                                    ColumnWidthTotal = 0;
                                                    if ((IndexConfig.Columns.Count > 0)) {
                                                        // 
                                                        //  Calc total width
                                                        // 
                                                        foreach (kvp in IndexConfig.Columns) {
                                                            indexConfigColumnClass column = kvp.Value;
                                                            ColumnWidthTotal = (ColumnWidthTotal + column.Width);
                                                        }
                                                        
                                                        if ((ColumnWidthTotal > 0)) {
                                                            Stream.Add("<table border=\"0\" cellpadding=\"5\" cellspacing=\"0\" width=\"90%\">");
                                                            foreach (kvp in IndexConfig.Columns) {
                                                                indexConfigColumnClass column;
                                                                column = kvp.Value;
                                                                // 
                                                                //  print column headers - anchored so they sort columns
                                                                // 
                                                                ColumnWidth = int.Parse((100 
                                                                                * (column.Width / ColumnWidthTotal)));
                                                                Models.Complex.CDefFieldModel field;
                                                                field = adminContent.fields(column.Name.ToLower());
                                                                // With...
                                                                fieldId = field.id;
                                                                Caption = field.caption;
                                                                if (field.inherited) {
                                                                    Caption = (Caption + "*");
                                                                    InheritedFieldCount = (InheritedFieldCount + 1);
                                                                }
                                                                
                                                                AStart = ("<a href=\"?" 
                                                                            + (cpCore.doc.refreshQueryString + ("&FieldName=" 
                                                                            + (genericController.encodeHTML(field.nameLc) + ("&fi=" 
                                                                            + (fieldId + ("&dtcn=" 
                                                                            + (ColumnPtr + ("&" 
                                                                            + (RequestNameAdminSubForm + ("=" + AdminFormIndex_SubFormSetColumns)))))))))));
                                                                Stream.Add(("<td width=\"" 
                                                                                + (ColumnWidth + ("%\" valign=\"top\" align=\"left\">" 
                                                                                + (SpanClassAdminNormal 
                                                                                + (Caption + "<br >"))))));
                                                                Stream.Add("<img src=\"/ccLib/images/black.GIF\" width=\"100%\" height=\"1\" >");
                                                                Stream.Add((AStart + ("&dta=" 
                                                                                + (ToolsActionRemoveField + "\"><img src=\"/ccLib/images/LibButtonDeleteUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><BR >"))));
                                                                Stream.Add((AStart + ("&dta=" 
                                                                                + (ToolsActionMoveFieldRight + "\"><img src=\"/ccLib/images/LibButtonMoveRightUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><BR >"))));
                                                                Stream.Add((AStart + ("&dta=" 
                                                                                + (ToolsActionMoveFieldLeft + "\"><img src=\"/ccLib/images/LibButtonMoveLeftUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><BR >"))));
                                                                // Call Stream.Add(AStart & "&dta=" & ToolsActionSetAZ & """><img src=""/ccLib/images/LibButtonSortazUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                                                // Call Stream.Add(AStart & "&dta=" & ToolsActionSetZA & """><img src=""/ccLib/images/LibButtonSortzaUp.gif"" width=""50"" height=""15"" border=""0"" ></A><BR >")
                                                                Stream.Add((AStart + ("&dta=" 
                                                                                + (ToolsActionExpand + "\"><img src=\"/ccLib/images/LibButtonOpenUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A><BR >"))));
                                                                Stream.Add((AStart + ("&dta=" 
                                                                                + (ToolsActionContract + "\"><img src=\"/ccLib/images/LibButtonCloseUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A>"))));
                                                                Stream.Add("</span></td>");
                                                            }
                                                            
                                                            Stream.Add("</tr>");
                                                            Stream.Add("</table>");
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  ----- If anything was inherited, put up the message
                                                    // 
                                                    if ((InheritedFieldCount > 0)) {
                                                        Stream.Add(@"<p class=""ccNormal"">* This field was inherited from the Content Definition's Parent. Inherited fields will automatically change when the field in the parent is changed. If you alter these settings, this connection will be broken, and the field will no longer inherit it's properties.</P class=""ccNormal"">");
                                                    }
                                                    
                                                    // 
                                                    //  ----- now output a list of fields to add
                                                    // 
                                                    if ((CDef.fields.Count == 0)) {
                                                        Stream.Add((SpanClassAdminNormal + "This Content Definition has no fields</span><br>"));
                                                    }
                                                    else {
                                                        Stream.Add((SpanClassAdminNormal + "<br>"));
                                                        foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                            Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                            // With...
                                                            // 
                                                            //  display the column if it is not in use
                                                            // 
                                                            if (!IndexConfig.Columns.ContainsKey(field.nameLc)) {
                                                                if (false) {
                                                                    //  this causes more problems then it fixes
                                                                    // If Not .Authorable Then
                                                                    // 
                                                                    //  not authorable
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (not authorable field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFile)) {
                                                                    // 
                                                                    //  file can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileText)) {
                                                                    // 
                                                                    //  filename can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (text file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileHTML)) {
                                                                    // 
                                                                    //  filename can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (html file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileCSS)) {
                                                                    // 
                                                                    //  css filename can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (css file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileXML)) {
                                                                    // 
                                                                    //  xml filename can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (xml file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileJavascript)) {
                                                                    // 
                                                                    //  javascript filename can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (javascript file field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdLongText)) {
                                                                    // 
                                                                    //  long text can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (long text field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdHTML)) {
                                                                    // 
                                                                    //  long text can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (long text field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdFileImage)) {
                                                                    // 
                                                                    //  long text can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (image field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdRedirect)) {
                                                                    // 
                                                                    //  long text can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (redirect field)<br>")));
                                                                }
                                                                else if ((field.fieldTypeId == FieldTypeIdManyToMany)) {
                                                                    // 
                                                                    //  many to many can not be search
                                                                    // 
                                                                    Stream.Add(("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"50\" height=\"15\" border=\"0\" > " 
                                                                                    + (field.caption + " (many-to-many field)<br>")));
                                                                }
                                                                else {
                                                                    // 
                                                                    //  can be used as column header
                                                                    // 
                                                                    Stream.Add(("<a href=\"?" 
                                                                                    + (cpCore.doc.refreshQueryString + ("&fi=" 
                                                                                    + (field.id + ("&dta=" 
                                                                                    + (ToolsActionAddField + ("&" 
                                                                                    + (RequestNameAddFieldID + ("=" 
                                                                                    + (field.id + ("&" 
                                                                                    + (RequestNameAdminSubForm + ("=" 
                                                                                    + (AdminFormIndex_SubFormSetColumns + ("\"><img src=\"/ccLib/images/LibButtonAddUp.gif\" width=\"50\" height=\"15\" border=\"0\" ></A> " 
                                                                                    + (field.caption + "<br>")))))))))))))))));
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                // --------------------------------------------------------------------------------
                                                //  print the content tables that have index forms to Configure
                                                // --------------------------------------------------------------------------------
                                                // 
                                                // FormPanel = FormPanel & SpanClassAdminNormal & "Select a Content Definition to Configure its index form<br >"
                                                // 'FormPanel = FormPanel & cpCore.main_GetFormInputHidden("af", AdminFormToolConfigureIndex)
                                                // FormPanel = FormPanel & cpCore.htmldoc.main_GetFormInputSelect2("ContentID", ContentID, "Content")
                                                // Call Stream.Add(cpcore.htmldoc.main_GetPanel(FormPanel))
                                                // '
                                                cpCore.siteProperties.setProperty("AllowContentAutoLoad", genericController.encodeText(AllowContentAutoLoad));
                                                // Stream.Add( cpCore.main_GetFormInputHidden("NeedToReloadConfig", NeedToReloadConfig))
                                                Content = ("" 
                                                            + (Stream.Text 
                                                            + (cpCore.html.html_GetFormInputHidden(RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns) + "")));
                                                GetForm_Index_SetColumns = Adminui.GetBody(Title, (ButtonOK + ("," + ButtonReset)), "", false, false, Description, "", 10, Content);
                                                // 
                                                cpCore.html.addTitle(Title);
                                                // TODO: Exit Function: Warning!!! Need to return the value
                                                return;
                                                // 
                                                //  ----- Error Trap
                                                // 
                                            ErrorTrap:
                                                handleLegacyClassError3("GetForm_Index_SetColumns");
                                                // 
                                                // ========================================================================
                                                // 
                                                // ========================================================================
                                                // 
                                                TurnOnLinkAlias(((bool)(UseContentWatchLink)));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 'Dim th as integer : th = profileLogAdminMethodEnter("TurnOnLinkAlias")
                                                // 
                                                int CS;
                                                string ErrorList;
                                                string linkAlias;
                                                // 
                                                if ((cpCore.doc.debug_iUserError != "")) {
                                                    errorController.error_AddUserError(cpCore, "Existing pages could not be checked for Link Alias names because there was another error on this page" +
                                                        ". Correct this error, and turn Link Alias on again to rerun the verification.");
                                                }
                                                else {
                                                    CS = cpCore.db.csOpen("Page Content");
                                                    while (cpCore.db.csOk(CS)) {
                                                        // 
                                                        //  Add the link alias
                                                        // 
                                                        linkAlias = cpCore.db.csGetText(CS, "LinkAlias");
                                                        if ((linkAlias != "")) {
                                                            // 
                                                            //  Add the link alias
                                                            // 
                                                            docController.addLinkAlias(cpCore, linkAlias, cpCore.db.csGetInteger(CS, "ID"), "", false, true);
                                                        }
                                                        else {
                                                            // 
                                                            //  Add the name
                                                            // 
                                                            linkAlias = cpCore.db.csGetText(CS, "name");
                                                            if ((linkAlias != "")) {
                                                                docController.addLinkAlias(cpCore, linkAlias, cpCore.db.csGetInteger(CS, "ID"), "", false, false);
                                                            }
                                                            
                                                        }
                                                        
                                                        // 
                                                        cpCore.db.csGoNext(CS);
                                                    }
                                                    
                                                    cpCore.db.csClose(CS);
                                                    if ((cpCore.doc.debug_iUserError != "")) {
                                                        // 
                                                        //  Throw out all the details of what happened, and add one simple error
                                                        // 
                                                        ErrorList = errorController.error_GetUserError(cpCore);
                                                        ErrorList = genericController.vbReplace(ErrorList, UserErrorHeadline, "", 1, 99, vbTextCompare);
                                                        errorController.error_AddUserError(cpCore, ("The following errors occurred while verifying Link Alias entries for your existing pages." + ErrorList));
                                                        // Call cpCore.htmldoc.main_AddUserError(ErrorList)
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                // 
                                                return;
                                                // 
                                                //  ----- Error Trap
                                                // 
                                            ErrorTrap:
                                                handleLegacyClassError3("TurnOnLinkAlias");
                                                // 
                                                // ========================================================================
                                                //    Editor features are stored in the \config\EditorFeatures.txt file
                                                //    This is a crlf delimited list, with each row including:
                                                //        admin:featurelist
                                                //        contentmanager:featurelist
                                                //        public:featurelist
                                                // ========================================================================
                                                // 
                                                ((string)(GetForm_EditConfig()));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 'Dim th as integer : th = profileLogAdminMethodEnter("GetForm_EditConfig")
                                                // 
                                                int CS;
                                                string EditorStyleRulesFilename;
                                                int Pos;
                                                int SrcPtr;
                                                string[] FeatureDetails;
                                                bool AllowAdmin;
                                                bool AllowCM;
                                                bool AllowPublic;
                                                int RowPtr;
                                                string AdminList = "";
                                                string CMList = "";
                                                string PublicList = "";
                                                string TDLeft;
                                                string TDCenter;
                                                int Ptr;
                                                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                                                string Button;
                                                string Copy;
                                                string ButtonList;
                                                adminUIController Adminui = new adminUIController(cpCore);
                                                string Caption;
                                                string Description;
                                                int StyleSN;
                                                string TBConfig;
                                                string[] TBArray;
                                                string[] DefaultFeatures;
                                                string FeatureName;
                                                string FeatureList;
                                                string[] Features;
                                                // 
                                                DefaultFeatures = InnovaEditorFeatureList.Split(",");
                                                Description = @"This tool is used to configure the wysiwyg content editor for different uses. Check the Administrator column if you want administrators to have access to this feature when editing a page. Check the Content Manager column to allow non-admins to have access to this feature. Check the Public column if you want those on the public site to have access to the feature when the editor is used for public forms.";
                                                Button = cpCore.docProperties.getText(RequestNameButton);
                                                if ((Button == ButtonCancel)) {
                                                    // 
                                                    //  Cancel button pressed, return with nothing goes to root form
                                                    // 
                                                    // Call cpCore.main_Redirect2(cpCore.app.SiteProperty_AdminURL, "EditConfig, Cancel Button Pressed")
                                                }
                                                else {
                                                    // 
                                                    //  From here down will return a form
                                                    // 
                                                    if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                                        // 
                                                        //  Does not have permission
                                                        // 
                                                        ButtonList = ButtonCancel;
                                                        Content.Add(Adminui.GetFormBodyAdminOnly());
                                                        cpCore.html.addTitle("Style Editor");
                                                        GetForm_EditConfig = Adminui.GetBody("Site Styles", ButtonList, "", true, true, Description, "", 0, Content.Text);
                                                    }
                                                    else {
                                                        // 
                                                        //  OK to see and use this form
                                                        // 
                                                        if (((Button == ButtonSave) 
                                                                    || (Button == ButtonOK))) {
                                                            // 
                                                            //  Save the Previous edits
                                                            // 
                                                            cpCore.siteProperties.setProperty("Editor Background Color", cpCore.docProperties.getText("editorbackgroundcolor"));
                                                            // 
                                                            for (Ptr = 0; (Ptr <= UBound(DefaultFeatures)); Ptr++) {
                                                                FeatureName = DefaultFeatures[Ptr];
                                                                if ((genericController.vbLCase(FeatureName) == "styleandformatting")) {
                                                                    // 
                                                                    //  must always be on or it throws js error (editor bug I guess)
                                                                    // 
                                                                    AdminList = (AdminList + ("," + FeatureName));
                                                                    CMList = (CMList + ("," + FeatureName));
                                                                    PublicList = (PublicList + ("," + FeatureName));
                                                                }
                                                                else {
                                                                    if (cpCore.docProperties.getBoolean((FeatureName + ".admin"))) {
                                                                        AdminList = (AdminList + ("," + FeatureName));
                                                                    }
                                                                    
                                                                    if (cpCore.docProperties.getBoolean((FeatureName + ".cm"))) {
                                                                        CMList = (CMList + ("," + FeatureName));
                                                                    }
                                                                    
                                                                    if (cpCore.docProperties.getBoolean((FeatureName + ".public"))) {
                                                                        PublicList = (PublicList + ("," + FeatureName));
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                            cpCore.privateFiles.saveFile(InnovaEditorFeaturefilename, ("admin:" 
                                                                            + (AdminList + ("\r\n" + ("contentmanager:" 
                                                                            + (CMList + ("\r\n" + ("public:" + PublicList))))))));
                                                            // 
                                                            //  Clear the editor style rules template cache so next edit gets new background color
                                                            // 
                                                            EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", "0", 1, 99, vbTextCompare);
                                                            cpCore.privateFiles.deleteFile(EditorStyleRulesFilename);
                                                            // 
                                                            CS = cpCore.db.csOpenSql_rev("default", "select id from cctemplates");
                                                            while (cpCore.db.csOk(CS)) {
                                                                EditorStyleRulesFilename = genericController.vbReplace(EditorStyleRulesFilenamePattern, "$templateid$", cpCore.db.csGet(CS, "ID"), 1, 99, vbTextCompare);
                                                                cpCore.privateFiles.deleteFile(EditorStyleRulesFilename);
                                                                cpCore.db.csGoNext(CS);
                                                            }
                                                            
                                                            cpCore.db.csClose(CS);
                                                        }
                                                        
                                                        // 
                                                        if ((Button == ButtonOK)) {
                                                            // 
                                                            //  exit with blank page
                                                            // 
                                                        }
                                                        else {
                                                            // 
                                                            //  Draw the form
                                                            // 
                                                            FeatureList = cpCore.cdnFiles.readFile(InnovaEditorFeaturefilename);
                                                            // If FeatureList = "" Then
                                                            //     FeatureList = cpCore.cluster.localClusterFiles.readFile("ccLib\" & "Config\DefaultEditorConfig.txt")
                                                            //     Call cpCore.privateFiles.saveFile(InnovaEditorFeaturefilename, FeatureList)
                                                            // End If
                                                            if ((FeatureList == "")) {
                                                                FeatureList = ("admin:" 
                                                                            + (InnovaEditorFeatureList + ("\r\n" + ("contentmanager:" 
                                                                            + (InnovaEditorFeatureList + ("\r\n" + ("public:" + InnovaEditorPublicFeatureList)))))));
                                                            }
                                                            
                                                            if ((FeatureList != "")) {
                                                                Features = FeatureList.Split("\r\n");
                                                                AdminList = genericController.vbReplace(Features[0], "admin:", "", 1, 99, vbTextCompare);
                                                                if ((UBound(Features) > 0)) {
                                                                    CMList = genericController.vbReplace(Features[1], "contentmanager:", "", 1, 99, vbTextCompare);
                                                                    if ((UBound(Features) > 1)) {
                                                                        PublicList = genericController.vbReplace(Features[2], "public:", "", 1, 99, vbTextCompare);
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                            // 
                                                            Copy = ("\r\n" + ("<tr class=\"ccAdminListCaption\">" + ("<td align=left style=\"width:200;\">Feature</td>" + ("<td align=center style=\"width:100;\">Administrators</td>" + ("<td align=center style=\"width:100;\">Content&nbsp;Managers</td>" + ("<td align=center style=\"width:100;\">Public</td>" + "</tr>"))))));
                                                            RowPtr = 0;
                                                            for (Ptr = 0; (Ptr <= UBound(DefaultFeatures)); Ptr++) {
                                                                FeatureName = DefaultFeatures[Ptr];
                                                                if ((genericController.vbLCase(FeatureName) == "styleandformatting")) {
                                                                    // 
                                                                    //  hide and force on during process - editor bug I think.
                                                                    // 
                                                                }
                                                                else {
                                                                    TDLeft = genericController.StartTableCell(,, bool.Parse((RowPtr % 2)), "left");
                                                                    TDCenter = genericController.StartTableCell(,, bool.Parse((RowPtr % 2)), "center");
                                                                    AllowAdmin = genericController.EncodeBoolean((("," 
                                                                                    + (AdminList + ",")).IndexOf(("," 
                                                                                        + (FeatureName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1));
                                                                    AllowCM = genericController.EncodeBoolean((("," 
                                                                                    + (CMList + ",")).IndexOf(("," 
                                                                                        + (FeatureName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1));
                                                                    AllowPublic = genericController.EncodeBoolean((("," 
                                                                                    + (PublicList + ",")).IndexOf(("," 
                                                                                        + (FeatureName + ",")), 0, System.StringComparison.OrdinalIgnoreCase) + 1));
                                                                    Copy = (Copy + ("\r\n" + ("<tr>" 
                                                                                + (TDLeft 
                                                                                + (FeatureName + ("</td>" 
                                                                                + (TDCenter 
                                                                                + (cpCore.html.html_GetFormInputCheckBox2((FeatureName + ".admin"), AllowAdmin) + ("</td>" 
                                                                                + (TDCenter 
                                                                                + (cpCore.html.html_GetFormInputCheckBox2((FeatureName + ".cm"), AllowCM) + ("</td>" 
                                                                                + (TDCenter 
                                                                                + (cpCore.html.html_GetFormInputCheckBox2((FeatureName + ".public"), AllowPublic) + ("</td>" + "</tr>")))))))))))))));
                                                                    RowPtr = (RowPtr + 1);
                                                                }
                                                                
                                                            }
                                                            
                                                            Copy = ("" + ("\r\n" + ("<div><b>body background style color</b> (default=\'white\')</div>" + ("\r\n" + ("<div>" 
                                                                        + (cpCore.html.html_GetFormInputText2("editorbackgroundcolor", cpCore.siteProperties.getText("Editor Background Color", "white")) + ("</div>" + ("\r\n" + ("<div>&nbsp;</div>" + ("\r\n" + ("<div><b>Toolbar features available</b></div>" + ("\r\n" + ("<table border=\"0\" cellpadding=\"4\" cellspacing=\"0\" width=\"500px\" align=left>" 
                                                                        + (genericController.htmlIndent(Copy) + ("\r\n" + kmaEndTable)))))))))))))));
                                                            Copy = ("\r\n" 
                                                                        + (genericController.StartTable(20, 0, 0) + ("<tr><td>" 
                                                                        + (genericController.htmlIndent(Copy) + ("</td></tr>" + ("\r\n" + kmaEndTable))))));
                                                            Content.Add(Copy);
                                                            ButtonList = (ButtonCancel + ("," 
                                                                        + (ButtonRefresh + ("," 
                                                                        + (ButtonSave + ("," + ButtonOK))))));
                                                            Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormEditorConfig));
                                                            cpCore.html.addTitle("Editor Settings");
                                                            GetForm_EditConfig = Adminui.GetBody("Editor Configuration", ButtonList, "", true, true, Description, "", 0, Content.Text);
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                }
                                                
                                                // TODO: Exit Function: Warning!!! Need to return the value
                                                return;
                                                // 
                                                //  ----- Error Trap
                                                // 
                                            ErrorTrap:
                                                handleLegacyClassError3("GetForm_EditConfig");
                                                // 
                                                // 
                                                // ========================================================================
                                                //  Page Content Settings Page
                                                // ========================================================================
                                                // 
                                                ((string)(GetForm_BuildCollection()));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_BuildCollection")
                                                // 
                                                string Description;
                                                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                                                string Button;
                                                adminUIController Adminui = new adminUIController(cpCore);
                                                string ButtonList;
                                                bool AllowAutoLogin;
                                                string Copy;
                                                // 
                                                Button = cpCore.docProperties.getText(RequestNameButton);
                                                if ((Button == ButtonCancel)) {
                                                    // 
                                                    //  Cancel just exits with no content
                                                    // 
                                                    // TODO: Exit Function: Warning!!! Need to return the value
                                                    return;
                                                }
                                                else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                                    // 
                                                    //  Not Admin Error
                                                    // 
                                                    ButtonList = ButtonCancel;
                                                    Content.Add(Adminui.GetFormBodyAdminOnly());
                                                }
                                                else {
                                                    Content.Add(Adminui.EditTableOpen);
                                                    // 
                                                    //  Set defaults
                                                    // 
                                                    AllowAutoLogin = cpCore.siteProperties.getBoolean("AllowAutoLogin", true);
                                                    switch (Button) {
                                                        case ButtonSave:
                                                        case ButtonOK:
                                                            // 
                                                            // 
                                                            // 
                                                            AllowAutoLogin = cpCore.docProperties.getBoolean("AllowAutoLogin");
                                                            // 
                                                            cpCore.siteProperties.setProperty("AllowAutoLogin", genericController.encodeText(AllowAutoLogin));
                                                            break;
                                                    }
                                                    if ((Button == ButtonOK)) {
                                                        // 
                                                        //  Exit on OK or cancel
                                                        // 
                                                        // TODO: Exit Function: Warning!!! Need to return the value
                                                        return;
                                                    }
                                                    
                                                    // 
                                                    //  List Add-ons to include
                                                    // 
                                                    Copy = cpCore.html.html_GetFormInputCheckBox2("AllowAutoLogin", AllowAutoLogin);
                                                    Copy = (Copy + @"<div>When checked, returning users are automatically logged-in, without requiring a username or password. This is very convenient, but creates a high security risk. Each time you login, you will be given the option to not allow Auto-Login from that computer.</div>");
                                                    Content.Add(Adminui.GetEditRow(Copy, "Allow Auto Login", "", false, false, ""));
                                                    // 
                                                    //  Buttons
                                                    // 
                                                    ButtonList = (ButtonCancel + ("," 
                                                                + (ButtonSave + ("," + ButtonOK))));
                                                    // 
                                                    //  Close Tables
                                                    // 
                                                    Content.Add(Adminui.EditTableClose);
                                                    Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormBuilderCollection));
                                                }
                                                
                                                // 
                                                Description = "Use this tool to modify the site security settings";
                                                GetForm_BuildCollection = Adminui.GetBody("Security Settings", ButtonList, "", true, true, Description, "", 0, Content.Text);
                                                Content = null;
                                            ErrorTrap:
                                                Content = null;
                                                handleLegacyClassError3("GetForm_BuildCollection");
                                                // 
                                                // 
                                                // 
                                                // ========================================================================
                                                // 
                                                // ========================================================================
                                                // 
                                                ((string)(GetForm_ClearCache()));
                                                string returnHtml = "";
                                                try {
                                                    stringBuilderLegacyController Content = new stringBuilderLegacyController();
                                                    string Button;
                                                    adminUIController Adminui = new adminUIController(cpCore);
                                                    string Description;
                                                    string ButtonList;
                                                    // 
                                                    Button = cpCore.docProperties.getText(RequestNameButton);
                                                    if ((Button == ButtonCancel)) {
                                                        // 
                                                        //  Cancel just exits with no content
                                                        // 
                                                        return "";
                                                    }
                                                    else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                                        // 
                                                        //  Not Admin Error
                                                        // 
                                                        ButtonList = ButtonCancel;
                                                        Content.Add(Adminui.GetFormBodyAdminOnly());
                                                    }
                                                    else {
                                                        Content.Add(Adminui.EditTableOpen);
                                                        // 
                                                        //  Set defaults
                                                        // 
                                                        // 
                                                        //  Process Requests
                                                        // 
                                                        switch (Button) {
                                                            case ButtonApply:
                                                            case ButtonOK:
                                                                // 
                                                                //  Clear the cache
                                                                // 
                                                                cpCore.cache.invalidateAll();
                                                                break;
                                                        }
                                                        if ((Button == ButtonOK)) {
                                                            // 
                                                            //  Exit on OK or cancel
                                                            // 
                                                            return "";
                                                        }
                                                        
                                                        // 
                                                        //  Buttons
                                                        // 
                                                        ButtonList = (ButtonCancel + ("," 
                                                                    + (ButtonApply + ("," + ButtonOK))));
                                                        // 
                                                        //  Close Tables
                                                        // 
                                                        Content.Add(Adminui.EditTableClose);
                                                        Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormClearCache));
                                                    }
                                                    
                                                    // 
                                                    Description = "Hit Apply or OK to clear all current content caches";
                                                    returnHtml = Adminui.GetBody("Clear Cache", ButtonList, "", true, true, Description, "", 0, Content.Text);
                                                    Content = null;
                                                }
                                                catch (Exception ex) {
                                                    cpCore.handleException(ex);
                                                    throw;
                                                }
                                                
                                                return returnHtml;
                                                // 
                                                // ========================================================================
                                                //  Tool to enter multiple Meta Keywords
                                                // ========================================================================
                                                // 
                                                ((string)(GetForm_MetaKeywordTool()));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // Dim th as integer: th = profileLogAdminMethodEnter( "GetForm_MetaKeywordTool")
                                                // 
                                                const object LoginMode_None = 1;
                                                const object LoginMode_AutoRecognize = 2;
                                                const object LoginMode_AutoLogin = 3;
                                                // 
                                                int LoginMode;
                                                string Help;
                                                stringBuilderLegacyController Content = new stringBuilderLegacyController();
                                                string Copy;
                                                string Button;
                                                string PageNotFoundPageID;
                                                adminUIController Adminui = new adminUIController(cpCore);
                                                string Description;
                                                string ButtonList;
                                                bool AllowLinkAlias;
                                                // Dim AllowExternalLinksInChildList As Boolean
                                                bool LinkForwardAutoInsert;
                                                string SectionLandingLink;
                                                string ServerPageDefault;
                                                string LandingPageID;
                                                string DocTypeDeclaration;
                                                bool AllowAutoRecognize;
                                                string KeywordList;
                                                // 
                                                Button = cpCore.docProperties.getText(RequestNameButton);
                                                if ((Button == ButtonCancel)) {
                                                    // 
                                                    //  Cancel just exits with no content
                                                    // 
                                                    // TODO: Exit Function: Warning!!! Need to return the value
                                                    return;
                                                }
                                                else if (!cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) {
                                                    // 
                                                    //  Not Admin Error
                                                    // 
                                                    ButtonList = ButtonCancel;
                                                    Content.Add(Adminui.GetFormBodyAdminOnly());
                                                }
                                                else {
                                                    Content.Add(Adminui.EditTableOpen);
                                                    // 
                                                    //  Process Requests
                                                    // 
                                                    switch (Button) {
                                                        case ButtonSave:
                                                        case ButtonOK:
                                                            // 
                                                            string[] Keywords;
                                                            string Keyword;
                                                            int Cnt;
                                                            int Ptr;
                                                            DataTable dt;
                                                            int CS;
                                                            KeywordList = cpCore.docProperties.getText("KeywordList");
                                                            if ((KeywordList != "")) {
                                                                KeywordList = genericController.vbReplace(KeywordList, "\r\n", ",");
                                                                Keywords = KeywordList.Split(",");
                                                                Cnt = (UBound(Keywords) + 1);
                                                                for (Ptr = 0; (Ptr 
                                                                            <= (Cnt - 1)); Ptr++) {
                                                                    Keyword = Keywords[Ptr].Trim();
                                                                    if ((Keyword != "")) {
                                                                        // Dim dt As DataTable
                                                                        dt = cpCore.db.executeQuery(("select top 1 ID from ccMetaKeywords where name=" + cpCore.db.encodeSQLText(Keyword)));
                                                                        if ((dt.Rows.Count == 0)) {
                                                                            CS = cpCore.db.csInsertRecord("Meta Keywords");
                                                                            if (cpCore.db.csOk(CS)) {
                                                                                cpCore.db.csSet(CS, "name", Keyword);
                                                                            }
                                                                            
                                                                            cpCore.db.csClose(CS);
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                            break;
                                                    }
                                                    if ((Button == ButtonOK)) {
                                                        // 
                                                        //  Exit on OK or cancel
                                                        // 
                                                        // TODO: Exit Function: Warning!!! Need to return the value
                                                        return;
                                                    }
                                                    
                                                    // 
                                                    //  KeywordList
                                                    // 
                                                    Copy = cpCore.html.html_GetFormInputTextExpandable("KeywordList", ,, 10);
                                                    Copy = (Copy + "<div>Paste your Meta Keywords into this text box, separated by either commas or enter keys. When you " +
                                                    "hit Save or OK, Meta Keyword records will be made out of each word. These can then be checked on any" +
                                                    " content page.</div>");
                                                    Content.Add(Adminui.GetEditRow(Copy, "Paste Meta Keywords", "", false, false, ""));
                                                    // 
                                                    //  Buttons
                                                    // 
                                                    ButtonList = (ButtonCancel + ("," 
                                                                + (ButtonSave + ("," + ButtonOK))));
                                                    // 
                                                    //  Close Tables
                                                    // 
                                                    Content.Add(Adminui.EditTableClose);
                                                    Content.Add(cpCore.html.html_GetFormInputHidden(RequestNameAdminSourceForm, AdminFormSecurityControl));
                                                }
                                                
                                                // 
                                                Description = "Use this tool to enter multiple Meta Keywords";
                                                GetForm_MetaKeywordTool = Adminui.GetBody("Meta Keyword Entry Tool", ButtonList, "", true, true, Description, "", 0, Content.Text);
                                                Content = null;
                                            ErrorTrap:
                                                Content = null;
                                                handleLegacyClassError3("GetForm_MetaKeywordTool");
                                                // 
                                                // 
                                                // 
                                                // 
                                                ((bool)(AllowAdminFieldCheck()));
                                                if (!AllowAdminFieldCheck_LocalLoaded) {
                                                    AllowAdminFieldCheck_LocalLoaded = true;
                                                    AllowAdminFieldCheck_Local = cpCore.siteProperties.getBoolean("AllowAdminFieldCheck", true);
                                                }
                                                
                                                AllowAdminFieldCheck = AllowAdminFieldCheck_Local;
                                                // 
                                                // 
                                                // 
                                                ((string)(GetAddonHelp(((int)(HelpAddonID)), ((string)(UsedIDString)))));
                                                string addonHelp = "";
                                                try {
                                                    string IconFilename;
                                                    int IconWidth;
                                                    int IconHeight;
                                                    int IconSprites;
                                                    bool IconIsInline;
                                                    int CS;
                                                    string AddonName = "";
                                                    string AddonHelpCopy = "";
                                                    DateTime AddonDateAdded;
                                                    DateTime AddonLastUpdated;
                                                    string SQL;
                                                    string IncludeHelp = "";
                                                    int IncludeID;
                                                    string IconImg = "";
                                                    string helpLink = "";
                                                    bool FoundAddon;
                                                    // 
                                                    if ((genericController.vbInstr(1, ("," 
                                                                    + (UsedIDString + ",")), ("," 
                                                                    + (HelpAddonID.ToString() + ","))) == 0)) {
                                                        CS = cpCore.db.csOpenRecord(cnAddons, HelpAddonID);
                                                        if (cpCore.db.csOk(CS)) {
                                                            FoundAddon = true;
                                                            AddonName = cpCore.db.csGet(CS, "Name");
                                                            AddonHelpCopy = cpCore.db.csGet(CS, "help");
                                                            AddonDateAdded = cpCore.db.csGetDate(CS, "dateadded");
                                                            if (Models.Complex.cdefModel.isContentFieldSupported(cpCore, cnAddons, "lastupdated")) {
                                                                AddonLastUpdated = cpCore.db.csGetDate(CS, "lastupdated");
                                                            }
                                                            
                                                            returnIndexConfig.MinValue;
                                                            AddonLastUpdated = AddonDateAdded;
                                                        }
                                                        
                                                        IconFilename = cpCore.db.csGet(CS, "Iconfilename");
                                                        IconWidth = cpCore.db.csGetInteger(CS, "IconWidth");
                                                        IconHeight = cpCore.db.csGetInteger(CS, "IconHeight");
                                                        IconSprites = cpCore.db.csGetInteger(CS, "IconSprites");
                                                        IconIsInline = cpCore.db.csGetBoolean(CS, "IsInline");
                                                        IconImg = genericController.GetAddonIconImg(("/" + cpCore.serverConfig.appConfig.adminRoute), IconWidth, IconHeight, IconSprites, IconIsInline, "", IconFilename, cpCore.serverConfig.appConfig.cdnFilesNetprefix, AddonName, AddonName, "", 0);
                                                        helpLink = cpCore.db.csGet(CS, "helpLink");
                                                    }
                                                    
                                                    cpCore.db.csClose(CS);
                                                    // 
                                                    if (FoundAddon) {
                                                        // 
                                                        //  Included Addons
                                                        // 
                                                        SQL = ("select IncludedAddonID from ccAddonIncludeRules where AddonID=" + HelpAddonID);
                                                        CS = cpCore.db.csOpenSql_rev("default", SQL);
                                                        while (cpCore.db.csOk(CS)) {
                                                            IncludeID = cpCore.db.csGetInteger(CS, "IncludedAddonID");
                                                            IncludeHelp = (IncludeHelp + GetAddonHelp(IncludeID, (HelpAddonID + ("," + IncludeID.ToString()))));
                                                            cpCore.db.csGoNext(CS);
                                                        }
                                                        
                                                        cpCore.db.csClose(CS);
                                                        // 
                                                        if ((helpLink != "")) {
                                                            if ((AddonHelpCopy != "")) {
                                                                AddonHelpCopy = (AddonHelpCopy + ("<p>For additional help with this add-on, please visit <a href=\"" 
                                                                            + (helpLink + ("\">" 
                                                                            + (helpLink + "</a>.</p>")))));
                                                            }
                                                            else {
                                                                AddonHelpCopy = (AddonHelpCopy + ("<p>For help with this add-on, please visit <a href=\"" 
                                                                            + (helpLink + ("\">" 
                                                                            + (helpLink + "</a>.</p>")))));
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((AddonHelpCopy == "")) {
                                                            AddonHelpCopy = (AddonHelpCopy + @"<p>Please refer to the help resources available for this collection. More information may also be available in the Contensive online Learning Center <a href=""http://support.contensive.com/Learning-Center"">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com for more information.</p>");
                                                        }
                                                        
                                                        addonHelp = ("" + ("<div class=\"ccHelpCon\">" + ("<div class=\"title\"><div style=\"float:right;\"><a href=\"?addonid=" 
                                                                    + (HelpAddonID + ("\">" 
                                                                    + (IconImg + ("</a></div>" 
                                                                    + (AddonName + (" Add-on</div>" + ("<div class=\"byline\">" + ("<div>Installed " 
                                                                    + (AddonDateAdded + ("</div>" + ("<div>Last Updated " 
                                                                    + (AddonLastUpdated + ("</div>" + ("</div>" + ("<div class=\"body\" style=\"clear:both;\">" 
                                                                    + (AddonHelpCopy + ("</div>" + "</div>"))))))))))))))))))));
                                                        addonHelp = (addonHelp + IncludeHelp);
                                                    }
                                                    
                                                }
                                                catch (Exception ex) {
                                                    cpCore.handleException(ex);
                                                    throw;
                                                }
                                                
                                                return addonHelp;
                                                // 
                                                // 
                                                // 
                                                ((string)(GetCollectionHelp(((int)(HelpCollectionID)), ((string)(UsedIDString)))));
                                                string returnHelp = "";
                                                try {
                                                    int CS;
                                                    string Collectionname = "";
                                                    string CollectionHelpCopy = "";
                                                    string CollectionHelpLink = "";
                                                    DateTime CollectionDateAdded;
                                                    DateTime CollectionLastUpdated;
                                                    string SQL;
                                                    string IncludeHelp = "";
                                                    int addonId;
                                                    // 
                                                    if ((genericController.vbInstr(1, ("," 
                                                                    + (UsedIDString + ",")), ("," 
                                                                    + (HelpCollectionID.ToString() + ","))) == 0)) {
                                                        CS = cpCore.db.csOpenRecord("Add-on Collections", HelpCollectionID);
                                                        if (cpCore.db.csOk(CS)) {
                                                            Collectionname = cpCore.db.csGet(CS, "Name");
                                                            CollectionHelpCopy = cpCore.db.csGet(CS, "help");
                                                            CollectionDateAdded = cpCore.db.csGetDate(CS, "dateadded");
                                                            if (Models.Complex.cdefModel.isContentFieldSupported(cpCore, "Add-on Collections", "lastupdated")) {
                                                                CollectionLastUpdated = cpCore.db.csGetDate(CS, "lastupdated");
                                                            }
                                                            
                                                            if (Models.Complex.cdefModel.isContentFieldSupported(cpCore, "Add-on Collections", "helplink")) {
                                                                CollectionHelpLink = cpCore.db.csGet(CS, "helplink");
                                                            }
                                                            
                                                            returnIndexConfig.MinValue;
                                                            CollectionLastUpdated = CollectionDateAdded;
                                                        }
                                                        
                                                    }
                                                    
                                                    cpCore.db.csClose(CS);
                                                    // 
                                                    //  Add-ons
                                                    // 
                                                    if (true) {
                                                        //  4.0.321" Then
                                                        // $$$$$ cache this
                                                        CS = cpCore.db.csOpen(cnAddons, ("CollectionID=" + HelpCollectionID), "name", ,, ,, "ID");
                                                        while (cpCore.db.csOk(CS)) {
                                                            IncludeHelp = (IncludeHelp + ("<div style=\"clear:both;\">" 
                                                                        + (GetAddonHelp(cpCore.db.csGetInteger(CS, "ID"), "") + "</div>")));
                                                            cpCore.db.csGoNext(CS);
                                                        }
                                                        
                                                        cpCore.db.csClose(CS);
                                                    }
                                                    else {
                                                        //  addoncollectionrules deprecated for collectionid
                                                        SQL = ("select AddonID from ccAddonCollectionRules where CollectionID=" + HelpCollectionID);
                                                        CS = cpCore.db.csOpenSql_rev("default", SQL);
                                                        while (cpCore.db.csOk(CS)) {
                                                            addonId = cpCore.db.csGetInteger(CS, "AddonID");
                                                            if ((addonId != 0)) {
                                                                IncludeHelp = (IncludeHelp + ("<div style=\"clear:both;\">" 
                                                                            + (GetAddonHelp(addonId, "") + "</div>")));
                                                            }
                                                            
                                                            cpCore.db.csGoNext(CS);
                                                        }
                                                        
                                                        cpCore.db.csClose(CS);
                                                    }
                                                    
                                                    // 
                                                    if (((CollectionHelpLink == "") 
                                                                && (CollectionHelpCopy == ""))) {
                                                        CollectionHelpCopy = @"<p>No help information could be found for this collection. Please use the online resources at <a href=""http://support.contensive.com/Learning-Center"">http://support.contensive.com/Learning-Center</a> or contact Contensive Support support@contensive.com by email.</p>";
                                                    }
                                                    else if ((CollectionHelpLink != "")) {
                                                        CollectionHelpCopy = ("" + ("<p>For information about this collection please visit <a href=\"" 
                                                                    + (CollectionHelpLink + ("\">" 
                                                                    + (CollectionHelpLink + ("</a>.</p>" + CollectionHelpCopy))))));
                                                    }
                                                    
                                                    // 
                                                    returnHelp = ("" + ("<div class=\"ccHelpCon\">" + ("<div class=\"title\">" 
                                                                + (Collectionname + (" Collection</div>" + ("<div class=\"byline\">" + ("<div>Installed " 
                                                                + (CollectionDateAdded + ("</div>" + ("<div>Last Updated " 
                                                                + (CollectionLastUpdated + ("</div>" + ("</div>" + ("<div class=\"body\">" 
                                                                + (CollectionHelpCopy + "</div>")))))))))))))));
                                                    if ((IncludeHelp != "")) {
                                                        returnHelp = (returnHelp + IncludeHelp);
                                                    }
                                                    
                                                    returnHelp = (returnHelp + "</div>");
                                                }
                                                catch (Exception ex) {
                                                    cpCore.handleException(ex);
                                                    throw;
                                                }
                                                
                                                return returnHelp;
                                                // 
                                                // 
                                                // 
                                                SetIndexSQL(((Models.Complex.cdefModel)(adminContent)), ((editRecordClass)(editRecord)), ((indexConfigClass)(IndexConfig)), ref ((bool)(Return_AllowAccess)), ref ((string)(return_sqlFieldList)), ref ((string)(return_sqlFrom)), ref ((string)(return_SQLWhere)), ref ((string)(return_SQLOrderBy)), ref ((bool)(return_IsLimitedToSubContent)), ref ((string)(return_ContentAccessLimitMessage)), ref ((Dictionary<string, bool>)(FieldUsedInColumns)), ((Dictionary<string, bool>)(IsLookupFieldValid)));
                                                try {
                                                    string LookupQuery;
                                                    string ContentName;
                                                    string SortFieldName;
                                                    // 
                                                    int LookupPtr;
                                                    string[] lookups;
                                                    string FindWordName;
                                                    string FindWordValue;
                                                    int FindMatchOption;
                                                    int WCount;
                                                    string SubContactList = "";
                                                    int ContentID;
                                                    int Pos;
                                                    int Cnt;
                                                    string[] ListSplit;
                                                    int SubContentCnt;
                                                    string list;
                                                    string SubQuery;
                                                    int GroupID;
                                                    string GroupName;
                                                    string JoinTablename;
                                                    // Dim FieldName As String
                                                    int Ptr;
                                                    bool IncludedInLeftJoin;
                                                    //   Dim SupportWorkflowFields As Boolean
                                                    int FieldPtr;
                                                    bool IncludedInColumns;
                                                    string LookupContentName;
                                                    // 'Dim arrayOfFields() As appServices_metaDataClass.CDefFieldClass
                                                    // 
                                                    Return_AllowAccess = true;
                                                    return_sqlFieldList = (return_sqlFieldList 
                                                                + (adminContent.ContentTableName + ".ID"));
                                                    return_sqlFrom = adminContent.ContentTableName;
                                                    foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                        // With...
                                                        FieldPtr = field.id;
                                                        //  quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                                        IncludedInColumns = false;
                                                        IncludedInLeftJoin = false;
                                                        if (!IsLookupFieldValid.ContainsKey(field.nameLc)) {
                                                            IsLookupFieldValid.Add(field.nameLc, false);
                                                        }
                                                        
                                                        if (!FieldUsedInColumns.ContainsKey(field.nameLc)) {
                                                            FieldUsedInColumns.Add(field.nameLc, false);
                                                        }
                                                        
                                                        // 
                                                        //  test if this field is one of the columns we are displaying
                                                        // 
                                                        IncludedInColumns = IndexConfig.Columns.ContainsKey(field.nameLc);
                                                        // 
                                                        //  disallow IncludedInColumns if a non-supported field type
                                                        // 
                                                        switch (field.fieldTypeId) {
                                                            case FieldTypeIdFileCSS:
                                                            case FieldTypeIdFile:
                                                            case FieldTypeIdFileImage:
                                                            case FieldTypeIdFileJavascript:
                                                            case FieldTypeIdLongText:
                                                            case FieldTypeIdManyToMany:
                                                            case FieldTypeIdRedirect:
                                                            case FieldTypeIdFileText:
                                                            case FieldTypeIdFileXML:
                                                            case FieldTypeIdHTML:
                                                            case FieldTypeIdFileHTML:
                                                                IncludedInColumns = false;
                                                                break;
                                                        }
                                                        // FieldName = genericController.vbLCase(.Name)
                                                        if (((field.fieldTypeId == FieldTypeIdMemberSelect) 
                                                                    || ((field.fieldTypeId == FieldTypeIdLookup) 
                                                                    && (field.lookupContentID != 0)))) {
                                                            // 
                                                            //  This is a lookup field -- test if IncludedInLeftJoins
                                                            // 
                                                            JoinTablename = "";
                                                            if ((field.fieldTypeId == FieldTypeIdMemberSelect)) {
                                                                LookupContentName = "people";
                                                            }
                                                            else {
                                                                LookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, field.lookupContentID);
                                                            }
                                                            
                                                            if ((LookupContentName != "")) {
                                                                JoinTablename = Models.Complex.cdefModel.getContentTablename(cpCore, LookupContentName);
                                                            }
                                                            
                                                            IncludedInLeftJoin = IncludedInColumns;
                                                            if ((IndexConfig.FindWords.Count > 0)) {
                                                                // 
                                                                //  test findwords
                                                                // 
                                                                if (IndexConfig.FindWords.ContainsKey(field.nameLc)) {
                                                                    if ((IndexConfig.FindWords(field.nameLc).MatchOption != FindWordMatchEnum.MatchIgnore)) {
                                                                        IncludedInLeftJoin = true;
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                            if ((!IncludedInLeftJoin 
                                                                        && (IndexConfig.Sorts.Count > 0))) {
                                                                // 
                                                                //  test sorts
                                                                // 
                                                                if (IndexConfig.Sorts.ContainsKey(field.nameLc.ToLower)) {
                                                                    IncludedInLeftJoin = true;
                                                                }
                                                                
                                                            }
                                                            
                                                            if (IncludedInLeftJoin) {
                                                                // 
                                                                //  include this lookup field
                                                                // 
                                                                FieldUsedInColumns.Item[field.nameLc] = true;
                                                                if ((JoinTablename != "")) {
                                                                    IsLookupFieldValid[field.nameLc] = true;
                                                                    return_sqlFieldList = (return_sqlFieldList + (", LookupTable" 
                                                                                + (FieldPtr + (".Name AS LookupTable" 
                                                                                + (FieldPtr + "Name")))));
                                                                    return_sqlFrom = ("(" 
                                                                                + (return_sqlFrom + (" LEFT JOIN " 
                                                                                + (JoinTablename + (" AS LookupTable" 
                                                                                + (FieldPtr + (" ON " 
                                                                                + (adminContent.ContentTableName + ("." 
                                                                                + (field.nameLc + (" = LookupTable" 
                                                                                + (FieldPtr + ".ID)"))))))))))));
                                                                }
                                                                
                                                                // End If
                                                            }
                                                            
                                                        }
                                                        
                                                        if (IncludedInColumns) {
                                                            // 
                                                            //  This field is included in the columns, so include it in the select
                                                            // 
                                                            return_sqlFieldList = (return_sqlFieldList + (" ," 
                                                                        + (adminContent.ContentTableName + ("." + field.nameLc))));
                                                            FieldUsedInColumns[field.nameLc] = true;
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Sub CDef filter
                                                    // 
                                                    // With...
                                                    if ((IndexConfig.SubCDefID > 0)) {
                                                        ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, IndexConfig.SubCDefID);
                                                        ("AND(" 
                                                                    + (Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName) + ")"));
                                                    }
                                                    
                                                    // 
                                                    //  Return_sqlFrom and Where Clause for Groups filter
                                                    // 
                                                    DateTime rightNow = DateTime.Now();
                                                    string sqlRightNow = cpCore.db.encodeSQLDate(rightNow);
                                                    if ((adminContent.ContentTableName.ToLower == "ccmembers")) {
                                                        // With...
                                                        if ((IndexConfig.GroupListCnt > 0)) {
                                                            for (Ptr = 0; (Ptr 
                                                                        <= (IndexConfig.GroupListCnt - 1)); Ptr++) {
                                                                GroupName = IndexConfig.GroupList;
                                                                Ptr;
                                                                if ((GroupName != "")) {
                                                                    GroupID = cpCore.db.getRecordID("Groups", GroupName);
                                                                    if (((GroupID == 0) 
                                                                                && genericController.vbIsNumeric(GroupName))) {
                                                                        GroupID = genericController.EncodeInteger(GroupName);
                                                                    }
                                                                    
                                                                    string groupTableAlias = ("GroupFilter" + Ptr);
                                                                    ("AND(" 
                                                                                + (groupTableAlias + (".GroupID=" 
                                                                                + (GroupID + (")and((" 
                                                                                + (groupTableAlias + (".dateExpires is null)or(" 
                                                                                + (groupTableAlias + (".dateExpires>" 
                                                                                + (sqlRightNow + "))"))))))))));
                                                                    return_sqlFrom = ("(" 
                                                                                + (return_sqlFrom + (" INNER JOIN ccMemberRules AS GroupFilter" 
                                                                                + (Ptr + (" ON GroupFilter" 
                                                                                + (Ptr + ".MemberID=ccMembers.ID)"))))));
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Add Name into Return_sqlFieldList
                                                    // 
                                                    // If Not SQLSelectIncludesName Then
                                                    //  SQLSelectIncludesName is declared, but not initialized
                                                    return_sqlFieldList = (return_sqlFieldList + (" ," 
                                                                + (adminContent.ContentTableName + ".Name")));
                                                    if (userHasContentAccess(adminContent.Id)) {
                                                        // 
                                                        //  This person can see all the records
                                                        // 
                                                        ("AND(" 
                                                                    + (Models.Complex.cdefModel.getContentControlCriteria(cpCore, adminContent.Name) + ")"));
                                                    }
                                                    else {
                                                        // 
                                                        //  Limit the Query to what they can see
                                                        // 
                                                        return_IsLimitedToSubContent = true;
                                                        SubQuery = "";
                                                        list = adminContent.ContentControlCriteria;
                                                        adminContent.Id = adminContent.Id;
                                                        SubContentCnt = 0;
                                                        if ((list != "")) {
                                                            Console.WriteLine(("console - adminContent.contentControlCriteria=" + list));
                                                            Debug.WriteLine(("debug - adminContent.contentControlCriteria=" + list));
                                                            logController.appendLog(cpCore, ("appendlog - adminContent.contentControlCriteria=" + list));
                                                            ListSplit = list.Split("=");
                                                            Cnt = (UBound(ListSplit) + 1);
                                                            if ((Cnt > 0)) {
                                                                for (Ptr = 0; (Ptr 
                                                                            <= (Cnt - 1)); Ptr++) {
                                                                    Pos = genericController.vbInstr(1, ListSplit[Ptr], ")");
                                                                    if ((Pos > 0)) {
                                                                        ContentID = genericController.EncodeInteger(ListSplit[Ptr].Substring(0, (Pos - 1)));
                                                                        if (((ContentID > 0) 
                                                                                    && ((ContentID != adminContent.Id) 
                                                                                    && userHasContentAccess(ContentID)))) {
                                                                            SubQuery = (SubQuery + ("OR(" 
                                                                                        + (adminContent.ContentTableName + (".ContentControlID=" 
                                                                                        + (ContentID + ")")))));
                                                                            return_ContentAccessLimitMessage = (return_ContentAccessLimitMessage + (", \'<a href=\"?cid=" 
                                                                                        + (ContentID + ("\">" 
                                                                                        + (Models.Complex.cdefModel.getContentNameByID(cpCore, ContentID) + "</a>\'")))));
                                                                            ("," + ContentID);
                                                                            SubContentCnt = (SubContentCnt + 1);
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((SubQuery == "")) {
                                                            // 
                                                            //  Person has no access
                                                            // 
                                                            Return_AllowAccess = false;
                                                            return;
                                                        }
                                                        else {
                                                            ("AND(" 
                                                                        + (SubQuery.Substring(2) + ")"));
                                                            return_ContentAccessLimitMessage = ("Your access to " 
                                                                        + (adminContent.Name + (" is limited to Sub-content(s) " + return_ContentAccessLimitMessage.Substring(2))));
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: Active Only
                                                    // 
                                                    if (IndexConfig.ActiveOnly) {
                                                        ("AND(" 
                                                                    + (adminContent.ContentTableName + ".active<>0)"));
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: edited by me
                                                    // 
                                                    if (IndexConfig.LastEditedByMe) {
                                                        ("AND(" 
                                                                    + (adminContent.ContentTableName + (".ModifiedBy=" 
                                                                    + (cpCore.doc.authContext.user.id + ")"))));
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: edited today
                                                    // 
                                                    if (IndexConfig.LastEditedToday) {
                                                        ("AND(" 
                                                                    + (adminContent.ContentTableName + (".ModifiedDate>=" 
                                                                    + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date) + ")"))));
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: edited past week
                                                    // 
                                                    if (IndexConfig.LastEditedPast7Days) {
                                                        ("AND(" 
                                                                    + (adminContent.ContentTableName + (".ModifiedDate>=" 
                                                                    + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date.AddDays(-7)) + ")"))));
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: edited past month
                                                    // 
                                                    if (IndexConfig.LastEditedPast30Days) {
                                                        ("AND(" 
                                                                    + (adminContent.ContentTableName + (".ModifiedDate>=" 
                                                                    + (cpCore.db.encodeSQLDate(cpCore.doc.profileStartTime.Date.AddDays(-30)) + ")"))));
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: Where Pairs
                                                    // 
                                                    for (WCount = 0; (WCount <= 9); WCount++) {
                                                        if ((WherePair(1, WCount) != "")) {
                                                            // 
                                                            //  Verify that the fieldname called out is in this table
                                                            // 
                                                            if ((adminContent.fields.Count > 0)) {
                                                                foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                                    Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                                    // With...
                                                                    if ((genericController.vbUCase(field.nameLc) == genericController.vbUCase(WherePair(0, WCount)))) {
                                                                        // 
                                                                        //  found it, add it in the sql
                                                                        // 
                                                                        ("AND(" 
                                                                                    + (adminContent.ContentTableName + ("." 
                                                                                    + (WherePair(0, WCount) + "="))));
                                                                        if (genericController.vbIsNumeric(WherePair(1, WCount))) {
                                                                            (WherePair(1, WCount) + ")");
                                                                        }
                                                                        else {
                                                                            ("\'" 
                                                                                        + (WherePair(1, WCount) + "\')"));
                                                                        }
                                                                        
                                                                        break;
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    // 
                                                    //  Where Clause: findwords
                                                    // 
                                                    if ((IndexConfig.FindWords.Count > 0)) {
                                                        foreach (kvp in IndexConfig.FindWords) {
                                                            indexConfigFindWordClass findword = kvp.Value;
                                                            FindMatchOption = findword.MatchOption;
                                                            if ((FindMatchOption != FindWordMatchEnum.MatchIgnore)) {
                                                                FindWordName = genericController.vbLCase(findword.Name);
                                                                FindWordValue = findword.Value;
                                                                // 
                                                                //  Get FieldType
                                                                // 
                                                                if ((adminContent.fields.Count > 0)) {
                                                                    foreach (KeyValuePair<string, Models.Complex.CDefFieldModel> keyValuePair in adminContent.fields) {
                                                                        Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                                        // With...
                                                                        FieldPtr = field.id;
                                                                        //  quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                                                        if ((genericController.vbLCase(field.nameLc) == FindWordName)) {
                                                                            switch (field.fieldTypeId) {
                                                                                case FieldTypeIdAutoIdIncrement:
                                                                                case FieldTypeIdInteger:
                                                                                    // 
                                                                                    //  integer
                                                                                    // 
                                                                                    int FindWordValueInteger = genericController.EncodeInteger(FindWordValue);
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.MatchEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchNotEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is not null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchEquals:
                                                                                        case FindWordMatchEnum.matchincludes:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("=" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueInteger) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchGreaterThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + (">" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueInteger) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchLessThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("<" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueInteger) + ")"))))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                case FieldTypeIdCurrency:
                                                                                case FieldTypeIdFloat:
                                                                                    // 
                                                                                    //  double
                                                                                    // 
                                                                                    double FindWordValueDouble = genericController.EncodeNumber(FindWordValue);
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.MatchEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchNotEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is not null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchEquals:
                                                                                        case FindWordMatchEnum.matchincludes:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("=" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueDouble) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchGreaterThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + (">" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueDouble) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchLessThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("<" 
                                                                                                        + (cpCore.db.encodeSQLNumber(FindWordValueDouble) + ")"))))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                case FieldTypeIdFile:
                                                                                case FieldTypeIdFileImage:
                                                                                    // 
                                                                                    //  Date
                                                                                    // 
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.MatchEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchNotEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is not null)"))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                case FieldTypeIdDate:
                                                                                    // 
                                                                                    //  Date
                                                                                    // 
                                                                                    DateTime findDate;
                                                                                    field.MinValue;
                                                                                    if (IsDate(FindWordValue)) {
                                                                                        findDate = DateTime.Parse(FindWordValue);
                                                                                    }
                                                                                    
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.MatchEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchNotEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is not null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchEquals:
                                                                                        case FindWordMatchEnum.matchincludes:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("=" 
                                                                                                        + (cpCore.db.encodeSQLDate(findDate) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchGreaterThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + (">" 
                                                                                                        + (cpCore.db.encodeSQLDate(findDate) + ")"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchLessThan:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("<" 
                                                                                                        + (cpCore.db.encodeSQLDate(findDate) + ")"))))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                case FieldTypeIdLookup:
                                                                                case FieldTypeIdMemberSelect:
                                                                                    // 
                                                                                    //  Lookup
                                                                                    // 
                                                                                    if (IsLookupFieldValid[field.nameLc]) {
                                                                                        // 
                                                                                        //  Content Lookup
                                                                                        // 
                                                                                        switch (FindMatchOption) {
                                                                                            case FindWordMatchEnum.MatchEmpty:
                                                                                                ("AND(LookupTable" 
                                                                                                            + (FieldPtr + ".ID is null)"));
                                                                                                break;
                                                                                            case FindWordMatchEnum.MatchNotEmpty:
                                                                                                ("AND(LookupTable" 
                                                                                                            + (FieldPtr + ".ID is not null)"));
                                                                                                break;
                                                                                            case FindWordMatchEnum.MatchEquals:
                                                                                                ("AND(LookupTable" 
                                                                                                            + (FieldPtr + (".Name=" 
                                                                                                            + (cpCore.db.encodeSQLText(FindWordValue) + ")"))));
                                                                                                break;
                                                                                            case FindWordMatchEnum.matchincludes:
                                                                                                ("AND(LookupTable" 
                                                                                                            + (FieldPtr + (".Name LIKE " 
                                                                                                            + (cpCore.db.encodeSQLText(("%" 
                                                                                                                + (FindWordValue + "%"))) + ")"))));
                                                                                                break;
                                                                                        }
                                                                                    }
                                                                                    else if ((field.lookupList != "")) {
                                                                                        // 
                                                                                        //  LookupList
                                                                                        // 
                                                                                        switch (FindMatchOption) {
                                                                                            case FindWordMatchEnum.MatchEmpty:
                                                                                                ("AND(" 
                                                                                                            + (adminContent.ContentTableName + ("." 
                                                                                                            + (FindWordName + " is null)"))));
                                                                                                break;
                                                                                            case FindWordMatchEnum.MatchNotEmpty:
                                                                                                ("AND(" 
                                                                                                            + (adminContent.ContentTableName + ("." 
                                                                                                            + (FindWordName + " is not null)"))));
                                                                                                break;
                                                                                            case FindWordMatchEnum.MatchEquals:
                                                                                            case FindWordMatchEnum.matchincludes:
                                                                                                lookups = field.lookupList.Split(",");
                                                                                                LookupQuery = "";
                                                                                                for (LookupPtr = 0; (LookupPtr <= UBound(lookups)); LookupPtr++) {
                                                                                                    if ((genericController.vbInstr(1, lookups[LookupPtr], FindWordValue, vbTextCompare) != 0)) {
                                                                                                        LookupQuery = (LookupQuery + ("OR(" 
                                                                                                                    + (adminContent.ContentTableName + ("." 
                                                                                                                    + (FindWordName + ("=" 
                                                                                                                    + (cpCore.db.encodeSQLNumber((LookupPtr + 1)) + ")")))))));
                                                                                                    }
                                                                                                    
                                                                                                }
                                                                                                
                                                                                                if ((LookupQuery != "")) {
                                                                                                    ("AND(" 
                                                                                                                + (LookupQuery.Substring(2) + ")"));
                                                                                                }
                                                                                                
                                                                                                break;
                                                                                        }
                                                                                    }
                                                                                    
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                case FieldTypeIdBoolean:
                                                                                    // 
                                                                                    //  Boolean
                                                                                    // 
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.matchincludes:
                                                                                            if (genericController.EncodeBoolean(FindWordValue)) {
                                                                                                ("AND(" 
                                                                                                            + (adminContent.ContentTableName + ("." 
                                                                                                            + (FindWordName + "<>0)"))));
                                                                                            }
                                                                                            else {
                                                                                                ("AND((" 
                                                                                                            + (adminContent.ContentTableName + ("." 
                                                                                                            + (FindWordName + ("=0)or(" 
                                                                                                            + (adminContent.ContentTableName + ("." 
                                                                                                            + (FindWordName + " is null))"))))))));
                                                                                            }
                                                                                            
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchTrue:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + "<>0)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchFalse:
                                                                                            ("AND((" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("=0)or(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null))"))))))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                                default:
                                                                                    switch (FindMatchOption) {
                                                                                        case FindWordMatchEnum.MatchEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchNotEmpty:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + " is not null)"))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.matchincludes:
                                                                                            FindWordValue = cpCore.db.encodeSQLText(FindWordValue);
                                                                                            FindWordValue = FindWordValue.Substring(1, (FindWordValue.Length - 2));
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + (" LIKE \'%" 
                                                                                                        + (FindWordValue + "%\')"))))));
                                                                                            break;
                                                                                        case FindWordMatchEnum.MatchEquals:
                                                                                            ("AND(" 
                                                                                                        + (adminContent.ContentTableName + ("." 
                                                                                                        + (FindWordName + ("=" 
                                                                                                        + (cpCore.db.encodeSQLText(FindWordValue) + ")"))))));
                                                                                            break;
                                                                                    }
                                                                                    break; // TODO: Warning!!! Review that break works as 'Exit For' as it is inside another 'breakable' statement:Switch
                                                                                    break;
                                                                            }
                                                                            break;
                                                                        }
                                                                        
                                                                    }
                                                                    
                                                                }
                                                                
                                                            }
                                                            
                                                        }
                                                        
                                                    }
                                                    
                                                    return_SQLWhere = return_SQLWhere.Substring(3);
                                                    // 
                                                    //  SQL Order by
                                                    // 
                                                    return_SQLOrderBy = "";
                                                    string orderByDelim = " ";
                                                    foreach (kvp in IndexConfig.Sorts) {
                                                        indexConfigSortClass sort = kvp.Value;
                                                        SortFieldName = genericController.vbLCase(sort.fieldName);
                                                        // 
                                                        //  Get FieldType
                                                        // 
                                                        if (adminContent.fields.ContainsKey(sort.fieldName)) {
                                                            // With...
                                                            FieldPtr = adminContent.fields(sort.fieldName).id;
                                                            //  quick fix for a replacement for the old fieldPtr (so multiple for loops will always use the same "table"+ptr string
                                                            if (((adminContent.fields(sort.fieldName).fieldTypeId == FieldTypeIdLookup) 
                                                                        && IsLookupFieldValid[sort.fieldName])) {
                                                                (orderByDelim + ("LookupTable" 
                                                                            + (FieldPtr + ".Name")));
                                                            }
                                                            else {
                                                                (orderByDelim 
                                                                            + (adminContent.ContentTableName + ("." + SortFieldName)));
                                                            }
                                                            
                                                        }
                                                        
                                                        if ((sort.direction > 1)) {
                                                            return_SQLOrderBy = (return_SQLOrderBy + " Desc");
                                                        }
                                                        
                                                        orderByDelim = ",";
                                                    }
                                                    
                                                }
                                                catch (Exception ex) {
                                                    cpCore.handleException(ex);
                                                    throw;
                                                }
                                                
                                                // 
                                                // ==============================================================================================
                                                //    If this field has no help message, check the field with the same name from it's inherited parent
                                                // ==============================================================================================
                                                // 
                                                getFieldHelpMsgs(((int)(ContentID)), ((string)(FieldName)), ref ((string)(return_Default)), ref ((string)(return_Custom)));
                                                // TODO: On Error GoTo Warning!!!: The statement is not translatable 
                                                // 'Dim th as integer : th = profileLogAdminMethodEnter( "getFieldHelpMsgs")
                                                // 
                                                string SQL;
                                                int CS;
                                                bool Found;
                                                int ParentID;
                                                // 
                                                Found = false;
                                                SQL = ("select h.HelpDefault,h.HelpCustom from ccfieldhelp h left join ccfields f on f.id=h.fieldid where f.c" +
                                                "ontentid=" 
                                                            + (ContentID + (" and f.name=" + cpCore.db.encodeSQLText(FieldName))));
                                                CS = cpCore.db.csOpenSql(SQL);
                                                if (cpCore.db.csOk(CS)) {
                                                    Found = true;
                                                    return_Default = cpCore.db.csGetText(CS, "helpDefault");
                                                    return_Custom = cpCore.db.csGetText(CS, "helpCustom");
                                                }
                                                
                                                cpCore.db.csClose(CS);
                                                // 
                                                if (!Found) {
                                                    ParentID = 0;
                                                    SQL = ("select parentid from cccontent where id=" + ContentID);
                                                    CS = cpCore.db.csOpenSql(SQL);
                                                    if (cpCore.db.csOk(CS)) {
                                                        ParentID = cpCore.db.csGetInteger(CS, "parentid");
                                                    }
                                                    
                                                    cpCore.db.csClose(CS);
                                                    if ((ParentID != 0)) {
                                                        getFieldHelpMsgs(ParentID, FieldName, return_Default, return_Custom);
                                                    }
                                                    
                                                }
                                                
                                                // 
                                                return;
                                                // 
                                            ErrorTrap:
                                                throw new Exception("unexpected exception");
                                                // 
                                                // ===========================================================================
                                                // '' <summary>
                                                // '' handle legacy errors in this class, v3
                                                // '' </summary>
                                                // '' <param name="MethodName"></param>
                                                // '' <param name="Context"></param>
                                                // '' <remarks></remarks>
                                                handleLegacyClassError3(((string)(MethodName)), Optional, ContextAsString=);
                                                // 
                                                throw new Exception(("error in method [" 
                                                                + (MethodName + ("], contect [" 
                                                                + (Context + "]")))));
                                                // 
                                                // ===========================================================================
                                                // '' <summary>
                                                // '' handle legacy errors in this class, v2
                                                // '' </summary>
                                                // '' <param name="MethodName"></param>
                                                // '' <param name="Context"></param>
                                                // '' <remarks></remarks>
                                                handleLegacyClassError2(((string)(MethodName)), Optional, ContextAsString=);
                                                // 
                                                throw new Exception(("error in method [" 
                                                                + (MethodName + ("], Context [" 
                                                                + (Context + "]")))));
                                                Err.Clear();
                                                // 
                                                // 
                                                // ===========================================================================
                                                // '' <summary>
                                                // '' handle legacy errors in this class, v1
                                                // '' </summary>
                                                // '' <param name="MethodName"></param>
                                                // '' <param name="ErrDescription"></param>
                                                // '' <remarks></remarks>
                                                handleLegacyClassError(((string)(MethodName)), ((string)(ErrDescription)));
                                                throw new Exception(("error in method [" 
                                                                + (MethodName + ("], ErrDescription [" 
                                                                + (ErrDescription + "]")))));
                                                // Private Sub pattern1()
                                                //     Dim admincontent As coreMetaDataClass.CDefClass
                                                //     For Each keyValuePair As KeyValuePair(Of String, coreMetaDataClass.CDefFieldClass) In admincontent.fields
                                                //         Dim field As coreMetaDataClass.CDefFieldClass = keyValuePair.Value
                                                //         '
                                                //     Next
                                                // End Sub
                                                // 
                                                // ====================================================================================================
                                                //  properties
                                                // ====================================================================================================
                                                // 
                                                //  ----- ccGroupRules storage for list of Content that a group can author
                                                // 
                                                ContentGroupRuleType;
                                                int ContentID;
                                                int GroupID;
                                                bool AllowAdd;
                                                bool AllowDelete;
                                                // 
                                                //  ----- generic id/name dictionary
                                                // 
                                                StorageType;
                                                int Id;
                                                string Name;
                                                // 
                                                //  ----- Group Rules
                                                // 
                                                GroupRuleType;
                                                int GroupID;
                                                bool AllowAdd;
                                                bool AllowDelete;
                                                // 
                                                //  ----- Used within Admin site to create fancyBox popups
                                                // 
                                                ((bool)(includeFancyBox));
                                                ((int)(fancyBoxPtr));
                                                ((string)(fancyBoxHeadJS));
                                                ((bool)(ClassInitialized));
                                                //  if true, the module has been
                                                const object allowSaveBeforeDuplicate = false;
                                                DeleteType;
                                                string Name;
                                                int ParentID;
                                                NavigatorType;
                                                string Name;
                                                string menuNameSpace;
                                                Collection2Type;
                                                int AddOnCnt;
                                                string[] AddonGuid;
                                                string[] AddonName;
                                                int MenuCnt;
                                                string[] Menus;
                                                int NavigatorCnt;
                                                NavigatorType[] Navigators;
                                                ((int)(CollectionCnt));
                                                ((Collection2Type)(Collections()));
                                                // 
                                                //  ----- Target Data Storage
                                                // 
                                                ((int)(requestedContentId));
                                                ((int)(requestedRecordId));
                                                // Private false As Boolean    ' set if content and site support workflow authoring
                                                ((bool)(BlockEditForm));
                                                //  true if there was an error loading the edit record - use to block the edit form
                                                // 
                                                //  ----- Storage for current EditRecord, loaded in LoadEditRecord
                                                // 
                                                editRecordFieldClass;
                                                ((object)(dbValue));
                                                ((object)(value));
                                                // 
                                                editRecordClass;
                                                ((void)(fieldsLc));
                                                new Dictionary<string, editRecordFieldClass>();
                                                ((int)(id));
                                                //  ID field of edit record (Record to be edited)
                                                ((int)(parentID));
                                                //  ParentID field of edit record (Record to be edited)
                                                ((string)(nameLc));
                                                //  name field of edit record
                                                ((bool)(active));
                                                //  active field of the edit record
                                                ((int)(contentControlId));
                                                //  ContentControlID of the edit record
                                                ((string)(contentControlId_Name));
                                                // 
                                                ((string)(menuHeadline));
                                                //  Used for Content Watch Link Label if default
                                                ((DateTime)(modifiedDate));
                                                //  Used for control section display
                                                ((int)(modifiedByMemberID));
                                                //    =
                                                ((DateTime)(dateAdded));
                                                //    =
                                                ((int)(createByMemberId));
                                                //    =
                                                ((int)(RootPageID));
                                                ((bool)(SetPageNotFoundPageID));
                                                ((bool)(SetLandingPageID));
                                                // 
                                                ((bool)(Loaded));
                                                //  true/false - set true when the field array values are loaded
                                                ((bool)(Saved));
                                                //  true if edit record was saved during this page
                                                ((bool)(Read_Only));
                                                //  set if this record can not be edited, for various reasons
                                                // 
                                                //  From cpCore.main_GetAuthoringStatus
                                                // 
                                                ((bool)(IsDeleted));
                                                //  true means the edit record has been deleted
                                                ((bool)(IsInserted));
                                                //  set if Workflow authoring insert
                                                ((bool)(IsModified));
                                                //  record has been modified since last published
                                                ((string)(LockModifiedName));
                                                //  member who first edited the record
                                                ((DateTime)(LockModifiedDate));
                                                //  Date when member modified record
                                                ((bool)(SubmitLock));
                                                //  set if a submit Lock, even if the current user is admin
                                                ((string)(SubmittedName));
                                                //  member who submitted the record
                                                ((DateTime)(SubmittedDate));
                                                //  Date when record was submitted
                                                ((bool)(ApproveLock));
                                                //  set if an approve Lock
                                                ((string)(ApprovedName));
                                                //  member who approved the record
                                                ((DateTime)(ApprovedDate));
                                                //  Date when record was approved
                                                // 
                                                //  From cpCore.main_GetAuthoringPermissions
                                                // 
                                                ((bool)(AllowInsert));
                                                ((bool)(AllowCancel));
                                                ((bool)(AllowSave));
                                                ((bool)(AllowDelete));
                                                ((bool)(AllowPublish));
                                                ((bool)(AllowAbort));
                                                ((bool)(AllowSubmit));
                                                ((bool)(AllowApprove));
                                                // 
                                                //  From cpCore.main_GetEditLock
                                                // 
                                                ((bool)(EditLock));
                                                //  set if an edit Lock by anyone else besides the current user
                                                ((int)(EditLockMemberID));
                                                //  Member who edit locked the record
                                                ((string)(EditLockMemberName));
                                                //  Member who edit locked the record
                                                ((DateTime)(EditLockExpires));
                                                //  Time when the edit lock expires
                                                // 
                                                // =============================================================================
                                                //  ----- Control Response
                                                // =============================================================================
                                                // 
                                                ((string)(AdminButton));
                                                //  Value returned from a submit button, process into action/form
                                                ((int)(AdminAction));
                                                //  The action to be performed before the next form
                                                ((int)(AdminForm));
                                                //  The next form to print
                                                ((int)(AdminSourceForm));
                                                //  The form that submitted that the button to process
                                                ((string)(WherePair(2, 10)));
                                                //  for passing where clause values from page to page
                                                ((int)(WherePairCount));
                                                //  the current number of WherePairCount in use
                                                // Private OrderByFieldPointer as integer
                                                const object OrderByFieldPointerDefault = -1;
                                                // Private Direction as integer
                                                ((int)(RecordTop));
                                                ((int)(RecordsPerPage));
                                                const object RecordsPerPageDefault = 50;
                                                // Private InputFieldName As String   ' Input FieldName used for DHTMLEdit
                                                ((int)(MenuDepth));
                                                //  The number of windows open (below this one)
                                                ((string)(TitleExtension));
                                                //  String that adds on to the end of the title
                                                // Private Findstring(50) As String                ' Value to search for each index column
                                                // 
                                                //  SpellCheck Features
                                                // 
                                                ((bool)(SpellCheckSupported));
                                                //  if true, spell checking is supported
                                                ((bool)(SpellCheckRequest));
                                                //  If true, send the spell check form to the browser
                                                ((bool)(SpellCheckResponse));
                                                //  if true, the user is sending the spell check back to process
                                                ((string)(SpellCheckWhiteCharacterList));
                                                ((string)(SpellCheckDictionaryFilename));
                                                //  Full path to user dictionary
                                                ((string)(SpellCheckIgnoreList));
                                                //  List of ignore words (used to verify the file is there)
                                                // 
                                                // =============================================================================
                                                //  preferences
                                                // =============================================================================
                                                // 
                                                ((int)(AdminMenuModeID));
                                                //  Controls the menu mode, set from cpCore.main_MemberAdminMenuModeID
                                                ((bool)(allowAdminTabs));
                                                //  true uses tab system
                                                ((string)(fieldEditorPreference));
                                                //  this is a hidden on the edit form. The popup editor preferences sets this hidden and submits
                                                // 
                                                // =============================================================================
                                                //    Content Tracking Editing
                                                // 
                                                //    These values are read from Edit form response, and are used to populate then
                                                //    ContentWatch and ContentWatchListRules records.
                                                // 
                                                //    They are read in before the current record is processed, then processed and
                                                //    Saved back to ContentWatch and ContentWatchRules after the current record is
                                                //    processed, so changes to the record can be reflected in the ContentWatch records.
                                                //    For instance, if the record is marked inactive, the ContentWatchLink is cleared
                                                //    and all ContentWatchListRules are deleted.
                                                // 
                                                // =============================================================================
                                                // 
                                                ((bool)(ContentWatchLoaded));
                                                //  flag set that shows the rest are valid
                                                // 
                                                ((int)(ContentWatchRecordID));
                                                ((string)(ContentWatchLink));
                                                ((int)(ContentWatchClicks));
                                                ((string)(ContentWatchLinkLabel));
                                                ((DateTime)(ContentWatchExpires));
                                                ((int)(ContentWatchListID()));
                                                //  list of all ContentWatchLists for this Content, read from response, then later saved to Rules
                                                ((int)(ContentWatchListIDSize));
                                                //  size of ContentWatchListID() array
                                                ((int)(ContentWatchListIDCount));
                                                //  number of valid entries in ContentWatchListID()
                                                // '
                                                // '=============================================================================
                                                // '   Calendar Event Editing
                                                // '=============================================================================
                                                // '
                                                // Private CalendarEventName As String
                                                // Private CalendarEventStartDate As Date
                                                // Private CalendarEventEndDate As Date
                                                // 
                                                // =============================================================================
                                                //  Other
                                                // =============================================================================
                                                // 
                                                ((int)(ObjectCount));
                                                //  Convert the following objects to this one
                                                ((int)(ButtonObjectCount));
                                                //  Count of Buttons in use
                                                ((int)(ImagePreloadCount));
                                                //  Number of images preloaded
                                                ((string)(ImagePreloads(2, 100)));
                                                //  names of all gifs already preloaded
                                                //                        (0,x) = imagename
                                                //                        (1,x) = ImageObject name for the image
                                                ((string)(JavaScriptString));
                                                //  Collected string of Javascript functions to print at end
                                                ((string)(AdminFormBottom));
                                                //  the HTML needed to complete the Admin Form after contents
                                                ((bool)(UserAllowContentEdit));
                                                //  set on load - checked within each edit/index page
                                                ((bool)(UserAllowContentAdd));
                                                ((bool)(UserAllowContentDelete));
                                                ((int)(TabStopCount));
                                                //  used to generate TabStop values
                                                ((int)(FormInputCount));
                                                //  used to generate labels for form input
                                                ((int)(EditSectionPanelCount));
                                                const object OpenLiveWindowTable = "<div ID=\"LiveWindowTable\">";
                                                const object CloseLiveWindowTable = "</div>";
                                                const object AdminFormErrorOpen = "<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td align=\"left\">";
                                                const object AdminFormErrorClose = "</td></tr></table>";
                                                const object RequestNameAdminDepth = "ad";
                                                const object RequestNameAdminForm = "af";
                                                const object RequestNameAdminSourceForm = "asf";
                                                const object RequestNameAdminAction = "aa";
                                                const object RequestNameTitleExtension = "tx";
                                                Enum;
                                                NodeTypeEnum;
                                                NodeTypeEntry = 0;
                                                NodeTypeCollection = 1;
                                                NodeTypeAddon = 2;
                                                NodeTypeContent = 3;
                                                Enum;
                                                // 
                                                const object IndexConfigPrefix = "IndexConfig:";
                                                Enum;
                                                FindWordMatchEnum;
                                                MatchIgnore = 0;
                                                MatchEmpty = 1;
                                                MatchNotEmpty = 2;
                                                MatchGreaterThan = 3;
                                                MatchLessThan = 4;
                                                matchincludes = 5;
                                                MatchEquals = 6;
                                                MatchTrue = 7;
                                                MatchFalse = 8;
                                                Enum;
                                                // 
                                                // 
                                                // 
                                                indexConfigSortClass;
                                                // Dim FieldPtr As Integer
                                                ((string)(fieldName));
                                                ((int)(direction));
                                                //  1=forward, 2=reverse, 0=ignore/remove this sort
                                                // 
                                                indexConfigFindWordClass;
                                                ((string)(Name));
                                                ((string)(Value));
                                                ((int)(Type));
                                                ((FindWordMatchEnum)(MatchOption));
                                                // 
                                                indexConfigColumnClass;
                                                ((string)(Name));
                                                // Public FieldId As Integer
                                                ((int)(Width));
                                                ((int)(SortPriority));
                                                ((int)(SortDirection));
                                                // 
                                                indexConfigClass;
                                                ((bool)(Loaded));
                                                ((int)(ContentID));
                                                ((int)(PageNumber));
                                                ((int)(RecordsPerPage));
                                                ((int)(RecordTop));
                                                // FindWordList As String
                                                ((void)(FindWords));
                                                new Dictionary<string, indexConfigFindWordClass>();
                                                ((bool)(ActiveOnly));
                                                ((bool)(LastEditedByMe));
                                                ((bool)(LastEditedToday));
                                                ((bool)(LastEditedPast7Days));
                                                ((bool)(LastEditedPast30Days));
                                                ((bool)(Open));
                                                // public SortCnt As Integer
                                                ((void)(Sorts));
                                                new Dictionary<string, indexConfigSortClass>();
                                                ((int)(GroupListCnt));
                                                ((string)(GroupList()));
                                                // public ColumnCnt As Integer
                                                ((void)(Columns));
                                                new Dictionary<string, indexConfigColumnClass>();
                                                ((int)(SubCDefID));
                                                // 
                                                //  Temp
                                                // 
                                                const object ToolsActionMenuMove = 1;
                                                const object ToolsActionAddField = 2;
                                                //  Add a field to the Index page
                                                const object ToolsActionRemoveField = 3;
                                                const object ToolsActionMoveFieldRight = 4;
                                                const object ToolsActionMoveFieldLeft = 5;
                                                const object ToolsActionSetAZ = 6;
                                                const object ToolsActionSetZA = 7;
                                                const object ToolsActionExpand = 8;
                                                const object ToolsActionContract = 9;
                                                const object ToolsActionEditMove = 10;
                                                const object ToolsActionRunQuery = 11;
                                                const object ToolsActionDuplicateDataSource = 12;
                                                const object ToolsActionDefineContentFieldFromTableFieldsFromTable = 13;
                                                const object ToolsActionFindAndReplace = 14;
                                                // 
                                                ((bool)(AllowAdminFieldCheck_Local));
                                                ((bool)(AllowAdminFieldCheck_LocalLoaded));
                                                // 
                                                const object AddonGuidPreferences = "{D9C2D64E-9004-4DBE-806F-60635B9F52C8}";
                                                ((string)(admin_GetAdminFormBody(((string)(Caption)), ((string)(ButtonListLeft)), ((string)(ButtonListRight)), ((bool)(AllowAdd)), ((bool)(AllowDelete)), ((string)(Description)), ((string)(ContentSummary)), ((int)(ContentPadding)), ((string)(Content)))));
                                                return (new adminUIController(cpCore) + GetBody(Caption, ButtonListLeft, ButtonListRight, AllowAdd, AllowDelete, Description, ContentSummary, ContentPadding, Content));
                                                // 
                                            }
                                            
                                        }
                                        
                                    }
                                    
                                    break;
                            }
                        }
                        
                    }
                    
                }
                
            }
            
        }
    }
}
