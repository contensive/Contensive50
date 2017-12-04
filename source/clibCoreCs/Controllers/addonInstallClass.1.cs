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
		//====================================================================================================
		//
		//   DownloadCollectionFiles
		//
		//   Download Library collection files into a folder
		//       Download Collection file and all attachments (DLLs) into working folder
		//       Unzips any collection files
		//       Returns true if it all downloads OK
		//
		private static bool DownloadCollectionFiles(coreClass cpCore, string workingPath, string CollectionGuid, ref DateTime return_CollectionLastChangeDate, ref string return_ErrorMessage)
		{
			bool tempDownloadCollectionFiles = false;
			tempDownloadCollectionFiles = false;
			try
			{
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
				do
				{
					try
					{
						tempDownloadCollectionFiles = true;
						return_ErrorMessage = "";
						//
						// -- pause for a second between fetches to pace the server (<10 hits in 10 seconds)
						Threading.Thread.Sleep(downloadDelay);
						//
						// -- download file
						System.Net.WebRequest rq = System.Net.WebRequest.Create(URL);
						rq.Timeout = 60000;
						System.Net.WebResponse response = rq.GetResponse();
						IO.Stream responseStream = response.GetResponseStream();
						XmlTextReader reader = new XmlTextReader(responseStream);
						Doc.Load(reader);
						break;
						//Call Doc.Load(URL)
					}
					catch (Exception ex)
					{
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
				if (string.IsNullOrEmpty(return_ErrorMessage))
				{
					//
					// continue if no errors
					//
					if (Doc.DocumentElement.Name.ToLower() != genericController.vbLCase(DownloadFileRootNode))
					{
						return_ErrorMessage = "The collection file from the server was Not valid for collection [" + CollectionGuid + "]";
						tempDownloadCollectionFiles = false;
						logController.appendInstallLog(cpCore, errorPrefix + "The response has a basename [" + Doc.DocumentElement.Name + "] but [" + DownloadFileRootNode + "] was expected.");
					}
					else
					{
						//
						//------------------------------------------------------------------
						// Parse the Download File and download each file into the working folder
						//------------------------------------------------------------------
						//
						if (Doc.DocumentElement.ChildNodes.Count == 0)
						{
							return_ErrorMessage = "The collection library status file from the server has a valid basename, but no childnodes.";
							logController.appendInstallLog(cpCore, errorPrefix + "The collection library status file from the server has a valid basename, but no childnodes. The collection was probably Not found");
							tempDownloadCollectionFiles = false;
						}
						else
						{
							foreach (XmlNode CDefSection in Doc.DocumentElement.ChildNodes)
							{
								switch (genericController.vbLCase(CDefSection.Name))
								{
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
										foreach (XmlNode CDefInterfaces in CDefSection.ChildNodes)
										{
											switch (genericController.vbLCase(CDefInterfaces.Name))
											{
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
													if (!string.IsNullOrEmpty(CollectionFileLink))
													{
														Pos = CollectionFileLink.LastIndexOf("/") + 1;
														if ((Pos <= 0) && (Pos < CollectionFileLink.Length))
														{
															//
															// Skip this file because the collecion file link has no slash (no file)
															//
															logController.appendInstallLog(cpCore, errorPrefix + "Collection [" + Collectionname + "] was Not installed because the Collection File Link does Not point to a valid file [" + CollectionFileLink + "]");
														}
														else
														{
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
													foreach (XmlNode ActiveXNode in CDefInterfaces.ChildNodes)
													{
														switch (genericController.vbLCase(ActiveXNode.Name))
														{
															case "filename":
																ResourceFilename = ActiveXNode.InnerText;
																break;
															case "link":
																ResourceLink = ActiveXNode.InnerText;
																break;
														}
													}
													if (string.IsNullOrEmpty(ResourceLink))
													{
														UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. An ActiveXDll node with filename [" + ResourceFilename + "] contained no 'Link' attribute.";
														logController.appendInstallLog(cpCore, errorPrefix + UserError);
													}
													else
													{
														if (string.IsNullOrEmpty(ResourceFilename))
														{
															//
															// Take Filename from Link
															//
															Pos = ResourceLink.LastIndexOf("/") + 1;
															if (Pos != 0)
															{
																ResourceFilename = ResourceLink.Substring(Pos);
															}
														}
														if (string.IsNullOrEmpty(ResourceFilename))
														{
															UserError = "There was an error processing a collection in the download file [" + Collectionname + "]. The ActiveX filename attribute was empty, and the filename could not be read from the link [" + ResourceLink + "].";
															logController.appendInstallLog(cpCore, errorPrefix + UserError);
														}
														else
														{
															cpCore.privateFiles.SaveRemoteFile(ResourceLink, workingPath + ResourceFilename);
														}
													}
													break;
											}
										}
										break;
								}
							}
							if (CollectionFileCnt == 0)
							{
								logController.appendInstallLog(cpCore, errorPrefix + "The collection was requested and downloaded, but was not installed because the download file did not have a collection root node.");
							}
						}
					}
				}
				//
				// no - register anything that downloaded correctly - if this collection contains an import, and one of the imports has a problem, all the rest need to continue
				//
				//'
				//If Not DownloadCollectionFiles Then
				//    '
				//    ' Must clear these out, if there is an error, a reset will keep the error message from making it to the page
				//    '
				//    Return_IISResetRequired = False
				//    Return_RegisterList = ""
				//End If
				//
			}
			catch (Exception ex)
			{
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
		public static bool installCollectionFromRemoteRepo(coreClass cpCore, string CollectionGuid, ref string return_ErrorMessage, string ImportFromCollectionsGuidList, bool IsNewBuild, ref List<string> nonCriticalErrorList)
		{
			bool UpgradeOK = true;
			try
			{
				//
				string CollectionVersionFolderName = null;
				string workingPath = null;
				DateTime CollectionLastChangeDate = default(DateTime);
				List<string> collectionGuidList = new List<string>();
				//Dim builder As New coreBuilderClass(cpCore)
				//Dim isBaseCollection As Boolean = (CollectionGuid = baseCollectionGuid)
				//
				// normalize guid
				//
				if (CollectionGuid.Length < 38)
				{
					if (CollectionGuid.Length == 32)
					{
						CollectionGuid = CollectionGuid.Substring(0, 8) + "-" + CollectionGuid.Substring(8, 4) + "-" + CollectionGuid.Substring(12, 4) + "-" + CollectionGuid.Substring(16, 4) + "-" + CollectionGuid.Substring(20);
					}
					if (CollectionGuid.Length == 36)
					{
						CollectionGuid = "{" + CollectionGuid + "}";
					}
				}
				//
				// Install it if it is not already here
				//
				CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid);
				if (string.IsNullOrEmpty(CollectionVersionFolderName))
				{
					//
					// Download all files for this collection and build the collection folder(s)
					//
					workingPath = cpCore.addon.getPrivateFilesAddonPath() + "temp_" + genericController.GetRandomInteger() + "\\";
					cpCore.privateFiles.createPath(workingPath);
					//
					UpgradeOK = DownloadCollectionFiles(cpCore, workingPath, CollectionGuid, ref CollectionLastChangeDate, ref return_ErrorMessage);
					if (!UpgradeOK)
					{
						UpgradeOK = UpgradeOK;
					}
					else
					{
						UpgradeOK = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, ref collectionGuidList, ref return_ErrorMessage, false);
						if (!UpgradeOK)
						{
							UpgradeOK = UpgradeOK;
						}
					}
					//
					cpCore.privateFiles.DeleteFileFolder(workingPath);
				}
				//
				// Upgrade the server from the collection files
				//
				if (UpgradeOK)
				{
					UpgradeOK = installCollectionFromLocalRepo(cpCore, CollectionGuid, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, ImportFromCollectionsGuidList, IsNewBuild, nonCriticalErrorList);
					if (!UpgradeOK)
					{
						UpgradeOK = UpgradeOK;
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
				throw ex;
			}
			return UpgradeOK;
		}
		//
		//====================================================================================================
		//
		// Upgrades all collections, registers and resets the server if needed
		//
		public static bool UpgradeLocalCollectionRepoFromRemoteCollectionRepo(coreClass cpCore, ref string return_ErrorMessage, ref string return_RegisterList, ref bool return_IISResetRequired, bool IsNewBuild, ref List<string> nonCriticalErrorList)
		{
			bool returnOk = true;
			try
			{
				bool localCollectionUpToDate = false;
				string[] GuidArray = {};
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
				string LibName = string.Empty;
				bool LibSystem = false;
				string LibGUID = null;
				string LibLastChangeDateStr = null;
				string LibContensiveVersion = string.Empty;
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
				if (allowLogging)
				{
					logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Enter");
				}
				LocalFile = getCollectionListFile(cpCore);
				if (!string.IsNullOrEmpty(LocalFile))
				{
					LocalCollections = new XmlDocument();
					try
					{
						LocalCollections.LoadXml(LocalFile);
					}
					catch (Exception ex)
					{
						if (allowLogging)
						{
							logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), parse error reading collections.xml");
						}
						Copy = "Error loading privateFiles\\addons\\Collections.xml";
						logController.appendInstallLog(cpCore, Copy);
						return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
						returnOk = false;
					}
					if (returnOk)
					{
						if (genericController.vbLCase(LocalCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))
						{
							if (allowLogging)
							{
								logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The addons\\Collections.xml file has an invalid root node");
							}
							Copy = "The addons\\Collections.xml has an invalid root node, [" + LocalCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
							//Copy = "The LocalCollections file [" & App.Path & "\Addons\Collections.xml] has an invalid root node, [" & LocalCollections.DocumentElement.name & "] was received and [" & CollectionListRootNode & "] was expected."
							logController.appendInstallLog(cpCore, Copy);
							return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
							returnOk = false;
						}
						else
						{
							//
							// Get a list of the collection guids on this server
							//

							GuidCnt = 0;
							if (genericController.vbLCase(LocalCollections.DocumentElement.Name) == "collectionlist")
							{
								foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes)
								{
									switch (genericController.vbLCase(LocalListNode.Name))
									{
										case "collection":
											foreach (XmlNode CollectionNode in LocalListNode.ChildNodes)
											{
												if (genericController.vbLCase(CollectionNode.Name) == "guid")
												{
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
							if (allowLogging)
							{
								logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), collection.xml file has " + GuidCnt + " collection nodes.");
							}
							if (GuidCnt > 0)
							{
								//
								// Request collection updates 10 at a time
								//
								GuidPtr = 0;
								while (GuidPtr < GuidCnt)
								{
									RequestPtr = 0;
									GuidList = "";
									while ((GuidPtr < GuidCnt) && RequestPtr < 10)
									{
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
									if (!string.IsNullOrEmpty(GuidList))
									{
										GuidList = GuidList.Substring(1);
										//
										//-----------------------------------------------------------------------------------------------
										//   Load LibraryCollections from the Support Site
										//-----------------------------------------------------------------------------------------------
										//
										if (allowLogging)
										{
											logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), requesting Library updates for [" + GuidList + "]");
										}
										//hint = "Getting CollectionList"
										LibraryCollections = new XmlDocument();
										SupportURL = "http://support.contensive.com/GetCollectionList?iv=" + cpCore.codeVersion() + "&guidlist=" + EncodeRequestVariable(GuidList);
										bool loadOK = true;
										try
										{
											LibraryCollections.Load(SupportURL);
										}
										catch (Exception ex)
										{
											if (allowLogging)
											{
												logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Error downloading or loading GetCollectionList from Support.");
											}
											Copy = "Error downloading or loading GetCollectionList from Support.";
											logController.appendInstallLog(cpCore, Copy + ", the request was [" + SupportURL + "]");
											return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
											returnOk = false;
											loadOK = false;
										}
										if (loadOK)
										{
											if (true)
											{
												if (genericController.vbLCase(LibraryCollections.DocumentElement.Name) != genericController.vbLCase(CollectionListRootNode))
												{
													Copy = "The GetCollectionList support site remote method returned an xml file with an invalid root node, [" + LibraryCollections.DocumentElement.Name + "] was received and [" + CollectionListRootNode + "] was expected.";
													if (allowLogging)
													{
														logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), " + Copy);
													}
													logController.appendInstallLog(cpCore, Copy + ", the request was [" + SupportURL + "]");
													return_ErrorMessage = return_ErrorMessage + "<P>" + Copy + "</P>";
													returnOk = false;
												}
												else
												{
													if (genericController.vbLCase(LocalCollections.DocumentElement.Name) != "collectionlist")
													{
														logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), The Library response did not have a collectioinlist top node, the request was [" + SupportURL + "]");
													}
													else
													{
														//
														//-----------------------------------------------------------------------------------------------
														// Search for Collection Updates Needed
														//-----------------------------------------------------------------------------------------------
														//
														foreach (XmlNode LocalListNode in LocalCollections.DocumentElement.ChildNodes)
														{
															localCollectionUpToDate = false;
															if (allowLogging)
															{
																logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Process local collection.xml node [" + LocalListNode.Name + "]");
															}
															switch (genericController.vbLCase(LocalListNode.Name))
															{
																case "collection":
																	LocalGuid = "";
																	LocalLastChangeDateStr = "";
																	LocalLastChangeDate = DateTime.MinValue;
																	LocalLastChangeNode = null;
																	foreach (XmlNode CollectionNode in LocalListNode.ChildNodes)
																	{
																		switch (genericController.vbLCase(CollectionNode.Name))
																		{
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
																	if (!string.IsNullOrEmpty(LocalGuid))
																	{
																		if (!DateHelper.IsDate(LocalLastChangeDateStr))
																		{
																			LocalLastChangeDate = DateTime.MinValue;
																		}
																		else
																		{
																			LocalLastChangeDate = genericController.EncodeDate(LocalLastChangeDateStr);
																		}
																	}
																	if (allowLogging)
																	{
																		logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), node is collection, LocalGuid [" + LocalGuid + "], LocalLastChangeDateStr [" + LocalLastChangeDateStr + "]");
																	}
																	//
																	// go through each collection on the Library and find the local collection guid
																	//
																	foreach (XmlNode LibListNode in LibraryCollections.DocumentElement.ChildNodes)
																	{
																		if (localCollectionUpToDate)
																		{
																			break;
																		}
																		switch (genericController.vbLCase(LibListNode.Name))
																		{
																			case "collection":
																				LibGUID = "";
																				LibLastChangeDateStr = "";
																				LibLastChangeDate = DateTime.MinValue;
																				foreach (XmlNode CollectionNode in LibListNode.ChildNodes)
																				{
																					switch (genericController.vbLCase(CollectionNode.Name))
																					{
																						case "name":
																							//
																							LibName = genericController.vbLCase(CollectionNode.InnerText);
																							break;
																						case "system":
																							//
																							LibSystem = genericController.EncodeBoolean(CollectionNode.InnerText);
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
																							LibLastChangeDateStr = LibLastChangeDateStr;
																							break;
																						case "contensiveversion":
																							//
																							LibContensiveVersion = CollectionNode.InnerText;
																							break;
																					}
																				}
																				if (!string.IsNullOrEmpty(LibGUID))
																				{
																					if (genericController.vbInstr(1, LibGUID, "58c9", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
																					{
																						LibGUID = LibGUID;
																					}
																					if ((!string.IsNullOrEmpty(LibGUID)) & (LibGUID == LocalGuid) & ((string.IsNullOrEmpty(LibContensiveVersion)) || (string.CompareOrdinal(LibContensiveVersion, cpCore.codeVersion()) <= 0)))
																					{
																						//
																						// LibCollection matches the LocalCollection - process the upgrade
																						//
																						if (allowLogging)
																						{
																							logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), Library collection node found that matches");
																						}
																						if (genericController.vbInstr(1, LibGUID, "58c9", Microsoft.VisualBasic.Constants.vbTextCompare) != 0)
																						{
																							LibGUID = LibGUID;
																						}
																						if (!DateHelper.IsDate(LibLastChangeDateStr))
																						{
																							LibLastChangeDate = DateTime.MinValue;
																						}
																						else
																						{
																							LibLastChangeDate = genericController.EncodeDate(LibLastChangeDateStr);
																						}
																						// TestPoint 1.1 - Test each collection for upgrade
																						if (LibLastChangeDate > LocalLastChangeDate)
																						{
																							//
																							// LibLastChangeDate <>0, and it is > local lastchangedate
																							//
																							workingPath = cpCore.addon.getPrivateFilesAddonPath() + "\\temp_" + genericController.GetRandomInteger() + "\\";
																							if (allowLogging)
																							{
																								logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), matching library collection is newer, start upgrade [" + workingPath + "].");
																							}
																							logController.appendInstallLog(cpCore, "Upgrading Collection [" + LibGUID + "], Library name [" + LibName + "], because LocalChangeDate [" + LocalLastChangeDate + "] < LibraryChangeDate [" + LibLastChangeDate + "]");
																							//
																							// Upgrade Needed
																							//
																							cpCore.privateFiles.createPath(workingPath);
																							//
																							returnOk = DownloadCollectionFiles(cpCore, workingPath, LibGUID, ref CollectionLastChangeDate, ref return_ErrorMessage);
																							if (allowLogging)
																							{
																								logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), DownloadCollectionFiles returned " + returnOk);
																							}
																							if (returnOk)
																							{
																								List<string> listGuidList = new List<string>();
																								returnOk = BuildLocalCollectionReposFromFolder(cpCore, workingPath, CollectionLastChangeDate, ref listGuidList, ref return_ErrorMessage, allowLogging);
																								if (allowLogging)
																								{
																									logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), BuildLocalCollectionFolder returned " + returnOk);
																								}
																							}
																							//
																							if (allowLogging)
																							{
																								logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), working folder not deleted because debugging. Delete tmp folders when finished.");
																							}
																							else
																							{
																								cpCore.privateFiles.DeleteFileFolder(workingPath);
																							}
																							//
																							// Upgrade the apps from the collection files, do not install on any apps
																							//
																							if (returnOk)
																							{
																								returnOk = installCollectionFromLocalRepo(cpCore, LibGUID, cpCore.siteProperties.dataBuildVersion, return_ErrorMessage, "", IsNewBuild, nonCriticalErrorList);
																								if (allowLogging)
																								{
																									logController.appendLog(cpCore, "UpgradeAllLocalCollectionsFromLib3(), UpgradeAllAppsFromLocalCollection returned " + returnOk);
																								}
																							}
																							//
																							// make sure this issue is logged and clear the flag to let other local collections install
																							//
																							if (!returnOk)
																							{
																								if (allowLogging)
																								{
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
																						if (!returnOk)
																						{
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
			}
			catch (Exception ex)
			{
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
		public static bool BuildLocalCollectionReposFromFolder(coreClass cpCore, string sourcePrivateFolderPath, DateTime CollectionLastChangeDate, ref List<string> return_CollectionGUIDList, ref string return_ErrorMessage, bool allowLogging)
		{
			bool success = false;
			try
			{
				if (cpCore.privateFiles.pathExists(sourcePrivateFolderPath))
				{
					logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" + sourcePrivateFolderPath + "]");
					IO.FileInfo[] SrcFileNamelist = cpCore.privateFiles.getFileList(sourcePrivateFolderPath);
					foreach (IO.FileInfo file in SrcFileNamelist)
					{
						if ((file.Extension == ".zip") || (file.Extension == ".xml"))
						{
							string collectionGuid = "";
							success = BuildLocalCollectionRepoFromFile(cpCore, sourcePrivateFolderPath + file.Name, CollectionLastChangeDate, ref collectionGuid, ref return_ErrorMessage, allowLogging);
							return_CollectionGUIDList.Add(collectionGuid);
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return success;
		}
		//
		//
		//
		public static bool BuildLocalCollectionRepoFromFile(coreClass cpCore, string collectionPathFilename, DateTime CollectionLastChangeDate, ref string return_CollectionGUID, ref string return_ErrorMessage, bool allowLogging)
		{
			bool tempBuildLocalCollectionRepoFromFile = false;
			bool result = true;
			try
			{
				string ResourceType = null;
				string CollectionVersionFolderName = string.Empty;
				DateTime ChildCollectionLastChangeDate = default(DateTime);
				string ChildWorkingPath = null;
				string ChildCollectionGUID = null;
				string ChildCollectionName = null;
				bool Found = false;
				XmlDocument CollectionFile = new XmlDocument();
				bool UpdatingCollection = false;
				string Collectionname = string.Empty;
				DateTime NowTime = default(DateTime);
				int NowPart = 0;
				IO.FileInfo[] SrcFileNamelist = null;
				string TimeStamp = null;
				int Pos = 0;
				string CollectionFolder = null;
				string CollectionGuid = string.Empty;
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
				xmlController XMLTools = new xmlController(cpCore);
				string CollectionFolderName = "";
				bool CollectionFileFound = false;
				bool ZipFileFound = false;
				string collectionPath = "";
				string collectionFilename = "";
				//
				// process all xml files in this workingfolder
				//
				if (allowLogging)
				{
					logController.appendLog(cpCore, "BuildLocalCollectionFolder(), Enter");
				}
				//
				cpCore.privateFiles.splitPathFilename(collectionPathFilename, collectionPath, collectionFilename);
				if (!cpCore.privateFiles.pathExists(collectionPath))
				{
					//
					// The working folder is not there
					//
					result = false;
					return_ErrorMessage = "<p>There was a problem with the installation. The installation folder is not valid.</p>";
					if (allowLogging)
					{
						logController.appendLog(cpCore, "BuildLocalCollectionFolder(), " + return_ErrorMessage);
					}
					logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, CheckFileFolder was false for the private folder [" + collectionPath + "]");
				}
				else
				{
					logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files in private folder [" + collectionPath + "]");
					//
					// move collection file to a temp directory
					//
					string tmpInstallPath = "tmpInstallCollection" + genericController.createGuid().Replace("{", "").Replace("}", "").Replace("-", "") + "\\";
					cpCore.privateFiles.copyFile(collectionPathFilename, tmpInstallPath + collectionFilename);
					if (collectionFilename.ToLower().Substring(collectionFilename.Length - 4) == ".zip")
					{
						cpCore.privateFiles.UnzipFile(tmpInstallPath + collectionFilename);
						cpCore.privateFiles.deleteFile(tmpInstallPath + collectionFilename);
					}
					//
					// install the individual files
					//
					SrcFileNamelist = cpCore.privateFiles.getFileList(tmpInstallPath);
					if (true)
					{
						//
						// Process all non-zip files
						//
						foreach (IO.FileInfo file in SrcFileNamelist)
						{
							Filename = file.Name;
							logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing files, filename=[" + Filename + "]");
							if (genericController.vbLCase(Filename.Substring(Filename.Length - 4)) == ".xml")
							{
								//
								logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, processing xml file [" + Filename + "]");
								//hint = hint & ",320"
								CollectionFile = new XmlDocument();

								object loadOk = true;
								try
								{
									CollectionFile.LoadXml(cpCore.privateFiles.readFile(tmpInstallPath + Filename));
								}
								catch (Exception ex)
								{
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
								if (loadOk)
								{
									//hint = hint & ",400"
									CollectionFileBaseName = genericController.vbLCase(CollectionFile.DocumentElement.Name);
									if ((CollectionFileBaseName != "contensivecdef") & (CollectionFileBaseName != CollectionFileRootNode) & (CollectionFileBaseName != genericController.vbLCase(CollectionFileRootNodeOld)))
									{
										//
										// Not a problem, this is just not a collection file
										//
										logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, xml base name wrong [" + CollectionFileBaseName + "]");
									}
									else
									{
										//
										// Collection File
										//
										//hint = hint & ",420"
										Collectionname = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "name", "");
										if (string.IsNullOrEmpty(Collectionname))
										{
											//
											// ----- Error condition -- it must have a collection name
											//
											result = false;
											return_ErrorMessage = "<p>There was a problem with this Collection. The collection file does not have a collection name.</p>";
											logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, collection has no name");
										}
										else
										{
											//
											//------------------------------------------------------------------
											// Build Collection folder structure in /Add-ons folder
											//------------------------------------------------------------------
											//
											//hint = hint & ",440"
											CollectionFileFound = true;
											CollectionGuid = GetXMLAttribute(cpCore, IsFound, CollectionFile.DocumentElement, "guid", Collectionname);
											if (string.IsNullOrEmpty(CollectionGuid))
											{
												//
												// I hope I do not regret this
												//
												CollectionGuid = Collectionname;
											}

											CollectionVersionFolderName = GetCollectionPath(cpCore, CollectionGuid);
											if (!string.IsNullOrEmpty(CollectionVersionFolderName))
											{
												//
												// This is an upgrade
												//
												//hint = hint & ",450"
												UpdatingCollection = true;
												Pos = genericController.vbInstr(1, CollectionVersionFolderName, "\\");
												if (Pos > 0)
												{
													CollectionFolderName = CollectionVersionFolderName.Substring(0, Pos - 1);
												}
											}
											else
											{
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
											if (!cpCore.privateFiles.pathExists(CollectionFolder))
											{
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
											if (NowPart < 10)
											{
												TimeStamp += "0";
											}
											TimeStamp += NowPart.ToString();
											NowPart = NowTime.Day;
											if (NowPart < 10)
											{
												TimeStamp += "0";
											}
											TimeStamp += NowPart.ToString();
											NowPart = NowTime.Hour;
											if (NowPart < 10)
											{
												TimeStamp += "0";
											}
											TimeStamp += NowPart.ToString();
											NowPart = NowTime.Minute;
											if (NowPart < 10)
											{
												TimeStamp += "0";
											}
											TimeStamp += NowPart.ToString();
											NowPart = NowTime.Second;
											if (NowPart < 10)
											{
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
											foreach (XmlNode CDefSection in CollectionFile.DocumentElement.ChildNodes)
											{
												switch (genericController.vbLCase(CDefSection.Name))
												{
													case "resource":
														//'
														//' resource node, if executable node, save to RegisterList
														//'
														//'hint = hint & ",510"
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
														//'
														//' Compatibility only - this is deprecated - Install ActiveX found in Add-ons
														//'
														//'hint = hint & ",530"
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
														if (string.IsNullOrEmpty(ChildCollectionGUID))
														{
															ChildCollectionGUID = CDefSection.InnerText;
														}
														string statusMsg = "Installing collection [" + ChildCollectionName + ", " + ChildCollectionGUID + "] referenced from collection [" + Collectionname + "]";
														logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, getCollection or importcollection, childCollectionName [" + ChildCollectionName + "], childCollectionGuid [" + ChildCollectionGUID + "]");
														if (genericController.vbInstr(1, CollectionVersionPath, ChildCollectionGUID, Microsoft.VisualBasic.Constants.vbTextCompare) == 0)
														{
															if (string.IsNullOrEmpty(ChildCollectionGUID))
															{
																//
																// -- Needs a GUID to install
																result = false;
																return_ErrorMessage = statusMsg + ". The installation can not continue because an imported collection could not be downloaded because it does not include a valid GUID.";
																logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, return message [" + return_ErrorMessage + "]");
															}
															else if (GetCollectionPath(cpCore, ChildCollectionGUID) == "")
															{
																logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], not found so needs to be installed");
																//
																// If it is not already installed, download and install it also
																//
																ChildWorkingPath = CollectionVersionPath + "\\" + ChildCollectionGUID + "\\";
																//
																// down an imported collection file
																//
																StatusOK = DownloadCollectionFiles(cpCore, ChildWorkingPath, ChildCollectionGUID, ref ChildCollectionLastChangeDate, ref return_ErrorMessage);
																if (!StatusOK)
																{

																	logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + statusMsg + "], downloadCollectionFiles returned error state, message [" + return_ErrorMessage + "]");
																	if (string.IsNullOrEmpty(return_ErrorMessage))
																	{
																		return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error while downloading the necessary collection file, [" + ChildCollectionGUID + "].";
																	}
																	else
																	{
																		return_ErrorMessage = statusMsg + ". The installation can not continue because there was an error while downloading the necessary collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
																	}
																}
																else
																{
																	logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], downloadCollectionFiles returned OK");
																	//
																	// install the downloaded file
																	//
																	List<string> ChildCollectionGUIDList = new List<string>();
																	StatusOK = BuildLocalCollectionReposFromFolder(cpCore, ChildWorkingPath, ChildCollectionLastChangeDate, ref ChildCollectionGUIDList, ref return_ErrorMessage, allowLogging);
																	if (!StatusOK)
																	{
																		logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + statusMsg + "], BuildLocalCollectionFolder returned error state, message [" + return_ErrorMessage + "]");
																		if (string.IsNullOrEmpty(return_ErrorMessage))
																		{
																			return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "].";
																		}
																		else
																		{
																			return_ErrorMessage = statusMsg + ". The installation can not continue because there was an unknown error installing the included collection file, guid [" + ChildCollectionGUID + "]. The error was [" + return_ErrorMessage + "]";
																		}
																	}
																}
																//
																// -- remove child installation working folder
																cpCore.privateFiles.DeleteFileFolder(ChildWorkingPath);
															}
															else
															{
																//
																//
																//
																logController.appendInstallLog(cpCore, "BuildLocalCollectionFolder, [" + ChildCollectionGUID + "], already installed");
															}
														}
														break;
												}
												if (!string.IsNullOrEmpty(return_ErrorMessage))
												{
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
							if (!string.IsNullOrEmpty(return_ErrorMessage))
							{
								//
								// if error, no more files
								//
								result = false;
								break;
							}
						}
						if ((string.IsNullOrEmpty(return_ErrorMessage)) && (!CollectionFileFound))
						{
							//
							// no errors, but the collection file was not found
							//
							if (ZipFileFound)
							{
								//
								// zip file found but did not qualify
								//
								return_ErrorMessage = "<p>There was a problem with the installation. The collection zip file was downloaded, but it did not include a valid collection xml file.</p>";
							}
							else
							{
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
				if (string.IsNullOrEmpty(return_ErrorMessage))
				{
					UpdateConfig(cpCore, Collectionname, CollectionGuid, CollectionLastChangeDate, CollectionVersionFolderName);
				}
				else
				{
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
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return result;
		}


	}
}
