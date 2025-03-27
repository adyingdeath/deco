package com.adyingdeath.deco.datapack;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a Minecraft function
 * This class stores the information needed to generate an mcfunction file
 */
public class Function {
    /**
     * The namespace of the function (e.g. "minecraft", "deco")
     */
    private String namespace;
    /**
     * The path of the function (e.g. "deco/test/")
     */
    private String path;
    /**
     * The name of the function
     */
    private String name;
    /**
     * The command body of the function
     */
    private List<String> commands;
    
    /**
     * Create a simple function without decorator
     * @param name The name of the function
     */
    public Function(String name) {
        this.name = name;
        this.commands = new ArrayList<>();
        this.namespace = "minecraft";
        this.path = "";
    }

    /**
     * Set the namespace of the function
     * @param namespace The namespace
     * @return The function instance for method chaining
     */
    public Function setNamespace(String namespace) {
        if (namespace != null && !namespace.isEmpty()) {
            this.namespace = namespace;
        }
        return this;
    }

    /**
     * Set the path of the function
     * @param path The path
     * @return The function instance for method chaining
     */
    public Function setPath(String path) {
        if (path != null && !path.isEmpty()) {
            this.path = path;
        }
        return this;
    }
    /**
     * Add a command to the function
     * @param command The command to add
     * @return The function instance for method chaining
     */
    public Function addCommand(String command) {
        if (command != null && !command.isEmpty()) {
            this.commands.add(command);
        }
        return this;
    }

    /**
     * Get the full path of the function
     * @return The full path (namespace:path/name)
     */
    public String getFullPath() {
        return namespace + ":" + path + "/" + name;
    }
    /**
     * Get the name of the function
     * @return The name
     */
    public String getName() {
        return name;
    }   

    /**
     * Get the commands of the function
     * @return The list of commands
     */
    public List<String> getCommands() {
        return commands;
    }
    
    /**
     * Get the namespace of the function
     * @return The namespace
     */
    public String getNamespace() {
        return namespace;
    }
    
    /**
     * Get the path of the function
     * @return The path
     */
    public String getPath() {
        return path;
    }
}
