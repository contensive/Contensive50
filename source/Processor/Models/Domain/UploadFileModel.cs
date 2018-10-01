
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Processor;
using Contensive.Processor.Models.Db;
using Contensive.Processor.Controllers;
using static Contensive.Processor.Controllers.GenericController;
using static Contensive.Processor.constants;
//
namespace Contensive.Processor.Models.Domain {
    public class UploadFileModel {
        //
        internal string filename {
            get {
                return _fileName;
            }
            set {
                _fileName = value;
            }
        } private string _fileName;
        //
        public int fileSize {
            get {
                return _fileSize;
            }
            set {
                _fileSize = value;
            }
        } private int _fileSize;
        //
        public byte[] value {
            get {
                return _value;
            }
            set {
                _value = value;
            }
        } private byte[] _value;
        //
        public string contentType {
            get {
                return _contentType;
            }
            set {
                _contentType = value;
            }
        } private string _contentType;
        //
        public bool isFile {
            get {
                if (!string.IsNullOrEmpty(_fileName)) return true;
                return false;
            }
        }
    }
}
