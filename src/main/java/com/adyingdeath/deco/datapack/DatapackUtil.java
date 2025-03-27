package com.adyingdeath.deco.datapack;

import java.util.Arrays;

public class DatapackUtil {
    public static String standardizeResourceLocation(String namespace, String path) {
        String[] pathParts = Arrays.stream(path.split("[/\\\\]"))
                .filter(part -> !part.isEmpty())
                .toArray(String[]::new);
        return namespace + ":" + String.join("/", pathParts);
    }
}
