package com.adyingdeath.deco.compile;

public class DecoFile {
    /**
     * The namespace of the file
     */
    private String namespace;
    /**
     * The path of the file relative to the namespace folder
     */
    private String path;
    /**
     * The filename of the file (without the path and extension)
     */
    private String filename;
    /**
     * The content of the file
     */
    private String content;

    /**
     * Constructor for DecoFile
     * 
     * @param namespace The namespace of the file
     * @param path The path of the file relative to the namespace folder
     * @param filename The filename of the file (without the path and extension)
     */
    public DecoFile(String namespace, String path, String filename) {
        this.namespace = namespace;
        this.path = path;
        this.filename = filename;
    }

    /**
     * Constructor for DecoFile with content
     * 
     * @param namespace The namespace of the file
     * @param path The path of the file relative to the namespace folder
     * @param filename The filename of the file (without the path and extension)
     * @param content The content of the file
     */
    public DecoFile(String namespace, String path, String filename, String content) {
        this.namespace = namespace;
        this.path = path;
        this.filename = filename;
        this.content = content;
    }

    /**
     * Set the content of the file
     * 
     * @param content The content of the file
     */
    public void setContent(String content) {
        this.content = content;
    }

    /**
     * Get the content of the file
     * 
     * @return The content of the file
     */
    public String getContent() {
        return content;
    }

    /**
     * Get the namespace of the file
     * 
     * @return The namespace of the file
     */
    public String getNamespace() {
        return namespace;
    }

    /**
     * Set the namespace of the file
     * 
     * @param namespace The namespace of the file
     */
    public void setNamespace(String namespace) {
        this.namespace = namespace;
    }

    /**
     * Get the path of the file
     * 
     * @return The path of the file
     */
    public String getPath() {
        return path;
    }

    /**
     * Set the path of the file
     * 
     * @param path The path of the file
     */
    public void setPath(String path) {
        this.path = path;
    }

    /**
     * Get the filename of the file
     * 
     * @return The filename of the file
     */
    public String getFilename() {
        return filename;
    }

    /**
     * Set the filename of the file
     * 
     * @param filename The filename of the file
     */
    public void setFilename(String filename) {
        this.filename = filename;
    }
}
