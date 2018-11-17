
namespace Contensive.BaseClasses {
    public abstract class CPUserErrorBaseClass {
        //Public Sub New(ByVal cmcObj As Contensive.Processor.cpCoreClass, ByRef CPParent As CPBaseClass)
        public abstract void Add(string Message); //Implements BaseClasses.CPUserErrorBaseClass.Add
        public abstract string GetList(); //Implements BaseClasses.CPUserErrorBaseClass.GetList
        public abstract bool OK(); //Implements BaseClasses.CPUserErrorBaseClass.OK
        //
        //====================================================================================================
        // deprecated
        //
    }
}

