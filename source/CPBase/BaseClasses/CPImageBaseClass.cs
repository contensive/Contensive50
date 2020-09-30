
using System;
using System.Collections.Generic;

namespace Contensive.BaseClasses {
    public abstract class CPImageBaseClass {
        //
        //==========================================================================================
        /// <summary>
        /// Return an image resized and cropped to best fit the hole. New image is saved back to the same path. 
        /// Test if in cache first, else load record and test for resized in alt list, else Resize the image and save back to the image's record
        /// </summary>
        /// <param name="imagePathFilename"></param>
        /// <param name="holeWidth"></param>
        /// <param name="holeHeight"></param>
        /// <param name="imageAltSizeList"></param>
        public abstract string GetBestFit(string imagePathFilename, int holeWidth, int holeHeight, List<string> imageAltSizeList);
        //
        //====================================================================================================
        // deprecated
        //
    }
}

