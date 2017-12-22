using System;
public static class ExtensionMethods {
    //
    // -- example extention method
    public static string UppercaseFirstLetter(this string value) {
        // Uppercase the first letter in the string.
        if (value.Length > 0) {
            char[] array = value.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }
        return value;
    }
    //====================================================================================================
    /// <summary>
    /// like vb Left. Return leftmost characters up to the maxLength (but no error if short)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Left(this string source, int maxLength) {
        if (string.IsNullOrEmpty(source)) {
            return "";
        } else if (maxLength < 0) {
            throw new ArgumentException("length [" + maxLength + "] must be 0+");
        } else if (source.Length <= maxLength) {
            return source;
        } else {
            return source.Substring(0, maxLength);
        }
    }
    //
    //====================================================================================================
    /// <summary>
    /// like vb Right()
    /// </summary>
    /// <param name="source"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    public static string Right(this string source, int maxLength) {
        if (string.IsNullOrEmpty(source)) {
            return "";
        } else if (maxLength < 0) {
            throw new ArgumentException("length [" + maxLength + "] must be 0+");
        } else if (source.Length <= maxLength) {
            return source;
        } else {
            return source.Substring(source.Length-maxLength,maxLength);
        }
    }
    //
    //====================================================================================================
    /// <summary>
    /// replacement for visual basic isNumeric
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static bool IsNumeric(this object expression) {
        try {
            if (expression == null) {
                return false;
            } else if (expression is DateTime) {
                return false;
            } else if ((expression is int) || (expression is Int16) || (expression is Int32) || (expression is Int64) || (expression is decimal) || (expression is float) || (expression is double) || (expression is bool)) {
                return true;
            } else if (expression is string) {
                double output = 0;
                return double.TryParse((string)expression, out output);
            } else {
                return false;
            }
        } catch {
            return false;
        }
    }
    //
    //====================================================================================================
    //
    public static bool isOld( this DateTime srcDate) {
        return (srcDate < new DateTime(1900, 1, 1));
    }
    //
    //====================================================================================================
    /// <summary>
    /// if date is invalid, set to minValue
    /// </summary>
    /// <param name="srcDate"></param>
    /// <returns></returns>
    public static DateTime MinValueIfOld(this DateTime srcDate) {
        DateTime returnDate = srcDate;
        if (srcDate < new DateTime(1900, 1, 1)) {
            returnDate = DateTime.MinValue;
        }
        return returnDate;
    }

}
