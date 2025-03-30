package com.adyingdeath.deco.datapack;

import java.util.Arrays;
import java.util.function.Predicate;
import java.util.stream.Stream;

public class ResourceLocation {
    private String namespace;
    private String path;
    private String name;

    public static void main(String[] args) {
        ResourceLocation rl = new ResourceLocation("deco:core/main");
        System.out.println(rl);
    }

    /**
     * Creates a new resource location from a string.
     * @param location The full location string, e.g. "deco:core/main"
     */
    public ResourceLocation(String location) {
        String[] locationSplit = location.split(":");
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
        this.namespace = namespace;

        // Handle pathWithName
        String[] pathSplit = Stream.of(pathWithName.split("[\\\\\\/]"))
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
        this.namespace = namespace;
        this.path = path;
        this.name = name;
    }

    /**
     * Returns the full location string, e.g. "deco:core/main"
     */
    public String toString() {
        return this.namespace + ":" + this.path + "/" + this.name;
    }

    public String getNamespace() {
        return namespace;
    }

    public void setNamespace(String namespace) {
        this.namespace = namespace;
    }

    public String getPath() {
        return path;
    }

    public void setPath(String path) {
        this.path = path;
    }

    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
}
