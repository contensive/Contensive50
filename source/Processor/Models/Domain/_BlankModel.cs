
using Contensive.Processor.Controllers;
//
namespace Contensive.Processor.Models.Domain {
    [System.Serializable]
    public class _BlankModel {
        private CoreController core;
        //
        //====================================================================================================
        /// <summary>
        /// new
        /// </summary>
        /// <param name="core"></param>
        public _BlankModel(CoreController core) : base() {
            this.core = core;
        }
        //
        //
        //
    }
}