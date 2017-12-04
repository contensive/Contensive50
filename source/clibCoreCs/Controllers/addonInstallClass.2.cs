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
		//=========================================================================================================================
		//   Upgrade Application from a local collection
		//
		//   Either Upgrade or Install the collection in the Application. - no checks
		//
		//   ImportFromCollectionsGuidList - If this collection is from an import, this is the guid of the import.
		//=========================================================================================================================
		//
		public static bool installCollectionFromLocalRepo(coreClass cpCore, string CollectionGuid, string ignore_BuildVersion, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, ref List<string> nonCriticalErrorList)
		{
			bool result = true;
			try
			{
				string CollectionVersionFolderName = "";
				DateTime CollectionLastChangeDate = default(DateTime);
				string tempVar = "";
				GetCollectionConfig(cpCore, CollectionGuid, ref CollectionVersionFolderName, ref CollectionLastChangeDate, ref tempVar);
				if (string.IsNullOrEmpty(CollectionVersionFolderName))
				{
					result = false;
					return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed from the local collections because the folder containing the Add-on's resources could not be found. It may not be installed locally.</P>";
				}
				else
				{
					//
					// Search Local Collection Folder for collection config file (xml file)
					//
					string CollectionVersionFolder = cpCore.addon.getPrivateFilesAddonPath() + CollectionVersionFolderName + "\\";
					IO.FileInfo[] srcFileInfoArray = cpCore.privateFiles.getFileList(CollectionVersionFolder);
					if (srcFileInfoArray.Count() == 0)
					{
						result = false;
						return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the folder containing the Add-on's resources was empty.</P>";
					}
					else
					{
						//
						// collect list of DLL files and add them to the exec files if they were missed
						List<string> assembliesInZip = new List<string>();
						foreach (IO.FileInfo file in srcFileInfoArray)
						{
							if (file.Extension.ToLower() == "dll")
							{
								if (!assembliesInZip.Contains(file.Name.ToLower()))
								{
									assembliesInZip.Add(file.Name.ToLower());
								}
							}
						}
						//
						// -- Process the other files
//INSTANT C# NOTE: There is no C# equivalent to VB's implicit 'once only' variable initialization within loops, so the following variable declaration has been placed prior to the loop:
						bool CollectionblockNavigatorNode_fileValueOK = false;
						foreach (IO.FileInfo file in srcFileInfoArray)
						{
							if (genericController.vbLCase(file.Name.Substring(file.Name.Length - 4)) == ".xml")
							{
								//
								// -- XML file -- open it to figure out if it is one we can use
								XmlDocument Doc = new XmlDocument();
								string CollectionFilename = file.Name;
								bool loadOK = true;
								try
								{
									Doc.Load(cpCore.privateFiles.rootLocalPath + CollectionVersionFolder + file.Name);
								}
								catch (Exception ex)
								{
									//
									// error - Need a way to reach the user that submitted the file
									//
									logController.appendInstallLog(cpCore, "There was an error reading the Meta data file [" + cpCore.privateFiles.rootLocalPath + CollectionVersionFolder + file.Name + "].");
									result = false;
									return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error</P>";
									loadOK = false;
								}
								if (loadOK)
								{
									if ((Doc.DocumentElement.Name.ToLower() == genericController.vbLCase(CollectionFileRootNode)) || (Doc.DocumentElement.Name.ToLower() == genericController.vbLCase(CollectionFileRootNodeOld)))
									{
										//
										//------------------------------------------------------------------------------------------------------
										// Collection File - import from sub so it can be re-entrant
										//------------------------------------------------------------------------------------------------------
										//
										bool IsFound = false;
										string Collectionname = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "name", "");
										if (string.IsNullOrEmpty(Collectionname))
										{
											//
											// ----- Error condition -- it must have a collection name
											//
											//Call AppendAddonLog("UpgradeAppFromLocalCollection, collection has no name")
											logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, collection has no name");
											result = false;
											return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the collection name in the xml collection file is blank</P>";
										}
										else
										{
											bool CollectionSystem_fileValueOK = false;
											bool CollectionUpdatable_fileValueOK = false;
//												Dim CollectionblockNavigatorNode_fileValueOK As Boolean
											bool CollectionSystem = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionSystem_fileValueOK, Doc.DocumentElement, "system", ""));
											int Parent_NavID = appBuilderController.verifyNavigatorEntry(cpCore, addonGuidManageAddon, "", "Manage Add-ons", "", "", "", false, false, false, true, "", "", "", 0);
											bool CollectionUpdatable = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionUpdatable_fileValueOK, Doc.DocumentElement, "updatable", ""));
											bool CollectionblockNavigatorNode = genericController.EncodeBoolean(GetXMLAttribute(cpCore, CollectionblockNavigatorNode_fileValueOK, Doc.DocumentElement, "blockNavigatorNode", ""));
											string FileGuid = GetXMLAttribute(cpCore, IsFound, Doc.DocumentElement, "guid", Collectionname);
											if (string.IsNullOrEmpty(FileGuid))
											{
												FileGuid = Collectionname;
											}
											if (CollectionGuid.ToLower() != genericController.vbLCase(FileGuid))
											{
												//
												//
												//
												result = false;
												logController.appendInstallLog(cpCore, "Local Collection file contains a different GUID for [" + Collectionname + "] then Collections.xml");
												return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the unique number identifying the collection, called the guid, does not match the collection requested.</P>";
											}
											else
											{
												if (string.IsNullOrEmpty(CollectionGuid))
												{
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
												foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
												{
													switch (CDefSection.Name.ToLower())
													{
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
															if (Pos != 0)
															{
																//
																// Source path is in filename
																//
																SrcPath = filename.Substring(0, Pos - 1);
																filename = filename.Substring(Pos);
																if (string.IsNullOrEmpty(resourcePath))
																{
																	//
																	// No Resource Path give, use the same folder structure from source
																	//
																	DstPath = SrcPath;
																}
																else
																{
																	//
																	// Copy file to resource path
																	//
																	DstPath = resourcePath;
																}
															}

															string DstFilePath = genericController.vbReplace(DstPath, "/", "\\");
															if (DstFilePath == "\\")
															{
																DstFilePath = "";
															}
															if (!string.IsNullOrEmpty(DstFilePath))
															{
																if (DstFilePath.Substring(0, 1) == "\\")
																{
																	DstFilePath = DstFilePath.Substring(1);
																}
																if (DstFilePath.Substring(DstFilePath.Length - 1) != "\\")
																{
																	DstFilePath = DstFilePath + "\\";
																}
															}

															switch (resourceType.ToLower())
															{
																case "www":
																	wwwFileList = wwwFileList + Environment.NewLine + DstFilePath + filename;
																	logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to www, src [" + CollectionVersionFolder + SrcPath + "], dst [" + cpCore.serverConfig.appConfig.appRootFilesPath + DstFilePath + "].");
																	//Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, copying file to www, src [" & CollectionVersionFolder & SrcPath & "], dst [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & "].")
																	cpCore.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, DstFilePath + filename, cpCore.appRootFiles);
																	if (genericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip")
																	{
																		logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping www file [" + cpCore.serverConfig.appConfig.appRootFilesPath + DstFilePath + filename + "].");
																		//Call logcontroller.appendInstallLog(cpCore, "install collection [" & Collectionname & "], GUID [" & CollectionGuid & "], pass 1, unzipping www file [" & cpCore.serverConfig.clusterPath & cpCore.serverconfig.appConfig.appRootFilesPath & DstFilePath & Filename & "].")
																		cpCore.appRootFiles.UnzipFile(DstFilePath + filename);
																	}
																	break;
																case "file":
																case "content":
																	ContentFileList = ContentFileList + Environment.NewLine + DstFilePath + filename;
																	logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, copying file to content, src [" + CollectionVersionFolder + SrcPath + "], dst [" + DstFilePath + "].");
																	cpCore.privateFiles.copyFile(CollectionVersionFolder + SrcPath + filename, DstFilePath + filename, cpCore.cdnFiles);
																	if (genericController.vbLCase(filename.Substring(filename.Length - 4)) == ".zip")
																	{
																		logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1, unzipping content file [" + DstFilePath + filename + "].");
																		cpCore.cdnFiles.UnzipFile(DstFilePath + filename);
																	}
																	break;
																default:
																	if (assembliesInZip.Contains(filename.ToLower()))
																	{
																		assembliesInZip.Remove(filename.ToLower());
																	}
																	ExecFileList = ExecFileList + Environment.NewLine + filename;
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
															if (string.IsNullOrEmpty(ChildCollectionGUID))
															{
																ChildCollectionGUID = CDefSection.InnerText;
															}
															if (ImportFromCollectionsGuidList + "," + CollectionGuid.IndexOf(ChildCollectionGUID, System.StringComparison.OrdinalIgnoreCase) + 1 != 0)
															{
																//
																// circular import detected, this collection is already imported
																//
																logController.appendInstallLog(cpCore, "Circular import detected. This collection attempts to import a collection that had previously been imported. A collection can not import itself. The collection is [" + Collectionname + "], GUID [" + CollectionGuid + "], pass 1. The collection to be imported is [" + ChildCollectionName + "], GUID [" + ChildCollectionGUID + "]");
															}
															else
															{
																logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1, import collection found, name [" + ChildCollectionName + "], guid [" + ChildCollectionGUID + "]");
																if (true)
																{
																	installCollectionFromRemoteRepo(cpCore, ChildCollectionGUID, return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, nonCriticalErrorList);
																}
																else
																{
																	if (string.IsNullOrEmpty(ChildCollectionGUID))
																	{
																		logController.appendInstallLog(cpCore, "The importcollection node [" + ChildCollectionName + "] can not be upgraded because it does not include a valid guid.");
																	}
																	else
																	{
																		//
																		// This import occurred while upgrading an application from the local collections (Db upgrade or AddonManager)
																		// Its OK to install it if it is missing, but you do not need to upgrade the local collections from the Library
																		//
																		// 5/18/2008 -----------------------------------
																		// See if it is in the local collections storage. If yes, just upgrade this app with it. If not,
																		// it must be downloaded and the entire server must be upgraded
																		//
																		string ChildCollectionVersionFolderName = "";
																		DateTime ChildCollectionLastChangeDate = default(DateTime);
																		string tempVar2 = "";
																		GetCollectionConfig(cpCore, ChildCollectionGUID, ref ChildCollectionVersionFolderName, ref ChildCollectionLastChangeDate, ref tempVar2);
																		if (!string.IsNullOrEmpty(ChildCollectionVersionFolderName))
																		{
																			//
																			// It is installed in the local collections, update just this site
																			//
																			result &= installCollectionFromLocalRepo(cpCore, ChildCollectionGUID, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, ImportFromCollectionsGuidList + "," + CollectionGuid, IsNewBuild, ref nonCriticalErrorList);
																		}
																	}
																}
															}
															break;
													}
												}
												//
												// -- any assemblies found in the zip that were not part of the resources section need to be added
												foreach (string filename in assembliesInZip)
												{
													ExecFileList = ExecFileList + Environment.NewLine + filename;
												}
												//
												//-------------------------------------------------------------------------------
												// create an Add-on Collection record
												//-------------------------------------------------------------------------------
												//
												bool OKToInstall = false;
												logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], pass 1 done, create collection record.");
												Models.Entity.AddonCollectionModel collection = Models.Entity.AddonCollectionModel.create(cpCore, CollectionGuid);
												if (collection != null)
												{
													//
													// Upgrade addon
													//
													if (CollectionLastChangeDate == DateTime.MinValue)
													{
														logController.appendInstallLog(cpCore, "collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has the collection, but the new version has no lastchangedate, so it will upgrade to this unknown (manual) version.");
														OKToInstall = true;
													}
													else if (collection.LastChangeDate < CollectionLastChangeDate)
													{
														logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an older version of collection. It will be upgraded.");
														OKToInstall = true;
													}
													else
													{
														logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App has an up-to-date version of collection. It will not be upgraded, but all imports in the new version will be checked.");
														OKToInstall = false;
													}
												}
												else
												{
													//
													// Install new on this application
													//
													collection = Models.Entity.AddonCollectionModel.add(cpCore);
													logController.appendInstallLog(cpCore, "install collection [" + Collectionname + "], GUID [" + CollectionGuid + "], App does not have this collection so it will be installed.");
													OKToInstall = true;
												}
												string DataRecordList = "";
												if (!OKToInstall)
												{
													//
													// Do not install, but still check all imported collections to see if they need to be installed
													// imported collections moved in front this check
													//
												}
												else
												{
													//
													// ----- gather help nodes
													//
													string CollectionHelpLink = "";
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														if (CDefSection.Name.ToLower() == "helplink")
														{
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
													if (CollectionSystem_fileValueOK)
													{
														collection.System = CollectionSystem;
													}
													if (CollectionUpdatable_fileValueOK)
													{
														collection.Updatable = CollectionUpdatable;
													}
													if (CollectionblockNavigatorNode_fileValueOK)
													{
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
													if (collection.id != 0)
													{
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
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														switch (genericController.vbLCase(CDefSection.Name))
														{
															case "contensivecdef":
																//
																// old cdef xection -- take the inner
																//
																foreach (XmlNode ChildNode in CDefSection.ChildNodes)
																{
																	CollectionWrapper += Environment.NewLine + ChildNode.OuterXml;
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
													if (!string.IsNullOrEmpty(CollectionWrapper))
													{
														//
														// -- Use the upgrade code to import this part
														CollectionWrapper = "<" + CollectionFileRootNode + ">" + CollectionWrapper + "</" + CollectionFileRootNode + ">";
														bool isBaseCollection = (baseCollectionGuid == CollectionGuid);
														installCollectionFromLocalRepo_BuildDbFromXmlData(cpCore, CollectionWrapper, IsNewBuild, isBaseCollection, nonCriticalErrorList);
														//
														// -- Process nodes to save Collection data
														XmlDocument NavDoc = new XmlDocument();
														loadOK = true;
														try
														{
															NavDoc.LoadXml(CollectionWrapper);
														}
														catch (Exception ex)
														{
															//
															// error - Need a way to reach the user that submitted the file
															//
															logController.appendInstallLog(cpCore, "Creating navigator entries, there was an error parsing the portion of the collection that contains cdef. Navigator entry creation was aborted. [There was an error reading the Meta data file.]");
															result = false;
															return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the xml collection file has an error.</P>";
															loadOK = false;
														}
														if (loadOK)
														{
															foreach (XmlNode CDefNode in NavDoc.DocumentElement.ChildNodes)
															{
																switch (genericController.vbLCase(CDefNode.Name))
																{
																	case "cdef":
																		string ContentName = GetXMLAttribute(cpCore, IsFound, CDefNode, "name", "");
																		//
																		// setup cdef rule
																		//
																		int ContentID = Models.Complex.cdefModel.getContentId(cpCore, ContentName);
																		if (ContentID > 0)
																		{
																			int CS = cpCore.db.csInsertRecord("Add-on Collection CDef Rules", 0);
																			if (cpCore.db.csOk(CS))
																			{
																				cpCore.db.csSet(CS, "Contentid", ContentID);
																				cpCore.db.csSet(CS, "CollectionID", collection.id);
																			}
																			cpCore.db.csClose(CS);
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
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														switch (genericController.vbLCase(CDefSection.Name))
														{
															case "data":
																//
																// import content
																//   This can only be done with matching guid
																//
																foreach (XmlNode ContentNode in CDefSection.ChildNodes)
																{
																	if (genericController.vbLCase(ContentNode.Name) == "record")
																	{
																		//
																		// Data.Record node
																		//
																		string ContentName = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "");
																		if (string.IsNullOrEmpty(ContentName))
																		{
																			logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.");
																			result = false;
																			return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
																		}
																		else
																		{
																			string ContentRecordGuid = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "");
																			string ContentRecordName = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "");
																			if ((string.IsNullOrEmpty(ContentRecordGuid)) && (string.IsNullOrEmpty(ContentRecordName)))
																			{
																				logController.appendInstallLog(cpCore, "install collection file contains a data record node with neither guid nor name. It must have either a name or a guid attribute. The content is [" + ContentName + "]");
																				result = false;
																				return_ErrorMessage = return_ErrorMessage + "<P>The collection was not installed because the Collection file contains a data record node with neither name nor guid. This is not allowed. The content is [" + ContentName + "].</P>";
																			}
																			else
																			{
																				//
																				// create or update the record
																				//
																				Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
																				int cs = -1;
																				if (!string.IsNullOrEmpty(ContentRecordGuid))
																				{
																					cs = cpCore.db.csOpen(ContentName, "ccguid=" + cpCore.db.encodeSQLText(ContentRecordGuid));
																				}
																				else
																				{
																					cs = cpCore.db.csOpen(ContentName, "name=" + cpCore.db.encodeSQLText(ContentRecordName));
																				}
																				bool recordfound = true;
																				if (!cpCore.db.csOk(cs))
																				{
																					//
																					// Insert the new record
																					//
																					recordfound = false;
																					cpCore.db.csClose(cs);
																					cs = cpCore.db.csInsertRecord(ContentName, 0);
																				}
																				if (cpCore.db.csOk(cs))
																				{
																					//
																					// Update the record
																					//
																					if (recordfound && (!string.IsNullOrEmpty(ContentRecordGuid)))
																					{
																						//
																						// found by guid, use guid in list and save name
																						//
																						cpCore.db.csSet(cs, "name", ContentRecordName);
																						DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordGuid;
																					}
																					else if (recordfound)
																					{
																						//
																						// record found by name, use name is list but do not add guid
																						//
																						DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordName;
																					}
																					else
																					{
																						//
																						// record was created
																						//
																						cpCore.db.csSet(cs, "ccguid", ContentRecordGuid);
																						cpCore.db.csSet(cs, "name", ContentRecordName);
																						DataRecordList = DataRecordList + Environment.NewLine + ContentName + "," + ContentRecordGuid;
																					}
																				}
																				cpCore.db.csClose(cs);
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
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														switch (genericController.vbLCase(CDefSection.Name))
														{
															case "data":
																//
																// import content
																//   This can only be done with matching guid
																//
																//OtherXML = OtherXML & vbCrLf & CDefSection.xml
																//
																foreach (XmlNode ContentNode in CDefSection.ChildNodes)
																{
																	if (genericController.vbLCase(ContentNode.Name) == "record")
																	{
																		//
																		// Data.Record node
																		//
																		string ContentName = GetXMLAttribute(cpCore, IsFound, ContentNode, "content", "");
																		if (string.IsNullOrEmpty(ContentName))
																		{
																			logController.appendInstallLog(cpCore, "install collection file contains a data.record node with a blank content attribute.");
																			result = false;
																			return_ErrorMessage = return_ErrorMessage + "<P>Collection file contains a data.record node with a blank content attribute.</P>";
																		}
																		else
																		{
																			string ContentRecordGuid = GetXMLAttribute(cpCore, IsFound, ContentNode, "guid", "");
																			string ContentRecordName = GetXMLAttribute(cpCore, IsFound, ContentNode, "name", "");
																			if ((!string.IsNullOrEmpty(ContentRecordGuid)) | (!string.IsNullOrEmpty(ContentRecordName)))
																			{
																				Models.Complex.cdefModel CDef = Models.Complex.cdefModel.getCdef(cpCore, ContentName);
																				int cs = -1;
																				if (!string.IsNullOrEmpty(ContentRecordGuid))
																				{
																					cs = cpCore.db.csOpen(ContentName, "ccguid=" + cpCore.db.encodeSQLText(ContentRecordGuid));
																				}
																				else
																				{
																					cs = cpCore.db.csOpen(ContentName, "name=" + cpCore.db.encodeSQLText(ContentRecordName));
																				}
																				if (cpCore.db.csOk(cs))
																				{
																					//
																					// Update the record
																					foreach (XmlNode FieldNode in ContentNode.ChildNodes)
																					{
																						if (FieldNode.Name.ToLower() == "field")
																						{
																							bool IsFieldFound = false;
																							string FieldName = GetXMLAttribute(cpCore, IsFound, FieldNode, "name", "").ToLower();
																							int fieldTypeId = -1;
																							int FieldLookupContentID = -1;
																							foreach (var keyValuePair in CDef.fields)
																							{
																								Models.Complex.CDefFieldModel field = keyValuePair.Value;
																								if (genericController.vbLCase(field.nameLc) == FieldName)
																								{
																									fieldTypeId = field.fieldTypeId;
																									FieldLookupContentID = field.lookupContentID;
																									IsFieldFound = true;
																									break;
																								}
																							}
																							//For Ptr = 0 To CDef.fields.count - 1
																							//    CDefField = CDef.fields(Ptr)
																							//    If genericController.vbLCase(CDefField.Name) = FieldName Then
																							//        fieldType = CDefField.fieldType
																							//        FieldLookupContentID = CDefField.LookupContentID
																							//        IsFieldFound = True
																							//        Exit For
																							//    End If
																							//Next
																							if (IsFieldFound)
																							{
																								string FieldValue = FieldNode.InnerText;
																								switch (fieldTypeId)
																								{
																									case FieldTypeIdAutoIdIncrement:
																									case FieldTypeIdRedirect:
																									{
																													//
																													// not supported
																													//
																									break;
																									}
																									case FieldTypeIdLookup:
																									{
																										//
																										// read in text value, if a guid, use it, otherwise assume name
																										//
																										if (FieldLookupContentID != 0)
																										{
																											string FieldLookupContentName = models.complex.cdefmodel.getContentNameByID(cpCore,FieldLookupContentID);
																											if (!string.IsNullOrEmpty(FieldLookupContentName))
																											{
																												if ((FieldValue.Substring(0, 1) == "{") && (FieldValue.Substring(FieldValue.Length - 1) == "}") && Models.Complex.cdefModel.isContentFieldSupported(cpCore, FieldLookupContentName, "ccguid"))
																												{
																													//
																													// Lookup by guid
																													//
																													int fieldLookupId = genericController.EncodeInteger(cpCore.db.GetRecordIDByGuid(FieldLookupContentName, FieldValue));
																													if (fieldLookupId <= 0)
																													{
																														return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
																													}
																													else
																													{
																														cpCore.db.csSet(cs, FieldName, fieldLookupId);
																													}
																												}
																												else
																												{
																													//
																													// lookup by name
																													//
																													if (!string.IsNullOrEmpty(FieldValue))
																													{
																														int fieldLookupId = cpCore.db.getRecordID(FieldLookupContentName, FieldValue);
																														if (fieldLookupId <= 0)
																														{
																															return_ErrorMessage = return_ErrorMessage + "<P>Warning: There was a problem translating field [" + FieldName + "] in record [" + ContentName + "] because the record it refers to was not found in this site.</P>";
																														}
																														else
																														{
																															cpCore.db.csSet(cs, FieldName, fieldLookupId);
																														}
																													}
																												}
																											}
																										}
																										else if (genericController.vbIsNumeric(FieldValue))
																										{
																											//
																											// must be lookup list
																											//
																											cpCore.db.csSet(cs, FieldName, FieldValue);
																										}
																										break;
																									}
																									default:
																									{
																										cpCore.db.csSet(cs, FieldName, FieldValue);
																										break;
																									}
																								}
																							}
																						}
																					}
																				}
																				cpCore.db.csClose(cs);
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
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														switch (genericController.vbLCase(CDefSection.Name))
														{
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
																if (string.IsNullOrEmpty(ChildCollectionGUID))
																{
																	ChildCollectionGUID = CDefSection.InnerText;
																}
																if (!string.IsNullOrEmpty(ChildCollectionGUID))
																{
																	int ChildCollectionID = 0;
																	int cs = -1;
																	cs = cpCore.db.csOpen("Add-on Collections", "ccguid=" + cpCore.db.encodeSQLText(ChildCollectionGUID),,,,,, "id");
																	if (cpCore.db.csOk(cs))
																	{
																		ChildCollectionID = cpCore.db.csGetInteger(cs, "id");
																	}
																	cpCore.db.csClose(cs);
																	if (ChildCollectionID != 0)
																	{
																		cs = cpCore.db.csInsertRecord("Add-on Collection Parent Rules", 0);
																		if (cpCore.db.csOk(cs))
																		{
																			cpCore.db.csSet(cs, "ParentID", collection.id);
																			cpCore.db.csSet(cs, "ChildID", ChildCollectionID);
																		}
																		cpCore.db.csClose(cs);
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
																if (!result)
																{
																	result = result;
																}
																break;
															case "interfaces":
																//
																// Legacy Interface Node
																//
																foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes)
																{
																	InstallCollectionFromLocalRepo_addonNode_Phase1(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, ref result, ref return_ErrorMessage);
																	if (!result)
																	{
																		result = result;
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
													foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
													{
														switch (genericController.vbLCase(CDefSection.Name))
														{
															case "addon":
															case "add-on":
																//
																// Add-on Node, do part 1 of 2
																//   (include add-on node must be done after all add-ons are installed)
																//
																InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefSection, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage);
																if (!result)
																{
																	result = result;
																}
																break;
															case "interfaces":
																//
																// Legacy Interface Node
																//
																foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes)
																{
																	InstallCollectionFromLocalRepo_addonNode_Phase2(cpCore, CDefInterfaces, "ccguid", cpCore.siteProperties.dataBuildVersion, collection.id, result, return_ErrorMessage);
																	if (!result)
																	{
																		result = result;
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
			}
			catch (Exception ex)
			{
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
		public static string getCollectionListFile(coreClass cpCore)
		{
			string returnXml = "";
			try
			{
				string LastChangeDate = string.Empty;
				IO.DirectoryInfo SubFolder = null;
				IO.DirectoryInfo[] SubFolderList = null;
				string FolderName = null;
				string collectionFilePathFilename = null;
				string CollectionGuid = null;
				string Collectionname = null;
				int Pos = 0;
				IO.DirectoryInfo[] FolderList = null;
				//
				collectionFilePathFilename = cpCore.addon.getPrivateFilesAddonPath + "Collections.xml";
				returnXml = cpCore.privateFiles.readFile(collectionFilePathFilename);
				if (string.IsNullOrEmpty(returnXml))
				{
					FolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath);
					if (FolderList.Count() > 0)
					{
						foreach (IO.DirectoryInfo folder in FolderList)
						{
							FolderName = folder.Name;
							Pos = genericController.vbInstr(1, FolderName, "\t");
							if (Pos > 1)
							{
								//hint = hint & ",800"
								FolderName = FolderName.Substring(0, Pos - 1);
								if (FolderName.Length > 34)
								{
									if (genericController.vbLCase(FolderName.Substring(0, 4)) != "temp")
									{
										CollectionGuid = FolderName.Substring(FolderName.Length - 32);
										Collectionname = FolderName.Substring(0, FolderName.Length - CollectionGuid.Length - 1);
										CollectionGuid = CollectionGuid.Substring(0, 8) + "-" + CollectionGuid.Substring(8, 4) + "-" + CollectionGuid.Substring(12, 4) + "-" + CollectionGuid.Substring(16, 4) + "-" + CollectionGuid.Substring(20);
										CollectionGuid = "{" + CollectionGuid + "}";
										SubFolderList = cpCore.privateFiles.getFolderList(cpCore.addon.getPrivateFilesAddonPath() + "\\" + FolderName);
										if (SubFolderList.Count() > 0)
										{
											SubFolder = SubFolderList[SubFolderList.Count() - 1];
											FolderName = FolderName + "\\" + SubFolder.Name;
											LastChangeDate = SubFolder.Name.Substring(4, 2) + "/" + SubFolder.Name.Substring(6, 2) + "/" + SubFolder.Name.Substring(0, 4);
											if (!DateHelper.IsDate(LastChangeDate))
											{
												LastChangeDate = "";
											}
										}
										returnXml = returnXml + Environment.NewLine + "\t" + "<Collection>";
										returnXml = returnXml + Environment.NewLine + "\t" + "\t" + "<name>" + Collectionname + "</name>";
										returnXml = returnXml + Environment.NewLine + "\t" + "\t" + "<guid>" + CollectionGuid + "</guid>";
										returnXml = returnXml + Environment.NewLine + "\t" + "\t" + "<lastchangedate>" + LastChangeDate + "</lastchangedate>";
										returnXml += Environment.NewLine + "\t" + "\t" + "<path>" + FolderName + "</path>";
										returnXml = returnXml + Environment.NewLine + "\t" + "</Collection>";
									}
								}
							}
						}
					}
					returnXml = "<CollectionList>" + returnXml + Environment.NewLine + "</CollectionList>";
					cpCore.privateFiles.saveFile(collectionFilePathFilename, returnXml);
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnXml;
		}
		//
		//
		//
		private static void UpdateConfig(coreClass cpCore, string Collectionname, string CollectionGuid, DateTime CollectionUpdatedDate, string CollectionVersionFolderName)
		{
			try
			{
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
				try
				{
					Doc.LoadXml(getCollectionListFile(cpCore));
				}
				catch (Exception ex)
				{
					logController.appendInstallLog(cpCore, "UpdateConfig, Error loading Collections.xml file.");
				}
				if (loadOK)
				{
					if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))
					{
						logController.appendInstallLog(cpCore, "UpdateConfig, The Collections.xml file has an invalid root node, [" + Doc.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.");
					}
					else
					{
						if (genericController.vbLCase(Doc.DocumentElement.Name) == "collectionlist")
						{
							CollectionFound = false;
							foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes)
							{
								switch (genericController.vbLCase(LocalListNode.Name))
								{
									case "collection":
										LocalGuid = "";
										foreach (XmlNode CollectionNode in LocalListNode.ChildNodes)
										{
											switch (genericController.vbLCase(CollectionNode.Name))
											{
												case "guid":
													//
													LocalGuid = genericController.vbLCase(CollectionNode.InnerText);
//INSTANT C# WARNING: Exit statements not matching the immediately enclosing block are converted using a 'goto' statement:
//ORIGINAL LINE: Exit For
													goto ExitLabel1;
											}
										}
										ExitLabel1:
										if (genericController.vbLCase(LocalGuid) == genericController.vbLCase(CollectionGuid))
										{
											CollectionFound = true;
											foreach (XmlNode CollectionNode in LocalListNode.ChildNodes)
											{
												switch (genericController.vbLCase(CollectionNode.Name))
												{
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
							if (!CollectionFound)
							{
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//
		//
		private static string GetCollectionPath(coreClass cpCore, string CollectionGuid)
		{
			string result = string.Empty;
			try
			{
				DateTime LastChangeDate = default(DateTime);
				string Collectionname = "";
				GetCollectionConfig(cpCore, CollectionGuid, ref result, ref LastChangeDate, ref Collectionname);
			}
			catch (Exception ex)
			{
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
		public static void GetCollectionConfig(coreClass cpCore, string CollectionGuid, ref string return_CollectionPath, ref DateTime return_LastChagnedate, ref string return_CollectionName)
		{
			try
			{
				string LocalPath = null;
				string LocalGuid = string.Empty;
				XmlDocument Doc = new XmlDocument();
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlNode CollectionNode = null;
//INSTANT C# NOTE: Commented this declaration since looping variables in 'foreach' loops are declared in the 'foreach' header in C#:
//				XmlNode LocalListNode = null;
				bool CollectionFound = false;
				string CollectionPath = string.Empty;
				DateTime LastChangeDate = default(DateTime);
				string hint = string.Empty;
				bool MatchFound = false;
				string LocalName = null;
				bool loadOK = false;
				//
				MatchFound = false;
				return_CollectionPath = "";
				return_LastChagnedate = DateTime.MinValue;
				loadOK = true;
				try
				{
					Doc.LoadXml(getCollectionListFile(cpCore));
				}
				catch (Exception ex)
				{
					//hint = hint & ",parse error"
					logController.appendInstallLog(cpCore, "GetCollectionConfig, Hint=[" + hint + "], Error loading Collections.xml file.");
					loadOK = false;
				}
				if (loadOK)
				{
					if (genericController.vbLCase(Doc.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))
					{
						logController.appendInstallLog(cpCore, "Hint=[" + hint + "], The Collections.xml file has an invalid root node");
					}
					else
					{
						if (true)
						{
							//If genericController.vbLCase(.name) <> "collectionlist" Then
							//    Call AppendClassLogFile("Server", "GetCollectionConfig", "Collections.xml file error, root node was not collectionlist, [" & .name & "].")
							//Else
							CollectionFound = false;
							//hint = hint & ",checking nodes [" & .ChildNodes.Count & "]"
							foreach (XmlNode LocalListNode in Doc.DocumentElement.ChildNodes)
							{
								LocalName = "no name found";
								LocalPath = "";
								switch (genericController.vbLCase(LocalListNode.Name))
								{
									case "collection":
										LocalGuid = "";
										foreach (XmlNode CollectionNode in LocalListNode.ChildNodes)
										{
											switch (genericController.vbLCase(CollectionNode.Name))
											{
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
								if (genericController.vbLCase(CollectionGuid) == LocalGuid)
								{
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//======================================================================================================
		// Installs Addons in a source folder
		//======================================================================================================
		//
		public static bool InstallCollectionsFromPrivateFolder(coreClass cpCore, string privateFolder, ref string return_ErrorMessage, ref List<string> return_CollectionGUIDList, bool IsNewBuild, ref List<string> nonCriticalErrorList)
		{
			bool returnSuccess = false;
			try
			{
				DateTime CollectionLastChangeDate;
				//
				CollectionLastChangeDate = DateTime.Now;
				returnSuccess = BuildLocalCollectionReposFromFolder(cpCore, privateFolder, CollectionLastChangeDate, return_CollectionGUIDList, return_ErrorMessage, false);
				if (!returnSuccess)
				{
					//
					// BuildLocal failed, log it and do not upgrade
					//
					logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
				}
				else
				{
					foreach (string collectionGuid in return_CollectionGUIDList)
					{
						if (!installCollectionFromLocalRepo(cpCore, collectionGuid, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, ref nonCriticalErrorList))
						{
							logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
							break;
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				returnSuccess = false;
				if (string.IsNullOrEmpty(return_ErrorMessage))
				{
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
		public static bool InstallCollectionsFromPrivateFile(coreClass cpCore, string pathFilename, ref string return_ErrorMessage, ref string return_CollectionGUID, bool IsNewBuild, ref List<string> nonCriticalErrorList)
		{
			bool returnSuccess = false;
			try
			{
				DateTime CollectionLastChangeDate;
				//
				CollectionLastChangeDate = DateTime.Now;
				returnSuccess = BuildLocalCollectionRepoFromFile(cpCore, pathFilename, CollectionLastChangeDate, return_CollectionGUID, return_ErrorMessage, false);
				if (!returnSuccess)
				{
					//
					// BuildLocal failed, log it and do not upgrade
					//
					logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder returned false with Error Message [" + return_ErrorMessage + "], exiting without calling UpgradeAllAppsFromLocalCollection");
				}
				else
				{
					returnSuccess = installCollectionFromLocalRepo(cpCore, return_CollectionGUID, cpCore.siteProperties.dataBuildVersion, ref return_ErrorMessage, "", IsNewBuild, ref nonCriticalErrorList);
					if (!returnSuccess)
					{
						//
						// Upgrade all apps failed
						//
						logController.appendInstallLog(cpCore, "UpgradeAllAppsFromLocalCollection returned false with Error Message [" + return_ErrorMessage + "].");
					}
					else
					{
						returnSuccess = true;
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				returnSuccess = false;
				if (string.IsNullOrEmpty(return_ErrorMessage))
				{
					return_ErrorMessage = "There was an unexpected error installing the collection, details [" + ex.Message + "]";
				}
			}
			return returnSuccess;
		}
		//
		//
		//
		private static int GetNavIDByGuid(coreClass cpCore, string ccGuid)
		{
			int navId = 0;
			try
			{
				int CS;
				//
				CS = cpCore.db.csOpen(cnNavigatorEntries, "ccguid=" + cpCore.db.encodeSQLText(ccGuid), "ID",,,,, "ID");
				if (cpCore.db.csOk(CS))
				{
					navId = cpCore.db.csGetInteger(CS, "id");
				}
				cpCore.db.csClose(CS);
			}
			catch (Exception ex)
			{
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
		private static void CopyInstallToDst(coreClass cpCore, string SrcPath, string DstPath, string BlockFileList, string BlockFolderList)
		{
			try
			{
				IO.FileInfo[] FileInfoArray = null;
				IO.DirectoryInfo[] FolderInfoArray = null;
				string SrcFolder = null;
				string DstFolder = null;
				//
				SrcFolder = SrcPath;
				if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\")
				{
					SrcFolder = SrcFolder.Substring(0, SrcFolder.Length - 1);
				}
				//
				DstFolder = DstPath;
				if (DstFolder.Substring(DstFolder.Length - 1) == "\\")
				{
					DstFolder = DstFolder.Substring(0, DstFolder.Length - 1);
				}
				//
				if (cpCore.privateFiles.pathExists(SrcFolder))
				{
					FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder);
					foreach (IO.FileInfo file in FileInfoArray)
					{
						if ((file.Extension == "dll") || (file.Extension == "exe") || (file.Extension == "zip"))
						{
							//
							// can not copy dll or exe
							//
							//Filename = Filename
						}
						else if ("," + BlockFileList + ",".IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)
						{
							//
							// can not copy the current collection file
							//
							//file.Name = file.Name
						}
						else
						{
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
					foreach (IO.DirectoryInfo folder in FolderInfoArray)
					{
						if ("," + BlockFolderList + ",".IndexOf("," + folder.Name + ",", System.StringComparison.OrdinalIgnoreCase) + 1 == 0)
						{
							CopyInstallToDst(cpCore, SrcPath + folder.Name + "\\", DstPath + folder.Name + "\\", BlockFileList, "");
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}
		//
		//
		//
		private static string GetCollectionFileList(coreClass cpCore, string SrcPath, string SubFolder, string ExcludeFileList)
		{
			string result = "";
			try
			{
				IO.FileInfo[] FileInfoArray = null;
				IO.DirectoryInfo[] FolderInfoArray = null;
				string SrcFolder;
				//
				SrcFolder = SrcPath + SubFolder;
				if (SrcFolder.Substring(SrcFolder.Length - 1) == "\\")
				{
					SrcFolder = SrcFolder.Substring(0, SrcFolder.Length - 1);
				}
				//
				if (cpCore.privateFiles.pathExists(SrcFolder))
				{
					FileInfoArray = cpCore.privateFiles.getFileList(SrcFolder);
					foreach (IO.FileInfo file in FileInfoArray)
					{
						if ("," + ExcludeFileList + ",".IndexOf("," + file.Name + ",", System.StringComparison.OrdinalIgnoreCase) + 1 != 0)
						{
							//
							// can not copy the current collection file
							//
							//Filename = Filename
						}
						else
						{
							//
							// copy this file to destination
							//
							result = result + Environment.NewLine + SubFolder + file.Name;
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
					foreach (IO.DirectoryInfo folder in FolderInfoArray)
					{
						result = result + GetCollectionFileList(cpCore, SrcPath, SubFolder + folder.Name + "\\", ExcludeFileList);
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
		//
		//
		private static void InstallCollectionFromLocalRepo_addonNode_Phase1(coreClass cpCore, XmlNode AddonNode, string AddonGuidFieldName, string ignore_BuildVersion, int CollectionID, ref bool return_UpgradeOK, ref string return_ErrorMessage)
		{
			try
			{
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
				string FieldValue = string.Empty;
				int CS2 = 0;
				string ScriptingNameorGuid = string.Empty;
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
				if ((Basename == "page") || (Basename == "process") || (Basename == "addon") || (Basename == "add-on"))
				{
					addonName = GetXMLAttribute(cpCore, IsFound, AddonNode, "name", "No Name");
					if (string.IsNullOrEmpty(addonName))
					{
						addonName = "No Name";
					}
					addonGuid = GetXMLAttribute(cpCore, IsFound, AddonNode, "guid", addonName);
					if (string.IsNullOrEmpty(addonGuid))
					{
						addonGuid = addonName;
					}
					navTypeName = GetXMLAttribute(cpCore, IsFound, AddonNode, "type", "");
					navTypeId = GetListIndex(navTypeName, navTypeIDList);
					if (navTypeId == 0)
					{
						navTypeId = NavTypeIDAddon;
					}
					Criteria = "(" + AddonGuidFieldName + "=" + cpCore.db.encodeSQLText(addonGuid) + ")";
					CS = cpCore.db.csOpen(cnAddons, Criteria,, false);
					if (cpCore.db.csOk(CS))
					{
						//
						// Update the Addon
						//
						logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, GUID match with existing Add-on, Updating Add-on [" + addonName + "], Guid [" + addonGuid + "]");
					}
					else
					{
						//
						// not found by GUID - search name against name to update legacy Add-ons
						//
						cpCore.db.csClose(CS);
						Criteria = "(name=" + cpCore.db.encodeSQLText(addonName) + ")and(" + AddonGuidFieldName + " is null)";
						CS = cpCore.db.csOpen(cnAddons, Criteria,, false);
						if (cpCore.db.csOk(CS))
						{
							logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on name matched an existing Add-on that has no GUID, Updating legacy Aggregate Function to Add-on [" + addonName + "], Guid [" + addonGuid + "]");
						}
					}
					if (!cpCore.db.csOk(CS))
					{
						//
						// not found by GUID or by name, Insert a new addon
						//
						cpCore.db.csClose(CS);
						CS = cpCore.db.csInsertRecord(cnAddons, 0);
						if (cpCore.db.csOk(CS))
						{
							logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Creating new Add-on [" + addonName + "], Guid [" + addonGuid + "]");
						}
					}
					if (!cpCore.db.csOk(CS))
					{
						//
						// Could not create new Add-on
						//
						logController.appendInstallLog(cpCore, "UpgradeAppFromLocalCollection, Add-on could not be created, skipping Add-on [" + addonName + "], Guid [" + addonGuid + "]");
					}
					else
					{
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
						if (AddonNode.ChildNodes.Count > 0)
						{
							foreach (XmlNode PageInterfaceWithinLoop in AddonNode.ChildNodes)
							{
								PageInterface = PageInterfaceWithinLoop;
								switch (genericController.vbLCase(PageInterfaceWithinLoop.Name))
								{
									case "activexdll":
										//
										// This is handled in BuildLocalCollectionFolder
										//
									break;
									case "editors":
										//
										// list of editors
										//
										foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes)
										{
											switch (genericController.vbLCase(TriggerNode.Name))
											{
												case "type":
													fieldType = TriggerNode.InnerText;
													fieldTypeID = cpCore.db.getRecordID("Content Field Types", fieldType);
													if (fieldTypeID > 0)
													{
														Criteria = "(addonid=" + addonId + ")and(contentfieldTypeID=" + fieldTypeID + ")";
														CS2 = cpCore.db.csOpen("Add-on Content Field Type Rules", Criteria);
														if (!cpCore.db.csOk(CS2))
														{
															cpCore.db.csClose(CS2);
															CS2 = cpCore.db.csInsertRecord("Add-on Content Field Type Rules", 0);
														}
														if (cpCore.db.csOk(CS2))
														{
															cpCore.db.csSet(CS2, "addonid", addonId);
															cpCore.db.csSet(CS2, "contentfieldTypeID", fieldTypeID);
														}
														cpCore.db.csClose(CS2);
													}
													break;
											}
										}
										break;
									case "processtriggers":
										//
										// list of events that trigger a process run for this addon
										//
										foreach (XmlNode TriggerNode in PageInterfaceWithinLoop.ChildNodes)
										{
											switch (genericController.vbLCase(TriggerNode.Name))
											{
												case "contentchange":
													TriggerContentID = 0;
													ContentNameorGuid = TriggerNode.InnerText;
													if (string.IsNullOrEmpty(ContentNameorGuid))
													{
														ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "guid", "");
														if (string.IsNullOrEmpty(ContentNameorGuid))
														{
															ContentNameorGuid = GetXMLAttribute(cpCore, IsFound, TriggerNode, "name", "");
														}
													}
													Criteria = "(ccguid=" + cpCore.db.encodeSQLText(ContentNameorGuid) + ")";
													CS2 = cpCore.db.csOpen("Content", Criteria);
													if (!cpCore.db.csOk(CS2))
													{
														cpCore.db.csClose(CS2);
														Criteria = "(ccguid is null)and(name=" + cpCore.db.encodeSQLText(ContentNameorGuid) + ")";
														CS2 = cpCore.db.csOpen("content", Criteria);
													}
													if (cpCore.db.csOk(CS2))
													{
														TriggerContentID = cpCore.db.csGetInteger(CS2, "ID");
													}
													cpCore.db.csClose(CS2);
													//If TriggerContentID = 0 Then
													//    CS2 = cpCore.db.cs_insertRecord("Scripting Modules", 0)
													//    If cpCore.db.cs_ok(CS2) Then
													//        Call cpCore.db.cs_set(CS2, "name", ScriptingNameorGuid)
													//        Call cpCore.db.cs_set(CS2, "ccguid", ScriptingNameorGuid)
													//        TriggerContentID = cpCore.db.cs_getInteger(CS2, "ID")
													//    End If
													//    Call cpCore.db.cs_Close(CS2)
													//End If
													if (TriggerContentID == 0)
													{
														//
														// could not find the content
														//
													}
													else
													{
														Criteria = "(addonid=" + addonId + ")and(contentid=" + TriggerContentID + ")";
														CS2 = cpCore.db.csOpen("Add-on Content Trigger Rules", Criteria);
														if (!cpCore.db.csOk(CS2))
														{
															cpCore.db.csClose(CS2);
															CS2 = cpCore.db.csInsertRecord("Add-on Content Trigger Rules", 0);
															if (cpCore.db.csOk(CS2))
															{
																cpCore.db.csSet(CS2, "addonid", addonId);
																cpCore.db.csSet(CS2, "contentid", TriggerContentID);
															}
														}
														cpCore.db.csClose(CS2);
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
										foreach (XmlNode ScriptingNode in PageInterfaceWithinLoop.ChildNodes)
										{
											switch (genericController.vbLCase(ScriptingNode.Name))
											{
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
										if (!string.IsNullOrEmpty(menuNameSpace))
										{
											NavIconTypeString = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "type", "");
											if (string.IsNullOrEmpty(NavIconTypeString))
											{
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
										NewValue = Convert.ToString(PageInterfaceWithinLoop.InnerText).Trim(' ');
										if (!string.IsNullOrEmpty(NewValue))
										{
											if (string.IsNullOrEmpty(ArgumentList))
											{
												ArgumentList = NewValue;
											}
											else if (NewValue != FieldValue)
											{
												ArgumentList = ArgumentList + Environment.NewLine + NewValue;
											}
										}
										break;
									case "style":
										//
										// import exclusive style
										//
										NodeName = GetXMLAttribute(cpCore, IsFound, PageInterfaceWithinLoop, "name", "");
										NewValue = Convert.ToString(PageInterfaceWithinLoop.InnerText).Trim(' ');
										if (NewValue.Substring(0, 1) != "{")
										{
											NewValue = "{" + NewValue;
										}
										if (NewValue.Substring(NewValue.Length - 1) != "}")
										{
											NewValue = NewValue + "}";
										}
										StyleSheet = StyleSheet + Environment.NewLine + NodeName + " " + NewValue;
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
										if (!string.IsNullOrEmpty(test))
										{
											StyleSheet = StyleSheet + Environment.NewLine + PageInterfaceWithinLoop.InnerText;
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
										if (!cpCore.db.cs_isFieldSupported(CS, FieldName))
										{
											//
											// Bad field name - need to report it somehow
											//
										}
										else
										{
											cpCore.db.csSet(CS, FieldName, FieldValue);
											if (genericController.EncodeBoolean(PageInterfaceWithinLoop.InnerText))
											{
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
										if (!string.IsNullOrEmpty(FieldValue))
										{
											//
											// Icons can be either in the root of the website or in content files
											//
											FieldValue = genericController.vbReplace(FieldValue, "\\", "/"); // make it a link, not a file
											if (genericController.vbInstr(1, FieldValue, "://") != 0)
											{
												//
												// the link is an absolute URL, leave it link this
												//
											}
											else
											{
												if (FieldValue.Substring(0, 1) != "/")
												{
													//
													// make sure it starts with a slash to be consistance
													//
													FieldValue = "/" + FieldValue;
												}
												if (FieldValue.Substring(0, 17) == "/contensivefiles/")
												{
													//
													// in content files, start link without the slash
													//
													FieldValue = FieldValue.Substring(17);
												}
											}
											cpCore.db.csSet(CS, "IconFilename", FieldValue);
											if (true)
											{
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
										if (true)
										{
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
										if (!cpCore.db.cs_isFieldSupported(CS, FieldName))
										{
											//
											// Bad field name - need to report it somehow
											//
											cpCore.handleException(new ApplicationException("bad field found [" + FieldName + "], in addon node [" + addonName + "], of collection [" + cpCore.db.getRecordName("add-on collections", CollectionID) + "]"));
										}
										else
										{
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
					cpCore.db.csClose(CS);
					//
					// -- if this is needed, the installation xml files are available in the addon install folder. - I do not believe this is important
					//       as if a collection is missing a dependancy, there is an error and you would expect to have to reinstall.
					//'
					//' Addon is now fully installed
					//' Go through all collection files on this site and see if there are
					//' any Dependencies on this add-on that need to be attached
					//' src args are those for the addon that includes the current addon
					//'   - if this addon is the target of another add-on's  "includeaddon" node
					//'
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
					//                                'On Error Resume Next
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
		}










	}
}
