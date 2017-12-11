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
    //
    // -- vb Left method added to string
    public static string Left(this string value, int maxLength) {
        if (string.IsNullOrEmpty(value)) return value;
        maxLength = Math.Abs(maxLength);

        return (value.Length <= maxLength
               ? value
               : value.Substring(0, maxLength)
               );
    }
}
