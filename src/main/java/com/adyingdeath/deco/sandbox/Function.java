package com.adyingdeath.deco.sandbox;

import java.util.ArrayList;
import java.util.List;

/**
 * Represents a Minecraft function
 * This class stores the information needed to generate an mcfunction file
 */
public class Function {
    /**
     * The namespace of the function (e.g. "minecraft", "custom")
     */
    private String namespace;

    /**
     * The path of the function (e.g. "functions/custom")
     */
    private String path;
    /**
     * The name of the function
     */
    private String name;
    /**
     * The decorator type of the function (e.g. "tick", "load")
     */
    private String decorator;
    
    /**
     * Parameters for the decorator
     */
    private List<String> decoratorParameters;
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
        this.decorator = null;
        this.decoratorParameters = new ArrayList<>();
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
     * Set the decorator of the function
     * @param decorator The decorator
     * @return The function instance for method chaining
     */
    public Function setDecorator(String decorator) {
        this.decorator = decorator;
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
     * Add a decorator parameter
     * @param parameter The parameter to add
     * @return The function instance for method chaining
     */
    public Function addDecoratorParameter(String parameter) {
        if (parameter != null && !parameter.isEmpty()) {
            this.decoratorParameters.add(parameter);
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
     * Get the decorator of the function
     * @return The decorator
     */
    public String getDecorator() {
        return decorator;
    }
    
    /**
     * Get the decorator parameters
     * @return The list of decorator parameters
     */
    public List<String> getDecoratorParameters() {
        return decoratorParameters;
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
    
    /**
     * Generate the function content as a string
     * @return The function content
     */
    public String generateContent() {
        StringBuilder content = new StringBuilder();
        
        // Add any header comments for the decorator
        if (decorator != null && !decorator.isEmpty()) {
            content.append("# @").append(decorator);
            
            if (!decoratorParameters.isEmpty()) {
                content.append("(");
                content.append(String.join(", ", decoratorParameters));
                content.append(")");
            }
            
            content.append("\n");
        }
        
        // Add all commands
        for (String command : commands) {
            content.append(command).append("\n");
        }
        
        return content.toString();
    }
}
