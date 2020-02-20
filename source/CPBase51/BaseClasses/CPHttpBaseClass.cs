

using Contensive.BaseModels;
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    /// <summary>
    /// Helper methods to perform common http requests
    /// </summary>
    public abstract class CPHttpBaseClass {
        //
        // ====================================================================================================
        /// <summary>
        /// Get url to a string. Use file objects to save to file
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public abstract string Get(string url);
        //
        // ====================================================================================================
        /// <summary>
        /// Post key/values to  a url. Use file objects to save to file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="requestArguments"></param>
        /// <returns></returns>
        public abstract string Post(string url, List<KeyValuePair<string, string>> requestArguments);
        //
        // ====================================================================================================
        /// <summary>
        /// post entity to a url. Use file objects to save to file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract string Post(string url, string entity);
    }
}

