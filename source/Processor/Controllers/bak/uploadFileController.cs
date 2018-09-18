

using Models.Context;
using Models.Entity;
using Controllers;

// 

namespace Controllers {
    
    public class uploadFileController {
        
        internal byte[] binaryHeader {
        }
    }
}
return returnForm;
EndGetEndPropertyEndclass End {
}

    
    // 
    //  Get the Count of Form collection
    // 
    public int Count {
        get {
            if ((ItemStorageCollection == null)) {
                Count = 0;
            }
            else {
                Count = ItemCount;
                // Count = ItemStorageCollection.Count
            }
            
        }
    }
    
    public bool FieldExists(string Key) {
        bool result = false;
        try {
            int ItemPointer;
            string UcaseKey;
            // 
            FieldExists = false;
            if (((ItemCount > 0) 
                        && !IsNull(Key))) {
                UcaseKey = genericController.vbUCase(Key);
                for (ItemPointer = 0; (ItemPointer 
                            <= (ItemCount - 1)); ItemPointer++) {
                    if ((ItemNames(ItemPointer) == UcaseKey)) {
                        FieldExists = true;
                        break;
                    }
                    
                }
                
            }
            
        }
        catch (Exception ex) {
            throw;
        }
        
    }
    
    public string Key(int Index) {
        string result = "";
        try {
            if ((Index < ItemCount)) {
                Key = ItemNames(Index);
            }
            
        }
        catch (Exception ex) {
            throw;
        }
        
        return result;
    }