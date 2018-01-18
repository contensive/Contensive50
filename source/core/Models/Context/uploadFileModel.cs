
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
namespace Contensive.Core.Models.Context {
    public class uploadFileModel {

        private string FileNameLocal;
        private int FileSizeLocal;
        private byte[] ValueLocal;
        private string ContentTypeLocal;
        //
        //
        //
        internal string Filename {
            get {
                return FileNameLocal;
            }
            set {
                FileNameLocal = value;
            }
        }
        //
        //
        //
        public int FileSize {
            get {
                return FileSizeLocal;
            }
            set {
                FileSizeLocal = value;
            }
        }
        //
        //
        //
        public byte[] Value {
            get {
                return ValueLocal;
            }
            set {
                ValueLocal = value;
            }
        }
        //
        //
        //
        public string ContentType {
            get {
                return ContentTypeLocal;
            }
            set {
                ContentTypeLocal = value;
            }
        }
        //
        //
        //
        public bool IsFile {
            get {
                bool tempIsFile = false;
                tempIsFile = false;
                if (!string.IsNullOrEmpty(FileNameLocal)) {
                    tempIsFile = true;
                }

                return tempIsFile;
            }
        }
    }
}
