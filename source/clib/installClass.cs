
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Contensive.Core;
//
namespace  Contensive.Core {
    static class installClass {
        //
        //====================================================================================================
        /// <summary>
        /// verify the config.json file, and set the programFiles property to the current assemblies running folder.
        /// </summary>
        public static  void verifyInstall() {
            try
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: [" + ex.ToString() + "]");
            }
        }
    }
}
