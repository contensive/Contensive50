
using Contensive.Processor;
//
namespace Contensive.Processor.Addons.Personalization {
    //
    public class LastNameClass : Contensive.BaseClasses.AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(Contensive.BaseClasses.CPBaseClass cpBase) {
            return ((CPClass)cpBase).core.session.user.lastName;
        }
    }
}
