package com.adyingdeath.deco.compile;

public class DecoUtil {
    /**
     * Process string in Deco language. Remove "" and '' and process escape characters.
     * @param str The string to process
     * @return The processed string
     */
    public static String processString(String str) {
        // Remove "" and '' on both sides of the string
        str = str.substring(1, str.length() - 1);
        // ToDo: Process escape characters
        //str = str.replace("\\n", "\n").replace("\\t", "\t").replace("\\r", "\r").replace("\\f", "\f").replace("\\b", "\b").replace("\\'", "\"").replace("\\\\", "\\");
        return str;
    }
}
