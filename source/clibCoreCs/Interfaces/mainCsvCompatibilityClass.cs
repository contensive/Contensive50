
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
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
// 
namespace Contensive.Core {
    public class mainCsvScriptCompatibilityClass {
        public const string ClassId = "D9099AAE-3FCB-4398-B94C-19EE7FA97B2B";
        public const string InterfaceId = "CE342EA5-339F-4C31-9F90-F878F527E17A";
        public const string EventsId = "21D9D0FB-9B5B-43C2-A7A5-3C84ABFAF90A";
        // 
        private coreClass cpCore;
        public mainCsvScriptCompatibilityClass(coreClass cpCore) {
            this.cpCore = cpCore;
        }
        
        // 
        // ====================================================================================================
        // 
        public string EncodeContent9(
                    object Source, 
                    int i0, 
                    string s2, 
                    int i3, 
                    int i4, 
                    bool b5, 
                    bool b6, 
                    bool b7, 
                    bool b8, 
                    bool b9, 
                    bool b10, 
                    string s11, 
                    string s12, 
                    bool b13, 
                    int i14, 
                    string s15, 
                    int i16) {
            return cpCore.html.convertActiveContentToHtmlForWysiwygEditor(Controllers.genericController.encodeText(Source));
        }
    }
}