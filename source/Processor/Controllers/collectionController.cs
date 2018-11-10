
using System;
using System.Xml;
using System.Collections.Generic;
using Contensive.Processor.Models.Db;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
using System.IO;
using System.Data;
using System.Threading;
using Contensive.Processor.Models.Domain;
using System.Linq;
using static Contensive.BaseClasses.CPFileSystemBaseClass;

namespace Contensive.Processor.Controllers {
    // todo: rework how adds are installed, this change can be done after weave launch
    // - current addon folder is called local addon folder and not in shared environment /local/addons
    // - add a node to the (local) collection.xml with last collection installation datetime (files added after this starts install)
    // - in private files, new folder with zip files to install /private/collectionInstall
    // - local server checks the list and runs install on new zips, if remote file system, download and install
    // - addon manager just copies zip file into the /private/collectionInstall folder
    //
    // todo -- To make it easy to add code to a site, be able to upload DLL files. Get the class names, find the collection and install in the correct collection folder
    //
    // todo -- Even in collection files, auto discover DLL file classes and create addons out of them. Create/update collections, create collection xml and install.
    //
    //====================================================================================================
    /// <summary>
    /// install addon collections.
    /// </summary>
    public class CollectionController {
        //
        //====================================================================================================
        /// <summary>
        /// Overlay a Src CDef on to the current one (Dst). Any Src CDEf entries found in Src are added to Dst.
        /// if SrcIsUserCDef is true, then the Src is overlayed on the Dst if there are any changes -- and .CDefChanged flag set
        /// </summary>
        private static bool addMiniCollectionSrcToDst(CoreController core, ref MiniCollectionModel dstCollection, MiniCollectionModel srcCollection) {
            bool returnOk = true;
            try {
                string SrcFieldName = null;
                bool updateDst = false;
                Models.Domain.CDefModel srcCdef = null;
                //
                // If the Src is the BaseCollection, the Dst must be the Application Collectio
                //   in this case, reset any BaseContent or BaseField attributes in the application that are not in the base
                //
                if (srcCollection.isBaseCollection) {
                    foreach (var dstKeyValuePair in dstCollection.cdef) {
                        Models.Domain.CDefModel dstWorkingCdef = dstKeyValuePair.Value;
                        string contentName = dstWorkingCdef.name;
                        if (dstCollection.cdef[contentName.ToLower()].isBaseContent) {
                            //
                            // this application collection Cdef is marked base, verify it is in the base collection
                            //
                            if (!srcCollection.cdef.ContainsKey(contentName.ToLower())) {
                                //
                                // cdef in dst is marked base, but it is not in the src collection, reset the cdef.isBaseContent and all field.isbasefield
                                //
                                var tempVar = dstCollection.cdef[contentName.ToLower()];
                                tempVar.isBaseContent = false;
                                tempVar.dataChanged = true;
                                foreach (var dstFieldKeyValuePair in tempVar.fields) {
                                    Models.Domain.CDefFieldModel field = dstFieldKeyValuePair.Value;
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
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", UpgradeCDef_AddSrcToDst");
                string dstName = null;
                //
                foreach (var srcKeyValuePair in srcCollection.cdef) {
                    srcCdef = srcKeyValuePair.Value;
                    string srcName = srcCdef.name;
                    //
                    // Search for this cdef in the Dst
                    //
                    updateDst = false;
                    Models.Domain.CDefModel dstCdef = null;
                    if (!dstCollection.cdef.ContainsKey(srcName.ToLower())) {
                        //
                        // add src to dst
                        //
                        dstCdef = new Models.Domain.CDefModel();
                        dstCollection.cdef.Add(srcName.ToLower(), dstCdef);
                        updateDst = true;
                    } else {
                        dstCdef = dstCollection.cdef[srcName.ToLower()];
                        dstName = srcName;
                        //
                        // found a match between Src and Dst
                        //
                        if (dstCdef.isBaseContent == srcCdef.isBaseContent) {
                            //
                            // Allow changes to user cdef only from user cdef, changes to base only from base
                            updateDst |= (dstCdef.activeOnly != srcCdef.activeOnly);
                            updateDst |= (dstCdef.adminOnly != srcCdef.adminOnly);
                            updateDst |= (dstCdef.developerOnly != srcCdef.developerOnly);
                            updateDst |= (dstCdef.allowAdd != srcCdef.allowAdd);
                            updateDst |= (dstCdef.allowCalendarEvents != srcCdef.allowCalendarEvents);
                            updateDst |= (dstCdef.allowContentTracking != srcCdef.allowContentTracking);
                            updateDst |= (dstCdef.allowDelete != srcCdef.allowDelete);
                            updateDst |= (dstCdef.allowTopicRules != srcCdef.allowTopicRules);
                            updateDst |= !textMatch(dstCdef.dataSourceName, srcCdef.dataSourceName);
                            updateDst |= !textMatch(dstCdef.tableName, srcCdef.tableName);
                            updateDst |= !textMatch(dstCdef.defaultSortMethod, srcCdef.defaultSortMethod);
                            updateDst |= !textMatch(dstCdef.dropDownFieldList, srcCdef.dropDownFieldList);
                            updateDst |= !textMatch(dstCdef.editorGroupName, srcCdef.editorGroupName);
                            updateDst |= (dstCdef.ignoreContentControl != srcCdef.ignoreContentControl);
                            updateDst |= (dstCdef.active != srcCdef.active);
                            updateDst |= (dstCdef.allowContentChildTool != srcCdef.allowContentChildTool);
                            updateDst |= (dstCdef.parentID != srcCdef.parentID);
                            updateDst |= !textMatch(dstCdef.iconLink, srcCdef.iconLink);
                            updateDst |= (dstCdef.iconHeight != srcCdef.iconHeight);
                            updateDst |= (dstCdef.iconWidth != srcCdef.iconWidth);
                            updateDst |= (dstCdef.iconSprites != srcCdef.iconSprites);
                            updateDst |= !textMatch(dstCdef.installedByCollectionGuid, srcCdef.installedByCollectionGuid);
                            updateDst |= !textMatch(dstCdef.guid, srcCdef.guid);
                            updateDst |= (dstCdef.isBaseContent != srcCdef.isBaseContent);
                        }
                    }
                    if (updateDst) {
                        //
                        // update the Dst with the Src
                        dstCdef.active = srcCdef.active;
                        dstCdef.activeOnly = srcCdef.activeOnly;
                        dstCdef.adminOnly = srcCdef.adminOnly;
                        dstCdef.aliasID = srcCdef.aliasID;
                        dstCdef.aliasName = srcCdef.aliasName;
                        dstCdef.allowAdd = srcCdef.allowAdd;
                        dstCdef.allowCalendarEvents = srcCdef.allowCalendarEvents;
                        dstCdef.allowContentChildTool = srcCdef.allowContentChildTool;
                        dstCdef.allowContentTracking = srcCdef.allowContentTracking;
                        dstCdef.allowDelete = srcCdef.allowDelete;
                        dstCdef.allowTopicRules = srcCdef.allowTopicRules;
                        dstCdef.guid = srcCdef.guid;
                        dstCdef.contentControlCriteria = srcCdef.contentControlCriteria;
                        dstCdef.dataSourceName = srcCdef.dataSourceName;
                        dstCdef.tableName = srcCdef.tableName;
                        dstCdef.dataSourceId = srcCdef.dataSourceId;
                        dstCdef.defaultSortMethod = srcCdef.defaultSortMethod;
                        dstCdef.developerOnly = srcCdef.developerOnly;
                        dstCdef.dropDownFieldList = srcCdef.dropDownFieldList;
                        dstCdef.editorGroupName = srcCdef.editorGroupName;
                        dstCdef.iconHeight = srcCdef.iconHeight;
                        dstCdef.iconLink = srcCdef.iconLink;
                        dstCdef.iconSprites = srcCdef.iconSprites;
                        dstCdef.iconWidth = srcCdef.iconWidth;
                        dstCdef.ignoreContentControl = srcCdef.ignoreContentControl;
                        dstCdef.installedByCollectionGuid = srcCdef.installedByCollectionGuid;
                        dstCdef.isBaseContent = srcCdef.isBaseContent;
                        dstCdef.isModifiedSinceInstalled = srcCdef.isModifiedSinceInstalled;
                        dstCdef.name = srcCdef.name;
                        dstCdef.parentID = srcCdef.parentID;
                        dstCdef.parentName = srcCdef.parentName;
                        dstCdef.selectCommaList = srcCdef.selectCommaList;
                        dstCdef.whereClause = srcCdef.whereClause;
                        dstCdef.includesAFieldChange = true;
                        dstCdef.dataChanged = true;
                    }
                    //
                    // Now check each of the field records for an addition, or a change
                    // DstPtr is still set to the Dst CDef
                    //
                    //Call AppendClassLogFile(core.app.config.name,"UpgradeCDef_AddSrcToDst", "CollectionSrc.CDef[SrcPtr].fields.count=" & CollectionSrc.CDef[SrcPtr].fields.count)
                    foreach (var srcFieldKeyValuePair in srcCdef.fields) {
                        Models.Domain.CDefFieldModel srcCdefField = srcFieldKeyValuePair.Value;
                        SrcFieldName = srcCdefField.nameLc;
                        updateDst = false;
                        if (!dstCollection.cdef.ContainsKey(srcName.ToLower())) {
                            //
                            // should have been the collection
                            //
                            throw (new ApplicationException("ERROR - cannot update destination content because it was not found after being added."));
                        } else {
                            dstCdef = dstCollection.cdef[srcName.ToLower()];
                            bool HelpChanged = false;
                            Models.Domain.CDefFieldModel dstCdefField = null;
                            if (dstCdef.fields.ContainsKey(SrcFieldName.ToLower())) {
                                //
                                // Src field was found in Dst fields
                                //

                                dstCdefField = dstCdef.fields[SrcFieldName.ToLower()];
                                updateDst = false;
                                if (dstCdefField.isBaseField == srcCdefField.isBaseField) {
                                    updateDst |= (srcCdefField.active != dstCdefField.active);
                                    updateDst |= (srcCdefField.adminOnly != dstCdefField.adminOnly);
                                    updateDst |= (srcCdefField.authorable != dstCdefField.authorable);
                                    updateDst |= !textMatch(srcCdefField.caption, dstCdefField.caption);
                                    updateDst |= (srcCdefField.contentId != dstCdefField.contentId);
                                    updateDst |= (srcCdefField.developerOnly != dstCdefField.developerOnly);
                                    updateDst |= (srcCdefField.editSortPriority != dstCdefField.editSortPriority);
                                    updateDst |= !textMatch(srcCdefField.editTabName, dstCdefField.editTabName);
                                    updateDst |= (srcCdefField.fieldTypeId != dstCdefField.fieldTypeId);
                                    updateDst |= (srcCdefField.htmlContent != dstCdefField.htmlContent);
                                    updateDst |= (srcCdefField.indexColumn != dstCdefField.indexColumn);
                                    updateDst |= (srcCdefField.indexSortDirection != dstCdefField.indexSortDirection);
                                    updateDst |= (encodeInteger(srcCdefField.indexSortOrder) != GenericController.encodeInteger(dstCdefField.indexSortOrder));
                                    updateDst |= !textMatch(srcCdefField.indexWidth, dstCdefField.indexWidth);
                                    updateDst |= (srcCdefField.lookupContentID != dstCdefField.lookupContentID);
                                    updateDst |= !textMatch(srcCdefField.lookupList, dstCdefField.lookupList);
                                    updateDst |= (srcCdefField.manyToManyContentID != dstCdefField.manyToManyContentID);
                                    updateDst |= (srcCdefField.manyToManyRuleContentID != dstCdefField.manyToManyRuleContentID);
                                    updateDst |= !textMatch(srcCdefField.ManyToManyRulePrimaryField, dstCdefField.ManyToManyRulePrimaryField);
                                    updateDst |= !textMatch(srcCdefField.ManyToManyRuleSecondaryField, dstCdefField.ManyToManyRuleSecondaryField);
                                    updateDst |= (srcCdefField.memberSelectGroupId_get(core) != dstCdefField.memberSelectGroupId_get(core));
                                    updateDst |= (srcCdefField.notEditable != dstCdefField.notEditable);
                                    updateDst |= (srcCdefField.password != dstCdefField.password);
                                    updateDst |= (srcCdefField.readOnly != dstCdefField.readOnly);
                                    updateDst |= (srcCdefField.redirectContentID != dstCdefField.redirectContentID);
                                    updateDst |= !textMatch(srcCdefField.redirectID, dstCdefField.redirectID);
                                    updateDst |= !textMatch(srcCdefField.redirectPath, dstCdefField.redirectPath);
                                    updateDst |= (srcCdefField.required != dstCdefField.required);
                                    updateDst |= (srcCdefField.RSSDescriptionField != dstCdefField.RSSDescriptionField);
                                    updateDst |= (srcCdefField.RSSTitleField != dstCdefField.RSSTitleField);
                                    updateDst |= (srcCdefField.Scramble != dstCdefField.Scramble);
                                    updateDst |= (srcCdefField.textBuffered != dstCdefField.textBuffered);
                                    updateDst |= (GenericController.encodeText(srcCdefField.defaultValue) != GenericController.encodeText(dstCdefField.defaultValue));
                                    updateDst |= (srcCdefField.uniqueName != dstCdefField.uniqueName);
                                    updateDst |= (srcCdefField.isBaseField != dstCdefField.isBaseField);
                                    updateDst |= !textMatch(srcCdefField.get_lookupContentName(core), dstCdefField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_lookupContentName(core), dstCdefField.get_lookupContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_manyToManyRuleContentName(core), dstCdefField.get_manyToManyRuleContentName(core));
                                    updateDst |= !textMatch(srcCdefField.get_redirectContentName(core), dstCdefField.get_redirectContentName(core));
                                    updateDst |= !textMatch(srcCdefField.installedByCollectionGuid, dstCdefField.installedByCollectionGuid);
                                }
                                //
                                // Check Help fields, track changed independantly so frequent help changes will not force timely cdef loads
                                //
                                bool HelpCustomChanged = !textMatch(srcCdefField.helpCustom, srcCdefField.helpCustom);
                                bool HelpDefaultChanged = !textMatch(srcCdefField.helpDefault, srcCdefField.helpDefault);
                                HelpChanged = HelpDefaultChanged || HelpCustomChanged;
                            } else {
                                //
                                // field was not found in dst, add it and populate
                                //
                                dstCdef.fields.Add(SrcFieldName.ToLower(), new Models.Domain.CDefFieldModel());
                                dstCdefField = dstCdef.fields[SrcFieldName.ToLower()];
                                updateDst = true;
                                HelpChanged = true;
                            }
                            //
                            // If okToUpdateDstFromSrc, update the Dst record with the Src record
                            //
                            if (updateDst) {
                                //
                                // Update Fields
                                //
                                dstCdefField.active = srcCdefField.active;
                                dstCdefField.adminOnly = srcCdefField.adminOnly;
                                dstCdefField.authorable = srcCdefField.authorable;
                                dstCdefField.caption = srcCdefField.caption;
                                dstCdefField.contentId = srcCdefField.contentId;
                                dstCdefField.defaultValue = srcCdefField.defaultValue;
                                dstCdefField.developerOnly = srcCdefField.developerOnly;
                                dstCdefField.editSortPriority = srcCdefField.editSortPriority;
                                dstCdefField.editTabName = srcCdefField.editTabName;
                                dstCdefField.fieldTypeId = srcCdefField.fieldTypeId;
                                dstCdefField.htmlContent = srcCdefField.htmlContent;
                                dstCdefField.indexColumn = srcCdefField.indexColumn;
                                dstCdefField.indexSortDirection = srcCdefField.indexSortDirection;
                                dstCdefField.indexSortOrder = srcCdefField.indexSortOrder;
                                dstCdefField.indexWidth = srcCdefField.indexWidth;
                                dstCdefField.lookupContentID = srcCdefField.lookupContentID;
                                dstCdefField.lookupList = srcCdefField.lookupList;
                                dstCdefField.manyToManyContentID = srcCdefField.manyToManyContentID;
                                dstCdefField.manyToManyRuleContentID = srcCdefField.manyToManyRuleContentID;
                                dstCdefField.ManyToManyRulePrimaryField = srcCdefField.ManyToManyRulePrimaryField;
                                dstCdefField.ManyToManyRuleSecondaryField = srcCdefField.ManyToManyRuleSecondaryField;
                                dstCdefField.memberSelectGroupId_set(core, srcCdefField.memberSelectGroupId_get(core));
                                dstCdefField.nameLc = srcCdefField.nameLc;
                                dstCdefField.notEditable = srcCdefField.notEditable;
                                dstCdefField.password = srcCdefField.password;
                                dstCdefField.readOnly = srcCdefField.readOnly;
                                dstCdefField.redirectContentID = srcCdefField.redirectContentID;
                                dstCdefField.redirectID = srcCdefField.redirectID;
                                dstCdefField.redirectPath = srcCdefField.redirectPath;
                                dstCdefField.required = srcCdefField.required;
                                dstCdefField.RSSDescriptionField = srcCdefField.RSSDescriptionField;
                                dstCdefField.RSSTitleField = srcCdefField.RSSTitleField;
                                dstCdefField.Scramble = srcCdefField.Scramble;
                                dstCdefField.textBuffered = srcCdefField.textBuffered;
                                dstCdefField.uniqueName = srcCdefField.uniqueName;
                                dstCdefField.isBaseField = srcCdefField.isBaseField;
                                dstCdefField.set_lookupContentName(core, srcCdefField.get_lookupContentName(core));
                                dstCdefField.set_manyToManyContentName(core, srcCdefField.get_manyToManyContentName(core));
                                dstCdefField.set_manyToManyRuleContentName(core, srcCdefField.get_manyToManyRuleContentName(core));
                                dstCdefField.set_redirectContentName(core, srcCdefField.get_redirectContentName(core));
                                dstCdefField.installedByCollectionGuid = srcCdefField.installedByCollectionGuid;
                                dstCdefField.dataChanged = true;
                                dstCdef.includesAFieldChange = true;
                            }
                            if (HelpChanged) {
                                dstCdefField.helpCustom = srcCdefField.helpCustom;
                                dstCdefField.helpDefault = srcCdefField.helpDefault;
                                dstCdefField.HelpChanged = true;
                            }
                        }
                    }
                }
                //
                // -------------------------------------------------------------------------------------------------
                // Check SQL Indexes
                // -------------------------------------------------------------------------------------------------
                //
                foreach (MiniCollectionModel.miniCollectionSQLIndexModel srcSqlIndex in srcCollection.sqlIndexes) {
                    string srcName = (srcSqlIndex.DataSourceName + "-" + srcSqlIndex.TableName + "-" + srcSqlIndex.IndexName).ToLower();
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    bool indexFound = false;
                    bool indexChanged = false;
                    MiniCollectionModel.miniCollectionSQLIndexModel indexToUpdate = new MiniCollectionModel.miniCollectionSQLIndexModel() { };
                    foreach (MiniCollectionModel.miniCollectionSQLIndexModel dstSqlIndex in dstCollection.sqlIndexes) {
                        dstName = (dstSqlIndex.DataSourceName + "-" + dstSqlIndex.TableName + "-" + dstSqlIndex.IndexName).ToLower();
                        if (textMatch(dstName, srcName)) {
                            //
                            // found a match between Src and Dst
                            indexFound = true;
                            indexToUpdate = dstSqlIndex;
                            indexChanged = !textMatch(dstSqlIndex.FieldNameList, srcSqlIndex.FieldNameList);
                            break;
                        }
                    }
                    if (!indexFound) {
                        //
                        // add src to dst
                        dstCollection.sqlIndexes.Add(srcSqlIndex);
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
                string DataBuildVersion = core.siteProperties.dataBuildVersion;
                foreach (var srcKvp in srcCollection.menus) {
                    string srcKey = srcKvp.Key.ToLower() ;
                    MiniCollectionModel.miniCollectionMenuModel srcMenu = srcKvp.Value;
                    string srcName = srcMenu.Name.ToLower();
                    string srcGuid = srcMenu.Guid;
                    string SrcParentName = GenericController.vbLCase(srcMenu.ParentName);
                    string SrcNameSpace = GenericController.vbLCase(srcMenu.menuNameSpace);
                    bool SrcIsNavigator = srcMenu.IsNavigator;
                    updateDst = false;
                    //
                    // Search for match using guid
                    MiniCollectionModel.miniCollectionMenuModel dstMenuMatch = new MiniCollectionModel.miniCollectionMenuModel() { } ;
                    bool IsMatch = false;
                    string DstKey = null;
                    bool DstIsNavigator = false;
                    foreach (var dstKvp in dstCollection.menus) {
                        string dstKey = dstKvp.Key.ToLower();
                        MiniCollectionModel.miniCollectionMenuModel dstMenu = dstKvp.Value;
                        string dstGuid = dstMenu.Guid;
                        if (dstGuid == srcGuid) {
                            DstIsNavigator = dstMenu.IsNavigator;
                            DstKey = GenericController.vbLCase(dstMenu.Key);
                            string SrcKey = null;
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
                        foreach (var dstKvp in dstCollection.menus) {
                            string dstKey = dstKvp.Key.ToLower();
                            MiniCollectionModel.miniCollectionMenuModel dstMenu = dstKvp.Value;
                            dstName = GenericController.vbLCase(dstMenu.Name);
                            if ((srcName == dstName) && (SrcIsNavigator == DstIsNavigator)) {
                                if (SrcIsNavigator) {
                                    //
                                    // Navigator - check namespace if Dst.guid is blank (builder to new version of menu)
                                    IsMatch = (SrcNameSpace == GenericController.vbLCase(dstMenu.menuNameSpace)) && (dstMenu.Guid == "");
                                } else {
                                    //
                                    // AdminMenu - check parentname
                                    IsMatch = (SrcParentName == GenericController.vbLCase(dstMenu.ParentName));
                                }
                                if (IsMatch) {
                                    dstMenuMatch = dstMenu;
                                    break;
                                }
                            }
                        }
                    }
                    if(IsMatch) {
                        updateDst |= (dstMenuMatch.Active != srcMenu.Active);
                        updateDst |= (dstMenuMatch.AdminOnly != srcMenu.AdminOnly);
                        updateDst |= !textMatch(dstMenuMatch.ContentName, srcMenu.ContentName);
                        updateDst |= (dstMenuMatch.DeveloperOnly != srcMenu.DeveloperOnly);
                        updateDst |= !textMatch(dstMenuMatch.LinkPage, srcMenu.LinkPage);
                        updateDst |= !textMatch(dstMenuMatch.Name, srcMenu.Name);
                        updateDst |= (dstMenuMatch.NewWindow != srcMenu.NewWindow);
                        updateDst |= !textMatch(dstMenuMatch.SortOrder, srcMenu.SortOrder);
                        updateDst |= !textMatch(dstMenuMatch.AddonName, srcMenu.AddonName);
                        updateDst |= !textMatch(dstMenuMatch.NavIconType, srcMenu.NavIconType);
                        updateDst |= !textMatch(dstMenuMatch.NavIconTitle, srcMenu.NavIconTitle);
                        updateDst |= !textMatch(dstMenuMatch.menuNameSpace, srcMenu.menuNameSpace);
                        updateDst |= !textMatch(dstMenuMatch.Guid, srcMenu.Guid);
                        dstCollection.menus.Remove(DstKey);
                    }
                    dstCollection.menus.Add(srcKey, srcMenu);
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
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.ArgumentList, srcCollection.AddOns[SrcPtr].ArgumentList)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Copy, srcCollection.AddOns[SrcPtr].Copy)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Link, srcCollection.AddOns[SrcPtr].Link)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.Name, srcCollection.AddOns[SrcPtr].Name)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.ObjectProgramID, srcCollection.AddOns[SrcPtr].ObjectProgramID)
                //                        okToUpdateDstFromSrc = okToUpdateDstFromSrc Or Not TextMatch(core,.SortOrder, srcCollection.AddOns[SrcPtr].SortOrder)
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
                for (srcStylePtr = 0; srcStylePtr < srcCollection.styleCnt; srcStylePtr++) {
                    string srcName = GenericController.vbLCase(srcCollection.styles[srcStylePtr].Name);
                    updateDst = false;
                    //
                    // Search for this name in the Dst
                    //
                    for (dstStylePtr = 0; dstStylePtr < dstCollection.styleCnt; dstStylePtr++) {
                        dstName = GenericController.vbLCase(dstCollection.styles[dstStylePtr].Name);
                        if (dstName == srcName) {
                            //
                            // found a match between Src and Dst
                            updateDst |= !textMatch(dstCollection.styles[dstStylePtr].Copy, srcCollection.styles[srcStylePtr].Copy);
                            break;
                        }
                    }
                    if (dstStylePtr == dstCollection.styleCnt) {
                        //
                        // CDef was not found, add it
                        //
                        Array.Resize(ref dstCollection.styles, dstCollection.styleCnt);
                        dstCollection.styleCnt = dstStylePtr + 1;
                        updateDst = true;
                    }
                    if (updateDst) {
                        var tempVar6 = dstCollection.styles[dstStylePtr];
                        //
                        // It okToUpdateDstFromSrc, update the Dst with the Src
                        //
                        tempVar6.dataChanged = true;
                        tempVar6.Copy = srcCollection.styles[srcStylePtr].Copy;
                        tempVar6.Name = srcCollection.styles[srcStylePtr].Name;
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        //
        private static MiniCollectionModel installCollection_GetApplicationMiniCollection(CoreController core, bool isNewBuild) {
            MiniCollectionModel returnColl = new MiniCollectionModel();
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
                    ExportFilename = "cdef_export_" + encodeText(GenericController.GetRandomInteger(core)) + ".xml";
                    ExportPathPage = "tmp\\" + ExportFilename;
                    exportApplicationCDefXml(core, ExportPathPage, true);
                    CollectionData = core.privateFiles.readFileText(ExportPathPage);
                    core.privateFiles.deleteFile(ExportPathPage);
                    returnColl = installCollection_LoadXmlToMiniCollection(core, CollectionData, false, false, isNewBuild, new MiniCollectionModel());
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnColl;
        }
       //
        //====================================================================================================
        //
        private static string GetMenuNameSpace(CoreController core, Dictionary<string,MiniCollectionModel.miniCollectionMenuModel> menus, MiniCollectionModel.miniCollectionMenuModel menu, string UsedIDList) {
            string returnAttr = "";
            try {
                string ParentName = null;
                int Ptr = 0;
                string Prefix = null;
                string LCaseParentName = null;

                //
                ParentName = menu.ParentName;
                if (!string.IsNullOrEmpty(ParentName)) {
                    LCaseParentName = GenericController.vbLCase(ParentName);
                    foreach ( var kvp in menus) {
                        MiniCollectionModel.miniCollectionMenuModel testMenu = kvp.Value;
                        if (GenericController.vbInstr(1, "," + UsedIDList + ",", "," + Ptr.ToString() + ",") == 0) {
                            if (LCaseParentName == GenericController.vbLCase(testMenu.Name) && (menu.IsNavigator == testMenu.IsNavigator)) {
                                Prefix = GetMenuNameSpace(core, menus, testMenu, UsedIDList + "," + menu.Guid);
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
                LogController.handleError( core,ex);
                throw;
            }
            return returnAttr;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Create an entry in the Sort Methods Table
        /// </summary>
        private static void VerifySortMethod(CoreController core, string Name, string OrderByCriteria) {
            try {
                //
                DataTable dt = null;
                SqlFieldListClass sqlList = new SqlFieldListClass();
                //
                sqlList.add("name", core.db.encodeSQLText(Name));
                sqlList.add("CreatedBy", "0");
                sqlList.add("OrderByClause", core.db.encodeSQLText(OrderByCriteria));
                sqlList.add("active", SQLTrue);
                sqlList.add("ContentControlID", Models.Domain.CDefModel.getContentId(core, "Sort Methods").ToString());
                //
                dt = core.db.openTable("Default", "ccSortMethods", "Name=" + core.db.encodeSQLText(Name), "ID", "ID", 1);
                if (dt.Rows.Count > 0) {
                    //
                    // update sort method
                    //
                    core.db.updateTableRecord("Default", "ccSortMethods", "ID=" + GenericController.encodeInteger(dt.Rows[0]["ID"]).ToString(), sqlList);
                } else {
                    //
                    // Create the new sort method
                    //
                    core.db.insertTableRecord("Default", "ccSortMethods", sqlList);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public static void verifySortMethods(CoreController core) {
            try {
                //
                LogController.logInfo(core, "Verify Sort Records");
                //
                VerifySortMethod(core, "By Name", "Name");
                VerifySortMethod(core, "By Alpha Sort Order Field", "SortOrder");
                VerifySortMethod(core, "By Date", "DateAdded");
                VerifySortMethod(core, "By Date Reverse", "DateAdded Desc");
                VerifySortMethod(core, "By Alpha Sort Order Then Oldest First", "SortOrder,ID");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Get a ContentID from the ContentName using just the tables
        /// </summary>
        private static void VerifyContentFieldTypes(CoreController core) {
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
                using (DataTable rs = core.db.executeQuery("Select ID from ccFieldTypes order by id")) {
                    if (!DbController.isDataTableOk(rs)) {
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
                            if (RowsFound != GenericController.encodeInteger(dr["ID"])) {
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
                    core.db.deleteTable("Default", "ccFieldTypes");
                    core.db.createSQLTable("Default", "ccFieldTypes");
                    RowsFound = 0;
                }
                //
                // ----- Add the number of rows needed
                //
                RowsNeeded = FieldTypeIdMax - RowsFound;
                if (RowsNeeded > 0) {
                    CID = Models.Domain.CDefModel.getContentId(core, "Content Field Types");
                    if (CID <= 0) {
                        //
                        // Problem
                        //
                        LogController.handleError( core,new ApplicationException("Content Field Types content definition was not found"));
                    } else {
                        while (RowsNeeded > 0) {
                            core.db.executeQuery("Insert into ccFieldTypes (active,contentcontrolid)values(1," + CID + ")");
                            RowsNeeded = RowsNeeded - 1;
                        }
                    }
                }
                //
                // ----- Update the Names of each row
                //
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Integer' where ID=1;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Text' where ID=2;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='LongText' where ID=3;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Boolean' where ID=4;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Date' where ID=5;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='File' where ID=6;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Lookup' where ID=7;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Redirect' where ID=8;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Currency' where ID=9;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='TextFile' where ID=10;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Image' where ID=11;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Float' where ID=12;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='AutoIncrement' where ID=13;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='ManyToMany' where ID=14;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Member Select' where ID=15;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='CSS File' where ID=16;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='XML File' where ID=17;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Javascript File' where ID=18;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Link' where ID=19;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='Resource Link' where ID=20;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML' where ID=21;");
                core.db.executeQuery("Update ccFieldTypes Set active=1,Name='HTML File' where ID=22;");
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        public static void exportApplicationCDefXml(CoreController core, string privateFilesPathFilename, bool IncludeBaseFields) {
            try {
                CollectionXmlController XML = new CollectionXmlController(core);
                string Content = XML.getApplicationCollectionXml(IncludeBaseFields);
                core.privateFiles.saveFile(privateFilesPathFilename, Content);
                XML = null;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private static bool downloadCollectionFiles(CoreController core, string workingPath, string CollectionGuid, ref DateTime return_CollectionLastChangeDate, ref string return_ErrorMessage) {
            bool tempDownloadCollectionFiles = false;
            tempDownloadCollectionFiles = false;
            try {
                LogController.logInfo(core, "downloading collection [" + CollectionGuid + "]");
                //
                // Request the Download file for this collection
                XmlDocument Doc = new XmlDocument();
                string URL = "http://support.contensive.com/GetCollection?iv=" + core.codeVersion() + "&guid=" + CollectionGuid;
                string errorPrefix = "DownloadCollectionFiles, Error reading the collection library status file from the server for Collection [" + CollectionGuid + "], download URL [" + URL + "]. ";
                int downloadRetry = 0;
                int downloadDelay = 2000;
                const int downloadRetryMax = 3;
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
                    } catch (Exception ex) {
                        //
                        // this error could be data related, and may not be critical. log issue and continue
                        downloadDelay += 2000;
                        return_ErrorMessage = "There was an error while requesting the download details for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        LogController.logInfo(core, errorPrefix + "There was a parse error reading the response [" + ex.ToString() + "]");
                    }
                    downloadRetry += 1;
                } while (downloadRetry < downloadRetryMax);
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    //
                    // continue if no errors
                    if (Doc.DocumentElement.Name.ToLower() != GenericController.vbLCase(DownloadFileRootNode)) {
                        return_ErrorMessage = "The collection file from the server was not valid for collection [" + CollectionGuid + "]";
                        tempDownloadCollectionFiles = false;
                        LogController.logInfo(core, errorPrefix + "The response has a basename [" + Doc.DocumentElement.Name + "] but [" + DownloadFileRootNode + "] was expected.");
                    } else {
                        //
                        // Parse the Download File and download each file into the working folder
                        if (Doc.DocumentElement.ChildNodes.Count == 0) {
                            return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes.";
                            LogController.logInfo(core, errorPrefix + "The collection library status file from the server has a valid basename, but no childnodes. The collection was probably Not found");
                            tempDownloadCollectionFiles = false;
                        } else {
                            //
                            int CollectionFileCnt = 0;
                            foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                string ResourceFilename = null;
                                string ResourceLink = null;
                                string CollectionVersion = null;
                                string CollectionFileLink = null;
                                string Collectionname = null;
                                switch (GenericController.vbLCase(CDefSection.Name)) {
                                    case "collection":
                                        //
                                        // Read in the interfaces and save to Add-ons
                                        ResourceFilename = "";
                                        ResourceLink = "";
                                        Collectionname = "";
                                        CollectionGuid = "";
                                        CollectionVersion = "";
                                        CollectionFileLink = "";
                                        foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                            int Pos = 0;
                                            string UserError = null;
                                            switch (GenericController.vbLCase(CDefInterfaces.Name)) {
                                                case "name":
                                                    Collectionname = CDefInterfaces.InnerText;
                                                    break;
                                                case "help":
                                                    //CollectionHelp = CDefInterfaces.innerText
                                                    core.privateFiles.saveFile(workingPath + "Collection.hlp", CDefInterfaces.InnerText);
                                                    break;
                                                case "guid":
                                                    CollectionGuid = CDefInterfaces.InnerText;
                                                    break;
                                                case "lastchangedate":
                                                    return_CollectionLastChangeDate = GenericController.encodeDate(CDefInterfaces.InnerText);
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
                                                            LogController.logInfo(core, errorPrefix + "Collection [" + Collectionname + "] was not installed because the Collection File Link does not point to a valid file [" + CollectionFileLink + "]");
                                                        } else {
                                                            string CollectionFilePath = workingPath + CollectionFileLink.Substring(Pos);
                                                            core.privateFiles.saveHttpRequestToFile(CollectionFileLink, CollectionFilePath);
                                                        }
                                                    }
                                                    break;
                                                case "activexdll":
                                                case "resourcelink":
                                                    //
                                                    // save the filenames and download them only if OKtoinstall
                                                    ResourceFilename = "";
                                                    ResourceLink = "";
                                                    foreach (XmlNode ActiveXNode in CDefInterfaces.ChildNodes) {
                                                        switch (GenericController.vbLCase(ActiveXNode.Name)) {
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
                                                        LogController.logInfo(core, errorPrefix + UserError);
                                                    } else {
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            //
                                                            // Take Filename from Link
                                                            Pos = ResourceLink.LastIndexOf("/") + 1;
                                                            if (Pos != 0) {
                                                                ResourceFilename = ResourceLink.Substring(Pos);
                                                            }
                                                        }
                                                        if (string.IsNullOrEmpty(ResourceFilename)) {
                                                            UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" + ResourceLink + "].";
                                                            LogController.logInfo(core, errorPrefix + UserError);
                                                        } else {
                                                            core.privateFiles.saveHttpRequestToFile(ResourceLink, workingPath + ResourceFilename);
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        break;
                                }
                            }
                            if (CollectionFileCnt == 0) {
                                LogController.logInfo(core, errorPrefix + "The collection was requested and downloaded, but was not installed because the download file did not have a collection root node.");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return tempDownloadCollectionFiles;
        }
        //
        //====================================================================================================
        //
        public static bool installCollectionFromRemoteRepo(CoreController core, string collectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            bool UpgradeOK = true;
            try {
                logPrefix += ", installCollectionFromRemoteRepo [" + collectionGuid + "]";
                if (string.IsNullOrWhiteSpace(collectionGuid)) {
                    LogController.logWarn(core, logPrefix + ", installCollectionFromRemoteRepo, collectionGuid is null");
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
                    string CollectionVersionFolderName = GetCollectionPath(core, collectionGuid);
                    if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                        //
                        // Download all files for this collection and build the collection folder(s)
                        //
                        string workingPath = core.addon.getPrivateFilesAddonPath() + "temp_" + GenericController.GetRandomInteger(core) + "\\";
                        core.privateFiles.createPath(workingPath);
                        //
                        DateTime CollectionLastChangeDate = default(DateTime);
                        UpgradeOK = downloadCollectionFiles(core, workingPath, collectionGuid, ref CollectionLastChangeDate, ref return_ErrorMessage);
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        } else {
                            List<string> collectionGuidList = new List<string>();
                            UpgradeOK = buildLocalCollectionReposFromFolder(core, workingPath, CollectionLastChangeDate, ref collectionGuidList, ref return_ErrorMessage, false);
                            if (!UpgradeOK) {
                                //UpgradeOK = UpgradeOK;
                            }
                        }
                        //
                        core.privateFiles.deleteFolder(workingPath);
                    }
                    //
                    // Upgrade the server from the collection files
                    //
                    if (UpgradeOK) {
                        UpgradeOK = installCollectionFromLocalRepo(core, collectionGuid, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections );
                        if (!UpgradeOK) {
                            //UpgradeOK = UpgradeOK;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return UpgradeOK;
        }
        //
        //====================================================================================================
        public static bool installCollectionFromRemoteRepo(CoreController core, string CollectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, bool repair, string logPrefix, ref List<string> installedCollections) {
            var tmpList = new List<string> { };
            return installCollectionFromRemoteRepo(core, CollectionGuid, ref return_ErrorMessage,ImportFromCollectionsGuidList, IsNewBuild, repair, ref tmpList, logPrefix, ref installedCollections);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrades all collections, registers and resets the server if needed
        /// </summary>
        public static bool upgradeLocalCollectionRepoFromRemoteCollectionRepo(CoreController core, ref string return_ErrorMessage, ref string return_RegisterList, ref bool return_IISResetRequired, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            bool returnOk = true;
            try {
                bool localCollectionUpToDate = false;
                string SupportURL = null;
                string GuidList = null;
                DateTime CollectionLastChangeDate = default(DateTime);
                string workingPath = null;
                string LocalGuid = null;
                DateTime LocalLastChangeDate = default(DateTime);
                string LibName = "";
                bool LibSystem = false;
                string LibGUID = null;
                string LibLastChangeDateStr = null;
                string LibContensiveVersion = "";
                DateTime LibLastChangeDate = default(DateTime);
                XmlDocument LibraryCollections = new XmlDocument();
                string Copy = null;
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                var localCollectionStoreList = new List<CollectionStoreClass>();
                if ( getLocalCollectionStoreList(core, ref localCollectionStoreList, ref return_ErrorMessage)) {
                    if (localCollectionStoreList.Count > 0) {
                        //
                        // Request collection updates 10 at a time
                        //
                        int packageSize = 0;
                        int packageNumber = 0;
                        foreach ( var collectionStore in localCollectionStoreList ) {
                            GuidList = GuidList + "," + collectionStore.guid;
                            packageSize += 1;
                            if (( packageSize>=10 ) | ( collectionStore == localCollectionStoreList.Last())) {
                                packageNumber += 1;
                                //
                                // -- send package of 10, or the last set
                                if (!string.IsNullOrEmpty(GuidList)) {
                                    LogController.logInfo(core, logPrefix +  " ,Fetch collection details for collections [" + GuidList + "]");
                                    GuidList = GuidList.Substring(1);
                                    //
                                    //-----------------------------------------------------------------------------------------------
                                    //   Load LibraryCollections from the Support Site
                                    //-----------------------------------------------------------------------------------------------
                                    //
                                    LibraryCollections = new XmlDocument();
                                    SupportURL = "http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion() + "&guidlist=" + encodeRequestVariable(GuidList);
                                    bool loadOK = true;
                                    if ( packageNumber>1 ) {
                                        Thread.Sleep(2000);
                                    }
                                    try {
                                        LibraryCollections.Load(SupportURL);
                                    } catch (Exception) {
                                        Copy = "Error downloading or loading GetCollectionList from Support.";
                                        LogController.logInfo(core, Copy + ", the request was [" + SupportURL + "]");
                                        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                        returnOk = false;
                                        loadOK = false;
                                    }
                                    if (loadOK) {
                                        {
                                            if (GenericController.vbLCase(LibraryCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                                                Copy = "The GetCollectionList support site remote method returned an xml file with an invalid root node, [" + LibraryCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                                                LogController.logInfo(core, Copy + ", the request was [" + SupportURL + "]");
                                                return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                                                returnOk = false;
                                            } else {
                                                //
                                                // -- Search for Collection Updates Needed
                                                foreach (var localTestCollection in localCollectionStoreList) {
                                                    localCollectionUpToDate = false;
                                                    LocalGuid = localTestCollection.guid.ToLower();
                                                    LocalLastChangeDate = localTestCollection.lastChangeDate;
                                                    //
                                                    // go through each collection on the Library and find the local collection guid
                                                    //
                                                    foreach (XmlNode LibListNode in LibraryCollections.DocumentElement.ChildNodes) {
                                                        if (localCollectionUpToDate) {
                                                            break;
                                                        }
                                                        switch (GenericController.vbLCase(LibListNode.Name)) {
                                                            case "collection":
                                                                LibGUID = "";
                                                                LibLastChangeDateStr = "";
                                                                LibLastChangeDate = DateTime.MinValue;
                                                                foreach (XmlNode CollectionNode in LibListNode.ChildNodes) {
                                                                    switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                                        case "name":
                                                                            //
                                                                            LibName = GenericController.vbLCase(CollectionNode.InnerText);
                                                                            break;
                                                                        case "system":
                                                                            //
                                                                            LibSystem = GenericController.encodeBoolean(CollectionNode.InnerText);
                                                                            break;
                                                                        case "guid":
                                                                            //
                                                                            LibGUID = GenericController.vbLCase(CollectionNode.InnerText);
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
                                                                    if ((!string.IsNullOrEmpty(LibGUID)) & (LibGUID == LocalGuid) & ((string.IsNullOrEmpty(LibContensiveVersion)) || (string.CompareOrdinal(LibContensiveVersion, core.codeVersion()) <= 0))) {
                                                                        LogController.logInfo(core, logPrefix +  " ,verify collection [" + LibGUID + "]");
                                                                        //
                                                                        // LibCollection matches the LocalCollection - process the upgrade
                                                                        //
                                                                        if (GenericController.vbInstr(1, LibGUID, "58c9", 1) != 0) {
                                                                            //LibGUID = LibGUID;
                                                                        }
                                                                        if (!DateController.IsDate(LibLastChangeDateStr)) {
                                                                            LibLastChangeDate = DateTime.MinValue;
                                                                        } else {
                                                                            LibLastChangeDate = GenericController.encodeDate(LibLastChangeDateStr);
                                                                        }
                                                                        // TestPoint 1.1 - Test each collection for upgrade
                                                                        if (LibLastChangeDate > LocalLastChangeDate) {
                                                                            //
                                                                            // LibLastChangeDate <>0, and it is > local lastchangedate
                                                                            //
                                                                            workingPath = core.addon.getPrivateFilesAddonPath() + "\\temp_" + GenericController.GetRandomInteger(core) + "\\";
                                                                            LogController.logInfo(core, logPrefix +  " ,Upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], because LocalChangeDate [" + LocalLastChangeDate + "] < LibraryChangeDate [" + LibLastChangeDate + "]");
                                                                            //
                                                                            // Upgrade Needed
                                                                            //
                                                                            core.privateFiles.createPath(workingPath);
                                                                            //
                                                                            returnOk = downloadCollectionFiles(core, workingPath, LibGUID, ref CollectionLastChangeDate, ref return_ErrorMessage);
                                                                            if (returnOk) {
                                                                                List<string> listGuidList = new List<string>();
                                                                                returnOk = buildLocalCollectionReposFromFolder(core, workingPath, CollectionLastChangeDate, ref listGuidList, ref return_ErrorMessage, false);
                                                                            }
                                                                            //
                                                                            core.privateFiles.deleteFolder(workingPath);
                                                                            //
                                                                            // Upgrade the apps from the collection files, do not install on any apps
                                                                            //
                                                                            if (returnOk) {
                                                                                returnOk = installCollectionFromLocalRepo(core, LibGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                                                                            }
                                                                            //
                                                                            // make sure this issue is logged and clear the flag to let other local collections install
                                                                            //
                                                                            if (!returnOk) {
                                                                                LogController.logInfo(core, logPrefix +  " ,There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection, the request was [" + SupportURL + "]");
                                                                                returnOk = true;
                                                                            }
                                                                        }
                                                                        //
                                                                        // this local collection has been resolved, go to the next local collection
                                                                        //
                                                                        localCollectionUpToDate = true;
                                                                        //
                                                                        if (!returnOk) {
                                                                            LogController.logInfo(core, logPrefix +  " ,There was a problem upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], error message [" + return_ErrorMessage + "], will clear error and continue with the next collection");
                                                                            returnOk = true;
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
                                packageSize = 0;
                                GuidList = "";
                            }
                        }
                    }
                };
                //collectionServerXml = getLocalCollectionStoreListXml(core);
                //if (!string.IsNullOrEmpty(collectionServerXml)) {
                //    LocalCollections = new XmlDocument();
                //    try {
                //        LocalCollections.LoadXml(collectionServerXml);
                //    } catch (Exception) {
                //        Copy = "Error loading privateFiles\\addons\\Collections.xml";
                //        logController.appendLogInstall(core, Copy);
                //        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                //        returnOk = false;
                //    }
                //    if (returnOk) {
                //        if (genericController.vbLCase(LocalCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode)) {
                //            Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                //            logController.appendLogInstall(core, Copy);
                //            return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                //            returnOk = false;
                //        } else {
                //            //
                //            // Get a list of the collection guids on this server
                //            //

                //            GuidCnt = 0;
                //            if (genericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist") {
                //                foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                //                    switch (genericController.vbLCase(LocalListNode.Name)) {
                //                        case "collection":
                //                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                //                                if (genericController.vbLCase(CollectionNode.Name) == "guid") {
                //                                    Array.Resize(ref GuidArray, GuidCnt + 1);
                //                                    GuidArray[GuidCnt] = CollectionNode.InnerText;
                //                                    GuidCnt = GuidCnt + 1;
                //                                    break;
                //                                }
                //                            }
                //                            break;
                //                    }
                //                }
                //            }



                //        }
                //    }
                //}
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrade a collection from the files in a working folder
        /// </summary>
        public static bool buildLocalCollectionReposFromFolder(CoreController core, string sourcePrivateFolderPath, DateTime CollectionLastChangeDate, ref List<string> return_CollectionGUIDList, ref string return_ErrorMessage, bool allowLogging) {
            bool success = false;
            try {
                if (core.privateFiles.pathExists(sourcePrivateFolderPath)) {
                    LogController.logInfo(core, "BuildLocalCollectionFolder, processing files in private folder [" + sourcePrivateFolderPath + "]");
                    List<CPFileSystemClass.FileDetail> SrcFileNamelist = core.privateFiles.getFileList(sourcePrivateFolderPath);
                    foreach (CPFileSystemClass.FileDetail file in SrcFileNamelist) {
                        if ((file.Extension == ".zip") || (file.Extension == ".xml")) {
                            string collectionGuid = "";
                            success = buildLocalCollectionRepoFromFile(core, sourcePrivateFolderPath + file.Name, CollectionLastChangeDate, ref collectionGuid, ref return_ErrorMessage, allowLogging);
                            return_CollectionGUIDList.Add(collectionGuid);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return success;
        }
        //
        //====================================================================================================
        //
        public static bool buildLocalCollectionRepoFromFile(CoreController core, string collectionPathFilename, DateTime CollectionLastChangeDate, ref string return_CollectionGUID, ref string return_ErrorMessage, bool allowLogging) {
            bool tempBuildLocalCollectionRepoFromFile = false;
            bool result = true;
            try {
                //XmlDocument Doc = new XmlDocument();
                //collectionXmlController XMLTools = new collectionXmlController(core);
                //
                // process all xml files in this workingfolder
                //
                if (allowLogging) {
                    LogController.logInfo(core, "BuildLocalCollectionFolder(), Enter");
                }
                string collectionPath = "";
                string collectionFilename = "";
                //
                core.privateFiles.splitDosPathFilename(collectionPathFilename, ref collectionPath, ref collectionFilename);
                string CollectionVersionFolderName = "";
                string Collectionname = "";
                string CollectionGuid = "";
                if (!core.privateFiles.pathExists(collectionPath)) {
                    //
                    // The working folder is not there
                    //
                    result = false;
                    return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
                    if (allowLogging) {
                        LogController.logInfo(core, "BuildLocalCollectionFolder(), " + return_ErrorMessage);
                    }
                    LogController.logInfo(core, "BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" + collectionPath + "]");
                } else {
                    LogController.logInfo(core, "BuildLocalCollectionFolder, processing files in private folder [" + collectionPath + "]");
                    //
                    // move collection file to a temp directory
                    //
                    string tmpInstallPath = "tmpInstallCollection" + GenericController.getGUIDString() + "\\";
                    core.privateFiles.copyFile(collectionPathFilename, tmpInstallPath + collectionFilename);
                    if (collectionFilename.ToLower().Substring(collectionFilename.Length - 4) == ".zip") {
                        core.privateFiles.UnzipFile(tmpInstallPath + collectionFilename);
                        core.privateFiles.deleteFile(tmpInstallPath + collectionFilename);
                    }
                    //
                    // install the individual files
                    //
                    List<FileDetail> SrcFileNamelist = core.privateFiles.getFileList(tmpInstallPath);
                    if (true) {
                        bool CollectionFileFound = false;
                        //
                        // Process all non-zip files
                        //
                        foreach (FileDetail file in SrcFileNamelist) {
                            string Filename = file.Name;
                            LogController.logInfo(core, "BuildLocalCollectionFolder, processing files, filename=[" + Filename + "]");
                            if (GenericController.vbLCase(Filename.Substring(Filename.Length - 4)) == ".xml") {
                                //
                                LogController.logInfo(core, "BuildLocalCollectionFolder, processing xml file [" + Filename + "]");
                                XmlDocument CollectionFile = new XmlDocument();
                                //hint = hint & ",320"
                                CollectionFile = new XmlDocument();
                                bool loadOk = true;
                                try {
                                    CollectionFile.LoadXml(core.privateFiles.readFileText(tmpInstallPath + Filename));
                                } catch (Exception ex) {
                                    //
                                    // There was a parse error in this xml file. Set the return message and the flag
                                    // If another xml files shows up, and process OK it will cover this error
                                    //
                                    //hint = hint & ",330"
                                    return_ErrorMessage = "There was a problem installing the Collection File [" + tmpInstallPath + Filename + "]. The error reported was [" + ex.Message + "].";
                                    LogController.logInfo(core, "BuildLocalCollectionFolder, error reading collection [" + collectionPathFilename + "]");
                                    //StatusOK = False
                                    loadOk = false;
                                }
                                if (loadOk) {
                                    //hint = hint & ",400"
                                    string CollectionFileBaseName = GenericController.vbLCase(CollectionFile.DocumentElement.Name);
                                    if ((CollectionFileBaseName != "contensivecdef") & (CollectionFileBaseName != CollectionFileRootNode) & (CollectionFileBaseName != GenericController.vbLCase(CollectionFileRootNodeOld))) {
                                        //
                                        // Not a problem, this is just not a collection file
                                        //
                                        LogController.logInfo(core, "BuildLocalCollectionFolder, xml base name wrong [" + CollectionFileBaseName + "]");
                                    } else {
                                        bool IsFound = false;
                                        //
                                        // Collection File
                                        //
                                        //hint = hint & ",420"
                                        Collectionname = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "name", "");
                                        if (string.IsNullOrEmpty(Collectionname)) {
                                            //
                                            // ----- Error condition -- it must have a collection name
                                            //
                                            result = false;
                                            return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>";
                                            LogController.logInfo(core, "BuildLocalCollectionFolder, collection has no name");
                                        } else {
                                            //
                                            //------------------------------------------------------------------
                                            // Build Collection folder structure in /Add-ons folder
                                            //------------------------------------------------------------------
                                            //
                                            //hint = hint & ",440"
                                            CollectionFileFound = true;
                                            CollectionGuid = XmlController.GetXMLAttribute(core, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
                                            if (string.IsNullOrEmpty(CollectionGuid)) {
                                                //
                                                // I hope I do not regret this
                                                //
                                                CollectionGuid = Collectionname;
                                            }
                                            CollectionGuid = CollectionGuid.ToLower();
                                            CollectionVersionFolderName = GetCollectionPath(core, CollectionGuid);
                                            string CollectionFolderName = "";
                                            if (!string.IsNullOrEmpty(CollectionVersionFolderName)) {
                                                //
                                                // This is an upgrade
                                                //
                                                int Pos = GenericController.vbInstr(1, CollectionVersionFolderName, "\\");
                                                if (Pos > 0) {
                                                    CollectionFolderName = CollectionVersionFolderName.Left(Pos - 1);
                                                }
                                            } else {
                                                //
                                                // This is an install
                                                //
                                                //hint = hint & ",460"
                                                CollectionFolderName = CollectionGuid;
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "{", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "}", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, "-", "");
                                                CollectionFolderName = GenericController.vbReplace(CollectionFolderName, " ", "");
                                                CollectionFolderName = Collectionname + "_" + CollectionFolderName;
                                                CollectionFolderName = CollectionFolderName.ToLower();
                                            }
                                            string CollectionFolder = core.addon.getPrivateFilesAddonPath() + CollectionFolderName + "\\";
                                            core.privateFiles.verifyPath(CollectionFolder);
                                            //
                                            // create a collection 'version' folder for these new files
                                            string TimeStamp = "";
                                            DateTime NowTime = default(DateTime);
                                            NowTime = DateTime.Now;
                                            int NowPart = NowTime.Year;
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
                                            string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName;
                                            string CollectionVersionPath = CollectionVersionFolder + "\\";
                                            core.privateFiles.createPath(CollectionVersionPath);

                                            core.privateFiles.copyFolder(tmpInstallPath, CollectionVersionFolder);
                                            //StatusOK = True
                                            //
                                            // Install activeX and search for importcollections
                                            //
                                            //hint = hint & ",500"
                                            foreach (XmlNode CDefSection in CollectionFile.DocumentElement.ChildNodes) {
                                                string ChildCollectionGUID = null;
                                                string ChildCollectionName = null;
                                                bool Found = false;
                                                switch (GenericController.vbLCase(CDefSection.Name)) {
                                                    case "resource":
                                                        //
                                                        // resource node, if executable node, save to RegisterList
                                                        //
                                                        //hint = hint & ",510"
                                                        //ResourceType = genericController.vbLCase(xmlController.GetXMLAttribute(core, IsFound, CDefSection, "type", ""))
                                                        //Dim resourceFilename As String = Trim(xmlController.GetXMLAttribute(core, IsFound, CDefSection, "name", ""))
                                                        //Dim resourcePathFilename As String = CollectionVersionPath & resourceFilename
                                                        //If resourceFilename = "" Then
                                                        //    '
                                                        //    ' filename is blank
                                                        //    '
                                                        //    'hint = hint & ",511"
                                                        //ElseIf Not core.privateFiles.fileExists(resourcePathFilename) Then
                                                        //    '
                                                        //    ' resource is not here
                                                        //    '
                                                        //    'hint = hint & ",513"
                                                        //    result = False
                                                        //    return_ErrorMessage = "<p>There was a problem with the Collection File. The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.</p>"
                                                        //    Call logController.appendInstallLog(core, "BuildLocalCollectionFolder, The resource referenced in the collection file [" & resourceFilename & "] was not included in the resource files.")
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
                                                        //    AOName =xmlController.GetXMLAttribute(core, IsFound, CDefInterfaces, "name", "No Name")
                                                        //    If AOName = "" Then
                                                        //        AOName = "No Name"
                                                        //    End If
                                                        //    AOGuid =xmlController.GetXMLAttribute(core, IsFound, CDefInterfaces, "guid", AOName)
                                                        //    If AOGuid = "" Then
                                                        //        AOGuid = AOName
                                                        //    End If
                                                        //Next
                                                        break;
                                                    case "getcollection":
                                                    case "importcollection":
                                                        //
                                                        // -- Download Collection file into install folder
                                                        ChildCollectionName = XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                        ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                        if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                            ChildCollectionGUID = CDefSection.InnerText;
                                                        }
                                                        string statusMsg = "Installing collection [" + ChildCollectionName + ", " + ChildCollectionGUID + "] referenced from collection [" + Collectionname + "]";
                                                        LogController.logInfo(core, "BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" + ChildCollectionName + "], childCollectionGuid [" + ChildCollectionGUID + "]");
                                                        if (GenericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, 1) == 0) {
                                                            if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                //
                                                                // -- Needs a GUID to install
                                                                result = false;
                                                                return_ErrorMessage = statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID.";
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, return message [" + return_ErrorMessage + "]");
                                                            } else if (GetCollectionPath(core, ChildCollectionGUID) == "") {
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], not found so needs to be installed");
                                                                //
                                                                // If it is not already installed, download and install it also
                                                                //
                                                                string ChildWorkingPath = CollectionVersionPath + "\\" + ChildCollectionGUID + "\\";
                                                                DateTime ChildCollectionLastChangeDate = default(DateTime);
                                                                //
                                                                // down an imported collection file
                                                                //
                                                                bool StatusOK = downloadCollectionFiles(core, ChildWorkingPath, ChildCollectionGUID, ref ChildCollectionLastChangeDate, ref return_ErrorMessage);
                                                                if (!StatusOK) {

                                                                    LogController.logInfo(core, "BuildLocalCollectionFolder, [" + statusMsg + "], downloadCollectionFiles returned error state, message [" + return_ErrorMessage + "]");
                                                                    if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" + ChildCollectionGUID + "].";
                                                                    } else {
                                                                        return_ErrorMessage = statusMsg + ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                    }
                                                                } else {
                                                                    LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], downloadCollectionFiles returned OK");
                                                                    //
                                                                    // install the downloaded file
                                                                    //
                                                                    List<string> ChildCollectionGUIDList = new List<string>();
                                                                    StatusOK = buildLocalCollectionReposFromFolder(core, ChildWorkingPath, ChildCollectionLastChangeDate, ref ChildCollectionGUIDList, ref return_ErrorMessage, allowLogging);
                                                                    if (!StatusOK) {
                                                                        LogController.logInfo(core, "BuildLocalCollectionFolder, [" + statusMsg + "], BuildLocalCollectionFolder returned error state, message [" + return_ErrorMessage + "]");
                                                                        if (string.IsNullOrEmpty(return_ErrorMessage)) {
                                                                            return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "].";
                                                                        } else {
                                                                            return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
                                                                        }
                                                                    }
                                                                }
                                                                //
                                                                // -- remove child installation working folder
                                                                core.privateFiles.deleteFolder(ChildWorkingPath);
                                                            } else {
                                                                //
                                                                //
                                                                //
                                                                LogController.logInfo(core, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], already installed");
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
                            bool ZipFileFound = false;
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
                    core.privateFiles.deleteFolder(tmpInstallPath);
                }
                //
                // If the collection parsed correctly, update the Collections.xml file
                //
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    UpdateConfig(core, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
                } else {
                    //
                    // there was an error processing the collection, be sure to save description in the log
                    //
                    result = false;
                    LogController.logInfo(core, "BuildLocalCollectionFolder, ERROR Exiting, ErrorMessage [" + return_ErrorMessage + "]");
                }
                //
                LogController.logInfo(core, "BuildLocalCollectionFolder, Exiting with ErrorMessage [" + return_ErrorMessage + "]");
                //
                tempBuildLocalCollectionRepoFromFile = (string.IsNullOrEmpty(return_ErrorMessage));
                return_CollectionGUID = CollectionGuid;
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Upgrade Application from a local collection
        /// </summary>
        public static bool installCollectionFromLocalRepo(CoreController core, string CollectionGuid, string ignore_BuildVersion, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            bool result = true;
            try {
                if (!installedCollections.Contains( CollectionGuid.ToLower() )) {
                    installedCollections.Add(CollectionGuid.ToLower());
                    logPrefix += ", installCollectionFromLocalRepo [" + CollectionGuid + "]";
                    string CollectionVersionFolderName = "";
                    DateTime CollectionLastChangeDate = default(DateTime);
                    string tempVar = "";
                    getCollectionConfig(core, CollectionGuid, ref CollectionVersionFolderName, ref CollectionLastChangeDate, ref tempVar);
                    if (string.IsNullOrEmpty(CollectionVersionFolderName)) {
                        result = false;
                        return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
                    } else {
                        //
                        // Search Local Collection Folder for collection config file (xml file)
                        //
                        string CollectionVersionFolder = core.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName + "\\";
                        List<FileDetail> srcFileInfoArray = core.privateFiles.getFileList(CollectionVersionFolder);
                        if (srcFileInfoArray.Count == 0) {
                            result = false;
                            return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>";
                        } else {
                            //
                            // collect list of DLL files and add them to the exec files if they were missed
                            List<string> assembliesInZip = new List<string>();
                            foreach (FileDetail file in srcFileInfoArray) {
                                if (file.Extension.ToLower() == "dll") {
                                    if (!assembliesInZip.Contains(file.Name.ToLower())) {
                                        assembliesInZip.Add(file.Name.ToLower());
                                    }
                                }
                            }
                            //
                            // -- Process the other files
                            bool CollectionblockNavigatorNode_fileValueOK = false;
                            foreach (FileDetail file in srcFileInfoArray) {
                                if (file.Extension == ".xml") {
                                    //
                                    // -- XML file -- open it to figure out if it is one we can use
                                    XmlDocument Doc = new XmlDocument();
                                    string CollectionFilename = file.Name;
                                    bool loadOK = true;
                                    string collectionFileContent = core.privateFiles.readFileText(CollectionVersionFolder + file.Name);
                                    try {
                                        Doc.LoadXml(collectionFileContent);
                                    } catch (Exception) {
                                        //
                                        // error - Need a way to reach the user that submitted the file
                                        //
                                        LogController.logInfo(core, logPrefix + " ,There was an error reading the Meta data file [" + core.privateFiles.localAbsRootPath + CollectionVersionFolder + file.Name + "].");
                                        result = false;
                                        return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error</P>";
                                        loadOK = false;
                                    }
                                    if (loadOK) {
                                        if ((Doc.DocumentElement.Name.ToLower() == GenericController.vbLCase(CollectionFileRootNode)) || (Doc.DocumentElement.Name.ToLower() == GenericController.vbLCase(CollectionFileRootNodeOld))) {
                                            //
                                            //------------------------------------------------------------------------------------------------------
                                            // Collection File - import from sub so it can be re-entrant
                                            //------------------------------------------------------------------------------------------------------
                                            //
                                            bool IsFound = false;
                                            string Collectionname = XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "name", "");
                                            if (string.IsNullOrEmpty(Collectionname)) {
                                                //
                                                // ----- Error condition -- it must have a collection name
                                                //
                                                //Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
                                                LogController.logInfo(core, logPrefix + " ,UpgradeAppFromLocalCollection, collection has no name");
                                                result = false;
                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the collection name in the xml collection file is blank</P>";
                                            } else {
                                                bool CollectionSystem_fileValueOK = false;
                                                bool CollectionUpdatable_fileValueOK = false;
                                                //												Dim CollectionblockNavigatorNode_fileValueOK As Boolean
                                                bool CollectionSystem = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""));
                                                int Parent_NavID = AppBuilderController.verifyNavigatorEntry(core, addonGuidManageAddon, "", "Manage Add-ons", "", "", "", false, false, false, true, "", "", "", 0);
                                                bool CollectionUpdatable = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""));
                                                bool CollectionblockNavigatorNode = GenericController.encodeBoolean(XmlController.GetXMLAttribute(core, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""));
                                                string FileGuid = XmlController.GetXMLAttribute(core, IsFound, Doc.DocumentElement, "guid", Collectionname);
                                                if (string.IsNullOrEmpty(FileGuid)) {
                                                    FileGuid = Collectionname;
                                                }
                                                if (CollectionGuid.ToLower() != GenericController.vbLCase(FileGuid)) {
                                                    //
                                                    //
                                                    //
                                                    result = false;
                                                    LogController.logInfo(core, logPrefix + " ,Local Collection file contains a different GUID for [" + Collectionname + "] then Collections.xml");
                                                    return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>";
                                                } else {
                                                    if (string.IsNullOrEmpty(CollectionGuid)) {
                                                        //
                                                        // I hope I do not regret this
                                                        //
                                                        CollectionGuid = Collectionname;
                                                    }
                                                    string onInstallAddonGuid = "";
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
                                                    LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 1");
                                                    string wwwFileList = "";
                                                    string ContentFileList = "";
                                                    string ExecFileList = "";
                                                    foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                        switch (CDefSection.Name.ToLower()) {
                                                            case "resource":
                                                                //
                                                                // set wwwfilelist, contentfilelist, execfilelist
                                                                //
                                                                string resourceType = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "type", "");
                                                                string resourcePath = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "path", "");
                                                                string filename = XmlController.GetXMLAttribute(core, IsFound, CDefSection, "name", "");
                                                                //
                                                                LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 1, resource found, name [" + filename + "], type [" + resourceType + "], path [" + resourcePath + "]");
                                                                //
                                                                filename = GenericController.convertToDosSlash(filename);
                                                                string SrcPath = "";
                                                                string dstPath = resourcePath;
                                                                int Pos = GenericController.vbInstr(1, filename, "\\");
                                                                if (Pos != 0) {
                                                                    //
                                                                    // Source path is in filename
                                                                    //
                                                                    SrcPath = filename.Left(Pos - 1);
                                                                    filename = filename.Substring(Pos);
                                                                    if (string.IsNullOrEmpty(resourcePath)) {
                                                                        //
                                                                        // -- No Resource Path give, use the same folder structure from source
                                                                        dstPath = SrcPath;
                                                                    } else {
                                                                        //
                                                                        // -- Copy file to resource path
                                                                        dstPath = resourcePath;
                                                                    }
                                                                }
                                                                //
                                                                // -- if the filename in the collection file is the wrong case, correct it now
                                                                filename = core.privateFiles.correctFilenameCase(CollectionVersionFolder + SrcPath + filename);
                                                                //
                                                                // == normalize dst
                                                                string dstDosPath = FileController.normalizeDosPath(dstPath);
                                                                //
                                                                // -- 
                                                                switch (resourceType.ToLower()) {
                                                                    case "www":
                                                                        wwwFileList += "\r\n" + dstDosPath + filename;
                                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to www, src [" + CollectionVersionFolder + SrcPath + "], dst [" + core.appConfig.localWwwPath + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.appRootFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping www file [" + core.appConfig.localWwwPath + dstDosPath + filename + "].");
                                                                            core.appRootFiles.UnzipFile(dstDosPath + filename);
                                                                        }
                                                                        break;
                                                                    case "file":
                                                                    case "content":
                                                                        ContentFileList += "\r\n" + dstDosPath + filename;
                                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to content, src [" + CollectionVersionFolder + SrcPath + "], dst [" + dstDosPath + "].");
                                                                        core.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, dstDosPath + filename, core.cdnFiles);
                                                                        if (GenericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip") {
                                                                            LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping content file [" + dstDosPath + filename + "].");
                                                                            core.cdnFiles.UnzipFile(dstDosPath + filename);
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
                                                                string ChildCollectionName = XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                                string ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    ChildCollectionGUID = CDefSection.InnerText;
                                                                }
                                                                if ((ImportFromCollectionsGuidList + "," + CollectionGuid).IndexOf(ChildCollectionGUID, System.StringComparison.OrdinalIgnoreCase) != -1) {
                                                                    //
                                                                    // circular import detected, this collection is already imported
                                                                    //
                                                                    LogController.logInfo(core, logPrefix + " ,Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1. The collection to be imported is [" + ChildCollectionName + "], GUID [" + ChildCollectionGUID + "]");
                                                                } else {
                                                                    LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 1, import collection found, name [" + ChildCollectionName + "], guid [" + ChildCollectionGUID + "]");
                                                                    installCollectionFromRemoteRepo(core, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                                                                    //if (true) {
                                                                    //    installCollectionFromRemoteRepo(core, ChildCollectionGUID, ref return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, ref nonCriticalErrorList);
                                                                    //} else {
                                                                    //    if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                    //        logController.appendInstallLog(core, "The importcollection node [" + ChildCollectionName + "] can not be upgraded because it does not include a valid guid.");
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
                                                                    //        GetCollectionConfig(core, ChildCollectionGUID, ref ChildCollectionVersionFolderName, ref ChildCollectionLastChangeDate, ref tempVar2);
                                                                    //        if (!string.IsNullOrEmpty(ChildCollectionVersionFolderName)) {
                                                                    //            //
                                                                    //            // It is installed in the local collections, update just this site
                                                                    //            //
                                                                    //            result &= installCollectionFromLocalRepo(core, ChildCollectionGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList + "," + CollectionGuid, IsNewBuild, ref nonCriticalErrorList);
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
                                                    LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 1 done, create collection record.");
                                                    AddonCollection collection = AddonCollection.create(core, CollectionGuid);
                                                    if (collection != null) {
                                                        //
                                                        // Upgrade addon
                                                        //
                                                        if (CollectionLastChangeDate == DateTime.MinValue) {
                                                            LogController.logInfo(core, logPrefix + " ,collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has the collection, but the new version has no lastchangedate, so it will upgrade to this unknown (manual) version.");
                                                            OKToInstall = true;
                                                        } else if (collection.lastChangeDate < CollectionLastChangeDate) {
                                                            LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an older version of collection. It will be upgraded.");
                                                            OKToInstall = true;
                                                        } else if (repair) {
                                                            LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection, but the repair option is true so it will be reinstalled.");
                                                            OKToInstall = true;
                                                        } else {
                                                            LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.");
                                                            OKToInstall = false;
                                                        }
                                                    } else {
                                                        //
                                                        // Install new on this application
                                                        //
                                                        collection = AddonCollection.add(core);
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App does not have this collection so it will be installed.");
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
                                                        collection.help = "";
                                                        collection.ccguid = CollectionGuid;
                                                        collection.lastChangeDate = CollectionLastChangeDate;
                                                        if (CollectionSystem_fileValueOK) {
                                                            collection.system = CollectionSystem;
                                                        }
                                                        if (CollectionUpdatable_fileValueOK) {
                                                            collection.updatable = CollectionUpdatable;
                                                        }
                                                        if (CollectionblockNavigatorNode_fileValueOK) {
                                                            collection.blockNavigatorNode = CollectionblockNavigatorNode;
                                                        }
                                                        collection.helpLink = CollectionHelpLink;
                                                        //
                                                        core.db.deleteContentRecords("Add-on Collection CDef Rules", "CollectionID=" + collection.id);
                                                        core.db.deleteContentRecords("Add-on Collection Parent Rules", "ParentID=" + collection.id);
                                                        //
                                                        // Store all resource found, new way and compatibility way
                                                        //
                                                        collection.contentFileList = ContentFileList;
                                                        collection.execFileList = ExecFileList;
                                                        collection.wwwFileList = wwwFileList;
                                                        //
                                                        // ----- remove any current navigator nodes installed by the collection previously
                                                        //
                                                        if (collection.id != 0) {
                                                            core.db.deleteContentRecords(cnNavigatorEntries, "installedbycollectionid=" + collection.id);
                                                        }
                                                        //
                                                        //-------------------------------------------------------------------------------
                                                        // ----- Pass 2
                                                        // Go through all collection nodes
                                                        // Process all cdef related nodes to the old upgrade
                                                        //-------------------------------------------------------------------------------
                                                        //
                                                        string CollectionWrapper = "";
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 2");
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
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
                                                            bool isBaseCollection = (baseCollectionGuid.ToLower() == CollectionGuid.ToLower());
                                                            installCollectionFromLocalRepo_BuildDbFromXmlData(core, CollectionWrapper, IsNewBuild, repair, isBaseCollection, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                                                            //
                                                            // -- Process nodes to save Collection data
                                                            XmlDocument NavDoc = new XmlDocument();
                                                            loadOK = true;
                                                            try {
                                                                NavDoc.LoadXml(CollectionWrapper);
                                                            } catch (Exception) {
                                                                //
                                                                // error - Need a way to reach the user that submitted the file
                                                                //
                                                                LogController.logInfo(core, logPrefix + " ,Creating navigator entries, there was an error parsing the portion of the collection that contains cdef. Navigator entry creation was aborted. [There was an error reading the Meta data file.]");
                                                                result = false;
                                                                return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error.</P>";
                                                                loadOK = false;
                                                            }
                                                            if (loadOK) {
                                                                foreach (XmlNode CDefNode in NavDoc.DocumentElement.ChildNodes) {
                                                                    switch (GenericController.vbLCase(CDefNode.Name)) {
                                                                        case "cdef":
                                                                            string ContentName = XmlController.GetXMLAttribute(core, IsFound, CDefNode, "name", "");
                                                                            //
                                                                            // setup cdef rule
                                                                            //
                                                                            int ContentID = Models.Domain.CDefModel.getContentId(core, ContentName);
                                                                            if (ContentID > 0) {
                                                                                int CS = core.db.csInsertRecord("Add-on Collection CDef Rules", 0);
                                                                                if (core.db.csOk(CS)) {
                                                                                    core.db.csSet(CS, "Contentid", ContentID);
                                                                                    core.db.csSet(CS, "CollectionID", collection.id);
                                                                                }
                                                                                core.db.csClose(ref CS);
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
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 3");
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
                                                                case "data":
                                                                    //
                                                                    // import content
                                                                    //   This can only be done with matching guid
                                                                    //
                                                                    foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                        if (GenericController.vbLCase(ContentNode.Name) == "record") {
                                                                            //
                                                                            // Data.Record node
                                                                            //
                                                                            string ContentName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                            if (string.IsNullOrEmpty(ContentName)) {
                                                                                LogController.logInfo(core, logPrefix + " ,install collection file contains a data.record node with a blank content attribute.");
                                                                                result = false;
                                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                            } else {
                                                                                string ContentRecordGuid = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                                string ContentRecordName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                                if ((string.IsNullOrEmpty(ContentRecordGuid)) && (string.IsNullOrEmpty(ContentRecordName))) {
                                                                                    LogController.logInfo(core, logPrefix + " ,install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" + ContentName + "]");
                                                                                    result = false;
                                                                                    return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" + ContentName + "].</P>";
                                                                                } else {
                                                                                    //
                                                                                    // create or update the record
                                                                                    //
                                                                                    Models.Domain.CDefModel CDef = Models.Domain.CDefModel.getCdef(core, ContentName);
                                                                                    int cs = -1;
                                                                                    if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                        cs = core.db.csOpen(ContentName, "ccguid=" + core.db.encodeSQLText(ContentRecordGuid));
                                                                                    } else {
                                                                                        cs = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(ContentRecordName));
                                                                                    }
                                                                                    bool recordfound = true;
                                                                                    if (!core.db.csOk(cs)) {
                                                                                        //
                                                                                        // Insert the new record
                                                                                        //
                                                                                        recordfound = false;
                                                                                        core.db.csClose(ref cs);
                                                                                        cs = core.db.csInsertRecord(ContentName, 0);
                                                                                    }
                                                                                    if (core.db.csOk(cs)) {
                                                                                        //
                                                                                        // Update the record
                                                                                        //
                                                                                        if (recordfound && (!string.IsNullOrEmpty(ContentRecordGuid))) {
                                                                                            //
                                                                                            // found by guid, use guid in list and save name
                                                                                            //
                                                                                            core.db.csSet(cs, "name", ContentRecordName);
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
                                                                                            core.db.csSet(cs, "ccguid", ContentRecordGuid);
                                                                                            core.db.csSet(cs, "name", ContentRecordName);
                                                                                            DataRecordList = DataRecordList + "\r\n" + ContentName + "," + ContentRecordGuid;
                                                                                        }
                                                                                    }
                                                                                    core.db.csClose(ref cs);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                        //
                                                        //-------------------------------------------------------------------------------
                                                        // ----- Pass 5, all other collection nodes
                                                        //
                                                        // Process all non-import <Collection> nodes
                                                        //-------------------------------------------------------------------------------
                                                        //
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 5");
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
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
                                                                    string ChildCollectionName = XmlController.GetXMLAttribute(core, Found, CDefSection, "name", "");
                                                                    string ChildCollectionGUID = XmlController.GetXMLAttribute(core, Found, CDefSection, "guid", CDefSection.InnerText);
                                                                    if (string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                        ChildCollectionGUID = CDefSection.InnerText;
                                                                    }
                                                                    if (!string.IsNullOrEmpty(ChildCollectionGUID)) {
                                                                        int ChildCollectionID = 0;
                                                                        int cs = -1;
                                                                        cs = core.db.csOpen("Add-on Collections", "ccguid=" + core.db.encodeSQLText(ChildCollectionGUID));
                                                                        if (core.db.csOk(cs)) {
                                                                            ChildCollectionID = core.db.csGetInteger(cs, "id");
                                                                        }
                                                                        core.db.csClose(ref cs);
                                                                        if (ChildCollectionID != 0) {
                                                                            cs = core.db.csInsertRecord("Add-on Collection Parent Rules", 0);
                                                                            if (core.db.csOk(cs)) {
                                                                                core.db.csSet(cs, "ParentID", collection.id);
                                                                                core.db.csSet(cs, "ChildID", ChildCollectionID);
                                                                            }
                                                                            core.db.csClose(ref cs);
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
                                                                    //    ScriptingName =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "name", "No Name")
                                                                    //    If ScriptingName = "" Then
                                                                    //        ScriptingName = "No Name"
                                                                    //    End If
                                                                    //    ScriptingGuid =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "guid", AOName)
                                                                    //    If ScriptingGuid = "" Then
                                                                    //        ScriptingGuid = ScriptingName
                                                                    //    End If
                                                                    //    Criteria = "(ccguid=" & core.db.encodeSQLText(ScriptingGuid) & ")"
                                                                    //    ScriptingModuleID = 0
                                                                    //    CS = core.db.cs_open("Scripting Modules", Criteria)
                                                                    //    If core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' Update the Addon
                                                                    //        '
                                                                    //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, GUID match with existing scripting module, Updating module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                    //    Else
                                                                    //        '
                                                                    //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                    //        '
                                                                    //        Call core.db.cs_Close(CS)
                                                                    //        Criteria = "(name=" & core.db.encodeSQLText(ScriptingName) & ")and(ccguid is null)"
                                                                    //        CS = core.db.cs_open("Scripting Modules", Criteria)
                                                                    //        If core.db.cs_ok(CS) Then
                                                                    //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Scripting Module matched an existing Module that has no GUID, Updating to [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                    //        End If
                                                                    //    End If
                                                                    //    If Not core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' not found by GUID or by name, Insert a new
                                                                    //        '
                                                                    //        Call core.db.cs_Close(CS)
                                                                    //        CS = core.db.cs_insertRecord("Scripting Modules", 0)
                                                                    //        If core.db.cs_ok(CS) Then
                                                                    //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Creating new Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                    //        End If
                                                                    //    End If
                                                                    //    If Not core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' Could not create new
                                                                    //        '
                                                                    //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Scripting Module could not be created, skipping Scripting Module [" & ScriptingName & "], Guid [" & ScriptingGuid & "]")
                                                                    //    Else
                                                                    //        ScriptingModuleID = core.db.cs_getInteger(CS, "ID")
                                                                    //        Call core.db.cs_set(CS, "code", CDefSection.InnerText)
                                                                    //        Call core.db.cs_set(CS, "name", ScriptingName)
                                                                    //        Call core.db.cs_set(CS, "ccguid", ScriptingGuid)
                                                                    //    End If
                                                                    //    Call core.db.cs_Close(CS)
                                                                    //    If ScriptingModuleID <> 0 Then
                                                                    //        '
                                                                    //        ' Add Add-on Collection Module Rule
                                                                    //        '
                                                                    //        CS = core.db.cs_insertRecord("Add-on Collection Module Rules", 0)
                                                                    //        If core.db.cs_ok(CS) Then
                                                                    //            Call core.db.cs_set(CS, "Collectionid", CollectionID)
                                                                    //            Call core.db.cs_set(CS, "ScriptingModuleID", ScriptingModuleID)
                                                                    //        End If
                                                                    //        Call core.db.cs_Close(CS)
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
                                                                    //    NodeName =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "name", "No Name")
                                                                    //    If NodeName = "" Then
                                                                    //        NodeName = "No Name"
                                                                    //    End If
                                                                    //    nodeGuid =xmlController.GetXMLAttribute(core,IsFound, CDefSection, "guid", AOName)
                                                                    //    If nodeGuid = "" Then
                                                                    //        nodeGuid = NodeName
                                                                    //    End If
                                                                    //    Criteria = "(ccguid=" & core.db.encodeSQLText(nodeGuid) & ")"
                                                                    //    ScriptingModuleID = 0
                                                                    //    CS = core.db.cs_open("Shared Styles", Criteria)
                                                                    //    If core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' Update the Addon
                                                                    //        '
                                                                    //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, GUID match with existing shared style, Updating [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                    //    Else
                                                                    //        '
                                                                    //        ' not found by GUID - search name against name to update legacy Add-ons
                                                                    //        '
                                                                    //        Call core.db.cs_Close(CS)
                                                                    //        Criteria = "(name=" & core.db.encodeSQLText(NodeName) & ")and(ccguid is null)"
                                                                    //        CS = core.db.cs_open("shared styles", Criteria)
                                                                    //        If core.db.cs_ok(CS) Then
                                                                    //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, shared style matched an existing Module that has no GUID, Updating to [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                    //        End If
                                                                    //    End If
                                                                    //    If Not core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' not found by GUID or by name, Insert a new
                                                                    //        '
                                                                    //        Call core.db.cs_Close(CS)
                                                                    //        CS = core.db.cs_insertRecord("shared styles", 0)
                                                                    //        If core.db.cs_ok(CS) Then
                                                                    //            Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, Creating new shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                    //        End If
                                                                    //    End If
                                                                    //    If Not core.db.cs_ok(CS) Then
                                                                    //        '
                                                                    //        ' Could not create new
                                                                    //        '
                                                                    //        Call logcontroller.appendInstallLog(core, "UpgradeAppFromLocalCollection, shared style could not be created, skipping shared style [" & NodeName & "], Guid [" & nodeGuid & "]")
                                                                    //    Else
                                                                    //        sharedStyleId = core.db.cs_getInteger(CS, "ID")
                                                                    //        Call core.db.cs_set(CS, "StyleFilename", CDefSection.InnerText)
                                                                    //        Call core.db.cs_set(CS, "name", NodeName)
                                                                    //        Call core.db.cs_set(CS, "ccguid", nodeGuid)
                                                                    //        Call core.db.cs_set(CS, "alwaysInclude",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "alwaysinclude", "0"))
                                                                    //        Call core.db.cs_set(CS, "prefix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "prefix", ""))
                                                                    //        Call core.db.cs_set(CS, "suffix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "suffix", ""))
                                                                    //        Call core.db.cs_set(CS, "suffix",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "suffix", ""))
                                                                    //        Call core.db.cs_set(CS, "sortOrder",xmlController.GetXMLAttribute(core,IsFound, CDefSection, "sortOrder", ""))
                                                                    //    End If
                                                                    //    Call core.db.cs_Close(CS)
                                                                    break;
                                                                case "addon":
                                                                case "add-on":
                                                                    //
                                                                    // Add-on Node, do part 1 of 2
                                                                    //   (include add-on node must be done after all add-ons are installed)
                                                                    //
                                                                    InstallCollectionFromLocalRepo_addonNode_Phase1(core, CDefSection, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    if (!result) {
                                                                        //result = result;
                                                                    }
                                                                    break;
                                                                case "interfaces":
                                                                    //
                                                                    // Legacy Interface Node
                                                                    //
                                                                    foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes) {
                                                                        InstallCollectionFromLocalRepo_addonNode_Phase1(core, CDefInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
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
                                                                    //    '    Call logcontroller.appendInstallLog(core, "Addon Collection for [" & Collectionname & "] contained an unknown node [" & CDefSection.Name & "]. This node will be ignored.")
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
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 6");
                                                        foreach (XmlNode collectionNode in Doc.DocumentElement.ChildNodes) {
                                                            switch (collectionNode.Name.ToLower()) {
                                                                case "addon":
                                                                case "add-on":
                                                                    //
                                                                    // Add-on Node, do part 1, verify the addon in the table with name and guid
                                                                    string addonName = XmlController.GetXMLAttribute(core, IsFound, collectionNode, "name", collectionNode.Name);
                                                                    if (addonName.ToLower() == "_oninstall") {
                                                                        onInstallAddonGuid = XmlController.GetXMLAttribute(core, IsFound, collectionNode, "guid", collectionNode.Name);
                                                                    }
                                                                    installCollectionFromLocalRepo_addonNode_Phase2(core, collectionNode, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                    break;
                                                                case "interfaces":
                                                                    //
                                                                    // Legacy Interface Node
                                                                    //
                                                                    foreach (XmlNode CDefInterfaces in collectionNode.ChildNodes) {
                                                                        installCollectionFromLocalRepo_addonNode_Phase2(core, CDefInterfaces, "ccguid", core.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
                                                                        if (!result) {
                                                                            //result = result;
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
                                                        LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], pass 4");
                                                        foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes) {
                                                            switch (GenericController.vbLCase(CDefSection.Name)) {
                                                                case "data":
                                                                    foreach (XmlNode ContentNode in CDefSection.ChildNodes) {
                                                                        if (ContentNode.Name.ToLower() == "record") {
                                                                            string ContentName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "content", "");
                                                                            if (string.IsNullOrEmpty(ContentName)) {
                                                                                LogController.logInfo(core, logPrefix + " ,install collection file contains a data.record node with a blank content attribute.");
                                                                                result = false;
                                                                                return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
                                                                            } else {
                                                                                string ContentRecordGuid = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "guid", "");
                                                                                string ContentRecordName = XmlController.GetXMLAttribute(core, IsFound, ContentNode, "name", "");
                                                                                if ((!string.IsNullOrEmpty(ContentRecordGuid)) | (!string.IsNullOrEmpty(ContentRecordName))) {
                                                                                    Models.Domain.CDefModel CDef = Models.Domain.CDefModel.getCdef(core, ContentName);
                                                                                    int cs = -1;
                                                                                    if (!string.IsNullOrEmpty(ContentRecordGuid)) {
                                                                                        cs = core.db.csOpen(ContentName, "ccguid=" + core.db.encodeSQLText(ContentRecordGuid));
                                                                                    } else {
                                                                                        cs = core.db.csOpen(ContentName, "name=" + core.db.encodeSQLText(ContentRecordName));
                                                                                    }
                                                                                    if (core.db.csOk(cs)) {
                                                                                        //
                                                                                        // Update the record
                                                                                        foreach (XmlNode FieldNode in ContentNode.ChildNodes) {
                                                                                            if (FieldNode.Name.ToLower() == "field") {
                                                                                                bool IsFieldFound = false;
                                                                                                string FieldName = XmlController.GetXMLAttribute(core, IsFound, FieldNode, "name", "").ToLower();
                                                                                                int fieldTypeId = -1;
                                                                                                int FieldLookupContentID = -1;
                                                                                                foreach (var keyValuePair in CDef.fields) {
                                                                                                    Models.Domain.CDefFieldModel field = keyValuePair.Value;
                                                                                                    if (GenericController.vbLCase(field.nameLc) == FieldName) {
                                                                                                        fieldTypeId = field.fieldTypeId;
                                                                                                        FieldLookupContentID = field.lookupContentID;
                                                                                                        IsFieldFound = true;
                                                                                                        break;
                                                                                                    }
                                                                                                }
                                                                                                if (IsFieldFound) {
                                                                                                    string FieldValue = FieldNode.InnerText;
                                                                                                    switch (fieldTypeId) {
                                                                                                        case FieldTypeIdAutoIdIncrement:
                                                                                                        case FieldTypeIdRedirect: {
                                                                                                                //
                                                                                                                // not supported
                                                                                                                break;
                                                                                                            }
                                                                                                        case FieldTypeIdLookup: {
                                                                                                                //
                                                                                                                // read in text value, if a guid, use it, otherwise assume name
                                                                                                                if (FieldLookupContentID != 0) {
                                                                                                                    string FieldLookupContentName = Models.Domain.CDefModel.getContentNameByID(core, FieldLookupContentID);
                                                                                                                    if (!string.IsNullOrEmpty(FieldLookupContentName)) {
                                                                                                                        if ((FieldValue.Left(1) == "{") && (FieldValue.Substring(FieldValue.Length - 1) == "}") && Models.Domain.CDefModel.isContentFieldSupported(core, FieldLookupContentName, "ccguid")) {
                                                                                                                            //
                                                                                                                            // Lookup by guid
                                                                                                                            int fieldLookupId = GenericController.encodeInteger(core.db.getRecordIDByGuid(FieldLookupContentName, FieldValue));
                                                                                                                            if (fieldLookupId <= 0) {
                                                                                                                                return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                            } else {
                                                                                                                                core.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                            }
                                                                                                                        } else {
                                                                                                                            //
                                                                                                                            // lookup by name
                                                                                                                            if (!string.IsNullOrEmpty(FieldValue)) {
                                                                                                                                int fieldLookupId = core.db.getRecordID(FieldLookupContentName, FieldValue);
                                                                                                                                if (fieldLookupId <= 0) {
                                                                                                                                    return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
                                                                                                                                } else {
                                                                                                                                    core.db.csSet(cs, FieldName, fieldLookupId);
                                                                                                                                }
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                                                                                } else if (FieldValue.IsNumeric()) {
                                                                                                                    //
                                                                                                                    // must be lookup list
                                                                                                                    core.db.csSet(cs, FieldName, FieldValue);
                                                                                                                }
                                                                                                                break;
                                                                                                            }
                                                                                                        default: {
                                                                                                                core.db.csSet(cs, FieldName, FieldValue);
                                                                                                                break;
                                                                                                            }
                                                                                                    }
                                                                                                }
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                    core.db.csClose(ref cs);
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                    break;
                                                            }
                                                        }
                                                        //
                                                        // --- end of pass
                                                        //
                                                    }
                                                    collection.dataRecordList = DataRecordList;
                                                    collection.save(core);
                                                    //
                                                    // -- execute onInstall addon if found
                                                    if (!string.IsNullOrEmpty(onInstallAddonGuid)) {
                                                        var addon = Models.Db.AddonModel.create(core, onInstallAddonGuid);
                                                        if (addon != null) {
                                                            var executeContext = new BaseClasses.CPUtilsBaseClass.addonExecuteContext() {
                                                                addonType = BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple,
                                                                errorContextMessage = "calling onInstall Addon [" + addon.name + "] for collection [" + collection.name + "]"
                                                            };
                                                            core.addon.execute(addon, executeContext);
                                                        }
                                                    }
                                                }
                                                //
                                                LogController.logInfo(core, logPrefix + " ,install collection [" + Collectionname + "], upgrade complete, flush cache");
                                                //
                                                // -- import complete, flush caches
                                                core.cache.invalidateAll();
                                            }
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
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        //
        private static void UpdateConfig(CoreController core, string Collectionname, string CollectionGuid, DateTime CollectionUpdatedDate, string CollectionVersionFolderName) {
            try {
                //
                bool loadOK = true;
                string LocalFilename = null;
                string LocalGuid = null;
                XmlDocument Doc = new XmlDocument();
                XmlNode NewCollectionNode = null;
                XmlNode NewAttrNode = null;
                bool CollectionFound = false;
                //
                loadOK = true;
                try {
                    Doc.LoadXml(getLocalCollectionStoreListXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, "UpdateConfig, Error loading Collections.xml file.");
                }
                if (loadOK) {
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, "UpdateConfig, The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
                    } else {
                        if (GenericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist") {
                            CollectionFound = false;
                            foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                                switch (GenericController.vbLCase(LocalListNode.Name)) {
                                    case "collection":
                                        LocalGuid = "";
                                        foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                            switch (GenericController.vbLCase(CollectionNode.Name)) {
                                                case "guid":
                                                    //
                                                    LocalGuid = GenericController.vbLCase(CollectionNode.InnerText);
                                                    goto ExitLabel1;
                                            }
                                        }
                                        ExitLabel1:
                                        if (GenericController.vbLCase(LocalGuid) == GenericController.vbLCase(CollectionGuid)) {
                                            CollectionFound = true;
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                switch (GenericController.vbLCase(CollectionNode.Name)) {
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
                            LocalFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                            //LocalFilename = GetProgramPath & "\Addons\Collections.xml"
                            core.privateFiles.saveFile(LocalFilename, Doc.OuterXml);
                            //Doc.Save(core.privateFiles.localAbsRootPath + LocalFilename);
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        //
        private static string GetCollectionPath(CoreController core, string CollectionGuid) {
            string result = "";
            try {
                DateTime LastChangeDate = default(DateTime);
                string Collectionname = "";
                getCollectionConfig(core, CollectionGuid, ref result, ref LastChangeDate, ref Collectionname);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collection path, lastChangeDate, and collectionName given the guid
        /// </summary>
        public static void getCollectionConfig(CoreController core, string CollectionGuid, ref string return_CollectionPath, ref DateTime return_LastChagnedate, ref string return_CollectionName) {
            try {
                string LocalGuid = "";
                XmlDocument Doc = new XmlDocument();
                string collectionPath = "";
                DateTime lastChangeDate = default(DateTime);
                string hint = "";
                string localName = null;
                bool loadOK = false;
                //
                return_CollectionPath = "";
                return_LastChagnedate = DateTime.MinValue;
                loadOK = true;
                try {
                    Doc.LoadXml(getLocalCollectionStoreListXml(core));
                } catch (Exception) {
                    LogController.logInfo(core, "GetCollectionConfig, Hint=[" + hint + "], Error loading Collections.xml file.");
                    loadOK = false;
                }
                if (loadOK) {
                    if (GenericController.vbLCase(Doc.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        LogController.logInfo(core, "Hint=[" + hint + "], The Collections.xml file has an invalid root node");
                    } else {
                        foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes) {
                            localName = "no name found";
                            switch (GenericController.vbLCase(LocalListNode.Name)) {
                                case "collection":
                                    LocalGuid = "";
                                    foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                        switch (GenericController.vbLCase(CollectionNode.Name)) {
                                            case "name":
                                                //
                                                // no - cannot change the case if files are already saved
                                                localName = CollectionNode.InnerText;
                                                //LocalName = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "guid":
                                                //
                                                // no - cannot change the case if files are already saved
                                                LocalGuid = CollectionNode.InnerText;
                                                //LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "path":
                                                //
                                                // no - cannot change the case if files are already saved
                                                collectionPath = CollectionNode.InnerText;
                                                //CollectionPath = genericController.vbLCase(CollectionNode.InnerText);
                                                break;
                                            case "lastchangedate":
                                                lastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                            if (CollectionGuid.ToLower() == LocalGuid.ToLower()) {
                                return_CollectionPath = collectionPath;
                                return_LastChagnedate = lastChangeDate;
                                return_CollectionName = localName;
                                break;
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs Addons in a source folder
        /// </summary>
        public static bool installCollectionsFromPrivateFolder(CoreController core, string privateFolder, ref string return_ErrorMessage, ref List<string> return_CollectionGUIDList, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            bool returnSuccess = false;
            try {
                logPrefix += ", installCollectionsFromPrivateFolder";
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = buildLocalCollectionReposFromFolder(core, privateFolder, CollectionLastChangeDate, ref return_CollectionGUIDList, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    LogController.logInfo(core, logPrefix +", BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    foreach (string collectionGuid in return_CollectionGUIDList) {
                        if (!installCollectionFromLocalRepo(core, collectionGuid, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections)) {
                            LogController.logInfo(core, logPrefix + ", UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                            break;
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Installs Addons in a source file
        /// </summary>
        public static bool installCollectionsFromPrivateFile(CoreController core, string pathFilename, ref string return_ErrorMessage, ref string return_CollectionGUID, bool IsNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            bool returnSuccess = false;
            try {
                logPrefix += ", installCollectionsFromPrivateFile";
                DateTime CollectionLastChangeDate;
                //
                CollectionLastChangeDate = DateTime.Now;
                returnSuccess = buildLocalCollectionRepoFromFile(core, pathFilename, CollectionLastChangeDate, ref return_CollectionGUID, ref return_ErrorMessage, false);
                if (!returnSuccess) {
                    //
                    // BuildLocal failed, log it and do not upgrade
                    //
                    LogController.logInfo(core, logPrefix +  ", BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
                } else {
                    returnSuccess = installCollectionFromLocalRepo(core, return_CollectionGUID, core.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                    if (!returnSuccess) {
                        //
                        // Upgrade all apps failed
                        //
                        LogController.logInfo(core, logPrefix + ", UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
                    } else {
                        returnSuccess = true;
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                returnSuccess = false;
                if (string.IsNullOrEmpty(return_ErrorMessage)) {
                    return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
                }
            }
            return returnSuccess;
        }
        //
        //======================================================================================================
        //
        private static int getNavIDByGuid(CoreController core, string ccGuid) {
            int navId = 0;
            try {
                int CS;
                //
                CS = core.db.csOpen(cnNavigatorEntries, "ccguid=" + core.db.encodeSQLText(ccGuid), "ID",true,0,false,false, "ID");
                if (core.db.csOk(CS)) {
                    navId = core.db.csGetInteger(CS, "id");
                }
                core.db.csClose(ref CS);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return navId;
        }
        //
        //======================================================================================================
        /// <summary>
        /// copy resources from install folder to www folder
        /// </summary>
        private static void copyInstallPathToDstPath(CoreController core, string SrcPath, string DstPath, string BlockFileList, string BlockFolderList) {
            try {
                
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
                if (core.privateFiles.pathExists(SrcFolder)) {
                    List< FileDetail> FileInfoArray = core.privateFiles.getFileList(SrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
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
                            core.privateFiles.copyFile(SrcPath + file.Name, DstPath + file.Name, core.appRootFiles);
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(SrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        if (("," + BlockFolderList + ",").IndexOf("," + folder.Name + ",", System.StringComparison.OrdinalIgnoreCase)  == -1) {
                            copyInstallPathToDstPath(core, SrcPath + folder.Name + "\\", DstPath + folder.Name + "\\", BlockFileList, "");
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        //
        private static string GetCollectionFileList(CoreController core, string SrcPath, string SubFolder, string ExcludeFileList) {
            string result = "";
            try {
                string SrcFolder;
                //
                SrcFolder = SrcPath + SubFolder;
                if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\") {
                    SrcFolder = SrcFolder.Left( SrcFolder.Length - 1);
                }
                //
                if (core.privateFiles.pathExists(SrcFolder)) {
                    List<FileDetail> FileInfoArray = core.privateFiles.getFileList(SrcFolder);
                    foreach (FileDetail file in FileInfoArray) {
                        if (("," + ExcludeFileList + ",").IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase)  != -1) {
                            //
                            // can not copy the current collection file
                            //
                            //Filename = Filename
                        } else {
                            //
                            // copy this file to destination
                            //
                            result += "\r\n" + SubFolder + file.Name;
                            //runAtServer.IPAddress = "127.0.0.1"
                            //runAtServer.Port = "4531"
                            //QS = "SrcFile=" & encodeRequestVariable(SrcPath & Filename) & "&DstFile=" & encodeRequestVariable(DstPath & Filename)
                            //Call runAtServer.ExecuteCmd("CopyFile", QS)
                            //Call core.app.privateFiles.CopyFile(SrcPath & Filename, DstPath & Filename)
                        }
                    }
                    //
                    // copy folders to dst
                    //
                    List<FolderDetail> FolderInfoArray = core.privateFiles.getFolderList(SrcFolder);
                    foreach (FolderDetail folder in FolderInfoArray) {
                        result += GetCollectionFileList(core, SrcPath, SubFolder + folder.Name + "\\", ExcludeFileList);
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        //
        private static void InstallCollectionFromLocalRepo_addonNode_Phase1(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool return_UpgradeOK, ref string return_ErrorMessage) {
            try {
                string Basename = GenericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    bool IsFound = false;
                    string addonName = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(addonName)) {
                        addonName = "No Name";
                    }
                    string addonGuid = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "guid", addonName);
                    if (string.IsNullOrEmpty(addonGuid)) {
                        addonGuid = addonName;
                    }
                    string navTypeName = XmlController.GetXMLAttribute(core, IsFound, AddonNode, "type", "");
                    int navTypeId = GetListIndex(navTypeName, navTypeIDList);
                    if (navTypeId == 0) {
                        navTypeId = NavTypeIDAddon;
                    }
                    string Criteria = "(" + AddonGuidFieldName + "=" + core.db.encodeSQLText(addonGuid) + ")";
                    int CS = core.db.csOpen(cnAddons, Criteria, "", false);
                    if (core.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        core.db.csClose(ref CS);
                        Criteria = "(name=" + core.db.encodeSQLText(addonName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = core.db.csOpen(cnAddons, Criteria,"", false);
                        if (core.db.csOk(CS)) {
                            LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // not found by GUID or by name, Insert a new addon
                        //
                        core.db.csClose(ref CS);
                        CS = core.db.csInsertRecord(cnAddons, 0);
                        if (core.db.csOk(CS)) {
                            LogController.logInfo(core, "UpgradeAppFromLocalCollection, Creating new Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                        }
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // Could not create new Add-on
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + addonName + "], Guid [" + addonGuid + "]");
                    } else {
                        int addonId = core.db.csGetInteger(CS, "ID");
                        //
                        // Initialize the add-on
                        // Delete any existing related records - so if this is an update with removed relationships, those are removed
                        //
                        //Call core.db.deleteContentRecords("Shared Styles Add-on Rules", "addonid=" & addonId)
                        //Call core.db.deleteContentRecords("Add-on Scripting Module Rules", "addonid=" & addonId)
                        core.db.deleteContentRecords("Add-on Include Rules", "addonid=" + addonId);
                        core.db.deleteContentRecords("Add-on Content Trigger Rules", "addonid=" + addonId);
                        //
                        core.db.csSet(CS, "collectionid", CollectionID);
                        core.db.csSet(CS, AddonGuidFieldName, addonGuid);
                        core.db.csSet(CS, "name", addonName);
                        core.db.csSet(CS, "navTypeId", navTypeId);
                        string ArgumentList = "";
                        string StyleSheet = "";
                        if (AddonNode.ChildNodes.Count > 0) {
                            foreach (XmlNode PageInterfaceWithinLoop in AddonNode.ChildNodes) {
                                if (!(PageInterfaceWithinLoop is XmlComment)) {
                                    XmlNode PageInterface = PageInterfaceWithinLoop;
                                    string test = null;
                                    int scriptinglanguageid = 0;
                                    string ScriptingCode = null;
                                    string FieldName = null;
                                    string NodeName = null;
                                    string NewValue = null;
                                    string menuNameSpace = null;
                                    string FieldValue = "";
                                    int CS2 = 0;
                                    string ScriptingEntryPoint = null;
                                    int ScriptingTimeout = 0;
                                    string ScriptingLanguage = null;
                                    switch (GenericController.vbLCase(PageInterfaceWithinLoop.Name)) {
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
                                                //
                                                int fieldTypeID = 0;
                                                string fieldType = null;
                                                switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                    case "type":
                                                        fieldType = TriggerNode.InnerText;
                                                        fieldTypeID = core.db.getRecordID("Content Field Types", fieldType);
                                                        if (fieldTypeID > 0) {
                                                            Criteria = "(addonid=" + addonId + ")and(contentfieldTypeID=" + fieldTypeID + ")";
                                                            CS2 = core.db.csOpen("Add-on Content Field Type Rules", Criteria);
                                                            if (!core.db.csOk(CS2)) {
                                                                core.db.csClose(ref CS2);
                                                                CS2 = core.db.csInsertRecord("Add-on Content Field Type Rules", 0);
                                                            }
                                                            if (core.db.csOk(CS2)) {
                                                                core.db.csSet(CS2, "addonid", addonId);
                                                                core.db.csSet(CS2, "contentfieldTypeID", fieldTypeID);
                                                            }
                                                            core.db.csClose(ref CS2);
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
                                                int TriggerContentID = 0;
                                                string ContentNameorGuid = null;
                                                switch (GenericController.vbLCase(TriggerNode.Name)) {
                                                    case "contentchange":
                                                        TriggerContentID = 0;
                                                        ContentNameorGuid = TriggerNode.InnerText;
                                                        if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                            ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "guid", "");
                                                            if (string.IsNullOrEmpty(ContentNameorGuid)) {
                                                                ContentNameorGuid = XmlController.GetXMLAttribute(core, IsFound, TriggerNode, "name", "");
                                                            }
                                                        }
                                                        Criteria = "(ccguid=" + core.db.encodeSQLText(ContentNameorGuid) + ")";
                                                        CS2 = core.db.csOpen("Content", Criteria);
                                                        if (!core.db.csOk(CS2)) {
                                                            core.db.csClose(ref CS2);
                                                            Criteria = "(ccguid is null)and(name=" + core.db.encodeSQLText(ContentNameorGuid) + ")";
                                                            CS2 = core.db.csOpen("content", Criteria);
                                                        }
                                                        if (core.db.csOk(CS2)) {
                                                            TriggerContentID = core.db.csGetInteger(CS2, "ID");
                                                        }
                                                        core.db.csClose(ref CS2);
                                                        //If TriggerContentID = 0 Then
                                                        //    CS2 = core.db.cs_insertRecord("Scripting Modules", 0)
                                                        //    If core.db.cs_ok(CS2) Then
                                                        //        Call core.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                        //        Call core.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                        //        TriggerContentID = core.db.cs_getInteger(CS2, "ID")
                                                        //    End If
                                                        //    Call core.db.cs_Close(CS2)
                                                        //End If
                                                        if (TriggerContentID == 0) {
                                                            //
                                                            // could not find the content
                                                            //
                                                        } else {
                                                            Criteria = "(addonid=" + addonId + ")and(contentid=" + TriggerContentID + ")";
                                                            CS2 = core.db.csOpen("Add-on Content Trigger Rules", Criteria);
                                                            if (!core.db.csOk(CS2)) {
                                                                core.db.csClose(ref CS2);
                                                                CS2 = core.db.csInsertRecord("Add-on Content Trigger Rules", 0);
                                                                if (core.db.csOk(CS2)) {
                                                                    core.db.csSet(CS2, "addonid", addonId);
                                                                    core.db.csSet(CS2, "contentid", TriggerContentID);
                                                                }
                                                            }
                                                            core.db.csClose(ref CS2);
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
                                            ScriptingLanguage = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "language", "");
                                            if (ScriptingLanguage.ToLower() == "jscript") {
                                                scriptinglanguageid = 2;
                                            } else {
                                                scriptinglanguageid = 1;
                                            }
                                            //scriptinglanguageid = core.db.getRecordID("scripting languages", ScriptingLanguage);
                                            core.db.csSet(CS, "scriptinglanguageid", scriptinglanguageid);
                                            ScriptingEntryPoint = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "entrypoint", "");
                                            core.db.csSet(CS, "ScriptingEntryPoint", ScriptingEntryPoint);
                                            ScriptingTimeout = GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "timeout", "5000"));
                                            core.db.csSet(CS, "ScriptingTimeout", ScriptingTimeout);
                                            ScriptingCode = "";
                                            //Call core.app.csv_SetCS(CS, "ScriptingCode", ScriptingCode)
                                            foreach (XmlNode ScriptingNode in PageInterfaceWithinLoop.ChildNodes) {
                                                switch (GenericController.vbLCase(ScriptingNode.Name)) {
                                                    case "code":
                                                        ScriptingCode = ScriptingCode + ScriptingNode.InnerText;
                                                        //Case "includemodule"

                                                        //    ScriptingModuleID = 0
                                                        //    ScriptingNameorGuid = ScriptingNode.InnerText
                                                        //    If ScriptingNameorGuid = "" Then
                                                        //        ScriptingNameorGuid =xmlController.GetXMLAttribute(core,IsFound, ScriptingNode, "guid", "")
                                                        //        If ScriptingNameorGuid = "" Then
                                                        //            ScriptingNameorGuid =xmlController.GetXMLAttribute(core,IsFound, ScriptingNode, "name", "")
                                                        //        End If
                                                        //    End If
                                                        //    Criteria = "(ccguid=" & core.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                        //    CS2 = core.db.cs_open("Scripting Modules", Criteria)
                                                        //    If Not core.db.cs_ok(CS2) Then
                                                        //        Call core.db.cs_Close(CS2)
                                                        //        Criteria = "(ccguid is null)and(name=" & core.db.encodeSQLText(ScriptingNameorGuid) & ")"
                                                        //        CS2 = core.db.cs_open("Scripting Modules", Criteria)
                                                        //    End If
                                                        //    If core.db.cs_ok(CS2) Then
                                                        //        ScriptingModuleID = core.db.cs_getInteger(CS2, "ID")
                                                        //    End If
                                                        //    Call core.db.cs_Close(CS2)
                                                        //    If ScriptingModuleID = 0 Then
                                                        //        CS2 = core.db.cs_insertRecord("Scripting Modules", 0)
                                                        //        If core.db.cs_ok(CS2) Then
                                                        //            Call core.db.cs_set(CS2, "name", ScriptingNameorGuid)
                                                        //            Call core.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
                                                        //            ScriptingModuleID = core.db.cs_getInteger(CS2, "ID")
                                                        //        End If
                                                        //        Call core.db.cs_Close(CS2)
                                                        //    End If
                                                        //    Criteria = "(addonid=" & addonId & ")and(scriptingmoduleid=" & ScriptingModuleID & ")"
                                                        //    CS2 = core.db.cs_open("Add-on Scripting Module Rules", Criteria)
                                                        //    If Not core.db.cs_ok(CS2) Then
                                                        //        Call core.db.cs_Close(CS2)
                                                        //        CS2 = core.db.cs_insertRecord("Add-on Scripting Module Rules", 0)
                                                        //        If core.db.cs_ok(CS2) Then
                                                        //            Call core.db.cs_set(CS2, "addonid", addonId)
                                                        //            Call core.db.cs_set(CS2, "scriptingmoduleid", ScriptingModuleID)
                                                        //        End If
                                                        //    End If
                                                        //    Call core.db.cs_Close(CS2)
                                                        break;
                                                }
                                            }
                                            core.db.csSet(CS, "ScriptingCode", ScriptingCode);
                                            break;
                                        case "activexprogramid":
                                            //
                                            // save program id
                                            //
                                            FieldValue = PageInterfaceWithinLoop.InnerText;
                                            core.db.csSet(CS, "ObjectProgramID", FieldValue);
                                            break;
                                        case "navigator":
                                            //
                                            // create a navigator entry with a parent set to this
                                            //
                                            core.db.csSave(CS);
                                            menuNameSpace = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "NameSpace", "");
                                            if (!string.IsNullOrEmpty(menuNameSpace)) {
                                                string NavIconTypeString = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "type", "");
                                                if (string.IsNullOrEmpty(NavIconTypeString)) {
                                                    NavIconTypeString = "Addon";
                                                }
                                                //Dim builder As New coreBuilderClass(core)
                                                AppBuilderController.verifyNavigatorEntry(core, "", menuNameSpace, addonName, "", "", "", false, false, false, true, addonName, NavIconTypeString, addonName, CollectionID);
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
                                            NodeName = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "name", "");
                                            NewValue = encodeText(PageInterfaceWithinLoop.InnerText).Trim(' ');
                                            if (NewValue.Left(1) != "{") {
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
                                            //    nodeNameOrGuid =xmlController.GetXMLAttribute(core,IsFound, PageInterface, "guid", "")
                                            //    If nodeNameOrGuid = "" Then
                                            //        nodeNameOrGuid =xmlController.GetXMLAttribute(core,IsFound, PageInterface, "name", "")
                                            //    End If
                                            //    Criteria = "(ccguid=" & core.db.encodeSQLText(nodeNameOrGuid) & ")"
                                            //    CS2 = core.db.cs_open("shared styles", Criteria)
                                            //    If Not core.db.cs_ok(CS2) Then
                                            //        Call core.db.cs_Close(CS2)
                                            //        Criteria = "(ccguid is null)and(name=" & core.db.encodeSQLText(nodeNameOrGuid) & ")"
                                            //        CS2 = core.db.cs_open("shared styles", Criteria)
                                            //    End If
                                            //    If core.db.cs_ok(CS2) Then
                                            //        sharedStyleId = core.db.cs_getInteger(CS2, "ID")
                                            //    End If
                                            //    Call core.db.cs_Close(CS2)
                                            //    If sharedStyleId = 0 Then
                                            //        CS2 = core.db.cs_insertRecord("shared styles", 0)
                                            //        If core.db.cs_ok(CS2) Then
                                            //            Call core.db.cs_set(CS2, "name", nodeNameOrGuid)
                                            //            Call core.db.cs_set(CS2, "ccguid", nodeNameOrGuid)
                                            //            sharedStyleId = core.db.cs_getInteger(CS2, "ID")
                                            //        End If
                                            //        Call core.db.cs_Close(CS2)
                                            //    End If
                                            //    Criteria = "(addonid=" & addonId & ")and(StyleId=" & sharedStyleId & ")"
                                            //    CS2 = core.db.cs_open("Shared Styles Add-on Rules", Criteria)
                                            //    If Not core.db.cs_ok(CS2) Then
                                            //        Call core.db.cs_Close(CS2)
                                            //        CS2 = core.db.cs_insertRecord("Shared Styles Add-on Rules", 0)
                                            //        If core.db.cs_ok(CS2) Then
                                            //            Call core.db.cs_set(CS2, "addonid", addonId)
                                            //            Call core.db.cs_set(CS2, "StyleId", sharedStyleId)
                                            //        End If
                                            //    End If
                                            //    Call core.db.cs_Close(CS2)
                                            break;
                                        case "stylesheet":
                                        case "styles":
                                            //
                                            // import exclusive stylesheet if more then whitespace
                                            //
                                            test = PageInterfaceWithinLoop.InnerText;
                                            test = GenericController.vbReplace(test, " ", "");
                                            test = GenericController.vbReplace(test, "\r", "");
                                            test = GenericController.vbReplace(test, "\n", "");
                                            test = GenericController.vbReplace(test, "\t", "");
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
                                            if (!core.db.csIsFieldSupported(CS, FieldName)) {
                                                //
                                                // Bad field name - need to report it somehow
                                                //
                                            } else {
                                                core.db.csSet(CS, FieldName, FieldValue);
                                                if (GenericController.encodeBoolean(PageInterfaceWithinLoop.InnerText)) {
                                                    //
                                                    // if template, admin or content - let non-developers have navigator entry
                                                    //
                                                }
                                            }
                                            break;
                                        case "icon":
                                            //
                                            // icon
                                            //
                                            FieldValue = XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "link", "");
                                            if (!string.IsNullOrEmpty(FieldValue)) {
                                                //
                                                // Icons can be either in the root of the website or in content files
                                                //
                                                FieldValue = GenericController.vbReplace(FieldValue, "\\", "/"); // make it a link, not a file
                                                if (GenericController.vbInstr(1, FieldValue, "://") != 0) {
                                                    //
                                                    // the link is an absolute URL, leave it link this
                                                    //
                                                } else {
                                                    if (FieldValue.Left(1) != "/") {
                                                        //
                                                        // make sure it starts with a slash to be consistance
                                                        //
                                                        FieldValue = "/" + FieldValue;
                                                    }
                                                    if (FieldValue.Left(17) == "/contensivefiles/") {
                                                        //
                                                        // in content files, start link without the slash
                                                        //
                                                        FieldValue = FieldValue.Substring(17);
                                                    }
                                                }
                                                core.db.csSet(CS, "IconFilename", FieldValue);
                                                if (true) {
                                                    core.db.csSet(CS, "IconWidth", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "width", "0")));
                                                    core.db.csSet(CS, "IconHeight", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "height", "0")));
                                                    core.db.csSet(CS, "IconSprites", GenericController.encodeInteger(XmlController.GetXMLAttribute(core, IsFound, PageInterfaceWithinLoop, "sprites", "0")));
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
                                                core.db.csSet(CS, "formxml", PageInterfaceWithinLoop.InnerXml);
                                            }
                                            break;
                                        case "javascript":
                                        case "javascriptinhead":
                                            //
                                            // these all translate to JSFilename
                                            //
                                            FieldName = "jsfilename";
                                            core.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);

                                            break;
                                        case "iniframe":
                                            //
                                            // typo - field is inframe
                                            //
                                            FieldName = "inframe";
                                            core.db.csSet(CS, FieldName, PageInterfaceWithinLoop.InnerText);
                                            break;
                                        default:
                                            //
                                            // All the other fields should match the Db fields
                                            //
                                            FieldName = PageInterfaceWithinLoop.Name;
                                            FieldValue = PageInterfaceWithinLoop.InnerText;
                                            if (!core.db.csIsFieldSupported(CS, FieldName)) {
                                                //
                                                // Bad field name - need to report it somehow
                                                //
                                                LogController.handleError(core, new ApplicationException("bad field found [" + FieldName + "], in addon node [" + addonName + "], of collection [" + core.db.getRecordName("add-on collections", CollectionID) + "]"));
                                            } else {
                                                core.db.csSet(CS, FieldName, FieldValue);
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        core.db.csSet(CS, "ArgumentList", ArgumentList);
                        core.db.csSet(CS, "StylesFilename", StyleSheet);
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
                    core.db.csClose(ref CS);
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
                    //CS = core.db.cs_open("Add-on Collections")
                    //Do While core.db.cs_ok(CS)
                    //    CollectionFile = core.db.cs_get(CS, "InstallFile")
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
                    //                                                        Criteria = "(" & AddonGuidFieldName & "=" & core.db.encodeSQLText(SrcAddonGuid) & ")"
                    //                                                    End If
                    //                                                ElseIf TestName <> "" Then
                    //                                                    If TestName = addonName Then
                    //                                                        Criteria = "(name=" & core.db.encodeSQLText(SrcAddonName) & ")"
                    //                                                    End If
                    //                                                End If
                    //                                                If Criteria <> "" Then
                    //                                                    '$$$$$ cache this
                    //                                                    CS2 = core.db.cs_open(cnAddons, Criteria, "ID")
                    //                                                    If core.db.cs_ok(CS2) Then
                    //                                                        SrcAddonID = core.db.cs_getInteger(CS2, "ID")
                    //                                                    End If
                    //                                                    Call core.db.cs_Close(CS2)
                    //                                                    AddRule = False
                    //                                                    If SrcAddonID = 0 Then
                    //                                                        UserError = "The add-on being installed is referenced by another add-on in collection [], but this add-on could not be found by the respoective criteria [" & Criteria & "]"
                    //                                                        Call logcontroller.appendInstallLog(core,  "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" & UserError & "]")
                    //                                                    Else
                    //                                                        CS2 = core.db.cs_openCsSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" & SrcAddonID & " and IncludedAddonID=" & addonId)
                    //                                                        AddRule = Not core.db.cs_ok(CS2)
                    //                                                        Call core.db.cs_Close(CS2)
                    //                                                    End If
                    //                                                    If AddRule Then
                    //                                                        CS2 = core.db.cs_insertRecord("Add-on Include Rules", 0)
                    //                                                        If core.db.cs_ok(CS2) Then
                    //                                                            Call core.db.cs_set(CS2, "Addonid", SrcAddonID)
                    //                                                            Call core.db.cs_set(CS2, "IncludedAddonID", addonId)
                    //                                                        End If
                    //                                                        Call core.db.cs_Close(CS2)
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
                    //            core.handleExceptionAndContinue(ex) : Throw
                    //        End Try
                    //    End If
                    //    Call core.db.cs_goNext(CS)
                    //Loop
                    //Call core.db.cs_Close(CS)
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// process the include add-on node of the add-on nodes. 
        /// this is the second pass, so all add-ons should be added
        /// no errors for missing addones, except the include add-on case
        /// </summary>
        private static string installCollectionFromLocalRepo_addonNode_Phase2(CoreController core, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage) {
            string result = "";
            try {
                bool AddRule = false;
                string IncludeAddonName = null;
                string IncludeAddonGuid = null;
                int IncludeAddonID = 0;
                string UserError = null;
                int CS2 = 0;
                int CS = 0;
                string Criteria = null;
                bool IsFound = false;
                string AOName = null;
                string AOGuid = null;
                string AddOnType = null;
                int addonId = 0;
                string Basename;
                //
                Basename = GenericController.vbLCase(AddonNode.Name);
                if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on")) {
                    AOName =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "name", "No Name");
                    if (string.IsNullOrEmpty(AOName)) {
                        AOName = "No Name";
                    }
                    AOGuid =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "guid", AOName);
                    if (string.IsNullOrEmpty(AOGuid)) {
                        AOGuid = AOName;
                    }
                    AddOnType =XmlController.GetXMLAttribute(core, IsFound, AddonNode, "type", "");
                    Criteria = "(" + AddonGuidFieldName + "=" + core.db.encodeSQLText(AOGuid) + ")";
                    CS = core.db.csOpen(cnAddons, Criteria,"", false);
                    if (core.db.csOk(CS)) {
                        //
                        // Update the Addon
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        //
                        // not found by GUID - search name against name to update legacy Add-ons
                        //
                        core.db.csClose(ref CS);
                        Criteria = "(name=" + core.db.encodeSQLText(AOName) + ")and(" + AddonGuidFieldName + " is null)";
                        CS = core.db.csOpen(cnAddons, Criteria,"", false);
                    }
                    if (!core.db.csOk(CS)) {
                        //
                        // Could not find add-on
                        //
                        LogController.logInfo(core, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + AOName + "], Guid [" + AOGuid + "]");
                    } else {
                        addonId = core.db.csGetInteger(CS, "ID");
                        if (AddonNode.ChildNodes.Count > 0) {
                            foreach (XmlNode PageInterface in AddonNode.ChildNodes) {
                                switch (GenericController.vbLCase(PageInterface.Name)) {
                                    case "includeaddon":
                                    case "includeadd-on":
                                    case "include addon":
                                    case "include add-on":
                                        //
                                        // include add-ons - NOTE - import collections must be run before interfaces
                                        // when importing a collectin that will be used for an include
                                        //
                                        if (true) {
                                            IncludeAddonName =XmlController.GetXMLAttribute(core, IsFound, PageInterface, "name", "");
                                            IncludeAddonGuid =XmlController.GetXMLAttribute(core, IsFound, PageInterface, "guid", IncludeAddonName);
                                            IncludeAddonID = 0;
                                            Criteria = "";
                                            if (!string.IsNullOrEmpty(IncludeAddonGuid)) {
                                                Criteria = AddonGuidFieldName + "=" + core.db.encodeSQLText(IncludeAddonGuid);
                                                if (string.IsNullOrEmpty(IncludeAddonName)) {
                                                    IncludeAddonName = "Add-on " + IncludeAddonGuid;
                                                }
                                            } else if (!string.IsNullOrEmpty(IncludeAddonName)) {
                                                Criteria = "(name=" + core.db.encodeSQLText(IncludeAddonName) + ")";
                                            }
                                            if (!string.IsNullOrEmpty(Criteria)) {
                                                CS2 = core.db.csOpen(cnAddons, Criteria);
                                                if (core.db.csOk(CS2)) {
                                                    IncludeAddonID = core.db.csGetInteger(CS2, "ID");
                                                }
                                                core.db.csClose(ref CS2);
                                                AddRule = false;
                                                if (IncludeAddonID == 0) {
                                                    UserError = "The include add-on [" + IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it.";
                                                    LogController.logInfo(core, "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" + UserError + "]");
                                                    ReturnUpgradeOK = false;
                                                    ReturnErrorMessage = ReturnErrorMessage + "<P>The collection was not installed because the add-on [" + AOName + "] requires an included add-on [" + IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>";
                                                } else {
                                                    CS2 = core.db.csOpenSql( "select ID from ccAddonIncludeRules where Addonid=" + addonId + " and IncludedAddonID=" + IncludeAddonID);
                                                    AddRule = !core.db.csOk(CS2);
                                                    core.db.csClose(ref CS2);
                                                }
                                                if (AddRule) {
                                                    CS2 = core.db.csInsertRecord("Add-on Include Rules", 0);
                                                    if (core.db.csOk(CS2)) {
                                                        core.db.csSet(CS2, "Addonid", addonId);
                                                        core.db.csSet(CS2, "IncludedAddonID", IncludeAddonID);
                                                    }
                                                    core.db.csClose(ref CS2);
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    core.db.csClose(ref CS);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
            //
        }
        //
        //======================================================================================================
        /// <summary>
        /// Import CDef on top of current configuration and the base configuration
        /// </summary>
        public static void installBaseCollection(CoreController core, bool isNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            try {
                logPrefix += ", installBaseCollection";
                //
                // -- new build
                // 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
                const string baseCollectionFilename = "aoBase5.xml";
                string baseCollectionXml = core.programFiles.readFileText(baseCollectionFilename);
                if (string.IsNullOrEmpty(baseCollectionXml)) {
                    //
                    // -- base collection notfound
                    throw new ApplicationException("Cannot load aoBase5.xml [" + core.programFiles.localAbsRootPath + "aoBase5.xml]");
                } else {
                    LogController.logInfo(core, logPrefix + ", Verify base collection -- new build");
                    MiniCollectionModel baseCollection = installCollection_LoadXmlToMiniCollection(core, baseCollectionXml, true, true, isNewBuild, new MiniCollectionModel());
                    installCollection_BuildDbFromMiniCollection(core, baseCollection, core.siteProperties.dataBuildVersion, isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                    //
                    // now treat as a regular collection and install - to pickup everything else 
                    string tmpFolderPath = "installBaseCollection" + GenericController.GetRandomInteger(core).ToString() + "\\";
                    core.privateFiles.createPath(tmpFolderPath);
                    core.programFiles.copyFile(baseCollectionFilename, tmpFolderPath + baseCollectionFilename, core.privateFiles);
                    List<string> ignoreList = new List<string>();
                    string returnErrorMessage = "";
                    if (!installCollectionsFromPrivateFolder(core, tmpFolderPath, ref returnErrorMessage, ref ignoreList, isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections)) {
                        throw new ApplicationException(returnErrorMessage);
                    }
                    core.privateFiles.deleteFolder(tmpFolderPath);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        //
        public static void installCollectionFromLocalRepo_BuildDbFromXmlData(CoreController core, string XMLText, bool isNewBuild, bool repair, bool isBaseCollection, ref List<string> nonCriticalErrorList, string logPrefix, ref List<string> installedCollections) {
            try {
                //
                LogController.logInfo(core, logPrefix + ", Application: " + core.appConfig.name);
                //
                // ----- Import any CDef files, allowing for changes
                MiniCollectionModel miniCollectionToAdd = new MiniCollectionModel();
                MiniCollectionModel miniCollectionWorking = installCollection_GetApplicationMiniCollection(core, isNewBuild);
                miniCollectionToAdd = installCollection_LoadXmlToMiniCollection(core, XMLText, isBaseCollection, false, isNewBuild, miniCollectionWorking);
                addMiniCollectionSrcToDst(core, ref miniCollectionWorking, miniCollectionToAdd);
                installCollection_BuildDbFromMiniCollection(core, miniCollectionWorking, core.siteProperties.dataBuildVersion, isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //======================================================================================================
        /// <summary>
        /// create a collection class from a collection xml file, cdef are added to the cdefs in the application collection
        /// </summary>
        private static MiniCollectionModel installCollection_LoadXmlToMiniCollection(CoreController core, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, MiniCollectionModel defaultCollection) {
            MiniCollectionModel result = null;
            try {
                Models.Domain.CDefModel DefaultCDef = null;
                Models.Domain.CDefFieldModel DefaultCDefField = null;
                string contentNameLc = null;
                CollectionXmlController XMLTools = new CollectionXmlController(core);
                //Dim AddonClass As New addonInstallClass(core)
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
                string DataSourceName = null;
                XmlDocument srcXmlDom = new XmlDocument();
                string NodeName = null;
                //
                LogController.logInfo(core, "Application: " + core.appConfig.name + ", UpgradeCDef_LoadDataToCollection");
                //
                result = new MiniCollectionModel();
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
                        LogController.logError(core, "UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" + ex.ToString() + "]");
                        throw new Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex);
                    }
                    if ((srcXmlDom.DocumentElement.Name.ToLower() != CollectionFileRootNode) & (srcXmlDom.DocumentElement.Name.ToLower() != "contensivecdef")) {
                        //
                        // -- root node must be collection (or legacy contensivecdef)
                        LogController.handleError( core,new ApplicationException("the archive file has a syntax error. Application name must be the first node."));
                    } else {
                        result.isBaseCollection = IsccBaseFile;
                        //
                        // Get Collection Name for logs
                        //
                        //hint = "get collection name"
                        Collectionname =XmlController.GetXMLAttribute(core, Found, srcXmlDom.DocumentElement, "name", "");
                        if (string.IsNullOrEmpty(Collectionname)) {
                            LogController.logInfo(core, "UpgradeCDef_LoadDataToCollection, Application: " + core.appConfig.name + ", Collection has no name");
                        } else {
                            //Call AppendClassLogFile(core.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & core.app.appEnvironment.name & ", Collection: " & Collectionname)
                        }
                        result.name = Collectionname;
                        //
                        // Load possible DefaultSortMethods
                        //
                        //hint = "preload sort methods"
                        //SortMethodList = vbTab & "By Name" & vbTab & "By Alpha Sort Order Field" & vbTab & "By Date" & vbTab & "By Date Reverse"
                        //If core.app.csv_IsContentFieldSupported("Sort Methods", "ID") Then
                        //    CS = core.app.OpenCSContent("Sort Methods", , , , , , , "Name")
                        //    Do While core.app.IsCSOK(CS)
                        //        SortMethodList = SortMethodList & vbTab & core.app.cs_getText(CS, "name")
                        //        core.app.nextCSRecord(CS)
                        //    Loop
                        //    Call core.app.closeCS(CS)
                        //End If
                        //SortMethodList = SortMethodList & vbTab
                        //
                        foreach (XmlNode CDef_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes) {
                            CDef_Node = CDef_NodeWithinLoop;
                            //isCdefTarget = False
                            NodeName = GenericController.vbLCase(CDef_NodeWithinLoop.Name);
                            //hint = "read node " & NodeName
                            switch (NodeName) {
                                case "cdef":
                                    //
                                    // Content Definitions
                                    //
                                    ContentName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "name", "");
                                    contentNameLc = GenericController.vbLCase(ContentName);
                                    if (string.IsNullOrEmpty(ContentName)) {
                                        throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "collection file contains a CDEF node with no name attribute. This is not allowed.", "dll", "builderClass", "UpgradeCDef_LoadDataToCollection", 0, "", "", False, True, "")
                                    } else {
                                        //
                                        // setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
                                        //
                                        if (defaultCollection.cdef.ContainsKey(contentNameLc)) {
                                            DefaultCDef = defaultCollection.cdef[contentNameLc];
                                        } else {
                                            DefaultCDef = new Models.Domain.CDefModel();
                                        }
                                        //
                                        ContentTableName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.tableName);
                                        if (!string.IsNullOrEmpty(ContentTableName)) {
                                            //
                                            // These two fields are needed to import the row
                                            //
                                            DataSourceName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "dataSource", DefaultCDef.dataSourceName);
                                            if (string.IsNullOrEmpty(DataSourceName)) {
                                                DataSourceName = "Default";
                                            }
                                            //
                                            // ----- Add CDef if not already there
                                            //
                                            if (!result.cdef.ContainsKey(ContentName.ToLower())) {
                                                result.cdef.Add(ContentName.ToLower(), new Models.Domain.CDefModel());
                                            }
                                            //
                                            // Get CDef attributes
                                            //
                                            Models.Domain.CDefModel tempVar = result.cdef[ContentName.ToLower()];
                                            string activeDefaultText = "1";
                                            if (!(DefaultCDef.active)) {
                                                activeDefaultText = "0";
                                            }
                                            ActiveText =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Active", activeDefaultText);
                                            if (string.IsNullOrEmpty(ActiveText)) {
                                                ActiveText = "1";
                                            }
                                            tempVar.active = GenericController.encodeBoolean(ActiveText);
                                            tempVar.activeOnly = true;
                                            //.adminColumns = ?
                                            tempVar.adminOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AdminOnly", DefaultCDef.adminOnly);
                                            tempVar.aliasID = "id";
                                            tempVar.aliasName = "name";
                                            tempVar.allowAdd =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowAdd", DefaultCDef.allowAdd);
                                            tempVar.allowCalendarEvents =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowCalendarEvents", DefaultCDef.allowCalendarEvents);
                                            tempVar.allowContentChildTool =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowContentChildTool", DefaultCDef.allowContentChildTool);
                                            tempVar.allowContentTracking =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowContentTracking", DefaultCDef.allowContentTracking);
                                            tempVar.allowDelete =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowDelete", DefaultCDef.allowDelete);
                                            tempVar.allowTopicRules =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AllowTopicRules", DefaultCDef.allowTopicRules);
                                            tempVar.guid =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", DefaultCDef.guid);
                                            tempVar.dataChanged = setAllDataChanged;
                                            tempVar.set_childIdList(core, new List<int>());
                                            tempVar.contentControlCriteria = "";
                                            tempVar.dataSourceName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentDataSourceName", DefaultCDef.dataSourceName);
                                            tempVar.tableName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.tableName);
                                            tempVar.dataSourceId = 0;
                                            tempVar.defaultSortMethod =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DefaultSortMethod", DefaultCDef.defaultSortMethod);
                                            if ((tempVar.defaultSortMethod == "") || (tempVar.defaultSortMethod.ToLower() == "name")) {
                                                tempVar.defaultSortMethod = "By Name";
                                            } else if (GenericController.vbLCase(tempVar.defaultSortMethod) == "sortorder") {
                                                tempVar.defaultSortMethod = "By Alpha Sort Order Field";
                                            } else if (GenericController.vbLCase(tempVar.defaultSortMethod) == "date") {
                                                tempVar.defaultSortMethod = "By Date";
                                            }
                                            tempVar.developerOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "DeveloperOnly", DefaultCDef.developerOnly);
                                            tempVar.dropDownFieldList =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DropDownFieldList", DefaultCDef.dropDownFieldList);
                                            tempVar.editorGroupName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "EditorGroupName", DefaultCDef.editorGroupName);
                                            tempVar.fields = new Dictionary<string, Models.Domain.CDefFieldModel>();
                                            tempVar.iconLink =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "IconLink", DefaultCDef.iconLink);
                                            tempVar.iconHeight =XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconHeight", DefaultCDef.iconHeight);
                                            tempVar.iconWidth =XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconWidth", DefaultCDef.iconWidth);
                                            tempVar.iconSprites =XmlController.GetXMLAttributeInteger(core, Found, CDef_NodeWithinLoop, "IconSprites", DefaultCDef.iconSprites);
                                            tempVar.ignoreContentControl =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "IgnoreContentControl", DefaultCDef.ignoreContentControl);
                                            tempVar.includesAFieldChange = false;
                                            tempVar.installedByCollectionGuid =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "installedByCollection", DefaultCDef.installedByCollectionGuid);
                                            tempVar.isBaseContent = IsccBaseFile ||XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "IsBaseContent", false);
                                            tempVar.isModifiedSinceInstalled =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "IsModified", DefaultCDef.isModifiedSinceInstalled);
                                            tempVar.name = ContentName;
                                            tempVar.parentName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Parent", DefaultCDef.parentName);
                                            tempVar.whereClause =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "WhereClause", DefaultCDef.whereClause);
                                            //
                                            // Get CDef field nodes
                                            //
                                            foreach (XmlNode CDefChildNode in CDef_NodeWithinLoop.ChildNodes) {
                                                //
                                                // ----- process CDef Field
                                                //
                                                if (textMatch(CDefChildNode.Name, "field")) {
                                                    FieldName =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Name", "");
                                                    if (FieldName.ToLower() == "middlename") {
                                                        //FieldName = FieldName;
                                                    }
                                                    //
                                                    // try to find field in the defaultcdef
                                                    //
                                                    if (DefaultCDef.fields.ContainsKey(FieldName)) {
                                                        DefaultCDefField = DefaultCDef.fields[FieldName];
                                                    } else {
                                                        DefaultCDefField = new Models.Domain.CDefFieldModel();
                                                    }
                                                    //
                                                    if (!(result.cdef[ContentName.ToLower()].fields.ContainsKey(FieldName.ToLower()))) {
                                                        result.cdef[ContentName.ToLower()].fields.Add(FieldName.ToLower(), new Models.Domain.CDefFieldModel());
                                                    }
                                                    var cdefField = result.cdef[ContentName.ToLower()].fields[FieldName.ToLower()];
                                                    cdefField.nameLc = FieldName.ToLower();
                                                    ActiveText = "0";
                                                    if (DefaultCDefField.active) {
                                                        ActiveText = "1";
                                                    }
                                                    ActiveText =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Active", ActiveText);
                                                    if (string.IsNullOrEmpty(ActiveText)) {
                                                        ActiveText = "1";
                                                    }
                                                    cdefField.active = GenericController.encodeBoolean(ActiveText);
                                                    //
                                                    // Convert Field Descriptor (text) to field type (integer)
                                                    //
                                                    string defaultFieldTypeName = core.db.getFieldTypeNameFromFieldTypeId(DefaultCDefField.fieldTypeId);
                                                    string fieldTypeName =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "FieldType", defaultFieldTypeName);
                                                    cdefField.fieldTypeId = core.db.getFieldTypeIdFromFieldTypeName(fieldTypeName);
                                                    //FieldTypeDescriptor =xmlController.GetXMLAttribute(core,Found, CDefChildNode, "FieldType", DefaultCDefField.fieldType)
                                                    //If genericController.vbIsNumeric(FieldTypeDescriptor) Then
                                                    //    .fieldType = genericController.EncodeInteger(FieldTypeDescriptor)
                                                    //Else
                                                    //    .fieldType = core.app.csv_GetFieldTypeByDescriptor(FieldTypeDescriptor)
                                                    //End If
                                                    //If .fieldType = 0 Then
                                                    //    .fieldType = FieldTypeText
                                                    //End If
                                                    cdefField.editSortPriority =XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "EditSortPriority", DefaultCDefField.editSortPriority);
                                                    cdefField.authorable =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Authorable", DefaultCDefField.authorable);
                                                    cdefField.caption =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "Caption", DefaultCDefField.caption);
                                                    cdefField.defaultValue =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "DefaultValue", DefaultCDefField.defaultValue);
                                                    cdefField.notEditable =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "NotEditable", DefaultCDefField.notEditable);
                                                    cdefField.indexColumn =XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "IndexColumn", DefaultCDefField.indexColumn);
                                                    cdefField.indexWidth =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "IndexWidth", DefaultCDefField.indexWidth);
                                                    cdefField.indexSortOrder =XmlController.GetXMLAttributeInteger(core, Found, CDefChildNode, "IndexSortOrder", DefaultCDefField.indexSortOrder);
                                                    cdefField.redirectID =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectID", DefaultCDefField.redirectID);
                                                    cdefField.redirectPath =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectPath", DefaultCDefField.redirectPath);
                                                    cdefField.htmlContent =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "HTMLContent", DefaultCDefField.htmlContent);
                                                    cdefField.uniqueName =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "UniqueName", DefaultCDefField.uniqueName);
                                                    cdefField.password =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Password", DefaultCDefField.password);
                                                    cdefField.adminOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "AdminOnly", DefaultCDefField.adminOnly);
                                                    cdefField.developerOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "DeveloperOnly", DefaultCDefField.developerOnly);
                                                    cdefField.readOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "ReadOnly", DefaultCDefField.readOnly);
                                                    cdefField.required =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Required", DefaultCDefField.required);
                                                    cdefField.RSSTitleField =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "RSSTitle", DefaultCDefField.RSSTitleField);
                                                    cdefField.RSSDescriptionField =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "RSSDescriptionField", DefaultCDefField.RSSDescriptionField);
                                                    cdefField.memberSelectGroupName_set(core,XmlController.GetXMLAttribute(core, Found, CDefChildNode, "MemberSelectGroup", ""));
                                                    cdefField.editTabName =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "EditTab", DefaultCDefField.editTabName);
                                                    cdefField.Scramble =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "Scramble", DefaultCDefField.Scramble);
                                                    cdefField.lookupList =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "LookupList", DefaultCDefField.lookupList);
                                                    cdefField.ManyToManyRulePrimaryField =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRulePrimaryField", DefaultCDefField.ManyToManyRulePrimaryField);
                                                    cdefField.ManyToManyRuleSecondaryField =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRuleSecondaryField", DefaultCDefField.ManyToManyRuleSecondaryField);
                                                    cdefField.set_lookupContentName(core,XmlController.GetXMLAttribute(core, Found, CDefChildNode, "LookupContent", DefaultCDefField.get_lookupContentName(core)));
                                                    // isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
                                                    // case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
                                                    // case 2 - when the base file is loaded, all fields must include the attribute
                                                    //Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
                                                    cdefField.isBaseField =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "IsBaseField", false) || IsccBaseFile;
                                                    cdefField.set_redirectContentName(core,XmlController.GetXMLAttribute(core, Found, CDefChildNode, "RedirectContent", DefaultCDefField.get_redirectContentName(core)));
                                                    cdefField.set_manyToManyContentName(core,XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.get_manyToManyContentName(core)));
                                                    cdefField.set_manyToManyRuleContentName(core,XmlController.GetXMLAttribute(core, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.get_manyToManyRuleContentName(core)));
                                                    cdefField.isModifiedSinceInstalled =XmlController.GetXMLAttributeBoolean(core, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled);
                                                    cdefField.installedByCollectionGuid =XmlController.GetXMLAttribute(core, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid);
                                                    cdefField.dataChanged = setAllDataChanged;
                                                    //
                                                    // ----- handle child nodes (help node)
                                                    //
                                                    cdefField.helpCustom = "";
                                                    cdefField.helpDefault = "";
                                                    foreach (XmlNode FieldChildNode in CDefChildNode.ChildNodes) {
                                                        //
                                                        // ----- process CDef Field
                                                        //
                                                        if (textMatch(FieldChildNode.Name, "HelpDefault")) {
                                                            cdefField.helpDefault = FieldChildNode.InnerText;
                                                        }
                                                        if (textMatch(FieldChildNode.Name, "HelpCustom")) {
                                                            cdefField.helpCustom = FieldChildNode.InnerText;
                                                        }
                                                        cdefField.HelpChanged = setAllDataChanged;
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
                                    IndexName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "indexname", "");
                                    TableName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "tableName", "");
                                    DataSourceName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "DataSourceName", "");
                                    if (string.IsNullOrEmpty(DataSourceName)) {
                                        DataSourceName = "default";
                                    }
                                    bool removeDup = false;
                                    MiniCollectionModel.miniCollectionSQLIndexModel dupToRemove = new MiniCollectionModel.miniCollectionSQLIndexModel();
                                    foreach (MiniCollectionModel.miniCollectionSQLIndexModel index in result.sqlIndexes) {
                                        if (textMatch(index.IndexName, IndexName) & textMatch(index.TableName, TableName) & textMatch(index.DataSourceName, DataSourceName)) {
                                            dupToRemove = index;
                                            removeDup = true;
                                            break;
                                        }
                                    }
                                    if (removeDup) {
                                        result.sqlIndexes.Remove(dupToRemove);
                                    }
                                    MiniCollectionModel.miniCollectionSQLIndexModel newIndex = new MiniCollectionModel.miniCollectionSQLIndexModel {
                                        IndexName = IndexName,
                                        TableName = TableName,
                                        DataSourceName = DataSourceName,
                                        FieldNameList = XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "FieldNameList", "")
                                    };
                                    result.sqlIndexes.Add(newIndex);
                                    break;
                                case "adminmenu":
                                case "menuentry":
                                case "navigatorentry":
                                    //
                                    // Admin Menus / Navigator Entries
                                    MenuName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
                                    menuNameSpace =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NameSpace", "");
                                    MenuGuid =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                    IsNavigator = (NodeName == "navigatorentry");
                                    string MenuKey = null;
                                    if (!IsNavigator) {
                                        MenuKey = GenericController.vbLCase(MenuName);
                                    } else {
                                        MenuKey = MenuGuid;
                                    }
                                    if ( !result.menus.ContainsKey(MenuKey)) {
                                        ActiveText =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Active", "1");
                                        if (string.IsNullOrEmpty(ActiveText)) {
                                            ActiveText = "1";
                                        }
                                        result.menus.Add(MenuKey, new MiniCollectionModel.miniCollectionMenuModel() {
                                            dataChanged = setAllDataChanged,
                                            Name = MenuName,
                                            Guid = MenuGuid,
                                            Key = MenuKey,
                                            Active = GenericController.encodeBoolean(ActiveText),
                                            menuNameSpace =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NameSpace", ""),
                                            ParentName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ParentName", ""),
                                            ContentName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ContentName", ""),
                                            LinkPage =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "LinkPage", ""),
                                            SortOrder =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "SortOrder", ""),
                                            AdminOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "AdminOnly", false),
                                            DeveloperOnly =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "DeveloperOnly", false),
                                            NewWindow =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "NewWindow", false),
                                            AddonName =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "AddonName", ""),
                                            NavIconType =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NavIconType", ""),
                                            NavIconTitle =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "NavIconTitle", ""),
                                            IsNavigator = IsNavigator
                                        });
                                    }
                                    break;
                                case "aggregatefunction":
                                case "addon":
                                    //
                                    // Aggregate Objects (just make them -- there are not too many
                                    //
                                    Name =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
                                    MiniCollectionModel.miniCollectionAddOnModel addon;
                                    if (result.addOns.ContainsKey(Name.ToLower())) {
                                        addon = result.addOns[Name.ToLower()];
                                    } else {
                                        addon = new MiniCollectionModel.miniCollectionAddOnModel();
                                        result.addOns.Add(Name.ToLower(), addon);
                                    }
                                    addon.dataChanged = setAllDataChanged;
                                    addon.Link =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Link", "");
                                    addon.ObjectProgramID =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ObjectProgramID", "");
                                    addon.ArgumentList =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "ArgumentList", "");
                                    addon.SortOrder =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "SortOrder", "");
                                    addon.Copy =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "copy", "");
                                    break;
                                case "style":
                                    //
                                    // style sheet entries
                                    //
                                    Name =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Name", "");
                                    if (result.styleCnt > 0) {
                                        for (Ptr = 0; Ptr < result.styleCnt; Ptr++) {
                                            if (textMatch(result.styles[Ptr].Name, Name)) {
                                                break;
                                            }
                                        }
                                    }
                                    if (Ptr >= result.styleCnt) {
                                        Ptr = result.styleCnt;
                                        result.styleCnt = result.styleCnt + 1;
                                        Array.Resize(ref result.styles, Ptr);
                                        result.styles[Ptr].Name = Name;
                                    }
                                    var tempVar5 = result.styles[Ptr];
                                    tempVar5.dataChanged = setAllDataChanged;
                                    tempVar5.Overwrite =XmlController.GetXMLAttributeBoolean(core, Found, CDef_NodeWithinLoop, "Overwrite", false);
                                    tempVar5.Copy = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "stylesheet":
                                    //
                                    // style sheet in one entry
                                    //
                                    result.styleSheet = CDef_NodeWithinLoop.InnerText;
                                    break;
                                case "getcollection":
                                case "importcollection":
                                    if (true) {
                                        //If Not UpgradeDbOnly Then
                                        //
                                        // Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
                                        //
                                        Collectionname =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "name", "");
                                        CollectionGuid =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                        if (string.IsNullOrEmpty(CollectionGuid)) {
                                            CollectionGuid = CDef_NodeWithinLoop.InnerText;
                                        }
                                        if (string.IsNullOrEmpty(CollectionGuid)) {
                                            status = "The collection you selected [" + Collectionname + "] can not be downloaded because it does not include a valid GUID.";
                                            //core.AppendLog("builderClass.UpgradeCDef_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
                                        } else {
                                            result.collectionImports.Add(new MiniCollectionModel.ImportCollectionType() {
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
                                    if (result.pageTemplateCnt > 0) {
                                        for (Ptr = 0; Ptr < result.pageTemplateCnt; Ptr++) {
                                            if (textMatch(result.pageTemplates[Ptr].Name, Name)) {
                                                break;
                                            }
                                        }
                                    }
                                    if (Ptr >= result.pageTemplateCnt) {
                                        Ptr = result.pageTemplateCnt;
                                        result.pageTemplateCnt = result.pageTemplateCnt + 1;
                                        Array.Resize(ref result.pageTemplates, Ptr);
                                        result.pageTemplates[Ptr].Name = Name;
                                    }
                                    var tempVar6 = result.pageTemplates[Ptr];
                                    tempVar6.Copy =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "Copy", "");
                                    tempVar6.Guid =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "guid", "");
                                    tempVar6.Style =XmlController.GetXMLAttribute(core, Found, CDef_NodeWithinLoop, "style", "");
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
                        foreach ( var kvp in result.menus) {
                            MiniCollectionModel.miniCollectionMenuModel menu = kvp.Value;
                            if ( !string.IsNullOrEmpty( menu.ParentName )) {
                                menu.menuNameSpace = GetMenuNameSpace(core, result.menus, menu, "");
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
        /// </summary>
        private static void installCollection_BuildDbFromMiniCollection(CoreController core, MiniCollectionModel Collection, string BuildVersion, bool isNewBuild, bool repair, ref List<string> nonCriticalErrorList, string logPrefix, ref List<String> installedCollections) {
            try {
                logPrefix += ", installCollection_BuildDbFromMiniCollection [" + Collection.name + "]";
                //
                LogController.logInfo(core, logPrefix + ", Application: " + core.appConfig.name + ", UpgradeCDef_BuildDbFromCollection");
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 1: verify core sql tables");
                //----------------------------------------------------------------------------------------------------------------------
                //
                AppBuilderController.VerifyBasicTables(core, logPrefix);
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 2: create SQL tables in default datasource");
                string ContentName = null;
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (true) {
                    string UsedTables = "";
                    foreach (var keypairvalue in Collection.cdef) {
                        Models.Domain.CDefModel workingCdef = keypairvalue.Value;
                        ContentName = workingCdef.name;
                        if (workingCdef.dataChanged) {
                            LogController.logInfo(core, logPrefix + ", creating sql table [" + workingCdef.tableName + "], datasource [" + workingCdef.dataSourceName + "]");
                            if (GenericController.vbLCase(workingCdef.dataSourceName) == "default" || workingCdef.dataSourceName == "") {
                                string TableName = workingCdef.tableName;
                                if (GenericController.vbInstr(1, "," + UsedTables + ",", "," + TableName + ",", 1) != 0) {
                                    //TableName = TableName;
                                } else {
                                    UsedTables = UsedTables + "," + TableName;
                                    core.db.createSQLTable(workingCdef.dataSourceName, TableName);
                                }
                            }
                        }
                    }
                    core.doc.clearMetaData();
                    core.cache.invalidateAll();
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 3: Verify all CDef names in ccContent so GetContentID calls will succeed");
                //----------------------------------------------------------------------------------------------------------------------
                //
                List<string> installedContentList = new List<string>();
                DataTable rs = core.db.executeQuery("SELECT Name from ccContent where (active<>0)");
                if (DbController.isDataTableOk(rs)) {
                    installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
                }
                rs.Dispose();
                string SQL = null;
                //
                foreach (var keypairvalue in Collection.cdef) {
                    if (keypairvalue.Value.dataChanged) {
                        LogController.logInfo(core, logPrefix + ", adding cdef name [" + keypairvalue.Value.name + "]");
                        if (!installedContentList.Contains(keypairvalue.Value.name.ToLower())) {
                            SQL = "Insert into ccContent (name,ccguid,active,createkey)values(" + core.db.encodeSQLText(keypairvalue.Value.name) + "," + core.db.encodeSQLText(keypairvalue.Value.guid) + ",1,0);";
                            core.db.executeQuery(SQL);
                            installedContentList.Add(keypairvalue.Value.name.ToLower());
                        }
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 4: Verify content records required for Content Server");
                //----------------------------------------------------------------------------------------------------------------------
                //
                verifySortMethods(core);
                VerifyContentFieldTypes(core);
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 5: verify 'Content' content definition");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.cdef) {
                    if (keypairvalue.Value.name.ToLower() == "content") {
                        installCollection_BuildDbFromCollection_AddCDefToDb(core, keypairvalue.Value, BuildVersion);
                        break;
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 6: Verify all definitions and fields");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var keypairvalue in Collection.cdef) {
                    CDefModel cdef = keypairvalue.Value;
                    bool fieldChanged = false;
                    if (!cdef.dataChanged ) {
                        foreach ( var field in cdef.fields) {
                            fieldChanged = field.Value.dataChanged;
                            if (fieldChanged) break;
                        }
                    }
                    if ((fieldChanged | cdef.dataChanged) & (cdef.name.ToLower() != "content")) {
                        installCollection_BuildDbFromCollection_AddCDefToDb(core, cdef, BuildVersion);
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 7: Verify all field help");
                //----------------------------------------------------------------------------------------------------------------------
                //
                int FieldHelpCID = core.db.getRecordID("content", "Content Field Help");
                foreach (var keypairvalue in Collection.cdef) {
                    Models.Domain.CDefModel workingCdef = keypairvalue.Value;
                    //ContentName = workingCdef.name;
                    foreach (var fieldKeyValuePair in workingCdef.fields) {
                        Models.Domain.CDefFieldModel workingField = fieldKeyValuePair.Value;
                        //string FieldName = field.nameLc;
                        //var field2 = Collection.cdef[ContentName.ToLower()].fields[((string)null).ToLower()];
                        if (workingField.HelpChanged) {
                            int fieldId = 0;
                            SQL = "select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" + core.db.encodeSQLText(workingField.nameLc) + ")and(c.name=" + core.db.encodeSQLText(workingCdef.name) + ") order by f.id";
                            rs = core.db.executeQuery(SQL);
                            if (DbController.isDataTableOk(rs)) {
                                fieldId = GenericController.encodeInteger(core.db.getDataRowColumnName(rs.Rows[0], "id"));
                            }
                            rs.Dispose();
                            if (fieldId == 0) {
                                throw (new ApplicationException("Unexpected exception")); //core.handleLegacyError3(core.appConfig.name, "Can not update help field for content [" & ContentName & "], field [" & FieldName & "] because the field was not found in the Db.", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection", 0, "", "", False, True, "")
                            } else {
                                SQL = "select id from ccfieldhelp where fieldid=" + fieldId + " order by id";
                                rs = core.db.executeQuery(SQL);
                                //
                                int FieldHelpID = 0;
                                if (DbController.isDataTableOk(rs)) {
                                    FieldHelpID = GenericController.encodeInteger(rs.Rows[0]["id"]);
                                } else {
                                    FieldHelpID = core.db.insertTableRecordGetId("default", "ccfieldhelp", 0);
                                }
                                rs.Dispose();
                                if (FieldHelpID != 0) {
                                    string Copy = workingField.helpCustom;
                                    if (string.IsNullOrEmpty(Copy)) {
                                        Copy = workingField.helpDefault;
                                        if (!string.IsNullOrEmpty(Copy)) {
                                            //Copy = Copy;
                                        }
                                    }
                                    SQL = "update ccfieldhelp set active=1,contentcontrolid=" + FieldHelpCID + ",fieldid=" + fieldId + ",helpdefault=" + core.db.encodeSQLText(Copy) + " where id=" + FieldHelpID;
                                    core.db.executeQuery(SQL);
                                }
                            }
                        }
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 8: create SQL indexes");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (MiniCollectionModel.miniCollectionSQLIndexModel index in Collection.sqlIndexes) {
                    if (index.dataChanged) {
                        LogController.logInfo(core, "creating index [" + index.IndexName + "], fields [" + index.FieldNameList + "], on table [" + index.TableName + "]");
                        core.db.createSQLIndex(index.DataSourceName, index.TableName, index.IndexName, index.FieldNameList);
                    }
                }
                core.doc.clearMetaData();
                core.cache.invalidateAll();
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 9: Verify All Menu Names, then all Menus");
                //----------------------------------------------------------------------------------------------------------------------
                //
                foreach (var kvp in Collection.menus) {
                    var menu = kvp.Value;
                    if (menu.dataChanged) {
                        LogController.logInfo(core, logPrefix + ", creating navigator entry [" + menu.Name + "], namespace [" + menu.menuNameSpace + "], guid [" + menu.Guid + "]");
                        AppBuilderController.verifyNavigatorEntry(core, menu.Guid, menu.menuNameSpace, menu.Name, menu.ContentName, menu.LinkPage, menu.SortOrder, menu.AdminOnly, menu.DeveloperOnly, menu.NewWindow, menu.Active, menu.AddonName, menu.NavIconType, menu.NavIconTitle, 0);
                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, "CDef Load, Upgrade collections added during upgrade process");
                //----------------------------------------------------------------------------------------------------------------------
                //
                LogController.logInfo(core, logPrefix + ", Installing Add-on Collections gathered during upgrade");
                foreach( var import in Collection.collectionImports) {
                    string CollectionPath = "";
                    DateTime lastChangeDate = new DateTime();
                    string emptyString = "";
                    getCollectionConfig(core, import.Guid , ref CollectionPath, ref lastChangeDate, ref emptyString);
                    string errorMessage = "";
                    if (!string.IsNullOrEmpty(CollectionPath)) {
                        //
                        // This collection is installed locally, install from local collections
                        //
                        installCollectionFromLocalRepo(core, import.Guid, core.codeVersion(), ref errorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                    } else {
                        //
                        // This is a new collection, install to the server and force it on this site
                        //
                        bool addonInstallOk = installCollectionFromRemoteRepo(core, import.Guid, ref errorMessage, "", isNewBuild, repair, ref nonCriticalErrorList, logPrefix, ref installedCollections);
                        if (!addonInstallOk) {
                            throw (new ApplicationException("Failure to install addon collection from remote repository. Collection [" + import.Guid + "] was referenced in collection [" + Collection.name + "]")); //core.handleLegacyError3(core.appConfig.name, "Error upgrading Addon Collection [" & Guid & "], " & errorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
                        }
                    }
                }
                //
                //----------------------------------------------------------------------------------------------------------------------
                LogController.logInfo(core, logPrefix + ", CDef Load, stage 9: Verify Styles");
                //----------------------------------------------------------------------------------------------------------------------
                //
                if (Collection.styleCnt > 0) {
                    string SiteStyles = core.cdnFiles.readFileText("templates/styles.css");
                    string[] SiteStyleSplit = { };
                    int SiteStyleCnt = 0;
                    if (!string.IsNullOrEmpty(SiteStyles.Trim(' '))) {
                        //
                        // Split with an extra character at the end to guarantee there is an extra split at the end
                        //
                        SiteStyleSplit = (SiteStyles + " ").Split('}');
                        SiteStyleCnt = SiteStyleSplit.GetUpperBound(0) + 1;
                    }
                    //Dim AddonClass As addonInstallClass
                    string StyleSheetAdd = "";
                    for (var Ptr = 0; Ptr < Collection.styleCnt; Ptr++) {
                        bool Found = false;
                        var tempVar4 = Collection.styles[Ptr];
                        if (tempVar4.dataChanged) {
                            string NewStyleName = tempVar4.Name;
                            string NewStyleValue = tempVar4.Copy;
                            NewStyleValue = GenericController.vbReplace(NewStyleValue, "}", "");
                            NewStyleValue = GenericController.vbReplace(NewStyleValue, "{", "");
                            if (SiteStyleCnt > 0) {
                                int SiteStylePtr = 0;
                                for (SiteStylePtr = 0; SiteStylePtr < SiteStyleCnt; SiteStylePtr++) {
                                    string StyleLine = SiteStyleSplit[SiteStylePtr];
                                    int PosNameLineEnd = StyleLine.LastIndexOf("{") + 1;
                                    if (PosNameLineEnd > 0) {
                                        int PosNameLineStart = StyleLine.LastIndexOf("\r\n", PosNameLineEnd - 1) + 1;
                                        if (PosNameLineStart > 0) {
                                            //
                                            // Check this site style for a match with the NewStyleName
                                            //
                                            PosNameLineStart = PosNameLineStart + 2;
                                            string TestStyleName = (StyleLine.Substring(PosNameLineStart - 1, PosNameLineEnd - PosNameLineStart)).Trim(' ');
                                            if (GenericController.vbLCase(TestStyleName) == GenericController.vbLCase(NewStyleName)) {
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
                        SiteStyles = SiteStyles 
                            + "\r\n\r\n/*"
                            + "\r\nStyles added " + DateTime.Now + "\r\n*/"
                            + "\r\n" + StyleSheetAdd;
                    }
                    core.appRootFiles.saveFile("templates/styles.css", SiteStyles);
                    //
                    // -- Update stylesheet cache
                    core.siteProperties.setProperty("StylesheetSerialNumber", "-1");
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Update a table from a collection cdef node
        /// </summary>
        private static void installCollection_BuildDbFromCollection_AddCDefToDb(CoreController core, Models.Domain.CDefModel cdef, string BuildVersion) {
            try {
                //
                LogController.logInfo(core, "Update db cdef [" + cdef.name + "]");
                //
                int ContentID = 0;
                bool ContentIsBaseContent = false;
                int FieldHelpCID = Models.Domain.CDefModel.getContentId(core, "Content Field Help");
                var tmpList = new List<string> { };
                var datasource = DataSourceModel.createByName(core, cdef.dataSourceName, ref tmpList);
                {
                    //
                    // -- get contentid and protect content with IsBaseContent true
                    string SQL = core.db.getSQLSelect("default", "ccContent", "ID,IsBaseContent", "name=" + core.db.encodeSQLText(cdef.name), "ID", "", 1);
                    DataTable dt = core.db.executeQuery(SQL);
                    if (DbController.isDataTableOk(dt)) {
                        if (dt.Rows.Count > 0) {
                            ContentID = GenericController.encodeInteger(core.db.getDataRowColumnName(dt.Rows[0], "ID"));
                            ContentIsBaseContent = GenericController.encodeBoolean(core.db.getDataRowColumnName(dt.Rows[0], "IsBaseContent"));
                        }
                    }
                    dt.Dispose();
                }
                //
                // -- Update Content Record
                // 20180412 - emailQueue cdef was there, table created, but cctable record missing and cdef.tableid=0, fixed here
                cdef.dataChanged = true;
                if (cdef.dataChanged) {
                    //
                    // -- Content needs to be updated
                    if (ContentIsBaseContent && !cdef.isBaseContent) {
                        //
                        // -- Can not update a base content with a non-base content
                        LogController.handleError( core,new ApplicationException("Warning: An attempt was made to update Content Definition [" + cdef.name + "] from base to non-base. This should only happen when a base cdef is removed from the base collection. The update was ignored."));
                        cdef.isBaseContent = ContentIsBaseContent;
                    }
                    //
                    // -- update definition (use SingleRecord as an update flag)
                    Models.Domain.CDefModel.addContent(core, true, datasource, cdef.tableName, cdef.name, cdef.adminOnly, cdef.developerOnly, cdef.allowAdd, cdef.allowDelete, cdef.parentName, cdef.defaultSortMethod, cdef.dropDownFieldList, false, cdef.allowCalendarEvents, cdef.allowContentTracking, cdef.allowTopicRules, cdef.allowContentChildTool, false, cdef.iconLink, cdef.iconWidth, cdef.iconHeight, cdef.iconSprites, cdef.guid, cdef.isBaseContent, cdef.installedByCollectionGuid);
                    if (ContentID == 0) {
                        LogController.logInfo(core, "Could not determine contentid after createcontent3 for [" + cdef.name + "], upgrade for this cdef aborted.");
                    } else {
                        //
                        // -- Other fields not in the csv call
                        int EditorGroupID = 0;
                        if (cdef.editorGroupName != "") {
                            DataTable dt = core.db.executeQuery("select ID from ccGroups where name=" + core.db.encodeSQLText(cdef.editorGroupName));
                            if (DbController.isDataTableOk(dt)) {
                                if (dt.Rows.Count > 0) {
                                    EditorGroupID = GenericController.encodeInteger(core.db.getDataRowColumnName(dt.Rows[0], "ID"));
                                }
                            }
                            dt.Dispose();
                        }
                        string SQL = "update ccContent"
                            + " set EditorGroupID=" + EditorGroupID + ",isbasecontent=" + core.db.encodeSQLBoolean(cdef.isBaseContent) + " where id=" + ContentID + "";
                        core.db.executeQuery(SQL);
                    }
                }
                //
                // -- update Content Field Records and Content Field Help records
                if (ContentID == 0 && (cdef.fields.Count > 0)) {
                    //
                    // -- cannot add fields if there is no content record
                    throw (new ApplicationException("Unexpected exception"));
                } else {
                    foreach (var nameValuePair in cdef.fields) {
                        Models.Domain.CDefFieldModel field = nameValuePair.Value;
                        int fieldId = 0;
                        if (field.dataChanged) {
                            fieldId = Models.Domain.CDefModel.verifyCDefField_ReturnID(core, cdef.name, field);
                        }
                        //
                        // -- update content field help records
                        if (field.HelpChanged) {
                            int FieldHelpID = 0;
                            DataTable dt = core.db.executeQuery("select ID from ccFieldHelp where fieldid=" + fieldId);
                            if (DbController.isDataTableOk(dt)) {
                                if (dt.Rows.Count > 0) {
                                    FieldHelpID = GenericController.encodeInteger(core.db.getDataRowColumnName(dt.Rows[0], "ID"));
                                }
                            }
                            dt.Dispose();
                            //
                            if (FieldHelpID == 0) {
                                FieldHelpID = core.db.insertTableRecordGetId("default", "ccFieldHelp", 0);
                            }
                            if (FieldHelpID != 0) {
                                string SQL = "update ccfieldhelp"
                                    + " set fieldid=" + fieldId + ",active=1"
                                    + ",contentcontrolid=" + FieldHelpCID + ",helpdefault=" + core.db.encodeSQLText(field.helpDefault) + ",helpcustom=" + core.db.encodeSQLText(field.helpCustom) + " where id=" + FieldHelpID;
                                core.db.executeQuery(SQL);
                            }
                        }
                    }
                    //
                    // clear the cdef cache and list
                    core.doc.clearMetaData();
                    core.cache.invalidateAll();
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return the collectionList file stored in the root of the addon folder.
        /// </summary>
        /// <returns></returns>
        public static string getLocalCollectionStoreListXml(CoreController core) {
            string returnXml = "";
            try {
                string LastChangeDate = "";
                string FolderName = null;
                string collectionFilePathFilename = null;
                string CollectionGuid = null;
                string Collectionname = null;
                //
                collectionFilePathFilename = core.addon.getPrivateFilesAddonPath() + "Collections.xml";
                returnXml = core.privateFiles.readFileText(collectionFilePathFilename);
                if (string.IsNullOrWhiteSpace(returnXml)) {
                    List<FolderDetail> FolderList = core.privateFiles.getFolderList(core.addon.getPrivateFilesAddonPath());
                    if (FolderList.Count > 0) {
                        foreach (FolderDetail folder in FolderList) {
                            FolderName = folder.Name;
                            if (FolderName.Length > 34) {
                                if (GenericController.vbLCase(FolderName.Left(4)) != "temp") {
                                    CollectionGuid = FolderName.Substring(FolderName.Length - 32);
                                    Collectionname = FolderName.Left(FolderName.Length - CollectionGuid.Length - 1);
                                    CollectionGuid = CollectionGuid.Left(8) + "-" + CollectionGuid.Substring(8, 4) + "-" + CollectionGuid.Substring(12, 4) + "-" + CollectionGuid.Substring(16, 4) + "-" + CollectionGuid.Substring(20);
                                    CollectionGuid = "{" + CollectionGuid + "}";
                                    List<FolderDetail> SubFolderList = core.privateFiles.getFolderList(core.addon.getPrivateFilesAddonPath() + "\\" + FolderName);
                                    if (SubFolderList.Count>0) {
                                        FolderDetail lastSubFolder = SubFolderList.Last<FolderDetail>();
                                        FolderName = FolderName + "\\" + lastSubFolder.Name;
                                        LastChangeDate = lastSubFolder.Name.Substring(4, 2) + "/" + lastSubFolder.Name.Substring(6, 2) + "/" + lastSubFolder.Name.Left(4);
                                        if (!DateController.IsDate(LastChangeDate)) {
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
                    returnXml = "<CollectionList>" + returnXml + "\r\n</CollectionList>";
                    core.privateFiles.saveFile(collectionFilePathFilename, returnXml);
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnXml;
        }
        //
        //====================================================================================================
        /// <summary>
        /// get a list of collections available on the server
        /// </summary>
        public static bool getLocalCollectionStoreList(CoreController core, ref List<CollectionStoreClass> localCollectionStoreList, ref string return_ErrorMessage) {
            bool returnOk = true;
            try {
                //
                //-----------------------------------------------------------------------------------------------
                //   Load LocalCollections from the Collections.xml file
                //-----------------------------------------------------------------------------------------------
                //
                string localCollectionStoreListXml = getLocalCollectionStoreListXml(core);
                if (!string.IsNullOrEmpty(localCollectionStoreListXml)) {
                    XmlDocument LocalCollections = new XmlDocument();
                    try {
                        LocalCollections.LoadXml(localCollectionStoreListXml);
                    } catch (Exception) {
                        string Copy = "Error loading privateFiles\\addons\\Collections.xml";
                        LogController.logInfo(core, Copy);
                        return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                        returnOk = false;
                    }
                    if (returnOk) {
                        if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                            string Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
                            LogController.logInfo(core, Copy);
                            return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
                            returnOk = false;
                        } else {
                            //
                            // Get a list of the collection guids on this server
                            //
                            if (GenericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist") {
                                foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes) {
                                    switch (GenericController.vbLCase(LocalListNode.Name)) {
                                        case "collection":
                                            var collection = new CollectionStoreClass();
                                            localCollectionStoreList.Add(collection);
                                            foreach (XmlNode CollectionNode in LocalListNode.ChildNodes) {
                                                if (CollectionNode.Name.ToLower() == "name") {
                                                    collection.name = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLower() == "guid") {
                                                    collection.guid = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLower() == "path") {
                                                    collection.path = CollectionNode.InnerText;
                                                } else if (CollectionNode.Name.ToLower() == "lastchangedate") {
                                                    collection.lastChangeDate = GenericController.encodeDate( CollectionNode.InnerText );
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                LogController.handleError( core,ex);
                throw;
            }
            return returnOk;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return a list of collections on the 
        /// </summary>
        public static bool getRemoteCollectionStoreList(CoreController core, ref List<CollectionStoreClass> remoteCollectionStoreList) {
            bool result = false;
            try {
                var LibCollections = new XmlDocument();
                bool parseError = false;
                try {
                    LibCollections.Load("http://support.contensive.com/GetCollectionList?iv=" + core.codeVersion());
                } catch (Exception) {
                    string UserError = "There was an error reading the Collection Library. The site may be unavailable.";
                    LogController.logInfo(core, UserError);
                    ErrorController.addUserError(core, UserError);
                    parseError = true;
                }
                if (!parseError) {
                    if (GenericController.vbLCase(LibCollections.DocumentElement.Name) != GenericController.vbLCase(CollectionListRootNode)) {
                        string UserError = "There was an error reading the Collection Library file. The '" + CollectionListRootNode + "' element was not found.";
                        LogController.logInfo(core, UserError);
                        ErrorController.addUserError(core, UserError);
                    } else {
                        foreach (XmlNode CDef_Node in LibCollections.DocumentElement.ChildNodes) {
                            var collection = new CollectionStoreClass();
                            remoteCollectionStoreList.Add(collection);
                            switch (GenericController.vbLCase(CDef_Node.Name)) {
                                case "collection":
                                    //
                                    // Read the collection
                                    //
                                    foreach (XmlNode CollectionNode in CDef_Node.ChildNodes) {
                                        switch (GenericController.vbLCase(CollectionNode.Name)) {
                                            case "name":
                                                collection.name = CollectionNode.InnerText;
                                                break;
                                            case "guid":
                                                collection.guid = CollectionNode.InnerText;
                                                break;
                                            case "version":
                                                collection.version = CollectionNode.InnerText;
                                                break;
                                            case "description":
                                                collection.description = CollectionNode.InnerText;
                                                break;
                                            case "contensiveversion":
                                                collection.contensiveVersion = CollectionNode.InnerText;
                                                break;
                                            case "lastchangedate":
                                                collection.lastChangeDate = GenericController.encodeDate(CollectionNode.InnerText);
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //======================================================================================================
        /// <summary>
        /// data from local collection repository
        /// </summary>
        public class CollectionStoreClass {
            public string name;
            public string guid;
            public string path;
            public DateTime lastChangeDate;
            public string version;
            public string description;
            public string contensiveVersion;
        }
    }
}
