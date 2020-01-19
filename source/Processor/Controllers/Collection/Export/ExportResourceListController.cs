
using System;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;

namespace Contensive.Processor.Controllers {
    public static class ExportResourceListController {
        // 
        // ====================================================================================================
        // 
        public static string getResourceList(CPBaseClass cp, string execFileList, string CollectionGuid, List<string> tempPathFileList, string tempExportPath) {
            try {
                string nodeList = "";
                if (!execFileList.Length.Equals(0)) {
                    DateTime LastChangeDate = default;
                    // 
                    // There are executable files to include in the collection
                    // If installed, source path is collectionpath, if not installed, collectionpath will be empty
                    // and file will be sourced right from addon path
                    // 
                    string CollectionPath = "";
                    ExportController.GetLocalCollectionArgs(cp, CollectionGuid, ref CollectionPath, ref LastChangeDate);
                    if (!CollectionPath.Length.Equals(0)) {
                        CollectionPath += @"\";
                    }
                    string[] Files = Strings.Split(execFileList, "\r\n");
                    for (int Ptr = 0; Ptr <= Information.UBound(Files); Ptr++) {
                        string PathFilename = Files[Ptr];
                        if (!PathFilename.Length.Equals(0)) {
                            PathFilename = Strings.Replace(PathFilename, @"\", "/");
                            string Path = "";
                            string Filename = PathFilename;
                            int Pos = Strings.InStrRev(PathFilename, "/");
                            if (Pos > 0) {
                                Filename = Strings.Mid(PathFilename, Pos + 1);
                                Path = Strings.Mid(PathFilename, 1, Pos - 1);
                            }
                            string ManualFilename = "";
                            if (Strings.LCase(Filename) != Strings.LCase(ManualFilename)) {
                                string AddonPath = @"addons\";
                                // AddFilename = AddonPath & CollectionPath & Filename
                                cp.PrivateFiles.Copy(AddonPath + CollectionPath + Filename, tempExportPath + Filename, cp.TempFiles);
                                if (!tempPathFileList.Contains(tempExportPath + Filename)) {
                                    tempPathFileList.Add(tempExportPath + Filename);
                                    nodeList = nodeList + "\r\n" + "\t" + "<Resource name=\"" + System.Net.WebUtility.HtmlEncode(Filename) + "\" type=\"executable\" path=\"" + System.Net.WebUtility.HtmlEncode(Path) + "\" />";
                                }
                            }
                        }
                    }
                }
                return nodeList;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return string.Empty;
            }
        }
    }
}

