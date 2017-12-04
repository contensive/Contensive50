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
		//============================================================================
		//   process the include add-on node of the add-on nodes
		//       this is the second pass, so all add-ons should be added
		//       no errors for missing addones, except the include add-on case
		//============================================================================
		//
		private static string InstallCollectionFromLocalRepo_addonNode_Phase2(coreClass cpCore, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool ReturnUpgradeOK, ref string ReturnErrorMessage)
		{
			string result = "";
			try
			{
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
				if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on"))
				{
					AOName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name");
					if (string.IsNullOrEmpty(AOName))
					{
						AOName = "No Name";
					}
					AOGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", AOName);
					if (string.IsNullOrEmpty(AOGuid))
					{
						AOGuid = AOName;
					}
					AddOnType = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "");
					Criteria = "(" + AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(AOGuid) + ")";
					CS = cpCore.db.csOpen(cnAddons, Criteria,, false);
					if (cpCore.db.csOk(CS))
					{
						//
						// Update the Addon
						//
						logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + AOName + "], Guid [" + AOGuid + "]");
					}
					else
					{
						//
						// not found by GUID - search name against name to update legacy Add-ons
						//
						cpCore.db.csClose(CS);
						Criteria = "(name=" + cpCore.db.encodeSQLText(AOName) + ")and(" + AddonGuidFieldName + " is null)";
						CS = cpCore.db.csOpen(cnAddons, Criteria,, false);
					}
					if (!cpCore.db.csOk(CS))
					{
						//
						// Could not find add-on
						//
						logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + AOName + "], Guid [" + AOGuid + "]");
					}
					else
					{
						addonId = cpCore.db.csGetInteger(CS, "ID");
						ArgumentList = "";
						StyleSheet = "";
						NavDeveloperOnly = true;
						if (AddonNode.ChildNodes.Count > 0)
						{
							foreach (XmlNode PageInterface in AddonNode.ChildNodes)
							{
								switch (genericController.vbLCase(PageInterface.Name))
								{
									case "includeaddon":
									case "includeadd-on":
									case "include addon":
									case "include add-on":
										//
										// include add-ons - NOTE - import collections must be run before interfaces
										// when importing a collectin that will be used for an include
										//
										if (true)
										{
											IncludeAddonName = GetXMLAttribute(cpCore, IsFound, PageInterface, "name", "");
											IncludeAddonGuid = GetXMLAttribute(cpCore, IsFound, PageInterface, "guid", IncludeAddonName);
											IncludeAddonID = 0;
											Criteria = "";
											if (!string.IsNullOrEmpty(IncludeAddonGuid))
											{
												Criteria = AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(IncludeAddonGuid);
												if (string.IsNullOrEmpty(IncludeAddonName))
												{
													IncludeAddonName = "Add-on " + IncludeAddonGuid;
												}
											}
											else if (!string.IsNullOrEmpty(IncludeAddonName))
											{
												Criteria = "(name=" + cpCore.db.encodeSQLText(IncludeAddonName) + ")";
											}
											if (!string.IsNullOrEmpty(Criteria))
											{
												CS2 = cpCore.db.csOpen(cnAddons, Criteria);
												if (cpCore.db.csOk(CS2))
												{
													IncludeAddonID = cpCore.db.csGetInteger(CS2, "ID");
												}
												cpCore.db.csClose(CS2);
												AddRule = false;
												if (IncludeAddonID == 0)
												{
													UserError = "The include add-on [" + IncludeAddonName + "] could not be added because it was not found. If it is in the collection being installed, it must appear before any add-ons that include it.";
													logController.appendInstallLog(cpCore, "UpgradeAddFromLocalCollection_InstallAddonNode, UserError [" + UserError + "]");
													ReturnUpgradeOK = false;
													ReturnErrorMessage = ReturnErrorMessage + "<P>The collection was not installed because the add-on [" + AOName + "] requires an included add-on [" + IncludeAddonName + "] which could not be found. If it is in the collection being installed, it must appear before any add-ons that include it.</P>";
												}
												else
												{
													CS2 = cpCore.db.csOpenSql_rev("default", "select ID from ccAddonIncludeRules where Addonid=" + addonId + " and IncludedAddonID=" + IncludeAddonID);
													AddRule = !cpCore.db.csOk(CS2);
													cpCore.db.csClose(CS2);
												}
												if (AddRule)
												{
													CS2 = cpCore.db.csInsertRecord("Add-on Include Rules", 0);
													if (cpCore.db.csOk(CS2))
													{
														cpCore.db.csSet(CS2, "Addonid", addonId);
														cpCore.db.csSet(CS2, "IncludedAddonID", IncludeAddonID);
													}
													cpCore.db.csClose(CS2);
												}
											}
										}
										break;
								}
							}
						}
					}
					cpCore.db.csClose(CS);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return result;
			//
		}
		//'
		//'====================================================================================================
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
		//        Dim FolderList As IO.DirectoryInfo()
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
		//                                    'On Error Resume Next
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
		//                                        For Each dir As IO.DirectoryInfo In FolderList
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
		//                                                'For Each file As IO.FileInfo In FileList
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
		//                                                RegisterPath = Trim(RegisterPaths(Ptr))
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
		//                        '        RegisterPath = Trim(RegisterPaths(Ptr))
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
		//        cpCore.handleException(ex)
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
		//ErrorTrap:
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
		//                RecordName = Parents(Ptr)
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
		//ErrorTrap:
		//            Call HandleClassTrapError(cpCore.app.config.name, Err.Number, Err.Source, Err.Description, "unknownMethodNameLegacyCall", True)
		//        End Function
		//'
		//'=========================================================================================================
		//'   Use AppendClassLogFile
		//'=========================================================================================================
		//'
		//private Sub AppendAddonFile(ByVal ApplicationName As String, ByVal Method As String, ByVal Cause As String)
		//    logController.appendLogWithLegacyRow( cpcore,ApplicationName, Cause, "dll", "AddonInstallClass", Method, 0, "", "", False, True, "", "AddonInstall", "")

		//End Sub
		//'
		//'===========================================================================
		//'   Append Log File
		//'===========================================================================
		//'
		//Private Shared Sub logcontroller.appendInstallLog(cpCore As coreClass, ByVal ignore As String, ByVal Method As String, ByVal LogMessage As String)
		//    Try
		//        Console.WriteLine("install, " & LogMessage)
		//        logController.appendLog(cpCore, LogMessage, "install")
		//    Catch ex As Exception
		//        cpCore.handleException(ex)
		//    End Try
		//End Sub
		//
		//=========================================================================================
		//   Import CDef on top of current configuration and the base configuration
		//
		//=========================================================================================
		//
		public static void installBaseCollection(coreClass cpCore, bool isNewBuild, ref List<string> nonCriticalErrorList)
		{
			try
			{
				string ignoreString = "";
				string returnErrorMessage = "";
				bool ignoreBoolean = false;
				bool isBaseCollection = true;
				//
				// -- new build
				// 20171029 -- upgrading should restore base collection fields as a fix to deleted required fields
				string baseCollectionXml = cpCore.programFiles.readFile("aoBase5.xml");
				if (string.IsNullOrEmpty(baseCollectionXml))
				{
					//
					// -- base collection notfound
					throw new ApplicationException("Cannot load aoBase5.xml [" + cpCore.programFiles.rootLocalPath + "aoBase5.xml]");
				}
				else
				{
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
					string tmpFolderPath = "tmp" + genericController.GetRandomInteger().ToString() + "\\";
					cpCore.privateFiles.createPath(tmpFolderPath);
					cpCore.programFiles.copyFile("aoBase5.xml", tmpFolderPath + "aoBase5.xml", cpCore.privateFiles);
					List<string> ignoreList = new List<string>();
					if (!InstallCollectionsFromPrivateFolder(cpCore, tmpFolderPath, returnErrorMessage, ignoreList, isNewBuild, nonCriticalErrorList))
					{
						throw new ApplicationException(returnErrorMessage);
					}
					cpCore.privateFiles.DeleteFileFolder(tmpFolderPath);
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
		public static void installCollectionFromLocalRepo_BuildDbFromXmlData(coreClass cpCore, string XMLText, bool isNewBuild, bool isBaseCollection, ref List<string> nonCriticalErrorList)
		{
			try
			{
				//
				logController.appendInstallLog(cpCore, "Application: " + cpCore.serverConfig.appConfig.name);
				//
				// ----- Import any CDef files, allowing for changes
				miniCollectionModel miniCollectionToAdd = new miniCollectionModel();
				miniCollectionModel miniCollectionWorking = installCollection_GetApplicationMiniCollection(cpCore, isNewBuild);
				miniCollectionToAdd = installCollection_LoadXmlToMiniCollection(cpCore, XMLText, isBaseCollection, false, isNewBuild, miniCollectionWorking);
				installCollection_AddMiniCollectionSrcToDst(cpCore, miniCollectionWorking, miniCollectionToAdd, true);
				installCollection_BuildDbFromMiniCollection(cpCore, miniCollectionWorking, cpCore.siteProperties.dataBuildVersion, isNewBuild, ref nonCriticalErrorList);
			}
			catch (Exception ex)
			{
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
		//ErrorTrap:
		//            Dim ex As New Exception("todo") : Call HandleClassError(ex, cpcore.app.config.name, "methodNameFPO") ' Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection", True, True)
		//            'dim ex as new exception("todo"): Call HandleClassError(ex,cpcore.app.appEnvironment.name,Err.Number, Err.Source, Err.Description, "UpgradeCDef_LoadFileToCollection hint=[" & Hint & "]", True, True)
		//            Resume Next
		//        End Sub
		//
		//=========================================================================================
		//   create a collection class from a collection xml file
		//       - cdef are added to the cdefs in the application collection
		//=========================================================================================
		//
		private static miniCollectionModel installCollection_LoadXmlToMiniCollection(coreClass cpCore, string srcCollecionXml, bool IsccBaseFile, bool setAllDataChanged, bool IsNewBuild, miniCollectionModel defaultCollection)
		{
			miniCollectionModel result = null;
			try
			{
				Models.Complex.cdefModel DefaultCDef = null;
				Models.Complex.CDefFieldModel DefaultCDefField = null;
				string contentNameLc = null;
				xmlController XMLTools = new xmlController(cpCore);
				//Dim AddonClass As New addonInstallClass(cpCore)
				string status = null;
				string CollectionGuid = null;
				string Collectionname = null;
				string ContentTableName = null;
				bool IsNavigator = false;
				string ActiveText = null;
				string Name = string.Empty;
				string MenuName = null;
				string IndexName = null;
				string TableName = null;
				int Ptr = 0;
				string FieldName = null;
				string ContentName = null;
				bool Found = false;
				string menuNameSpace = null;
				string MenuGuid = null;
				string MenuKey = null;
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
				if (string.IsNullOrEmpty(srcCollecionXml))
				{
					//
					// -- empty collection is an error
					throw (new ApplicationException("UpgradeCDef_LoadDataToCollection, srcCollectionXml is blank or null"));
				}
				else
				{
					try
					{
						srcXmlDom.LoadXml(srcCollecionXml);
					}
					catch (Exception ex)
					{
						//
						// -- xml load error
						logController.appendLog(cpCore, "UpgradeCDef_LoadDataToCollection Error reading xml archive, ex=[" + ex.ToString() + "]");
						throw new Exception("Error in UpgradeCDef_LoadDataToCollection, during doc.loadXml()", ex);
					}
					if ((srcXmlDom.DocumentElement.Name.ToLower() != CollectionFileRootNode) & (srcXmlDom.DocumentElement.Name.ToLower() != "contensivecdef"))
					{
						//
						// -- root node must be collection (or legacy contensivecdef)
						cpCore.handleException(new ApplicationException("the archive file has a syntax error. Application name must be the first node."));
					}
					else
					{
						result.isBaseCollection = IsccBaseFile;
						//
						// Get Collection Name for logs
						//
						//hint = "get collection name"
						Collectionname = GetXMLAttribute(cpCore, Found, srcXmlDom.DocumentElement, "name", "");
						if (string.IsNullOrEmpty(Collectionname))
						{
							logController.appendInstallLog(cpCore, "UpgradeCDef_LoadDataToCollection, Application: " + cpCore.serverConfig.appConfig.name + ", Collection has no name");
						}
						else
						{
							//Call AppendClassLogFile(cpcore.app.config.name,"UpgradeCDef_LoadDataToCollection", "UpgradeCDef_LoadDataToCollection, Application: " & cpcore.app.appEnvironment.name & ", Collection: " & Collectionname)
						}
						result.name = Collectionname;
						//'
						//' Load possible DefaultSortMethods
						//'
						//'hint = "preload sort methods"
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
						foreach (XmlNode CDef_NodeWithinLoop in srcXmlDom.DocumentElement.ChildNodes)
						{
								CDef_Node = CDef_NodeWithinLoop;
							//isCdefTarget = False
							NodeName = genericController.vbLCase(CDef_NodeWithinLoop.Name);
							//hint = "read node " & NodeName
							switch (NodeName)
							{
								case "cdef":
									//
									// Content Definitions
									//
									ContentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "name", "");
									contentNameLc = genericController.vbLCase(ContentName);
									if (string.IsNullOrEmpty(ContentName))
									{
										throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "collection file contains a CDEF node with no name attribute. This is not allowed.", "dll", "builderClass", "UpgradeCDef_LoadDataToCollection", 0, "", "", False, True, "")
									}
									else
									{
										//
										// setup a cdef from the application collection to use as a default for missing attributes (for inherited cdef)
										//
										if (defaultCollection.CDef.ContainsKey(contentNameLc))
										{
											DefaultCDef = defaultCollection.CDef(contentNameLc);
										}
										else
										{
											DefaultCDef = new Models.Complex.cdefModel();
										}
										//
										ContentTableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.ContentTableName);
										if (!string.IsNullOrEmpty(ContentTableName))
										{
											//
											// These two fields are needed to import the row
											//
											DataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "dataSource", DefaultCDef.ContentDataSourceName);
											if (string.IsNullOrEmpty(DataSourceName))
											{
												DataSourceName = "Default";
											}
											//
											// ----- Add CDef if not already there
											//
											if (!result.CDef.ContainsKey(ContentName.ToLower()))
											{
												result.CDef.Add(ContentName.ToLower(), new Models.Complex.cdefModel());
											}
											//
											// Get CDef attributes
											//
											var tempVar = result.CDef(ContentName.ToLower());
											string activeDefaultText = "1";
											if (!(DefaultCDef.Active))
											{
												activeDefaultText = "0";
											}
											ActiveText = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Active", activeDefaultText);
											if (string.IsNullOrEmpty(ActiveText))
											{
												ActiveText = "1";
											}
											tempVar.Active = genericController.EncodeBoolean(ActiveText);
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
											tempVar.childIdList(cpCore) = new List<int>();
											tempVar.ContentControlCriteria = "";
											tempVar.ContentDataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentDataSourceName", DefaultCDef.ContentDataSourceName);
											tempVar.ContentTableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentTableName", DefaultCDef.ContentTableName);
											tempVar.dataSourceId = 0;
											tempVar.DefaultSortMethod = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "DefaultSortMethod", DefaultCDef.DefaultSortMethod);
											if ((tempVar.DefaultSortMethod == "") || (tempVar.DefaultSortMethod.ToLower() == "name"))
											{
												tempVar.DefaultSortMethod = "By Name";
											}
											else if (genericController.vbLCase(tempVar.DefaultSortMethod) == "sortorder")
											{
												tempVar.DefaultSortMethod = "By Alpha Sort Order Field";
											}
											else if (genericController.vbLCase(tempVar.DefaultSortMethod) == "date")
											{
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
											foreach (XmlNode CDefChildNode in CDef_NodeWithinLoop.ChildNodes)
											{
												//
												// ----- process CDef Field
												//
												if (TextMatch(cpCore, CDefChildNode.Name, "field"))
												{
													FieldName = GetXMLAttribute(cpCore, Found, CDefChildNode, "Name", "");
													if (FieldName.ToLower() == "middlename")
													{
														FieldName = FieldName;
													}
													//
													// try to find field in the defaultcdef
													//
													if (DefaultCDef.fields.ContainsKey(FieldName))
													{
														DefaultCDefField = DefaultCDef.fields(FieldName);
													}
													else
													{
														DefaultCDefField = new Models.Complex.CDefFieldModel();
													}
													//
													if (!(result.CDef(ContentName.ToLower()).fields.ContainsKey(FieldName.ToLower())))
													{
														result.CDef(ContentName.ToLower()).fields.Add(FieldName.ToLower(), new Models.Complex.CDefFieldModel());
													}
													var tempVar2 = result.CDef(ContentName.ToLower()).fields(FieldName.ToLower());
													tempVar2.nameLc = FieldName.ToLower();
													ActiveText = "0";
													if (DefaultCDefField.active)
													{
														ActiveText = "1";
													}
													ActiveText = GetXMLAttribute(cpCore, Found, CDefChildNode, "Active", ActiveText);
													if (string.IsNullOrEmpty(ActiveText))
													{
														ActiveText = "1";
													}
													tempVar2.active = genericController.EncodeBoolean(ActiveText);
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
													tempVar2.lookupContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "LookupContent", DefaultCDefField.lookupContentName(cpCore));
													// isbase should be set if the base file is loading, regardless of the state of any isBaseField attribute -- which will be removed later
													// case 1 - when the application collection is loaded from the exported xml file, isbasefield must follow the export file although the data is not the base collection
													// case 2 - when the base file is loaded, all fields must include the attribute
													//Return_Collection.CDefExt(CDefPtr).Fields(FieldPtr).IsBaseField = IsccBaseFile
													tempVar2.isBaseField = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsBaseField", false) || IsccBaseFile;
													tempVar2.RedirectContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "RedirectContent", DefaultCDefField.RedirectContentName(cpCore));
													tempVar2.ManyToManyContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyContent", DefaultCDefField.ManyToManyContentName(cpCore));
													tempVar2.ManyToManyRuleContentName(cpCore) = GetXMLAttribute(cpCore, Found, CDefChildNode, "ManyToManyRuleContent", DefaultCDefField.ManyToManyRuleContentName(cpCore));
													tempVar2.isModifiedSinceInstalled = GetXMLAttributeBoolean(cpCore, Found, CDefChildNode, "IsModified", DefaultCDefField.isModifiedSinceInstalled);
													tempVar2.installedByCollectionGuid = GetXMLAttribute(cpCore, Found, CDefChildNode, "installedByCollectionId", DefaultCDefField.installedByCollectionGuid);
													tempVar2.dataChanged = setAllDataChanged;
													//
													// ----- handle child nodes (help node)
													//
													tempVar2.HelpCustom = "";
													tempVar2.HelpDefault = "";
													foreach (XmlNode FieldChildNode in CDefChildNode.ChildNodes)
													{
														//
														// ----- process CDef Field
														//
														if (TextMatch(cpCore, FieldChildNode.Name, "HelpDefault"))
														{
															tempVar2.HelpDefault = FieldChildNode.InnerText;
														}
														if (TextMatch(cpCore, FieldChildNode.Name, "HelpCustom"))
														{
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
									TableName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "TableName", "");
									DataSourceName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "DataSourceName", "");
									if (string.IsNullOrEmpty(DataSourceName))
									{
										DataSourceName = "default";
									}
									if (result.SQLIndexCnt > 0)
									{
										for (Ptr = 0; Ptr < result.SQLIndexCnt; Ptr++)
										{
											if (TextMatch(cpCore, result.SQLIndexes(Ptr).IndexName, IndexName) & TextMatch(cpCore, result.SQLIndexes(Ptr).TableName, TableName) & TextMatch(cpCore, result.SQLIndexes(Ptr).DataSourceName, DataSourceName))
											{
												break;
											}
										}
									}
									if (Ptr >= result.SQLIndexCnt)
									{
										Ptr = result.SQLIndexCnt;
										result.SQLIndexCnt = result.SQLIndexCnt + 1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim Preserve result.SQLIndexes(Ptr);
										result.SQLIndexes(Ptr).IndexName = IndexName;
										result.SQLIndexes(Ptr).TableName = TableName;
										result.SQLIndexes(Ptr).DataSourceName = DataSourceName;
									}
									result.SQLIndexes(Ptr).FieldNameList = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "FieldNameList", "");
									break;
								case "adminmenu":
								case "menuentry":
								case "navigatorentry":

									//
									// Admin Menus / Navigator Entries
									//
									MenuName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
									menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NameSpace", "");
									MenuGuid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", "");
									IsNavigator = (NodeName == "navigatorentry");
									//
									// Set MenuKey to what we will expect to find in the .guid
									//
									// make a local out of getdatabuildversion
									//
									if (!IsNavigator)
									{
										MenuKey = genericController.vbLCase(MenuName);
									}
									else if (false)
									{
										MenuKey = genericController.vbLCase("nav." + menuNameSpace + "." + MenuName);
									}
									else
									{
										MenuKey = MenuGuid;
									}
									//
									// Go through all current menus and check for duplicates
									//
									if (result.MenuCnt > 0)
									{
										for (Ptr = 0; Ptr < result.MenuCnt; Ptr++)
										{
											// 1/16/2009 - JK - empty keys should not be allowed
											if (result.Menus(Ptr).Key != "")
											{
												if (TextMatch(cpCore, result.Menus(Ptr).Key, MenuKey))
												{
													break;
												}
											}
										}
									}
									if (Ptr >= result.MenuCnt)
									{
										//
										// Add new entry
										//
										Ptr = result.MenuCnt;
										result.MenuCnt = result.MenuCnt + 1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim Preserve result.Menus(Ptr);
										//.Menus(Ptr).Name = MenuName
									}
									var tempVar3 = result.Menus(Ptr);
									ActiveText = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Active", "1");
									if (string.IsNullOrEmpty(ActiveText))
									{
										ActiveText = "1";
									}
									//
									// Update Entry
									//
									tempVar3.dataChanged = setAllDataChanged;
									tempVar3.Name = MenuName;
									tempVar3.Guid = MenuGuid;
									tempVar3.Key = MenuKey;
									tempVar3.Active = genericController.EncodeBoolean(ActiveText);
									tempVar3.menuNameSpace = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NameSpace", "");
									tempVar3.ParentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ParentName", "");
									tempVar3.ContentName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ContentName", "");
									tempVar3.LinkPage = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "LinkPage", "");
									tempVar3.SortOrder = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "SortOrder", "");
									tempVar3.AdminOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "AdminOnly", false);
									tempVar3.DeveloperOnly = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "DeveloperOnly", false);
									tempVar3.NewWindow = GetXMLAttributeBoolean(cpCore, Found, CDef_NodeWithinLoop, "NewWindow", false);
									tempVar3.AddonName = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "AddonName", "");
									tempVar3.NavIconType = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NavIconType", "");
									tempVar3.NavIconTitle = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "NavIconTitle", "");
									tempVar3.IsNavigator = IsNavigator;
									break;
								case "aggregatefunction":
								case "addon":
									//
									// Aggregate Objects (just make them -- there are not too many
									//
									Name = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
									if (result.AddOnCnt > 0)
									{
										for (Ptr = 0; Ptr < result.AddOnCnt; Ptr++)
										{
											if (TextMatch(cpCore, result.AddOns(Ptr).Name, Name))
											{
												break;
											}
										}
									}
									if (Ptr >= result.AddOnCnt)
									{
										Ptr = result.AddOnCnt;
										result.AddOnCnt = result.AddOnCnt + 1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim Preserve result.AddOns(Ptr);
										result.AddOns(Ptr).Name = Name;
									}
									var tempVar4 = result.AddOns(Ptr);
									tempVar4.dataChanged = setAllDataChanged;
									tempVar4.Link = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Link", "");
									tempVar4.ObjectProgramID = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ObjectProgramID", "");
									tempVar4.ArgumentList = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "ArgumentList", "");
									tempVar4.SortOrder = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "SortOrder", "");
									tempVar4.Copy = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "copy", "");
									result.AddOns(Ptr).Copy = CDef_NodeWithinLoop.InnerText;
									break;
								case "style":
									//
									// style sheet entries
									//
									Name = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "Name", "");
									if (result.StyleCnt > 0)
									{
										for (Ptr = 0; Ptr < result.StyleCnt; Ptr++)
										{
											if (TextMatch(cpCore, result.Styles(Ptr).Name, Name))
											{
												break;
											}
										}
									}
									if (Ptr >= result.StyleCnt)
									{
										Ptr = result.StyleCnt;
										result.StyleCnt = result.StyleCnt + 1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim Preserve result.Styles(Ptr);
										result.Styles(Ptr).Name = Name;
									}
									var tempVar5 = result.Styles(Ptr);
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
									if (true)
									{
										//If Not UpgradeDbOnly Then
										//
										// Import collections are blocked from the BuildDatabase upgrade b/c the resulting Db must be portable
										//
										Collectionname = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "name", "");
										CollectionGuid = GetXMLAttribute(cpCore, Found, CDef_NodeWithinLoop, "guid", "");
										if (string.IsNullOrEmpty(CollectionGuid))
										{
											CollectionGuid = CDef_NodeWithinLoop.InnerText;
										}
										if (string.IsNullOrEmpty(CollectionGuid))
										{
											status = "The collection you selected [" + Collectionname + "] can not be downloaded because it does not include a valid GUID.";
											//cpCore.AppendLog("builderClass.UpgradeCDef_LoadDataToCollection, UserError [" & status & "], The error was [" & Doc.ParseError.reason & "]")
										}
										else
										{
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
											ReDim Preserve result.collectionImports(result.ImportCnt);
											result.collectionImports(result.ImportCnt).Guid = CollectionGuid;
											result.collectionImports(result.ImportCnt).Name = Collectionname;
											result.ImportCnt = result.ImportCnt + 1;
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
									if (result.PageTemplateCnt > 0)
									{
										for (Ptr = 0; Ptr < result.PageTemplateCnt; Ptr++)
										{
											if (TextMatch(cpCore, result.PageTemplates(Ptr).Name, Name))
											{
												break;
											}
										}
									}
									if (Ptr >= result.PageTemplateCnt)
									{
										Ptr = result.PageTemplateCnt;
										result.PageTemplateCnt = result.PageTemplateCnt + 1;
//INSTANT C# TODO TASK: The following 'ReDim' could not be resolved. A possible reason may be that the object of the ReDim was not declared as an array:
										ReDim Preserve result.PageTemplates(Ptr);
										result.PageTemplates(Ptr).Name = Name;
									}
									var tempVar6 = result.PageTemplates(Ptr);
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
						if (result.MenuCnt > 0)
						{
							for (Ptr = 0; Ptr < result.MenuCnt; Ptr++)
							{
								if (result.Menus(Ptr).ParentName != "")
								{
									result.Menus(Ptr).menuNameSpace = GetMenuNameSpace(cpCore, result, Ptr, result.Menus(Ptr).IsNavigator, "");
									//.Menus(Ptr).ParentName = ""
									Ptr = Ptr;
								}
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
			return result;
		}
		//
		//========================================================================
		/// <summary>
		/// Verify ccContent and ccFields records from the cdef nodes of a a collection file. This is the last step of loading teh cdef nodes of a collection file. ParentId field is set based on ParentName node.
		/// </summary>
		/// <param name="Collection"></param>
		/// <param name="return_IISResetRequired"></param>
		/// <param name="BuildVersion"></param>
		private static void installCollection_BuildDbFromMiniCollection(coreClass cpCore, miniCollectionModel Collection, string BuildVersion, bool isNewBuild, ref List<string> nonCriticalErrorList)
		{
			try
			{
				//
				int FieldHelpID = 0;
				int FieldHelpCID = 0;
				int fieldId = 0;
				string FieldName = null;
				//Dim AddonClass As addonInstallClass
				string StyleSheetAdd = string.Empty;
				string NewStyleValue = null;
				string SiteStyles = null;
				int PosNameLineEnd = 0;
				int PosNameLineStart = 0;
				int SiteStylePtr = 0;
				string StyleLine = null;
				string[] SiteStyleSplit = {};
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
				// Dim builder As New coreBuilderClass(cpCore)
				string InstallCollectionList = ""; //Collections to Install when upgrade is complete
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
				if (true)
				{
					string UsedTables = "";
					foreach (var keypairvalue in Collection.CDef)
					{
						Models.Complex.cdefModel workingCdef = keypairvalue.Value;
						ContentName = workingCdef.Name;
						if (workingCdef.dataChanged)
						{
							logController.appendInstallLog(cpCore, "creating sql table [" + workingCdef.ContentTableName + "], datasource [" + workingCdef.ContentDataSourceName + "]");
							if (genericController.vbLCase(workingCdef.ContentDataSourceName) == "default" || workingCdef.ContentDataSourceName == "")
							{
								TableName = workingCdef.ContentTableName;
								if (genericController.vbInstr(1, "," + UsedTables + ",", "," + TableName + ",", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
								{
									TableName = TableName;
								}
								else
								{
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
				if (isDataTableOk(rs))
				{
					installedContentList = new List<string>(convertDataTableColumntoItemList(rs));
				}
				rs.Dispose();
				//
				foreach (var keypairvalue in Collection.CDef)
				{
					if (keypairvalue.Value.dataChanged)
					{
						logController.appendInstallLog(cpCore, "adding cdef name [" + keypairvalue.Value.Name + "]");
						if (!installedContentList.Contains(keypairvalue.Value.Name.ToLower()))
						{
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
				foreach (var keypairvalue in Collection.CDef)
				{
					if (keypairvalue.Value.Name.ToLower() == "content")
					{
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
				foreach (var keypairvalue in Collection.CDef)
				{
					//
					// todo tmp fix, changes to field caption in base.xml do not set fieldChange
					if (true) // If .dataChanged Or .includesAFieldChange Then
					{
						if (keypairvalue.Value.Name.ToLower() != "content")
						{
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
				foreach (var keypairvalue in Collection.CDef)
				{
					Models.Complex.cdefModel workingCdef = keypairvalue.Value;
					ContentName = workingCdef.Name;
					foreach (var fieldKeyValuePair in workingCdef.fields)
					{
						Models.Complex.CDefFieldModel field = fieldKeyValuePair.Value;
						FieldName = field.nameLc;
						var tempVar = Collection.CDef(ContentName.ToLower()).fields(FieldName.ToLower());
						if (tempVar.HelpChanged)
						{
							fieldId = 0;
							SQL = "select f.id from ccfields f left join cccontent c on c.id=f.contentid where (f.name=" + cpCore.db.encodeSQLText(FieldName) + ")and(c.name=" + cpCore.db.encodeSQLText(ContentName) + ") order by f.id";
							rs = cpCore.db.executeQuery(SQL);
							if (isDataTableOk(rs))
							{
								fieldId = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "id"));
							}
							rs.Dispose();
							if (fieldId == 0)
							{
								throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not update help field for content [" & ContentName & "], field [" & FieldName & "] because the field was not found in the Db.", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection", 0, "", "", False, True, "")
							}
							else
							{
								SQL = "select id from ccfieldhelp where fieldid=" + fieldId + " order by id";
								rs = cpCore.db.executeQuery(SQL);
								if (isDataTableOk(rs))
								{
									FieldHelpID = genericController.EncodeInteger(rs.Rows(0).Item("id"));
								}
								else
								{
									FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccfieldhelp", 0);
								}
								rs.Dispose();
								if (FieldHelpID != 0)
								{
									Copy = tempVar.HelpCustom;
									if (string.IsNullOrEmpty(Copy))
									{
										Copy = tempVar.HelpDefault;
										if (!string.IsNullOrEmpty(Copy))
										{
											Copy = Copy;
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
				for (var Ptr = 0; Ptr < Collection.SQLIndexCnt; Ptr++)
				{
					var tempVar2 = Collection.SQLIndexes(Ptr);
					if (tempVar2.dataChanged)
					{
						logController.appendInstallLog(cpCore, "creating index [" + tempVar2.IndexName + "], fields [" + tempVar2.FieldNameList + "], on table [" + tempVar2.TableName + "]");
						//
						// stop the errors here, so a bad field does not block the upgrade
						//
						//On Error Resume Next
						cpCore.db.createSQLIndex(tempVar2.DataSourceName, tempVar2.TableName, tempVar2.IndexName, tempVar2.FieldNameList);
					}
				}
				cpCore.doc.clearMetaData();
				cpCore.cache.invalidateAll();
				//
				//----------------------------------------------------------------------------------------------------------------------
				logController.appendInstallLog(cpCore, "CDef Load, stage 8a: Verify All Menu Names, then all Menus");
				//----------------------------------------------------------------------------------------------------------------------
				//
				for (var Ptr = 0; Ptr < Collection.MenuCnt; Ptr++)
				{
					var tempVar3 = Collection.Menus(Ptr);
					if (Ptr == 140)
					{
						Ptr = Ptr;
					}
					if (genericController.vbLCase(tempVar3.Name) == "manage add-ons" && tempVar3.IsNavigator)
					{
						tempVar3.Name = tempVar3.Name;
					}
					if (tempVar3.dataChanged)
					{
						logController.appendInstallLog(cpCore, "creating navigator entry [" + tempVar3.Name + "], namespace [" + tempVar3.menuNameSpace + "], guid [" + tempVar3.Guid + "]");
						appBuilderController.verifyNavigatorEntry(cpCore, tempVar3.Guid, tempVar3.menuNameSpace, tempVar3.Name, tempVar3.ContentName, tempVar3.LinkPage, tempVar3.SortOrder, tempVar3.AdminOnly, tempVar3.DeveloperOnly, tempVar3.NewWindow, tempVar3.Active, tempVar3.AddonName, tempVar3.NavIconType, tempVar3.NavIconTitle, 0);
						//If .IsNavigator Then
						//Else
						//    ContentName = cnNavigatorEntries
						//    Call logcontroller.appendInstallLog(cpCore,  "creating menu entry [" & .Name & "], parentname [" & .ParentName & "]")
						//    Call Controllers.appBuilderController.admin_VerifyMenuEntry(cpCore, .ParentName, .Name, .ContentName, .LinkPage, .SortOrder, .AdminOnly, .DeveloperOnly, .NewWindow, .Active, ContentName, .AddonName)
						//End If
					}
				}
				// 20160710 - this is old code (aggregatefunctions, etc are not in cdef anymore. Use the CollectionX methods to install addons
				//'
				//'----------------------------------------------------------------------------------------------------------------------
				//Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "CDef Load, stage 8c: Verify Add-ons")
				//'----------------------------------------------------------------------------------------------------------------------
				//'
				//NodeCount = 0
				//If .AddOnCnt > 0 Then
				//    For Ptr = 0 To .AddOnCnt - 1
				//        With .AddOns(Ptr)
				//            If .dataChanged Then
				//                Call AppendClassLogFile(cpCore.app.config.name, "UpgradeCDef_BuildDbFromCollection", "creating add-on [" & .Name & "]")
				//                'Dim 
				//                'InstallCollectionFromLocalRepo_addonNode_Phase1(cpcore,"crap - this takes an xml node and I have a collection object...")
				//                If .Link <> "" Then
				//                    Call csv_VerifyAggregateScript(.Name, .Link, .ArgumentList, .SortOrder)
				//                ElseIf .ObjectProgramID <> "" Then
				//                    Call csv_VerifyAggregateObject(.Name, .ObjectProgramID, .ArgumentList, .SortOrder)
				//                Else
				//                    Call csv_VerifyAggregateReplacement2(.Name, .Copy, .ArgumentList, .SortOrder)
				//                End If
				//            End If
				//        End With
				//    Next
				//End If
				//
				//----------------------------------------------------------------------------------------------------------------------
				logController.appendInstallLog(cpCore, "CDef Load, stage 8d: Verify Import Collections");
				//----------------------------------------------------------------------------------------------------------------------
				//
				if (Collection.ImportCnt > 0)
				{
					//AddonClass = New addonInstallClass(cpCore)
					for (var Ptr = 0; Ptr < Collection.ImportCnt; Ptr++)
					{
						InstallCollectionList = InstallCollectionList + "," + Collection.collectionImports(Ptr).Guid;
					}
				}
				//
				//---------------------------------------------------------------------
				// ----- Upgrade collections added during upgrade process
				//---------------------------------------------------------------------
				//
				string errorMessage = "";
				string[] Guids = null;
				string Guid = null;
				string CollectionPath = "";
				DateTime lastChangeDate = new DateTime();
				bool ignoreRefactor = false;
				logController.appendInstallLog(cpCore, "Installing Add-on Collections gathered during upgrade");
				if (string.IsNullOrEmpty(InstallCollectionList))
				{
					logController.appendInstallLog(cpCore, "No Add-on collections added during upgrade");
				}
				else
				{
					errorMessage = "";
					Guids = InstallCollectionList.Split(',');
					for (var Ptr = 0; Ptr <= Guids.GetUpperBound(0); Ptr++)
					{
						errorMessage = "";
						Guid = Guids[Ptr];
						if (!string.IsNullOrEmpty(Guid))
						{
							GetCollectionConfig(cpCore, Guid, CollectionPath, lastChangeDate, "");
							if (!string.IsNullOrEmpty(CollectionPath))
							{
								//
								// This collection is installed locally, install from local collections
								//
								installCollectionFromLocalRepo(cpCore, Guid, cpCore.codeVersion, errorMessage, "", isNewBuild, nonCriticalErrorList);
							}
							else
							{
								//
								// This is a new collection, install to the server and force it on this site
								//
								bool addonInstallOk = installCollectionFromRemoteRepo(cpCore, Guid, errorMessage, "", isNewBuild, nonCriticalErrorList);
								if (!addonInstallOk)
								{
									throw (new ApplicationException("Failure to install addon collection from remote repository. Collection [" + Guid + "] was referenced in collection [" + Collection.name + "]")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Error upgrading Addon Collection [" & Guid & "], " & errorMessage, "dll", "builderClass", "Upgrade2", 0, "", "", False, True, "")
								}

							}
						}
					}
				}
				//
				//----------------------------------------------------------------------------------------------------------------------
				logController.appendInstallLog(cpCore, "CDef Load, stage 9: Verify Styles");
				//----------------------------------------------------------------------------------------------------------------------
				//
				NodeCount = 0;
				if (Collection.StyleCnt > 0)
				{
					SiteStyles = cpCore.cdnFiles.readFile("templates/styles.css");
					if (!string.IsNullOrEmpty(SiteStyles.Trim(' ')))
					{
						//
						// Split with an extra character at the end to guarantee there is an extra split at the end
						//
						SiteStyleSplit = (SiteStyles + " ").Split('}');
						SiteStyleCnt = SiteStyleSplit.GetUpperBound(0) + 1;
					}
					for (var Ptr = 0; Ptr < Collection.StyleCnt; Ptr++)
					{
						Found = false;
						var tempVar4 = Collection.Styles(Ptr);
						if (tempVar4.dataChanged)
						{
							NewStyleName = tempVar4.Name;
							NewStyleValue = tempVar4.Copy;
							NewStyleValue = genericController.vbReplace(NewStyleValue, "}", "");
							NewStyleValue = genericController.vbReplace(NewStyleValue, "{", "");
							if (SiteStyleCnt > 0)
							{
								for (SiteStylePtr = 0; SiteStylePtr < SiteStyleCnt; SiteStylePtr++)
								{
									StyleLine = SiteStyleSplit[SiteStylePtr];
									PosNameLineEnd = StyleLine.LastIndexOf("{") + 1;
									if (PosNameLineEnd > 0)
									{
										PosNameLineStart = StyleLine.LastIndexOf(Environment.NewLine, PosNameLineEnd - 1) + 1;
										if (PosNameLineStart > 0)
										{
											//
											// Check this site style for a match with the NewStyleName
											//
											PosNameLineStart = PosNameLineStart + 2;
											TestStyleName = (StyleLine.Substring(PosNameLineStart - 1, PosNameLineEnd - PosNameLineStart)).Trim(' ');
											if (genericController.vbLCase(TestStyleName) == genericController.vbLCase(NewStyleName))
											{
												Found = true;
												if (tempVar4.Overwrite)
												{
													//
													// Found - Update style
													//
													SiteStyleSplit[SiteStylePtr] = Environment.NewLine + tempVar4.Name + " {" + NewStyleValue;
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
							if (!Found)
							{
								StyleSheetAdd = StyleSheetAdd + Environment.NewLine + NewStyleName + " {" + NewStyleValue + "}";
							}
						}
					}
					SiteStyles = string.Join("}", SiteStyleSplit);
					if (!string.IsNullOrEmpty(StyleSheetAdd))
					{
						SiteStyles = SiteStyles + Environment.NewLine + Environment.NewLine + "/*"
						+ Environment.NewLine + "Styles added " + DateTime.Now + Environment.NewLine + "*/"
						+ Environment.NewLine + StyleSheetAdd;
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//========================================================================
		// ----- Load the archive file application
		//========================================================================
		//
		private static void installCollection_BuildDbFromCollection_AddCDefToDb(coreClass cpCore, Models.Complex.cdefModel cdef, string BuildVersion)
		{
			try
			{
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
				if (!(false))
				{
					//
					logController.appendInstallLog(cpCore, "Upgrading CDef [" + cdef.Name + "]");
					//
					ContentID = 0;
					ContentName = cdef.Name;
					ContentIsBaseContent = false;
					FieldHelpCID = Models.Complex.cdefModel.getContentId(cpCore, "Content Field Help");
					Contensive.Core.Models.Entity.dataSourceModel datasource = Models.Entity.dataSourceModel.createByName(cpCore, cdef.ContentDataSourceName, new List<string>());
					//
					// get contentid and protect content with IsBaseContent true
					//
					SQL = cpCore.db.GetSQLSelect("default", "ccContent", "ID,IsBaseContent", "name=" + cpCore.db.encodeSQLText(ContentName), "ID",, 1);
					rs = cpCore.db.executeQuery(SQL);
					if (isDataTableOk(rs))
					{
						if (rs.Rows.Count > 0)
						{
							//EditorGroupID = cpcore.app.getDataRowColumnName(RS.rows(0), "ID")
							ContentID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"));
							ContentIsBaseContent = genericController.EncodeBoolean(cpCore.db.getDataRowColumnName(rs.Rows(0), "IsBaseContent"));
						}
					}
					rs.Dispose();
					//
					// ----- Update Content Record
					//
					if (cdef.dataChanged)
					{
						//
						// Content needs to be updated
						//
						if (ContentIsBaseContent && !cdef.IsBaseContent)
						{
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
						if (ContentID == 0)
						{
							logController.appendInstallLog(cpCore, "Could not determine contentid after createcontent3 for [" + ContentName + "], upgrade for this cdef aborted.");
						}
						else
						{
							//
							// ----- Other fields not in the csv call
							//
							EditorGroupID = 0;
							if (cdef.EditorGroupName != "")
							{
								rs = cpCore.db.executeQuery("select ID from ccGroups where name=" + cpCore.db.encodeSQLText(cdef.EditorGroupName));
								if (isDataTableOk(rs))
								{
									if (rs.Rows.Count > 0)
									{
										EditorGroupID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"));
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
					if (ContentID == 0 && (cdef.fields.Count > 0))
					{
						//
						// CAn not add fields if there is no content record
						//
						throw (new ApplicationException("Unexpected exception")); //cpCore.handleLegacyError3(cpCore.serverConfig.appConfig.name, "Can not add field records to content [" & ContentName & "] because the content definition was not found", "dll", "builderClass", "UpgradeCDef_BuildDbFromCollection_AddCDefToDb", 0, "", "", False, True, "")
					}
					else
					{
						//
						//
						//
						FieldSize = 0;
						FieldCount = 0;
						foreach (var nameValuePair in cdef.fields)
						{
							Models.Complex.CDefFieldModel field = nameValuePair.Value;
							if (field.dataChanged)
							{
								fieldId = Models.Complex.cdefModel.verifyCDefField_ReturnID(cpCore, ContentName, field);
							}
							//
							// ----- update content field help records
							//
							if (field.HelpChanged)
							{
								rs = cpCore.db.executeQuery("select ID from ccFieldHelp where fieldid=" + fieldId);
								if (isDataTableOk(rs))
								{
									if (rs.Rows.Count > 0)
									{
										FieldHelpID = genericController.EncodeInteger(cpCore.db.getDataRowColumnName(rs.Rows(0), "ID"));
									}
								}
								rs.Dispose();
								//
								if (FieldHelpID == 0)
								{
									FieldHelpID = cpCore.db.insertTableRecordGetId("default", "ccFieldHelp", 0);
								}
								if (FieldHelpID != 0)
								{
									SQL = "update ccfieldhelp"
										+ " set fieldid=" + fieldId + ",active=1"
										+ ",contentcontrolid=" + FieldHelpCID + ",helpdefault=" + cpCore.db.encodeSQLText(field.HelpDefault) + ",helpcustom=" + cpCore.db.encodeSQLText(field.HelpCustom) + " where id=" + FieldHelpID;
									cpCore.db.executeQuery(SQL);
								}
							}
						}
						//'
						//' started doing something here -- research it.!!!!!
						//'
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}






	}
}
