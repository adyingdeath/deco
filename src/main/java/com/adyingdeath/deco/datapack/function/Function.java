package com.adyingdeath.deco.datapack.function;

import com.adyingdeath.deco.datapack.ResourceLocation;

import java.util.ArrayList;
import java.util.Arrays;
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
     *
     * @param command The command to add
     */
    public void addCommand(String command) {
        if (command != null && !command.isEmpty()) {
            if (command.contains("\n")) {
                // If the command contains a newline, split it into multiple commands and add them to the list one by one
                String[] commandList = command.split("\n");
                for (String cmd : commandList) {
                    if (cmd.trim().isEmpty()) {
                        continue;
                    }
                    this.commands.add(cmd);
                }
            } else {
                // If the command doesn't contain a newline, add it to the list
                this.commands.add(command);
            }
        }
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
