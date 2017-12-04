using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Controllers;


using System.Xml;
using Contensive.Core;
using Models.Entity;
//
namespace Contensive.Addons.AdminSite
{
	public partial class getAdminSiteClass : Contensive.BaseClasses.AddonBaseClass
	{
		//
		//========================================================================
		//   Print the index form, values and all
		//       creates a sql with leftjoins, and renames lookups as TableLookupxName
		//       where x is the TarGetFieldPtr of the field that is FieldTypeLookup
		//
		//   Input:
		//       AdminContent.contenttablename is required
		//       OrderByFieldPtr
		//       OrderByDirection
		//       RecordTop
		//       RecordsPerPage
		//       Findstring( ColumnPointer )
		//========================================================================
		//
		private string GetForm_Index(Models.Complex.cdefModel adminContent, editRecordClass editRecord, bool IsEmailContent)
		{
			string returnForm = "";
			try
			{
				const string FilterClosedLabel = "<div style=\"font-size:9px;text-align:center;\">&nbsp;<br>F<br>i<br>l<br>t<br>e<br>r<br>s</div>";
				//
				string Copy = "";
				string RightCopy = null;
				int TitleRows = 0;
				// refactor -- is was using page manager code, and it only detected if the page is the current domain's 
				//Dim LandingPageID As Integer
				//Dim IsPageContent As Boolean
				//Dim IsLandingPage As Boolean
				int PageCount = 0;
				bool AllowAdd = false;
				bool AllowDelete = false;
				int recordCnt = 0;
				bool AllowAccessToContent = false;
				string ContentName = null;
				string ContentAccessLimitMessage = "";
				bool IsLimitedToSubContent = false;
				string GroupList = "";
				string[] Groups = null;
				string FieldCaption = null;
				string SubTitle = null;
				string SubTitlePart = null;
				string Title = null;
				string AjaxQS = null;
				string FilterColumn = "";
				string DataColumn = null;
				string DataTable_DataRows = "";
				string FilterDataTable = "";
				string DataTable_FindRow = "";
				string DataTable = null;
				string DataTable_HdrRow = "";
				string IndexFilterContent = "";
				string IndexFilterHead = "";
				string IndexFilterJS = "";
				bool IndexFilterOpen = false;
				indexConfigClass IndexConfig = null;
				int Ptr = 0;
				string SortTitle = null;
				string HeaderDescription = "";
				bool AllowFilterNav = false;
				int ColumnPointer = 0;
				int WhereCount = 0;
				string sqlWhere = "";
				string sqlOrderBy = "";
				string sqlFieldList = "";
				string sqlFrom = "";
				int CS = 0;
				string SQL = null;
				string RowColor = "";
				int RecordPointer = 0;
				int RecordLast = 0;
				int RecordTop_NextPage = 0;
				int RecordTop_PreviousPage = 0;
				int ColumnWidth = 0;
				string ButtonBar = null;
				string TitleBar = null;
				string FindWordValue = null;
				string ButtonObject = null;
				string ButtonFace = null;
				string ButtonHref = null;
				string URI = null;
				//Dim DataSourceName As String
				//Dim DataSourceType As Integer
				string FieldName = null;
				Dictionary<string, bool> FieldUsedInColumns = new Dictionary<string, bool>(); // used to prevent select SQL from being sorted by a field that does not appear
				int ColumnWidthTotal = 0;
				int SubForm = 0;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				int RecordID = 0;
				string RecordName = null;
				string LeftButtons = "";
				string RightButtons = "";
				adminUIController Adminui = new adminUIController(cpCore);
				Dictionary<string, bool> IsLookupFieldValid = new Dictionary<string, bool>();
				bool allowCMEdit = false;
				bool allowCMAdd = false;
				bool allowCMDelete = false;
				//
				// --- make sure required fields are present
				//
				if (adminContent.Id == 0)
				{
					//
					// Bad content id
					//
					Stream.Add(GetForm_Error("This form requires a valid content definition, and one was not found for content ID [" + adminContent.Id + "].", "No content definition was specified [ContentID=0]. Please contact your application developer for more assistance."));
				}
				else if (string.IsNullOrEmpty(adminContent.Name))
				{
					//
					// Bad content name
					//
					Stream.Add(GetForm_Error("No content definition could be found for ContentID [" + adminContent.Id + "]. This could be a menu error. Please contact your application developer for more assistance.", "No content definition for ContentID [" + adminContent.Id + "] could be found."));
				}
				else if (adminContent.ContentTableName == "")
				{
					//
					// No tablename
					//
					Stream.Add(GetForm_Error("The content definition [" + adminContent.Name + "] is not associated with a valid database table. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] ContentTablename is empty."));
				}
				else if (adminContent.fields.Count == 0)
				{
					//
					// No Fields
					//
					Stream.Add(GetForm_Error("This content [" + adminContent.Name + "] cannot be accessed because it has no fields. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] has no field records."));
				}
				else if (adminContent.DeveloperOnly & (!cpCore.doc.authContext.isAuthenticatedDeveloper(cpCore)))
				{
					//
					// Developer Content and not developer
					//
					Stream.Add(GetForm_Error("Access to this content [" + adminContent.Name + "] requires developer permissions. Please contact your application developer for more assistance.", "Content [" + adminContent.Name + "] has no field records."));
				}
				else
				{
					Models.Entity.dataSourceModel datasource = Models.Entity.dataSourceModel.create(cpCore, adminContent.dataSourceId, new List<string>());
					//
					// get access rights
					//
					cpCore.doc.authContext.getContentAccessRights(cpCore, adminContent.Name, allowCMEdit, allowCMAdd, allowCMDelete);
					//
					// detemine which subform to disaply
					//
					SubForm = cpCore.docProperties.getInteger(RequestNameAdminSubForm);
					if (SubForm != 0)
					{
						switch (SubForm)
						{
							case AdminFormIndex_SubFormExport:
								Copy = GetForm_Index_Export(adminContent, editRecord);
								break;
							case AdminFormIndex_SubFormSetColumns:
								Copy = GetForm_Index_SetColumns(adminContent, editRecord);
								break;
							case AdminFormIndex_SubFormAdvancedSearch:
								Copy = GetForm_Index_AdvancedSearch(adminContent, editRecord);
								break;
						}
					}
					Stream.Add(Copy);
					if (string.IsNullOrEmpty(Copy))
					{
						//
						// If subforms return empty, go to parent form
						//
						AllowFilterNav = true;
						//
						// -- Load Index page customizations
						IndexConfig = LoadIndexConfig(adminContent);
						SetIndexSQL_ProcessIndexConfigRequests(adminContent, editRecord, IndexConfig);
						SetIndexSQL_SaveIndexConfig(IndexConfig);
						//
						// Get the SQL parts
						//
						SetIndexSQL(adminContent, editRecord, IndexConfig, AllowAccessToContent, sqlFieldList, sqlFrom, sqlWhere, sqlOrderBy, IsLimitedToSubContent, ContentAccessLimitMessage, FieldUsedInColumns, IsLookupFieldValid);
						if ((!allowCMEdit) || (!AllowAccessToContent))
						{
							//
							// two conditions should be the same -- but not time to check - This user does not have access to this content
							//
							errorController.error_AddUserError(cpCore, "Your account does not have access to any records in '" + adminContent.Name + "'.");
						}
						else
						{
							//
							// Get the total record count
							//
							SQL = "select count(" + adminContent.ContentTableName + ".ID) as cnt from " + sqlFrom;
							if (!string.IsNullOrEmpty(sqlWhere))
							{
								SQL += " where " + sqlWhere;
							}
							CS = cpCore.db.csOpenSql_rev(datasource.Name, SQL);
							if (cpCore.db.csOk(CS))
							{
								recordCnt = cpCore.db.csGetInteger(CS, "cnt");
							}
							cpCore.db.csClose(CS);
							//
							// Assumble the SQL
							//
							SQL = "select";
							if (datasource.type != DataSourceTypeODBCMySQL)
							{
								SQL += " Top " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
							}
							SQL += " " + sqlFieldList + " From " + sqlFrom;
							if (!string.IsNullOrEmpty(sqlWhere))
							{
								SQL += " WHERE " + sqlWhere;
							}
							if (!string.IsNullOrEmpty(sqlOrderBy))
							{
								SQL += " Order By" + sqlOrderBy;
							}
							if (datasource.type == DataSourceTypeODBCMySQL)
							{
								SQL += " Limit " + (IndexConfig.RecordTop + IndexConfig.RecordsPerPage);
							}
							//
							// Refresh Query String
							//
							cpCore.doc.addRefreshQueryString("tr", IndexConfig.RecordTop.ToString());
							cpCore.doc.addRefreshQueryString("asf", AdminForm.ToString());
							cpCore.doc.addRefreshQueryString("cid", adminContent.Id.ToString());
							cpCore.doc.addRefreshQueryString(RequestNameTitleExtension, genericController.EncodeRequestVariable(TitleExtension));
							if (WherePairCount > 0)
							{
								for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++)
								{
									cpCore.doc.addRefreshQueryString("wl" + WhereCount, WherePair(0, WhereCount));
									cpCore.doc.addRefreshQueryString("wr" + WhereCount, WherePair(1, WhereCount));
								}
							}
							//
							// ----- ButtonBar
							//
							AllowAdd = adminContent.AllowAdd & (!IsLimitedToSubContent) && (allowCMAdd);
							if (MenuDepth > 0)
							{
								LeftButtons = LeftButtons + cpCore.html.html_GetFormButton(ButtonClose,,, "window.close();");
							}
							else
							{
								LeftButtons = LeftButtons + cpCore.html.html_GetFormButton(ButtonCancel);
								//LeftButtons = LeftButtons & cpCore.main_GetFormButton(ButtonCancel, , , "return processSubmit(this)")
							}
							if (AllowAdd)
							{
								LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonAdd + "\">";
								//LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
							}
							else
							{
								LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=\"" + ButtonAdd + "\">";
								//LeftButtons = LeftButtons & "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=""" & ButtonAdd & """ onClick=""return processSubmit(this);"">"
							}
							AllowDelete = (adminContent.AllowDelete) && (allowCMDelete);
							if (AllowDelete)
							{
								LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonDelete + "\" onClick=\"if(!DeleteCheck())return false;\">";
							}
							else
							{
								LeftButtons = LeftButtons + "<input TYPE=SUBMIT NAME=BUTTON DISABLED VALUE=\"" + ButtonDelete + "\" onClick=\"if(!DeleteCheck())return false;\">";
							}
							if (IndexConfig.PageNumber == 1)
							{
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonFirst + "\" DISABLED>";
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonPrevious + "\" DISABLED>";
							}
							else
							{
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonFirst + "\">";
								//RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonFirst & """ onClick=""return processSubmit(this);"">"
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonPrevious + "\">";
								//RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonPrevious & """ onClick=""return processSubmit(this);"">"
							}
							//RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonFirst)
							//RightButtons = RightButtons & cpCore.main_GetFormButton(ButtonPrevious)
							if (recordCnt > (IndexConfig.PageNumber * IndexConfig.RecordsPerPage))
							{
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonNext + "\">";
								//RightButtons = RightButtons & "<input TYPE=SUBMIT NAME=BUTTON VALUE=""" & ButtonNext & """ onClick=""return processSubmit(this);"">"
							}
							else
							{
								RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonNext + "\" DISABLED>";
							}
							RightButtons = RightButtons + "<input TYPE=SUBMIT NAME=BUTTON VALUE=\"" + ButtonRefresh + "\">";
							if (recordCnt <= 1)
							{
								PageCount = 1;
							}
							else
							{
								PageCount = Convert.ToInt32(1 + Convert.ToInt32(Math.Floor(Convert.ToDouble((recordCnt - 1) / IndexConfig.RecordsPerPage))));
							}
							ButtonBar = Adminui.GetButtonBarForIndex(LeftButtons, RightButtons, IndexConfig.PageNumber, IndexConfig.RecordsPerPage, PageCount);
							//ButtonBar = AdminUI.GetButtonBar(LeftButtons, RightButtons)
							//
							// ----- TitleBar
							//
							Title = "";
							SubTitle = "";
							SubTitlePart = "";
							if (IndexConfig.ActiveOnly)
							{
								SubTitle = SubTitle + ", active records";
							}
							SubTitlePart = "";
							if (IndexConfig.LastEditedByMe)
							{
								SubTitlePart = SubTitlePart + " by " + cpCore.doc.authContext.user.name;
							}
							if (IndexConfig.LastEditedPast30Days)
							{
								SubTitlePart = SubTitlePart + " in the past 30 days";
							}
							if (IndexConfig.LastEditedPast7Days)
							{
								SubTitlePart = SubTitlePart + " in the week";
							}
							if (IndexConfig.LastEditedToday)
							{
								SubTitlePart = SubTitlePart + " today";
							}
							if (!string.IsNullOrEmpty(SubTitlePart))
							{
								SubTitle = SubTitle + ", last edited" + SubTitlePart;
							}
							foreach (var kvp in IndexConfig.FindWords)
							{
								indexConfigFindWordClass findWord = kvp.Value;
								if (!string.IsNullOrEmpty(findWord.Name))
								{
									FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, adminContent.Name, findWord.Name, "caption"));
									switch (findWord.MatchOption)
									{
										case FindWordMatchEnum.MatchEmpty:
											SubTitle = SubTitle + ", " + FieldCaption + " is empty";
											break;
										case FindWordMatchEnum.MatchEquals:
											SubTitle = SubTitle + ", " + FieldCaption + " = '" + findWord.Value + "'";
											break;
										case FindWordMatchEnum.MatchFalse:
											SubTitle = SubTitle + ", " + FieldCaption + " is false";
											break;
										case FindWordMatchEnum.MatchGreaterThan:
											SubTitle = SubTitle + ", " + FieldCaption + " &gt; '" + findWord.Value + "'";
											break;
										case FindWordMatchEnum.matchincludes:
											SubTitle = SubTitle + ", " + FieldCaption + " includes '" + findWord.Value + "'";
											break;
										case FindWordMatchEnum.MatchLessThan:
											SubTitle = SubTitle + ", " + FieldCaption + " &lt; '" + findWord.Value + "'";
											break;
										case FindWordMatchEnum.MatchNotEmpty:
											SubTitle = SubTitle + ", " + FieldCaption + " is not empty";
											break;
										case FindWordMatchEnum.MatchTrue:
											SubTitle = SubTitle + ", " + FieldCaption + " is true";
											break;
									}

								}
							}
							if (IndexConfig.SubCDefID > 0)
							{
								ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, IndexConfig.SubCDefID);
								if (!string.IsNullOrEmpty(ContentName))
								{
									SubTitle = SubTitle + ", in Sub-content '" + ContentName + "'";
								}
							}
							//
							// add groups to caption
							//
							if ((adminContent.ContentTableName.ToLower() == "ccmembers") && (IndexConfig.GroupListCnt > 0))
							{
								//If (LCase(AdminContent.ContentTableName) = "ccmembers") And (.GroupListCnt > 0) Then
								SubTitlePart = "";
								for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++)
								{
									if (IndexConfig.GroupList(Ptr) != "")
									{
										GroupList = GroupList + "\t" + IndexConfig.GroupList(Ptr);
									}
								}
								if (!string.IsNullOrEmpty(GroupList))
								{
									Groups = Microsoft.VisualBasic.Strings.Split(GroupList.Substring(1), "\t", -1, Microsoft.VisualBasic.CompareMethod.Binary);
									if (Groups.GetUpperBound(0) == 0)
									{
										SubTitle = SubTitle + ", in group '" + Groups[0] + "'";
									}
									else if (Groups.GetUpperBound(0) == 1)
									{
										SubTitle = SubTitle + ", in groups '" + Groups[0] + "' and '" + Groups[1] + "'";
									}
									else
									{
										for (Ptr = 0; Ptr < Groups.GetUpperBound(0); Ptr++)
										{
											SubTitlePart = SubTitlePart + ", '" + Groups[Ptr] + "'";
										}
										SubTitle = SubTitle + ", in groups" + SubTitlePart.Substring(1) + " and '" + Groups[Ptr] + "'";
									}

								}
							}
							//
							// add sort details to caption
							//
							SubTitlePart = "";
							foreach (var kvp in IndexConfig.Sorts)
							{
								indexConfigSortClass sort = kvp.Value;
								if (sort.direction > 0)
								{
									SubTitlePart = SubTitlePart + " and " + adminContent.fields(sort.fieldName).caption;
									if (sort.direction > 1)
									{
										SubTitlePart += " reverse";
									}
								}
							}
							if (!string.IsNullOrEmpty(SubTitlePart))
							{
								SubTitle += ", sorted by" + SubTitlePart.Substring(4);
							}
							//
							Title = adminContent.Name;
							if (TitleExtension != "")
							{
								Title = Title + " " + TitleExtension;
							}
							switch (recordCnt)
							{
								case 0:
									RightCopy = "no records found";
									break;
								case 1:
									RightCopy = "1 record found";
									break;
								default:
									RightCopy = recordCnt + " records found";
									break;
							}
							RightCopy = RightCopy + ", page " + IndexConfig.PageNumber;
							Title = "<div>"
								+ "<span style=\"float:left;\"><strong>" + Title + "</strong></span>"
								+ "<span style=\"float:right;\">" + RightCopy + "</span>"
								+ "</div>";
							TitleRows = 0;
							if (!string.IsNullOrEmpty(SubTitle))
							{
								Title = Title + "<div style=\"clear:both\">Filter: " + genericController.encodeHTML(SubTitle.Substring(2)) + "</div>";
								TitleRows = TitleRows + 1;
							}
							if (!string.IsNullOrEmpty(ContentAccessLimitMessage))
							{
								Title = Title + "<div style=\"clear:both\">" + ContentAccessLimitMessage + "</div>";
								TitleRows = TitleRows + 1;
							}
							if (TitleRows == 0)
							{
								Title = Title + "<div style=\"clear:both\">&nbsp;</div>";
							}
							//
							TitleBar = SpanClassAdminNormal + Title + "</span>";
							//TitleBar = TitleBar & cpCore.main_GetHelpLink(46, "Using the Admin Index Page", BubbleCopy_AdminIndexPage)
							//
							// ----- Filter Data Table
							//
							if (AllowFilterNav)
							{
								//
								// Filter Nav - if enabled, just add another cell to the row
								//
								IndexFilterOpen = cpCore.visitProperty.getBoolean("IndexFilterOpen", false);
								if (IndexFilterOpen)
								{
									//
									// Ajax Filter Open
									//
									IndexFilterHead = ""
										+ Environment.NewLine + "<div class=\"ccHeaderCon\">"
										+ Environment.NewLine + "<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\">"
										+ "\r" + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
										+ "\r" + "<td valign=Middle class=\"left\">Filters</td>"
										+ "\r" + "<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><img alt=\"Close Filters\" title=\"Close Filters\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
										+ "\r" + "</tr></table>"
										+ Environment.NewLine + "</div>"
										+ Environment.NewLine + "<div id=\"IndexFilterHeadClosed\" class=\"closed\" style=\"display:none;\">"
										+ "\r" + "<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><img title=\"Open Navigator\" alt=\"Open Filter\" src=\"/ccLib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
										+ Environment.NewLine + "</div>"
										+ Environment.NewLine + "</div>"
										+ "";
									IndexFilterContent = ""
										+ Environment.NewLine + "<div class=\"ccContentCon\">"
										+ Environment.NewLine + "<div id=\"IndexFilterContentOpened\" class=\"opened\">" + GetForm_IndexFilterContent(adminContent) + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
										+ Environment.NewLine + "<div id=\"IndexFilterContentClosed\" class=\"closed\" style=\"display:none;\">" + FilterClosedLabel + "</div>"
										+ Environment.NewLine + "</div>";
									IndexFilterJS = ""
										+ Environment.NewLine + "<script Language=\"JavaScript\" type=\"text/javascript\">"
										+ Environment.NewLine + "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
										+ Environment.NewLine + "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentClosed','none');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','')}"
										+ Environment.NewLine + "</script>";
								}
								else
								{
									//
									// Ajax Filter Closed
									//
									IndexFilterHead = ""
										+ Environment.NewLine + "<div class=\"ccHeaderCon\">"
										+ Environment.NewLine + "<div id=\"IndexFilterHeCursorTypeEnum.ADOPENed\" class=\"opened\" style=\"display:none;\">"
										+ "\r" + "<table border=0 cellpadding=0 cellspacing=0 width=\"100%\"><tr>"
										+ "\r" + "<td valign=Middle class=\"left\">Filter</td>"
										+ "\r" + "<td valign=Middle class=\"right\"><a href=\"#\" onClick=\"CloseIndexFilter();return false\"><img alt=\"Close Filter\" title=\"Close Navigator\" src=\"/ccLib/images/ClosexRev1313.gif\" width=13 height=13 border=0></a></td>"
										+ "\r" + "</tr></table>"
										+ Environment.NewLine + "</div>"
										+ Environment.NewLine + "<div id=\"IndexFilterHeadClosed\" class=\"closed\">"
										+ "\r" + "<a href=\"#\" onClick=\"OpenIndexFilter();return false\"><img title=\"Open Navigator\" alt=\"Open Navigator\" src=\"/ccLib/images/OpenRightRev1313.gif\" width=13 height=13 border=0 style=\"text-align:right;\"></a>"
										+ Environment.NewLine + "</div>"
										+ Environment.NewLine + "</div>"
										+ "";
									IndexFilterContent = ""
										+ Environment.NewLine + "<div class=\"ccContentCon\">"
										+ Environment.NewLine + "<div id=\"IndexFilterContentOpened\" class=\"opened\" style=\"display:none;\"><div style=\"text-align:center;\"><img src=\"/ccLib/images/ajax-loader-small.gif\" width=16 height=16></div></div>"
										+ Environment.NewLine + "<div id=\"IndexFilterContentClosed\" class=\"closed\">" + FilterClosedLabel + "</div>"
										+ Environment.NewLine + "<div id=\"IndexFilterContentMinWidth\" style=\"display:none;\"><img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"200\" height=\"1\" style=\"clear:both\"></div>"
										+ Environment.NewLine + "</div>";
									AjaxQS = cpCore.doc.refreshQueryString;
									AjaxQS = genericController.ModifyQueryString(AjaxQS, RequestNameAjaxFunction, AjaxOpenIndexFilterGetContent);
									IndexFilterJS = ""
										+ Environment.NewLine + "<script Language=\"JavaScript\" type=\"text/javascript\">"
										+ Environment.NewLine + "var IndexFilterPop=false;"
										+ Environment.NewLine + "function CloseIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','none');SetDisplay('IndexFilterHeadClosed','block');SetDisplay('IndexFilterContentOpened','none');SetDisplay('IndexFilterContentMinWidth','none');SetDisplay('IndexFilterContentClosed','block');cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxCloseIndexFilter + "','','')}"
										+ Environment.NewLine + "function OpenIndexFilter() {SetDisplay('IndexFilterHeCursorTypeEnum.ADOPENed','block');SetDisplay('IndexFilterHeadClosed','none');SetDisplay('IndexFilterContentOpened','block');SetDisplay('IndexFilterContentMinWidth','block');SetDisplay('IndexFilterContentClosed','none');if(!IndexFilterPop){cj.ajax.qs('" + AjaxQS + "','','IndexFilterContentOpened');IndexFilterPop=true;}else{cj.ajax.qs('" + RequestNameAjaxFunction + "=" + AjaxOpenIndexFilter + "','','');}}"
										+ Environment.NewLine + "</script>";
								}
							}
							//
							// Dual Window Right - Data
							//
							FilterDataTable += "<td valign=top class=\"ccPanel\">";
							//
							DataTable_HdrRow += "<tr>";
							//
							// Row Number Column
							//
							DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\">Row</td>";
							//
							// Delete Select Box Columns
							//
							if (!AllowDelete)
							{
								DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\"><input TYPE=CheckBox disabled=\"disabled\"></td>";
							}
							else
							{
								DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\"><input TYPE=CheckBox OnClick=\"CheckInputs('DelCheck',this.checked);\"></td>";
							}
							//
							// Calculate total width
							//
							ColumnWidthTotal = 0;
							foreach (var kvp in IndexConfig.Columns)
							{
								indexConfigColumnClass column = kvp.Value;
								if (column.Width < 1)
								{
									column.Width = 1;
								}
								ColumnWidthTotal = ColumnWidthTotal + column.Width;
							}
							//
							// Edit Column
							//
							DataTable_HdrRow += "<td width=20 align=center valign=bottom class=\"ccAdminListCaption\">Edit</td>";
							foreach (var kvp in IndexConfig.Columns)
							{
								indexConfigColumnClass column = kvp.Value;
								//
								// ----- print column headers - anchored so they sort columns
								//
								ColumnWidth = Convert.ToInt32((100 * column.Width) / (double)ColumnWidthTotal);
								//fieldId = column.FieldId
								FieldName = column.Name;
								//
								//if this is a current sort ,add the reverse flag
								//
								ButtonHref = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameAdminForm + "=" + AdminFormIndex + "&SetSortField=" + FieldName + "&RT=0&" + RequestNameTitleExtension + "=" + genericController.EncodeRequestVariable(TitleExtension) + "&cid=" + adminContent.Id + "&ad=" + MenuDepth;
								foreach (var sortKvp in IndexConfig.Sorts)
								{
									indexConfigSortClass sort = sortKvp.Value;

								}
								if (!IndexConfig.Sorts.ContainsKey(FieldName))
								{
									ButtonHref += "&SetSortDirection=1";
								}
								else
								{
									switch (IndexConfig.Sorts(FieldName).direction)
									{
										case 1:
											ButtonHref += "&SetSortDirection=2";
											break;
										case 2:
											ButtonHref += "&SetSortDirection=0";
											break;
										default:
										break;
									}
								}
								//
								//----- column header includes WherePairCount
								//
								if (WherePairCount > 0)
								{
									for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++)
									{
										if (WherePair(0, WhereCount) != "")
										{
											ButtonHref += "&wl" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair(0, WhereCount));
											ButtonHref += "&wr" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair(1, WhereCount));
										}
									}
								}
								ButtonFace = adminContent.fields(FieldName.ToLower()).caption;
								ButtonFace = genericController.vbReplace(ButtonFace, " ", "&nbsp;");
								SortTitle = "Sort A-Z";
								//
								if (IndexConfig.Sorts.ContainsKey(FieldName))
								{
									switch (IndexConfig.Sorts(FieldName).direction)
									{
										case 1:
											ButtonFace = ButtonFace + "<img src=\"/ccLib/images/arrowdown.gif\" width=8 height=8 border=0>";
											SortTitle = "Sort Z-A";
											break;
										case 2:
											ButtonFace = ButtonFace + "<img src=\"/ccLib/images/arrowup.gif\" width=8 height=8 border=0>";
											SortTitle = "Remove Sort";
											break;
										default:
										break;
									}
								}
								ButtonObject = "Button" + ButtonObjectCount;
								ButtonObjectCount = ButtonObjectCount + 1;
								DataTable_HdrRow += "<td width=\"" + ColumnWidth + "%\" valign=bottom align=left class=\"ccAdminListCaption\">";
								DataTable_HdrRow += ("<a title=\"" + SortTitle + "\" href=\"" + genericController.encodeHTML(ButtonHref) + "\" class=\"ccAdminListCaption\">" + ButtonFace + "</A>");
								DataTable_HdrRow += ("</td>");
							}
							DataTable_HdrRow += ("</tr>");
							//
							//   select and print Records
							//
							//DataSourceName = cpCore.db.getDataSourceNameByID(adminContent.dataSourceId)
							CS = cpCore.db.csOpenSql(SQL, datasource.Name, IndexConfig.RecordsPerPage, IndexConfig.PageNumber);
							if (cpCore.db.csOk(CS))
							{
								RowColor = "";
								RecordPointer = IndexConfig.RecordTop;
								RecordLast = IndexConfig.RecordTop + IndexConfig.RecordsPerPage;
								//
								// --- Print out the records
								//
								//IsPageContent = (LCase(adminContent.ContentTableName) = "ccpagecontent")
								//If IsPageContent Then
								//    LandingPageID = cpCore.main_GetLandingPageID
								//End If
								while ((cpCore.db.csOk(CS)) && (RecordPointer < RecordLast))
								{
									RecordID = cpCore.db.csGetInteger(CS, "ID");
									RecordName = cpCore.db.csGetText(CS, "name");
									//IsLandingPage = IsPageContent And (RecordID = LandingPageID)
									if (RowColor == "class=\"ccAdminListRowOdd\"")
									{
										RowColor = "class=\"ccAdminListRowEven\"";
									}
									else
									{
										RowColor = "class=\"ccAdminListRowOdd\"";
									}
									DataTable_DataRows += Environment.NewLine + "<tr>";
									//
									// --- Record Number column
									//
									DataTable_DataRows += "<td align=right " + RowColor + ">" + SpanClassAdminSmall + "[" + (RecordPointer + 1) + "]</span></td>";
									//
									// --- Delete Checkbox Columns
									//
									if (AllowDelete)
									{
										//If AllowDelete And Not IsLandingPage Then
										//If AdminContent.AllowDelete And Not IsLandingPage Then
										DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox NAME=row" + RecordPointer + " VALUE=1 ID=\"DelCheck\"><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
									}
									else
									{
										DataTable_DataRows += "<td align=center " + RowColor + "><input TYPE=CheckBox disabled=\"disabled\" NAME=row" + RecordPointer + " VALUE=1><input type=hidden name=rowid" + RecordPointer + " VALUE=" + RecordID + "></span></td>";
									}
									//
									// --- Edit button column
									//
									DataTable_DataRows += "<td align=center " + RowColor + ">";
									URI = "\\" + cpCore.serverConfig.appConfig.adminRoute + "?" + RequestNameAdminAction + "=" + AdminActionNop + "&cid=" + adminContent.Id + "&id=" + RecordID + "&" + RequestNameTitleExtension + "=" + genericController.EncodeRequestVariable(TitleExtension) + "&ad=" + MenuDepth + "&" + RequestNameAdminSourceForm + "=" + AdminForm + "&" + RequestNameAdminForm + "=" + AdminFormEdit;
									if (WherePairCount > 0)
									{
										for (WhereCount = 0; WhereCount < WherePairCount; WhereCount++)
										{
											URI = URI + "&wl" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair(0, WhereCount)) + "&wr" + WhereCount + "=" + genericController.EncodeRequestVariable(WherePair(1, WhereCount));
										}
									}
									DataTable_DataRows += ("<a href=\"" + genericController.encodeHTML(URI) + "\"><img src=\"/ccLib/images/IconContentEdit.gif\" border=\"0\"></a>");
									DataTable_DataRows += ("</td>");
									//
									// --- field columns
									//
									foreach (var columnKvp in IndexConfig.Columns)
									{
										indexConfigColumnClass column = columnKvp.Value;
										string columnNameLc = column.Name.ToLower();
										if (FieldUsedInColumns.ContainsKey(columnNameLc))
										{
											if (FieldUsedInColumns.Item(columnNameLc))
											{
												DataTable_DataRows += (Environment.NewLine + "<td valign=\"middle\" " + RowColor + " align=\"left\">" + SpanClassAdminNormal);
												DataTable_DataRows += GetForm_Index_GetCell(adminContent, editRecord, column.Name, CS, IsLookupFieldValid(columnNameLc), genericController.vbLCase(adminContent.ContentTableName) == "ccemail");
												DataTable_DataRows += ("&nbsp;</span></td>");
											}
										}
									}
									DataTable_DataRows += ("\n" + "    </tr>");
									cpCore.db.csGoNext(CS);
									RecordPointer = RecordPointer + 1;
								}
								DataTable_DataRows += "<input type=hidden name=rowcnt value=" + RecordPointer + ">";
								//
								// --- print out the stuff at the bottom
								//
								RecordTop_NextPage = IndexConfig.RecordTop;
								if (cpCore.db.csOk(CS))
								{
									RecordTop_NextPage = RecordPointer;
								}
								RecordTop_PreviousPage = IndexConfig.RecordTop - IndexConfig.RecordsPerPage;
								if (RecordTop_PreviousPage < 0)
								{
									RecordTop_PreviousPage = 0;
								}
							}
							cpCore.db.csClose(CS);
							//
							// Header at bottom
							//
							if (RowColor == "class=\"ccAdminListRowOdd\"")
							{
								RowColor = "class=\"ccAdminListRowEven\"";
							}
							else
							{
								RowColor = "class=\"ccAdminListRowOdd\"";
							}
							if (RecordPointer == 0)
							{
								//
								// No records found
								//
								DataTable_DataRows += ("<tr>" + "<td " + RowColor + " align=center>-</td>" + "<td " + RowColor + " align=center>-</td>" + "<td " + RowColor + " align=center>-</td>" + "<td colspan=" + IndexConfig.Columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">no records were found</td>" + "</tr>");
							}
							else
							{
								if (RecordPointer < RecordLast)
								{
									//
									// End of list
									//
									DataTable_DataRows += ("<tr>" + "<td " + RowColor + " align=center>-</td>" + "<td " + RowColor + " align=center>-</td>" + "<td " + RowColor + " align=center>-</td>" + "<td colspan=" + IndexConfig.Columns.Count + " " + RowColor + " style=\"text-align:left ! important;\">----- end of list</td>" + "</tr>");
								}
								//
								// Add another header to the data rows
								//
								DataTable_DataRows += DataTable_HdrRow;
							}
							//'
							//' ----- DataTable_FindRow
							//'
							//ReDim Findstring(IndexConfig.Columns.Count)
							//For ColumnPointer = 0 To IndexConfig.Columns.Count - 1
							//    FieldName = IndexConfig.Columns(ColumnPointer).Name
							//    If genericController.vbLCase(FieldName) = FindWordName Then
							//        Findstring(ColumnPointer) = FindWordValue
							//    End If
							//Next
							//        ReDim Findstring(CustomAdminColumnCount)
							//        For ColumnPointer = 0 To CustomAdminColumnCount - 1
							//            FieldPtr = CustomAdminColumn(ColumnPointer).FieldPointer
							//            With AdminContent.fields(FieldPtr)
							//                If genericController.vbLCase(.Name) = FindWordName Then
							//                    Findstring(ColumnPointer) = FindWordValue
							//                End If
							//            End With
							//        Next
							//
							DataTable_FindRow = DataTable_FindRow + "<tr><td colspan=" + (3 + IndexConfig.Columns.Count) + " style=\"background-color:black;height:1;\"></td></tr>";
							//DataTable_FindRow = DataTable_FindRow & "<tr><td colspan=" & (3 + CustomAdminColumnCount) & " style=""background-color:black;height:1;""><img alt=""space"" src=""/ccLib/images/spacer.gif"" width=""1"" height=""1"" ></td></tr>"
							DataTable_FindRow = DataTable_FindRow + "<tr>";
							DataTable_FindRow = DataTable_FindRow + "<td colspan=3 width=\"60\" class=\"ccPanel\" align=center style=\"text-align:center ! important;\">";
							DataTable_FindRow = DataTable_FindRow + Environment.NewLine + "<script language=\"javascript\" type=\"text/javascript\">"
								+ Environment.NewLine + "function KeyCheck(e){"
								+ Environment.NewLine + "  var code = e.keyCode;"
								+ Environment.NewLine + "  if(code==13){"
								+ Environment.NewLine + "    document.getElementById('FindButton').focus();"
								+ Environment.NewLine + "    document.getElementById('FindButton').click();"
								+ Environment.NewLine + "    return false;"
								+ Environment.NewLine + "  }"
								+ Environment.NewLine + "} "
								+ Environment.NewLine + "</script>";
							DataTable_FindRow = DataTable_FindRow + "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"60\" height=\"1\" ><br >" + cpCore.html.html_GetFormButton(ButtonFind,, "FindButton") + "</td>";
							ColumnPointer = 0;
							foreach (var kvp in IndexConfig.Columns)
							{
								indexConfigColumnClass column = kvp.Value;
								//For ColumnPointer = 0 To CustomAdminColumnCount - 1
								ColumnWidth = column.Width;
								//fieldId = .FieldId
								FieldName = genericController.vbLCase(column.Name);
								FindWordValue = "";
								if (IndexConfig.FindWords.ContainsKey(FieldName))
								{
									var tempVar = IndexConfig.FindWords(FieldName);
									if ((tempVar.MatchOption == FindWordMatchEnum.matchincludes) || (tempVar.MatchOption == FindWordMatchEnum.MatchEquals))
									{
										FindWordValue = tempVar.Value;
									}
								}
								DataTable_FindRow = DataTable_FindRow + Environment.NewLine + "<td valign=\"top\" align=\"center\" class=\"ccPanel3DReverse\" style=\"padding-top:2px;padding-bottom:2px;\">"
									+ "<input type=hidden name=\"FindName" + ColumnPointer + "\" value=\"" + FieldName + "\">"
									+ "<input onkeypress=\"KeyCheck(event);\"  type=text id=\"F" + ColumnPointer + "\" name=\"FindValue" + ColumnPointer + "\" value=\"" + FindWordValue + "\" style=\"width:98%\">"
									+ "</td>";
								ColumnPointer += 1;
							}
							DataTable_FindRow = DataTable_FindRow + "</tr>";
							//
							// Assemble DataTable
							//
							DataTable = ""
								+ "<table ID=\"DataTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
								+ DataTable_HdrRow + DataTable_DataRows + DataTable_FindRow + "</table>";
							//DataTable = GetForm_Index_AdvancedSearch()
							//
							// Assemble DataFilterTable
							//
							if (!string.IsNullOrEmpty(IndexFilterContent))
							{
								FilterColumn = "<td valign=top style=\"border-right:1px solid black;\" class=\"ccToolsCon\">" + IndexFilterJS + IndexFilterHead + IndexFilterContent + "</td>";
								//FilterColumn = "<td valign=top class=""ccPanel3DReverse ccAdminEditBody"" style=""border-right:1px solid black;"">" & IndexFilterJS & IndexFilterHead & IndexFilterContent & "</td>"
							}
							DataColumn = "<td width=\"99%\" valign=top>" + DataTable + "</td>";
							FilterDataTable = ""
								+ "<table ID=\"DataFilterTable\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"Background-Color:white;\">"
								+ "<tr>"
								+ FilterColumn + DataColumn + "</tr>"
								+ "</table>";
							//
							// Assemble LiveWindowTable
							//
							// Stream.Add( OpenLiveWindowTable)
							Stream.Add(Environment.NewLine + cpCore.html.html_GetFormStart(, "adminForm"));
							Stream.Add("<input type=\"hidden\" name=\"indexGoToPage\" value=\"\">");
							Stream.Add(ButtonBar);
							Stream.Add(Adminui.GetTitleBar(TitleBar, HeaderDescription));
							Stream.Add(FilterDataTable);
							Stream.Add(ButtonBar);
							Stream.Add(cpCore.html.main_GetPanel("<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"1\", height=\"10\" >"));
							Stream.Add("<input type=hidden name=Columncnt VALUE=" + IndexConfig.Columns.Count + ">");
							Stream.Add("</form>");
							//  Stream.Add( CloseLiveWindowTable)
							cpCore.html.addTitle(adminContent.Name);
						}
					}
					//End If
					//
				}
				returnForm = Stream.Text;
				//
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnForm;
		}
		//
		//========================================================================
		// ----- Get an XML nodes attribute based on its name
		//========================================================================
		//
		private string GetXMLAttribute(bool Found, XmlNode Node, string Name, string DefaultIfNotFound)
		{
				string tempGetXMLAttribute = null;
			try
			{
				//
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlAttribute NodeAttribute = null;
				XmlNode ResultNode = null;
				string UcaseName = null;
				//
				Found = false;
				ResultNode = Node.Attributes.GetNamedItem(Name);
				if (ResultNode == null)
				{
					UcaseName = genericController.vbUCase(Name);
					foreach (XmlAttribute NodeAttribute in Node.Attributes)
					{
						if (genericController.vbUCase(NodeAttribute.Name) == UcaseName)
						{
							tempGetXMLAttribute = NodeAttribute.Value;
							Found = true;
							break;
						}
					}
				}
				else
				{
					tempGetXMLAttribute = ResultNode.Value;
					Found = true;
				}
				if (!Found)
				{
					tempGetXMLAttribute = DefaultIfNotFound;
				}
				return tempGetXMLAttribute;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			handleLegacyClassError3("GetXMLAttribute");
			return tempGetXMLAttribute;
		}
		//
		// REFACTOR -- THIS SHOULD BE A REMOTE METHOD AND NOT CALLED FROM CPCORE.
		//==========================================================================================================================================
		/// <summary>
		/// Get index view filter content - remote method
		/// </summary>
		/// <param name="adminContent"></param>
		/// <returns></returns>
		public string GetForm_IndexFilterContent(Models.Complex.cdefModel adminContent)
		{
			string returnContent = "";
			try
			{
				int RecordID = 0;
				string Name = null;
				string TableName = null;
				string FieldCaption = null;
				string ContentName = null;
				int CS = 0;
				string SQL = null;
				string Caption = null;
				string Link = null;
				bool IsAuthoringMode = false;
				string FirstCaption = "";
				string RQS = null;
				string QS = null;
				int Ptr = 0;
				string SubFilterList = null;
				indexConfigClass IndexConfig = null;
				string list = null;
				string[] ListSplit = null;
				int Cnt = 0;
				int Pos = 0;
				int subContentID = 0;
				//
				IndexConfig = LoadIndexConfig(adminContent);
				//
				ContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, adminContent.Id);
				IsAuthoringMode = true;
				RQS = "cid=" + adminContent.Id + "&af=1";
				//
				//-------------------------------------------------------------------------------------
				// Remove filters
				//-------------------------------------------------------------------------------------
				//
				if ((IndexConfig.SubCDefID > 0) || (IndexConfig.GroupListCnt != 0) | (IndexConfig.FindWords.Count != 0) | IndexConfig.ActiveOnly | IndexConfig.LastEditedByMe | IndexConfig.LastEditedToday | IndexConfig.LastEditedPast7Days | IndexConfig.LastEditedPast30Days)
				{
					//
					// Remove Filters
					//
					returnContent += "<div class=\"ccFilterHead\">Remove&nbsp;Filters</div>";
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveAll", "1");
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					returnContent += "<div class=\"ccFilterSubHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;Remove All</a></div>";
					//
					// Last Edited Edited by me
					//
					SubFilterList = "";
					if (IndexConfig.LastEditedByMe)
					{
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", 0.ToString(), true);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">By&nbsp;Me</a></div>";
					}
					if (IndexConfig.LastEditedToday)
					{
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", 0.ToString(), true);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Today</a></div>";
					}
					if (IndexConfig.LastEditedPast7Days)
					{
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", 0.ToString(), true);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Past Week</a></div>";
					}
					if (IndexConfig.LastEditedPast30Days)
					{
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", 0.ToString(), true);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Past 30 Days</a></div>";
					}
					if (!string.IsNullOrEmpty(SubFilterList))
					{
						returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
					}
					//
					// Sub Content definitions
					//
					string SubContentName = null;
					SubFilterList = "";
					if (IndexConfig.SubCDefID > 0)
					{
						SubContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, IndexConfig.SubCDefID);
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveCDef", Convert.ToString(IndexConfig.SubCDefID));
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">" + SubContentName + "</a></div>";
					}
					if (!string.IsNullOrEmpty(SubFilterList))
					{
						returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
					}
					//
					// Group Filter List
					//
					string GroupName = null;
					SubFilterList = "";
					if (IndexConfig.GroupListCnt > 0)
					{
						for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++)
						{
							GroupName = IndexConfig.GroupList(Ptr);
							if (IndexConfig.GroupList(Ptr) != "")
							{
								if (GroupName.Length > 30)
								{
									GroupName = GroupName.Substring(0, 15) + "..." + GroupName.Substring(GroupName.Length - 15);
								}
								QS = RQS;
								QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveGroup", IndexConfig.GroupList(Ptr));
								Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
								SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">" + GroupName + "</a></div>";
							}
						}
					}
					if (!string.IsNullOrEmpty(SubFilterList))
					{
						returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
					}
					//
					// Other Filter List
					//
					SubFilterList = "";
					if (IndexConfig.ActiveOnly)
					{
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", 0.ToString(), true);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						SubFilterList = SubFilterList + "<div class=\"ccFilterIndent ccFilterList\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">Active&nbsp;Only</a></div>";
					}
					if (!string.IsNullOrEmpty(SubFilterList))
					{
						returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
					}
					//
					// FindWords
					//
					foreach (var findWordKvp in IndexConfig.FindWords)
					{
						indexConfigFindWordClass findWord = findWordKvp.Value;
						FieldCaption = genericController.encodeText(Models.Complex.cdefModel.GetContentFieldProperty(cpCore, ContentName, findWord.Name, "caption"));
						QS = RQS;
						QS = genericController.ModifyQueryString(QS, "IndexFilterRemoveFind", findWord.Name);
						Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
						switch (findWord.MatchOption)
						{
							case FindWordMatchEnum.matchincludes:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;includes&nbsp;'" + findWord.Value + "'</a></div>";
								break;
							case FindWordMatchEnum.MatchEmpty:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;empty</a></div>";
								break;
							case FindWordMatchEnum.MatchEquals:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;=&nbsp;'" + findWord.Value + "'</a></div>";
								break;
							case FindWordMatchEnum.MatchFalse:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;false</a></div>";
								break;
							case FindWordMatchEnum.MatchGreaterThan:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;&gt;&nbsp;'" + findWord.Value + "'</a></div>";
								break;
							case FindWordMatchEnum.MatchLessThan:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;&lt;&nbsp;'" + findWord.Value + "'</a></div>";
								break;
							case FindWordMatchEnum.MatchNotEmpty:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;not&nbsp;empty</a></div>";
								break;
							case FindWordMatchEnum.MatchTrue:
								returnContent += "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\"><img src=\"/ccLib/images/delete1313.gif\" width=13 height=13 border=0 style=\"vertical-align:middle;\">&nbsp;" + FieldCaption + "&nbsp;is&nbsp;true</a></div>";
								break;
						}
					}
					//
					returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				}
				//
				//-------------------------------------------------------------------------------------
				// Add filters
				//-------------------------------------------------------------------------------------
				//
				returnContent += "<div class=\"ccFilterHead\">Add&nbsp;Filters</div>";
				//
				// Last Edited
				//
				SubFilterList = "";
				if (!IndexConfig.LastEditedByMe)
				{
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedByMe", "1", true);
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">By&nbsp;Me</a></div>";
				}
				if (!IndexConfig.LastEditedToday)
				{
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedToday", "1", true);
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Today</a></div>";
				}
				if (!IndexConfig.LastEditedPast7Days)
				{
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast7Days", "1", true);
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past Week</a></div>";
				}
				if (!IndexConfig.LastEditedPast30Days)
				{
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterLastEditedPast30Days", "1", true);
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Past 30 Days</a></div>";
				}
				if (!string.IsNullOrEmpty(SubFilterList))
				{
					returnContent += "<div class=\"ccFilterSubHead\">Last&nbsp;Edited</div>" + SubFilterList;
				}
				//
				// Sub Content Definitions
				//
				SubFilterList = "";
				list = Models.Complex.cdefModel.getContentControlCriteria(cpCore, ContentName);
				if (!string.IsNullOrEmpty(list))
				{
					ListSplit = list.Split('=');
					Cnt = ListSplit.GetUpperBound(0) + 1;
					if (Cnt > 0)
					{
						for (Ptr = 0; Ptr < Cnt; Ptr++)
						{
							Pos = genericController.vbInstr(1, ListSplit[Ptr], ")");
							if (Pos > 0)
							{
								subContentID = genericController.EncodeInteger(ListSplit[Ptr].Substring(0, Pos - 1));
								if (subContentID > 0 && (subContentID != adminContent.Id) & (subContentID != IndexConfig.SubCDefID))
								{
									Caption = "<span style=\"white-space:nowrap;\">" + Models.Complex.cdefModel.getContentNameByID(cpCore, subContentID) + "</span>";
									QS = RQS;
									QS = genericController.ModifyQueryString(QS, "IndexFilterAddCDef", subContentID.ToString(), true);
									Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
									SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
								}
							}
						}
					}
				}
				if (!string.IsNullOrEmpty(SubFilterList))
				{
					returnContent += "<div class=\"ccFilterSubHead\">In Sub-content</div>" + SubFilterList;
				}
				//
				// people filters
				//
				TableName = Models.Complex.cdefModel.getContentTablename(cpCore, ContentName);
				SubFilterList = "";
				if (genericController.vbLCase(TableName) == genericController.vbLCase("ccMembers"))
				{
					SQL = cpCore.db.GetSQLSelect("default", "ccGroups", "ID,Caption,Name", "(active<>0)", "Caption,Name");
					CS = cpCore.db.csOpenSql_rev("default", SQL);
					while (cpCore.db.csOk(CS))
					{
						Name = cpCore.db.csGetText(CS, "Name");
						Ptr = 0;
						if (IndexConfig.GroupListCnt > 0)
						{
							for (Ptr = 0; Ptr < IndexConfig.GroupListCnt; Ptr++)
							{
								if (Name == IndexConfig.GroupList(Ptr))
								{
									break;
								}
							}
						}
						if (Ptr == IndexConfig.GroupListCnt)
						{
							RecordID = cpCore.db.csGetInteger(CS, "ID");
							Caption = cpCore.db.csGetText(CS, "Caption");
							if (string.IsNullOrEmpty(Caption))
							{
								Caption = Name;
								if (string.IsNullOrEmpty(Caption))
								{
									Caption = "Group " + RecordID;
								}
							}
							if (Caption.Length > 30)
							{
								Caption = Caption.Substring(0, 15) + "..." + Caption.Substring(Caption.Length - 15);
							}
							Caption = "<span style=\"white-space:nowrap;\">" + Caption + "</span>";
							QS = RQS;
							if (!string.IsNullOrEmpty(Name.Trim(' ')))
							{
								QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", Name, true);
							}
							else
							{
								QS = genericController.ModifyQueryString(QS, "IndexFilterAddGroup", RecordID.ToString(), true);
							}
							Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
							SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">" + Caption + "</a></div>";
						}
						cpCore.db.csGoNext(CS);
					}
				}
				if (!string.IsNullOrEmpty(SubFilterList))
				{
					returnContent += "<div class=\"ccFilterSubHead\">In Group(s)</div>" + SubFilterList;
				}
				//
				// Active Only
				//
				SubFilterList = "";
				if (!IndexConfig.ActiveOnly)
				{
					QS = RQS;
					QS = genericController.ModifyQueryString(QS, "IndexFilterActiveOnly", "1", true);
					Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
					SubFilterList = SubFilterList + "<div class=\"ccFilterIndent\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Active&nbsp;Only</a></div>";
				}
				if (!string.IsNullOrEmpty(SubFilterList))
				{
					returnContent += "<div class=\"ccFilterSubHead\">Other</div>" + SubFilterList;
				}
				//
				returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				//
				// Advanced Search Link
				//
				QS = RQS;
				QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormAdvancedSearch, true);
				Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
				returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Advanced&nbsp;Search</a></div>";
				//
				returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				//
				// Set Column Link
				//
				QS = RQS;
				QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormSetColumns, true);
				Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
				returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Set&nbsp;Columns</a></div>";
				//
				returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				//
				// Import Link
				//
				QS = RQS;
				QS = genericController.ModifyQueryString(QS, RequestNameAdminForm, AdminFormImportWizard, true);
				Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
				returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Import</a></div>";
				//
				returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				//
				// Export Link
				//
				QS = RQS;
				QS = genericController.ModifyQueryString(QS, RequestNameAdminSubForm, AdminFormIndex_SubFormExport, true);
				Link = "/" + cpCore.serverConfig.appConfig.adminRoute + "?" + QS;
				returnContent += "<div class=\"ccFilterHead\"><a class=\"ccFilterLink\" href=\"" + Link + "\">Export</a></div>";
				//
				returnContent += "<div style=\"border-bottom:1px dotted #808080;\">&nbsp;</div>";
				//
				returnContent = "<div style=\"padding-left:10px;padding-right:10px;\">" + returnContent + "</div>";
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnContent;
		}
		//
	}
}
