
//
using Contensive.Processor.Controllers;

namespace Contensive.Processor.Models.Domain {
    //
    //====================================================================================================
    //
    public class DocPropertiesModel {
        public string Name;
        public string Value;
        public string NameValue;
        public string tempfilename;
        public int FileSize;
        public string fileType;
        public DocPropertyTypesEnum propertyType;
        //
        public enum DocPropertyTypesEnum {
            serverVariable = 1,
            header = 2,
            form = 3,
            file = 4,
            queryString = 5,
            userDefined = 6
        }
    }
}