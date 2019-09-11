
using System.Collections.Generic;

namespace Contensive.Models {
    //
    // ====================================================================================================
    /// <summary>
    /// Model to create name=value lists
    /// </summary>
    public class SqlFieldListClass {
        /// <summary>
        /// store
        /// </summary>
        private List<NameValueModel> _sqlList = new List<NameValueModel>();
        /// <summary>
        /// add a name=value pair to the list. values MUST be punctuated correctly for their type in sql (quoted text, etc)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void add(string name, string value) {
            _sqlList.Add(new NameValueModel() {
                name = name,
                value = value
            });
        }
        /// <summary>
        /// get name=value list
        /// </summary>
        /// <returns></returns>
        public string getNameValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name + "=" + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get list of names
        /// </summary>
        /// <returns></returns>
        public string getNameList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.name;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get list of values
        /// </summary>
        /// <returns></returns>
        public string getValueList() {
            string returnPairs = "";
            string delim = "";
            foreach (var nameValue in _sqlList) {
                returnPairs += delim + nameValue.value;
                delim = ",";
            }
            return returnPairs;
        }
        /// <summary>
        /// get count of name=value pairs
        /// </summary>
        public int count {
            get {
                return _sqlList.Count;
            }
        }

    }
}
