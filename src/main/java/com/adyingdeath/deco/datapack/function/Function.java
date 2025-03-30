package com.adyingdeath.deco.datapack.function;

import com.adyingdeath.deco.datapack.ResourceLocation;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a Minecraft function
 * This class stores the information needed to generate an mcfunction file
 */
public class Function {
    private ResourceLocation location;
    /**
     * The command body of the function
     */
    private List<String> commands;
    
    /**
     * Create a simple function without decorator
     * @param location The name of the function
     */
    public Function(String location) {
        this.location = new ResourceLocation(location);
        this.commands = new ArrayList<>();
    }

    /**
     * Set the namespace of the function
     * @param namespace The namespace
     * @return The function instance for method chaining
     */
    public Function setNamespace(String namespace) {
        if (namespace != null && !namespace.isEmpty()) {
            this.location.setNamespace(namespace);
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
            this.location.setPath(path);
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
     * Get the location of the function
     * @return The location
     */
    public ResourceLocation getLocation() {
        return location;
    }

    /**
     * Set the location of the function
     * @param location The location
     * @return The function instance for method chaining
     */
    public Function setLocation(ResourceLocation location) {
        this.location = location;
        return this;
    }

    /**
     * Get the commands of the function
     * @return The list of commands
     */
    public List<String> getCommands() {
        return commands;
    }
}
