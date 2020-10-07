
namespace Contensive.BaseModels {
    //
    //====================================================================================================
    /// <summary>
    /// An object that holds an encoded version of a cache key. This object is primarily used to
    /// differentiate between the 'key', a string that describes the data, and what has to be used
    /// within the cache system that includes other attributes.
    /// </summary>
    public abstract class CacheKeyHashBaseModel {
        //
        // ====================================================================================================
        //
        public abstract string hash { get; set; }
    }
}

