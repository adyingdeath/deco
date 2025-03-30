package com.adyingdeath.deco.datapack;

import java.util.Arrays;
import java.util.function.Predicate;
import java.util.stream.Stream;

public class ResourceLocation {
    private String namespace;
    private String path;
    private String name;

    /**
     * Standardizes a resource location string.
     * @param location The resource location string to standardize
     * @return The standardized resource location string, or null if the string is not a valid resource location
     */
    public static String standardize(String location) {
        String[] locationSplit = location.toLowerCase().split(":");
        if (locationSplit.length != 2) {
            return null;
        }
        String[] pathParts = Arrays.stream(locationSplit[1].split("[/\\\\]"))
                .filter(part -> !part.isEmpty())
                .toArray(String[]::new);
        return locationSplit[0] + ":" + String.join("/", pathParts);
    }

    /**
     * Standardizes a resource location string.
     * @param namespace The namespace of the resource location
     * @param path The path of the resource location
     * @return The standardized resource location string
     */
    public static String standardize(String namespace, String path) {
        String[] pathParts = Arrays.stream(path.toLowerCase().split("[/\\\\]"))
                .filter(part -> !part.isEmpty())
                .toArray(String[]::new);
        return namespace.toLowerCase() + ":" + String.join("/", pathParts);
    }

    /**
     * Creates a new resource location from a string.
     * @param location The full location string, e.g. "deco:core/main"
     */
    public ResourceLocation(String location) {
        String[] locationSplit = location.toLowerCase().split(":");
        if (locationSplit.length != 2) {
            return;
        }
        this.namespace = locationSplit[0];

        String[] pathSplit = Stream.of(locationSplit[1].split("[\\\\\\/]"))
            .filter((str) -> !str.isEmpty())
            .map(String::trim)
            .toArray(String[]::new);
        if (pathSplit.length == 1) {
            this.path = "";
            this.name = pathSplit[0];
        } else {
            this.name = pathSplit[pathSplit.length - 1];
            pathSplit = Arrays.copyOf(pathSplit, pathSplit.length - 1);
            this.path = String.join("/", pathSplit);
        }
    }

    /**
     * Creates a new resource location from a namespace and a path with name.
     * Will standardize the path to use forward slashes, and remove any empty slices.
     * @param namespace The namespace of the resource location
     * @param pathWithName The path with name, e.g. "core/main"
     */
    public ResourceLocation(String namespace, String pathWithName) {
        this.namespace = namespace.toLowerCase();

        // Handle pathWithName
        String[] pathSplit = Stream.of(pathWithName.toLowerCase().split("[\\\\\\/]"))
                .filter((str) -> !str.isEmpty())
                .map(String::trim)
                .toArray(String[]::new);
        if (pathSplit.length == 1) {
            this.path = "";
            this.name = pathSplit[0];
        } else {
            this.name = pathSplit[pathSplit.length - 1];
            pathSplit = Arrays.copyOf(pathSplit, pathSplit.length - 1);
            this.path = String.join("/", pathSplit);
        }
    }

    /**
     * Creates a new resource location from a namespace, path, and name.
     * @param namespace The namespace of the resource location
     * @param path The path of the resource location
     * @param name The name of the resource location
     */
    public ResourceLocation(String namespace, String path, String name) {
        this.namespace = namespace.toLowerCase();
        this.path = path.toLowerCase();
        this.name = name.toLowerCase();
    }

    /**
     * Returns the full location string, e.g. "deco:core/main"
     */
    public String toString() {
        if (this.path.isEmpty()) {
            return this.namespace + ":" + this.name;
        } else {
            return this.namespace + ":" + this.path + "/" + this.name;
        }
    }

    public String getNamespace() {
        return namespace;
    }

    public void setNamespace(String namespace) {
        this.namespace = namespace.toLowerCase();
    }

    public String getPath() {
        return path;
    }

    public void setPath(String path) {
        this.path = path.toLowerCase();
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name.toLowerCase();
    }
}
