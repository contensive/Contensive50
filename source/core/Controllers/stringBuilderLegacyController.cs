
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
namespace Contensive.Core.Controllers {
    public class stringBuilderLegacyController {
        //
        private int iSize;
        private const int iChunk = 100;
        private int iCount;
        private string[] Holder;
        //
        //==========================================================================================
        /// <summary>
        /// add a string to the stringbuilder
        /// </summary>
        /// <param name="NewString"></param>
        public void Add(string NewString) {
            try {
                if (iCount >= iSize) {
                    iSize = iSize + iChunk;
                    Array.Resize(ref Holder, iSize + 1);
                }
                Holder[iCount] = NewString;
                iCount = iCount + 1;
            } catch (Exception ex) {
                throw new ApplicationException("Exception in coreFastString.Add()", ex);
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// read the string out of the string builder
        /// </summary>
        /// <returns></returns>
        public string Text {
            get {
                return string.Join("", Holder) + "";
            }
        }
    }
}