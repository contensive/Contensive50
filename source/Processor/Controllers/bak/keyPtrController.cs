

using Controllers;

// 

namespace Controllers {
    
    // 
    [Serializable()]
    public class keyPtrController {
        
        // 
        [Serializable()]
        public class storageClass {
            
            // 
            public int ArraySize;
            
            public int ArrayCount;
            
            public bool ArrayDirty;
            
            public string[] UcaseKeyArray;
            
            public string[] PointerArray;
            
            public int ArrayPointer;
        }
        
        // 
        private storageClass store = new storageClass();
        
        // 
        // 
        // 
        public string exportPropertyBag() {
            string returnBag = "";
            try {
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                // 
                returnBag = json.Serialize(store);
                // returnBag = Newtonsoft.Json.JsonConvert.SerializeObject(store)
                // Catch ex As Newtonsoft.Json.JsonException
                //     Throw New indexException("ExportPropertyBag JSON error", ex)
            }
            catch (Exception ex) {
                throw new indexException("ExportPropertyBag error", ex);
            }
            
            return returnBag;
        }
        
        // 
        // 
        // 
        public void importPropertyBag(string bag) {
            try {
                System.Web.Script.Serialization.JavaScriptSerializer json = new System.Web.Script.Serialization.JavaScriptSerializer();
                // 
                store = json.Deserialize(Of, storageClass)[bag];
                // store = Newtonsoft.Json.JsonConvert.DeserializeObject(Of storageClass)(bag)
                // Catch ex As Newtonsoft.Json.JsonException
                //     Throw New indexException("ImportPropertyBag JSON error", ex)
            }
            catch (Exception ex) {
                throw new indexException("ImportPropertyBag error", ex);
            }
            
        }
        
        // 
        // ========================================================================
        //    Returns a pointer into the index for this Key
        //    Used only by GetIndexValue and setIndexValue
        //    Returns -1 if there is no match
        // ========================================================================
        // 
        private int GetArrayPointer(string Key) {
            int ArrayPointer = -1;
            try {
                string UcaseTargetKey;
                // Dim ElementKey As String
                int HighGuess;
                int LowGuess;
                int PointerGuess;
                string test;
                test = "";
                if (store.ArrayDirty) {
                    Sort();
                }
                
                // 
                ArrayPointer = -1;
                if ((store.ArrayCount > 0)) {
                    UcaseTargetKey = genericController.vbReplace(Key.ToUpper(), "\r\n", "");
                    LowGuess = -1;
                    HighGuess = (store.ArrayCount - 1);
                    while (((HighGuess - LowGuess) 
                                > 1)) {
                        //  20150823 jk added to prevent implicit conversion
                        PointerGuess = int.Parse(Int(((HighGuess + LowGuess) 
                                            / 2)));
                        if ((UcaseTargetKey == store.UcaseKeyArray(PointerGuess))) {
                            HighGuess = PointerGuess;
                            break; //Warning!!! Review that break works as 'Exit Do' as it could be in a nested instruction like switch
                        }
                        else if ((UcaseTargetKey < store.UcaseKeyArray(PointerGuess))) {
                            HighGuess = PointerGuess;
                        }
                        else {
                            LowGuess = PointerGuess;
                        }
                        
                    }
                    
                    if ((UcaseTargetKey == store.UcaseKeyArray(HighGuess))) {
                        ArrayPointer = HighGuess;
                    }
                    
                }
                
            }
            catch (Exception ex) {
                throw new indexException("getArrayPointer error", ex);
            }
            
            return ArrayPointer;
        }
        
        // 
        // ========================================================================
        //    Returns the matching pointer from a ContentIndex
        //    Returns -1 if there is no match
        // ========================================================================
        // 
        public int getPtr(string Key) {
            int returnKey = -1;
            try {
                string test;
                bool MatchFound;
                string UcaseKey;
                test = "";
                UcaseKey = genericController.vbReplace(Key.ToUpper(), "\r\n", "");
                // UcaseKey = genericController.vbUCase(Key)
                store.ArrayPointer = this.GetArrayPointer(Key);
                if ((store.ArrayPointer > -1)) {
                    //  Make sure this is the first match
                    MatchFound = true;
                    while (MatchFound) {
                        store.ArrayPointer = (store.ArrayPointer - 1);
                        if ((store.ArrayPointer < 0)) {
                            MatchFound = false;
                        }
                        else {
                            MatchFound = (store.UcaseKeyArray(store.ArrayPointer) == UcaseKey);
                        }
                        
                    }
                    
                    store.ArrayPointer = (store.ArrayPointer + 1);
                    returnKey = genericController.EncodeInteger(store.PointerArray(store.ArrayPointer));
                }
                
            }
            catch (Exception ex) {
                throw new indexException("GetPointer error", ex);
            }
            
            return returnKey;
        }
        
        // 
        // ========================================================================
        //    Add an element to an ContentIndex
        // 
        //    if the entry is a duplicate, it is added anyway
        // ========================================================================
        // 
        public void setPtr(string Key, int Pointer) {
            try {
                string keyToSave;
                // 
                keyToSave = genericController.vbReplace(Key.ToUpper(), "\r\n", "");
                // 
                if ((store.ArrayCount >= store.ArraySize)) {
                    store.ArraySize = (store.ArraySize + KeyPointerArrayChunk);
                    object Preserve;
                    store.PointerArray(store.ArraySize);
                    object Preserve;
                    store.UcaseKeyArray(store.ArraySize);
                }
                
                store.ArrayPointer = store.ArrayCount;
                store.ArrayCount = (store.ArrayCount + 1);
                store.UcaseKeyArray(store.ArrayPointer) = keyToSave;
                store.PointerArray(store.ArrayPointer) = Pointer.ToString();
                store.ArrayDirty = true;
            }
            catch (Exception ex) {
                throw new indexException("SetPointer error", ex);
            }
            
        }
        
        // 
        // ========================================================================
        //    Returns the next matching pointer from a ContentIndex
        //    Returns -1 if there is no match
        // ========================================================================
        // 
        public int getNextPtrMatch(string Key) {
            int nextPointerMatch = -1;
            try {
                string UcaseKey;
                // 
                if ((store.ArrayPointer 
                            < (store.ArrayCount - 1))) {
                    store.ArrayPointer = (store.ArrayPointer + 1);
                    UcaseKey = genericController.vbUCase(Key);
                    if ((store.UcaseKeyArray(store.ArrayPointer) == UcaseKey)) {
                        nextPointerMatch = genericController.EncodeInteger(store.PointerArray(store.ArrayPointer));
                    }
                    else {
                        store.ArrayPointer = (store.ArrayPointer - 1);
                    }
                    
                }
                
            }
            catch (Exception ex) {
                throw new indexException("GetNextPointerMatch error", ex);
            }
            
            return nextPointerMatch;
        }
        
        // 
        // ========================================================================
        //    Returns the first Pointer in the current index
        //    returns empty if there are no Pointers indexed
        // ========================================================================
        // 
        public int getFirstPtr() {
            int firstPointer = -1;
            try {
                if (store.ArrayDirty) {
                    Sort();
                }
                
                // 
                //  GetFirstPointer = -1
                if ((store.ArrayCount > 0)) {
                    store.ArrayPointer = 0;
                    firstPointer = genericController.EncodeInteger(store.PointerArray(store.ArrayPointer));
                }
                
                // 
            }
            catch (Exception ex) {
                throw new indexException("GetFirstPointer error", ex);
            }
            
            return firstPointer;
        }
        
        // 
        // ========================================================================
        //    Returns the next Pointer, past the last one returned
        //    Returns empty if the index is at the end
        // ========================================================================
        // 
        public int getNextPtr() {
            int nextPointer = -1;
            try {
                if (store.ArrayDirty) {
                    Sort();
                }
                
                // 
                // nextPointer = -1
                if (((store.ArrayPointer + 1) 
                            < store.ArrayCount)) {
                    store.ArrayPointer = (store.ArrayPointer + 1);
                    nextPointer = genericController.EncodeInteger(store.PointerArray(store.ArrayPointer));
                }
                
            }
            catch (Exception ex) {
                throw new indexException("GetPointer error", ex);
            }
            
            return nextPointer;
        }
        
        // 
        // ========================================================================
        // 
        // ========================================================================
        // 
        private void BubbleSort() {
            try {
                string TempUcaseKey;
                string tempPtrString;
                // Dim TempPointer as integer
                bool CleanPass;
                int MaxPointer;
                int SlowPointer;
                int FastPointer;
                string test;
                int PointerDelta;
                test = "";
                if ((store.ArrayCount > 1)) {
                    PointerDelta = 1;
                    MaxPointer = (store.ArrayCount - 2);
                    for (SlowPointer = MaxPointer; (SlowPointer <= 0); SlowPointer = (SlowPointer + -1)) {
                        CleanPass = true;
                        for (FastPointer = MaxPointer; (FastPointer 
                                    <= (MaxPointer - SlowPointer)); FastPointer = (FastPointer + -1)) {
                            if ((store.UcaseKeyArray(FastPointer) > store.UcaseKeyArray((FastPointer + PointerDelta)))) {
                                TempUcaseKey = store.UcaseKeyArray((FastPointer + PointerDelta));
                                tempPtrString = store.PointerArray((FastPointer + PointerDelta));
                                store.UcaseKeyArray((FastPointer + PointerDelta)) = store.UcaseKeyArray(FastPointer);
                                store.PointerArray((FastPointer + PointerDelta)) = store.PointerArray(FastPointer);
                                store.UcaseKeyArray(FastPointer) = TempUcaseKey;
                                store.PointerArray(FastPointer) = tempPtrString;
                                CleanPass = false;
                            }
                            
                        }
                        
                        if (CleanPass) {
                            break;
                        }
                        
                    }
                    
                }
                
                store.ArrayDirty = false;
            }
            catch (Exception ex) {
                throw new indexException("BubbleSort error", ex);
            }
            
        }
        
        // 
        // ========================================================================
        // 
        //  Made by Michael Ciurescu (CVMichael from vbforums.com)
        //  Original thread: http://www.vbforums.com/showthread.php?t=231925
        // 
        // ========================================================================
        // 
        private void QuickSort() {
            try {
                if ((store.ArrayCount >= 2)) {
                    this.QuickSort_Segment(store.UcaseKeyArray, store.PointerArray, 0, (store.ArrayCount - 1));
                }
                
            }
            catch (Exception ex) {
                throw new indexException("QuickSort error", ex);
            }
            
        }
        
        // 
        // 
        // ========================================================================
        // 
        //  Made by Michael Ciurescu (CVMichael from vbforums.com)
        //  Original thread: http://www.vbforums.com/showthread.php?t=231925
        // 
        // ========================================================================
        // 
        private void QuickSort_Segment(string[] C, string[] P, int First, int Last) {
            try {
                int Low;
                int High;
                string MidValue;
                string TC;
                string TP;
                // 
                Low = First;
                High = Last;
                MidValue = C((First + Last), 2);
                // 
                while ((C(Low) < MidValue)) {
                    Low = (Low + 1);
                    while () {
                        while ((C(High) > MidValue)) {
                            High = (High - 1);
                        }
                        
                        if ((Low <= High)) {
                            TC = C(Low);
                            TP = P(Low);
                            C(Low) = C(High);
                            P(Low) = P(High);
                            C(High) = TC;
                            P(High) = TP;
                            Low = (Low + 1);
                            High = (High - 1);
                        }
                        
                        while ((Low <= High)) {
                            if ((First < High)) {
                                this.QuickSort_Segment(C, P, First, High);
                            }
                            
                            if ((Low < Last)) {
                                this.QuickSort_Segment(C, P, Low, Last);
                            }
                            
                            ((Exception)(ex));
                            throw new indexException("QuickSort_Segment error", ex);
                        }
                        
                    }
                    
                    // 
                    // 
                    // 
                    Sort();
                    try {
                        this.QuickSort();
                        store.ArrayDirty = false;
                    }
                    catch (Exception ex) {
                        throw new indexException("Sort error", ex);
                    }
                    
                    // 
                    // 
                    // 
                    indexException;
                    System.Exception;
                    System.Runtime.Serialization.ISerializable;
                    // 
                    // Private _message As String
                    // 
                    base.New();
                    //  Add implementation.
                    ((string)(message));
                    base.New(message);
                    //  Add implementation.
                    ((string)(message));
                    ((Exception)(inner));
                    base.New(message, inner);
                    //  Add implementation.
                    //  This constructor is needed for serialization.
                    ((System.Runtime.Serialization.SerializationInfo)(info));
                    ((System.Runtime.Serialization.StreamingContext)(context));
                    base.New(info, context);
                    //  Add implementation.
                }
                
            }
            
        }
    }
}