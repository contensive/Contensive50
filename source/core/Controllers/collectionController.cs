
using System;
using System.Reflection;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.IO;
using System.Data;
using System.Threading;
using Contensive.Core.Models.Complex;
//
//
namespace Contensive.Core {
    //
    //====================================================================================================
    /// <summary>
    /// install addon collections
    /// </summary>
    public class collectionController {
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
        private static bool installCollection_AddMiniCollectionSrcToDst(coreClass cpCore, ref miniCollectionModel dstCollection, miniCollectionModel srcCollection, bool SrcIsUserCDef) {
            bool returnOk = true;
            try {
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
                string srcName = null;
                string dstName = null;
                string SrcFieldName = null;
                bool okToUpdateDstFromSrc = false;
                Models.Complex.cdefModel srcCollectionCdef = null;
                bool DebugSrcFound = false;
                bool DebugDstFound = false;
                //
                // If the Src is the BaseCollection, the Dst must be the Application Collectio
                //   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                //
                if (srcCollection.isBaseCollection) {
                    foreach (var dstKeyValuePair in dstCollection.CDef) {
                        Models.Complex.cdefModel dstWorkingCdef = dstKeyValuePair.Value;
                        string contentName = dstWorkingCdef.Name;
                        if (dstCollection.CDef[contentName.ToLower()].IsBaseContent) {
                            //
                            // this application collection Cdef is marked base, verify it is in the base collection
                            //
                            if (!srcCollection.CDef.ContainsKey(contentName.ToLower())) {
                                //
                                // cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                //
                                var tempVar = dstCollection.CDef[contentName.ToLower()];
                                tempVar.IsBaseContent = false;
                                tempVar.dataChanged = true;
                                foreach (var dstFieldKeyValuePair in tempVar.fields) {
                                    Models.Complex.CDefFieldModel field = dstFieldKeyValuePair.Value;
                                    if (field.isBaseField) {
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
                //   if the cdef does not match, set cdefext[Ptr].CDefChanged true
                //   if any field does not match, set cdefext...field...CDefChanged
                //   if the is no CollectionDst for the CollectionSrc, add it and set okToUpdateDstFromSrc
                // -------------------------------------------------------------------------------------------------
                //
                logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name + ", UpgradeCDef_AddSrcToDst");
                //
                foreach (var srcKeyValuePair in srcCollection.CDef) {
                    srcCollectionCdef = srcKeyValuePair.Value;

                    srcName = srcCollectionCdef.Name;
                    //If genericController.vbLCase(SrcContentName) = "site sections" Then
                    //    SrcContentName = SrcContentName
                    //End If
                    DebugSrcFound = false;
                    if (srcName.IndexOf(cnNavigatorEntries)>=0 ) {
                        DebugSrcFound = true;
                    }
                    //
                    // Search for this cdef in the Dst
                    //
                    okToUpdateDstFromSrc = false;
                    if (!dstCollection.CDef.ContainsKey(srcName.ToLower())) {
                        //
                        // add src to dst
                        //
                        dstCollection.CDef.Add(srcName.ToLower(), new Models.Complex.cdefModel());
                        okToUpdateDstFromSrc = true;
                    } else {
                        dstCollectionCdef = dstCollection.CDef[srcName.ToLower()];
                        dstName = srcName;
                        //
                        // found a match between Src and Dst
                        //
                        if (dstCollectionCdef.IsBaseContent == srcCollectionCdef.IsBaseContent) {
                            //
                            // Allow changes to user cdef only from user cdef, changes to base only from base
                            //
                            n = "ActiveOnly";
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.ActiveOnly != srcCollectionCdef.ActiveOnly);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AdminOnly";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AdminOnly != srcCollectionCdef.AdminOnly);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "DeveloperOnly";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.DeveloperOnly != srcCollectionCdef.DeveloperOnly);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowAdd";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowAdd != srcCollectionCdef.AllowAdd);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowCalendarEvents";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowCalendarEvents != srcCollectionCdef.AllowCalendarEvents);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowContentTracking";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowContentTracking != srcCollectionCdef.AllowContentTracking);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowDelete";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowDelete != srcCollectionCdef.AllowDelete);
                            //
                            //If Not okToUpdateDstFromSrc Then n = "AllowMetaContent"
                            //okToUpdateDstFromSrc = okToUpdateDstFromSrc Or (.AllowMetaContent <> srcCollectionCdef.AllowMetaContent)
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowTopicRules";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowTopicRules != srcCollectionCdef.AllowTopicRules);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "ContentDataSourceName";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.ContentDataSourceName, srcCollectionCdef.ContentDataSourceName);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "ContentTableName";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.ContentTableName, srcCollectionCdef.ContentTableName);
                            //
                            if (DebugDstFound) {
                                //DebugDstFound = DebugDstFound;
                            }
                            if (!okToUpdateDstFromSrc) {
                                n = "DefaultSortMethod";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.DefaultSortMethod, srcCollectionCdef.DefaultSortMethod);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "DropDownFieldList";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.DropDownFieldList, srcCollectionCdef.DropDownFieldList);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "EditorGroupName";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.EditorGroupName, srcCollectionCdef.EditorGroupName);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IgnoreContentControl";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IgnoreContentControl != srcCollectionCdef.IgnoreContentControl);
                            if (okToUpdateDstFromSrc) {
                                //okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                            }
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "Active";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.Active != srcCollectionCdef.Active);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "AllowContentChildTool";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.AllowContentChildTool != srcCollectionCdef.AllowContentChildTool);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "ParentId";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.parentID != srcCollectionCdef.parentID);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IconLink";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.IconLink, srcCollectionCdef.IconLink);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IconHeight";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconHeight != srcCollectionCdef.IconHeight);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IconWidth";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconWidth != srcCollectionCdef.IconWidth);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IconSprites";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IconSprites != srcCollectionCdef.IconSprites);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "installedByCollectionGuid";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.installedByCollectionGuid, srcCollectionCdef.installedByCollectionGuid);
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "ccGuid";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollectionCdef.guid, srcCollectionCdef.guid);
                            //
                            // IsBaseContent
                            //   if Dst IsBase, and Src is not, this change will be blocked following the changes anyway
                            //   if Src IsBase, and Dst is not, Dst should be changed, and IsBaseContent can be treated like any other field
                            //
                            if (!okToUpdateDstFromSrc) {
                                n = "IsBaseContent";
                            }
                            okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstCollectionCdef.IsBaseContent != srcCollectionCdef.IsBaseContent);
                            if (okToUpdateDstFromSrc) {
                                //okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                            }
                            if (okToUpdateDstFromSrc) {
                                if (dstCollectionCdef.IsBaseContent & !srcCollectionCdef.IsBaseContent) {
                                    //
                                    // Dst is a base CDef, Src is not. This update is not allowed. Log it and skip the Add
                                    //
                                    Copy = "An attempt was made to update a Base Content Definition [" + dstName + "] from a collection that is not the Base Collection. This is not allowed.";
                                    logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, " + Copy);
                                    throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, Copy, "dll", "builderClass", "UpgradeCDef_AddSrcToDst", 0, "", "", False, True, "")
                                    okToUpdateDstFromSrc = false;
                                } else {
                                    //
                                    // Just log the change for tracking
                                    //
                                    logController.appendInstallLog(cpCore, "UpgradeCDef_AddSrcToDst, (Logging only) While merging two collections (probably application and an upgrade), one or more attributes for a content definition or field were different, first change was CDef=" + srcName + ", field=" + n);
                                }
                            }
                        }
                    }
                    if (okToUpdateDstFromSrc) {
                        var tempVar2 = dstCollection.CDef[srcName.ToLower()];
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
                    //Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef[SrcPtr].fields.count=" & CollectionSrc.CDef[SrcPtr].fields.count)
                    foreach (var srcFieldKeyValuePair in srcCollectionCdef.fields) {
                        srcCollectionCdefField = srcFieldKeyValuePair.Value;
                        SrcFieldName = srcCollectionCdefField.nameLc;
                        okToUpdateDstFromSrc = false;
                        if (!dstCollection.CDef.ContainsKey(srcName.ToLower())) {
                            //
                            // should have been the collection
                            //
                            throw (new ApplicationException("ERROR - cannot update destination content because it was not found after being added."));
                        } else {
                            dstCollectionCdef = dstCollection.CDef[srcName.ToLower()];
                            if (dstCollectionCdef.fields.ContainsKey(SrcFieldName.ToLower())) {
                                //
                                // Src field was found in Dst fields
                                //

                                dstCollectionCdefField = dstCollectionCdef.fields[SrcFieldName.ToLower()];
                                okToUpdateDstFromSrc = false;
                                if (dstCollectionCdefField.isBaseField == srcCollectionCdefField.isBaseField) {
                                    n = "Active";
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.active != dstCollectionCdefField.active);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "AdminOnly";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.adminOnly != dstCollectionCdefField.adminOnly);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Authorable";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.authorable != dstCollectionCdefField.authorable);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Caption";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.caption, dstCollectionCdefField.caption);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ContentID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.contentId != dstCollectionCdefField.contentId);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "DeveloperOnly";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.developerOnly != dstCollectionCdefField.developerOnly);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "EditSortPriority";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.editSortPriority != dstCollectionCdefField.editSortPriority);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "EditTab";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.editTabName, dstCollectionCdefField.editTabName);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "FieldType";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.fieldTypeId != dstCollectionCdefField.fieldTypeId);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "HTMLContent";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.htmlContent != dstCollectionCdefField.htmlContent);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexColumn";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.indexColumn != dstCollectionCdefField.indexColumn);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexSortDirection";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.indexSortDirection != dstCollectionCdefField.indexSortDirection);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexSortOrder";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (EncodeInteger(srcCollectionCdefField.indexSortOrder) != genericController.EncodeInteger(dstCollectionCdefField.indexSortOrder));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IndexWidth";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.indexWidth, dstCollectionCdefField.indexWidth);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupContentID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.lookupContentID != dstCollectionCdefField.lookupContentID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupList";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.lookupList, dstCollectionCdefField.lookupList);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyContentID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.manyToManyContentID != dstCollectionCdefField.manyToManyContentID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleContentID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.manyToManyRuleContentID != dstCollectionCdefField.manyToManyRuleContentID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRulePrimaryField";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.ManyToManyRulePrimaryField, dstCollectionCdefField.ManyToManyRulePrimaryField);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleSecondaryField";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.ManyToManyRuleSecondaryField, dstCollectionCdefField.ManyToManyRuleSecondaryField);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "MemberSelectGroupID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.MemberSelectGroupID != dstCollectionCdefField.MemberSelectGroupID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "NotEditable";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.NotEditable != dstCollectionCdefField.NotEditable);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Password";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Password != dstCollectionCdefField.Password);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ReadOnly";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.ReadOnly != dstCollectionCdefField.ReadOnly);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectContentID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RedirectContentID != dstCollectionCdefField.RedirectContentID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectID";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.RedirectID, dstCollectionCdefField.RedirectID);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectPath";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.RedirectPath, dstCollectionCdefField.RedirectPath);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Required";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Required != dstCollectionCdefField.Required);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RSSDescriptionField";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RSSDescriptionField != dstCollectionCdefField.RSSDescriptionField);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RSSTitleField";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.RSSTitleField != dstCollectionCdefField.RSSTitleField);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "Scramble";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.Scramble != dstCollectionCdefField.Scramble);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "TextBuffered";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.TextBuffered != dstCollectionCdefField.TextBuffered);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "DefaultValue";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (genericController.encodeText(srcCollectionCdefField.defaultValue) != genericController.encodeText(dstCollectionCdefField.defaultValue));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "UniqueName";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.UniqueName != dstCollectionCdefField.UniqueName);
                                    if (okToUpdateDstFromSrc) {
                                        //okToUpdateDstFromSrc = okToUpdateDstFromSrc;
                                    }
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "IsBaseField";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || (srcCollectionCdefField.isBaseField != dstCollectionCdefField.isBaseField);
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "LookupContentName";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_lookupContentName(cpCore), dstCollectionCdefField.get_lookupContentName(cpCore));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyContentName";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_lookupContentName(cpCore), dstCollectionCdefField.get_lookupContentName(cpCore));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "ManyToManyRuleContentName";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_ManyToManyRuleContentName(cpCore), dstCollectionCdefField.get_ManyToManyRuleContentName(cpCore));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "RedirectContentName";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.get_RedirectContentName(cpCore), dstCollectionCdefField.get_RedirectContentName(cpCore));
                                    //
                                    if (!okToUpdateDstFromSrc) {
                                        n = "installedByCollectionid";
                                    }
                                    okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, srcCollectionCdefField.installedByCollectionGuid, dstCollectionCdefField.installedByCollectionGuid);
                                    //
                                    if (okToUpdateDstFromSrc) {
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
                            } else {
                                //
                                // field was not found in dst, add it and populate
                                //
                                dstCollectionCdef.fields.Add(SrcFieldName.ToLower(), new Models.Complex.CDefFieldModel());
                                dstCollectionCdefField = dstCollectionCdef.fields[SrcFieldName.ToLower()];
                                okToUpdateDstFromSrc = true;
                                HelpChanged = true;
                            }
                            //
                            // If okToUpdateDstFromSrc, update the Dst record with the Src record
                            //
                            if (okToUpdateDstFromSrc) {
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
                                if (HelpChanged) {
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
                foreach (miniCollectionModel.collectionSQLIndexModel srcSqlIndex in srcCollection.SQLIndexes) {
                    srcName = (srcSqlIndex.DataSourceName + "-" + srcSqlIndex.TableName + "-" + srcSqlIndex.IndexName).ToLower();
                    okToUpdateDstFromSrc = false;
                    //
                    // Search for this name in the Dst
                    bool indexFound = false;
                    bool indexChanged = false;
                    miniCollectionModel.collectionSQLIndexModel indexToUpdate = new miniCollectionModel.collectionSQLIndexModel() { };
                    foreach (miniCollectionModel.collectionSQLIndexModel dstSqlIndex in dstCollection.SQLIndexes) {
                        dstName = (dstSqlIndex.DataSourceName + "-" + dstSqlIndex.TableName + "-" + dstSqlIndex.IndexName).ToLower();
                        if (TextMatch(cpCore, dstName, srcName)) {
                            //
                            // found a match between Src and Dst
                            indexFound = true;
                            indexToUpdate = dstSqlIndex;
                            indexChanged = !TextMatch(cpCore, dstSqlIndex.FieldNameList, srcSqlIndex.FieldNameList);
                            break;
                        }
                    }
                    if (!indexFound) {
                        //
                        // add src to dst
                        dstCollection.SQLIndexes.Add(srcSqlIndex);
                    } else if (indexChanged && (indexToUpdate != null )) {
                        //
                        // update dst to src

                        indexToUpdate.dataChanged = true;
                        indexToUpdate.DataSourceName = srcSqlIndex.DataSourceName;
                        indexToUpdate.FieldNameList = srcSqlIndex.FieldNameList;
                        indexToUpdate.IndexName = srcSqlIndex.IndexName;
                        indexToUpdate.TableName = srcSqlIndex.TableName;
                    }
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Check menus
                //-------------------------------------------------------------------------------------------------
                //
                DataBuildVersion = cpCore.siteProperties.dataBuildVersion;
                foreach (var srcKvp in srcCollection.Menus) {
                    string srcKey = srcKvp.Key.ToLower() ;
                    miniCollectionModel.collectionMenuModel srcMenu = srcKvp.Value;
                    srcName = srcMenu.Name.ToLower();
                    string srcGuid = srcMenu.Guid;
                    string SrcParentName = genericController.vbLCase(srcMenu.ParentName);
                    string SrcNameSpace = genericController.vbLCase(srcMenu.menuNameSpace);
                    SrcIsNavigator = srcMenu.IsNavigator;
                    okToUpdateDstFromSrc = false;
                    //
                    // Search for match using guid
                    miniCollectionModel.collectionMenuModel dstMenuMatch = new miniCollectionModel.collectionMenuModel() { } ;
                    IsMatch = false;
                    foreach (var dstKvp in dstCollection.Menus) {
                        string dstKey = dstKvp.Key.ToLower();
                        miniCollectionModel.collectionMenuModel dstMenu = dstKvp.Value;
                        string dstGuid = dstMenu.Guid;
                        if (dstGuid == srcGuid) {
                            DstIsNavigator = dstMenu.IsNavigator;
                            DstKey = genericController.vbLCase(dstMenu.Key);
                            IsMatch = (DstKey == SrcKey) && (SrcIsNavigator == DstIsNavigator);
                            if (IsMatch) {
                                dstMenuMatch = dstMenu;
                                break;
                            }

                        }
                    }
                    if (!IsMatch) {
                        //
                        // no match found on guid, try name and ( either namespace or parentname )
                        foreach (var dstKvp in dstCollection.Menus) {
                            string dstKey = dstKvp.Key.ToLower();
                            miniCollectionModel.collectionMenuModel dstMenu = dstKvp.Value;
                            dstName = genericController.vbLCase(dstMenu.Name);
                            if ((srcName == dstName) && (SrcIsNavigator == DstIsNavigator)) {
                                if (SrcIsNavigator) {
                                    //
                                    // Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    IsMatch = (SrcNameSpace == genericController.vbLCase(dstMenu.menuNameSpace)) && (dstMenu.Guid == "");
                                } else {
                                    //
                                    // AdminMenu - check parentname
                                    IsMatch = (SrcParentName == genericController.vbLCase(dstMenu.ParentName));
                                }
                                if (IsMatch) {
                                    dstMenuMatch = dstMenu;
                                    break;
                                }
                            }
                        }
                    }
                    if(IsMatch) {
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstMenuMatch.Active != srcMenu.Active);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstMenuMatch.AdminOnly != srcMenu.AdminOnly);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.ContentName, srcMenu.ContentName);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstMenuMatch.DeveloperOnly != srcMenu.DeveloperOnly);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.LinkPage, srcMenu.LinkPage);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.Name, srcMenu.Name);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || (dstMenuMatch.NewWindow != srcMenu.NewWindow);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.SortOrder, srcMenu.SortOrder);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.AddonName, srcMenu.AddonName);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.NavIconType, srcMenu.NavIconType);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.NavIconTitle, srcMenu.NavIconTitle);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.menuNameSpace, srcMenu.menuNameSpace);
                        okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstMenuMatch.Guid, srcMenu.Guid);
                        dstCollection.Menus.Remove(DstKey);
                    }
                    dstCollection.Menus.Add(srcKey, srcMenu);
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Check addons -- yes, this should be done.
                //-------------------------------------------------------------------------------------------------
                //
                //If False Then
                //    '
                //    ' remove this for now -- later add ImportCollections to track the collections (not addons)
                //    '
                //    '
                //    '
                //    For SrcPtr = 0 To srcCollection.AddOnCnt - 1
                //        SrcContentName = genericController.vbLCase(srcCollection.AddOns[SrcPtr].Name)
                //        okToUpdateDstFromSrc = False
                //        '
                //        ' Search for this name in the Dst
                //        '
                //        For DstPtr = 0 To dstCollection.AddOnCnt - 1
                //            DstName = genericController.vbLCase(dstCollection.AddOns[dstPtr].Name)
                //            If DstName = SrcContentName Then
                //                '
                //                ' found a match between Src and Dst
                //                '
                //                If SrcIsUserCDef Then
                //                    '
                //                    ' test for cdef attribute changes
                //                    '
                //                    With dstCollection.AddOns[dstPtr]
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ArgumentList, srcCollection.AddOns[SrcPtr].ArgumentList)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Copy, srcCollection.AddOns[SrcPtr].Copy)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Link, srcCollection.AddOns[SrcPtr].Link)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.Name, srcCollection.AddOns[SrcPtr].Name)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.ObjectProgramID, srcCollection.AddOns[SrcPtr].ObjectProgramID)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(cpcore,.SortOrder, srcCollection.AddOns[SrcPtr].SortOrder)
                //                    End With
                //                End If
                //                Exit For
                //            End If
                //        Next
                //        If DstPtr = dstCollection.AddOnCnt Then
                //            '
                //            ' CDef was not found, add it
                //            '
                //            Array.Resize( ref asdf,asdf) // redim preserve  dstCollection.AddOns(dstCollection.AddOnCnt)
                //            dstCollection.AddOnCnt = DstPtr + 1
                //            okToUpdateDstFromSrc = True
                //        End If
                //        If okToUpdateDstFromSrc Then
                //            With dstCollection.AddOns[dstPtr]
                //                '
                //                ' It okToUpdateDstFromSrc, update the Dst with the Src
                //                '
                //                .CDefChanged = True
                //                .ArgumentList = srcCollection.AddOns[SrcPtr].ArgumentList
                //                .Copy = srcCollection.AddOns[SrcPtr].Copy
                //                .Link = srcCollection.AddOns[SrcPtr].Link
                //                .Name = srcCollection.AddOns[SrcPtr].Name
                //                .ObjectProgramID = srcCollection.AddOns[SrcPtr].ObjectProgramID
                //                .SortOrder = srcCollection.AddOns[SrcPtr].SortOrder
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
                for (srcStylePtr = 0; srcStylePtr < srcCollection.StyleCnt; srcStylePtr++) {
                    srcName = genericController.vbLCase(srcCollection.Styles[srcStylePtr].Name);
                    okToUpdateDstFromSrc = false;
                    //
                    // Search for this name in the Dst
                    //
                    for (dstStylePtr = 0; dstStylePtr < dstCollection.StyleCnt; dstStylePtr++) {
                        dstName = genericController.vbLCase(dstCollection.Styles[dstStylePtr].Name);
                        if (dstName == srcName) {
                            //
                            // found a match between Src and Dst
                            //
                            if (SrcIsUserCDef) {
                                //
                                // test for cdef attribute changes
                                //
                                okToUpdateDstFromSrc = okToUpdateDstFromSrc || !TextMatch(cpCore, dstCollection.Styles[dstStylePtr].Copy, srcCollection.Styles[srcStylePtr].Copy);
                            }
                            break;
                        }
                    }
                    if (dstStylePtr == dstCollection.StyleCnt) {
                        //
                        // CDef was not found, add it
                        //
                        Array.Resize(ref dstCollection.Styles, dstCollection.StyleCnt);
                        dstCollection.StyleCnt = dstStylePtr + 1;
                        okToUpdateDstFromSrc = true;
                    }
                    if (okToUpdateDstFromSrc) {
                        var tempVar6 = dstCollection.Styles[dstStylePtr];
                        //
                        // It okToUpdateDstFromSrc, update the Dst with the Src
                        //
                        tempVar6.dataChanged = true;
                        tempVar6.Copy = srcCollection.Styles[srcStylePtr].Copy;
                        tempVar6.Name = srcCollection.Styles[srcStylePtr].Name;
                    }
                }
                //
                //-------------------------------------------------------------------------------------------------
                // Add Collections
                //-------------------------------------------------------------------------------------------------
                //
                foreach( var import in srcCollection.collectionImports) {
                    dstCollection.collectionImports.Add(import);
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
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOk;
        }
        //
        //===========================================================================
        //   Error handler
        //===========================================================================
        //
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
        private static miniCollectionModel installCollection_GetApplicationMiniCollection(coreClass cpCore, bool isNewBuild) {
            miniCollectionModel returnColl = new miniCollectionModel();
            try {
                //
                string ExportFilename = null;
                string ExportPathPage = null;
                string CollectionData = null;
                //
                if (!isNewBuild) {
                    //
                    // if this is not an empty database, get the application collection, else return empty
                    //
                    ExportFilename = "cdef_export_" + encodeText(genericController.GetRandomInteger(cpCore)) + ".xml";
                    ExportPathPage = "tmp\\" + ExportFilename;
                    exportApplicationCDefXml(cpCore, ExportPathPage, true);
                    CollectionData = cpCore.privateFiles.readFile(ExportPathPage);
                    cpCore.privateFiles.deleteFile(ExportPathPage);
                    returnColl = installCollection_LoadXmlToMiniCollection(cpCore, CollectionData, false, false, isNewBuild, new miniCollectionModel());
                }
            } catch (Exception ex) {
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
        public static string GetXMLAttribute(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            string returnAttr = "";
            try {
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlAttribute NodeAttribute = null;
                XmlNode ResultNode = null;
                string UcaseName = null;
                //
                Found = false;
                ResultNode = Node.Attributes.GetNamedItem(Name);
                if (ResultNode == null) {
                    UcaseName = genericController.vbUCase(Name);
                    foreach (XmlAttribute NodeAttribute in Node.Attributes) {
                        if (genericController.vbUCase(NodeAttribute.Name) == UcaseName) {
                            returnAttr = NodeAttribute.Value;
                            Found = true;
                            break;
                        }
                    }
                    if (!Found) {
                        returnAttr = DefaultIfNotFound;
                    }
                } else {
                    returnAttr = ResultNode.Value;
                    Found = true;
                }
            } catch (Exception ex) {
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
        private static double GetXMLAttributeNumber(coreClass cpCore, bool Found, XmlNode Node, string Name, string DefaultIfNotFound) {
            return EncodeNumber(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound));
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private static bool GetXMLAttributeBoolean(coreClass cpCore, bool Found, XmlNode Node, string Name, bool DefaultIfNotFound) {
            return genericController.encodeBoolean(GetXMLAttribute(cpCore, Found, Node, Name, encodeText(DefaultIfNotFound)));
        }
        //
        //========================================================================
        //
        //========================================================================
        //
        private static int GetXMLAttributeInteger(coreClass cpCore, bool Found, XmlNode Node, string Name, int DefaultIfNotFound) {
            return genericController.EncodeInteger(GetXMLAttribute(cpCore, Found, Node, Name, DefaultIfNotFound.ToString()));
        }
        //
        //==================================================================================================================
        //
        //==================================================================================================================
        //
        private static bool TextMatch(coreClass cpCore, string Source1, string Source2) {
            if ( (Source1 == null) || (Source2 == null )) {
                return false;
            }else {
                return (Source1.ToLower() == Source2.ToLower());
            }
        }
        //
        //
        //
        private static string GetMenuNameSpace(coreClass cpCore, Dictionary<string,miniCollectionModel.collectionMenuModel> menus, miniCollectionModel.collectionMenuModel menu, string UsedIDList) {
            string returnAttr = "";
            try {
                string ParentName = null;
                int Ptr = 0;
                string Prefix = null;
                string LCaseParentName = null;

                //
                ParentName = menu.ParentName;
                if (!string.IsNullOrEmpty(ParentName)) {
                    LCaseParentName = genericController.vbLCase(ParentName);
                    foreach ( var kvp in menus) {
                        miniCollectionModel.collectionMenuModel testMenu = kvp.Value;
                        if (genericController.vbInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0) {
                            if (LCaseParentName == genericController.vbLCase(testMenu.Name) && (menu.IsNavigator == testMenu.IsNavigator)) {
                                Prefix = GetMenuNameSpace(cpCore, menus, testMenu, UsedIDList + "," + menu.Guid);
                                if (string.IsNullOrEmpty(Prefix)) {
                                    returnAttr = ParentName;
                                } else {
                                    returnAttr = Prefix + "." + ParentName;
                                }
                                break;
                            }
                        }

                    }
                }
            } catch (Exception ex) {
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
        private static void VerifySortMethod(coreClass cpCore, string Name, string OrderByCriteria) {
            try {
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
                if (dt.Rows.Count > 0) {
                    //
                    // update sort method
                    //
                    cpCore.db.updateTableRecord("Default", "ccSortMethods", "ID=" + genericController.EncodeInteger(dt.Rows[0]["ID"]).ToString(), sqlList);
                } else {
                    //
                    // Create the new sort method
                    //
                    cpCore.db.insertTableRecord("Default", "ccSortMethods", sqlList);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void VerifySortMethods(coreClass cpCore) {
            try {
                //
                logController.appendInstallLog(cpCore, "Verify Sort Records");
                //
                VerifySortMethod(cpCore, "By Name", "Name");
                VerifySortMethod(cpCore, "By Alpha Sort Order Field", "SortOrder");
                VerifySortMethod(cpCore, "By Date", "DateAdded");
                VerifySortMethod(cpCore, "By Date Reverse", "DateAdded Desc");
                VerifySortMethod(cpCore, "By Alpha Sort Order Then Oldest First", "SortOrder,ID");
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=============================================================================
        //   Get a ContentID from the ContentName using just the tables
        //=============================================================================
        //
        private static void VerifyContentFieldTypes(coreClass cpCore) {
            try {
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
                using (DataTable rs = cpCore.db.executeQuery("Select ID from ccFieldTypes order by id")) {
                    if (!isDataTableOk(rs)) {
                        //
                        // problem
                        //
                        TableBad = true;
                    } else {
                        //
                        // Verify the records that are there
                        //
                        RowsFound = 0;
                        foreach (DataRow dr in rs.Rows) {
                            RowsFound = RowsFound + 1;
                            if (RowsFound != genericController.EncodeInteger(dr["ID"])) {
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
                if (TableBad) {
                    cpCore.db.deleteTable("Default", "ccFieldTypes");
                    cpCore.db.createSQLTable("Default", "ccFieldTypes");
                    RowsFound = 0;
                }
                //
                // ----- Add the number of rows needed
                //
                RowsNeeded = FieldTypeIdMax - RowsFound;
                if (RowsNeeded > 0) {
                    CID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Types");
                    if (CID <= 0) {
                        //
                        // Problem
                        //
                        cpCore.handleException(new ApplicationException("Content Field Types content definition was not found"));
                    } else {
                        while (RowsNeeded > 0) {
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
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        ////
        ////=============================================================================
        ////
        ////=============================================================================
        ////
        //public void csv_VerifyAggregateScript(coreClass cpCore, string Name, string Link, string ArgumentList, string SortOrder) {
        //    try {
        //        //
        //        int CSEntry = 0;
        //        string ContentName = null;
        //        string MethodName;
        //        //
        //        MethodName = "csv_VerifyAggregateScript";
        //        //
        //        ContentName = "Aggregate Function Scripts";
        //        CSEntry = cpCore.db.csOpen(ContentName, "(name=" + cpCore.db.encodeSQLText(Name) + ")", "", false, 0, false, false, "Name,Link,ObjectProgramID,ArgumentList,SortOrder");
        //        //
        //        // If no current entry, create one
        //        //
        //        if (!cpCore.db.csOk(CSEntry)) {
        //            cpCore.db.csClose(ref CSEntry);
        //            CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID);
        //            if (cpCore.db.csOk(CSEntry)) {
        //                cpCore.db.csSet(CSEntry, "name", Name);
        //            }
        //        }
        //        if (cpCore.db.csOk(CSEntry)) {
        //            cpCore.db.csSet(CSEntry, "Link", Link);
        //            cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
        //            cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
        //        }
        //        cpCore.db.csClose(ref CSEntry);
        //    } catch (Exception ex) {
        //        cpCore.handleException(ex);
        //        throw;
        //    }
        //}
        ////
        ////=============================================================================
        ////
        ////=============================================================================
        ////
        //public void csv_VerifyAggregateReplacement(coreClass cpcore, string Name, string Copy, string SortOrder) {
        //    csv_VerifyAggregateReplacement2(cpcore, Name, Copy, "", SortOrder);
        //}
        ////
        //=============================================================================
        //
        //=============================================================================
        ////
        //public static void csv_VerifyAggregateReplacement2(coreClass cpCore, string Name, string Copy, string ArgumentList, string SortOrder) {
        //    try {
        //        //
        //        int CSEntry = 0;
        //        string ContentName = null;
        //        string MethodName;
        //        //
        //        MethodName = "csv_VerifyAggregateReplacement2";
        //        //
        //        ContentName = "Aggregate Function Replacements";
        //        CSEntry = cpCore.db.csOpen(ContentName, "(name=" + cpCore.db.encodeSQLText(Name) + ")", "", false, 0, false, false, "Name,Copy,SortOrder,ArgumentList");
        //        //
        //        // If no current entry, create one
        //        //
        //        if (!cpCore.db.csOk(CSEntry)) {
        //            cpCore.db.csClose(ref CSEntry);
        //            CSEntry = cpCore.db.csInsertRecord(ContentName, SystemMemberID);
        //            if (cpCore.db.csOk(CSEntry)) {
        //                cpCore.db.csSet(CSEntry, "name", Name);
        //            }
        //        }
        //        if (cpCore.db.csOk(CSEntry)) {
        //            cpCore.db.csSet(CSEntry, "Copy", Copy);
        //            cpCore.db.csSet(CSEntry, "SortOrder", SortOrder);
        //            cpCore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
        //        }
        //        cpCore.db.csClose(ref CSEntry);
        //    } catch (Exception ex) {
        //        cpCore.handleException(ex);
        //        throw;
        //    }
        //}
        ////
        ////=============================================================================
        ////
        ////=============================================================================
        ////
        //public void csv_VerifyAggregateObject(coreClass cpcore, string Name, string ObjectProgramID, string ArgumentList, string SortOrder) {
        //    try {
        //        //
        //        int CSEntry = 0;
        //        string ContentName = null;
        //        string MethodName;
        //        //
        //        MethodName = "csv_VerifyAggregateObject";
        //        //
        //        // Locate current entry
        //        //
        //        ContentName = "Aggregate Function Objects";
        //        CSEntry = cpcore.db.csOpen(ContentName, "(name=" + cpcore.db.encodeSQLText(Name) + ")", "", false, 0, false, false, "Name,Link,ObjectProgramID,ArgumentList,SortOrder");
        //        //
        //        // If no current entry, create one
        //        //
        //        if (!cpcore.db.csOk(CSEntry)) {
        //            cpcore.db.csClose(ref CSEntry);
        //            CSEntry = cpcore.db.csInsertRecord(ContentName, SystemMemberID);
        //            if (cpcore.db.csOk(CSEntry)) {
        //                cpcore.db.csSet(CSEntry, "name", Name);
        //            }
        //        }
        //        if (cpcore.db.csOk(CSEntry)) {
        //            cpcore.db.csSet(CSEntry, "ObjectProgramID", ObjectProgramID);
        //            cpcore.db.csSet(CSEntry, "ArgumentList", ArgumentList);
        //            cpcore.db.csSet(CSEntry, "SortOrder", SortOrder);
        //        }
        //        cpcore.db.csClose(ref CSEntry);
        //    } catch (Exception ex) {
        //        cpcore.handleException(ex);
        //        throw;
        //    }
        //}
        ////
        //========================================================================
        //
        //========================================================================
        //
        public static void exportApplicationCDefXml(coreClass cpCore, string privateFilesPathFilename, bool IncludeBaseFields) {
            try {
                collectionXmlController XML = null;
                string Content = null;
                //
                XML = new collectionXmlController(cpCore);
                Content = XML.GetXMLContentDefinition3("", IncludeBaseFields);
                cpCore.privateFiles.saveFile(privateFilesPathFilename, Content);
                XML = null;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }


        //
        //====================================================================================================
        //
        //   DownloadCollectionFiles
        //
        //   Download Library collection files into a folder
        //       Download Collection file and all attachments (DLLs) into working folder
        //       Unzips any collection files
        //       Returns true if it all downloads OK
        //
        private static bool DownloadCollectionFiles(coreClass cpCore, string workingPath, string CollectionGuid, ref DateTime return_CollectionLastChangeDate, ref string return_ErrorMessage) {
            bool tempDownloadCollectionFiles = false;
            tempDownloadCollectionFiles = false;
            try {
                //
                int CollectionFileCnt = 0;
                string CollectionFilePath = null;
                XmlDocument Doc = new XmlDocument();
                string URL = null;
                string ResourceFilename = null;
                string ResourceLink = null;
                string CollectionVersion = null;
                string CollectionFileLink = null;
                XmlDocument CollectionFile = new XmlDocument();
                string Collectionname = null;
                int Pos = 0;
                string UserError = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CDefSection = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CDefInterfaces = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode ActiveXNode = null;
                string errorPrefix = null;
                int downloadRetry = 0;
                const int downloadRetryMax = 3;
                //
                logController.appendInstallLog(cpCore, "downloading collection [" + CollectionGuid + "]");
                //
                //---------------------------------------------------------------------------------------------------------------
                // Request the Download file for this collection
                //---------------------------------------------------------------------------------------------------------------
                //
                Doc = new XmlDocument();
                URL = "http://support.contensive.com/GetCollection?iv=" + cpCore.codeVersion() + "&guid=" + CollectionGuid;
                errorPrefix = "DownloadCollectionFiles, Error reading the collection library status file from the server for Collection [" + CollectionGuid + "], download URL [" + URL + "]. ";
                downloadRetry = 0;
                int downloadDelay = 2000;
                do {
                    try {
                        tempDownloadCollectionFiles = true;
                        return_ErrorMessage = "";
                        //
                        // -- pause for a second between fetches to pace the server (<10 hits in 10 seconds)
                        Thread.Sleep(downloadDelay);
                        //
                        // -- download file
                        System.Net.WebRequest rq = System.Net.WebRequest.Create(URL);
                        rq.Timeout = 60000;
                        System.Net.WebResponse response = rq.GetResponse();
                        Stream responseStream = response.GetResponseStream();
                        XmlTextReader reader = new XmlTextReader(responseStream);
                        Doc.Load(reader);
                        break;
                        //Call Doc.Load(URL)
                    } catch (Exception ex) {
                        //
                        // this error could be data related, and may not be critical. log issue and continue
                        //
                        downloadDelay += 2000;
                        return_ErrorMessage = "There was an error while requesting the download details for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        logController.appendInstallLog(cpCore, errorPrefix + "There was a parse error reading the response [" + ex.ToString() + "]");
                    }
                    downloadRetry += 1;
                } while (downloadRetry < downloadRetryMax);
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    //
                    // continue if no errors
                    //
                    if (Doc.DocumentElement.Name.ToLower() != genericController.vbLCase(DownloadFileRootNode)) {
                        return_ErrorMessage = "The collection file from the server was Not valid for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        logController.appendInstallLog(cpCore, errorPrefix + "The response has a basename [" + Doc.DocumentElement.Name + "] but [" + DownloadFileRootNode + "] was expected.");
                    } else {
                        //
                        //------------------------------------------------------------------
                        // Parse the Download File and download each file into the working folder
                        //------------------------------------------------------------------
                        //
                        if (Doc.DocumentElement.ChildNodes.Count == 0) {
                            return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes.";
                            logController.appendInstallLog(cpCore, errorPrefix + "The collection library status file from the server has a valid basename, but no childnodes. The collection was probably Not found");
                            tempDownloadCollectionFiles = false;
                        } else {
                            foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                switch (genericController.vbLCase(CDefSection.Name)) {
                                    case "collection":
                                        //
                                        // Read in the interfaces and save to Add-ons
                                        //
                                        ResourceFilename = "";
                                        ResourceLink = "";
                                        Collectionname = "";
                                        CollectionGuid = "";
                                        CollectionVersion = "";
                                        CollectionFileLink = "";
                                        foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                            switch (genericController.vbLCase(CDefInterfaces.Name)) {
                                                case "name":
                                                    Collectionname = CDefInterfaces.InnerText;
                                                    break;
                                                case "help":
                                                    //CollectionHelp = CDefInterfaces.innerText
                                                    cpCore.privateFiles.saveFile(workingPath + "Collection.hlp", CDefInterfaces.InnerText);
                                                    break;
                                                case "guid":
                                                    CollectionGuid = CDefInterfaces.InnerText;
                                                    break;
                                                case "lastchangedate":
                                                    return_CollectionLastChangeDate = genericController.EncodeDate(CDefInterfaces.InnerText);
                                                    break;
                                                case "version":
                                                    CollectionVersion = CDefInterfaces.InnerText;
                                                    break;
                                                case "collectionfilelink":
                                                    CollectionFileLink = CDefInterfaces.InnerText;
                                                    CollectionFileCnt = CollectionFileCnt + 1;
                                                    if (!string.IsNullOrEmpty(CollectionFileLink)) {
                                                        Pos = CollectionFileLink.LastIndexOf("/") + 1;
                                                        if ((Pos <= 0) && (Pos < CollectionFileLink.Length)) {
                                                            //
                                                            // Skip this file because the collecion file link has no slash (no file)
                                                            //
                                                            logController.appendInstallLog(cpCore, errorPrefix + "Collection [" + Collectionname + "] was Not installed because the Collection File Link does Not point to a valid file [" + CollectionFileLink + "]");
                                                        } else {
                                                            CollectionFilePath = workingPath + CollectionFileLink.Substring(Pos);
                                                            cpCore.privateFiles.SaveRemoteFile(CollectionFileLink, CollectionFilePath);
                                                            // BuildCollectionFolder takes care of the unzipping.
                                                            //If genericController.vbLCase(Right(CollectionFilePath, 4)) = ".zip" Then
                                                            //    Call UnzipAndDeleteFile_AndWait(CollectionFilePath)
                                                            //End If
                                                            //DownloadCollectionFiles = True
                                                        }
                                                    }
                                                    break;
                                                case "activexdll":
                                                case "resourcelink":
                                                    //
                                                    // save the filenames and download them only if OKtoinstall
                                                    //
                                                    ResourceFilename = "";
                                                    ResourceLink = "";
                                                    foreach (XmlNode ActiveXNode in CDefInterfaces.ChildNodes) {
                                                        switch (genericController.vbLCase(ActiveXNode.Name)) {
                                                            case "filename":
                                                                ResourceFilename = ActiveXNode.InnerText;
                                                                break;
                                                            case "link":
                                                                ResourceLink = ActiveXNode.InnerText;
                                                                break;
                                                        }
                                                    }
                                                    if (string.IsNullOrEmpty(ResourceLink)) {
                                                        UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. An ActiveXDll node with filename [" + ResourceFilename + "] contained no 'Link' attribute.";
                                                        logController.appendInstallLog(cpCore, errorPrefix + UserError);
                                                    } else {
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            //
                                                            // Take Filename from Link
                                                            //
                                                            Pos = ResourceLink.LastIndexOf("/") + 1;
                                                            if (Pos != 0) {
                                                                ResourceFilename = ResourceLink.Substring(Pos);
                                                            }
                                                        }
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" + ResourceLink + "].";
                                                            logController.appendInstallLog(cpCore, errorPrefix + UserError);
                                                        } else {
                                                            cpCore.privateFiles.SaveRemoteFile(ResourceLink, workingPath + ResourceFilename);
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            if (CollectionFileCnt == 0) {
                                logController.appendInstallLog(cpCore, errorPrefix + "The collection was requested and downloaded, but was not installed because the download file did not have a collection root node.");
                            }
                        }
                    }
                }
                //
                // no - register anything that downloaded correctly - if this collection contains an import, and one of the imports has a problem, all the rest need to continue
                //
                //
                //If Not DownloadCollectionFiles Then
                //    '
                //    ' Must clear these out, if there is an error, a reset will keep the error message from making it to the page
                //    '
                //    Return_IISResetRequired = False
                //    Return_RegisterList = ""
                //End If
                //
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return tempDownloadCollectionFiles;
        }
        //
        //====================================================================================================
        //
        //   Upgrade all Apps from a Library Collection
        //
        //   If the collection is not in the local collection, download it, otherwise, use what is in local (do not check for updates)
        //     -Does not check if local collections are up-to-date, assume they are. (builderAllLocalCollectionsFromLib2 upgrades local collections)
        //
        //   If TargetAppName is blank, force install on all apps. (force install means install if missing)
        //
        //   Go through each app and call UpgradeAllAppsFromLocalCollect with allowupgrade FALSE (if found in app already, skip the app)
        //       If Collection is already installed on an App, do not builder.
        //
        //   Returns true if no errors during upgrade
        //
        //=========================================================================================================================
        //
        public static bool installCollectionFromRemoteRepo(coreClass cpCore, string collectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool UpgradeOK = true;
            try {
                if (string.IsNullOrWhiteSpace(collectionGuid)) {
                    logController.appendLog(cpCore, "collectionGuid is null", "debug");
                } else {
                    //
                    // normalize guid
                    if (collectionGuid.Length < 38) {
                        if (collectionGuid.Length == 32) {
                            collectionGuid = collectionGuid.Left(8) + "-" + collectionGuid.Substring(8, 4) + "-" + collectionGuid.Substring(12, 4) + "-" + collectionGuid.Substring(16, 4) + "-" + collectionGuid.Substring(20);
                        }
                        if (collectionGuid.Length == 36) {
                            collectionGuid = "{" + collectionGuid + "}";
                        }
                    }
                    //
                    // Install it if it is not already here
                    //
                    string CollectionVersionFolderName = GetCollectionPath(cpCore, collectionGuid);
                    if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                        //
                        // Download all files for this collection and build the collection folder(s)
                        //
                        string workingPath = cpCore.addon.getPrivateFilesAddonPath() + "temp_" + genericController.GetRandomInteger(cpCore) + "\\";
                        cpCore.privateFiles.createPath(workingPath);
                        //
                        DateTime CollectionLastChangeDate = default(DateTime);
                        UpgradeOK = DownloadCollectionFiles(cpCore, workingPath, collectionGuid, ref CollectionLastChangeDate, ref return_ErrorMessage);
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        } else {
                            List<string> collectionGuidList = new List<string>();
                            UpgradeOK = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, ref collectionGuidList, ref return_ErrorMessage, false);
                            if (!UpgradeOK) {
                                //UpgradeOK = UpgradeOK;
                            }
                        }
                        //
                        cpCore.privateFiles.DeleteFileFolder(workingPath);
                    }
                    //
                    // Upgrade the server from the collection files
                    //
                    if (UpgradeOK) {
                        UpgradeOK = installCollectionFromLocalRepo(cpCore, collectionGuid, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, ref nonCriticalErrorList);
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return UpgradeOK;
        }
        public static bool installCollectionFromRemoteRepo(coreClass cpCore, string CollectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild){
            var tmpList = new List<string> { };
            return installCollectionFromRemoteRepo(cpCore, CollectionGuid, ref return_ErrorMessage,ImportFromCollectionsGuidList, IsNewBuild, ref tmpList);
        }
        //
        //====================================================================================================
        //
        // Upgrades all collections, registers and resets the server if needed
        //
        public static bool UpgradeLocalCollectionRepoFromRemoteCollectionRepo(coreClass cpCore, ref string return_ErrorMessage, ref string return_RegisterList, ref bool return_IISResetRequired, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool returnOk = true;
            try {
                bool localCollectionUpToDate = false;
                string[] GuidArray = { };
                int GuidCnt = 0;
                int GuidPtr = 0;
                int RequestPtr = 0;
                string SupportURL = null;
                string GuidList = null;
                DateTime CollectionLastChangeDate = default(DateTime);
                string workingPath = null;
                string LocalFile = null;
                string LocalGuid = null;
                string LocalLastChangeDateStr = null;
                DateTime LocalLastChangeDate = default(DateTime);
                string LibName = "";
                bool LibSystem = false;
                string LibGUID = null;
                string LibLastChangeDateStr = null;
                string LibContensiveVersion = "";
                DateTime LibLastChangeDate = default(DateTime);
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode LibListNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode LocalListNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CollectionNode = null;
                XmlNode LocalLastChangeNode = null;
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                string Copy = null;
                bool allowLogging;
                //Dim builder As New coreBuilderClass(cpCore)
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                allowLogging = false;
                //
                if (allowLogging) {
                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Enter");
                }
                LocalFile = getCollectionListFile(cpCore);
                if (!string.IsNullOrEmpty(LocalFile)) {
                    LocalCollections = new XmlDocument();
                    try {
                        LocalCollections.LoadXml(LocalFile);
                    } catch (Exception ex) {
                        if (allowLogging) {
                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), parse error reading collections.xml");
                        }
                        Copy = "Error loading privateFiles\\addons\\Collections.xml";
                        logController.appendInstallLog(cpCore, Copy);
                        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                        returnOk = false;
                    }
                    if (returnOk) {
                        if (genericController.vbLCase(LocalCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                            if (allowLogging) {
                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The addons\\Collections.xml file has an invalid root node");
                            }
                            Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                            //Copy = "The LocalCollections file [" & App.Path & "\Addons\Collections.xml] has an invalid root node, [" & LocalCollections.DocumentElement.name & "] was received and [" & CollectionListRootNode & "] was expected."
                            logController.appendInstallLog(cpCore, Copy);
                            return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                            returnOk = false;
                        } else {
                            //
                            // Get a list of the collection guids on this server
                            //

                            GuidCnt = 0;
                            if (genericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist") {
                                foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                    switch (genericController.vbLCase(LocalListNode.Name)) {
                                        case "collection":
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                if (genericController.vbLCase(CollectionNode.Name) == "guid") {
                                                    Array.Resize(ref GuidArray, GuidCnt + 1);
                                                    GuidArray[GuidCnt] = CollectionNode.InnerText;
                                                    GuidCnt = GuidCnt + 1;
                                                    break;
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            if (allowLogging) {
                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), collection.xml file has " + GuidCnt + " collection nodes.");
                            }
                            if (GuidCnt > 0) {
                                //
                                // Request collection updates 10 at a time
                                //
                                GuidPtr = 0;
                                while (GuidPtr < GuidCnt) {
                                    RequestPtr = 0;
                                    GuidList = "";
                                    while ((GuidPtr < GuidCnt) && RequestPtr < 10) {
                                        GuidList = GuidList + "," + GuidArray[GuidPtr];
                                        GuidPtr = GuidPtr + 1;
                                        RequestPtr = RequestPtr + 1;
                                    }
                                    //
                                    // Request these 10 from the support library
                                    //
                                    //If genericController.vbInstr(1, GuidList, "58c9", vbTextCompare) <> 0 Then
                                    //    GuidList = GuidList
                                    //End If
                                    if (!string.IsNullOrEmpty(GuidList)) {
                                        GuidList = GuidList.Substring(1);
                                        //
                                        //-----------------------------------------------------------------------------------------------
                                        //   Load LibraryCollections from the Support Site
                                        //-----------------------------------------------------------------------------------------------
                                        //
                                        if (allowLogging) {
                                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), requesting Library updates for [" + GuidList + "]");
                                        }
                                        //hint = "Getting CollectionList"
                                        LibraryCollections = new XmlDocument();
                                        SupportURL = "http://support.contensive.com/GetCollectionList?iv=" + cpCore.codeVersion() + "&guidlist=" + EncodeRequestVariable(GuidList);
                                        bool loadOK = true;
                                        try {
                                            LibraryCollections.Load(SupportURL);
                                        } catch (Exception ex) {
                                            if (allowLogging) {
                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Error downloading or loading GetCollectionList from Support.");
                                            }
                                            Copy = "Error downloading or loading GetCollectionList from Support.";
                                            logController.appendInstallLog(cpCore, Copy + ", the request was [" + SupportURL + "]");
                                            return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                            returnOk = false;
                                            loadOK = false;
                                        }
                                        if (loadOK) {
                                            if (true) {
                                                if (genericController.vbLCase(LibraryCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                                                    Copy = "The GetCollectionList support site remote method returned an xml file with an invalid root node, [" + LibraryCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                                                    if (allowLogging) {
                                                        logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), " + Copy);
                                                    }
                                                    logController.appendInstallLog(cpCore, Copy + ", the request was [" + SupportURL + "]");
                                                    return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                                    returnOk = false;
                                                } else {
                                                    if (genericController.vbLCase(LocalCollections.DocumentElement.Name) != "collectionlist") {
                                                        logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The Library response did not have a collectioinlist top node, the request was [" + SupportURL + "]");
                                                    } else {
                                                        //
                                                        //-----------------------------------------------------------------------------------------------
                                                        // Search for Collection Updates Needed
                                                        //-----------------------------------------------------------------------------------------------
                                                        //
                                                        foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                                            localCollectionUpToDate = false;
                                                            if (allowLogging) {
                                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Process local collection.xml node [" + LocalListNode.Name + "]");
                                                            }
                                                            switch (genericController.vbLCase(LocalListNode.Name)) {
                                                                case "collection":
                                                                    LocalGuid = "";
                                                                    LocalLastChangeDateStr = "";
                                                                    LocalLastChangeDate = DateTime.MinValue;
                                                                    LocalLastChangeNode = null;
                                                                    foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                                        switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                            case "guid":
                                                                                //
                                                                                LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                                                //LocalGUID = genericController.vbReplace(LocalGUID, "{", "")
                                                                                //LocalGUID = genericController.vbReplace(LocalGUID, "}", "")
                                                                                //LocalGUID = genericController.vbReplace(LocalGUID, "-", "")
                                                                                break;
                                                                            case "lastchangedate":
                                                                                //
                                                                                LocalLastChangeDateStr = CollectionNode.InnerText;
                                                                                LocalLastChangeNode = CollectionNode;
                                                                                break;
                                                                        }
                                                                    }
                                                                    if (!string.IsNullOrEmpty(LocalGuid)) {
                                                                        if (!dateController.IsDate(LocalLastChangeDateStr)) {
                                                                            LocalLastChangeDate = DateTime.MinValue;
                                                                        } else {
                                                                            LocalLastChangeDate = genericController.EncodeDate(LocalLastChangeDateStr);
                                                                        }
                                                                    }
                                                                    if (allowLogging) {
                                                                        logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), node is collection, LocalGuid [" + LocalGuid + "], LocalLastChangeDateStr [" + LocalLastChangeDateStr + "]");
                                                                    }
                                                                    //
                                                                    // go through each collection on the Library and find the local collection guid
                                                                    //
                                                                    foreach (XmlNode LibListNode in LibraryCollections.DocumentElement.ChildNodes) {
                                                                        if (localCollectionUpToDate) {
                                                                            break;
                                                                        }
                                                                        switch (genericController.vbLCase(LibListNode.Name)) {
                                                                            case "collection":
                                                                                LibGUID = "";
                                                                                LibLastChangeDateStr = "";
                                                                                LibLastChangeDate = DateTime.MinValue;
                                                                                foreach (XmlNode CollectionNode in LibListNode.ChildNodes) {
                                                                                    switch (genericController.vbLCase(CollectionNode.Name)) {
                                                                                        case "name":
                                                                                            //
                                                                                            LibName = genericController.vbLCase(CollectionNode.InnerText);
                                                                                            break;
                                                                                        case "system":
                                                                                            //
                                                                                            LibSystem = genericController.encodeBoolean(CollectionNode.InnerText);
                                                                                            break;
                                                                                        case "guid":
                                                                                            //
                                                                                            LibGUID = genericController.vbLCase(CollectionNode.InnerText);
                                                                                            //LibGUID = genericController.vbReplace(LibGUID, "{", "")
                                                                                            //LibGUID = genericController.vbReplace(LibGUID, "}", "")
                                                                                            //LibGUID = genericController.vbReplace(LibGUID, "-", "")
                                                                                            break;
                                                                                        case "lastchangedate":
                                                                                            //
                                                                                            LibLastChangeDateStr = CollectionNode.InnerText;
                                                                                            //LibLastChangeDateStr = LibLastChangeDateStr;
                                                                                            break;
                                                                                        case "contensiveversion":
                                                                                            //
                                                                                            LibContensiveVersion = CollectionNode.InnerText;
                                                                                            break;
                                                                                    }
                                                                                }
                                                                                if (!string.IsNullOrEmpty(LibGUID)) {
                                                                                    if (genericController.vbInstr(1, LibGUID, "58c9", 1) != 0) {
                                                                                        //LibGUID = LibGUID;
                                                                                    }
                                                                                    if ((!string.IsNullOrEmpty(LibGUID)) & (LibGUID == LocalGuid) & ((string.IsNullOrEmpty(LibContensiveVersion)) || (string.CompareOrdinal(LibContensiveVersion, cpCore.codeVersion()) <= 0))) {
                                                                                        //
                                                                                        // LibCollection matches the LocalCollection - process the upgrade
                                                                                        //
                                                                                        if (allowLogging) {
                                                                                            logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Library collection node found that matches");
                                                                                        }
                                                                                        if (genericController.vbInstr(1, LibGUID, "58c9", 1) != 0) {
                                                                                            //LibGUID = LibGUID;
                                                                                        }
                                                                                        if (!dateController.IsDate(LibLastChangeDateStr)) {
                                                                                            LibLastChangeDate = DateTime.MinValue;
                                                                                        } else {
                                                                                            LibLastChangeDate = genericController.EncodeDate(LibLastChangeDateStr);
                                                                                        }
                                                                                        // TestPoint 1.1 - Test each collection for upgrade
                                                                                        if (LibLastChangeDate > LocalLastChangeDate) {
                                                                                            //
                                                                                            // LibLastChangeDate <>0, and it is > local lastchangedate
                                                                                            //
                                                                                            workingPath = cpCore.addon.getPrivateFilesAddonPath() + "\\temp_" + genericController.GetRandomInteger(cpCore) + "\\";
                                                                                            if (allowLogging) {
                                                                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), matching library collection is newer, start upgrade [" + workingPath + "].");
                                                                                            }
                                                                                            logController.appendInstallLog(cpCore, "Upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], because LocalChangeDate [" + LocalLastChangeDate + "] < LibraryChangeDate [" + LibLastChangeDate + "]");
                                                                                            //
                                                                                            // Upgrade Needed
                                                                                            //
                                                                                            cpCore.privateFiles.createPath(workingPath);
                                                                                            //
                                                                                            returnOk = DownloadCollectionFiles(cpCore, workingPath, LibGUID, ref CollectionLastChangeDate, ref return_ErrorMessage);
                                                                                            if (allowLogging) {
                                                                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), DownloadCollectionFiles returned " + returnOk);
                                                                                            }
                                                                                            if (returnOk) {
                                                                                                List<string> listGuidList = new List<string>();
                                                                                                returnOk = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, ref listGuidList, ref return_ErrorMessage, allowLogging);
                                                                                                if (allowLogging) {
                                                                                                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), BuildLocalCollectionFolder returned " + returnOk);
                                                                                                }
                                                                                            }
                                                                                            //
                                                                                            if (allowLogging) {
                                                                                                logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), working folder not deleted because debugging. Delete tmp folders when finished.");
                                                                                            } else {
                                                                                                cpCore.privateFiles.DeleteFileFolder(workingPath);
                                                                                            }
                                                                                            //
                                                                                            // Upgrade the apps from the collection files, do not install on any apps
                                                                                            //
                                                                                            if (returnOk) {
                                                                                                returnOk = installCollectionFromLocalRepo(cpCore, LibGUID, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, ref nonCriticalErrorList);
                                                                                                if (allowLogging) {
                                                                                                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), UpgradeAllAppsFromLocalCollection returned " + returnOk);
                                                                                                }
                                                                                            }
                                                                                            //
                                                                                            // make sure this issue is logged and clear the flag to let other local collections install
                                                                                            //
                                                                                            if (!returnOk) {
                                                                                                if (allowLogging) {
                                                                                                    logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), for this local collection, process returned " + returnOk);
                                                                                                }
                                                                                                logController.appendInstallLog(cpCore, "There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection, the request was [" + SupportURL + "]");
                                                                                                returnOk = true;
                                                                                            }
                                                                                        }
                                                                                        //
                                                                                        // this local collection has been resolved, go to the next local collection
                                                                                        //
                                                                                        localCollectionUpToDate = true;
                                                                                        //
                                                                                        if (!returnOk) {
                                                                                            logController.appendInstallLog(cpCore, "There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection");
                                                                                            returnOk = true;
                                                                                        }
                                                                                    }
                                                                                }
                                                                                break;
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
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        //
        //   Upgrade a collection from the files in a working folder
        //
        //   This routine always installs the Collection
        //       Builds Add-on folder, copies Resourcefiles (XML, DLLs, images) into the folder, and returns a list of DLLs to register
        //       Then goes from app to app to find either the TargetInstallApp, or any with this Collection already installed
        //       ImportCollections should call the UpgradeFromLibraryGUID
        //
        //   First, it unzips any zip file in the working folder
        //   The collection file is in the WorkingFolder.
        //   If there are attachments (DLLs), they should be in the WorkingFolder also
        //
        //   This is the routine that updates the Collections.xml file
        //       - if it parses ok
        //
        public static bool BuildLocalCollectionReposFromFolder(coreClass cpCore, string sourcePrivateFolderPath, DateTime CollectionLastChangeDate, ref List<string> return_CollectionGUIDList, ref string return_ErrorMessage, bool allowLogging) {
            bool success = false;
            try {
                if (cpCore.privateFiles.pathExists(sourcePrivateFolderPath)) {
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" + sourcePrivateFolderPath + "]");
                    FileInfo[] SrcFileNamelist = cpCore.privateFiles.getFileList(sourcePrivateFolderPath);
                    foreach (FileInfo file in SrcFileNamelist) {
                        if ((file.Extension == ".zip") || (file.Extension == ".xml")) {
                            string collectionGuid = "";
                            success = BuildLocalCollectionRepoFromFile(cpCore, sourcePrivateFolderPath + file.Name, CollectionLastChangeDate, ref collectionGuid, ref return_ErrorMessage, allowLogging);
                            return_CollectionGUIDList.Add(collectionGuid);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return success;
        }
        //
        //
        //
        public static bool BuildLocalCollectionRepoFromFile(coreClass cpCore, string collectionPathFilename, DateTime CollectionLastChangeDate, ref string return_CollectionGUID, ref string return_ErrorMessage, bool allowLogging) {
            bool tempBuildLocalCollectionRepoFromFile = false;
            bool result = true;
            try {
                string ResourceType = null;
                string CollectionVersionFolderName = "";
                DateTime ChildCollectionLastChangeDate = default(DateTime);
                string ChildWorkingPath = null;
                string ChildCollectionGUID = null;
                string ChildCollectionName = null;
                bool Found = false;
                XmlDocument CollectionFile = new XmlDocument();
                bool UpdatingCollection = false;
                string Collectionname = "";
                DateTime NowTime = default(DateTime);
                int NowPart = 0;
                FileInfo[] SrcFileNamelist = null;
                string TimeStamp = null;
                int Pos = 0;
                string CollectionFolder = null;
                string CollectionGuid = "";
                string AOGuid = null;
                string AOName = null;
                bool IsFound = false;
                string Filename = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CDefSection = null;
                XmlDocument Doc = new XmlDocument();
                XmlNode CDefInterfaces = null;
                bool StatusOK = false;
                string CollectionFileBaseName = null;
                collectionXmlController XMLTools = new collectionXmlController(cpCore);
                string CollectionFolderName = "";
                bool CollectionFileFound = false;
                bool ZipFileFound = false;
                string collectionPath = "";
                string collectionFilename = "";
                //
                // process all xml files in this workingfolder
                //
                if (allowLogging) {
                    logController.appendLog(cpCore, "BuildLocalCollectionFolder(), Enter");
                }
                //
                cpCore.privateFiles.splitPathFilename(collectionPathFilename, ref collectionPath, ref collectionFilename);
                if (!cpCore.privateFiles.pathExists(collectionPath)) {
                    //
                    // The working folder is not there
                    //
                    result = false;
                    return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
                    if (allowLogging) {
                        logController.appendLog(cpCore, "BuildLocalCollectionFolder(), " + return_ErrorMessage);
                    }
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" + collectionPath + "]");
                } else {
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" + collectionPath + "]");
                    //
                    // move collection file to a temp directory
                    //
                    string tmpInstallPath = "tmpInstallCollection" + genericController.createGuid().Replace("{", "").Replace("}", "").Replace("-", "") + "\\";
                    cpCore.privateFiles.copyFile(collectionPathFilename, tmpInstallPath + collectionFilename);
                    if (collectionFilename.ToLower().Substring(collectionFilename.Length - 4) == ".zip") {
                        cpCore.privateFiles.UnzipFile(tmpInstallPath + collectionFilename);
                        cpCore.privateFiles.deleteFile(tmpInstallPath + collectionFilename);
                    }
                    //
                    // install the individual files
                    //
                    SrcFileNamelist = cpCore.privateFiles.getFileList(tmpInstallPath);
                    if (true) {
                        //
                        // Process all non-zip files
                        //
                        foreach (FileInfo file in SrcFileNamelist) {
                            Filename = file.Name;
                            logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files, filename=[" + Filename + "]");
                            if (genericController.vbLCase(Filename.Substring(Filename.Length - 4)) == ".xml") {
                                //
                                logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing xml file [" + Filename + "]");
                                //hint = hint & ",320"
                                CollectionFile = new XmlDocument();
                                bool loadOk = true;
                                try {
                                    CollectionFile.LoadXml(cpCore.privateFiles.readFile(tmpInstallPath + Filename));
                                } catch (Exception ex) {
                                    //
                                    // There was a parse error in this xml file. Set the return message and the flag
                                    // If another xml files shows up, and process OK it will cover this error
                                    //
                                    //hint = hint & ",330"
                                    return_ErrorMessage = "There was a problem installing the Collection File [" + tmpInstallPath + Filename + "]. The error reported was [" + ex.Message + "].";
                                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, error reading collection [" + collectionPathFilename + "]");
                                    //StatusOK = False
                                    loadOk = false;
                                }
                                if (loadOk) {
                                    //hint = hint & ",400"
                                    CollectionFileBaseName = genericController.vbLCase(CollectionFile.DocumentElement.Name);
                                    if ((CollectionFileBaseName != "contensivecdef") & (CollectionFileBaseName != CollectionFileRootNode) & (CollectionFileBaseName != genericController.vbLCase(CollectionFileRootNodeOld))) {
                                        //
                                        // Not a problem, this is just not a collection file
                                        //
                                        logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, xml base name wrong [" + CollectionFileBaseName + "]");
                                    } else {
                                        //
                                        // Collection File
                                        //
                                        //hint = hint & ",420"
                                        Collectionname = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(Collectionname)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            result = false;
                                            return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>";
                                            logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, collection has no name");
                                        } else {
                                            //
                                            //------------------------------------------------------------------
                                            // Build Collection folder structure in /Add-ons folder
                                            //------------------------------------------------------------------
                                            //
                                            //hint = hint & ",440"
                                            CollectionFileFound = true;
                                            CollectionGuid = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
                                            if (string.IsNullOrEmpty(CollectionGuid)) {
                                                //
                                                // I hope I do not regret this
                                                //
                                                CollectionGuid = Collectionname;
                                            }

                                            CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid);
                                            if (!string.IsNullOrEmpty(CollectionVersionFolderName)) {
                                                //
                                                // This is an upgrade
                                                //
                                                //hint = hint & ",450"
                                                UpdatingCollection = true;
                                                Pos = genericController.vbInstr(1, CollectionVersionFolderName, "\\");
                                                if (Pos > 0) {
                                                    CollectionFolderName = CollectionVersionFolderName.Left( Pos - 1);
                                                }
                                            } else {
                                                //
                                                // This is an install
                                                //
                                                //hint = hint & ",460"
                                                CollectionFolderName = CollectionGuid;
                                                CollectionFolderName = genericController.vbReplace(CollectionFolderName, "{", "");
                                                CollectionFolderName = genericController.vbReplace(CollectionFolderName, "}", "");
                                                CollectionFolderName = genericController.vbReplace(CollectionFolderName, "-", "");
                                                CollectionFolderName = genericController.vbReplace(CollectionFolderName, " ", "");
                                                CollectionFolderName = Collectionname + "_" + CollectionFolderName;
                                            }
                                            CollectionFolder = cpCore.addon.getPrivateFilesAddonPath() + CollectionFolderName + "\\";
                                            if (!cpCore.privateFiles.pathExists(CollectionFolder)) {
                                                //
                                                // Create collection folder
                                                //
                                                //hint = hint & ",470"
                                                cpCore.privateFiles.createPath(CollectionFolder);
                                            }
                                            //
                                            // create a collection 'version' folder for these new files
                                            //
                                            TimeStamp = "";
                                            NowTime = DateTime.Now;
                                            NowPart = NowTime.Year;
                                            TimeStamp += NowPart.ToString();
                                            NowPart = NowTime.Month;
                                            if (NowPart < 10) {
                                                TimeStamp += "0";
                                            }
                                            TimeStamp += NowPart.ToString();
                                            NowPart = NowTime.Day;
                                            if (NowPart < 10) {
                                                TimeStamp += "0";
                                            }
                                            TimeStamp += NowPart.ToString();
                                            NowPart = NowTime.Hour;
                                            if (NowPart < 10) {
                                                TimeStamp += "0";
                                            }
                                            TimeStamp += NowPart.ToString();
                                            NowPart = NowTime.Minute;
                                            if (NowPart < 10) {
                                                TimeStamp += "0";
                                            }
                                            TimeStamp += NowPart.ToString();
                                            NowPart = NowTime.Second;
                                            if (NowPart < 10) {
                                                TimeStamp += "0";
                                            }
                                            TimeStamp += NowPart.ToString();
                                            CollectionVersionFolderName = CollectionFolderName + "\\" + TimeStamp;
                                            string CollectionVersionFolder = cpCore.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName;
                                            string CollectionVersionPath = CollectionVersionFolder + "\\";
                                            cpCore.privateFiles.createPath(CollectionVersionPath);

                                            cpCore.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder);
                                            //StatusOK = True
                                            //
                                            // Install activeX and search for importcollections
                                            //
                                            //hint = hint & ",500"
                                            foreach (XmlNode CDefSection in CollectionFile.DocumentElement.ChildNodes) {
                                                switch (genericController.vbLCase(CDefSection.Name)) {
                                                    case "resource":
                                                        //
                                                        // resource node, if executable node, save to RegisterList
                                                        //
                                                        //hint = hint & ",510"
                                                        //ResourceType = genericController.vbLCase(GetXMLAttribute(cpCore, IsFound, CDefSection, "type", ""))
                                                        //Dim resourceFilename As String = Trim(GetXMLAttribute(cpCore, IsFound, CDefSection, "name", ""))
                                                        //Dim resourcePathFilename As String = CollectionVersionPath & resourceFilename
                                                        //If resourceFilename = "" Then
                                                        //    '
                                                        //    ' filename is blank
                                                        //    '
                                                        //    'hint = hint & ",511"
                                                        //ElseIf Not cpCore.privateFiles.fileExists(resourcePathFilename) Then
                                                        //    '
                                                        //    ' resource is not here
                                                        //    '
                                                        //    'hint = hint & ",513"
                                                        //    result = False
                                                        //    return_ErrorMessage = "<p>There was a problem with the Collection File. The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.</p>"
                                                        //    Call logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.")
                                                        //    'StatusOK = False
                                                        //Else
                                                        //    Select Case ResourceType
                                                        //        Case "executable"
                                                        //            '
                                                        //            ' Executable resources - add to register list
                                                        //            '
                                                        //            'hint = hint & ",520"
                                                        //            If False Then
                                                        //                '
                                                        //                ' file is already installed
                                                        //                '
                                                        //                'hint = hint & ",521"
                                                        //            Else
                                                        //                '
                                                        //                ' Add the file to be registered
                                                        //                '
                                                        //            End If
                                                        //        Case "www"
                                                        //        Case "file"
                                                        //    End Select
                                                        //End If
                                                        break;
                                                    case "interfaces":
                                                        //
                                                        // Compatibility only - this is deprecated - Install ActiveX found in Add-ons
                                                        //
                                                        //hint = hint & ",530"
                                                        //For Each CDefInterfaces In CDefSection.ChildNodes
                                                        //    AOName = GetXMLAttribute(cpCore, IsFound, CDefInterfaces, "name", "No Name")
                                                        //    If AOName = "" Then
                                                        //        AOName = "No Name"
                                                        //    End If
                                                        //    AOGuid = GetXMLAttribute(cpCore, IsFound, CDefInterfaces, "guid", AOName)
                                                        //    If AOGuid = "" Then
                                                        //        AOGuid = AOName
                                                        //    End If
                                                        //Next
                                                        break;
                                                    case "getcollection":
                                                    case "importcollection":
                                                        //
                                                        // -- Download Collection file into install folder
                                                        ChildCollectionName = GetXMLAttribute(cpCore, Found, CDefSection, "name", "");
                                                        ChildCollectionGUID = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText);
                                                        if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                            ChildCollectionGUID = CDefSection.InnerText;
                                                        }
                                                        string statusMsg = "Installing collection [" + ChildCollectionName + ", " + ChildCollectionGUID + "] referenced from collection [" + Collectionname + "]";
                                                        logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" + ChildCollectionName + "], childCollectionGuid [" + ChildCollectionGUID + "]");
                                                        if (genericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, 1) == 0) {
                                                            if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                //
                                                                // -- Needs a GUID to install
                                                                result = false;
                                                                return_ErrorMessage = statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID.";
                                                                logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, return message [" + return_ErrorMessage + "]");
                                                            } else if (GetCollectionPath(cpCore, ChildCollectionGUID) == "") {
                                                                logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], not found so needs to be installed");
                                                                //
                                                                // If it is not already installed, download and install it also
                                                                //
                                                                ChildWorkingPath = CollectionVersionPath + "\\" + ChildCollectionGUID + "\\";
                                                                //
                                                                // down an imported collection file
                                                                //
                                                                StatusOK = DownloadCollectionFiles(cpCore, ChildWorkingPath, ChildCollectionGUID, ref ChildCollectionLastChangeDate, ref return_ErrorMessage);
                                                                if (!StatusOK) {

                                                                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + statusMsg + "], downloadCollectionFiles returned error state, message [" + return_ErrorMessage + "]");
                                                                    if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" + ChildCollectionGUID + "].";
                                                                    } else {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                    }
                                                                } else {
                                                                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], downloadCollectionFiles returned OK");
                                                                    //
                                                                    // install the downloaded file
                                                                    //
                                                                    List<string> ChildCollectionGUIDList = new List<string>();
                                                                    StatusOK = BuildLocalCollectionReposFromFolder(cpCore, ChildWorkingPath, ChildCollectionLastChangeDate, ref ChildCollectionGUIDList, ref return_ErrorMessage, allowLogging);
                                                                    if (!StatusOK) {
                                                                        logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + statusMsg + "], BuildLocalCollectionFolder returned error state, message [" + return_ErrorMessage + "]");
                                                                        if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                            return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "].";
                                                                        } else {
                                                                            return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                        }
                                                                    }
                                                                }
                                                                //
                                                                // -- remove child installation working folder
                                                                cpCore.privateFiles.DeleteFileFolder(ChildWorkingPath);
                                                            } else {
                                                                //
                                                                //
                                                                //
                                                                logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], already installed");
                                                            }
                                                        }
                                                        break;
                                                }
                                                if (!string.IsNullOrEmpty(return_ErrorMessage)) {
                                                    //
                                                    // if error, no more nodes in this collection file
                                                    //
                                                    result = false;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(return_ErrorMessage)) {
                                //
                                // if error, no more files
                                //
                                result = false;
                                break;
                            }
                        }
                        if ((string.IsNullOrEmpty(return_ErrorMessage)) && (!CollectionFileFound)) {
                            //
                            // no errors, but the collection file was not found
                            //
                            if (ZipFileFound) {
                                //
                                // zip file found but did not qualify
                                //
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not include a valid collection xml file.</p>";
                            } else {
                                //
                                // zip file not found
                                //
                                return_ErrorMessage = "<p>There was a problem with the installation. The collection zip was not downloaded successfully.</p>";
                            }
                            //StatusOK = False
                        }
                    }
                    //
                    // delete the working folder
                    //
                    cpCore.privateFiles.DeleteFileFolder(tmpInstallPath);
                }
                //
                // If the collection parsed correctly, update the Collections.xml file
                //
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    UpdateConfig(cpCore, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
                } else {
                    //
                    // there was an error processing the collection, be sure to save description in the log
                    //
                    result = false;
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" + return_ErrorMessage + "]");
                }
                //
                logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, Exiting with ErrorMessage [" + return_ErrorMessage + "]");
                //
                tempBuildLocalCollectionRepoFromFile = (string.IsNullOrEmpty(return_ErrorMessage));
                return_CollectionGUID = CollectionGuid;
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }

        //
        //=========================================================================================================================
        //   Upgrade Application from a local collection
        //
        //   Either Upgrade or Install the collection in the Application. - no checks
        //
        //   ImportFromCollectionsGuidList - If this collection is from an import, this is the guid of the import.
        //=========================================================================================================================
        //
        public static bool installCollectionFromLocalRepo(coreClass cpCore, string CollectionGuid, string ignore_BuildVersion, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool result = true;
            try {
                string CollectionVersionFolderName = "";
                DateTime CollectionLastChangeDate = default(DateTime);
                string tempVar = "";
                GetCollectionConfig(cpCore, CollectionGuid, ref CollectionVersionFolderName, ref CollectionLastChangeDate, ref tempVar);
                if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                    result = false;
                    return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
                } else {
                    //
                    // Search Local Collection Folder for collection config file (xml file)
                    //
                    string CollectionVersionFolder = cpCore.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName + "\\";
                    FileInfo[] srcFileInfoArray = cpCore.privateFiles.getFileList(CollectionVersionFolder);
                    if (srcFileInfoArray.Length == 0) {
                        result = false;
                        return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>";
                    } else {
                        //
                        // collect list of DLL files and add them to the exec files if they were missed
                        List<string> assembliesInZip = new List<string>();
                        foreach (FileInfo file in srcFileInfoArray) {
                            if (file.Extension.ToLower() == "dll") {
                                if (!assembliesInZip.Contains(file.Name.ToLower())) {
                                    assembliesInZip.Add(file.Name.ToLower());
                                }
                            }
                        }
                        //
                        // -- Process the other files
                        //INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
                        bool CollectionblockNavigatorNode_fileValueOK = false;
                        foreach (FileInfo file in srcFileInfoArray) {
                            if (genericController.vbLCase(file.Name.Substring(file.Name.Length - 4)) == ".xml") {
                                //
                                // -- XML file -- open it to figure out if it is one we can use
                                XmlDocument Doc = new XmlDocument();
                                string CollectionFilename = file.Name;
                                bool loadOK = true;
                                try {
                                    Doc.Load(cpCore.privateFiles.rootLocalPath + CollectionVersionFolder + file.Name);
                                } catch (Exception ex) {
                                    //
                                    // error - Need a way to reach the user that submitted the file
                                    //
                                    logController.appendInstallLog(cpCore, "There was an error reading the Meta data file [" + cpCore.privateFiles.rootLocalPath + CollectionVersionFolder + file.Name + "].");
                                    result = false;
                                    return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error</P>";
                                    loadOK = false;
                                }
                                if (loadOK) {
                                    if ((Doc.DocumentElement.Name.ToLower() == genericController.vbLCase(CollectionFileRootNode)) || (Doc.DocumentElement.Name.ToLower() == genericController.vbLCase(CollectionFileRootNodeOld))) {
                                        //
                                        //------------------------------------------------------------------------------------------------------
                                        // Collection File - import from sub so it can be re-entrant
                                        //------------------------------------------------------------------------------------------------------
                                        //
                                        bool IsFound = false;
                                        string Collectionname = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(Collectionname)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            //Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
                                            logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, collection has no name");
                                            result = false;
                                            return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the collection name in the xml collection file is blank</P>";
                                        } else {
                                            bool CollectionSystem_fileValueOK = false;
                                            bool CollectionUpdatable_fileValueOK = false;
                                            //												Dim CollectionblockNavigatorNode_fileValueOK As Boolean
                                            bool CollectionSystem = genericController.encodeBoolean(GetXMLAttribute(cpCore, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""));
                                            int Parent_NavID = appBuilderController.verifyNavigatorEntry(cpCore, addonGuidManageAddon, "", "Manage Add-ons", "", "", "", false, false, false, true, "", "", "", 0);
                                            bool CollectionUpdatable = genericController.encodeBoolean(GetXMLAttribute(cpCore, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""));
                                            bool CollectionblockNavigatorNode = genericController.encodeBoolean(GetXMLAttribute(cpCore, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""));
                                            string FileGuid = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "guid", Collectionname);
                                            if (string.IsNullOrEmpty(FileGuid)) {
                                                FileGuid = Collectionname;
                                            }
                                            if (CollectionGuid.ToLower() != genericController.vbLCase(FileGuid)) {
                                                //
                                                //
                                                //
                                                result = false;
                                                logController.appendInstallLog(cpCore, "Local Collection file contains a different GUID for [" + Collectionname + "] then Collections.xml");
                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>";
                                            } else {
                                                if (string.IsNullOrEmpty(CollectionGuid)) {
                                                    //
                                                    // I hope I do not regret this
                                                    //
                                                    CollectionGuid = Collectionname;
                                                }
                                                //
                                                //-------------------------------------------------------------------------------
                                                // ----- Pass 1
                                                // Go through all collection nodes
                                                // Process ImportCollection Nodes - so includeaddon nodes will work
                                                // these must be processes regardless of the state of this collection in this app
                                                // Get Resource file list
                                                //-------------------------------------------------------------------------------
                                                //
                                                //CollectionAddOnCnt = 0
                                                logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1");
                                                string wwwFileList = "";
                                                string ContentFileList = "";
                                                string ExecFileList = "";
                                                foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                    switch (CDefSection.Name.ToLower()) {
                                                        case "resource":
                                                            //
                                                            // set wwwfilelist, contentfilelist, execfilelist
                                                            //
                                                            string resourceType = GetXMLAttribute(cpCore, IsFound, CDefSection, "type", "");
                                                            string resourcePath = GetXMLAttribute(cpCore, IsFound, CDefSection, "path", "");
                                                            string filename = GetXMLAttribute(cpCore, IsFound, CDefSection, "name", "");
                                                            //
                                                            logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1, resource found, name [" + filename + "], type [" + resourceType + "], path [" + resourcePath + "]");
                                                            //
                                                            filename = genericController.convertToDosSlash(filename);
                                                            string SrcPath = "";
                                                            string DstPath = resourcePath;
                                                            int Pos = genericController.vbInstr(1, filename, "\\");
                                                            if (Pos != 0) {
                                                                //
                                                                // Source path is in filename
                                                                //
                                                                SrcPath = filename.Left( Pos - 1);
                                                                filename = filename.Substring(Pos);
                                                                if (string.IsNullOrEmpty(resourcePath)) {
                                                                    //
                                                                    // No Resource Path give, use the same folder structure from source
                                                                    //
                                                                    DstPath = SrcPath;
                                                                } else {
                                                                    //
                                                                    // Copy file to resource path
                                                                    //
                                                                    DstPath = resourcePath;
                                                                }
                                                            }

                                                            string DstFilePath = genericController.vbReplace(DstPath, "/", "\\");
                                                            if (DstFilePath == "\\") {
                                                                DstFilePath = "";
                                                            }
                                                            if (!string.IsNullOrEmpty(DstFilePath)) {
                                                                if (DstFilePath.Left( 1) == "\\") {
                                                                    DstFilePath = DstFilePath.Substring(1);
                                                                }
                                                                if (DstFilePath.Substring(DstFilePath.Length - 1) != "\\") {
                                                                    DstFilePath = DstFilePath + "\\";
                                                                }
                                                            }

                                                            switch (resourceType.ToLower()) {
                                                                case "www":
                                                                    wwwFileList = wwwFileList + "\r\n" + DstFilePath + filename;
                                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to www, src [" + CollectionVersionFolder + SrcPath + "], dst [" + cpCore.serverConfig.appConfig.appRootFilesPath + DstFilePath + "].");
                                                                    //Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, copying file to www, src [" & CollectionVersionFolder & SrcPath & "], dst [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & "].")
                                                                    cpCore.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, DstFilePath + filename, cpCore.appRootFiles);
                                                                    if (genericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                        logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping www file [" + cpCore.serverConfig.appConfig.appRootFilesPath + DstFilePath + filename + "].");
                                                                        //Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, unzipping www file [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & Filename & "].")
                                                                        cpCore.appRootFiles.UnzipFile(DstFilePath + filename);
                                                                    }
                                                                    break;
                                                                case "file":
                                                                case "content":
                                                                    ContentFileList = ContentFileList + "\r\n" + DstFilePath + filename;
                                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to content, src [" + CollectionVersionFolder + SrcPath + "], dst [" + DstFilePath + "].");
                                                                    cpCore.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, DstFilePath + filename, cpCore.cdnFiles);
                                                                    if (genericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                        logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping content file [" + DstFilePath + filename + "].");
                                                                        cpCore.cdnFiles.UnzipFile(DstFilePath + filename);
                                                                    }
                                                                    break;
                                                                default:
                                                                    if (assembliesInZip.Contains(filename.ToLower())) {
                                                                        assembliesInZip.Remove(filename.ToLower());
                                                                    }
                                                                    ExecFileList = ExecFileList + "\r\n" + filename;
                                                                    break;
                                                            }
                                                            break;
                                                        case "getcollection":
                                                        case "importcollection":
                                                            //
                                                            // Get path to this collection and call into it
                                                            //
                                                            bool Found = false;
                                                            string ChildCollectionName = GetXMLAttribute(cpCore, Found, CDefSection, "name", "");
                                                            string ChildCollectionGUID = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText);
                                                            if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                ChildCollectionGUID = CDefSection.InnerText;
                                                            }
                                                            if ((ImportFromCollectionsGuidList + "," + CollectionGuid).IndexOf(ChildCollectionGUID, System.StringComparison.OrdinalIgnoreCase)  != -1) {
                                                                //
                                                                // circular import detected, this collection is already imported
                                                                //
                                                                logController.appendInstallLog(cpCore, "Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1. The collection to be imported is [" + ChildCollectionName + "], GUID [" + ChildCollectionGUID + "]");
                                                            } else {
                                                                logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1, import collection found, name [" + ChildCollectionName + "], guid [" + ChildCollectionGUID + "]");
                                                                installCollectionFromRemoteRepo(cpCore, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, ref nonCriticalErrorList);
                                                                //if (true) {
                                                                //    installCollectionFromRemoteRepo(cpCore, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, ref nonCriticalErrorList);
                                                                //} else {
                                                                //    if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                //        logController.appendInstallLog(cpCore, "The importcollection node [" + ChildCollectionName + "] can not be upgraded because it does not include a valid guid.");
                                                                //    } else {
                                                                //        //
                                                                //        // This import occurred while upgrading an application from the local collections (Db upgrade or AddonManager)
                                                                //        // Its OK to install it if it is missing, but you do not need to upgrade the local collections from the Library
                                                                //        //
                                                                //        // 5/18/2008 -----------------------------------
                                                                //        // See if it is in the local collections storage. If yes, just upgrade this app with it. If not,
                                                                //        // it must be downloaded and the entire server must be upgraded
                                                                //        //
                                                                //        string ChildCollectionVersionFolderName = "";
                                                                //        DateTime ChildCollectionLastChangeDate = default(DateTime);
                                                                //        string tempVar2 = "";
                                                                //        GetCollectionConfig(cpCore, ChildCollectionGUID, ref ChildCollectionVersionFolderName, ref ChildCollectionLastChangeDate, ref tempVar2);
                                                                //        if (!string.IsNullOrEmpty(ChildCollectionVersionFolderName)) {
                                                                //            //
                                                                //            // It is installed in the local collections, update just this site
                                                                //            //
                                                                //            result &= installCollectionFromLocalRepo(cpCore, ChildCollectionGUID, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList + "," + CollectionGuid, IsNewBuild, ref nonCriticalErrorList);
                                                                //        }
                                                                //    }
                                                                //}
                                                            }
                                                            break;
                                                    }
                                                }
                                                //
                                                // -- any assemblies found in the zip that were not part of the resources section need to be added
                                                foreach (string filename in assembliesInZip) {
                                                    ExecFileList = ExecFileList + "\r\n" + filename;
                                                }
                                                //
                                                //-------------------------------------------------------------------------------
                                                // create an Add-on Collection record
                                                //-------------------------------------------------------------------------------
                                                //
                                                bool OKToInstall = false;
                                                logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1 done, create collection record.");
                                                AddonCollectionModel collection = AddonCollectionModel.create(cpCore, CollectionGuid);
                                                if (collection != null) {
                                                    //
                                                    // Upgrade addon
                                                    //
                                                    if (CollectionLastChangeDate == DateTime.MinValue) {
                                                        logController.appendInstallLog(cpCore, "collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has the collection, but the new version has no lastchangedate, so it will upgrade to this unknown (manual) version.");
                                                        OKToInstall = true;
                                                    } else if (collection.LastChangeDate < CollectionLastChangeDate) {
                                                        logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an older version of collection. It will be upgraded.");
                                                        OKToInstall = true;
                                                    } else {
                                                        logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.");
                                                        OKToInstall = false;
                                                    }
                                                } else {
                                                    //
                                                    // Install new on this application
                                                    //
                                                    collection = AddonCollectionModel.add(cpCore);
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App does not have this collection so it will be installed.");
                                                    OKToInstall = true;
                                                }
                                                string DataRecordList = "";
                                                if (!OKToInstall) {
                                                    //
                                                    // Do not install, but still check all imported collections to see if they need to be installed
                                                    // imported collections moved in front this check
                                                    //
                                                } else {
                                                    //
                                                    // ----- gather help nodes
                                                    //
                                                    string CollectionHelpLink = "";
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        if (CDefSection.Name.ToLower() == "helplink") {
                                                            //
                                                            // only save the first
                                                            CollectionHelpLink = CDefSection.InnerText;
                                                            break;
                                                        }
                                                    }
                                                    //
                                                    // ----- set or clear all fields
                                                    collection.name = Collectionname;
                                                    collection.Help = "";
                                                    collection.ccguid = CollectionGuid;
                                                    collection.LastChangeDate = CollectionLastChangeDate;
                                                    if (CollectionSystem_fileValueOK) {
                                                        collection.System = CollectionSystem;
                                                    }
                                                    if (CollectionUpdatable_fileValueOK) {
                                                        collection.Updatable = CollectionUpdatable;
                                                    }
                                                    if (CollectionblockNavigatorNode_fileValueOK) {
                                                        collection.blockNavigatorNode = CollectionblockNavigatorNode;
                                                    }
                                                    collection.helpLink = CollectionHelpLink;
                                                    //
                                                    cpCore.db.deleteContentRecords("Add-on Collection CDef Rules", "CollectionID=" + collection.id);
                                                    cpCore.db.deleteContentRecords("Add-on Collection Parent Rules", "ParentID=" + collection.id);
                                                    //
                                                    // Store all resource found, new way and compatibility way
                                                    //
                                                    collection.ContentFileList = ContentFileList;
                                                    collection.ExecFileList = ExecFileList;
                                                    collection.wwwFileList = wwwFileList;
                                                    //
                                                    // ----- remove any current navigator nodes installed by the collection previously
                                                    //
                                                    if (collection.id != 0) {
                                                        cpCore.db.deleteContentRecords(cnNavigatorEntries, "installedbycollectionid=" + collection.id);
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Pass 2
                                                    // Go through all collection nodes
                                                    // Process all cdef related nodes to the old upgrade
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    string CollectionWrapper = "";
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 2");
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (genericController.vbLCase(CDefSection.Name)) {
                                                            case "contensivecdef":
                                                                //
                                                                // old cdef xection -- take the inner
                                                                //
                                                                foreach (XmlNode ChildNode in CDefSection.ChildNodes) {
                                                                    CollectionWrapper += "\r\n" + ChildNode.OuterXml;
                                                                }
                                                                break;
                                                            case "cdef":
                                                            case "sqlindex":
                                                            case "style":
                                                            case "styles":
                                                            case "stylesheet":
                                                            case "adminmenu":
                                                            case "menuentry":
                                                            case "navigatorentry":
                                                                //
                                                                // handled by Upgrade class
                                                                CollectionWrapper += CDefSection.OuterXml;
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Post Pass 2
                                                    // if cdef were found, import them now
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    if (!string.IsNullOrEmpty(CollectionWrapper)) {
                                                        //
                                                        // -- Use the upgrade code to import this part
                                                        CollectionWrapper = "<" + CollectionFileRootNode + ">" + CollectionWrapper + "</" + CollectionFileRootNode + ">";
                                                        bool isBaseCollection = (baseCollectionGuid == CollectionGuid);
                                                        installCollectionFromLocalRepo_BuildDbFromXmlData(cpCore, CollectionWrapper, IsNewBuild, isBaseCollection, ref nonCriticalErrorList);
                                                        //
                                                        // -- Process nodes to save Collection data
                                                        XmlDocument NavDoc = new XmlDocument();
                                                        loadOK = true;
                                                        try {
                                                            NavDoc.LoadXml(CollectionWrapper);
                                                        } catch (Exception ex) {
                                                            //
                                                            // error - Need a way to reach the user that submitted the file
                                                            //
                                                            logController.appendInstallLog(cpCore, "Creating navigator entries, there was an error parsing the portion of the collection that contains cdef. Navigator entry creation was aborted. [There was an error reading the Meta data file.]");
                                                            result = false;
                                                            return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error.</P>";
                                                            loadOK = false;
                                                        }
                                                        if (loadOK) {
                                                            foreach (XmlNode CDefNode in NavDoc.DocumentElement.ChildNodes) {
                                                                switch (genericController.vbLCase(CDefNode.Name)) {
                                                                    case "cdef":
                                                                        string ContentName = GetXMLAttribute(cpCore, IsFound, CDefNode, "name", "");
                                                                        //
                                                                        // setup cdef rule
                                                                        //
                                                                        int ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
                                                                        if (ContentID > 0) {
                                                                            int CS = cpCore.db.csInsertRecord("Add-on Collection CDef Rules", 0);
                                                                            if (cpCore.db.csOk(CS)) {
                                                                                cpCore.db.csSet(CS, "Contentid", ContentID);
                                                                                cpCore.db.csSet(CS, "CollectionID", collection.id);
                                                                            }
                                                                            cpCore.db.csClose(ref CS);
                                                                        }
                                                                        break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Pass3
                                                    // create any data records
                                                    //
                                                    //   process after cdef builds
                                                    //   process seperate so another pass can create any lookup data from these records
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 3");
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (genericController.vbLCase(CDefSection.Name)) {
                                                            case "data":
                                                                //
                                                                // import content
                                                                //   This can only be done with matching guid
                                                                //
                                                                foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                    if (genericController.vbLCase(ContentNode.Name) == "record") {
                                                                        //
                                                                        // Data.Record node
                                                                        //
                                                                        string ContentName = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "");
                                                                        if (string.IsNullOrEmpty(ContentName)) {
                                                                            logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.");
                                                                            result = false;
                                                                            return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                        } else {
                                                                            string ContentRecordGuid = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "");
                                                                            string ContentRecordName = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "");
                                                                            if ((string.IsNullOrEmpty(ContentRecordGuid)) && (string.IsNullOrEmpty(ContentRecordName))) {
                                                                                logController.appendInstallLog(cpCore, "install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" + ContentName + "]");
                                                                                result = false;
                                                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" + ContentName + "].</P>";
                                                                            } else {
                                                                                //
                                                                                // create or update the record
                                                                                //
                                                                                Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                                                                                int cs = -1;
                                                                                if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                    cs = cpCore.db.csOpen(ContentName, "ccguid=" + cpCore.db.encodeSQLText(ContentRecordGuid));
                                                                                } else {
                                                                                    cs = cpCore.db.csOpen(ContentName, "name=" + cpCore.db.encodeSQLText(ContentRecordName));
                                                                                }
                                                                                bool recordfound = true;
                                                                                if (!cpCore.db.csOk(cs)) {
                                                                                    //
                                                                                    // Insert the new record
                                                                                    //
                                                                                    recordfound = false;
                                                                                    cpCore.db.csClose(ref cs);
                                                                                    cs = cpCore.db.csInsertRecord(ContentName, 0);
                                                                                }
                                                                                if (cpCore.db.csOk(cs)) {
                                                                                    //
                                                                                    // Update the record
                                                                                    //
                                                                                    if (recordfound && (!string.IsNullOrEmpty(ContentRecordGuid))) {
                                                                                        //
                                                                                        // found by guid, use guid in list and save name
                                                                                        //
                                                                                        cpCore.db.csSet(cs, "name", ContentRecordName);
                                                                                        DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordGuid;
                                                                                    } else if (recordfound) {
                                                                                        //
                                                                                        // record found by name, use name is list but do not add guid
                                                                                        //
                                                                                        DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordName;
                                                                                    } else {
                                                                                        //
                                                                                        // record was created
                                                                                        //
                                                                                        cpCore.db.csSet(cs, "ccguid", ContentRecordGuid);
                                                                                        cpCore.db.csSet(cs, "name", ContentRecordName);
                                                                                        DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordGuid;
                                                                                    }
                                                                                }
                                                                                cpCore.db.csClose(ref cs);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Pass 4, process fields in data nodes
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 4");
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (genericController.vbLCase(CDefSection.Name)) {
                                                            case "data":
                                                                //
                                                                // import content
                                                                //   This can only be done with matching guid
                                                                //
                                                                //OtherXML = OtherXML & vbCrLf & CDefSection.xml
                                                                //
                                                                foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                    if (genericController.vbLCase(ContentNode.Name) == "record") {
                                                                        //
                                                                        // Data.Record node
                                                                        //
                                                                        string ContentName = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "");
                                                                        if (string.IsNullOrEmpty(ContentName)) {
                                                                            logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.");
                                                                            result = false;
                                                                            return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                        } else {
                                                                            string ContentRecordGuid = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "");
                                                                            string ContentRecordName = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "");
                                                                            if ((!string.IsNullOrEmpty(ContentRecordGuid)) | (!string.IsNullOrEmpty(ContentRecordName))) {
                                                                                Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
                                                                                int cs = -1;
                                                                                if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                    cs = cpCore.db.csOpen(ContentName, "ccguid=" + cpCore.db.encodeSQLText(ContentRecordGuid));
                                                                                } else {
                                                                                    cs = cpCore.db.csOpen(ContentName, "name=" + cpCore.db.encodeSQLText(ContentRecordName));
                                                                                }
                                                                                if (cpCore.db.csOk(cs)) {
                                                                                    //
                                                                                    // Update the record
                                                                                    foreach (XmlNode FieldNode in ContentNode.ChildNodes) {
                                                                                        if (FieldNode.Name.ToLower() == "field") {
                                                                                            bool IsFieldFound = false;
                                                                                            string FieldName = GetXMLAttribute(cpCore, IsFound, FieldNode, "name", "").ToLower();
                                                                                            int fieldTypeId = -1;
                                                                                            int FieldLookupContentID = -1;
                                                                                            foreach (var keyValuePair in CDef.fields) {
                                                                                                Models.Complex.CDefFieldModel field = keyValuePair.Value;
                                                                                                if (genericController.vbLCase(field.nameLc) == FieldName) {
                                                                                                    fieldTypeId = field.fieldTypeId;
                                                                                                    FieldLookupContentID = field.lookupContentID;
                                                                                                    IsFieldFound = true;
                                                                                                    break;
                                                                                                }
                                                                                            }
                                                                                            //For Ptr = 0 To CDef.fields.count - 1
                                                                                            //    CDefField = CDef.fields[Ptr]
                                                                                            //    If genericController.vbLCase(CDefField.Name) = FieldName Then
                                                                                            //        fieldType = CDefField.fieldType
                                                                                            //        FieldLookupContentID = CDefField.LookupContentID
                                                                                            //        IsFieldFound = True
                                                                                            //        Exit For
                                                                                            //    End If
                                                                                            //Next
                                                                                            if (IsFieldFound) {
                                                                                                string FieldValue = FieldNode.InnerText;
                                                                                                switch (fieldTypeId) {
                                                                                                    case FieldTypeIdAutoIdIncrement:
                                                                                                    case FieldTypeIdRedirect: {
                                                                                                            //
                                                                                                            // not supported
                                                                                                            //
                                                                                                            break;
                                                                                                        }
                                                                                                    case FieldTypeIdLookup: {
                                                                                                            //
                                                                                                            // read in text value, if a guid, use it, otherwise assume name
                                                                                                            //
                                                                                                            if (FieldLookupContentID != 0) {
                                                                                                                string FieldLookupContentName = Models.Complex.cdefModel.getContentNameByID(cpCore, FieldLookupContentID);
                                                                                                                if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                                                                                                    if ((FieldValue.Left( 1) == "{") && (FieldValue.Substring(FieldValue.Length - 1) == "}") && Models.Complex.cdefModel.isContentFieldSupported(cpCore, FieldLookupContentName, "ccguid")) {
                                                                                                                        //
                                                                                                                        // Lookup by guid
                                                                                                                        //
                                                                                                                        int fieldLookupId = genericController.EncodeInteger(cpCore.db.GetRecordIDByGuid(FieldLookupContentName, FieldValue));
                                                                                                                        if (fieldLookupId <= 0) {
                                                                                                                            return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                        } else {
                                                                                                                            cpCore.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                        }
                                                                                                                    } else {
                                                                                                                        //
                                                                                                                        // lookup by name
                                                                                                                        //
                                                                                                                        if (!string.IsNullOrEmpty(FieldValue)) {
                                                                                                                            int fieldLookupId = cpCore.db.getRecordID(FieldLookupContentName, FieldValue);
                                                                                                                            if (fieldLookupId <= 0) {
                                                                                                                                return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                            } else {
                                                                                                                                cpCore.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                }
                                                                                                            } else if (FieldValue.IsNumeric()) {
                                                                                                                //
                                                                                                                // must be lookup list
                                                                                                                //
                                                                                                                cpCore.db.csSet(cs, FieldName, FieldValue);
                                                                                                            }
                                                                                                            break;
                                                                                                        }
                                                                                                    default: {
                                                                                                            cpCore.db.csSet(cs, FieldName, FieldValue);
                                                                                                            break;
                                                                                                        }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                                cpCore.db.csClose(ref cs);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    // --- end of pass
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Pass 5, all other collection nodes
                                                    //
                                                    // Process all non-import <Collection> nodes
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 5");
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (genericController.vbLCase(CDefSection.Name)) {
                                                            case "cdef":
                                                            case "data":
                                                            case "help":
                                                            case "resource":
                                                            case "helplink":
                                                                //
                                                                // ignore - processed in previous passes
                                                                break;
                                                            case "getcollection":
                                                            case "importcollection":
                                                                //
                                                                // processed, but add rule for collection record
                                                                bool Found = false;
                                                                string ChildCollectionName = GetXMLAttribute(cpCore, Found, CDefSection, "name", "");
                                                                string ChildCollectionGUID = GetXMLAttribute(cpCore, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    ChildCollectionGUID = CDefSection.InnerText;
                                                                }
                                                                if (!string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    int ChildCollectionID = 0;
                                                                    int cs = -1;
                                                                    cs = cpCore.db.csOpen("Add-on Collections", "ccguid=" + cpCore.db.encodeSQLText(ChildCollectionGUID));
                                                                    if (cpCore.db.csOk(cs)) {
                                                                        ChildCollectionID = cpCore.db.csGetInteger(cs, "id");
                                                                    }
                                                                    cpCore.db.csClose(ref cs);
                                                                    if (ChildCollectionID != 0) {
                                                                        cs = cpCore.db.csInsertRecord("Add-on Collection Parent Rules", 0);
                                                                        if (cpCore.db.csOk(cs)) {
                                                                            cpCore.db.csSet(cs, "ParentID", collection.id);
                                                                            cpCore.db.csSet(cs, "ChildID", ChildCollectionID);
                                                                        }
                                                                        cpCore.db.csClose(ref cs);
                                                                    }
                                                                }
                                                                break;
                                                            case "scriptingmodule":
                                                            case "scriptingmodules":
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection includes a scripting module which is no longer supported. Move scripts to the code tab.</P>";
                                                                //    '
                                                                //    ' Scripting modules
                                                                //    '
                                                                //    ScriptingModuleID = 0
                                                                //    ScriptingName = GetXMLAttribute(cpcore,IsFound, CDefSection, "name", "No Name")
                                                                //    If ScriptingName = "" Then
                                                                //        ScriptingName = "No Name"
                                                                //    End If
                                                                //    ScriptingGuid = GetXMLAttribute(cpcore,IsFound, CDefSection, "guid", AOName)
                                                                //    If ScriptingGuid = "" Then
                                                                //        ScriptingGuid = ScriptingName
                                                                //    End If
                                                                //    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(ScriptingGuid) & ")"
                                                                //    ScriptingModuleID = 0
                                                                //    CS = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                                //    If cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Update the Addon
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing scripting module, Updating module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //    Else
                                                                //        '
                                                                //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                //        '
                                                                //        Call cpCore.db.cs_Close(CS)
                                                                //        Criteria = "(name=" & cpCore.db.encodeSQLText(ScriptingName) & ")and(ccguid is null)"
                                                                //        CS = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                                //        If cpCore.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Scripting Module matched an existing Module that has no GUID, Updating to [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' not found by GUID or by name, Insert a new
                                                                //        '
                                                                //        Call cpCore.db.cs_Close(CS)
                                                                //        CS = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                                //        If cpCore.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Could not create new
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Scripting Module could not be created, skipping Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                //    Else
                                                                //        ScriptingModuleID = cpCore.db.cs_getInteger(CS, "ID")
                                                                //        Call cpCore.db.cs_set(CS, "code", CDefSection.InnerText)
                                                                //        Call cpCore.db.cs_set(CS, "name", ScriptingName)
                                                                //        Call cpCore.db.cs_set(CS, "ccguid", ScriptingGuid)
                                                                //    End If
                                                                //    Call cpCore.db.cs_Close(CS)
                                                                //    If ScriptingModuleID <> 0 Then
                                                                //        '
                                                                //        ' Add Add-on Collection Module Rule
                                                                //        '
                                                                //        CS = cpCore.db.cs_insertRecord("Add-on Collection Module Rules", 0)
                                                                //        If cpCore.db.cs_ok(CS) Then
                                                                //            Call cpCore.db.cs_set(CS, "Collectionid", CollectionID)
                                                                //            Call cpCore.db.cs_set(CS, "ScriptingModuleID", ScriptingModuleID)
                                                                //        End If
                                                                //        Call cpCore.db.cs_Close(CS)
                                                                //    End If
                                                                break;
                                                            case "sharedstyle":
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection includes a shared style which is no longer supported. Move styles to the default styles tab.</P>";

                                                                //    '
                                                                //    ' added 9/3/2012
                                                                //    ' Shared Style
                                                                //    '
                                                                //    sharedStyleId = 0
                                                                //    NodeName = GetXMLAttribute(cpcore,IsFound, CDefSection, "name", "No Name")
                                                                //    If NodeName = "" Then
                                                                //        NodeName = "No Name"
                                                                //    End If
                                                                //    nodeGuid = GetXMLAttribute(cpcore,IsFound, CDefSection, "guid", AOName)
                                                                //    If nodeGuid = "" Then
                                                                //        nodeGuid = NodeName
                                                                //    End If
                                                                //    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(nodeGuid) & ")"
                                                                //    ScriptingModuleID = 0
                                                                //    CS = cpCore.db.cs_open("Shared Styles", Criteria)
                                                                //    If cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Update the Addon
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing shared style, Updating [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //    Else
                                                                //        '
                                                                //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                //        '
                                                                //        Call cpCore.db.cs_Close(CS)
                                                                //        Criteria = "(name=" & cpCore.db.encodeSQLText(NodeName) & ")and(ccguid is null)"
                                                                //        CS = cpCore.db.cs_open("shared styles", Criteria)
                                                                //        If cpCore.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, shared style matched an existing Module that has no GUID, Updating to [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' not found by GUID or by name, Insert a new
                                                                //        '
                                                                //        Call cpCore.db.cs_Close(CS)
                                                                //        CS = cpCore.db.cs_insertRecord("shared styles", 0)
                                                                //        If cpCore.db.cs_ok(CS) Then
                                                                //            Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //        End If
                                                                //    End If
                                                                //    If Not cpCore.db.cs_ok(CS) Then
                                                                //        '
                                                                //        ' Could not create new
                                                                //        '
                                                                //        Call logcontroller.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, shared style could not be created, skipping shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                //    Else
                                                                //        sharedStyleId = cpCore.db.cs_getInteger(CS, "ID")
                                                                //        Call cpCore.db.cs_set(CS, "StyleFilename", CDefSection.InnerText)
                                                                //        Call cpCore.db.cs_set(CS, "name", NodeName)
                                                                //        Call cpCore.db.cs_set(CS, "ccguid", nodeGuid)
                                                                //        Call cpCore.db.cs_set(CS, "alwaysInclude", GetXMLAttribute(cpcore,IsFound, CDefSection, "alwaysinclude", "0"))
                                                                //        Call cpCore.db.cs_set(CS, "prefix", GetXMLAttribute(cpcore,IsFound, CDefSection, "prefix", ""))
                                                                //        Call cpCore.db.cs_set(CS, "suffix", GetXMLAttribute(cpcore,IsFound, CDefSection, "suffix", ""))
                                                                //        Call cpCore.db.cs_set(CS, "suffix", GetXMLAttribute(cpcore,IsFound, CDefSection, "suffix", ""))
                                                                //        Call cpCore.db.cs_set(CS, "sortOrder", GetXMLAttribute(cpcore,IsFound, CDefSection, "sortOrder", ""))
                                                                //    End If
                                                                //    Call cpCore.db.cs_Close(CS)
                                                                break;
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1 of 2
                                                                //   (include add-on node must be done after all add-ons are installed)
                                                                //
                                                                InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore, CDefSection, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                if (!result) {
                                                                    //result = result;
                                                                }
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                                                    InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) {
                                                                        //result = result;
                                                                    }
                                                                }
                                                                //Case "otherxml", "importcollection", "sqlindex", "style", "styles", "stylesheet", "adminmenu", "menuentry", "navigatorentry"
                                                                //    '
                                                                //    ' otherxml
                                                                //    '
                                                                //    If genericController.vbLCase(CDefSection.OuterXml) <> "<otherxml></otherxml>" Then
                                                                //        OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                //    End If
                                                                //    'Case Else
                                                                //    '    '
                                                                //    '    ' Unknown node in collection file
                                                                //    '    '
                                                                //    '    OtherXML = OtherXML & vbCrLf & CDefSection.OuterXml
                                                                //    '    Call logcontroller.appendInstallLog(cpCore, "Addon Collection for [" & Collectionname & "] contained an unknown node [" & CDefSection.Name & "]. This node will be ignored.")
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    // --- end of pass
                                                    //
                                                    //-------------------------------------------------------------------------------
                                                    // ----- Pass 6
                                                    //
                                                    // process include add-on node of add-on nodes
                                                    //-------------------------------------------------------------------------------
                                                    //
                                                    logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 6");
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (genericController.vbLCase(CDefSection.Name)) {
                                                            case "addon":
                                                            case "add-on":
                                                                //
                                                                // Add-on Node, do part 1 of 2
                                                                //   (include add-on node must be done after all add-ons are installed)
                                                                //
                                                                InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefSection, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                if (!result) {
                                                                    //result = result;
                                                                }
                                                                break;
                                                            case "interfaces":
                                                                //
                                                                // Legacy Interface Node
                                                                //
                                                                foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                                                    InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) {
                                                                        //result = result;
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    //
                                                    // --- end of pass
                                                    //
                                                }
                                                collection.DataRecordList = DataRecordList;
                                                collection.save(cpCore);
                                            }
                                            //
                                            logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], upgrade complete, flush cache");
                                            //
                                            // -- import complete, flush caches
                                            cpCore.cache.invalidateAll();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                //
                // Log error and exit with failure. This way any other upgrading will still continue
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collectionList file stored in the root of the addon folder.
        /// </summary>
        /// <returns></returns>
        public static string getCollectionListFile(coreClass cpCore) {
            string returnXml = "";
            try {
                string LastChangeDate = "";
                DirectoryInfo SubFolder = null;
                DirectoryInfo[] SubFolderList = null;
                string FolderName = null;
                string collectionFilePathFilename = null;
                string CollectionGuid = null;
                string Collectionname = null;
                int Pos = 0;
                DirectoryInfo[] FolderList = null;
                //
                collectionFilePathFilename = cpCore.addon.getPrivateFilesAddonPath() + "Collections.xml";
                returnXml = cpCore.privateFiles.readFile(collectionFilePathFilename);
                if (string.IsNullOrEmpty(returnXml)) {
                    FolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath());
                    if (FolderList.Length > 0) {
                        foreach (DirectoryInfo folder in FolderList) {
                            FolderName = folder.Name;
                            Pos = genericController.vbInstr(1, FolderName, "\t");
                            if (Pos > 1) {
                                //hint = hint & ",800"
                                FolderName = FolderName.Left( Pos - 1);
                                if (FolderName.Length > 34) {
                                    if (genericController.vbLCase(FolderName.Left( 4)) != "temp") {
                                        CollectionGuid = FolderName.Substring(FolderName.Length - 32);
                                        Collectionname = FolderName.Left( FolderName.Length - CollectionGuid.Length - 1);
                                        CollectionGuid = CollectionGuid.Left( 8) + "-" + CollectionGuid.Substring(8, 4) + "-" + CollectionGuid.Substring(12, 4) + "-" + CollectionGuid.Substring(16, 4) + "-" + CollectionGuid.Substring(20);
                                        CollectionGuid = "{" + CollectionGuid + "}";
                                        SubFolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath() + "\\" + FolderName);
                                        if (SubFolderList.Length > 0) {
                                            SubFolder = SubFolderList[SubFolderList.Length - 1];
                                            FolderName = FolderName + "\\" + SubFolder.Name;
                                            LastChangeDate = SubFolder.Name.Substring(4, 2) + "/" + SubFolder.Name.Substring(6, 2) + "/" + SubFolder.Name.Left( 4);
                                            if (!dateController.IsDate(LastChangeDate)) {
                                                LastChangeDate = "";
                                            }
                                        }
                                        returnXml = returnXml + "\r\n\t<Collection>";
                                        returnXml = returnXml + "\r\n\t\t<name>" + Collectionname + "</name>";
                                        returnXml = returnXml + "\r\n\t\t<guid>" + CollectionGuid + "</guid>";
                                        returnXml = returnXml + "\r\n\t\t<lastchangedate>" + LastChangeDate + "</lastchangedate>";
                                        returnXml += "\r\n\t\t<path>" + FolderName + "</path>";
                                        returnXml = returnXml + "\r\n\t</Collection>";
                                    }
                                }
                            }
                        }
                    }
                    returnXml = "<CollectionList>" + returnXml + "\r\n</CollectionList>";
                    cpCore.privateFiles.saveFile(collectionFilePathFilename, returnXml);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnXml;
        }
        //
        //
        //
        private static void UpdateConfig(coreClass cpCore, string Collectionname, string CollectionGuid, DateTime CollectionUpdatedDate, string CollectionVersionFolderName) {
            try {
                //
                bool loadOK = true;
                string LocalFilename = null;
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CollectionNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode LocalListNode = null;
                XmlNode NewCollectionNode = null;
                XmlNode NewAttrNode = null;
                bool CollectionFound = false;
                //
                loadOK = true;
                try {
                    Doc.LoadXml(getCollectionListFile(cpCore));
                } catch (Exception ex) {
                    logController.appendInstallLog(cpCore, "UpdateConfig, Error loading Collections.xml file.");
                }
                if (loadOK) {
                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                        logController.appendInstallLog(cpCore, "UpdateConfig, The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        if (genericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                            CollectionFound = false;
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                switch (genericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (genericController.vbLCase(CollectionNode.Name)) {
                                                case "guid":
                                                    //
                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                    //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                                    //ORIGINAL LINE: Exit For
                                                    goto ExitLabel1;
                                            }
                                        }
                                        ExitLabel1:
                                        if (genericController.vbLCase(LocalGuid) == genericController.vbLCase(CollectionGuid)) {
                                            CollectionFound = true;
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                switch (genericController.vbLCase(CollectionNode.Name)) {
                                                    case "name":
                                                        CollectionNode.InnerText = Collectionname;
                                                        break;
                                                    case "lastchangedate":
                                                        CollectionNode.InnerText = CollectionUpdatedDate.ToString();
                                                        break;
                                                    case "path":
                                                        CollectionNode.InnerText = CollectionVersionFolderName;
                                                        break;
                                                }
                                            }
                                            //INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
                                            //ORIGINAL LINE: Exit For
                                            goto ExitLabel2;
                                        }
                                        break;
                                }
                            }
                            ExitLabel2:
                            if (!CollectionFound) {
                                NewCollectionNode = Doc.CreateNode(XmlNodeType.Element, "collection", "");
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "name", "");
                                NewAttrNode.InnerText = Collectionname;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "lastchangedate", "");
                                NewAttrNode.InnerText = CollectionUpdatedDate.ToString();
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "guid", "");
                                NewAttrNode.InnerText = CollectionGuid;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                NewAttrNode = Doc.CreateNode(XmlNodeType.Element, "path", "");
                                NewAttrNode.InnerText = CollectionVersionFolderName;
                                NewCollectionNode.AppendChild(NewAttrNode);
                                //
                                Doc.DocumentElement.AppendChild(NewCollectionNode);
                            }
                            //
                            // Save the result
                            //
                            LocalFilename = cpCore.addon.getPrivateFilesAddonPath() + "Collections.xml";
                            //LocalFilename = GetProgramPath & "\Addons\Collections.xml"
                            Doc.Save(cpCore.privateFiles.rootLocalPath + LocalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //
        //
        private static string GetCollectionPath(coreClass cpCore, string CollectionGuid) {
            string result = "";
            try {
                DateTime LastChangeDate = default(DateTime);
                string Collectionname = "";
                GetCollectionConfig(cpCore, CollectionGuid, ref result, ref LastChangeDate, ref Collectionname);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collection path, lastChangeDate, and collectionName given the guid
        /// </summary>
        /// <param name="CollectionGuid"></param>
        /// <param name="return_CollectionPath"></param>
        /// <param name="return_LastChagnedate"></param>
        /// <param name="return_CollectionName"></param>
        public static void GetCollectionConfig(coreClass cpCore, string CollectionGuid, ref string return_CollectionPath, ref DateTime return_LastChagnedate, ref string return_CollectionName) {
            try {
                string LocalPath = null;
                string LocalGuid = "";
                XmlDocument Doc = new XmlDocument();
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CollectionNode = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode LocalListNode = null;
                bool CollectionFound = false;
                string CollectionPath = "";
                DateTime LastChangeDate = default(DateTime);
                string hint = "";
                bool MatchFound = false;
                string LocalName = null;
                bool loadOK = false;
                //
                MatchFound = false;
                return_CollectionPath = "";
                return_LastChagnedate = DateTime.MinValue;
                loadOK = true;
                try {
                    Doc.LoadXml(getCollectionListFile(cpCore));
                } catch (Exception ex) {
                    //hint = hint & ",parse error"
                    logController.appendInstallLog(cpCore, "GetCollectionConfig, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                        logController.appendInstallLog(cpCore, "Hint=[" + hint + "], The Collections.xml file has an invalid root node");
                    } else {
                        if (true) {
                            //If genericController.vbLCase(.name) <> "collectionlist" Then
                            //    Call AppendClassLogFile("Server", "GetCollectionConfig", "Collections.xml file error, root node was not collectionlist, [" & .name & "].")
                            //Else
                            CollectionFound = false;
                            //hint = hint & ",checking nodes [" & .ChildNodes.Count & "]"
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                LocalName = "no name found";
                                LocalPath = "";
                                switch (genericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (genericController.vbLCase(CollectionNode.Name)) {
                                                case "name":
                                                    //
                                                    LocalName = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "guid":
                                                    //
                                                    LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "path":
                                                    //
                                                    CollectionPath = genericController.vbLCase(CollectionNode.InnerText);
                                                    break;
                                                case "lastchangedate":
                                                    LastChangeDate = genericController.EncodeDate(CollectionNode.InnerText);
                                                    break;
                                            }
                                        }
                                        break;
                                }
                                //hint = hint & ",checking node [" & LocalName & "]"
                                if (genericController.vbLCase(CollectionGuid) == LocalGuid) {
                                    return_CollectionPath = CollectionPath;
                                    return_LastChagnedate = LastChangeDate;
                                    return_CollectionName = LocalName;
                                    //Call AppendClassLogFile("Server", "GetCollectionConfigArg", "GetCollectionConfig, match found, CollectionName=" & LocalName & ", CollectionPath=" & CollectionPath & ", LastChangeDate=" & LastChangeDate)
                                    MatchFound = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //======================================================================================================
        // Installs Addons in a source folder
        //======================================================================================================
        //
        public static bool InstallCollectionsFromPrivateFolder(coreClass cpCore, string privateFolder, ref string return_ErrorMessage, ref List<string> return_CollectionGUIDList, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool returnSuccess = false;
            try {
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = BuildLocalCollectionReposFromFolder(cpCore, privateFolder, CollectionLastChangeDate, ref return_CollectionGUIDList, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    foreach (string collectionGuid in return_CollectionGUIDList) {
                        if (!installCollectionFromLocalRepo(cpCore, collectionGuid, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, ref nonCriticalErrorList)) {
                            logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        // Installs Addons in a source file
        //======================================================================================================
        //
        public static bool InstallCollectionsFromPrivateFile(coreClass cpCore, string pathFilename, ref string return_ErrorMessage, ref string return_CollectionGUID, bool IsNewBuild, ref List<string> nonCriticalErrorList) {
            bool returnSuccess = false;
            try {
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = BuildLocalCollectionRepoFromFile(cpCore, pathFilename, CollectionLastChangeDate, ref return_CollectionGUID, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    returnSuccess = installCollectionFromLocalRepo(cpCore, return_CollectionGUID, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, ref nonCriticalErrorList);
                    if (!returnSuccess) {
                        //
                        // Upgrade all apps failed
                        //
                        logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                    } else {
                        returnSuccess = true;
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //
        //
        private static int GetNavIDByGuid(coreClass cpCore, string ccGuid) {
            int navId = 0;
            try {
                int CS;
                //
                CS = cpCore.db.csOpen(cnNavigatorEntries, "ccguid=" + cpCore.db.encodeSQLText(ccGuid), "ID",true,0,false,false, "ID");
                if (cpCore.db.csOk(CS)) {
                    navId = cpCore.db.csGetInteger(CS, "id");
                }
                cpCore.db.csClose(ref CS);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return navId;
        }
        //        '
        //===============================================================================================
        //   copy resources from install folder to www folder
        //       block some file extensions
        //===============================================================================================
        //
        private static void CopyInstallToDst(coreClass cpCore, string SrcPath, string DstPath, string BlockFileList, string BlockFolderList) {
            try {
                FileInfo[] FileInfoArray = null;
                DirectoryInfo[] FolderInfoArray = null;
                string SrcFolder = null;
                string DstFolder = null;
                //
                SrcFolder = SrcPath;
                if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\") {
                    SrcFolder = SrcFolder.Left( SrcFolder.Length - 1);
                }
                //
                DstFolder = DstPath;
                if (DstFolder.Substring(DstFolder.Length - 1) == "\\") {
                    DstFolder = DstFolder.Left( DstFolder.Length - 1);
                }
                //
                if (cpCore.privateFiles.pathExists(SrcFolder)) {
                    FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder);
                    foreach (FileInfo file in FileInfoArray) {
                        if ((file.Extension == "dll") || (file.Extension == "exe") || (file.Extension == "zip")) {
                            //
                            // can not copy dll or exe
                            //
                            //Filename = Filename
                        } else if (("," + BlockFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase)  != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //file.Name = file.Name
                        } else {
                            //
                            // copy this file to destination
                            //
                            cpCore.privateFiles.copyFile(SrcPath + file.Name, DstPath + file.Name, cpCore.appRootFiles);
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    FolderInfoArray = cpCore.privateFiles.getFolderList(SrcFolder);
                    foreach (DirectoryInfo folder in FolderInfoArray) {
                        if (("," + BlockFolderList + ",").IndexOf("," + folder.Name + ",", System.StringComparison.OrdinalIgnoreCase)  == -1) {
                            CopyInstallToDst(cpCore, SrcPath + folder.Name + "\\", DstPath + folder.Name + "\\", BlockFileList, "");
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //
        //
        private static string GetCollectionFileList(coreClass cpCore, string SrcPath, string SubFolder, string ExcludeFileList) {
            string result = "";
            try {
                FileInfo[] FileInfoArray = null;
                DirectoryInfo[] FolderInfoArray = null;
                string SrcFolder;
                //
                SrcFolder = SrcPath + SubFolder;
                if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\") {
                    SrcFolder = SrcFolder.Left( SrcFolder.Length - 1);
                }
                //
                if (cpCore.privateFiles.pathExists(SrcFolder)) {
                    FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder);
                    foreach (FileInfo file in FileInfoArray) {
                        if (("," + ExcludeFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase)  != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //Filename = Filename
                        } else {
                            //
                            // copy this file to destination
                            //
                            result = result + "\r\n" + SubFolder + file.Name;
                            //runAtServer.IPAddress = "127.0.0.1"
                            //runAtServer.Port = "4531"
                            //QS = "SrcFile=" & encodeRequestVariable(SrcPath & Filename) & "&DstFile=" & encodeRequestVariable(DstPath & Filename)
                            //Call runAtServer.ExecuteCmd("CopyFile", QS)
                            //Call cpCore.app.privateFiles.CopyFile(SrcPath & Filename, DstPath & Filename)
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    FolderInfoArray = cpCore.privateFiles.getFolderList(SrcFolder);
                    foreach (DirectoryInfo folder in FolderInfoArray) {
                        result = result + GetCollectionFileList(cpCore, SrcPath, SubFolder + folder.Name + "\\", ExcludeFileList);
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //
        //
        private static void InstallCollectionFromLocalRepo_addonNode_Phase1(coreClass cpCore, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool return_UpgradeOK, ref string return_ErrorMessage) {
            try {
                //
                int fieldTypeID = 0;
                string fieldType = null;
                string test = null;
                int TriggerContentID = 0;
                string ContentNameorGuid = null;
                int navTypeId = 0;
                int scriptinglanguageid = 0;
                string ScriptingCode = null;
                string FieldName = null;
                string NodeName = null;
                string NewValue = null;
                string NavIconTypeString = null;
                string menuNameSpace = null;
                string FieldValue = "";
                int CS2 = 0;
                string ScriptingNameorGuid = "";
                //  Dim ScriptingModuleID As Integer
                string ScriptingEntryPoint = null;
                int ScriptingTimeout = 0;
                string ScriptingLanguage = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode ScriptingNode = null;
                XmlNode PageInterface = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode TriggerNode = null;
                bool NavDeveloperOnly = false;
                string StyleSheet = null;
                string ArgumentList = null;
                int CS = 0;
                string Criteria = null;
                bool IsFound = false;
                string addonName = null;
                string addonGuid = null;
                string navTypeName = null;
                int addonId = 0;
                string Basename;
                //
                Basename = genericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    addonName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(addonName)) {
                        addonName = "No Name";
                    }
                    addonGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", addonName);
                    if (string.IsNullOrEmpty(addonGuid)) {
                        addonGuid = addonName;
                    }
                    navTypeName = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "");
                    navTypeId = GetListIndex(navTypeName, navTypeIDList);
                    if (navTypeId == 0) {
                        navTypeId = NavTypeIDAddon;
                    }
                    Criteria = "(" + AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(addonGuid) + ")";
                    CS = cpCore.db.csOpen(cnAddons, Criteria,"", false);
                    if (cpCore.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        cpCore.db.csClose(ref CS);
                        Criteria = "(name=" + cpCore.db.encodeSQLText(addonName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = cpCore.db.csOpen(cnAddons, Criteria,"", false);
                        if (cpCore.db.csOk(CS)) {
                            logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!cpCore.db.csOk(CS)) {
                        //
                        // not found by GUID or by name, Insert a new addon
                        //
                        cpCore.db.csClose(ref CS);
                        CS = cpCore.db.csInsertRecord(cnAddons, 0);
                        if (cpCore.db.csOk(CS)) {
                            logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!cpCore.db.csOk(CS)) {
                        //
                        // Could not create new Add-on
                        //
                        logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        addonId = cpCore.db.csGetInteger(CS, "ID");
                        //
                        // Initialize the add-on
                        // Delete any existing related records - so if this is an update with removed relationships, those are removed
                        //
                        //Call cpCore.db.deleteContentRecords("Shared Styles Add-on Rules", "addonid=" & addonId)
                        //Call cpCore.db.deleteContentRecords("Add-on Scripting Module Rules", "addonid=" & addonId)
                        cpCore.db.deleteContentRecords("Add-on Include Rules", "addonid=" + addonId);
                        cpCore.db.deleteContentRecords("Add-on Content Trigger Rules", "addonid=" + addonId);
                        //
                        cpCore.db.csSet(CS, "collectionid", CollectionID);
                        cpCore.db.csSet(CS, AddonGuidFieldName, addonGuid);
                        cpCore.db.csSet(CS, "name", addonName);
                        cpCore.db.csSet(CS, "navTypeId", navTypeId);
                        ArgumentList = "";
                        StyleSheet = "";
                        NavDeveloperOnly = true;
                        if (AddonNode.ChildNodes.Count > 0) {
                            foreach (XmlNode PageInterfaceWithinLoop in AddonNode.ChildNodes) {
                                PageInterface = PageInterfaceWithinLoop;
                                switch (genericController.vbLCase(PageInterfaceWithinLoop.Name)) {
                                    case "activexdll":
                                        //
                                        // This is handled in BuildLocalCollectionFolder
                                        //
                                        break;
                                    case "editors":
                                        //
                                        // list of editors
                                        //
                                        foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes) {
                                            switch (genericController.vbLCase(TriggerNode.Name)) {
                                                case "type":
                                                    fieldType = TriggerNode.InnerText;
                                                    fieldTypeID = cpCore.db.getRecordID("Content Field Types", fieldType);
                                                    if (fieldTypeID > 0) {
                                                        Criteria = "(addonid=" + addonId + ")and(contentfieldTypeID=" + fieldTypeID + ")";
                                                        CS2 = cpCore.db.csOpen("Add-on Content Field Type Rules", Criteria);
                                                        if (!cpCore.db.csOk(CS2)) {
                                                            cpCore.db.csClose(ref CS2);
                                                            CS2 = cpCore.db.csInsertRecord("Add-on Content Field Type Rules", 0);
                                                        }
                                                        if (cpCore.db.csOk(CS2)) {
                                                            cpCore.db.csSet(CS2, "addonid", addonId);
                                                            cpCore.db.csSet(CS2, "contentfieldTypeID", fieldTypeID);
                                                        }
                                                        cpCore.db.csClose(ref CS2);
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case "processtriggers":
                                        //
                                        // list of events that trigger a process run for this addon
                                        //
                                        foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes) {
                                            switch (genericController.vbLCase(TriggerNode.Name)) {
                                                case "contentchange":
                                                    TriggerContentID = 0;
                                                    ContentNameorGuid = TriggerNode.InnerText;
                                                    if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                        ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "guid", "");
                                                        if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                            ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "name", "");
                                                        }
                                                    }
                                                    Criteria = "(ccguid=" + cpCore.db.encodeSQLText(ContentNameorGuid) + ")";
                                                    CS2 = cpCore.db.csOpen("Content", Criteria);
                                                    if (!cpCore.db.csOk(CS2)) {
                                                        cpCore.db.csClose(ref CS2);
                                                        Criteria = "(ccguid is null)and(name=" + cpCore.db.encodeSQLText(ContentNameorGuid) + ")";
                                                        CS2 = cpCore.db.csOpen("content", Criteria);
                                                    }
                                                    if (cpCore.db.csOk(CS2)) {
                                                        TriggerContentID = cpCore.db.csGetInteger(CS2, "ID");
                                                    }
                                                    cpCore.db.csClose(ref CS2);
                                                    //If TriggerContentID = 0 Then
                                                    //    CS2 = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                    //    If cpCore.db.cs_ok(CS2) Then
                                                    //        Call cpCore.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    //        Call cpCore.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    //        TriggerContentID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    //    End If
                                                    //    Call cpCore.db.cs_Close(CS2)
                                                    //End If
                                                    if (TriggerContentID == 0) {
                                                        //
                                                        // could not find the content
                                                        //
                                                    } else {
                                                        Criteria = "(addonid=" + addonId + ")and(contentid=" + TriggerContentID + ")";
                                                        CS2 = cpCore.db.csOpen("Add-on Content Trigger Rules", Criteria);
                                                        if (!cpCore.db.csOk(CS2)) {
                                                            cpCore.db.csClose(ref CS2);
                                                            CS2 = cpCore.db.csInsertRecord("Add-on Content Trigger Rules", 0);
                                                            if (cpCore.db.csOk(CS2)) {
                                                                cpCore.db.csSet(CS2, "addonid", addonId);
                                                                cpCore.db.csSet(CS2, "contentid", TriggerContentID);
                                                            }
                                                        }
                                                        cpCore.db.csClose(ref CS2);
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                    case "scripting":
                                        //
                                        // include add-ons - NOTE - import collections must be run before interfaces
                                        // when importing a collectin that will be used for an include
                                        //
                                        ScriptingLanguage = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "language", "");
                                        scriptinglanguageid = cpCore.db.getRecordID("scripting languages", ScriptingLanguage);
                                        cpCore.db.csSet(CS, "scriptinglanguageid", scriptinglanguageid);
                                        ScriptingEntryPoint = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "entrypoint", "");
                                        cpCore.db.csSet(CS, "ScriptingEntryPoint", ScriptingEntryPoint);
                                        ScriptingTimeout = genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "timeout", "5000"));
                                        cpCore.db.csSet(CS, "ScriptingTimeout", ScriptingTimeout);
                                        ScriptingCode = "";
                                        //Call cpCore.app.csv_SetCS(CS, "ScriptingCode", ScriptingCode)
                                        foreach (XmlNode ScriptingNode in PageInterfaceWithinLoop.ChildNodes) {
                                            switch (genericController.vbLCase(ScriptingNode.Name)) {
                                                case "code":
                                                    ScriptingCode = ScriptingCode + ScriptingNode.InnerText;
                                                    //Case "includemodule"

                                                    //    ScriptingModuleID = 0
                                                    //    ScriptingNameorGuid = ScriptingNode.InnerText
                                                    //    If ScriptingNameorGuid = "" Then
                                                    //        ScriptingNameorGuid = GetXMLAttribute(cpcore,IsFound, ScriptingNode, "guid", "")
                                                    //        If ScriptingNameorGuid = "" Then
                                                    //            ScriptingNameorGuid = GetXMLAttribute(cpcore,IsFound, ScriptingNode, "name", "")
                                                    //        End If
                                                    //    End If
                                                    //    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    //    CS2 = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                    //    If Not cpCore.db.cs_ok(CS2) Then
                                                    //        Call cpCore.db.cs_Close(CS2)
                                                    //        Criteria = "(ccguid is null)and(name=" & cpCore.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                    //        CS2 = cpCore.db.cs_open("Scripting Modules", Criteria)
                                                    //    End If
                                                    //    If cpCore.db.cs_ok(CS2) Then
                                                    //        ScriptingModuleID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    //    End If
                                                    //    Call cpCore.db.cs_Close(CS2)
                                                    //    If ScriptingModuleID = 0 Then
                                                    //        CS2 = cpCore.db.cs_insertRecord("Scripting Modules", 0)
                                                    //        If cpCore.db.cs_ok(CS2) Then
                                                    //            Call cpCore.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                    //            Call cpCore.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                    //            ScriptingModuleID = cpCore.db.cs_getInteger(CS2, "ID")
                                                    //        End If
                                                    //        Call cpCore.db.cs_Close(CS2)
                                                    //    End If
                                                    //    Criteria = "(addonid=" & addonId & ")and(scriptingmoduleid=" & ScriptingModuleID & ")"
                                                    //    CS2 = cpCore.db.cs_open("Add-on Scripting Module Rules", Criteria)
                                                    //    If Not cpCore.db.cs_ok(CS2) Then
                                                    //        Call cpCore.db.cs_Close(CS2)
                                                    //        CS2 = cpCore.db.cs_insertRecord("Add-on Scripting Module Rules", 0)
                                                    //        If cpCore.db.cs_ok(CS2) Then
                                                    //            Call cpCore.db.cs_set(CS2, "addonid", addonId)
                                                    //            Call cpCore.db.cs_set(CS2, "scriptingmoduleid", ScriptingModuleID)
                                                    //        End If
                                                    //    End If
                                                    //    Call cpCore.db.cs_Close(CS2)
                                                    break;
                                            }
                                        }
                                        cpCore.db.csSet(CS, "ScriptingCode", ScriptingCode);
                                        break;
                                    case "activexprogramid":
                                        //
                                        // save program id
                                        //
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        cpCore.db.csSet(CS, "ObjectProgramID", FieldValue);
                                        break;
                                    case "navigator":
                                        //
                                        // create a navigator entry with a parent set to this
                                        //
                                        cpCore.db.csSave2(CS);
                                        menuNameSpace = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "NameSpace", "");
                                        if (!string.IsNullOrEmpty(menuNameSpace)) {
                                            NavIconTypeString = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "type", "");
                                            if (string.IsNullOrEmpty(NavIconTypeString)) {
                                                NavIconTypeString = "Addon";
                                            }
                                            //Dim builder As New coreBuilderClass(cpCore)
                                            appBuilderController.verifyNavigatorEntry(cpCore, "", menuNameSpace, addonName, "", "", "", false, false, false, true, addonName, NavIconTypeString, addonName, CollectionID);
                                        }
                                        break;
                                    case "argument":
                                    case "argumentlist":
                                        //
                                        // multiple argumentlist elements are concatinated with crlf
                                        //
                                        NewValue = encodeText(PageInterfaceWithinLoop.InnerText).Trim(' ');
                                        if (!string.IsNullOrEmpty(NewValue)) {
                                            if (string.IsNullOrEmpty(ArgumentList)) {
                                                ArgumentList = NewValue;
                                            } else if (NewValue != FieldValue) {
                                                ArgumentList = ArgumentList + "\r\n" + NewValue;
                                            }
                                        }
                                        break;
                                    case "style":
                                        //
                                        // import exclusive style
                                        //
                                        NodeName = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "name", "");
                                        NewValue = encodeText(PageInterfaceWithinLoop.InnerText).Trim(' ');
                                        if (NewValue.Left( 1) != "{") {
                                            NewValue = "{" + NewValue;
                                        }
                                        if (NewValue.Substring(NewValue.Length - 1) != "}") {
                                            NewValue = NewValue + "}";
                                        }
                                        StyleSheet = StyleSheet + "\r\n" + NodeName + " " + NewValue;
                                        //Case "includesharedstyle"
                                        //    '
                                        //    ' added 9/3/2012
                                        //    '
                                        //    sharedStyleId = 0
                                        //    nodeNameOrGuid = GetXMLAttribute(cpcore,IsFound, PageInterface, "guid", "")
                                        //    If nodeNameOrGuid = "" Then
                                        //        nodeNameOrGuid = GetXMLAttribute(cpcore,IsFound, PageInterface, "name", "")
                                        //    End If
                                        //    Criteria = "(ccguid=" & cpCore.db.encodeSQLText(nodeNameOrGuid) & ")"
                                        //    CS2 = cpCore.db.cs_open("shared styles", Criteria)
                                        //    If Not cpCore.db.cs_ok(CS2) Then
                                        //        Call cpCore.db.cs_Close(CS2)
                                        //        Criteria = "(ccguid is null)and(name=" & cpCore.db.encodeSQLText(nodeNameOrGuid) & ")"
                                        //        CS2 = cpCore.db.cs_open("shared styles", Criteria)
                                        //    End If
                                        //    If cpCore.db.cs_ok(CS2) Then
                                        //        sharedStyleId = cpCore.db.cs_getInteger(CS2, "ID")
                                        //    End If
                                        //    Call cpCore.db.cs_Close(CS2)
                                        //    If sharedStyleId = 0 Then
                                        //        CS2 = cpCore.db.cs_insertRecord("shared styles", 0)
                                        //        If cpCore.db.cs_ok(CS2) Then
                                        //            Call cpCore.db.cs_set(CS2, "name", nodeNameOrGuid)
                                        //            Call cpCore.db.cs_set(CS2, "ccguid", nodeNameOrGuid)
                                        //            sharedStyleId = cpCore.db.cs_getInteger(CS2, "ID")
                                        //        End If
                                        //        Call cpCore.db.cs_Close(CS2)
                                        //    End If
                                        //    Criteria = "(addonid=" & addonId & ")and(StyleId=" & sharedStyleId & ")"
                                        //    CS2 = cpCore.db.cs_open("Shared Styles Add-on Rules", Criteria)
                                        //    If Not cpCore.db.cs_ok(CS2) Then
                                        //        Call cpCore.db.cs_Close(CS2)
                                        //        CS2 = cpCore.db.cs_insertRecord("Shared Styles Add-on Rules", 0)
                                        //        If cpCore.db.cs_ok(CS2) Then
                                        //            Call cpCore.db.cs_set(CS2, "addonid", addonId)
                                        //            Call cpCore.db.cs_set(CS2, "StyleId", sharedStyleId)
                                        //        End If
                                        //    End If
                                        //    Call cpCore.db.cs_Close(CS2)
                                        break;
                                    case "stylesheet":
                                    case "styles":
                                        //
                                        // import exclusive stylesheet if more then whitespace
                                        //
                                        test = PageInterfaceWithinLoop.InnerText;
                                        test = genericController.vbReplace(test, " ", "");
                                        test = genericController.vbReplace(test, "\r", "");
                                        test = genericController.vbReplace(test, "\n", "");
                                        test = genericController.vbReplace(test, "\t", "");
                                        if (!string.IsNullOrEmpty(test)) {
                                            StyleSheet = StyleSheet + "\r\n" + PageInterfaceWithinLoop.InnerText;
                                        }
                                        break;
                                    case "template":
                                    case "content":
                                    case "admin":
                                        //
                                        // these add-ons will be "non-developer only" in navigation
                                        //
                                        FieldName = PageInterfaceWithinLoop.Name;
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        if (!cpCore.db.cs_isFieldSupported(CS, FieldName)) {
                                            //
                                            // Bad field name - need to report it somehow
                                            //
                                        } else {
                                            cpCore.db.csSet(CS, FieldName, FieldValue);
                                            if (genericController.encodeBoolean(PageInterfaceWithinLoop.InnerText)) {
                                                //
                                                // if template, admin or content - let non-developers have navigator entry
                                                //
                                                NavDeveloperOnly = false;
                                            }
                                        }
                                        break;
                                    case "icon":
                                        //
                                        // icon
                                        //
                                        FieldValue = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "link", "");
                                        if (!string.IsNullOrEmpty(FieldValue)) {
                                            //
                                            // Icons can be either in the root of the website or in content files
                                            //
                                            FieldValue = genericController.vbReplace(FieldValue, "\\", "/"); // make it a link, not a file
                                            if (genericController.vbInstr(1, FieldValue, "://") != 0) {
                                                //
                                                // the link is an absolute URL, leave it link this
                                                //
                                            } else {
                                                if (FieldValue.Left( 1) != "/") {
                                                    //
                                                    // make sure it starts with a slash to be consistance
                                                    //
                                                    FieldValue = "/" + FieldValue;
                                                }
                                                if (FieldValue.Left( 17) == "/contensivefiles/") {
                                                    //
                                                    // in content files, start link without the slash
                                                    //
                                                    FieldValue = FieldValue.Substring(17);
                                                }
                                            }
                                            cpCore.db.csSet(CS, "IconFilename", FieldValue);
                                            if (true) {
                                                cpCore.db.csSet(CS, "IconWidth", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "width", "0")));
                                                cpCore.db.csSet(CS, "IconHeight", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "height", "0")));
                                                cpCore.db.csSet(CS, "IconSprites", genericController.EncodeInteger(GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "sprites", "0")));
                                            }
                                        }
                                        break;
                                    case "includeaddon":
                                    case "includeadd-on":
                                    case "include addon":
                                    case "include add-on":
                                        //
                                        // processed in phase2 of this routine, after all the add-ons are installed
                                        //
                                        break;
                                    case "form":
                                        //
                                        // The value of this node is the xml instructions to create a form. Take then
                                        //   entire node, children and all, and save them in the formxml field.
                                        //   this replaces the settings add-on type, and soo to be report add-on types as well.
                                        //   this removes the ccsettingpages and settingcollectionrules, etc.
                                        //
                                        if (true) {
                                            NavDeveloperOnly = false;
                                            cpCore.db.csSet(CS, "formxml", PageInterfaceWithinLoop.InnerXml);
                                        }
                                        break;
                                    case "javascript":
                                    case "javascriptinhead":
                                        //
                                        // these all translate to JSFilename
                                        //
                                        FieldName = "jsfilename";
                                        cpCore.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);

                                        break;
                                    case "iniframe":
                                        //
                                        // typo - field is inframe
                                        //
                                        FieldName = "inframe";
                                        cpCore.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);
                                        break;
                                    default:
                                        //
                                        // All the other fields should match the Db fields
                                        //
                                        FieldName = PageInterfaceWithinLoop.Name;
                                        FieldValue = PageInterfaceWithinLoop.InnerText;
                                        if (!cpCore.db.cs_isFieldSupported(CS, FieldName)) {
                                            //
                                            // Bad field name - need to report it somehow
                                            //
                                            cpCore.handleException(new ApplicationException("bad field found [" + FieldName + "], in addon node [" + addonName + "], of collection [" + cpCore.db.getRecordName("add-on collections", CollectionID) + "]"));
                                        } else {
                                            cpCore.db.csSet(CS, FieldName, FieldValue);
                                        }
                                        break;
                                }
                            }
                        }
                        cpCore.db.csSet(CS, "ArgumentList", ArgumentList);
                        cpCore.db.csSet(CS, "StylesFilename", StyleSheet);
                        // these are dynamic now
                        //            '
                        //            ' Setup special setting/tool/report Navigator Entry
                        //            '
                        //            If navTypeId = NavTypeIDTool Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{801F1F07-20E6-4A5D-AF26-71007CCB834F}"), addonid, 0, NavIconTypeTool, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                        //            If navTypeId = NavTypeIDReport Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{2ED078A2-6417-46CB-8572-A13F64C4BF18}"), addonid, 0, NavIconTypeReport, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                        //            If navTypeId = NavTypeIDSetting Then
                        //                AddonNavID = GetNonRootNavigatorID(asv, AOName, GetNavIDByGuid(asv, "{5FDDC758-4A15-4F98-8333-9CE8B8BFABC4}"), addonid, 0, NavIconTypeSetting, AOName & " Add-on", NavDeveloperOnly, 0, "", 0, 0, CollectionID, navAdminOnly)
                        //            End If
                    }
                    cpCore.db.csClose(ref CS);
                    //
                    // -- if this is needed, the installation xml files are available in the addon install folder. - I do not believe this is important
                    //       as if a collection is missing a dependancy, there is an error and you would expect to have to reinstall.
                    //
                    // Addon is now fully installed
                    // Go through all collection files on this site and see if there are
                    // any Dependencies on this add-on that need to be attached
                    // src args are those for the addon that includes the current addon
                    //   - if this addon is the target of another add-on's  "includeaddon" node
                    //
                    //Doc = New XmlDocument
                    //CS = cpCore.db.cs_open("Add-on Collections")
                    //Do While cpCore.db.cs_ok(CS)
                    //    CollectionFile = cpCore.db.cs_get(CS, "InstallFile")
                    //    If CollectionFile <> "" Then
                    //        Try
                    //            Call Doc.LoadXml(CollectionFile)
                    //            If Doc.DocumentElement.HasChildNodes Then
                    //                For Each TestObject In Doc.DocumentElement.ChildNodes
                    //                    '
                    //                    ' 20161002 - maybe this should be testing for an xmlElemetn, not node
                    //                    '
                    //                    If (TypeOf (TestObject) Is XmlElement) Then
                    //                        SrcMainNode = DirectCast(TestObject, XmlElement)
                    //                        If genericController.vbLCase(SrcMainNode.Name) = "addon" Then
                    //                            SrcAddonGuid = SrcMainNode.GetAttribute("guid")
                    //                            SrcAddonName = SrcMainNode.GetAttribute("name")
                    //                            If SrcMainNode.HasChildNodes Then
                    //                                '//On Error //Resume Next
                    //                                For Each TestObject2 In SrcMainNode.ChildNodes
                    //                                    'For Each SrcAddonNode In SrcMainNode.childNodes
                    //                                    If TypeOf TestObject2 Is XmlNode Then
                    //                                        SrcAddonNode = DirectCast(TestObject2, XmlElement)
                    //                                        If True Then
                    //                                            'If Err.Number <> 0 Then
                    //                                            '    ' this is to catch nodes that are not elements
                    //                                            '    Err.Clear
                    //                                            'Else
                    //                                            'On Error GoTo ErrorTrap
                    //                                            If genericController.vbLCase(SrcAddonNode.Name) = "includeaddon" Then
                    //                                                TestGuid = SrcAddonNode.GetAttribute("guid")
                    //                                                TestName = SrcAddonNode.GetAttribute("name")
                    //                                                Criteria = ""
                    //                                                If TestGuid <> "" Then
                    //                                                    If TestGuid = addonGuid Then
                    //                                                        Criteria = "(" & AddonGuidFieldName & "=" & cpCore.db.encodeSQLText(SrcAddonGuid) & ")"
                    //                                                    End If
                    //                                                ElseIf TestName <> "" Then
                    //                                                    If TestName = addonName Then
                    //                                                        Criteria = "(name=" & cpCore.db.encodeSQLText(SrcAddonName) & ")"
                    //                                                    End If
                    //                                                End If
                    //                                                If Criteria <> "" Then
                    //                                                    '$$$$$ cache this
                    //                                                    CS2 = cpCore.db.cs_open(cnAddons, Criteria, "ID")
                    //                                                    If cpCore.db.cs_ok(CS2) Then
                    //                                                        SrcAddonID = cpCore.db.cs_getInteger(CS2, "ID")
                    //                                                    End If
                    //                                                    Call cpCore.db.cs_Close(CS2)
                    //                                                    AddRule = False
                    //                                                    If SrcAddonID = 0 Then
                    //                                                        UserError = "The add-on being installed is referenced by another add-on in collection [], but this add-on could not be found by the respoective criteria [" & Criteria & "]"
                    //                                                        Call logcontroller.appendInstallLog(cpCore,  "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" & UserError & "]")
                    //                                                    Else
                    //                                                        CS2 = cpCore.db.cs_openCsSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" & SrcAddonID & " and IncludedAddonID=" & addonId)
                    //                                                        AddRule = Not cpCore.db.cs_ok(CS2)
                    //                                                        Call cpCore.db.cs_Close(CS2)
                    //                                                    End If
                    //                                                    If AddRule Then
                    //                                                        CS2 = cpCore.db.cs_insertRecord("Add-on Include Rules", 0)
                    //                                                        If cpCore.db.cs_ok(CS2) Then
                    //                                                            Call cpCore.db.cs_set(CS2, "Addonid", SrcAddonID)
                    //                                                            Call cpCore.db.cs_set(CS2, "IncludedAddonID", addonId)
                    //                                                        End If
                    //                                                        Call cpCore.db.cs_Close(CS2)
                    //                                                    End If
                    //                                                End If
                    //                                            End If
                    //                                        End If
                    //                                    End If
                    //                                Next
                    //                            End If
                    //                        End If
                    //                    Else
                    //                        CS = CS
                    //                    End If
                    //                Next
                    //            End If
                    //        Catch ex As Exception
                    //            cpCore.handleExceptionAndContinue(ex) : Throw
                    //        End Try
                    //    End If
                    //    Call cpCore.db.cs_goNext(CS)
                    //Loop
                    //Call cpCore.db.cs_Close(CS)
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }



        //
        //============================================================================
        //   process the include add-on node of the add-on nodes
        //       this is the second pass, so all add-ons should be added
        //       no errors for missing addones, except the include add-on case
        //============================================================================
        //
        private static string InstallCollectionFromLocalRepo_addonNode_Phase2(coreClass cpCore, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage) {
            string result = "";
            try {
                bool AddRule = false;
                string IncludeAddonName = null;
                string IncludeAddonGuid = null;
                int IncludeAddonID = 0;
                string UserError = null;
                int CS2 = 0;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode PageInterface = null;
                bool NavDeveloperOnly = false;
                string StyleSheet = null;
                string ArgumentList = null;
                int CS = 0;
                string Criteria = null;
                bool IsFound = false;
                string AOName = null;
                string AOGuid = null;
                string AddOnType = null;
                int addonId = 0;
                string Basename;
                //
                Basename = genericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    AOName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(AOName)) {
                        AOName = "No Name";
                    }
                    AOGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", AOName);
                    if (string.IsNullOrEmpty(AOGuid)) {
                        AOGuid = AOName;
                    }
                    AddOnType = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "");
                    Criteria = "(" + AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(AOGuid) + ")";
                    CS = cpCore.db.csOpen(cnAddons, Criteria,"", false);
                    if (cpCore.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        cpCore.db.csClose(ref CS);
                        Criteria = "(name=" + cpCore.db.encodeSQLText(AOName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = cpCore.db.csOpen(cnAddons, Criteria,"", false);
                    }
                    if (!cpCore.db.csOk(CS)) {
                        //
                        // Could not find add-on
                        //
                        logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        addonId = cpCore.db.csGetInteger(CS, "ID");
                        ArgumentList = "";
                        StyleSheet = "";
                        NavDeveloperOnly = true;
                        if (AddonNode.ChildNodes.Count > 0) {
                            foreach (XmlNode PageInterface in AddonNode.ChildNodes) {
                                switch (genericController.vbLCase(PageInterface.Name)) {
                                    case "includeaddon":
                                    case "includeadd-on":
                                    case "include addon":
                                    case "include add-on":
                                        //
                                        // include add-ons - NOTE - import collections must be run before interfaces
                                        // when importing a collectin that will be used for an include
                                        //
                                        if (true) {
                                            IncludeAddonName = GetXMLAttribute(cpCore, IsFound, PageInterface, "name", "");
                                            IncludeAddonGuid = GetXMLAttribute(cpCore, IsFound, PageInterface, "guid", IncludeAddonName);
                                            IncludeAddonID = 0;
                                            Criteria = "";
                                            if (!string.IsNullOrEmpty(IncludeAddonGuid)) {
                                                Criteria = AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(IncludeAddonGuid);
                                                if (string.IsNullOrEmpty(IncludeAddonName)) {
                                                    IncludeAddonName = "Add-on " + IncludeAddonGuid;
                                                }
                                            } else if (!string.IsNullOrEmpty(IncludeAddonName)) {
                                                Criteria = "(name=" + cpCore.db.encodeSQLText(IncludeAddonName) + ")";
                                            }
                                            if (!string.IsNullOrEmpty(Criteria)) {
                                                CS2 = cpCore.db.csOpen(cnAddons, Criteria);
                                                if (cpCore.db.csOk(CS2)) {
                                                    IncludeAddonID = cpCore.db.csGetInteger(CS2, "ID");
                                                }
                                                cpCore.db.csClose(ref CS2);
                                                AddRule = false;
                                                if (IncludeAddonID == 0) {
                                                    UserError = "The include add-on [" + IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it.";
                                                    logController.appendInstallLog(cpCore, "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" + UserError + "]");
                                                    ReturnUpgradeOK = false;
                                                    ReturnErrorMessage = ReturnErrorMessage + "<P>The collection was not installed because the add-on [" + AOName + "] requires an included add-on [" + IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>";
                                                } else {
                                                    CS2 = cpCore.db.csOpenSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" + addonId + " and IncludedAddonID=" + IncludeAddonID);
                                                    AddRule = !cpCore.db.csOk(CS2);
                                                    cpCore.db.csClose(ref CS2);
                                                }
                                                if (AddRule) {
                                                    CS2 = cpCore.db.csInsertRecord("Add-on Include Rules", 0);
                                                    if (cpCore.db.csOk(CS2)) {
                                                        cpCore.db.csSet(CS2, "Addonid", addonId);
                                                        cpCore.db.csSet(CS2, "IncludedAddonID", IncludeAddonID);
                                                    }
                                                    cpCore.db.csClose(ref CS2);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    cpCore.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
            //
        }
        //
        //====================================================================================================
        //Public Sub housekeepAddonFolder()
        //    Try
        //        Dim RegisterPathList As String
        //        Dim RegisterPath As String
        //        Dim RegisterPaths() As String
        //        Dim Path As String
        //        Dim NodeCnt As Integer
        //        Dim IsActiveFolder As Boolean
        //        Dim Cmd As String
        //        Dim CollectionRootPath As String
        //        Dim Pos As Integer
        //        Dim FolderList As DirectoryInfo()
        //        Dim LocalPath As String
        //        Dim LocalGuid As String
        //        Dim Doc As New XmlDocument
        //        Dim CollectionNode As XmlNode
        //        Dim LocalListNode As XmlNode
        //        Dim FolderPtr As Integer
        //        Dim CollectionPath As String
        //        Dim LastChangeDate As Date
        //        Dim hint As String
        //        Dim LocalName As String
        //        Dim Ptr As Integer
        //        '
        //        Call AppendClassLogFile("Server", "RegisterAddonFolder", "Entering RegisterAddonFolder")
        //        '
        //        Dim loadOK As Boolean = True
        //        Try
        //            Call Doc.LoadXml(getCollectionListFile)
        //        Catch ex As Exception
        //            'hint = hint & ",parse error"
        //            Call AppendClassLogFile("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], Error loading Collections.xml file.")
        //            loadOK = False
        //        End Try
        //        If loadOK Then
        //            '
        //            Call AppendClassLogFile("Server", "RegisterAddonFolder", "Collection.xml loaded ok")
        //            '
        //            If genericController.vbLCase(Doc.DocumentElement.Name) <> genericController.vbLCase(CollectionListRootNode) Then
        //                Call AppendClassLogFile("Server", "", "RegisterAddonFolder, Hint=[" & hint & "], The Collections.xml file has an invalid root node, [" & Doc.DocumentElement.Name & "] was received and [" & CollectionListRootNode & "] was expected.")
        //            Else
        //                '
        //                Call AppendClassLogFile("Server", "RegisterAddonFolder", "Collection.xml root name ok")
        //                '
        //                With Doc.DocumentElement
        //                    If True Then
        //                        'If genericController.vbLCase(.name) <> "collectionlist" Then
        //                        '    Call AppendClassLogFile("Server", "", "RegisterAddonFolder, basename was not collectionlist, [" & .name & "].")
        //                        'Else
        //                        NodeCnt = 0
        //                        RegisterPathList = ""
        //                        For Each LocalListNode In .ChildNodes
        //                            '
        //                            ' Get the collection path
        //                            '
        //                            CollectionPath = ""
        //                            LocalGuid = ""
        //                            LocalName = "no name found"
        //                            LocalPath = ""
        //                            Select Case genericController.vbLCase(LocalListNode.Name)
        //                                Case "collection"
        //                                    LocalGuid = ""
        //                                    For Each CollectionNode In LocalListNode.ChildNodes
        //                                        Select Case genericController.vbLCase(CollectionNode.Name)
        //                                            Case "name"
        //                                                '
        //                                                LocalName = genericController.vbLCase(CollectionNode.InnerText)
        //                                            Case "guid"
        //                                                '
        //                                                LocalGuid = genericController.vbLCase(CollectionNode.InnerText)
        //                                            Case "path"
        //                                                '
        //                                                CollectionPath = genericController.vbLCase(CollectionNode.InnerText)
        //                                            Case "lastchangedate"
        //                                                LastChangeDate =  genericController.EncodeDate(CollectionNode.InnerText)
        //                                        End Select
        //                                    Next
        //                            End Select
        //                            '
        //                            Call AppendClassLogFile("Server", "RegisterAddonFolder", "Node[" & NodeCnt & "], LocalName=[" & LocalName & "], LastChangeDate=[" & LastChangeDate & "], CollectionPath=[" & CollectionPath & "], LocalGuid=[" & LocalGuid & "]")
        //                            '
        //                            ' Go through all subpaths of the collection path, register the version match, unregister all others
        //                            '
        //                            'fs = New fileSystemClass
        //                            If CollectionPath = "" Then
        //                                '
        //                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "no collection path, skipping")
        //                                '
        //                            Else
        //                                CollectionPath = genericController.vbLCase(CollectionPath)
        //                                CollectionRootPath = CollectionPath
        //                                Pos = InStrRev(CollectionRootPath, "\")
        //                                If Pos <= 0 Then
        //                                    '
        //                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "CollectionPath has no '\', skipping")
        //                                    '
        //                                Else
        //                                    CollectionRootPath = Left(CollectionRootPath, Pos - 1)
        //                                    Path = cpCore.app.getAddonPath() & "\" & CollectionRootPath & "\"
        //                                    'Path = GetProgramPath & "\addons\" & CollectionRootPath & "\"
        //                                    '//On Error //Resume Next
        //                                    If cpCore.app.privateFiles.checkPath(Path) Then
        //                                        FolderList = cpCore.app.privateFiles.getFolders(Path)
        //                                        If Err.Number <> 0 Then
        //                                            Err.Clear()
        //                                        End If
        //                                    End If
        //                                    'On Error GoTo ErrorTrap
        //                                    If FolderList.Length = 0 Then
        //                                        '
        //                                        Call AppendClassLogFile("Server", "RegisterAddonFolder", "no subfolders found in physical path [" & Path & "], skipping")
        //                                        '
        //                                    Else
        //                                        For Each dir As DirectoryInfo In FolderList
        //                                            IsActiveFolder = False
        //                                            '
        //                                            ' register or unregister all files in this folder
        //                                            '
        //                                            If dir.Name = "" Then
        //                                                '
        //                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....empty folder [" & dir.Name & "], skipping")
        //                                                '
        //                                            Else
        //                                                '
        //                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Folder [" & dir.Name & "]")
        //                                                IsActiveFolder = (CollectionRootPath & "\" & dir.Name = CollectionPath)
        //                                                If IsActiveFolder And (FolderPtr <> (FolderList.Count - 1)) Then
        //                                                    '
        //                                                    ' This one is active, but not the last
        //                                                    '
        //                                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Active addon is not the most current, this folder is the active folder, but there are more recent folders. This folder will be preserved.")
        //                                                End If
        //                                                ' 20161005 - no longer need to register activeX
        //                                                'FileList = cpCore.app.privateFiles.GetFolderFiles(Path & "\" & dir.Name)
        //                                                'For Each file As FileInfo In FileList
        //                                                '    If Right(file.Name, 4) = ".dll" Then
        //                                                '        If IsActiveFolder Then
        //                                                '            '
        //                                                '            ' register this file
        //                                                '            '
        //                                                '            RegisterPathList = RegisterPathList & vbCrLf & Path & dir.Name & "\" & file.Name
        //                                                '            '                                                                Cmd = "%comspec% /c regsvr32 """ & RegisterPathList & """ /s"
        //                                                '            '                                                                Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Regsiter DLL [" & Cmd & "]")
        //                                                '            '                                                                Call runProcess(cp.core,Cmd, , True)
        //                                                '        Else
        //                                                '            '
        //                                                '            ' unregister this file
        //                                                '            '
        //                                                '            Cmd = "%comspec% /c regsvr32 /u """ & Path & dir.Name & "\" & file.Name & """ /s"
        //                                                '            Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Unregsiter DLL [" & Cmd & "]")
        //                                                '            Call runProcess(cpCore, Cmd, , True)
        //                                                '        End If
        //                                                '    End If
        //                                                'Next
        //                                                '
        //                                                ' only keep last two non-matching folders and the active folder
        //                                                '
        //                                                If IsActiveFolder Then
        //                                                    IsActiveFolder = IsActiveFolder
        //                                                Else
        //                                                    If FolderPtr < (FolderList.Count - 3) Then
        //                                                        Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Deleting path because non-active and not one of the newest 2 [" & Path & dir.Name & "]")
        //                                                        Call cpCore.app.privateFiles.DeleteFileFolder(Path & dir.Name)
        //                                                    End If
        //                                                End If
        //                                            End If
        //                                        Next
        //                                        '
        //                                        ' register files found in the active folder last
        //                                        '
        //                                        If RegisterPathList <> "" Then
        //                                            RegisterPaths = Split(RegisterPathList, vbCrLf)
        //                                            For Ptr = 0 To UBound(RegisterPaths)
        //                                                RegisterPath = Trim(RegisterPaths[Ptr])
        //                                                If RegisterPath <> "" Then
        //                                                    Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
        //                                                    Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
        //                                                    Call runProcess(cpCore, Cmd, , True)
        //                                                End If
        //                                            Next
        //                                            RegisterPathList = ""
        //                                        End If
        //                                    End If
        //                                End If
        //                            End If
        //                            NodeCnt = NodeCnt + 1
        //                        Next
        //                        ' 20161005 - no longer need to register activeX
        //                        ''
        //                        '' register files found in the active folder last
        //                        ''
        //                        'If RegisterPathList <> "" Then
        //                        '    RegisterPaths = Split(RegisterPathList, vbCrLf)
        //                        '    For Ptr = 0 To UBound(RegisterPaths)
        //                        '        RegisterPath = Trim(RegisterPaths[Ptr])
        //                        '        If RegisterPath <> "" Then
        //                        '            Cmd = "%comspec% /c regsvr32 """ & RegisterPath & """ /s"
        //                        '            Call AppendClassLogFile("Server", "RegisterAddonFolder", "....Register DLL [" & Cmd & "]")
        //                        '            Call runProcess(cpCore, Cmd, , True)
        //                        '        End If
        //                        '    Next
        //                        'End If
        //                    End If
        //                End With
        //            End If
        //        End If
        //        '
        //        Call AppendClassLogFile("Server", "RegisterAddonFolder", "Exiting RegisterAddonFolder")
        //    Catch ex As Exception
        //        cpCore.handleException(ex);
        //    End Try
        //End Sub
        //        '
        //        '=============================================================================
        //        '   Verify an Admin Navigator Entry
        //        '       Entries are unique by their ccGuid
        //        '       Includes InstalledByCollectionID
        //        '       returns the entry id
        //        '=============================================================================
        //        '
        //        private Function csv_VerifyNavigatorEntry4(ByVal asv As appServicesClass, ByVal ccGuid As String, ByVal menuNameSpace As String, ByVal EntryName As String, ByVal ContentName As String, ByVal LinkPage As String, ByVal SortOrder As String, ByVal AdminOnly As Boolean, ByVal DeveloperOnly As Boolean, ByVal NewWindow As Boolean, ByVal Active As Boolean, ByVal MenuContentName As String, ByVal AddonName As String, ByVal NavIconType As String, ByVal NavIconTitle As String, ByVal InstalledByCollectionID As Integer) As Integer
        //            On Error GoTo ErrorTrap : 'Const Tn = "VerifyNavigatorEntry4" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        //            '
        //            Const AddonContentName = cnAddons
        //            '
        //            Dim DupFound As Boolean
        //            Dim EntryID As Integer
        //            Dim DuplicateID As Integer
        //            Dim Parents() As String
        //            Dim ParentPtr As Integer
        //            Dim Criteria As String
        //            Dim Ptr As Integer
        //            Dim RecordID As Integer
        //            Dim RecordName As String
        //            Dim SelectList As String
        //            Dim ErrorDescription As String
        //            Dim CSEntry As Integer
        //            Dim ContentID As Integer
        //            Dim ParentID As Integer
        //            Dim addonId As Integer
        //            Dim CS As Integer
        //            Dim SupportAddonID As Boolean
        //            Dim SupportGuid As Boolean
        //            Dim SupportNavGuid As Boolean
        //            Dim SupportccGuid As Boolean
        //            Dim SupportNavIcon As Boolean
        //            Dim GuidFieldName As String
        //            Dim SupportInstalledByCollectionID As Boolean
        //            '
        //            If Trim(EntryName) <> "" Then
        //                If genericController.vbLCase(EntryName) = "manage add-ons" Then
        //                    EntryName = EntryName
        //                End If
        //                '
        //                ' Setup misc arguments
        //                '
        //                SelectList = "Name,ContentID,ParentID,LinkPage,SortOrder,AdminOnly,DeveloperOnly,NewWindow,Active,NavIconType,NavIconTitle"
        //                SupportAddonID = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "AddonID")
        //                SupportInstalledByCollectionID = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "InstalledByCollectionID")
        //                If SupportAddonID Then
        //                    SelectList = SelectList & ",AddonID"
        //                Else
        //                    SelectList = SelectList & ",0 as AddonID"
        //                End If
        //                If SupportInstalledByCollectionID Then
        //                    SelectList = SelectList & ",InstalledByCollectionID"
        //                End If
        //                If cpCore.app.csv_IsContentFieldSupported(MenuContentName, "ccGuid") Then
        //                    SupportGuid = True
        //                    SupportccGuid = True
        //                    GuidFieldName = "ccguid"
        //                    SelectList = SelectList & ",ccGuid"
        //                ElseIf cpCore.app.csv_IsContentFieldSupported(MenuContentName, "NavGuid") Then
        //                    SupportGuid = True
        //                    SupportNavGuid = True
        //                    GuidFieldName = "navguid"
        //                    SelectList = SelectList & ",NavGuid"
        //                Else
        //                    SelectList = SelectList & ",'' as ccGuid"
        //                End If
        //                SupportNavIcon = cpCore.app.csv_IsContentFieldSupported(MenuContentName, "NavIconType")
        //                addonId = 0
        //                If SupportAddonID And (AddonName <> "") Then
        //                    CS = cpCore.app.csOpen(AddonContentName, "name=" & EncodeSQLText(AddonName), "ID", False, , , , "ID", 1)
        //                    If cpCore.app.csv_IsCSOK(CS) Then
        //                        addonId = cpCore.app.csv_cs_getInteger(CS, "ID")
        //                    End If
        //                    Call cpCore.app.csv_CloseCS(CS)
        //                End If
        //                ParentID = csv_GetParentIDFromNameSpace(asv, MenuContentName, menuNameSpace)
        //                ContentID = -1
        //                If ContentName <> "" Then
        //                    ContentID = cpCore.app.csv_GetContentID(ContentName)
        //                End If
        //                '
        //                ' Locate current entry(s)
        //                '
        //                CSEntry = -1
        //                Criteria = ""
        //                If True Then
        //                    ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                    If ccGuid = "" Then
        //                        '
        //                        ' ----- Find match by menuNameSpace
        //                        '
        //                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(Parentid=" & ParentID & ")and((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))", "ID", True, , , , SelectList)
        //                    Else
        //                        '
        //                        ' ----- Find match by guid
        //                        '
        //                        CSEntry = cpCore.app.csOpen(MenuContentName, "(" & GuidFieldName & "=" & EncodeSQLText(ccGuid) & ")", "ID", True, , , , SelectList)
        //                    End If
        //                    If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        //                        '
        //                        ' ----- if not found by guid, look for a name/parent match with a blank guid
        //                        '
        //                        Call cpCore.app.csv_CloseCS(CSEntry)
        //                        Criteria = "AND((" & GuidFieldName & " is null)or(" & GuidFieldName & "=''))"
        //                    End If
        //                End If
        //                If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        //                    If ParentID = 0 Then
        //                        ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                        Criteria = Criteria & "And(name=" & EncodeSQLText(EntryName) & ")and(ParentID is null)"
        //                    Else
        //                        ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to update the active entries
        //                        Criteria = Criteria & "And(name=" & EncodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")"
        //                    End If
        //                    CSEntry = cpCore.app.csOpen(MenuContentName, Mid(Criteria, 4), "ID", True, , , , SelectList)
        //                End If
        //                '
        //                ' If no current entry, create one
        //                '
        //                If Not cpCore.app.csv_IsCSOK(CSEntry) Then
        //                    cpCore.app.csv_CloseCS(CSEntry)
        //                    '
        //                    ' This entry was not found - insert a new record if there is no other name/menuNameSpace match
        //                    '
        //                    If False Then
        //                        '
        //                        ' OK - the first entry search was name/menuNameSpace
        //                        '
        //                        DupFound = False
        //                    ElseIf ParentID = 0 Then
        //                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(ParentID is null)", "ID", False, , , , SelectList)
        //                        DupFound = cpCore.app.csv_IsCSOK(CSEntry)
        //                        cpCore.app.csv_CloseCS(CSEntry)
        //                    Else
        //                        CSEntry = cpCore.app.csOpen(MenuContentName, "(name=" & EncodeSQLText(EntryName) & ")and(ParentID=" & ParentID & ")", "ID", False, , , , SelectList)
        //                        DupFound = cpCore.app.csv_IsCSOK(CSEntry)
        //                        cpCore.app.csv_CloseCS(CSEntry)
        //                    End If
        //                    If DupFound Then
        //                        '
        //                        ' Must block this entry because a menuNameSpace duplicate exists
        //                        '
        //                        CSEntry = -1
        //                    Else
        //                        '
        //                        ' Create new entry
        //                        '
        //                        CSEntry = cpCore.app.csv_InsertCSRecord(MenuContentName, SystemMemberID)
        //                    End If
        //                End If
        //                If cpCore.app.csv_IsCSOK(CSEntry) Then
        //                    EntryID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        //                    If EntryID = 265 Then
        //                        EntryID = EntryID
        //                    End If
        //                    Call cpCore.app.csv_SetCS(CSEntry, "name", EntryName)
        //                    If ParentID = 0 Then
        //                        Call cpCore.app.csv_SetCS(CSEntry, "ParentID", Nothing)
        //                    Else
        //                        Call cpCore.app.csv_SetCS(CSEntry, "ParentID", ParentID)
        //                    End If
        //                    If (ContentID = -1) Then
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "ContentID", Nothing)
        //                    Else
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "ContentID", ContentID)
        //                    End If
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "LinkPage", LinkPage)
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "SortOrder", SortOrder)
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "AdminOnly", AdminOnly)
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "DeveloperOnly", DeveloperOnly)
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "NewWindow", NewWindow)
        //                    Call cpCore.app.csv_SetCSField(CSEntry, "Active", Active)
        //                    If SupportAddonID Then
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "AddonID", addonId)
        //                    End If
        //                    If SupportGuid Then
        //                        Call cpCore.app.csv_SetCSField(CSEntry, GuidFieldName, ccGuid)
        //                    End If
        //                    If SupportNavIcon Then
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "NavIconTitle", NavIconTitle)
        //                        Dim NavIconID As Integer
        //                        NavIconID = GetListIndex(NavIconType, NavIconTypeList)
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "NavIconType", NavIconID)
        //                    End If
        //                    If SupportInstalledByCollectionID Then
        //                        Call cpCore.app.csv_SetCSField(CSEntry, "InstalledByCollectionID", InstalledByCollectionID)
        //                    End If
        //                    '
        //                    ' merge any duplicate guid matches
        //                    '
        //                    Call cpCore.app.csv_NextCSRecord(CSEntry)
        //                    Do While cpCore.app.csv_IsCSOK(CSEntry)
        //                        DuplicateID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        //                        Call cpCore.app.ExecuteSQL( "update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                        Call cpCore.app.csv_DeleteContentRecord(MenuContentName, DuplicateID)
        //                        Call cpCore.app.csv_NextCSRecord(CSEntry)
        //                    Loop
        //                End If
        //                Call cpCore.app.csv_CloseCS(CSEntry)
        //                '
        //                ' Merge duplicates with menuNameSpace.Name match
        //                '
        //                If EntryID <> 0 Then
        //                    If ParentID = 0 Then
        //                        CSEntry = cpCore.app.csv_OpenCSSQL("default", "select * from ccMenuEntries where (parentid is null)and(name=" & EncodeSQLText(EntryName) & ")and(id<>" & EntryID & ")", 0)
        //                    Else
        //                        CSEntry = cpCore.app.csv_OpenCSSQL("default", "select * from ccMenuEntries where (parentid=" & ParentID & ")and(name=" & EncodeSQLText(EntryName) & ")and(id<>" & EntryID & ")", 0)
        //                    End If
        //                    Do While cpCore.app.csv_IsCSOK(CSEntry)
        //                        DuplicateID = cpCore.app.csv_cs_getInteger(CSEntry, "ID")
        //                        Call cpCore.app.ExecuteSQL( "update ccMenuEntries set ParentID=" & EntryID & " where ParentID=" & DuplicateID)
        //                        Call cpCore.app.csv_DeleteContentRecord(MenuContentName, DuplicateID)
        //                        Call cpCore.app.csv_NextCSRecord(CSEntry)
        //                    Loop
        //                    Call cpCore.app.csv_CloseCS(CSEntry)
        //                End If
        //            End If
        //            '
        //            csv_VerifyNavigatorEntry4 = EntryID
        //            '
        //            Exit Function
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Call HandleClassTrapError(Err.Number, Err.Source, Err.Description, "csv_VerifyNavigatorEntry4", True, False)
        //        End Function
        //        '
        //        '
        //        '
        //        private Function csv_GetParentIDFromNameSpace(ByVal asv As appServicesClass, ByVal ContentName As String, ByVal menuNameSpace As String) As Integer
        //            On Error GoTo ErrorTrap : 'Const Tn = "GetParentIDFrommenuNameSpace" : ''Dim th as integer : th = profileLogMethodEnter(Tn)
        //            '
        //            Dim Parents() As String
        //            Dim ParentID As Integer
        //            Dim Ptr As Integer
        //            Dim RecordName As String
        //            Dim Criteria As String
        //            Dim CS As Integer
        //            Dim RecordID As Integer
        //            '
        //            Parents = Split(menuNameSpace, ".")
        //            ParentID = 0
        //            For Ptr = 0 To UBound(Parents)
        //                RecordName = Parents[Ptr]
        //                If ParentID = 0 Then
        //                    Criteria = "(name=" & EncodeSQLText(RecordName) & ")and(Parentid is null)"
        //                Else
        //                    Criteria = "(name=" & EncodeSQLText(RecordName) & ")and(Parentid=" & ParentID & ")"
        //                End If
        //                RecordID = 0
        //                ' 12/19/2008 change to ActiveOnly - because if there is a non-guid entry, it is marked inactive. We only want to attach to the active entries
        //                CS = cpCore.app.csOpen(ContentName, Criteria, "ID", True, , , , "ID", 1)
        //                If cpCore.app.csv_IsCSOK(CS) Then
        //                    RecordID = (cpCore.app.csv_cs_getInteger(CS, "ID"))
        //                End If
        //                Call cpCore.app.csv_CloseCS(CS)
        //                If RecordID = 0 Then
        //                    CS = cpCore.app.csv_InsertCSRecord(ContentName, SystemMemberID)
        //                    If cpCore.app.csv_IsCSOK(CS) Then
        //                        RecordID = cpCore.app.csv_cs_getInteger(CS, "ID")
        //                        Call cpCore.app.csv_SetCS(CS, "name", RecordName)
        //                        If ParentID <> 0 Then
        //                            Call cpCore.app.csv_SetCS(CS, "parentID", ParentID)
        //                        End If
        //                    End If
        //                    Call cpCore.app.csv_CloseCS(CS)
        //                End If
        //                ParentID = RecordID
        //            Next
        //            '
        //            csv_GetParentIDFromNameSpace = ParentID

        //            '
        //            Exit Function
        //            '
        ////ErrorTrap:
        //            Call HandleClassTrapError(cpCore.app.config.name, Err.Number, Err.Source, Err.Description, "unknownMethodNameLegacyCall", True)
        //        End Function
        //
        //=========================================================================================
        //   Import CDef on top of current configuration and the base configuration
        //
        //=========================================================================================
        //
        public static void installBaseCollection(coreClass cpCore, bool isNewBuild, ref List<string> nonCriticalErrorList) {
            try {
                string ignoreString = "";
                string returnErrorMessage = "";
                bool ignoreBoolean = false;
                bool isBaseCollection = true;
                //
                // -- new build
                // 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
                string baseCollectionXml = cpCore.programFiles.readFile("aoBase5.xml");
                if (string.IsNullOrEmpty(baseCollectionXml)) {
                    //
                    // -- base collection notfound
                    throw new ApplicationException("Cannot load aoBase5.xml [" + cpCore.programFiles.rootLocalPath + "aoBase5.xml]");
                } else {
                    logController.appendInstallLog(cpCore, "Verify base collection -- new build");
                    miniCollectionModel baseCollection = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, true, true, isNewBuild, new miniCollectionModel());
                    installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, ref nonCriticalErrorList);
                    //If isNewBuild Then
                    //    '
                    //    ' -- new build
                    //    Call logcontroller.appendInstallLog(cpCore,  "Verify base collection -- new build")
                    //    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    //    Call installCollection_BuildDbFromMiniCollection(cpCore, baseCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    //    'Else
                    //    '    '
                    //    '    ' -- verify current build
                    //    '    Call logcontroller.appendInstallLog(cpCore,  "Verify base collection - existing build")
                    //    '    Dim baseCollection As miniCollectionModel = installCollection_LoadXmlToMiniCollection(cpCore, baseCollectionXml, True, True, isNewBuild, New miniCollectionModel)
                    //    '    Dim workingCollection As miniCollectionModel = installCollection_GetApplicationMiniCollection(cpCore, False)
                    //    '    Call installCollection_AddMiniCollectionSrcToDst(cpCore, workingCollection, baseCollection, False)
                    //    '    Call installCollection_BuildDbFromMiniCollection(cpCore, workingCollection, cpCore.siteProperties.dataBuildVersion, isNewBuild, nonCriticalErrorList)
                    //End If
                    //
                    // now treat as a regular collection and install - to pickup everything else 
                    //
                    string tmpFolderPath = "installBaseCollection" + genericController.GetRandomInteger(cpCore).ToString() + "\\";
                    cpCore.privateFiles.createPath(tmpFolderPath);
                    cpCore.programFiles.copyFile("aoBase5.xml", tmpFolderPath + "aoBase5.xml", cpCore.privateFiles);
                    List<string> ignoreList = new List<string>();
                    if (!InstallCollectionsFromPrivateFolder(cpCore, tmpFolderPath, ref returnErrorMessage, ref ignoreList, isNewBuild, ref nonCriticalErrorList)) {
                        throw new ApplicationException(returnErrorMessage);
                    }
                    cpCore.privateFiles.DeleteFileFolder(tmpFolderPath);
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //=========================================================================================
        //
        //=========================================================================================
        //
        public static void installCollectionFromLocalRepo_BuildDbFromXmlData(coreClass cpCore, string XMLText, bool isNewBuild, bool isBaseCollection, ref List<string> nonCriticalErrorList) {
            try {
                //
                logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name);
                //
                // ----- Import any CDef files, allowing for changes
                miniCollectionModel miniCollectionToAdd = new miniCollectionModel();
                miniCollectionModel miniCollectionWorking = installCollection_GetApplicationMiniCollection(cpCore, isNewBuild);
                miniCollectionToAdd = installCollection_LoadXmlToMiniCollection(cpCore, XMLText, isBaseCollection, false, isNewBuild, miniCollectionWorking);
                installCollection_AddMiniCollectionSrcToDst(cpCore, ref miniCollectionWorking, miniCollectionToAdd, true);
                installCollection_BuildDbFromMiniCollection(cpCore, miniCollectionWorking, cpCore.siteProperties.dataBuildVersion, isNewBuild, ref nonCriticalErrorList);
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //        '
        //        '=========================================================================================
        //        '
        //        '=========================================================================================
        //        '
        //        Private Sub UpgradeCDef_LoadFileToCollection(ByVal FilePathPage As String, byref return_Collection As CollectionType, ByVal ForceChanges As Boolean, IsNewBuild As Boolean)
        //            On Error GoTo ErrorTrap
        //            '
        //            'Dim fs As New fileSystemClass
        //            Dim IsccBaseFile As Boolean
        //            '
        //            IsccBaseFile = (InStr(1, FilePathPage, "ccBase.xml", vbTextCompare) <> 0)
        //            Call AppendClassLogFile(cpcore.app.config.name, "UpgradeCDef_LoadFileToCollection", "Application: " & cpcore.app.config.name & ", loading [" & FilePathPage & "]")
        //            Call UpgradeCDef_LoadDataToCollection(cpcore.app.publicFiles.ReadFile(FilePathPage), Return_Collection, IsccBaseFile, ForceChanges, IsNewBuild)
        //            '
        //            Exit Sub
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection", True, True)
        //            'dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection hint=[" & Hint & "]", True, True)
        //            //Resume Next
        //        End Sub
        //
        //=========================================================================================
        //   create a collection class from a collection xml file
        //       - cdef are added to the cdefs in the application collection
        //=========================================================================================
        //
        private static miniCollectionModel installCollection_LoadXmlToMiniCollection(coreClass cpCore, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, miniCollectionModel defaultCollection) {
            miniCollectionModel result = null;
            try {
                Models.Complex.cdefModel DefaultCDef = null;
                Models.Complex.CDefFieldModel DefaultCDefField = null;
                string contentNameLc = null;
                collectionXmlController XMLTools = new collectionXmlController(cpCore);
                //Dim AddonClass As New addonInstallClass(cpCore)
                string status = null;
                string CollectionGuid = null;
                string Collectionname = null;
                string ContentTableName = null;
                bool IsNavigator = false;
                string ActiveText = null;
                string Name = "";
                string MenuName = null;
                string IndexName = null;
                string TableName = null;
                int Ptr = 0;
                string FieldName = null;
                string ContentName = null;
                bool Found = false;
                string menuNameSpace = null;
                string MenuGuid = null;
               
                XmlNode CDef_Node = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode CDefChildNode = null;
                string DataSourceName = null;
                XmlDocument srcXmlDom = new XmlDocument();
                string NodeName = null;
                //INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
                //				XmlNode FieldChildNode = null;
                //
                logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name + ", UpgradeCDef_LoadDataToCollection");
                //
                result = new miniCollectionModel();
                //
                if (string.IsNullOrEmpty(srcCollecionXml)) {
                    //
                    // -- empty collection is an error
                    throw (new ApplicationException("UpgradeCDef_LoadDataToCollection, srcCollectionXml is blank or null"));
                } else {
                    try {
                        srcXmlDom.LoadXml(srcCollecionXml);
                    } catch (Exception ex) {
                        //
                        // -- xml load error
                        logController.appendLog(cpCore, "UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" + ex.ToString() + "]");
                        throw new Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    if ((srcXmlDom.DocumentElement.Name.ToLower() != CollectionFileRootNode) & (srcXmlDom.DocumentElement.Name.ToLower() != "contensivecdef")) {
                        //
                        // -- root node must be collection (or legacy contensivecdef)
                        cpCore.handleException(new ApplicationException("the archive file has a syntax error. Application name must be the first node."));
                    } else {
                        result.isBaseCollection = IsccBaseFile;
                        //
                        // Get Collection Name for logs
                        //
                        //hint = "get collection name"
                        Collectionname = GetXMLAttribute(cpCore, Found, srcXmlDom.DocumentElement, "name", "");
                        if (string.IsNullOrEmpty(Collectionname)) {
                            logController.appendInstallLog(cpCore, "UpgradeCDef_LoadDataToCollection, Application: " + cpCore.serverConfig.appConfig.name + ", Collection has no name");
                        } else {
                            //Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & cpcore.app.appEnvironment.name & ", Collection: " & Collectionname)
                        }
                        result.name = Collectionname;
                        //
                        // Load possible DefaultSortMethods
                        //
                        //hint = "preload sort methods"
                        //SortMethodList = vbTab & "By Name" & vbTab & "By Alpha Sort Order Field" & vbTab & "By Date" & vbTab & "By Date Reverse"
                        //If cpCore.app.csv_IsContentFieldSupported("Sort Methods", "ID") Then
                        //    CS = cpCore.app.OpenCSContent("Sort Methods", , , , , , , "Name")
                        //    Do While cpCore.app.IsCSOK(CS)
                        //        SortMethodList = SortMethodList & vbTab & cpCore.app.cs_getText(CS, "name")
                        //        cpCore.app.nextCSRecord(CS)
                        //    Loop
                        //    Call cpCore.app.closeCS(CS)
                        //End If
                        //SortMethodList = SortMethodList & vbTab
                        //
                        foreach (XmlNode CDef_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes) {
                            CDef_Node = CDef_NodeWithinLoop;
                            //isCdefTarget = False
                            NodeName = genericController.vbLCase(CDef_NodeWithinLoop.Name);
                            //hint = "read node " & NodeName
                            switch (NodeName) {
                                case "cdef":
                                    //
                                    // Content Definitions
                                    //
                                    ContentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "name", "");
                                    contentNameLc = genericController.vbLCase(ContentName);
                                    if (string.IsNullOrEmpty(ContentName)) {
                                        throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "collection file contains a CDEF node with no name attribute. This is not allowed.", "dll", "builderClass", "UpgradeCDef_LoadDataToCollection", 0, "", "", False, True, "")
                                    } else {
                                        //
                                        // setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
                                        //
                                        if (defaultCollection.CDef.ContainsKey(contentNameLc)) {
                                            DefaultCDef = defaultCollection.CDef[contentNameLc];
                                        } else {
                                            DefaultCDef = new Models.Complex.cdefModel();
                                        }
                                        //
                                        ContentTableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.ContentTableName);
                                        if (!string.IsNullOrEmpty(ContentTableName)) {
                                            //
                                            // These two fields are needed to import the row
                                            //
                                            DataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "dataSource", DefaultCDef.ContentDataSourceName);
                                            if (string.IsNullOrEmpty(DataSourceName)) {
                                                DataSourceName = "Default";
                                            }
                                            //
                                            // ----- Add CDef if not already there
                                            //
                                            if (!result.CDef.ContainsKey(ContentName.ToLower())) {
                                                result.CDef.Add(ContentName.ToLower(), new Models.Complex.cdefModel());
                                            }
                                            //
                                            // Get CDef attributes
                                            //
                                            Models.Complex.cdefModel tempVar = result.CDef[ContentName.ToLower()];
                                            string activeDefaultText = "1";
                                            if (!(DefaultCDef.Active)) {
                                                activeDefaultText = "0";
                                            }
                                            ActiveText = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Active", activeDefaultText);
                                            if (string.IsNullOrEmpty(ActiveText)) {
                                                ActiveText = "1";
                                            }
                                            tempVar.Active = genericController.encodeBoolean(ActiveText);
                                            tempVar.ActiveOnly = true;
                                            //.adminColumns = ?
                                            tempVar.AdminOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AdminOnly", DefaultCDef.AdminOnly);
                                            tempVar.AliasID = "id";
                                            tempVar.AliasName = "name";
                                            tempVar.AllowAdd = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowAdd", DefaultCDef.AllowAdd);
                                            tempVar.AllowCalendarEvents = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowCalendarEvents", DefaultCDef.AllowCalendarEvents);
                                            tempVar.AllowContentChildTool = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowContentChildTool", DefaultCDef.AllowContentChildTool);
                                            tempVar.AllowContentTracking = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowContentTracking", DefaultCDef.AllowContentTracking);
                                            tempVar.AllowDelete = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowDelete", DefaultCDef.AllowDelete);
                                            tempVar.AllowTopicRules = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AllowTopicRules", DefaultCDef.AllowTopicRules);
                                            tempVar.guid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", DefaultCDef.guid);
                                            tempVar.dataChanged = setAllDataChanged;
                                            tempVar.set_childIdList(cpCore, new List<int>());
                                            tempVar.ContentControlCriteria = "";
                                            tempVar.ContentDataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentDataSourceName", DefaultCDef.ContentDataSourceName);
                                            tempVar.ContentTableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.ContentTableName);
                                            tempVar.dataSourceId = 0;
                                            tempVar.DefaultSortMethod = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
                                            if ((tempVar.DefaultSortMethod == "") || (tempVar.DefaultSortMethod.ToLower() == "name")) {
                                                tempVar.DefaultSortMethod = "By Name";
                                            } else if (genericController.vbLCase(tempVar.DefaultSortMethod) == "sortorder") {
                                                tempVar.DefaultSortMethod = "By Alpha Sort Order Field";
                                            } else if (genericController.vbLCase(tempVar.DefaultSortMethod) == "date") {
                                                tempVar.DefaultSortMethod = "By Date";
                                            }
                                            tempVar.DeveloperOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "DeveloperOnly", DefaultCDef.DeveloperOnly);
                                            tempVar.DropDownFieldList = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "DropDownFieldList", DefaultCDef.DropDownFieldList);
                                            tempVar.EditorGroupName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "EditorGroupName", DefaultCDef.EditorGroupName);
                                            tempVar.fields = new Dictionary<string, Models.Complex.CDefFieldModel>();
                                            tempVar.IconLink = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "IconLink", DefaultCDef.IconLink);
                                            tempVar.IconHeight = GetXMLAttributeInteger(cpCore, Found, CDef_NodeWithinLoop, "IconHeight", DefaultCDef.IconHeight);
                                            tempVar.IconWidth = GetXMLAttributeInteger(cpCore, Found, CDef_NodeWithinLoop, "IconWidth", DefaultCDef.IconWidth);
                                            tempVar.IconSprites = GetXMLAttributeInteger(cpCore, Found, CDef_NodeWithinLoop, "IconSprites", DefaultCDef.IconSprites);
                                            tempVar.IgnoreContentControl = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "IgnoreContentControl", DefaultCDef.IgnoreContentControl);
                                            tempVar.includesAFieldChange = false;
                                            tempVar.installedByCollectionGuid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "installedByCollection", DefaultCDef.installedByCollectionGuid);
                                            tempVar.IsBaseContent = IsccBaseFile || GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "IsBaseContent", false);
                                            tempVar.IsModifiedSinceInstalled = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "IsModified", DefaultCDef.IsModifiedSinceInstalled);
                                            tempVar.Name = ContentName;
                                            tempVar.parentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Parent", DefaultCDef.parentName);
                                            tempVar.WhereClause = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "WhereClause", DefaultCDef.WhereClause);
                                            //
                                            // Get CDef field nodes
                                            //
                                            foreach (XmlNode CDefChildNode in CDef_NodeWithinLoop.ChildNodes) {
                                                //
                                                // ----- process CDef Field
                                                //
                                                if (TextMatch(cpCore, CDefChildNode.Name, "field")) {
                                                    FieldName = GetXMLAttribute(cpCore, Found, CDefChildNode, "Name", "");
                                                    if (FieldName.ToLower() == "middlename") {
                                                        //FieldName = FieldName;
                                                    }
                                                    //
                                                    // try to find field in the defaultcdef
                                                    //
                                                    if (DefaultCDef.fields.ContainsKey(FieldName)) {
                                                        DefaultCDefField = DefaultCDef.fields[FieldName];
                                                    } else {
                                                        DefaultCDefField = new Models.Complex.CDefFieldModel();
                                                    }
                                                    //
                                                    if (!(result.CDef[ContentName.ToLower()].fields.ContainsKey(FieldName.ToLower()))) {
                                                        result.CDef[ContentName.ToLower()].fields.Add(FieldName.ToLower(), new Models.Complex.CDefFieldModel());
                                                    }
                                                    var tempVar2 = result.CDef[ContentName.ToLower()].fields[FieldName.ToLower()];
                                                    tempVar2.nameLc = FieldName.ToLower();
                                                    ActiveText = "0";
                                                    if (DefaultCDefField.active) {
                                                        ActiveText = "1";
                                                    }
                                                    ActiveText = GetXMLAttribute(cpCore, Found, CDefChildNode, "Active", ActiveText);
                                                    if (string.IsNullOrEmpty(ActiveText)) {
                                                        ActiveText = "1";
                                                    }
                                                    tempVar2.active = genericController.encodeBoolean(ActiveText);
                                                    //
                                                    // Convert Field Descriptor (text) to field type (integer)
                                                    //
                                                    string defaultFieldTypeName = cpCore.db.getFieldTypeNameFromFieldTypeId(DefaultCDefField.fieldTypeId);
                                                    string fieldTypeName = GetXMLAttribute(cpCore, Found, CDefChildNode, "FieldType", defaultFieldTypeName);
                                                    tempVar2.fieldTypeId = cpCore.db.getFieldTypeIdFromFieldTypeName(fieldTypeName);
                                                    //FieldTypeDescriptor = GetXMLAttribute(cpcore,Found, CDefChildNode, "FieldType", DefaultCDefField.fieldType)
                                                    //If genericController.vbIsNumeric(FieldTypeDescriptor) Then
                                                    //    .fieldType = genericController.EncodeInteger(FieldTypeDescriptor)
                                                    //Else
                                                    //    .fieldType = cpCore.app.csv_GetFieldTypeByDescriptor(FieldTypeDescriptor)
                                                    //End If
                                                    //If .fieldType = 0 Then
                                                    //    .fieldType = FieldTypeText
                                                    //End If
                                                    tempVar2.editSortPriority = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "EditSortPriority", DefaultCDefField.editSortPriority);
                                                    tempVar2.authorable = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Authorable", DefaultCDefField.authorable);
                                                    tempVar2.caption = GetXMLAttribute(cpCore, Found, CDefChildNode, "Caption", DefaultCDefField.caption);
                                                    tempVar2.defaultValue = GetXMLAttribute(cpCore, Found, CDefChildNode, "DefaultValue", DefaultCDefField.defaultValue);
                                                    tempVar2.NotEditable = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "NotEditable", DefaultCDefField.NotEditable);
                                                    tempVar2.indexColumn = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexColumn", DefaultCDefField.indexColumn);
                                                    tempVar2.indexWidth = GetXMLAttribute(cpCore, Found, CDefChildNode, "IndexWidth", DefaultCDefField.indexWidth);
                                                    tempVar2.indexSortOrder = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "IndexSortOrder", DefaultCDefField.indexSortOrder);
                                                    tempVar2.RedirectID = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectID", DefaultCDefField.RedirectID);
                                                    tempVar2.RedirectPath = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectPath", DefaultCDefField.RedirectPath);
                                                    tempVar2.htmlContent = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "HTMLContent", DefaultCDefField.htmlContent);
                                                    tempVar2.UniqueName = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "UniqueName", DefaultCDefField.UniqueName);
                                                    tempVar2.Password = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Password", DefaultCDefField.Password);
                                                    tempVar2.adminOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "AdminOnly", DefaultCDefField.adminOnly);
                                                    tempVar2.developerOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "DeveloperOnly", DefaultCDefField.developerOnly);
                                                    tempVar2.ReadOnly = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "ReadOnly", DefaultCDefField.ReadOnly);
                                                    tempVar2.Required = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Required", DefaultCDefField.Required);
                                                    tempVar2.RSSTitleField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSTitle", DefaultCDefField.RSSTitleField);
                                                    tempVar2.RSSDescriptionField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "RSSDescriptionField", DefaultCDefField.RSSDescriptionField);
                                                    tempVar2.MemberSelectGroupID = GetXMLAttributeInteger(cpCore, Found, CDefChildNode, "MemberSelectGroupID", DefaultCDefField.MemberSelectGroupID);
                                                    tempVar2.editTabName = GetXMLAttribute(cpCore, Found, CDefChildNode, "EditTab", DefaultCDefField.editTabName);
                                                    tempVar2.Scramble = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "Scramble", DefaultCDefField.Scramble);
                                                    tempVar2.lookupList = GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupList", DefaultCDefField.lookupList);
                                                    tempVar2.ManyToManyRulePrimaryField = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRulePrimaryField", DefaultCDefField.ManyToManyRulePrimaryField);
                                                    tempVar2.ManyToManyRuleSecondaryField = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleSecondaryField", DefaultCDefField.ManyToManyRuleSecondaryField);
                                                    tempVar2.set_lookupContentName(cpCore,GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.get_lookupContentName(cpCore)));
                                                    // isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                    // case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                    // case 2 - when the base file is loaded, all fields must include the attribute
                                                    //Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
                                                    tempVar2.isBaseField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsBaseField", false) || IsccBaseFile;
                                                    tempVar2.set_RedirectContentName(cpCore, GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectContent", DefaultCDefField.get_RedirectContentName(cpCore)));
                                                    tempVar2.set_ManyToManyContentName(cpCore, GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.get_ManyToManyContentName(cpCore)));
                                                    tempVar2.set_ManyToManyRuleContentName(cpCore, GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.get_ManyToManyRuleContentName(cpCore)));
                                                    tempVar2.isModifiedSinceInstalled = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled);
                                                    tempVar2.installedByCollectionGuid = GetXMLAttribute(cpCore, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid);
                                                    tempVar2.dataChanged = setAllDataChanged;
                                                    //
                                                    // ----- handle child nodes (help node)
                                                    //
                                                    tempVar2.HelpCustom = "";
                                                    tempVar2.HelpDefault = "";
                                                    foreach (XmlNode FieldChildNode in CDefChildNode.ChildNodes) {
                                                        //
                                                        // ----- process CDef Field
                                                        //
                                                        if (TextMatch(cpCore, FieldChildNode.Name, "HelpDefault")) {
                                                            tempVar2.HelpDefault = FieldChildNode.InnerText;
                                                        }
                                                        if (TextMatch(cpCore, FieldChildNode.Name, "HelpCustom")) {
                                                            tempVar2.HelpCustom = FieldChildNode.InnerText;
                                                        }
                                                        tempVar2.HelpChanged = setAllDataChanged;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case "sqlindex":
                                    //
                                    // SQL Indexes
                                    //
                                    IndexName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "indexname", "");
                                    TableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "tableName", "");
                                    DataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "DataSourceName", "");
                                    if (string.IsNullOrEmpty(DataSourceName)) {
                                        DataSourceName = "default";
                                    }
                                    bool removeDup = false;
                                    miniCollectionModel.collectionSQLIndexModel dupToRemove = new miniCollectionModel.collectionSQLIndexModel();
                                    foreach (miniCollectionModel.collectionSQLIndexModel index in result.SQLIndexes) {
                                        if (TextMatch(cpCore, index.IndexName, IndexName) & TextMatch(cpCore, index.TableName, TableName) & TextMatch(cpCore, index.DataSourceName, DataSourceName)) {
                                            dupToRemove = index;
                                            removeDup = true;
                                            break;
                                        }
                                    }
                                    if (removeDup) {
                                        result.SQLIndexes.Remove(dupToRemove);
                                    }
                                    miniCollectionModel.collectionSQLIndexModel newIndex = new miniCollectionModel.collectionSQLIndexModel();
                                    newIndex.IndexName = IndexName;
                                    newIndex.TableName = TableName;
                                    newIndex.DataSourceName = DataSourceName;
                                    newIndex.FieldNameList = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "FieldNameList", "");
                                    result.SQLIndexes.Add(newIndex);
                                    break;
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry":
                                    //
                                    // Admin Menus / Navigator Entries
                                    MenuName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
                                    menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NameSpace", "");
                                    MenuGuid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", "");
                                    IsNavigator = (NodeName == "navigatorentry");
                                    string MenuKey = null;
                                    if (!IsNavigator) {
                                        MenuKey = genericController.vbLCase(MenuName);
                                    } else {
                                        MenuKey = MenuGuid;
                                    }
                                    if ( !result.Menus.ContainsKey(MenuKey)) {
                                        ActiveText = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Active", "1");
                                        if (string.IsNullOrEmpty(ActiveText)) {
                                            ActiveText = "1";
                                        }
                                        result.Menus.Add(MenuKey, new miniCollectionModel.collectionMenuModel() {
                                            dataChanged = setAllDataChanged,
                                            Name = MenuName,
                                            Guid = MenuGuid,
                                            Key = MenuKey,
                                            Active = genericController.encodeBoolean(ActiveText),
                                            menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NameSpace", ""),
                                            ParentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ParentName", ""),
                                            ContentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentName", ""),
                                            LinkPage = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "LinkPage", ""),
                                            SortOrder = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "SortOrder", ""),
                                            AdminOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AdminOnly", false),
                                            DeveloperOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "DeveloperOnly", false),
                                            NewWindow = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "NewWindow", false),
                                            AddonName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "AddonName", ""),
                                            NavIconType = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NavIconType", ""),
                                            NavIconTitle = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NavIconTitle", ""),
                                            IsNavigator = IsNavigator
                                        });
                                    }
                                    break;
                                case "aggregatefunction":
                                case "addon":
                                    //
                                    // Aggregate Objects (just make them -- there are not too many
                                    //
                                    Name = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
                                    miniCollectionModel.collectionAddOnModel addon;
                                    if (result.AddOns.ContainsKey(Name.ToLower())) {
                                        addon = result.AddOns[Name.ToLower()];
                                    } else {
                                        addon = new miniCollectionModel.collectionAddOnModel();
                                        result.AddOns.Add(Name.ToLower(), addon);
                                    }
                                    addon.dataChanged = setAllDataChanged;
                                    addon.Link = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Link", "");
                                    addon.ObjectProgramID = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ObjectProgramID", "");
                                    addon.ArgumentList = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ArgumentList", "");
                                    addon.SortOrder = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "SortOrder", "");
                                    addon.Copy = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "copy", "");
                                    break;
                                case "style":
                                    //
                                    // style sheet entries
                                    //
                                    Name = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
                                    if (result.StyleCnt > 0) {
                                        for (Ptr = 0; Ptr < result.StyleCnt; Ptr++) {
                                            if (TextMatch(cpCore, result.Styles[Ptr].Name, Name)) {
                                                break;
                                            }
                                        }
                                    }
                                    if (Ptr >= result.StyleCnt) {
                                        Ptr = result.StyleCnt;
                                        result.StyleCnt = result.StyleCnt + 1;
                                        Array.Resize(ref result.Styles, Ptr);
                                        result.Styles[Ptr].Name = Name;
                                    }
                                    var tempVar5 = result.Styles[Ptr];
                                    tempVar5.dataChanged = setAllDataChanged;
                                    tempVar5.Overwrite = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "Overwrite", false);
                                    tempVar5.Copy = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "stylesheet":
                                    //
                                    // style sheet in one entry
                                    //
                                    result.StyleSheet = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "getcollection":
                                case "importcollection":
                                    if (true) {
                                        //If Not UpgradeDbOnly Then
                                        //
                                        // Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                        //
                                        Collectionname = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "name", "");
                                        CollectionGuid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", "");
                                        if (string.IsNullOrEmpty(CollectionGuid)) {
                                            CollectionGuid = CDef_NodeWithinLoop.InnerText;
                                        }
                                        if (string.IsNullOrEmpty(CollectionGuid)) {
                                            status = "The collection you selected [" + Collectionname + "] can not be downloaded because it does not include a valid GUID.";
                                            //cpCore.AppendLog("builderClass.UpgradeCDef_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
                                        } else {
                                            result.collectionImports.Add(new miniCollectionModel.ImportCollectionType() {
                                                Name = Collectionname,
                                                Guid = CollectionGuid
                                            });
                                        }
                                    }
                                    break;
                                case "pagetemplate":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Page Templates
                                    //-------------------------------------------------------------------------------------------------
                                    // *********************************************************************************
                                    // Page Template - started, but Return_Collection and LoadDataToCDef are all that is done do far
                                    //
                                    if (result.PageTemplateCnt > 0) {
                                        for (Ptr = 0; Ptr < result.PageTemplateCnt; Ptr++) {
                                            if (TextMatch(cpCore, result.PageTemplates[Ptr].Name, Name)) {
                                                break;
                                            }
                                        }
                                    }
                                    if (Ptr >= result.PageTemplateCnt) {
                                        Ptr = result.PageTemplateCnt;
                                        result.PageTemplateCnt = result.PageTemplateCnt + 1;
                                        Array.Resize(ref result.PageTemplates, Ptr);
                                        result.PageTemplates[Ptr].Name = Name;
                                    }
                                    var tempVar6 = result.PageTemplates[Ptr];
                                    tempVar6.Copy = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Copy", "");
                                    tempVar6.Guid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", "");
                                    tempVar6.Style = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "style", "");
                                    //Case "sitesection"
                                    //    '
                                    //    '-------------------------------------------------------------------------------------------------
                                    //    ' Site Sections
                                    //    '-------------------------------------------------------------------------------------------------
                                    //    '
                                    //Case "dynamicmenu"
                                    //    '
                                    //    '-------------------------------------------------------------------------------------------------
                                    //    ' Dynamic Menus
                                    //    '-------------------------------------------------------------------------------------------------
                                    //    '
                                    break;
                                case "pagecontent":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Page Content
                                    //-------------------------------------------------------------------------------------------------
                                    //
                                    break;
                                case "copycontent":
                                    //
                                    //-------------------------------------------------------------------------------------------------
                                    // Copy Content
                                    //-------------------------------------------------------------------------------------------------
                                    //
                                    break;
                            }
                        }
                        //hint = "nodes done"
                        //
                        // Convert Menus.ParentName to Menu.menuNameSpace
                        //
                        foreach ( var kvp in result.Menus) {
                            miniCollectionModel.collectionMenuModel menu = kvp.Value;
                            if ( !string.IsNullOrEmpty( menu.ParentName )) {
                                menu.menuNameSpace = GetMenuNameSpace(cpCore, result.Menus, menu, "");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
        /// </summary>
        private static void installCollection_BuildDbFromMiniCollection(coreClass cpCore, miniCollectionModel Collection, string BuildVersion, bool isNewBuild, ref List<string> nonCriticalErrorList) {
            try {
                //
                int FieldHelpID = 0;
                int FieldHelpCID = 0;
                int fieldId = 0;
                string FieldName = null;
                //Dim AddonClass As addonInstallClass
                string StyleSheetAdd = "";
                string NewStyleValue = null;
                string SiteStyles = null;
                int PosNameLineEnd = 0;
                int PosNameLineStart = 0;
                int SiteStylePtr = 0;
                string StyleLine = null;
                string[] SiteStyleSplit = { };
                int SiteStyleCnt = 0;
                string NewStyleName = null;
                string TestStyleName = null;
                string SQL = null;
                DataTable rs = null;
                string Copy = null;
                string ContentName = null;
                int NodeCount = 0;
                string TableName = null;
                bool RequireReload = false;
                bool Found = false;
                                                   //
                logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name + ", UpgradeCDef_BuildDbFromCollection");
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 0.5: verify core sql tables");
                //----------------------------------------------------------------------------------------------------------------------
                //
                appBuilderController.VerifyBasicTables(cpCore);
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 1: create SQL tables in default datasource");
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (true) {
                    string UsedTables = "";
                    foreach (var keypairvalue in Collection.CDef) {
                        Models.Complex.cdefModel workingCdef = keypairvalue.Value;
                        ContentName = workingCdef.Name;
                        if (workingCdef.dataChanged) {
                            logController.appendInstallLog(cpCore, "creating sql table [" + workingCdef.ContentTableName + "], datasource [" + workingCdef.ContentDataSourceName + "]");
                            if (genericController.vbLCase(workingCdef.ContentDataSourceName) == "default" || workingCdef.ContentDataSourceName == "") {
                                TableName = workingCdef.ContentTableName;
                                if (genericController.vbInstr(1, "," + UsedTables + ",", "," + TableName + ",", 1) != 0) {
                                    //TableName = TableName;
                                } else {
                                    UsedTables = UsedTables + "," + TableName;
                                    cpCore.db.createSQLTable(workingCdef.ContentDataSourceName, TableName);
                                }
                            }
                        }
                    }
                    cpCore.doc.clearMetaData();
                    cpCore.cache.invalidateAll();
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 2: Verify all CDef names in ccContent so GetContentID calls will succeed");
                //----------------------------------------------------------------------------------------------------------------------
                //
                NodeCount = 0;
                List<string> installedContentList = new List<string>();
                rs = cpCore.db.executeQuery("SELECT Name from ccContent where (active<>0)");
                if (isDataTableOk(rs)) {
                    installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
                }
                rs.Dispose();
                //
                foreach (var keypairvalue in Collection.CDef) {
                    if (keypairvalue.Value.dataChanged) {
                        logController.appendInstallLog(cpCore, "adding cdef name [" + keypairvalue.Value.Name + "]");
                        if (!installedContentList.Contains(keypairvalue.Value.Name.ToLower())) {
                            SQL = "Insert into ccContent (name,ccguid,active,createkey)values(" + cpCore.db.encodeSQLText(keypairvalue.Value.Name) + "," + cpCore.db.encodeSQLText(keypairvalue.Value.guid) + ",1,0);";
                            cpCore.db.executeQuery(SQL);
                            installedContentList.Add(keypairvalue.Value.Name.ToLower());
                            RequireReload = true;
                        }
                    }
                }
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 4: Verify content records required for Content Server");
                //----------------------------------------------------------------------------------------------------------------------
                //
                VerifySortMethods(cpCore);
                VerifyContentFieldTypes(cpCore);
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 5: verify 'Content' content definition");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.CDef) {
                    if (keypairvalue.Value.Name.ToLower() == "content") {
                        logController.appendInstallLog(cpCore, "adding cdef [" + keypairvalue.Value.Name + "]");
                        installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion);
                        RequireReload = true;
                        break;
                    }
                }
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 6.1: Verify all definitions and fields");
                //----------------------------------------------------------------------------------------------------------------------
                //
                RequireReload = false;
                foreach (var keypairvalue in Collection.CDef) {
                    //
                    // todo tmp fix, changes to field caption in base.xml do not set fieldChange
                    if (true) // If .dataChanged Or .includesAFieldChange Then
                    {
                        if (keypairvalue.Value.Name.ToLower() != "content") {
                            logController.appendInstallLog(cpCore, "adding cdef [" + keypairvalue.Value.Name + "]");
                            installCollection_BuildDbFromCollection_AddCDefToDb(cpCore, keypairvalue.Value, BuildVersion);
                            RequireReload = true;
                        }
                    }
                }
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 6.2: Verify all field help");
                //----------------------------------------------------------------------------------------------------------------------
                //
                FieldHelpCID = cpCore.db.getRecordID("content", "Content Field Help");
                foreach (var keypairvalue in Collection.CDef) {
                    Models.Complex.cdefModel workingCdef = keypairvalue.Value;
                    ContentName = workingCdef.Name;
                    foreach (var fieldKeyValuePair in workingCdef.fields) {
                        Models.Complex.CDefFieldModel field = fieldKeyValuePair.Value;
                        FieldName = field.nameLc;
                        var tempVar = Collection.CDef[ContentName.ToLower()].fields[FieldName.ToLower()];
                        if (tempVar.HelpChanged) {
                            fieldId = 0;
                            SQL = "select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" + cpCore.db.encodeSQLText(FieldName) + ")and(c.name=" + cpCore.db.encodeSQLText(ContentName) + ") order by f.id";
                            rs = cpCore.db.executeQuery(SQL);
                            if (isDataTableOk(rs)) {
                                fieldId = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "id"));
                            }
                            rs.Dispose();
                            if (fieldId == 0) {
                                throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not update help field for content [" & ContentName & "], field [" & FieldName & "] because the field was not found in the Db.", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection", 0, "", "", False, True, "")
                            } else {
                                SQL = "select id from ccfieldhelp where fieldid=" + fieldId + " order by id";
                                rs = cpCore.db.executeQuery(SQL);
                                if (isDataTableOk(rs)) {
                                    FieldHelpID = genericController.EncodeInteger(rs.Rows[0]["id"]);
                                } else {
                                    FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccfieldhelp", 0);
                                }
                                rs.Dispose();
                                if (FieldHelpID != 0) {
                                    Copy = tempVar.HelpCustom;
                                    if (string.IsNullOrEmpty(Copy)) {
                                        Copy = tempVar.HelpDefault;
                                        if (!string.IsNullOrEmpty(Copy)) {
                                            //Copy = Copy;
                                        }
                                    }
                                    SQL = "update ccfieldhelp set active=1,contentcontrolid=" + FieldHelpCID + ",fieldid=" + fieldId + ",helpdefault=" + cpCore.db.encodeSQLText(Copy) + " where id=" + FieldHelpID;
                                    cpCore.db.executeQuery(SQL);
                                }
                            }
                        }
                    }
                }
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 7: create SQL indexes");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (miniCollectionModel.collectionSQLIndexModel index in Collection.SQLIndexes) {
                    if (index.dataChanged) {
                        logController.appendInstallLog(cpCore, "creating index [" + index.IndexName + "], fields [" + index.FieldNameList + "], on table [" + index.TableName + "]");
                        cpCore.db.createSQLIndex(index.DataSourceName, index.TableName, index.IndexName, index.FieldNameList);
                    }
                }
                cpCore.doc.clearMetaData();
                cpCore.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 8a: Verify All Menu Names, then all Menus");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var kvp in Collection.Menus) {
                    var menu = kvp.Value;
                    if (menu.dataChanged) {
                        logController.appendInstallLog(cpCore, "creating navigator entry [" + menu.Name + "], namespace [" + menu.menuNameSpace + "], guid [" + menu.Guid + "]");
                        appBuilderController.verifyNavigatorEntry(cpCore, menu.Guid, menu.menuNameSpace, menu.Name, menu.ContentName, menu.LinkPage, menu.SortOrder, menu.AdminOnly, menu.DeveloperOnly, menu.NewWindow, menu.Active, menu.AddonName, menu.NavIconType, menu.NavIconTitle, 0);
                    }
                }
                //
                //---------------------------------------------------------------------
                // ----- Upgrade collections added during upgrade process
                //---------------------------------------------------------------------
                //
                string errorMessage = "";
                string Guid = null;
                string CollectionPath = "";
                DateTime lastChangeDate = new DateTime();
                logController.appendInstallLog(cpCore, "Installing Add-on Collections gathered during upgrade");
                foreach( var import in Collection.collectionImports) {
                    errorMessage = "";
                    String emptyString = "";
                    GetCollectionConfig(cpCore, import.Guid , ref CollectionPath, ref lastChangeDate, ref emptyString);
                    if (!string.IsNullOrEmpty(CollectionPath)) {
                        //
                        // This collection is installed locally, install from local collections
                        //
                        installCollectionFromLocalRepo(cpCore, Guid, cpCore.codeVersion(), ref errorMessage, "", isNewBuild, ref nonCriticalErrorList);
                    } else {
                        //
                        // This is a new collection, install to the server and force it on this site
                        //
                        bool addonInstallOk = installCollectionFromRemoteRepo(cpCore, Guid, ref errorMessage, "", isNewBuild, ref nonCriticalErrorList);
                        if (!addonInstallOk) {
                            throw (new ApplicationException("Failure to install addon collection from remote repository. Collection [" + Guid + "] was referenced in collection [" + Collection.name + "]")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error upgrading Addon Collection [" & Guid & "], " & errorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                        }

                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                logController.appendInstallLog(cpCore, "CDef Load, stage 9: Verify Styles");
                //----------------------------------------------------------------------------------------------------------------------
                //
                NodeCount = 0;
                if (Collection.StyleCnt > 0) {
                    SiteStyles = cpCore.cdnFiles.readFile("templates/styles.css");
                    if (!string.IsNullOrEmpty(SiteStyles.Trim(' '))) {
                        //
                        // Split with an extra character at the end to guarantee there is an extra split at the end
                        //
                        SiteStyleSplit = (SiteStyles + " ").Split('}');
                        SiteStyleCnt = SiteStyleSplit.GetUpperBound(0) + 1;
                    }
                    for (var Ptr = 0; Ptr < Collection.StyleCnt; Ptr++) {
                        Found = false;
                        var tempVar4 = Collection.Styles[Ptr];
                        if (tempVar4.dataChanged) {
                            NewStyleName = tempVar4.Name;
                            NewStyleValue = tempVar4.Copy;
                            NewStyleValue = genericController.vbReplace(NewStyleValue, "}", "");
                            NewStyleValue = genericController.vbReplace(NewStyleValue, "{", "");
                            if (SiteStyleCnt > 0) {
                                for (SiteStylePtr = 0; SiteStylePtr < SiteStyleCnt; SiteStylePtr++) {
                                    StyleLine = SiteStyleSplit[SiteStylePtr];
                                    PosNameLineEnd = StyleLine.LastIndexOf("{") + 1;
                                    if (PosNameLineEnd > 0) {
                                        PosNameLineStart = StyleLine.LastIndexOf("\r\n", PosNameLineEnd - 1) + 1;
                                        if (PosNameLineStart > 0) {
                                            //
                                            // Check this site style for a match with the NewStyleName
                                            //
                                            PosNameLineStart = PosNameLineStart + 2;
                                            TestStyleName = (StyleLine.Substring(PosNameLineStart - 1, PosNameLineEnd - PosNameLineStart)).Trim(' ');
                                            if (genericController.vbLCase(TestStyleName) == genericController.vbLCase(NewStyleName)) {
                                                Found = true;
                                                if (tempVar4.Overwrite) {
                                                    //
                                                    // Found - Update style
                                                    //
                                                    SiteStyleSplit[SiteStylePtr] = "\r\n" + tempVar4.Name + " {" + NewStyleValue;
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            //
                            // Add or update the stylesheet
                            //
                            if (!Found) {
                                StyleSheetAdd = StyleSheetAdd + "\r\n" + NewStyleName + " {" + NewStyleValue + "}";
                            }
                        }
                    }
                    SiteStyles = string.Join("}", SiteStyleSplit);
                    if (!string.IsNullOrEmpty(StyleSheetAdd)) {
                        SiteStyles = SiteStyles + "\r\n\r\n/*"
                        + "\r\nStyles added " + DateTime.Now + "\r\n*/"
                        + "\r\n" + StyleSheetAdd;
                    }
                    cpCore.appRootFiles.saveFile("templates/styles.css", SiteStyles);
                    //
                    // Update stylesheet cache
                    //
                    cpCore.siteProperties.setProperty("StylesheetSerialNumber", "-1");
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
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Update a table from a collection cdef node
        /// </summary>
        private static void installCollection_BuildDbFromCollection_AddCDefToDb(coreClass cpCore, Models.Complex.cdefModel cdef, string BuildVersion) {
            try {
                //
                int FieldHelpCID = 0;
                int FieldHelpID = 0;
                int fieldId = 0;
                int ContentID = 0;
                DataTable rs = null;
                int EditorGroupID = 0;
                int FieldCount = 0;
                int FieldSize = 0;
                string ContentName = null;
                //Dim DataSourceName As String
                string SQL = null;
                bool ContentIsBaseContent = false;
                //
                logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name + ", UpgradeCDef_BuildDbFromCollection_AddCDefToDb");
                //
                if (!(false)) {
                    //
                    logController.appendInstallLog(cpCore, "Upgrading CDef [" + cdef.Name + "]");
                    //
                    ContentID = 0;
                    ContentName = cdef.Name;
                    ContentIsBaseContent = false;
                    FieldHelpCID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Help");
                    var tmpList = new List<string> { };
                    Contensive.Core.Models.Entity.dataSourceModel datasource = dataSourceModel.createByName(cpCore, cdef.ContentDataSourceName, ref tmpList);
                    //
                    // get contentid and protect content with IsBaseContent true
                    //
                    SQL = cpCore.db.GetSQLSelect("default", "ccContent", "ID,IsBaseContent", "name=" + cpCore.db.encodeSQLText(ContentName), "ID","", 1);
                    rs = cpCore.db.executeQuery(SQL);
                    if (isDataTableOk(rs)) {
                        if (rs.Rows.Count > 0) {
                            //EditorGroupID = cpcore.app.getDataRowColumnName(RS.rows(0), "ID")
                            ContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                            ContentIsBaseContent = genericController.encodeBoolean(cpCore.db.getDataRowColumnName(rs.Rows[0], "IsBaseContent"));
                        }
                    }
                    rs.Dispose();
                    //
                    // ----- Update Content Record
                    //
                    if (cdef.dataChanged) {
                        //
                        // Content needs to be updated
                        //
                        if (ContentIsBaseContent && !cdef.IsBaseContent) {
                            //
                            // Can not update a base content with a non-base content
                            //
                            cpCore.handleException(new ApplicationException("Warning: An attempt was made to update Content Definition [" + cdef.Name + "] from base to non-base. This should only happen when a base cdef is removed from the base collection. The update was ignored."));
                            cdef.IsBaseContent = ContentIsBaseContent;
                            //cpCore.handleLegacyError3( "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
                        }
                        //
                        // ----- update definition (use SingleRecord as an update flag)
                        //
                        Models.Complex.cdefModel.addContent(cpCore, true, datasource, cdef.ContentTableName, ContentName, cdef.AdminOnly, cdef.DeveloperOnly, cdef.AllowAdd, cdef.AllowDelete, cdef.parentName, cdef.DefaultSortMethod, cdef.DropDownFieldList, false, cdef.AllowCalendarEvents, cdef.AllowContentTracking, cdef.AllowTopicRules, cdef.AllowContentChildTool, false, cdef.IconLink, cdef.IconWidth, cdef.IconHeight, cdef.IconSprites, cdef.guid, cdef.IsBaseContent, cdef.installedByCollectionGuid);
                        if (ContentID == 0) {
                            logController.appendInstallLog(cpCore, "Could not determine contentid after createcontent3 for [" + ContentName + "], upgrade for this cdef aborted.");
                        } else {
                            //
                            // ----- Other fields not in the csv call
                            //
                            EditorGroupID = 0;
                            if (cdef.EditorGroupName != "") {
                                rs = cpCore.db.executeQuery("select ID from ccGroups where name=" + cpCore.db.encodeSQLText(cdef.EditorGroupName));
                                if (isDataTableOk(rs)) {
                                    if (rs.Rows.Count > 0) {
                                        EditorGroupID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                                    }
                                }
                                rs.Dispose();
                            }
                            SQL = "update ccContent"
                                + " set EditorGroupID=" + EditorGroupID + ",isbasecontent=" + cpCore.db.encodeSQLBoolean(cdef.IsBaseContent) + " where id=" + ContentID + "";
                            cpCore.db.executeQuery(SQL);
                        }
                    }
                    //
                    // ----- update Content Field Records and Content Field Help records
                    //
                    if (ContentID == 0 && (cdef.fields.Count > 0)) {
                        //
                        // CAn not add fields if there is no content record
                        //
                        throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not add field records to content [" & ContentName & "] because the content definition was not found", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
                    } else {
                        //
                        //
                        //
                        FieldSize = 0;
                        FieldCount = 0;
                        foreach (var nameValuePair in cdef.fields) {
                            Models.Complex.CDefFieldModel field = nameValuePair.Value;
                            if (field.dataChanged) {
                                fieldId = Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field);
                            }
                            //
                            // ----- update content field help records
                            //
                            if (field.HelpChanged) {
                                rs = cpCore.db.executeQuery("select ID from ccFieldHelp where fieldid=" + fieldId);
                                if (isDataTableOk(rs)) {
                                    if (rs.Rows.Count > 0) {
                                        FieldHelpID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows[0], "ID"));
                                    }
                                }
                                rs.Dispose();
                                //
                                if (FieldHelpID == 0) {
                                    FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccFieldHelp", 0);
                                }
                                if (FieldHelpID != 0) {
                                    SQL = "update ccfieldhelp"
                                        + " set fieldid=" + fieldId + ",active=1"
                                        + ",contentcontrolid=" + FieldHelpCID + ",helpdefault=" + cpCore.db.encodeSQLText(field.HelpDefault) + ",helpcustom=" + cpCore.db.encodeSQLText(field.HelpCustom) + " where id=" + FieldHelpID;
                                    cpCore.db.executeQuery(SQL);
                                }
                            }
                        }
                        //
                        // started doing something here -- research it.!!!!!
                        //
                        //For FieldPtr = 0 To .fields.Count - 1
                        //    fieldId = 0
                        //    With .fields(FieldPtr)
                        //    End With
                        //Next
                        //
                        // clear the cdef cache and list
                        //
                        cpCore.doc.clearMetaData();
                        cpCore.cache.invalidateAll();
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
        }
    }
}
