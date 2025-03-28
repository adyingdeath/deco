package com.adyingdeath.deco.datapack;

import java.util.Arrays;
import java.util.Random;

public class DatapackUtil {
    public static String standardizeResourceLocation(String namespace, String path) {
        String[] pathParts = Arrays.stream(path.split("[/\\\\]"))
                .filter(part -> !part.isEmpty())
                .toArray(String[]::new);
        return namespace + ":" + String.join("/", pathParts);
    }
    
    /**
     * Generates a random string of specified length
     * @param length the length of the random string to generate
     * @return a random string of the specified length
     */
    public static String randomCode(int length) {
        if (length <= 0) {
            return "";
        }
        
        String characters = "abcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder();
        Random random = new Random();
        
        for (int i = 0; i < length; i++) {
            int index = random.nextInt(characters.length());
            result.append(characters.charAt(index));
        }
        
        return result.toString();
    }
}
