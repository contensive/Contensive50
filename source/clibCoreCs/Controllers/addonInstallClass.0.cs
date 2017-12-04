using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using System.Xml;
using Contensive.Core.Controllers;
using Contensive.Core.Controllers.genericController;
using Contensive.Core.Models;
using Contensive.Core.Models.Entity;
//
namespace Contensive.Core
{
	//
	//====================================================================================================
	/// <summary>
	/// install addon collections
	/// </summary>
	public class addonInstallClass
	{
		//
		//==========================================================================================================================
		//   Overlay a Src CDef on to the current one (Dst)
		//       Any Src CDEf entries found in Src are added to Dst.
		//       if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
		//
		//       isBaseContent
		//           if dst not found, it is created to match src
		//           if dst found, it is updated only if isBase matches
		//               content attributes updated if .isBaseContent matches
		//               field attributes updated if .isBaseField matches
		//==========================================================================================================================
		//
		private static bool installCollection_AddMiniCollectionSrcToDst(coreClass cpCore, ref miniCollectionModel dstCollection, miniCollectionModel srcCollection, bool SrcIsUserCDef)
		{
			bool returnOk = true;
			try
			{
				string HelpSrc = null;
				bool HelpCustomChanged = false;
				bool HelpDefaultChanged = false;
				bool HelpChanged = false;
				string Copy = null;
				string n = null;
				Models.Complex.CDefFieldModel srcCollectionCdefField = null;
				Models.Complex.cdefModel dstCollectionCdef = null;
				Models.Complex.CDefFieldModel dstCollectionCdefField = null;
				bool IsMatch = false;
				string DstKey = null;
				string SrcKey = null;
				string DataBuildVersion = null;
				bool SrcIsNavigator = false;
				bool DstIsNavigator = false;
				string SrcContentName = null;
				string DstName = null;
				string SrcFieldName = null;
				bool okToUpdateDstFromSrc = false;
				Models.Complex.cdefModel srcCollectionCdef = null;
				bool DebugSrcFound = false;
				bool DebugDstFound = false;
				//
				// If the Src is the BaseCollection, the Dst must be the Application Collectio
				//   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
				//
				if (srcCollection.isBaseCollection)
				{
					foreach (var dstKeyValuePair in dstCollection.CDef)
					{
						Models.Complex.cdefModel dstWorkingCdef = dstKeyValuePair.Value;
						string contentName = dstWorkingCdef.Name;
						if (dstCollection.CDef(contentName.ToLower()).IsBaseContent)
						{
							//
							// this application collection Cdef is marked base, verify it is in the base collection
							//
							if (!srcCollection.CDef.ContainsKey(contentName.ToLower()))
							{
								//
								// cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
								//
								var tempVar = dstCollection.CDef(contentName.ToLower());
								tempVar.IsBaseContent = false;
								tempVar.dataChanged = true;
								foreach (var dstFieldKeyValuePair in tempVar.fields)
								{
									Models.Complex.CDefFieldModel field = dstFieldKeyValuePair.Value;
									if (field.isBaseField)
									{
										field.isBaseField = false;
										//field.Changed = True
									}
								}
							}
						}
					}
				}
				//
				//
				// -------------------------------------------------------------------------------------------------
				// Go through all CollectionSrc and find the CollectionDst match
				//   if it is an exact match, do nothing
				//   if the cdef does not match, set cdefext(ptr).CDefChanged true
				//   if any field does not match, set cdefext...field...CDefChanged
				//   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
				// -------------------------------------------------------------------------------------------------
				//
				logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name + ", UpgradeCDef_AddSrcToDst");
				//
				foreach (var srcKeyValuePair in srcCollection.CDef)
				{
					srcCollectionCdef = srcKeyValuePair.Value;

					SrcContentName = srcCollectionCdef.Name;
					//If genericController.vbLCase(SrcContentName) = "site sections" Then
					//    SrcContentName = SrcContentName
					//End If
					DebugSrcFound = false;
					if (genericController.vbInstr(1, SrcContentName, cnNavigatorEntries, Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
					{
						DebugSrcFound = true;
					}
					//
					// Search for this cdef in the Dst
					//
					okToUpdateDstFromSrc = false;
					if (!dstCollection.CDef.ContainsKey(SrcContentName.ToLower()))
					{
						//
						// add src to dst
						//
						dstCollection.CDef.Add(SrcContentName.ToLower(), new Models.Complex.cdefModel());
						okToUpdateDstFromSrc = true;
					}
					else
					{
						dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower());
						DstName = SrcContentName;
						//
						// found a match between Src and Dst
						//
						if (dstCollectionCdef.IsBaseContent == srcCollectionCdef.IsBaseContent)
						{
							//
							// Allow changes to user cdef only from user cdef, changes to base only from base
							//
							n = "ActiveOnly";
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.ActiveOnly != srcCollectionCdef.ActiveOnly);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AdminOnly";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AdminOnly != srcCollectionCdef.AdminOnly);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "DeveloperOnly";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.DeveloperOnly != srcCollectionCdef.DeveloperOnly);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowAdd";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowAdd != srcCollectionCdef.AllowAdd);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowCalendarEvents";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowCalendarEvents != srcCollectionCdef.AllowCalendarEvents);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowContentTracking";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowContentTracking != srcCollectionCdef.AllowContentTracking);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowDelete";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowDelete != srcCollectionCdef.AllowDelete);
							//
							//If Not okToUpdateDstFromSrc Then n = "AllowMetaContent"
							//okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowMetaContent <> srcCollectionCdef.AllowMetaContent)
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowTopicRules";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowTopicRules != srcCollectionCdef.AllowTopicRules);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "ContentDataSourceName";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.ContentDataSourceName, srcCollectionCdef.ContentDataSourceName);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "ContentTableName";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.ContentTableName, srcCollectionCdef.ContentTableName);
							//
							if (DebugDstFound)
							{
								DebugDstFound = DebugDstFound;
							}
							if (!okToUpdateDstFromSrc)
							{
								n = "DefaultSortMethod";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.DefaultSortMethod, srcCollectionCdef.DefaultSortMethod);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "DropDownFieldList";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.DropDownFieldList, srcCollectionCdef.DropDownFieldList);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "EditorGroupName";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.EditorGroupName, srcCollectionCdef.EditorGroupName);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IgnoreContentControl";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IgnoreContentControl != srcCollectionCdef.IgnoreContentControl);
							if (okToUpdateDstFromSrc)
							{
								okToUpdateDstFromSrc = okToUpdateDstFromSrc;
							}
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "Active";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.Active != srcCollectionCdef.Active);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "AllowContentChildTool";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowContentChildTool != srcCollectionCdef.AllowContentChildTool);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "ParentId";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.parentID != srcCollectionCdef.parentID);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IconLink";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.IconLink, srcCollectionCdef.IconLink);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IconHeight";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconHeight != srcCollectionCdef.IconHeight);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IconWidth";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconWidth != srcCollectionCdef.IconWidth);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IconSprites";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconSprites != srcCollectionCdef.IconSprites);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "installedByCollectionGuid";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.installedByCollectionGuid, srcCollectionCdef.installedByCollectionGuid);
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "ccGuid";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.guid, srcCollectionCdef.guid);
							//
							// IsBaseContent
							//   if Dst IsBase, and Src is not, this change will be blocked following the changes anyway
							//   if Src IsBase, and Dst is not, Dst should be changed, and IsBaseContent can be treated like any other field
							//
							if (!okToUpdateDstFromSrc)
							{
								n = "IsBaseContent";
							}
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IsBaseContent != srcCollectionCdef.IsBaseContent);
							if (okToUpdateDstFromSrc)
							{
								okToUpdateDstFromSrc = okToUpdateDstFromSrc;
							}
							if (okToUpdateDstFromSrc)
							{
								if (dstCollectionCdef.IsBaseContent & !srcCollectionCdef.IsBaseContent)
								{
									//
									// Dst is a base CDef, Src is not. This update is not allowed. Log it and skip the Add
									//
									Copy = "An attempt was made to update a Base Content Definition [" + DstName + "] from a collection that is not the Base Collection. This is not allowed.";
									logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, " + Copy);
									throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, Copy, "dll", "builderClass", "UpgradeCDef_AddSrcToDst", 0, "", "", False, True, "")
									okToUpdateDstFromSrc = false;
								}
								else
								{
									//
									// Just log the change for tracking
									//
									logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, (Logging only) While merging two collections (probably application and an upgrade), one or more attributes for a content definition or field were different, first change was CDef=" + SrcContentName + ", field=" + n);
								}
							}
						}
					}
					if (okToUpdateDstFromSrc)
					{
						var tempVar2 = dstCollection.CDef(SrcContentName.ToLower());
						//
						// It okToUpdateDstFromSrc, update the Dst with the Src
						//
						tempVar2.Active = srcCollectionCdef.Active;
						tempVar2.ActiveOnly = srcCollectionCdef.ActiveOnly;
						tempVar2.AdminOnly = srcCollectionCdef.AdminOnly;
						//.adminColumns = srcCollectionCdef.adminColumns
						tempVar2.AliasID = srcCollectionCdef.AliasID;
						tempVar2.AliasName = srcCollectionCdef.AliasName;
						tempVar2.AllowAdd = srcCollectionCdef.AllowAdd;
						tempVar2.AllowCalendarEvents = srcCollectionCdef.AllowCalendarEvents;
						tempVar2.AllowContentChildTool = srcCollectionCdef.AllowContentChildTool;
						tempVar2.AllowContentTracking = srcCollectionCdef.AllowContentTracking;
						tempVar2.AllowDelete = srcCollectionCdef.AllowDelete;
						tempVar2.AllowTopicRules = srcCollectionCdef.AllowTopicRules;
						tempVar2.guid = srcCollectionCdef.guid;
						tempVar2.dataChanged = true;
						tempVar2.ContentControlCriteria = srcCollectionCdef.ContentControlCriteria;
						tempVar2.ContentDataSourceName = srcCollectionCdef.ContentDataSourceName;
						tempVar2.ContentTableName = srcCollectionCdef.ContentTableName;
						tempVar2.dataSourceId = srcCollectionCdef.dataSourceId;
						tempVar2.DefaultSortMethod = srcCollectionCdef.DefaultSortMethod;
						tempVar2.DeveloperOnly = srcCollectionCdef.DeveloperOnly;
						tempVar2.DropDownFieldList = srcCollectionCdef.DropDownFieldList;
						tempVar2.EditorGroupName = srcCollectionCdef.EditorGroupName;
						//.fields
						tempVar2.IconHeight = srcCollectionCdef.IconHeight;
						tempVar2.IconLink = srcCollectionCdef.IconLink;
						tempVar2.IconSprites = srcCollectionCdef.IconSprites;
						tempVar2.IconWidth = srcCollectionCdef.IconWidth;
						//.Id
						tempVar2.IgnoreContentControl = srcCollectionCdef.IgnoreContentControl;
						tempVar2.includesAFieldChange = true;
						tempVar2.installedByCollectionGuid = srcCollectionCdef.installedByCollectionGuid;
						tempVar2.IsBaseContent = srcCollectionCdef.IsBaseContent;
						tempVar2.IsModifiedSinceInstalled = srcCollectionCdef.IsModifiedSinceInstalled;
						tempVar2.Name = srcCollectionCdef.Name;
						tempVar2.parentID = srcCollectionCdef.parentID;
						tempVar2.parentName = srcCollectionCdef.parentName;
						tempVar2.SelectCommaList = srcCollectionCdef.SelectCommaList;
						//.selectList
						//.TimeStamp
						tempVar2.WhereClause = srcCollectionCdef.WhereClause;
					}
					//
					// Now check each of the field records for an addition, or a change
					// DstPtr is still set to the Dst CDef
					//
					//Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef(SrcPtr).fields.count=" & CollectionSrc.CDef(SrcPtr).fields.count)
					foreach (var srcFieldKeyValuePair in srcCollectionCdef.fields)
					{
						srcCollectionCdefField = srcFieldKeyValuePair.Value;
						SrcFieldName = srcCollectionCdefField.nameLc;
						okToUpdateDstFromSrc = false;
						if (!dstCollection.CDef.ContainsKey(SrcContentName.ToLower()))
						{
							//
							// should have been the collection
							//
							throw (new ApplicationException("ERROR - cannot update destination content because it was not found after being added."));
						}
						else
						{
							dstCollectionCdef = dstCollection.CDef(SrcContentName.ToLower());
							if (dstCollectionCdef.fields.ContainsKey(SrcFieldName.ToLower()))
							{
								//
								// Src field was found in Dst fields
								//

								dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower());
								okToUpdateDstFromSrc = false;
								if (dstCollectionCdefField.isBaseField == srcCollectionCdefField.isBaseField)
								{
									n = "Active";
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.active != dstCollectionCdefField.active);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "AdminOnly";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.adminOnly != dstCollectionCdefField.adminOnly);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "Authorable";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.authorable != dstCollectionCdefField.authorable);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "Caption";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.caption, dstCollectionCdefField.caption);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ContentID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.contentId != dstCollectionCdefField.contentId);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "DeveloperOnly";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.developerOnly != dstCollectionCdefField.developerOnly);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "EditSortPriority";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.editSortPriority != dstCollectionCdefField.editSortPriority);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "EditTab";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.editTabName, dstCollectionCdefField.editTabName);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "FieldType";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.fieldTypeId != dstCollectionCdefField.fieldTypeId);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "HTMLContent";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.htmlContent != dstCollectionCdefField.htmlContent);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "IndexColumn";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.indexColumn != dstCollectionCdefField.indexColumn);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "IndexSortDirection";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.indexSortDirection != dstCollectionCdefField.indexSortDirection);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "IndexSortOrder";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (EncodeInteger(srcCollectionCdefField.indexSortOrder) != genericController.EncodeInteger(dstCollectionCdefField.indexSortOrder));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "IndexWidth";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.indexWidth, dstCollectionCdefField.indexWidth);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "LookupContentID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.lookupContentID != dstCollectionCdefField.lookupContentID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "LookupList";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.lookupList, dstCollectionCdefField.lookupList);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyContentID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.manyToManyContentID != dstCollectionCdefField.manyToManyContentID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyRuleContentID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.manyToManyRuleContentID != dstCollectionCdefField.manyToManyRuleContentID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyRulePrimaryField";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.ManyToManyRulePrimaryField, dstCollectionCdefField.ManyToManyRulePrimaryField);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyRuleSecondaryField";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.ManyToManyRuleSecondaryField, dstCollectionCdefField.ManyToManyRuleSecondaryField);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "MemberSelectGroupID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.MemberSelectGroupID != dstCollectionCdefField.MemberSelectGroupID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "NotEditable";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.NotEditable != dstCollectionCdefField.NotEditable);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "Password";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Password != dstCollectionCdefField.Password);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ReadOnly";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.ReadOnly != dstCollectionCdefField.ReadOnly);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RedirectContentID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RedirectContentID != dstCollectionCdefField.RedirectContentID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RedirectID";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.RedirectID, dstCollectionCdefField.RedirectID);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RedirectPath";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.RedirectPath, dstCollectionCdefField.RedirectPath);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "Required";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Required != dstCollectionCdefField.Required);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RSSDescriptionField";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RSSDescriptionField != dstCollectionCdefField.RSSDescriptionField);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RSSTitleField";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RSSTitleField != dstCollectionCdefField.RSSTitleField);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "Scramble";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Scramble != dstCollectionCdefField.Scramble);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "TextBuffered";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.TextBuffered != dstCollectionCdefField.TextBuffered);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "DefaultValue";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (genericController.encodeText(srcCollectionCdefField.defaultValue) != genericController.encodeText(dstCollectionCdefField.defaultValue));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "UniqueName";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.UniqueName != dstCollectionCdefField.UniqueName);
									if (okToUpdateDstFromSrc)
									{
										okToUpdateDstFromSrc = okToUpdateDstFromSrc;
									}
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "IsBaseField";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.isBaseField != dstCollectionCdefField.isBaseField);
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "LookupContentName";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_lookupContentName(cpCore), dstCollectionCdefField.get_lookupContentName(cpCore));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyContentName";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_lookupContentName(cpCore), dstCollectionCdefField.get_lookupContentName(cpCore));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "ManyToManyRuleContentName";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_ManyToManyRuleContentName(cpCore), dstCollectionCdefField.get_ManyToManyRuleContentName(cpCore));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "RedirectContentName";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_RedirectContentName(cpCore), dstCollectionCdefField.get_RedirectContentName(cpCore));
									//
									if (!okToUpdateDstFromSrc)
									{
										n = "installedByCollectionid";
									}
									okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.installedByCollectionGuid, dstCollectionCdefField.installedByCollectionGuid);
									//
									if (okToUpdateDstFromSrc)
									{
										//okToUpdateDstFromSrc = okToUpdateDstFromSrc;
									}
								}
								//
								// Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
								//
								HelpSrc = srcCollectionCdefField.HelpCustom;
								HelpCustomChanged = !TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpCustom);
								//
								HelpSrc = srcCollectionCdefField.HelpDefault;
								HelpDefaultChanged = !TextMatch(cpCore, HelpSrc, srcCollectionCdefField.HelpDefault);
								//
								HelpChanged = HelpDefaultChanged || HelpCustomChanged;
							}
							else
							{
								//
								// field was not found in dst, add it and populate
								//
								dstCollectionCdef.fields.Add(SrcFieldName.ToLower(), new Models.Complex.CDefFieldModel());
								dstCollectionCdefField = dstCollectionCdef.fields(SrcFieldName.ToLower());
								okToUpdateDstFromSrc = true;
								HelpChanged = true;
							}
							//
							// If okToUpdateDstFromSrc, update the Dst record with the Src record
							//
							if (okToUpdateDstFromSrc)
							{
								//
								// Update Fields
								//
								dstCollectionCdefField.active = srcCollectionCdefField.active;
								dstCollectionCdefField.adminOnly = srcCollectionCdefField.adminOnly;
								dstCollectionCdefField.authorable = srcCollectionCdefField.authorable;
								dstCollectionCdefField.caption = srcCollectionCdefField.caption;
								dstCollectionCdefField.contentId = srcCollectionCdefField.contentId;
								dstCollectionCdefField.defaultValue = srcCollectionCdefField.defaultValue;
								dstCollectionCdefField.developerOnly = srcCollectionCdefField.developerOnly;
								dstCollectionCdefField.editSortPriority = srcCollectionCdefField.editSortPriority;
								dstCollectionCdefField.editTabName = srcCollectionCdefField.editTabName;
								dstCollectionCdefField.fieldTypeId = srcCollectionCdefField.fieldTypeId;
								dstCollectionCdefField.htmlContent = srcCollectionCdefField.htmlContent;
								dstCollectionCdefField.indexColumn = srcCollectionCdefField.indexColumn;
								dstCollectionCdefField.indexSortDirection = srcCollectionCdefField.indexSortDirection;
								dstCollectionCdefField.indexSortOrder = srcCollectionCdefField.indexSortOrder;
								dstCollectionCdefField.indexWidth = srcCollectionCdefField.indexWidth;
								dstCollectionCdefField.lookupContentID = srcCollectionCdefField.lookupContentID;
								dstCollectionCdefField.lookupList = srcCollectionCdefField.lookupList;
								dstCollectionCdefField.manyToManyContentID = srcCollectionCdefField.manyToManyContentID;
								dstCollectionCdefField.manyToManyRuleContentID = srcCollectionCdefField.manyToManyRuleContentID;
								dstCollectionCdefField.ManyToManyRulePrimaryField = srcCollectionCdefField.ManyToManyRulePrimaryField;
								dstCollectionCdefField.ManyToManyRuleSecondaryField = srcCollectionCdefField.ManyToManyRuleSecondaryField;
								dstCollectionCdefField.MemberSelectGroupID = srcCollectionCdefField.MemberSelectGroupID;
								dstCollectionCdefField.nameLc = srcCollectionCdefField.nameLc;
								dstCollectionCdefField.NotEditable = srcCollectionCdefField.NotEditable;
								dstCollectionCdefField.Password = srcCollectionCdefField.Password;
								dstCollectionCdefField.ReadOnly = srcCollectionCdefField.ReadOnly;
								dstCollectionCdefField.RedirectContentID = srcCollectionCdefField.RedirectContentID;
								dstCollectionCdefField.RedirectID = srcCollectionCdefField.RedirectID;
								dstCollectionCdefField.RedirectPath = srcCollectionCdefField.RedirectPath;
								dstCollectionCdefField.Required = srcCollectionCdefField.Required;
								dstCollectionCdefField.RSSDescriptionField = srcCollectionCdefField.RSSDescriptionField;
								dstCollectionCdefField.RSSTitleField = srcCollectionCdefField.RSSTitleField;
								dstCollectionCdefField.Scramble = srcCollectionCdefField.Scramble;
								dstCollectionCdefField.TextBuffered = srcCollectionCdefField.TextBuffered;
								dstCollectionCdefField.UniqueName = srcCollectionCdefField.UniqueName;
								dstCollectionCdefField.isBaseField = srcCollectionCdefField.isBaseField;
								dstCollectionCdefField.set_lookupContentName(cpCore, srcCollectionCdefField.get_lookupContentName(cpCore));
								dstCollectionCdefField.set_ManyToManyContentName(cpCore, srcCollectionCdefField.get_ManyToManyContentName(cpCore));
								dstCollectionCdefField.set_ManyToManyRuleContentName(cpCore, srcCollectionCdefField.get_ManyToManyRuleContentName(cpCore));
								dstCollectionCdefField.set_RedirectContentName(cpCore, srcCollectionCdefField.get_RedirectContentName(cpCore));
								dstCollectionCdefField.installedByCollectionGuid = srcCollectionCdefField.installedByCollectionGuid;
								dstCollectionCdefField.dataChanged = true;
								if (HelpChanged)
								{
									dstCollectionCdefField.HelpCustom = srcCollectionCdefField.HelpCustom;
									dstCollectionCdefField.HelpDefault = srcCollectionCdefField.HelpDefault;
									dstCollectionCdefField.HelpChanged = true;
								}
								dstCollectionCdef.includesAFieldChange = true;
							}
							//
						}
					}
				}
				//
				// -------------------------------------------------------------------------------------------------
				// Check SQL Indexes
				// -------------------------------------------------------------------------------------------------
				//
				int dstSqlIndexPtr = 0;
				int SrcsSqlIndexPtr = 0;
				for (SrcsSqlIndexPtr = 0; SrcsSqlIndexPtr < srcCollection.SQLIndexCnt; SrcsSqlIndexPtr++)
				{
					SrcContentName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName + "-" + srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName + "-" + srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName;
					okToUpdateDstFromSrc = false;
					//
					// Search for this name in the Dst
					//
					for (dstSqlIndexPtr = 0; dstSqlIndexPtr < dstCollection.SQLIndexCnt; dstSqlIndexPtr++)
					{
						DstName = dstCollection.SQLIndexes(dstSqlIndexPtr).DataSourceName + "-" + dstCollection.SQLIndexes(dstSqlIndexPtr).TableName + "-" + dstCollection.SQLIndexes(dstSqlIndexPtr).IndexName;
						if (TextMatch(cpCore, DstName, SrcContentName))
						{
							//
							// found a match between Src and Dst
							//
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollection.SQLIndexes(dstSqlIndexPtr).FieldNameList, srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList);
							break;
						}
					}
					if (dstSqlIndexPtr == dstCollection.SQLIndexCnt)
					{
						//
						// CDef was not found, add it
						//
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
						ReDim Preserve dstCollection.SQLIndexes(dstCollection.SQLIndexCnt);
						dstCollection.SQLIndexCnt = dstSqlIndexPtr + 1;
						okToUpdateDstFromSrc = true;
					}
					if (okToUpdateDstFromSrc)
					{
						var tempVar3 = dstCollection.SQLIndexes(dstSqlIndexPtr);
						//
						// It okToUpdateDstFromSrc, update the Dst with the Src
						//
						tempVar3.dataChanged = true;
						tempVar3.DataSourceName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).DataSourceName;
						tempVar3.FieldNameList = srcCollection.SQLIndexes(SrcsSqlIndexPtr).FieldNameList;
						tempVar3.IndexName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).IndexName;
						tempVar3.TableName = srcCollection.SQLIndexes(SrcsSqlIndexPtr).TableName;
					}
				}
				//
				//-------------------------------------------------------------------------------------------------
				// Check menus
				//-------------------------------------------------------------------------------------------------
				//
				int DstMenuPtr = 0;
				string SrcNameSpace = null;
				string SrcParentName = null;
				DataBuildVersion = cpCore.siteProperties.dataBuildVersion;
				for (var SrcMenuPtr = 0; SrcMenuPtr < srcCollection.MenuCnt; SrcMenuPtr++)
				{
					DstMenuPtr = 0;
					SrcContentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Name);
					SrcParentName = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).ParentName);
					SrcNameSpace = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).menuNameSpace);
					SrcIsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator;
					if (SrcIsNavigator)
					{
						if (SrcContentName == "manage add-ons")
						{
							SrcContentName = SrcContentName;
						}
					}
					okToUpdateDstFromSrc = false;
					//
					SrcKey = genericController.vbLCase(srcCollection.Menus(SrcMenuPtr).Key);
					//
					// Search for match using guid
					//
					IsMatch = false;
					for (DstMenuPtr = 0; DstMenuPtr < dstCollection.MenuCnt; DstMenuPtr++)
					{
						DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name);
						if (DstName == SrcContentName)
						{
							DstName = DstName;
							DstIsNavigator = dstCollection.Menus(DstMenuPtr).IsNavigator;
							DstKey = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Key);
							if (genericController.vbLCase(DstName) == "settings")
							{
								DstName = DstName;
							}
							IsMatch = (DstKey == SrcKey) && (SrcIsNavigator == DstIsNavigator);
							if (IsMatch)
							{
								break;
							}
						}
					}
					if (!IsMatch)
					{
						//
						// no match found on guid, try name and ( either namespace or parentname )
						//
						for (DstMenuPtr = 0; DstMenuPtr < dstCollection.MenuCnt; DstMenuPtr++)
						{
							DstName = genericController.vbLCase(dstCollection.Menus(DstMenuPtr).Name);
							if (genericController.vbLCase(DstName) == "settings")
							{
								DstName = DstName;
							}
							if ((SrcContentName == DstName) && (SrcIsNavigator == DstIsNavigator))
							{
								if (SrcIsNavigator)
								{
									//
									// Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
									//
									IsMatch = (SrcNameSpace == genericController.vbLCase(dstCollection.Menus(DstMenuPtr).menuNameSpace)) && (dstCollection.Menus(DstMenuPtr).Guid == "");
								}
								else
								{
									//
									// AdminMenu - check parentname
									//
									IsMatch = (SrcParentName == genericController.vbLCase(dstCollection.Menus(DstMenuPtr).ParentName));
								}
								if (IsMatch)
								{
									break;
								}
							}
						}
					}
					if (!IsMatch)
					{
						//If DstPtr = CollectionDst.MenuCnt Then
						//
						// menu was not found, add it
						//
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
						ReDim Preserve dstCollection.Menus(dstCollection.MenuCnt);
						dstCollection.MenuCnt = dstCollection.MenuCnt + 1;
						okToUpdateDstFromSrc = true;
						//End If
					}
					else
					{
						//If IsMatch Then
						//
						// found a match between Src and Dst
						//
						if (SrcIsUserCDef || SrcIsNavigator)
						{
							//
							// Special case -- Navigators update from all upgrade sources so Base migrates changes
							// test for cdef attribute changes
							//
							var tempVar4 = dstCollection.Menus(DstMenuPtr);
							//With dstCollection.Menus(dstCollection.MenuCnt)
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (tempVar4.Active != srcCollection.Menus(SrcMenuPtr).Active);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (tempVar4.AdminOnly != srcCollection.Menus(SrcMenuPtr).AdminOnly);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.ContentName, srcCollection.Menus(SrcMenuPtr).ContentName);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (tempVar4.DeveloperOnly != srcCollection.Menus(SrcMenuPtr).DeveloperOnly);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.LinkPage, srcCollection.Menus(SrcMenuPtr).LinkPage);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.Name, srcCollection.Menus(SrcMenuPtr).Name);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || (tempVar4.NewWindow != srcCollection.Menus(SrcMenuPtr).NewWindow);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.SortOrder, srcCollection.Menus(SrcMenuPtr).SortOrder);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.AddonName, srcCollection.Menus(SrcMenuPtr).AddonName);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.NavIconType, srcCollection.Menus(SrcMenuPtr).NavIconType);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.NavIconTitle, srcCollection.Menus(SrcMenuPtr).NavIconTitle);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.menuNameSpace, srcCollection.Menus(SrcMenuPtr).menuNameSpace);
							okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, tempVar4.Guid, srcCollection.Menus(SrcMenuPtr).Guid);
							//okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ParentName, CollectionSrc.Menus(SrcPtr).ParentName)
						}
						//Exit For
					}
					if (okToUpdateDstFromSrc)
					{
						var tempVar5 = dstCollection.Menus(DstMenuPtr);
						//
						// It okToUpdateDstFromSrc, update the Dst with the Src
						//
						tempVar5.dataChanged = true;
						tempVar5.Guid = srcCollection.Menus(SrcMenuPtr).Guid;
						tempVar5.Name = srcCollection.Menus(SrcMenuPtr).Name;
						tempVar5.IsNavigator = srcCollection.Menus(SrcMenuPtr).IsNavigator;
						tempVar5.Active = srcCollection.Menus(SrcMenuPtr).Active;
						tempVar5.AdminOnly = srcCollection.Menus(SrcMenuPtr).AdminOnly;
						tempVar5.ContentName = srcCollection.Menus(SrcMenuPtr).ContentName;
						tempVar5.DeveloperOnly = srcCollection.Menus(SrcMenuPtr).DeveloperOnly;
						tempVar5.LinkPage = srcCollection.Menus(SrcMenuPtr).LinkPage;
						tempVar5.NewWindow = srcCollection.Menus(SrcMenuPtr).NewWindow;
						tempVar5.ParentName = srcCollection.Menus(SrcMenuPtr).ParentName;
						tempVar5.menuNameSpace = srcCollection.Menus(SrcMenuPtr).menuNameSpace;
						tempVar5.SortOrder = srcCollection.Menus(SrcMenuPtr).SortOrder;
						tempVar5.AddonName = srcCollection.Menus(SrcMenuPtr).AddonName;
						tempVar5.NavIconType = srcCollection.Menus(SrcMenuPtr).NavIconType;
						tempVar5.NavIconTitle = srcCollection.Menus(SrcMenuPtr).NavIconTitle;
					}
				}
				//'
				//'-------------------------------------------------------------------------------------------------
				//' Check addons -- yes, this should be done.
				//'-------------------------------------------------------------------------------------------------
				//'
				//If False Then
				//    '
				//    ' remove this for now -- later add ImportCollections to track the collections (not addons)
				//    '
				//    '
				//    '
				//    For SrcPtr = 0 To srcCollection.AddOnCnt - 1
				//        SrcContentName = genericController.vbLCase(srcCollection.AddOns(SrcPtr).Name)
				//        okToUpdateDstFromSrc = False
				//        '
				//        ' Search for this name in the Dst
				//        '
				//        For DstPtr = 0 To dstCollection.AddOnCnt - 1
				//            DstName = genericController.vbLCase(dstCollection.AddOns(DstPtr).Name)
				//            If DstName = SrcContentName Then
				//                '
				//                ' found a match between Src and Dst
				//                '
				//                If SrcIsUserCDef Then
				//                    '
				//                    ' test for cdef attribute changes
				//                    '
				//                    With dstCollection.AddOns(DstPtr)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ArgumentList, srcCollection.AddOns(SrcPtr).ArgumentList)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Copy, srcCollection.AddOns(SrcPtr).Copy)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Link, srcCollection.AddOns(SrcPtr).Link)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Name, srcCollection.AddOns(SrcPtr).Name)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ObjectProgramID, srcCollection.AddOns(SrcPtr).ObjectProgramID)
				//                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.SortOrder, srcCollection.AddOns(SrcPtr).SortOrder)
				//                    End With
				//                End If
				//                Exit For
				//            End If
				//        Next
				//        If DstPtr = dstCollection.AddOnCnt Then
				//            '
				//            ' CDef was not found, add it
				//            '
				//            ReDim Preserve dstCollection.AddOns(dstCollection.AddOnCnt)
				//            dstCollection.AddOnCnt = DstPtr + 1
				//            okToUpdateDstFromSrc = True
				//        End If
				//        If okToUpdateDstFromSrc Then
				//            With dstCollection.AddOns(DstPtr)
				//                '
				//                ' It okToUpdateDstFromSrc, update the Dst with the Src
				//                '
				//                .CDefChanged = True
				//                .ArgumentList = srcCollection.AddOns(SrcPtr).ArgumentList
				//                .Copy = srcCollection.AddOns(SrcPtr).Copy
				//                .Link = srcCollection.AddOns(SrcPtr).Link
				//                .Name = srcCollection.AddOns(SrcPtr).Name
				//                .ObjectProgramID = srcCollection.AddOns(SrcPtr).ObjectProgramID
				//                .SortOrder = srcCollection.AddOns(SrcPtr).SortOrder
				//            End With
				//        End If
				//    Next
				//End If
				//
				//-------------------------------------------------------------------------------------------------
				// Check styles
				//-------------------------------------------------------------------------------------------------
				//
				int srcStylePtr = 0;
				int dstStylePtr = 0;
				for (srcStylePtr = 0; srcStylePtr < srcCollection.StyleCnt; srcStylePtr++)
				{
					SrcContentName = genericController.vbLCase(srcCollection.Styles(srcStylePtr).Name);
					okToUpdateDstFromSrc = false;
					//
					// Search for this name in the Dst
					//
					for (dstStylePtr = 0; dstStylePtr < dstCollection.StyleCnt; dstStylePtr++)
					{
						DstName = genericController.vbLCase(dstCollection.Styles(dstStylePtr).Name);
						if (DstName == SrcContentName)
						{
							//
							// found a match between Src and Dst
							//
							if (SrcIsUserCDef)
							{
								//
								// test for cdef attribute changes
								//
								okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollection.Styles(dstStylePtr).Copy, srcCollection.Styles(srcStylePtr).Copy);
							}
							break;
						}
					}
					if (dstStylePtr == dstCollection.StyleCnt)
					{
						//
						// CDef was not found, add it
						//
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
						ReDim Preserve dstCollection.Styles(dstCollection.StyleCnt);
						dstCollection.StyleCnt = dstStylePtr + 1;
						okToUpdateDstFromSrc = true;
					}
					if (okToUpdateDstFromSrc)
					{
						var tempVar6 = dstCollection.Styles(dstStylePtr);
						//
						// It okToUpdateDstFromSrc, update the Dst with the Src
						//
						tempVar6.dataChanged = true;
						tempVar6.Copy = srcCollection.Styles(srcStylePtr).Copy;
						tempVar6.Name = srcCollection.Styles(srcStylePtr).Name;
					}
				}
				//
				//-------------------------------------------------------------------------------------------------
				// Add Collections
				//-------------------------------------------------------------------------------------------------
				//
				int dstPtr = 0;
				for (var SrcPtr = 0; SrcPtr < srcCollection.ImportCnt; SrcPtr++)
				{
					dstPtr = dstCollection.ImportCnt;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
					ReDim Preserve dstCollection.collectionImports(dstPtr);
					dstCollection.collectionImports(dstPtr) = srcCollection.collectionImports(SrcPtr);
					dstCollection.ImportCnt = dstPtr + 1;
				}
				//
				//-------------------------------------------------------------------------------------------------
				// Page Templates
				//-------------------------------------------------------------------------------------------------
				//
				//
				//-------------------------------------------------------------------------------------------------
				// Site Sections
				//-------------------------------------------------------------------------------------------------
				//
				//
				//-------------------------------------------------------------------------------------------------
				// Dynamic Menus
				//-------------------------------------------------------------------------------------------------
				//
				//
				//-------------------------------------------------------------------------------------------------
				// Page Content
				//-------------------------------------------------------------------------------------------------
				//
				//
				//-------------------------------------------------------------------------------------------------
				// Copy Content
				//-------------------------------------------------------------------------------------------------
				//
				//
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnOk;
		}
		//'
		//'===========================================================================
		//'   Error handler
		//'===========================================================================
		//'
		//Private Sub HandleClassTrapError(ByVal ApplicationName As String, ByVal ErrNumber As Integer, ByVal ErrSource As String, ByVal ErrDescription As String, ByVal MethodName As String, ByVal ErrorTrap As Boolean, Optional ByVal ResumeNext As Boolean = False)
		//    '
		//    'Call App.LogEvent("addonInstallClass.HandleClassTrapError called from " & MethodName)
		//    '
		//   throw (New ApplicationException("Unexpected exception"))'cpCore.handleLegacyError3(ApplicationName, "unknown", "dll", "AddonInstallClass", MethodName, ErrNumber, ErrSource, ErrDescription, ErrorTrap, ResumeNext, "")
		//    '
		//End Sub
		//
		//
		//
		private static miniCollectionModel installCollection_GetApplicationMiniCollection(coreClass cpCore, bool isNewBuild)
		{
			miniCollectionModel returnColl = new miniCollectionModel();
			try
			{
				//
				string ExportFilename = null;
				string ExportPathPage = null;
				string CollectionData = null;
				//
				if (!isNewBuild)
				{
					//
					// if this is not an empty database, get the application collection, else return empty
					//
					ExportFilename = "cdef_export_" + Convert.ToString(genericController.GetRandomInteger()) + ".xml";
					ExportPathPage = "tmp\\" + ExportFilename;
					exportApplicationCDefXml(cpCore, ExportPathPage, true);
					CollectionData = cpCore.privateFiles.readFile(ExportPathPage);
					cpCore.privateFiles.deleteFile(ExportPathPage);
					returnColl = installCollection_LoadXmlToMiniCollection(cpCore, CollectionData, false, false, isNewBuild, new miniCollectionModel());
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnColl;
		}
		//
		//========================================================================
		// ----- Get an XML nodes attribute based on its name
		//========================================================================
		//
		public static string GetXMLAttribute(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound)
		{
			string returnAttr = "";
			try
			{
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
							returnAttr = NodeAttribute.Value;
							Found = true;
							break;
						}
					}
					if (!Found)
					{
						returnAttr = DefaultIfNotFound;
					}
				}
				else
				{
					returnAttr = ResultNode.Value;
					Found = true;
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnAttr;
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private static double GetXMLAttributeNumber(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound)
		{
			return EncodeNumber(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound));
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private static bool GetXMLAttributeBoolean(coreClass cpCore, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound)
		{
			return genericController.EncodeBoolean(GetXMLAttribute(cpCore, Found, Node, Name, Convert.ToString(DefaultIfNotFound)));
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		private static int GetXMLAttributeInteger(coreClass cpCore, bool Found, XmlNode Node, string Name, int DefaultIfNotFound)
		{
			return genericController.EncodeInteger(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound.ToString()));
		}
		//
		//==================================================================================================================
		//
		//==================================================================================================================
		//
		private static bool TextMatch(coreClass cpCore, string Source1, string Source2)
		{
			return (Source1.ToLower() == genericController.vbLCase(Source2));
		}
		//
		//
		//
		private static string GetMenuNameSpace(coreClass cpCore, miniCollectionModel Collection, int MenuPtr, bool IsNavigator, string UsedIDList)
		{
			string returnAttr = "";
			try
			{
				string ParentName = null;
				int Ptr = 0;
				string Prefix = null;
				string LCaseParentName = null;

				//
				ParentName = Collection.Menus(MenuPtr).ParentName;
				if (!string.IsNullOrEmpty(ParentName))
				{
					LCaseParentName = genericController.vbLCase(ParentName);
					for (Ptr = 0; Ptr < Collection.MenuCnt; Ptr++)
					{
						if (genericController.vbInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0)
						{
							if (LCaseParentName == genericController.vbLCase(Collection.Menus(Ptr).Name) && (IsNavigator == Collection.Menus(Ptr).IsNavigator))
							{
								Prefix = GetMenuNameSpace(cpCore, Collection, Ptr, IsNavigator, UsedIDList + "," + MenuPtr);
								if (string.IsNullOrEmpty(Prefix))
								{
									returnAttr = ParentName;
								}
								else
								{
									returnAttr = Prefix + "." + ParentName;
								}
								break;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnAttr;
		}
		//
		//=============================================================================
		//   Create an entry in the Sort Methods Table
		//=============================================================================
		//
		private static void VerifySortMethod(coreClass cpCore, string Name, string OrderByCriteria)
		{
			try
			{
				//
				DataTable dt = null;
				sqlFieldListClass sqlList = new sqlFieldListClass();
				//
				sqlList.add("name", cpCore.db.encodeSQLText(Name));
				sqlList.add("CreatedBy", "0");
				sqlList.add("OrderByClause", cpCore.db.encodeSQLText(OrderByCriteria));
				sqlList.add("active", SQLTrue);
				sqlList.add("ContentControlID", Models.Complex.cdefModel.getContentId(cpCore, "Sort Methods").ToString());
				//
				dt = cpCore.db.openTable("Default", "ccSortMethods", "Name=" + cpCore.db.encodeSQLText(Name), "ID", "ID", 1);
				if (dt.Rows.Count > 0)
				{
					//
					// update sort method
					//
					cpCore.db.updateTableRecord("Default", "ccSortMethods", "ID=" + genericController.EncodeInteger(dt.Rows(0).Item("ID")).ToString(), sqlList);
				}
				else
				{
					//
					// Create the new sort method
					//
					cpCore.db.insertTableRecord("Default", "ccSortMethods", sqlList);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=========================================================================================
		//
		//=========================================================================================
		//
		public static void VerifySortMethods(coreClass cpCore)
		{
			try
			{
				//
				logController.appendInstallLog(cpCore, "Verify Sort Records");
				//
				VerifySortMethod(cpCore, "By Name", "Name");
				VerifySortMethod(cpCore, "By Alpha Sort Order Field", "SortOrder");
				VerifySortMethod(cpCore, "By Date", "DateAdded");
				VerifySortMethod(cpCore, "By Date Reverse", "DateAdded Desc");
				VerifySortMethod(cpCore, "By Alpha Sort Order Then Oldest First", "SortOrder,ID");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=============================================================================
		//   Get a ContentID from the ContentName using just the tables
		//=============================================================================
		//
		private static void VerifyContentFieldTypes(coreClass cpCore)
		{
			try
			{
				//
				int RowsFound = 0;
				int CID = 0;
				bool TableBad = false;
				int RowsNeeded = 0;
				//
				// ----- make sure there are enough records
				//
				TableBad = false;
				RowsFound = 0;
				using (DataTable rs = cpCore.db.executeQuery("Select ID from ccFieldTypes order by id"))
				{
					if (!isDataTableOk(rs))
					{
						//
						// problem
						//
						TableBad = true;
					}
					else
					{
						//
						// Verify the records that are there
						//
						RowsFound = 0;
						foreach (DataRow dr in rs.Rows)
						{
							RowsFound = RowsFound + 1;
							if (RowsFound != genericController.EncodeInteger(dr.Item("ID")))
							{
								//
								// Bad Table
								//
								TableBad = true;
								break;
							}
						}
					}

				}
				//
				// ----- Replace table if needed
				//
				if (TableBad)
				{
					cpCore.db.deleteTable("Default", "ccFieldTypes");
					cpCore.db.createSQLTable("Default", "ccFieldTypes");
					RowsFound = 0;
				}
				//
				// ----- Add the number of rows needed
				//
				RowsNeeded = FieldTypeIdMax - RowsFound;
				if (RowsNeeded > 0)
				{
					CID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Types");
					if (CID <= 0)
					{
						//
						// Problem
						//
						cpCore.handleException(new ApplicationException("Content Field Types content definition was not found"));
					}
					else
					{
						while (RowsNeeded > 0)
						{
							cpCore.db.executeQuery("Insert into ccFieldTypes (active,contentcontrolid)values(1," + CID + ")");
							RowsNeeded = RowsNeeded - 1;
						}
					}
				}
				//
				// ----- Update the Names of each row
				//
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Integer' where ID=1;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Text' where ID=2;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='LongText' where ID=3;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Boolean' where ID=4;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Date' where ID=5;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='File' where ID=6;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Lookup' where ID=7;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Redirect' where ID=8;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Currency' where ID=9;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='TextFile' where ID=10;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Image' where ID=11;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Float' where ID=12;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='AutoIncrement' where ID=13;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='ManyToMany' where ID=14;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Member Select' where ID=15;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='CSS File' where ID=16;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='XML File' where ID=17;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Javascript File' where ID=18;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Link' where ID=19;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='Resource Link' where ID=20;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML' where ID=21;");
				cpCore.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML File' where ID=22;");
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=============================================================================
		//
		//=============================================================================
		//
		public void csv_VerifyAggregateScript(coreClass cpCore, string Name, string Link, string ArgumentList, string SortOrder)
		{
			try
			{
				//
				int CSEntry = 0;
				string ContentName = null;
				string MethodName;
				//
				MethodName = "csv_VerifyAggregateScript";
				//
				ContentName = "Aggregate Function Scripts";
				CSEntry = cpCore.db.csOpen(ContentName, "(name=" + cpCore.db.encodeSQLText(Name) + ")",, false,,,, "Name,Link,ObjectProgramID,ArgumentList,SortOrder");
				//
				// If no current entry, create one
				//
				if (!cpCore.db.csOk(CSEntry))
				{
					cpCore.db.csClose(CSEntry);
					CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID);
					if (cpCore.db.csOk(CSEntry))
					{
						cpCore.db.csSet(CSEntry, "name", Name);
					}
				}
				if (cpCore.db.csOk(CSEntry))
				{
					cpCore.db.csSet(CSEntry, "Link", Link);
					cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
					cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
				}
				cpCore.db.csClose(CSEntry);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=============================================================================
		//
		//=============================================================================
		//
		public void csv_VerifyAggregateReplacement(coreClass cpcore, string Name, string Copy, string SortOrder)
		{
			csv_VerifyAggregateReplacement2(cpcore, Name, Copy, "", SortOrder);
		}
		//
		//=============================================================================
		//
		//=============================================================================
		//
		public static void csv_VerifyAggregateReplacement2(coreClass cpCore, string Name, string Copy, string ArgumentList, string SortOrder)
		{
			try
			{
				//
				int CSEntry = 0;
				string ContentName = null;
				string MethodName;
				//
				MethodName = "csv_VerifyAggregateReplacement2";
				//
				ContentName = "Aggregate Function Replacements";
				CSEntry = cpCore.db.csOpen(ContentName, "(name=" + cpCore.db.encodeSQLText(Name) + ")",, false,,,, "Name,Copy,SortOrder,ArgumentList");
				//
				// If no current entry, create one
				//
				if (!cpCore.db.csOk(CSEntry))
				{
					cpCore.db.csClose(CSEntry);
					CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID);
					if (cpCore.db.csOk(CSEntry))
					{
						cpCore.db.csSet(CSEntry, "name", Name);
					}
				}
				if (cpCore.db.csOk(CSEntry))
				{
					cpCore.db.csSet(CSEntry, "Copy", Copy);
					cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
					cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
				}
				cpCore.db.csClose(CSEntry);
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//=============================================================================
		//
		//=============================================================================
		//
		public void csv_VerifyAggregateObject(coreClass cpcore, string Name, string ObjectProgramID, string ArgumentList, string SortOrder)
		{
			try
			{
				//
				int CSEntry = 0;
				string ContentName = null;
				string MethodName;
				//
				MethodName = "csv_VerifyAggregateObject";
				//
				// Locate current entry
				//
				ContentName = "Aggregate Function Objects";
				CSEntry = cpcore.db.csOpen(ContentName, "(name=" + cpcore.db.encodeSQLText(Name) + ")",, false,,,, "Name,Link,ObjectProgramID,ArgumentList,SortOrder");
				//
				// If no current entry, create one
				//
				if (!cpcore.db.csOk(CSEntry))
				{
					cpcore.db.csClose(CSEntry);
					CSEntry = cpcore.db.csInsertRecord(ContentName, SystemMemberID);
					if (cpcore.db.csOk(CSEntry))
					{
						cpcore.db.csSet(CSEntry, "name", Name);
					}
				}
				if (cpcore.db.csOk(CSEntry))
				{
					cpcore.db.csSet(CSEntry, "ObjectProgramID", ObjectProgramID);
					cpcore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
					cpcore.db.csSet(CSEntry, "SortOrder", SortOrder);
				}
				cpcore.db.csClose(CSEntry);
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
		}
		//
		//========================================================================
		//
		//========================================================================
		//
		public static void exportApplicationCDefXml(coreClass cpCore, string privateFilesPathFilename, bool IncludeBaseFields)
		{
			try
			{
				xmlController XML = null;
				string Content = null;
				//
				XML = new xmlController(cpCore);
				Content = XML.GetXMLContentDefinition3("", IncludeBaseFields);
				cpCore.privateFiles.saveFile(privateFilesPathFilename, Content);
				XML = null;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
	}
}
